// Stream B. Pause button and pause menu (B-16). The button shows while the player is actually playing;
// the menu sends UiAction.Pause / UiAction.Resume (GameManager stops and restarts time) and
// RestartDayRequested (GameManager restarts the day with three backups).
// A sound button in the menu mutes or unmutes all music and sound (AudioListener.volume, so the audio code
// doesn't need to know). Its icon is a speaker with sound waves, or a cross while muted: no words needed.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject pauseButton;
        [SerializeField] GameObject menu;
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button resumeButton;
        [SerializeField] TMP_Text resumeLabel;
        [SerializeField] Button restartButton;
        [SerializeField] TMP_Text restartLabel;

        static bool muted;
        Image[] soundBars;
        GameObject soundCross;
        TMP_Text soundWaves;

        void Awake()
        {
            BuildSoundButton();
            pauseButton.GetComponent<Button>().onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(RestartDay);
            menu.SetActive(false);
            pauseButton.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void Start() => pauseButton.SetActive(CanPause(Services.Flow.State));

        // Only while the player is playing: not on title, story, cards, results or a test run in progress.
        static bool CanPause(GameState state) => state switch
        {
            GameState.WorldPeace or GameState.Alert or GameState.JobShift or GameState.Build or GameState.FinalAttack => true,
            _ => false
        };

        void OnStateChanged(GameState from, GameState to)
        {
            pauseButton.SetActive(CanPause(to) && !menu.activeSelf);
            if (menu.activeSelf && !CanPause(to)) Resume(); // never leave the game frozen behind another screen
        }

        void Pause()
        {
            if (menu.activeSelf) return;
            var ui = UiStrings.Ui;
            titleText.text = ui.pause ?? "";
            resumeLabel.text = ui.resume ?? "";
            restartLabel.text = ui.restartDay ?? "";
            menu.SetActive(true);
            pauseButton.SetActive(false);
            GameEvents.RaiseUiAction(UiAction.Pause);
        }

        void Resume()
        {
            if (!menu.activeSelf) return;
            menu.SetActive(false);
            pauseButton.SetActive(CanPause(Services.Flow.State));
            GameEvents.RaiseUiAction(UiAction.Resume);
        }

        // ---------- Sound ----------

        void BuildSoundButton()
        {
            var card = resumeButton.transform.parent;
            var button = UiKit.Button("Sound", card, ButtonStyle.Secondary, out var label, 100);
            button.transform.SetSiblingIndex(resumeButton.transform.GetSiblingIndex() + 1);
            label.text = "";
            var icon = UiKit.Rect("Icon", button.transform);
            icon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.sizeDelta = new Vector2(120, 64);
            // A speaker: a small box and a cone (a turned square half hidden behind the box), then the sound waves.
            var cone = UiKit.Rect("Cone", icon);
            cone.anchorMin = cone.anchorMax = new Vector2(0, 0.5f);
            cone.anchoredPosition = new Vector2(30, 0);
            cone.sizeDelta = new Vector2(38, 38);
            cone.localRotation = Quaternion.Euler(0, 0, 45);
            var box = UiKit.Rect("Box", icon);
            box.anchorMin = box.anchorMax = new Vector2(0, 0.5f);
            box.anchoredPosition = new Vector2(16, 0);
            box.sizeDelta = new Vector2(26, 30);
            soundBars = new[] { UiKit.Image(cone.gameObject, UiStyle.Accent), UiKit.Image(box.gameObject, UiStyle.Accent) };
            var waves = UiKit.Text("Waves", icon, UiStyle.Heading, UiStyle.Accent, TextAlignmentOptions.MidlineRight, UiFont.Mono);
            UiKit.Stretch(waves.rectTransform);
            waves.text = "))";
            soundWaves = waves;
            var cross = UiKit.Text("Cross", icon, UiStyle.Heading, UiStyle.Danger, TextAlignmentOptions.MidlineRight, UiFont.Mono);
            UiKit.Stretch(cross.rectTransform);
            cross.text = "✕";
            soundCross = cross.gameObject;
            button.onClick.AddListener(() =>
            {
                muted = !muted;
                ShowSound();
            });
            ShowSound();
        }

        void ShowSound()
        {
            AudioListener.volume = muted ? 0f : 1f;
            foreach (var bar in soundBars) bar.color = muted ? UiStyle.Locked : UiStyle.Accent;
            soundCross.SetActive(muted);
            soundWaves.gameObject.SetActive(!muted);
        }

        void RestartDay()
        {
            if (!menu.activeSelf) return;
            // Resume first: GameManager's restart doesn't touch the time scale.
            Resume();
            GameEvents.RaiseRestartDay();
        }
    }
}
