// Stream B. The top bar: day and progress label, the backup nest and the complaints meter.
// Listens to GameEvents only; never changes game state.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class TopBar : MonoBehaviour
    {
        [SerializeField] GameObject bar;
        [SerializeField] TMP_Text dayLabel;
        [SerializeField] TMP_Text goalText; // Day 3: the puzzle's goal line, in a banner under the bar
        [SerializeField] GameObject goalBanner;
        [SerializeField] TMP_Text backupsLabel;
        [SerializeField] RectTransform backupPips;
        [SerializeField] GameObject complaintsGroup;
        [SerializeField] TMP_Text complaintsLabel;
        [SerializeField] RectTransform complaintPips;
        [SerializeField] GameObject toolkitSpace;
        [SerializeField] Image paxPortrait;      // the player: Pax, next to the pause button // keeps the meters clear of the Cisco toolkit button on the map

        const float PipSize = 36f;

        readonly List<Image> backupImages = new();
        readonly List<Image> complaintImages = new();
        int day = 1;
        int complaintMax;

        void OnEnable()
        {
            GameEvents.StateChanged += OnStateChanged;
            GameEvents.DayStarted += OnDayStarted;
            GameEvents.AlertRaised += OnAlert;
            GameEvents.JobEntered += OnJobEntered;
            GameEvents.PuzzleStarted += OnPuzzleStarted;
            GameEvents.BackupsChanged += OnBackupsChanged;
            GameEvents.Complaint += OnComplaint;
        }

        void OnDisable()
        {
            GameEvents.StateChanged -= OnStateChanged;
            GameEvents.DayStarted -= OnDayStarted;
            GameEvents.AlertRaised -= OnAlert;
            GameEvents.JobEntered -= OnJobEntered;
            GameEvents.PuzzleStarted -= OnPuzzleStarted;
            GameEvents.BackupsChanged -= OnBackupsChanged;
            GameEvents.Complaint -= OnComplaint;
        }

        void Start()
        {
            var ui = UiStrings.Ui;
            if (backupsLabel != null) backupsLabel.text = ui.backups ?? "";
            if (complaintsLabel != null) complaintsLabel.text = ui.complaints ?? "";

            var flow = Services.Flow;
            day = flow.Day;
            dayLabel.text = UiStrings.DayTitle(day);
            SetBarVisible(IsInGame(flow.State));
            ShowComplaints(0, 0);
            OnBackupsChanged(Services.Backups.Left, Services.Backups.Max);
            if (paxPortrait != null)
            {
                paxPortrait.sprite = Portraits.Get(Portraits.Neutral);
                paxPortrait.transform.parent.gameObject.SetActive(paxPortrait.sprite != null);
            }
        }

        static bool IsInGame(GameState state) => state switch
        {
            GameState.Boot or GameState.Title or GameState.Intro or GameState.Collapse or GameState.Peace => false,
            _ => true
        };

        bool inGame;

        void SetBarVisible(bool visible) => inGame = visible;

        // The Splunk Trace draws its own header over the top of the screen: step aside while it is open.
        void LateUpdate()
        {
            var show = inGame && !HuntScreen.IsOpen;
            if (bar != null && bar.activeSelf != show) bar.SetActive(show);
            var goal = show && goalText != null && !string.IsNullOrEmpty(goalText.text);
            if (goalBanner != null && goalBanner.activeSelf != goal) goalBanner.SetActive(goal);
            var map = Stations.IsMapState(Services.Flow.State);
            if (toolkitSpace != null && toolkitSpace.activeSelf != map) toolkitSpace.SetActive(map);
        }

        void OnStateChanged(GameState from, GameState to)
        {
            SetBarVisible(IsInGame(to));
            if (goalText != null && to != GameState.Build && to != GameState.TestRun) goalText.text = "";
            // Every test run starts with a clean complaints meter; the map has no meter (it belongs to a shift).
            if (to == GameState.TestRun) ShowComplaints(0, complaintMax);
            if (Stations.IsMapState(to) && to != GameState.FinalAttack) ShowComplaints(0, 0);
        }

        void OnDayStarted(int newDay, DayMode mode)
        {
            day = newDay;
            dayLabel.text = UiStrings.DayTitle(day);
            ShowComplaints(0, 0); // complaints only show once a shift or test run with a limit starts
        }

        void OnAlert(AlertInfo info) =>
            dayLabel.text = UiStrings.Format(UiStrings.Ui.dayAlertFormat, day, info.groupIndex + 1, info.groupCount);

        void OnPuzzleStarted(int puzzleId, int index, int count)
        {
            dayLabel.text = UiStrings.Format(UiStrings.Ui.dayPuzzleFormat, day, index, count);
            ShowComplaints(0, ComplaintRules.Limit(FindPuzzleLimit(puzzleId)));
            if (goalText != null) goalText.text = FindPuzzle(puzzleId)?.goal ?? "";
        }

        void OnJobEntered(JobContext ctx) =>
            ShowComplaints(0, ctx.isTutorial ? 0 : FindShiftLimit(ctx.day, ctx.job));

        void OnComplaint(ComplaintInfo info) => ShowComplaints(info.count, ComplaintRules.Limit(info.max));

        void OnBackupsChanged(int left, int max)
        {
            EnsurePips(backupPips, backupImages, max);
            for (var i = 0; i < backupImages.Count; i++)
                backupImages[i].color = i < left ? (Color)Palette.ByteCream : Palette.GridDim[2];
        }

        /// <summary>max 0 hides the meter (Day 1, or a shift with complaints off).</summary>
        void ShowComplaints(int count, int max)
        {
            complaintMax = max;
            if (complaintsGroup != null) complaintsGroup.SetActive(max > 0);
            if (max <= 0) return;
            EnsurePips(complaintPips, complaintImages, max);
            for (var i = 0; i < complaintImages.Count; i++)
                complaintImages[i].color = i < count ? (Color)Palette.AlertPink : Palette.GridDim[2];
        }

        static void EnsurePips(RectTransform row, List<Image> images, int count)
        {
            if (row == null) return;
            while (images.Count < count)
            {
                var go = new GameObject($"Pip{images.Count}", typeof(RectTransform), typeof(Image), typeof(Outline));
                go.transform.SetParent(row, false);
                ((RectTransform)go.transform).sizeDelta = new Vector2(PipSize, PipSize);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                var outline = go.GetComponent<Outline>();
                outline.effectColor = Palette.Ink;
                outline.effectDistance = new Vector2(4f, -4f);
                images.Add(image);
            }
            while (images.Count > count)
            {
                Destroy(images[^1].gameObject);
                images.RemoveAt(images.Count - 1);
            }
        }

        static int FindShiftLimit(int forDay, JobId job)
        {
            var days = Services.Data?.Days?.days;
            if (days == null) return 0;
            var key = job.ToKey();
            foreach (var d in days)
            {
                if (d == null || d.day != forDay || d.shifts == null) continue;
                foreach (var shift in d.shifts)
                    if (shift != null && shift.job == key) return shift.maxWrongBlocks;
            }
            return 0;
        }

        static int FindPuzzleLimit(int puzzleId) => FindPuzzle(puzzleId)?.pass?.maxWrongBlocks ?? 0;

        static ByteWorld.Data.PuzzleData FindPuzzle(int puzzleId)
        {
            var puzzles = Services.Data?.Build?.puzzles;
            if (puzzles == null) return null;
            foreach (var p in puzzles)
                if (p != null && p.id == puzzleId) return p;
            return null;
        }
    }
}
