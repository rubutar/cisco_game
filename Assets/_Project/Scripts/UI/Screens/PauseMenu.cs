// Stream B. Pause button and pause menu (B-16). The button shows while the player is actually playing;
// the menu sends UiAction.Pause / UiAction.Resume (GameManager stops and restarts time) and
// RestartDayRequested (GameManager restarts the day with three backups).
// A sound button in the card's top-right corner mutes or unmutes all music and sound (AudioListener.volume, so the audio code
// doesn't need to know). Its icon is a pixel speaker with sound waves, or a red cross while muted: no words needed.
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

        // Pixel-art speaker icons, 16 x 12: S = speaker, W = sound waves, X = the mute cross.
        static readonly string[] SpeakerOn =
        {
            "......S.........",
            ".....SS.....W...",
            "....SSS..W...W..",
            "SSSSSSS...W...W.",
            "SSSSSSS...W...W.",
            "SSSSSSS...W...W.",
            "SSSSSSS...W...W.",
            "SSSSSSS...W...W.",
            "SSSSSSS...W...W.",
            "....SSS..W...W..",
            ".....SS.....W...",
            "......S.........",
        };

        static readonly string[] SpeakerOff =
        {
            "......S.........",
            ".....SS.........",
            "....SSS.........",
            "SSSSSSS..X...X..",
            "SSSSSSS...X.X...",
            "SSSSSSS....X....",
            "SSSSSSS....X....",
            "SSSSSSS...X.X...",
            "SSSSSSS..X...X..",
            "....SSS.........",
            ".....SS.........",
            "......S.........",
        };

        Image soundIcon;
        Sprite soundOn, soundOff;

        void BuildSoundButton()
        {
            var card = resumeButton.transform.parent;
            var button = UiKit.Button("Sound", card, ButtonStyle.Secondary, out var label, 100);
            // Top-right corner of the card, out of the Resume / Restart column (Rudi: a speaker between the
            // two main buttons reads as one of them).
            var layout = button.GetComponent<LayoutElement>();
            if (layout == null) layout = button.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-28, -28);
            rect.sizeDelta = new Vector2(120, 100);
            label.text = "";
            var icon = UiKit.Rect("Icon", button.transform);
            icon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.sizeDelta = new Vector2(96, 72); // 16 x 12 pixels at 6x
            soundIcon = UiKit.Image(icon.gameObject, Color.white);
            soundIcon.preserveAspect = true;
            soundOn = PixelIcon(SpeakerOn, UiStyle.Accent, UiStyle.Accent, UiStyle.Danger);
            soundOff = PixelIcon(SpeakerOff, UiStyle.Locked, UiStyle.Locked, UiStyle.Danger);
            button.onClick.AddListener(() =>
            {
                muted = !muted;
                ShowSound();
            });
            ShowSound();
        }

        static Sprite PixelIcon(string[] rows, Color speaker, Color waves, Color cross)
        {
            int h = rows.Length, w = rows[0].Length;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var c = rows[h - 1 - y][x]; // textures start at the bottom row
                tex.SetPixel(x, y, c switch
                {
                    'S' => speaker,
                    'W' => waves,
                    'X' => cross,
                    _ => Color.clear
                });
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1);
        }

        void ShowSound()
        {
            AudioListener.volume = muted ? 0f : 1f;
            soundIcon.sprite = muted ? soundOff : soundOn;
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
