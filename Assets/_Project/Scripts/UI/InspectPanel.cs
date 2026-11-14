// Stream B. The inspect panel used by all four jobs (IInspectPanel, PRD 4.6): a title, up to four
// label/value rows and two buttons. The job script fills InspectData from the JSON; this panel only
// displays it and reports which button was pressed. Registers itself in Awake.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class InspectPanel : MonoBehaviour, IInspectPanel
    {
        public const int MaxFields = 4;

        [SerializeField] GameObject card;
        [SerializeField] TMP_Text titleText;
        [SerializeField] GameObject[] fieldRows = new GameObject[MaxFields];
        [SerializeField] TMP_Text[] fieldLabels = new TMP_Text[MaxFields];
        [SerializeField] TMP_Text[] fieldValues = new TMP_Text[MaxFields];
        [SerializeField] Button primaryButton;
        [SerializeField] TMP_Text primaryLabel;
        [SerializeField] Button secondaryButton;
        [SerializeField] TMP_Text secondaryLabel;

        Action onPrimary;
        Action onSecondary;

        public bool IsOpen { get; private set; }

        void Awake()
        {
            Services.Register<IInspectPanel>(this);
            primaryButton.onClick.AddListener(() => Choose(true));
            secondaryButton.onClick.AddListener(() => Choose(false));
            card.SetActive(false);
        }

        void OnDestroy() => Services.Unregister<IInspectPanel>(this);

        public void Show(InspectData data, Action primary, Action secondary)
        {
            onPrimary = primary;
            onSecondary = secondary;

            titleText.text = data.title ?? "";
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(data.title));

            var count = data.fields?.Length ?? 0;
            if (count > MaxFields)
                Debug.LogWarning($"[InspectPanel] {count} fields given, showing the first {MaxFields}.");
            for (var i = 0; i < MaxFields; i++)
            {
                var used = i < count;
                fieldRows[i].SetActive(used);
                if (!used) continue;
                fieldLabels[i].text = data.fields[i].label ?? "";
                fieldValues[i].text = data.fields[i].value ?? "";
            }

            primaryLabel.text = data.primaryLabel ?? "";
            secondaryLabel.text = data.secondaryLabel ?? "";
            primaryButton.interactable = true;
            secondaryButton.interactable = true;

            IsOpen = true;
            card.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)card.transform);
        }

        public void Hide()
        {
            IsOpen = false;
            onPrimary = null;
            onSecondary = null;
            card.SetActive(false);
        }

        // Close first, then report, so the job can open the next item from inside the callback.
        void Choose(bool primary)
        {
            if (!IsOpen) return; // ignore a second tap in the same frame
            var callback = primary ? onPrimary : onSecondary;
            Hide();
            callback?.Invoke();
        }
    }
}
