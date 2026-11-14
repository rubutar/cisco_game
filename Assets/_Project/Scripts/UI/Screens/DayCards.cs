// Stream B. Day end and day start cards (screens D1 and D2). Full-screen: night for the end of a day,
// morning for the start of the next. Text comes from dialogue.json dayEnd/dayStart; the buttons send
// UiAction.Sleep and UiAction.StartDay. GameManager advances the day (A-14), not this screen.
using ByteWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class DayCards : MonoBehaviour
    {
        [SerializeField] GameObject screen;
        [SerializeField] Image backdrop;
        [SerializeField] TMP_Text titleText;
        [SerializeField] GameObject speakerTag;
        [SerializeField] TMP_Text speakerText;
        [SerializeField] TMP_Text lineText;
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonLabel;

        UiAction action;

        void Awake()
        {
            button.onClick.AddListener(OnButton);
            screen.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnStateChanged(GameState from, GameState to)
        {
            var day = Services.Flow.Day;
            var dialogue = Services.Data?.Dialogue;
            switch (to)
            {
                case GameState.DayEnd:
                    Show(Find(dialogue?.dayEnd, day), Palette.Ink, UiStrings.Ui.sleep, UiAction.Sleep);
                    break;
                case GameState.DayStart:
                    Show(Find(dialogue?.dayStart, day), Palette.SignalBlue, UiStrings.Ui.startDay, UiAction.StartDay);
                    break;
                default:
                    screen.SetActive(false);
                    break;
            }
        }

        void Show(DayLine line, Color background, string label, UiAction onPress)
        {
            Services.Dialogue.Hide();
            Services.Inspect.Hide();

            backdrop.color = background;
            titleText.text = line?.title ?? "";
            var speaker = UiStrings.SpeakerName(line?.speaker);
            speakerText.text = speaker;
            speakerTag.SetActive(!string.IsNullOrEmpty(speaker));
            lineText.text = line?.line ?? "";
            lineText.gameObject.SetActive(!string.IsNullOrEmpty(lineText.text));
            buttonLabel.text = label ?? "";
            action = onPress;
            screen.SetActive(true);
        }

        static DayLine Find(DayLine[] lines, int day)
        {
            if (lines == null) return null;
            foreach (var line in lines)
                if (line != null && line.day == day) return line;
            return null;
        }

        void OnButton()
        {
            if (!screen.activeSelf) return;
            screen.SetActive(false);
            GameEvents.RaiseUiAction(action);
        }
    }
}
