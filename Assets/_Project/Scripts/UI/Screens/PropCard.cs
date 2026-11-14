// Stream B. The prop card (PRD-v2 4.9, B-10): tapping a prop on the map (PropTapped from Person A's World)
// opens a short card: the prop's name and place, the concept chip, one line, and the "try" button. Trying it
// adds a Field Note (counted on the Learner Card). Text from dialogue.json props[]. Only on the map.
// ShowInfo reuses the card for a one-off explainer (Quest 3's managed switch): no Field Note, the button closes it.
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class PropCard : MonoBehaviour
    {
        [SerializeField] GameObject overlay;
        [SerializeField] Button dimButton;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text placeText;
        [SerializeField] GameObject conceptChip;
        [SerializeField] TMP_Text conceptText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button tryButton;
        [SerializeField] TMP_Text tryLabel;
        [SerializeField] TMP_Text notesText;

        string propId;
        bool infoOnly;
        GameState shownIn;
        static PropCard instance;

        /// <summary>Shows a props[] entry as a plain explainer, in any state. Does nothing if the entry is missing.</summary>
        public static void ShowInfo(string id)
        {
            if (instance == null) return;
            var prop = Find(id);
            if (prop == null) return;
            instance.Show(id, prop, true);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Awake()
        {
            instance = this;
            tryButton.onClick.AddListener(OnTry);
            dimButton.onClick.AddListener(Close);
            overlay.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.PropTapped += OnPropTapped;
            GameEvents.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameEvents.PropTapped -= OnPropTapped;
            GameEvents.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (!overlay.activeSelf) return;
            var stay = infoOnly ? to == shownIn : to == GameState.WorldPeace || to == GameState.Alert;
            if (!stay) Close();
        }

        void OnPropTapped(string id)
        {
            var state = Services.Flow.State;
            if (state != GameState.WorldPeace && state != GameState.Alert) return;
            var prop = Find(id);
            if (prop == null) return;
            Show(id, prop, false);
        }

        void Show(string id, PropData prop, bool info)
        {
            propId = id;
            infoOnly = info;
            shownIn = Services.Flow.State;
            nameText.text = prop.name ?? "";
            placeText.text = prop.place ?? "";
            placeText.gameObject.SetActive(!string.IsNullOrEmpty(prop.place));
            conceptText.text = prop.concept ?? "";
            conceptChip.SetActive(!string.IsNullOrEmpty(prop.concept));
            bodyText.text = prop.text ?? "";
            tryLabel.text = (info && string.IsNullOrEmpty(prop.tryLabel) ? UiStrings.Ui.next : prop.tryLabel) ?? "";
            tryButton.gameObject.SetActive(!string.IsNullOrEmpty(tryLabel.text));
            RefreshNotes();
            if (info) notesText.gameObject.SetActive(false);
            overlay.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)nameText.transform.parent);
        }

        void OnTry()
        {
            if (!overlay.activeSelf) return;
            if (!infoOnly) LearnerProgress.AddFieldNote(propId);
            Close();
        }

        void RefreshNotes()
        {
            notesText.text = UiStrings.Format(UiStrings.Ui.fieldNotesFormat, LearnerProgress.FieldNotes, LearnerProgress.FieldNotesTotal);
            notesText.gameObject.SetActive(!string.IsNullOrEmpty(notesText.text));
        }

        void Close() => overlay.SetActive(false);

        static PropData Find(string id)
        {
            var props = Services.Data?.Dialogue?.props;
            if (props == null || string.IsNullOrEmpty(id)) return null;
            foreach (var p in props)
                if (p != null && p.id == id) return p;
            return null;
        }
    }
}
