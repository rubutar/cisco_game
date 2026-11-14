// Stream B. The speech bubble (IDialogueService), styled as a Webex message (PRD-v2 4.2, B-02): the Town
// Space name on a tab, the speaker's avatar tile with initials, name and timestamp, then the line.
// One line at a time; a tap anywhere continues. Narration (no speaker) hides the avatar and header.
// ShowHint (Quest 1 hints) points at a world position and shows "Try again" instead.
// Lives on UIRoot.prefab; registers itself in Awake so gameplay finds it through Services.Dialogue.
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ByteWorld
{
    public class DialogueBubble : MonoBehaviour, IDialogueService, IPointerClickHandler
    {
        const int MaxLineLength = 110; // PRD 4.1: one bubble per line, ~110 characters

        [SerializeField] GameObject tapCatcher;
        [SerializeField] RectTransform panel;
        [SerializeField] GameObject nameTag;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] GameObject continueIcon;
        [SerializeField] Button retryButton;
        [SerializeField] TMP_Text retryLabel;
        [SerializeField] RectTransform pointer;

        [Header("Webex")]
        [SerializeField] TMP_Text spaceText;
        [SerializeField] Image avatar;
        [SerializeField] TMP_Text avatarText;
        [SerializeField] TMP_Text timeText;
        [SerializeField] float bodyLeftWithAvatar = 200;
        [SerializeField] float bodyLeftWithout = 56;

        readonly Queue<string> pending = new();
        Action onDone;
        Action onRetry;
        Vector3 hintWorldPosition;
        int shownFrame;

        public bool IsShowing { get; private set; }

        void Awake()
        {
            Services.Register<IDialogueService>(this);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryPressed);
            SetVisible(false, false);
        }

        void OnDestroy() => Services.Unregister<IDialogueService>(this);

        public void Say(string speaker, string text, Action done = null) => SayLines(speaker, new[] { text }, done);

        public void SayLines(string speaker, IReadOnlyList<string> lines, Action done = null)
        {
            pending.Clear();
            if (lines != null)
                foreach (var line in lines)
                    if (!string.IsNullOrEmpty(line)) pending.Enqueue(line);

            onDone = done;
            onRetry = null;
            if (pending.Count == 0)
            {
                Finish();
                return;
            }

            SetSpeaker(speaker);
            SetVisible(true, false);
            ShowLine(pending.Dequeue());
        }

        public void ShowHint(string speaker, string text, Vector3 worldPosition, Action retry)
        {
            pending.Clear();
            onDone = null;
            onRetry = retry;
            hintWorldPosition = worldPosition;
            SetSpeaker(speaker);
            if (retryLabel != null) retryLabel.text = UiStrings.Ui.tryAgain ?? "";
            SetVisible(true, true);
            ShowLine(text);
            PlacePointer();
        }

        public void Hide()
        {
            pending.Clear();
            onDone = null;
            onRetry = null;
            SetVisible(false, false);
        }

        // A tap anywhere (the tap catcher fills the screen) moves to the next line. Hints wait for "Try again".
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsShowing || onRetry != null) return;
            if (Time.frameCount == shownFrame) return; // the tap that opened the bubble doesn't skip it
            if (pending.Count > 0) ShowLine(pending.Dequeue());
            else Finish();
        }

        void Update()
        {
            if (!IsShowing) return;
            if (pointer != null && pointer.gameObject.activeSelf) PlacePointer();
            if (continueIcon != null && continueIcon.activeSelf)
            {
                // Gentle 2-step bob, snapped to whole pixels so it stays crisp.
                var step = Mathf.FloorToInt(Time.unscaledTime * 3f) % 2;
                ((RectTransform)continueIcon.transform).anchoredPosition = new Vector2(-40f, 36f + step * 6f);
            }
        }

        void OnRetryPressed()
        {
            var retry = onRetry;
            Hide();
            retry?.Invoke();
        }

        void Finish()
        {
            var done = onDone;
            Hide();
            done?.Invoke();
        }

        void ShowLine(string line)
        {
            if (line.Length > MaxLineLength)
                Debug.LogWarning($"[DialogueBubble] Line is {line.Length} characters (max ~{MaxLineLength}): {line}");
            bodyText.text = line;
            shownFrame = Time.frameCount;
        }

        void SetSpeaker(string speaker)
        {
            var name = UiStrings.SpeakerName(speaker);
            var hasName = !string.IsNullOrEmpty(name);
            if (nameText != null) nameText.text = name;
            if (nameTag != null) nameTag.SetActive(hasName);
            if (spaceText != null) spaceText.text = UiStrings.Ui.gameTitle ?? "";
            if (timeText != null) timeText.text = hasName ? Webex.NextTime() : "";
            if (avatar != null)
            {
                avatar.gameObject.SetActive(hasName);
                avatar.color = UiStyle.AvatarFill(speaker);
            }
            if (avatarText != null)
            {
                avatarText.text = Webex.Initials(name);
                avatarText.color = UiStyle.AvatarText(speaker);
            }

            // Header and body sit right of the avatar; narration uses the full width.
            var left = hasName ? bodyLeftWithAvatar : bodyLeftWithout;
            var body = bodyText.rectTransform;
            body.offsetMin = new Vector2(left, body.offsetMin.y);
            body.offsetMax = new Vector2(body.offsetMax.x, hasName ? -96 : -52);
            if (nameTag != null)
            {
                var header = (RectTransform)nameTag.transform;
                header.anchoredPosition = new Vector2(left, header.anchoredPosition.y);
            }
        }

        void SetVisible(bool visible, bool hint)
        {
            IsShowing = visible;
            if (panel != null) panel.gameObject.SetActive(visible);
            if (tapCatcher != null) tapCatcher.SetActive(visible && !hint);
            if (continueIcon != null) continueIcon.SetActive(visible && !hint);
            if (retryButton != null) retryButton.gameObject.SetActive(visible && hint);
            if (pointer != null) pointer.gameObject.SetActive(visible && hint);
            // Keep the line clear of the Try again button on hints, and of the continue arrow otherwise.
            var body = bodyText.rectTransform;
            body.offsetMax = new Vector2(hint ? -420 : -96, body.offsetMax.y);
        }

        void PlacePointer()
        {
            var cam = Camera.main;
            if (cam == null || pointer == null)
            {
                if (pointer != null) pointer.gameObject.SetActive(false);
                return;
            }

            var screen = cam.WorldToScreenPoint(hintWorldPosition);
            var parent = (RectTransform)pointer.parent;
            if (screen.z < 0f ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, null, out var local))
            {
                pointer.gameObject.SetActive(false);
                return;
            }

            var bob = (Mathf.FloorToInt(Time.unscaledTime * 3f) % 2) * 8f;
            pointer.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y + 70f + bob));
        }
    }
}
