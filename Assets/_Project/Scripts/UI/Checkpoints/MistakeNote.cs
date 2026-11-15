// Stream B UI, added by A at Rudi's request. A wrong decision in a station shift drops a small card in at the top:
// a pink "× Try again" and a "See materials →" link that opens the station's materials panel over the shift
// (MaterialsPanel.OpenOverShift pauses the shift while it's open). The hint itself stays in the dialogue
// bubble, so the card never repeats it. Built in code by MaterialsPanel; labels from dialogue.json ui.
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class MistakeNote : MonoBehaviour
    {
        const float ShowSeconds = 7f;

        MaterialsPanel materials;
        RectTransform card;
        TMP_Text heading;
        TMP_Text linkLabel;
        string station;
        Coroutine hide;

        public static MistakeNote Create(RectTransform parent, MaterialsPanel materials)
        {
            var go = new GameObject("MistakeNote", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var note = go.AddComponent<MistakeNote>();
            note.materials = materials;
            note.Build();
            return note;
        }

        void Build()
        {
            card = (RectTransform)transform;
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 1f);
            card.anchoredPosition = new Vector2(0, -130);
            card.sizeDelta = new Vector2(720, 96);
            var back = gameObject.AddComponent<Image>();
            back.color = Palette.ByteCream;
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.AlertPink;
            outline.effectDistance = new Vector2(6, -6);

            heading = UiKit.Text("Heading", transform, UiStyle.Body, Palette.AlertPink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            Place(heading.rectTransform, new Vector2(0, 0), new Vector2(0.45f, 1), new Vector2(36, 0), Vector2.zero);

            // Syed's secondary button style, so the link looks like every other button in the game.
            var link = UiKit.Button("Link", transform, ButtonStyle.Secondary, out linkLabel, 72);
            Place((RectTransform)link.transform, new Vector2(0.45f, 0), new Vector2(1, 1), new Vector2(0, 12), new Vector2(-16, -12));
            link.onClick.AddListener(OpenMaterials);

            gameObject.SetActive(false);
        }

        static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        void Awake()
        {
            GameEvents.ItemResolved += OnItemResolved;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            GameEvents.ItemResolved -= OnItemResolved;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnItemResolved(ItemResolvedInfo info)
        {
            if (info.correct)
            {
                Hide();
                return;
            }
            if (Services.Flow.State != GameState.JobShift) return;
            station = StationFor(info.job);
            var ui = UiStrings.Ui;
            heading.text = $"× {ui.tryAgain ?? ""}";
            linkLabel.text = $"{ui.seeMaterials ?? ""} →";
            linkLabel.transform.parent.gameObject.SetActive(station != null && materials != null);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (hide != null) StopCoroutine(hide);
            hide = StartCoroutine(HideLater());
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.JobShift) Hide();
        }

        IEnumerator HideLater()
        {
            yield return new WaitForSecondsRealtime(ShowSeconds);
            // Stay while the materials are open; the player came back to it.
            while (materials != null && materials.IsOpen) yield return null;
            Hide();
        }

        void Hide()
        {
            if (hide != null) StopCoroutine(hide);
            hide = null;
            gameObject.SetActive(false);
        }

        void OpenMaterials()
        {
            if (station == null || materials == null) return;
            materials.OpenOverShift(station);
            Hide();
        }

        static string StationFor(JobId job)
        {
            var key = job.ToKey();
            foreach (var c in Services.Checkpoints.All)
                if (c.job == key) return c.id;
            return null;
        }
    }
}
