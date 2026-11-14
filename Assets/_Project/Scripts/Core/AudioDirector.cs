// Stream A. Music and sound effects (Resources/Audio, made by Tools/audio/make_audio.py), on GameRoot.prefab.
// Listens to the shared events only, so neither stream has to call it: the music follows the game state
// (crossfading between tracks) and each gameplay event plays its sound. A missing clip is simply silent.
// It also holds the game's one AudioListener: GameRoot lives across every scene, and the scene cameras
// don't carry one (any a scene brings is removed, so there's never two).
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ByteWorld
{
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        const float MusicVolume = 0.35f;
        const float SfxVolume = 0.7f;
        const float FadeSeconds = 0.6f;

        readonly Dictionary<string, AudioClip> clips = new();
        AudioSource[] music;        // two sources, crossfaded
        AudioSource sfx;
        int current;
        string playing;
        Coroutine fade;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            music = new[] { NewSource(true), NewSource(true) };
            sfx = NewSource(false);
            sfx.volume = SfxVolume;
        }

        AudioSource NewSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = 0f;
            return source;
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            var mine = GetComponent<AudioListener>();
            foreach (var listener in FindObjectsByType<AudioListener>())
                if (listener != mine) Destroy(listener);
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.AlertRaised += OnAlert;
            GameEvents.ItemResolved += OnItemResolved;
            GameEvents.ShiftSucceeded += OnShiftSucceeded;
            GameEvents.StationFinished += OnStationFinished;
            GameEvents.Snatched += OnSnatched;
            GameEvents.Collapsed += OnCollapsed;
            GameEvents.Complaint += OnComplaint;
            GameEvents.HeroArrived += OnHeroArrived;
            GameEvents.PropTapped += OnPropTapped;
            GameEvents.TestRunFinished += OnTestRunFinished;
            GameEvents.PuzzlePassed += OnPuzzlePassed;
            GameEvents.UiActionRequested += OnUiAction;
            GameEvents.UiCommandRequested += OnUiCommand;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.AlertRaised -= OnAlert;
            GameEvents.ItemResolved -= OnItemResolved;
            GameEvents.ShiftSucceeded -= OnShiftSucceeded;
            GameEvents.StationFinished -= OnStationFinished;
            GameEvents.Snatched -= OnSnatched;
            GameEvents.Collapsed -= OnCollapsed;
            GameEvents.Complaint -= OnComplaint;
            GameEvents.HeroArrived -= OnHeroArrived;
            GameEvents.PropTapped -= OnPropTapped;
            GameEvents.TestRunFinished -= OnTestRunFinished;
            GameEvents.PuzzlePassed -= OnPuzzlePassed;
            GameEvents.UiActionRequested -= OnUiAction;
            GameEvents.UiCommandRequested -= OnUiCommand;
        }

        // ---------- Public ----------

        /// <summary>Plays a sound effect from Resources/Audio/Sfx by name ("pin", "tap", ...).</summary>
        public void Play(string name)
        {
            var clip = Load("Sfx/" + name);
            if (clip != null) sfx.PlayOneShot(clip);
        }

        // ---------- Music ----------

        void OnStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Title:
                case GameState.Intro:
                    Music("theme");
                    break;
                case GameState.WorldPeace:
                case GameState.Alert:
                case GameState.DayEnd:
                case GameState.DayStart:
                    Music("map");
                    break;
                case GameState.JobIntro:
                case GameState.JobShift:
                case GameState.FinalAttack:
                    Music("shift");
                    break;
                case GameState.BuildIntro:
                case GameState.Build:
                case GameState.Reveal:
                    Music("build");
                    break;
                case GameState.TestRun:
                    Play("run");
                    break;
                case GameState.Peace:
                    Music(null);
                    StartCoroutine(AfterJingle());
                    break;
                case GameState.Collapse:
                    Music(null);
                    break;
            }
        }

        IEnumerator AfterJingle()
        {
            var jingle = Load("Music/peace");
            if (jingle != null) sfx.PlayOneShot(jingle);
            yield return new WaitForSecondsRealtime(jingle != null ? jingle.length : 0f);
            if (Services.Flow.State == GameState.Peace) Music("theme");
        }

        /// <summary>Crossfades to a track (null fades out). The same track keeps playing.</summary>
        void Music(string name)
        {
            if (name == playing) return;
            playing = name;
            var clip = name != null ? Load("Music/" + name) : null;
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Crossfade(clip));
        }

        IEnumerator Crossfade(AudioClip clip)
        {
            var outgoing = music[current];
            var incoming = music[1 - current];
            current = 1 - current;
            if (clip != null)
            {
                incoming.clip = clip;
                incoming.volume = 0f;
                incoming.Play();
            }
            var startOut = outgoing.volume;
            for (var t = 0f; t < FadeSeconds; t += Time.unscaledDeltaTime)
            {
                var k = t / FadeSeconds;
                outgoing.volume = startOut * (1f - k);
                if (clip != null) incoming.volume = MusicVolume * k;
                yield return null;
            }
            outgoing.volume = 0f;
            outgoing.Stop();
            if (clip != null) incoming.volume = MusicVolume;
            fade = null;
        }

        // ---------- Sound effects ----------

        void OnAlert(AlertInfo info) => Play("alert");

        void OnItemResolved(ItemResolvedInfo info) => Play(!info.correct ? "wrong" : info.allowed ? "allow" : "block");

        void OnShiftSucceeded(JobId job) => Play("success");
        void OnStationFinished(string id) => Play("toolkit");
        void OnSnatched(SnatchInfo info) => Play("fail");
        void OnCollapsed(int day) => Play("collapse");
        void OnComplaint(ComplaintInfo info) => Play("wrong");
        void OnHeroArrived(string id) => Play("arrive");
        void OnPropTapped(string id) => Play("prop");
        void OnPuzzlePassed(int puzzleId) => Play("correct");

        void OnTestRunFinished(RunResult result)
        {
            if (!result.passed) Play("wrong");
        }

        void OnUiAction(UiAction action)
        {
            if (action != UiAction.Pause && action != UiAction.Resume) Play("click");
        }

        void OnUiCommand(string command)
        {
            if (command.StartsWith("goin:") || command.StartsWith("materials:") || command.StartsWith("walk:")) Play("click");
        }

        AudioClip Load(string path)
        {
            if (clips.TryGetValue(path, out var clip)) return clip;
            clip = Resources.Load<AudioClip>("Audio/" + path);
            clips[path] = clip;
            return clip;
        }
    }
}
