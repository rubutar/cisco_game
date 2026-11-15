// Stream B. Pause button and pause menu (B-16). The button shows while the player is actually playing;
// the menu sends UiAction.Pause / UiAction.Resume (GameManager stops and restarts time) and
// RestartDayRequested (GameManager restarts the day with three backups).
// Sound (A): a speaker button in the card's top-right corner mutes and unmutes all sound through
// AudioDirector; its icons are Sprites/UI/speaker_on and speaker_off. Added at runtime, so the prefab
// and UIRootBuilder stay as they are.
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

        Image soundIcon;
        Sprite soundOn, soundOff;

        void Awake()
        {
            pauseButton.GetComponent<Button>().onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(RestartDay);
            menu.SetActive(false);
            pauseButton.SetActive(false);
            AddSoundButton();
        }

        void AddSoundButton()
        {
            soundOn = Resources.Load<Sprite>("Sprites/UI/speaker_on");
            soundOff = Resources.Load<Sprite>("Sprites/UI/speaker_off");
            if (soundOn == null || titleText == null) return;

            // Same look as the pause button: a signal-blue tile with an ink outline and a cream icon.
            var go = new GameObject("SoundButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Outline));
            go.transform.SetParent(titleText.transform.parent, false);
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-28, -28);
            rect.sizeDelta = new Vector2(88, 88);
            var tile = go.GetComponent<Image>();
            tile.color = Palette.SignalBlue;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(4, -4);
            var button = go.GetComponent<Button>();
            button.targetGraphic = tile;
            button.onClick.AddListener(ToggleSound);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            ((RectTransform)icon.transform).sizeDelta = new Vector2(80, 80);
            soundIcon = icon.GetComponent<Image>();
            soundIcon.raycastTarget = false;
            ShowSound();
        }

        void ToggleSound()
        {
            var audio = AudioDirector.Instance;
            if (audio == null) return;
            audio.SetMuted(!audio.Muted);
            ShowSound();
        }

        void ShowSound()
        {
            if (soundIcon == null) return;
            var muted = AudioDirector.Instance != null && AudioDirector.Instance.Muted;
            soundIcon.sprite = muted && soundOff != null ? soundOff : soundOn;
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
            ShowSound();
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

        void RestartDay()
        {
            if (!menu.activeSelf) return;
            // Resume first: GameManager's restart doesn't touch the time scale.
            Resume();
            GameEvents.RaiseRestartDay();
        }
    }
}
