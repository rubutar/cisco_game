// Stream B. The three job screens around a shift (PRD screen flow):
//   Job intro  – place banner + the Byte's intro lines in the bubble, then UiAction.JobIntroDone.
//   Success    – card with the Byte's success line and "Back to Byte World" (UiAction.BackToWorld).
//   Snatched   – card: "<Byte> was snatched!", their last line, the backup's line, backups left,
//                and "Keep watching" (UiAction.KeepWatching).
// Screens only display and send UiActions; GameManager owns every state change.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class JobScreens : MonoBehaviour
    {
        // Day 2 double alerts: a Byte only complains about waiting if the player helped another Byte from the
        // same alert first (every Byte waits a moment after its alert, so time alone isn't enough).
        int alertGroup = -1;
        int jobsEnteredInGroup;

        [Header("Job intro")]
        [SerializeField] GameObject placeBanner;
        [SerializeField] TMP_Text placeText;

        [Header("Result card (success and snatched)")]
        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text cardTitle;
        [SerializeField] TMP_Text cardLine;
        [SerializeField] TMP_Text cardLine2;
        [SerializeField] TMP_Text cardNote;
        [SerializeField] Button cardButton;
        [SerializeField] TMP_Text cardButtonLabel;

        [Header("Pax (B-18)")]
        [SerializeField] Image cardPortrait;    // worried on the snatched card
        [SerializeField] Image bannerPortrait;  // determined on the way in

        JobId currentJob;
        SnatchInfo lastSnatch;
        bool snatchPending; // Snatched event seen, card not shown yet
        UiAction cardAction;

        void Awake()
        {
            cardButton.onClick.AddListener(OnCardButton);
            placeBanner.SetActive(false);
            overlay.SetActive(false);
        }

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.JobEntered += OnJobEntered;
            GameEvents.ShiftSucceeded += OnShiftSucceeded;
            GameEvents.Snatched += OnSnatched;
            GameEvents.AlertRaised += OnAlertRaised;
            GameEvents.DayStarted += OnDayStarted;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.JobEntered -= OnJobEntered;
            GameEvents.ShiftSucceeded -= OnShiftSucceeded;
            GameEvents.Snatched -= OnSnatched;
            GameEvents.AlertRaised -= OnAlertRaised;
            GameEvents.DayStarted -= OnDayStarted;
        }

        // AlertController raises the same group again for the Bytes still waiting; only a new group resets.
        void OnAlertRaised(AlertInfo info)
        {
            if (info.groupIndex == alertGroup) return;
            alertGroup = info.groupIndex;
            jobsEnteredInGroup = 0;
        }

        void OnDayStarted(int day, DayMode mode) => alertGroup = -1;

        // ---------- Job intro ----------

        // GameManager sets JobIntro first and raises JobEntered right after, so the intro starts here.
        void OnJobEntered(JobContext ctx)
        {
            currentJob = ctx.job;
            var keptWaiting = jobsEnteredInGroup > 0;
            jobsEnteredInGroup++;
            if (Services.Flow.State != GameState.JobIntro) return;

            var job = UiStrings.Job(ctx.job);
            var lines = UiStrings.JobDay(ctx.job, ctx.day);
            placeText.text = job?.place ?? "";
            placeBanner.SetActive(!string.IsNullOrEmpty(placeText.text));
            SetPortrait(bannerPortrait, Portraits.Determined);

            var say = new System.Collections.Generic.List<string>();
            if (keptWaiting && !string.IsNullOrEmpty(lines?.waitingLine)) say.Add(lines.waitingLine);
            if (lines?.introLines != null) say.AddRange(lines.introLines);

            Services.Dialogue.SayLines(ctx.job.ToKey(), say, () => GameEvents.RaiseUiAction(UiAction.JobIntroDone));
        }

        // ---------- Success and snatched ----------

        // Same ordering as Snatched: the state may already be JobSuccess, so redraw with the right Byte.
        void OnShiftSucceeded(JobId job)
        {
            currentJob = job;
            if (Services.Flow.State == GameState.JobSuccess) ShowSuccess();
        }

        // GameManager switches to Snatched from inside the same Snatched event, so the state change can
        // arrive before or after this handler runs. Show the card once both have happened.
        void OnSnatched(SnatchInfo info)
        {
            if (info.source != SnatchSource.JobShift) return;
            lastSnatch = info;
            snatchPending = true;
            if (Services.Flow.State == GameState.Snatched) ShowSnatched();
        }

        void OnStateChanged(GameState from, GameState to)
        {
            if (to != GameState.JobIntro) placeBanner.SetActive(false);
            overlay.SetActive(false);

            switch (to)
            {
                case GameState.JobSuccess: ShowSuccess(); break;
                case GameState.Snatched when snatchPending: ShowSnatched(); break;
            }
        }

        void ShowSuccess()
        {
            var job = UiStrings.Job(currentJob);
            var lines = UiStrings.JobDay(currentJob, Services.Flow.Day);

            // v2: the station's Field Card, with Back to Byte World (B-03). Old content without one keeps the plain card.
            var card = lines?.fieldCard;
            if (!string.IsNullOrEmpty(card?.term) && RevealCard.Instance != null)
            {
                Services.Inspect.Hide();
                Services.Dialogue.Hide();
                overlay.SetActive(false);
                RevealCard.Instance.ShowFieldCard(currentJob.ToKey(), lines.successLine, card, UiStrings.Ui.backToWorld,
                    () => GameEvents.RaiseUiAction(UiAction.BackToWorld));
                return;
            }

            ShowCard(
                title: job?.name ?? "",
                titleColor: Palette.SignalBlue,
                line: lines?.successLine,
                line2: null,
                note: null,
                button: UiStrings.Ui.backToWorld,
                action: UiAction.BackToWorld);
        }

        void ShowSnatched()
        {
            snatchPending = false;
            var job = UiStrings.Job(lastSnatch.job);
            var lines = UiStrings.JobDay(lastSnatch.job, Services.Flow.Day);
            var ui = UiStrings.Ui;
            // A full complaints meter is a different story from an attacker getting through (PRD 4.5).
            var lastWords = lastSnatch.attacker == "complaints" ? lines?.complaintFailLine : lines?.snatchLine;
            ShowCard(
                title: UiStrings.Format(ui.snatchedFormat, job?.name ?? ""),
                titleColor: Palette.AlertPink,
                line: lastWords,
                line2: lines?.backupLine,
                note: null, // no backups count: the team dropped the backups meter
                button: ui.keepWatching,
                action: UiAction.KeepWatching);
        }

        void ShowCard(string title, Color titleColor, string line, string line2, string note, string button, UiAction action)
        {
            // The shift is over or paused: clear anything the job left open.
            Services.Inspect.Hide();
            Services.Dialogue.Hide();

            SetText(cardTitle, title);
            cardTitle.color = titleColor;
            SetText(cardLine, line);
            SetText(cardLine2, line2);
            SetText(cardNote, note);
            cardButtonLabel.text = button ?? "";
            cardAction = action;
            if (action == UiAction.KeepWatching) SetPortrait(cardPortrait, Portraits.Worried);
            else if (cardPortrait != null) cardPortrait.gameObject.SetActive(false);
            overlay.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)cardTitle.transform.parent);
        }

        static void SetPortrait(Image image, string expression)
        {
            if (image == null) return;
            image.sprite = Portraits.Get(expression);
            image.gameObject.SetActive(image.sprite != null);
        }

        static void SetText(TMP_Text text, string value)
        {
            text.text = value ?? "";
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        void OnCardButton()
        {
            if (!overlay.activeSelf) return;
            overlay.SetActive(false);
            GameEvents.RaiseUiAction(cardAction);
        }
    }
}
