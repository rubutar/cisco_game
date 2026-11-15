// Stream B. The Cisco toolkit (PRD-v2 4.4.5, 4.11 rule 6, B-16):
//   * a top-bar button "Cisco toolkit 3/7" on the map,
//   * a toast on StationFinished: "Cisco Umbrella added to your Cisco toolkit", with the tool badge and Pax happy,
//   * the half-screen panel: the Visitor Pass with its tool badges (colour once earned), every station with its
//     tool and state, grouped by quest, and the Learner Card at the bottom (dim until the last station is done).
// Text from checkpoints.json and dialogue.json ui (toolkit, toolkitCountFormat, toolkitToastFormat).
using System.Collections;
using System.Collections.Generic;
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class ToolkitPanel : MonoBehaviour
    {
        const float ToastSeconds = 3f;

        [SerializeField] Button openButton;
        [SerializeField] TMP_Text openLabel;
        [SerializeField] RectTransform panel;
        [SerializeField] TMP_Text titleText;
        [SerializeField] Image pass;
        [SerializeField] Image paxPortrait;
        [SerializeField] RectTransform badgeRow;
        [SerializeField] RectTransform stationList;
        [SerializeField] LearnerCard learnerCard;
        [SerializeField] CanvasGroup learnerGroup;
        [SerializeField] Button closeButton;
        [SerializeField] RectTransform toast;
        [SerializeField] Image toastBadge;
        [SerializeField] Image toastPortrait;
        [SerializeField] TMP_Text toastText;

        readonly List<Image> badges = new();
        Coroutine slide, toastRoutine;

        public bool IsOpen => panel.gameObject.activeSelf;

        void Awake()
        {
            openButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);
            panel.gameObject.SetActive(false);
            toast.gameObject.SetActive(false);
            openButton.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.CheckpointStatesChanged += Refresh;
            GameEvents.StationFinished += OnStationFinished;
            GameEvents.UiCommandRequested += OnCommand;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.CheckpointStatesChanged -= Refresh;
            GameEvents.StationFinished -= OnStationFinished;
            GameEvents.UiCommandRequested -= OnCommand;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            var map = Stations.IsMapState(to);
            openButton.gameObject.SetActive(map);
            if (!map) CloseNow();
            Refresh();
        }

        void OnCommand(string command)
        {
            if (command == "back" && IsOpen) Close();
        }

        void LateUpdate()
        {
            // Out of the way while the toolkit or the materials panel covers the right half.
            var show = Stations.IsMapState(Services.Flow.State) && !IsOpen && !MaterialsPanel.AnyOpen && !HuntScreen.IsOpen;
            if (openButton.gameObject.activeSelf != show) openButton.gameObject.SetActive(show);
        }

        void Refresh()
        {
            var cps = Services.Checkpoints;
            if (cps == null) return;
            var ui = UiStrings.Ui;
            openLabel.text = UiStrings.Format(ui.toolkitCountFormat, cps.ToolsCollected, cps.ToolsTotal);
            if (string.IsNullOrEmpty(openLabel.text)) openLabel.text = ui.toolkit ?? "";
            if (IsOpen) Fill();
        }

        // ---------- Toast ----------

        void OnStationFinished(string id)
        {
            var c = Services.Checkpoints?.Get(id);
            Refresh();
            if (c == null) return;
            toastText.text = UiStrings.Format(UiStrings.Ui.toolkitToastFormat, c.tool ?? "");
            if (string.IsNullOrEmpty(toastText.text)) return;
            toastBadge.sprite = Stations.Badge(c, false);
            toastBadge.gameObject.SetActive(toastBadge.sprite != null);
            toastPortrait.sprite = Portraits.Get(Portraits.Happy);
            toastPortrait.gameObject.SetActive(toastPortrait.sprite != null);
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(Toast());
        }

        IEnumerator Toast()
        {
            toast.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(toast);
            var height = toast.rect.height + 40;
            for (var t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                toast.anchoredPosition = new Vector2(0, Mathf.Lerp(height, -130, t / 0.2f));
                yield return null;
            }
            toast.anchoredPosition = new Vector2(0, -130);
            yield return new WaitForSecondsRealtime(ToastSeconds);
            toast.gameObject.SetActive(false);
            toastRoutine = null;
        }

        // ---------- Panel ----------

        public void Open()
        {
            if (!Stations.IsMapState(Services.Flow.State)) return;
            Fill();
            panel.gameObject.SetActive(true);
            if (slide != null) StopCoroutine(slide);
            slide = StartCoroutine(Slide(true));
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (slide != null) StopCoroutine(slide);
            slide = StartCoroutine(Slide(false));
        }

        void CloseNow()
        {
            if (slide != null) StopCoroutine(slide);
            slide = null;
            panel.gameObject.SetActive(false);
        }

        IEnumerator Slide(bool show)
        {
            var width = panel.rect.width;
            var from = show ? width : 0f;
            var to = show ? 0f : width;
            for (var t = 0f; t < UiStyle.PanelSlideSeconds; t += Time.unscaledDeltaTime)
            {
                var k = 1f - Mathf.Pow(1f - t / UiStyle.PanelSlideSeconds, 3f);
                panel.anchoredPosition = new Vector2(Mathf.Lerp(from, to, k), 0);
                yield return null;
            }
            panel.anchoredPosition = new Vector2(to, 0);
            if (!show) CloseNow();
            slide = null;
        }

        void Fill()
        {
            var cps = Services.Checkpoints;
            var all = cps?.All;
            if (all == null) return;
            var ui = UiStrings.Ui;
            titleText.text = UiStrings.Format(ui.toolkitCountFormat, cps.ToolsCollected, cps.ToolsTotal);
            pass.sprite = UiKit.Sprite("UI/visitor_pass");
            pass.gameObject.SetActive(pass.sprite != null);
            paxPortrait.sprite = Portraits.Get(cps.ToolsCollected > 0 ? Portraits.Happy : Portraits.Neutral);
            paxPortrait.gameObject.SetActive(paxPortrait.sprite != null);

            // The pass badges, one per station, in checkpoints.json order.
            while (badges.Count < all.Length)
            {
                var b = UiKit.Rect($"Badge{badges.Count}", badgeRow);
                var image = UiKit.Image(b.gameObject, Color.white);
                image.preserveAspect = true;
                UiKit.Size(b.gameObject, 64, 64);
                badges.Add(image);
            }
            for (var i = 0; i < badges.Count; i++)
            {
                var has = i < all.Length;
                badges[i].gameObject.SetActive(has);
                if (has) badges[i].sprite = Stations.Badge(all[i], cps.StateOf(all[i].id) != CheckpointState.Done);
            }

            // Stations grouped by quest.
            foreach (Transform child in stationList) Destroy(child.gameObject);
            var lastQuest = -1;
            foreach (var c in all)
            {
                if (c.quest != lastQuest)
                {
                    lastQuest = c.quest;
                    var rule = UiKit.Rect("Divider", stationList);
                    UiKit.Image(rule.gameObject, Palette.CliffLight);
                    UiKit.Size(rule.gameObject, 0, 4);
                }
                StationRow(c, cps.StateOf(c.id));
            }

            // The Learner Card: dim until every station is done.
            var finished = cps.ToolsCollected >= cps.ToolsTotal && cps.ToolsTotal > 0;
            learnerCard.Refresh(finished);
            learnerGroup.alpha = finished ? 1f : 0.4f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        }

        void StationRow(CheckpointData c, CheckpointState state)
        {
            var row = UiKit.Rect($"Station {c.id}", stationList);
            UiKit.Row(row.gameObject, 16);
            var badge = UiKit.Rect("Badge", row);
            var image = UiKit.Image(badge.gameObject, Color.white);
            image.sprite = Stations.Badge(c, state == CheckpointState.Locked);
            image.preserveAspect = true;
            UiKit.Size(badge.gameObject, 56, 56);
            var text = UiKit.Rect("Text", row);
            UiKit.Column(text.gameObject, 0);
            text.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var tool = UiKit.Text("Tool", text, UiStyle.Small, state == CheckpointState.Locked ? UiStyle.LockedDark : UiStyle.Ink,
                TextAlignmentOptions.MidlineLeft, UiFont.Title);
            tool.text = c.tool ?? "";
            var station = UiKit.Text("Station", text, UiStyle.Small, UiStyle.Accent);
            station.text = c.station ?? "";
            var mark = UiKit.Rect("State", row);
            UiKit.Image(mark.gameObject, Stations.StateColor(state));
            UiKit.Border(mark.gameObject, 3);
            UiKit.Size(mark.gameObject, 28, 28);
        }
    }
}
