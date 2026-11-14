// Stream B. Builds Resources/UI/UIRoot.prefab through the Editor API, so the prefab YAML is never
// written by hand. Re-running replaces the prefab. Colours come from Palette; no game text here
// (every label is filled at runtime from dialogue.json).
// Menu: Byte World > UI > Build UIRoot Prefab
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ByteWorld.EditorTools
{
    public static class UIRootBuilder
    {
        public const string PrefabPath = "Assets/_Project/Resources/UI/UIRoot.prefab";

        [MenuItem("Byte World/UI/Build UIRoot Prefab")]
        public static void Build()
        {
            var root = new GameObject("UIRoot", typeof(RectTransform));
            try
            {
                BuildCanvas(root);
                BuildEventSystem(root.transform);
                BuildTitleScreen(root.transform);
                BuildIntroScreen(root.transform);
                var safe = NewRect("SafeArea", root.transform);
                Stretch(safe);
                safe.gameObject.AddComponent<SafeArea>();
                BuildBuildView(safe);
                BuildTopBar(safe);
                BuildInspectPanel(safe);
                BuildDialogueBubble(safe);
                BuildJobScreens(root.transform);
                BuildDayCards(root.transform);
                BuildCollapseScreen(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out var ok);
                if (ok) Debug.Log($"[UIRootBuilder] Saved {PrefabPath}");
                else Debug.LogError($"[UIRootBuilder] Could not save {PrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void BuildCanvas(GameObject root)
        {
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.pixelPerfect = true;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();
        }

        static void BuildEventSystem(Transform parent)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetParent(parent, false);
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        // ---------- Title screen ----------

        static void BuildTitleScreen(Transform parent)
        {
            var host = NewRect("TitleScreen", parent);
            Stretch(host);
            var title = host.gameObject.AddComponent<TitleScreen>();

            // Full-screen backdrop: covers the whole screen (not just the safe area) and blocks world taps.
            var screen = NewRect("Screen", host);
            Stretch(screen);
            AddImage(screen.gameObject, Palette.Ink, true);

            // Blue copy offset behind the title: a chunky pixel drop shadow (uGUI Shadow doesn't affect TMP).
            var logoShadow = NewText("LogoShadow", screen, 180, Palette.SignalBlue, TextAlignmentOptions.Center);
            logoShadow.rectTransform.anchorMin = logoShadow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            logoShadow.rectTransform.anchoredPosition = new Vector2(12, 158);
            logoShadow.rectTransform.sizeDelta = new Vector2(1600, 240);
            logoShadow.fontStyle = FontStyles.Bold;

            var logo = NewText("Logo", screen, 180, Palette.ByteCream, TextAlignmentOptions.Center);
            logo.rectTransform.anchorMin = logo.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            logo.rectTransform.anchoredPosition = new Vector2(0, 170);
            logo.rectTransform.sizeDelta = new Vector2(1600, 240);
            logo.fontStyle = FontStyles.Bold;
            logo.raycastTarget = true; // tappable for the 5-tap debug menu
            logo.gameObject.AddComponent<SecretTaps>();

            var tagline = NewText("Tagline", screen, 52, Palette.NeonCyan, TextAlignmentOptions.Center);
            tagline.rectTransform.anchorMin = tagline.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            tagline.rectTransform.anchoredPosition = new Vector2(0, 20);
            tagline.rectTransform.sizeDelta = new Vector2(1600, 80);

            var play = NewRect("PlayButton", screen);
            play.anchorMin = play.anchorMax = new Vector2(0.5f, 0.5f);
            play.anchoredPosition = new Vector2(0, -190);
            play.sizeDelta = new Vector2(440, 130);
            var playImage = AddImage(play.gameObject, Palette.AlertPink, true);
            var playOutline = play.gameObject.AddComponent<Outline>();
            playOutline.effectColor = Palette.ByteCream;
            playOutline.effectDistance = new Vector2(8, -8);
            var button = play.gameObject.AddComponent<Button>();
            button.targetGraphic = playImage;
            var playLabel = NewText("Label", play, 64, Palette.ByteCream, TextAlignmentOptions.Center);
            Stretch(playLabel.rectTransform);

            var so = new SerializedObject(title);
            so.FindProperty("screen").objectReferenceValue = screen.gameObject;
            so.FindProperty("titleText").objectReferenceValue = logo;
            so.FindProperty("titleShadow").objectReferenceValue = logoShadow;
            so.FindProperty("taglineText").objectReferenceValue = tagline;
            so.FindProperty("playButton").objectReferenceValue = button;
            so.FindProperty("playLabel").objectReferenceValue = playLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Job intro, success and snatched ----------

        static void BuildJobScreens(Transform parent)
        {
            var host = NewRect("JobScreens", parent);
            Stretch(host);
            var screens = host.gameObject.AddComponent<JobScreens>();

            // Place banner for the job intro: a tag under the top bar. Doesn't block taps.
            var banner = NewRect("PlaceBanner", host);
            banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 1);
            banner.pivot = new Vector2(0.5f, 1);
            banner.anchoredPosition = new Vector2(0, -150);
            banner.sizeDelta = new Vector2(0, 96);
            AddImage(banner.gameObject, Palette.SignalBlue, false);
            AddOutline(banner.gameObject, 8);
            var bannerLayout = banner.gameObject.AddComponent<HorizontalLayoutGroup>();
            bannerLayout.padding = new RectOffset(56, 56, 0, 0);
            bannerLayout.childAlignment = TextAnchor.MiddleCenter;
            bannerLayout.childControlWidth = true;
            bannerLayout.childControlHeight = true;
            bannerLayout.childForceExpandWidth = false;
            bannerLayout.childForceExpandHeight = true;
            banner.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var place = NewText("Place", banner, 52, Palette.ByteCream, TextAlignmentOptions.Center);
            place.fontStyle = FontStyles.Bold;

            // Result card: dims the whole screen and blocks taps until the button is pressed.
            var overlay = NewRect("Overlay", host);
            Stretch(overlay);
            var dim = (Color)Palette.Ink;
            dim.a = 0.75f;
            AddImage(overlay.gameObject, dim, true);

            var card = NewRect("Card", overlay);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1100, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, 10);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(64, 64, 56, 56);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewText("Title", card, 68, Palette.SignalBlue, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            title.textWrappingMode = TextWrappingModes.Normal;
            var line = NewText("Line", card, 46, Palette.Ink, TextAlignmentOptions.Center);
            line.fontStyle = FontStyles.Italic;
            line.textWrappingMode = TextWrappingModes.Normal;
            var line2 = NewText("Line2", card, 42, Palette.SignalBlue, TextAlignmentOptions.Center);
            line2.textWrappingMode = TextWrappingModes.Normal;
            var note = NewText("Note", card, 34, Palette.CliffLight, TextAlignmentOptions.Center);
            note.textWrappingMode = TextWrappingModes.Normal;

            var buttonRow = NewRect("ButtonRow", card);
            var rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 12, 0);
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var button = NewButton("Button", buttonRow, Palette.AlertPink, out var buttonLabel);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = 560;
            element.preferredWidth = 560;
            buttonLabel.fontSize = 44;

            var so = new SerializedObject(screens);
            so.FindProperty("placeBanner").objectReferenceValue = banner.gameObject;
            so.FindProperty("placeText").objectReferenceValue = place;
            so.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
            so.FindProperty("cardTitle").objectReferenceValue = title;
            so.FindProperty("cardLine").objectReferenceValue = line;
            so.FindProperty("cardLine2").objectReferenceValue = line2;
            so.FindProperty("cardNote").objectReferenceValue = note;
            so.FindProperty("cardButton").objectReferenceValue = button;
            so.FindProperty("cardButtonLabel").objectReferenceValue = buttonLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Day end and day start ----------

        static void BuildDayCards(Transform parent)
        {
            var host = NewRect("DayCards", parent);
            Stretch(host);
            var cards = host.gameObject.AddComponent<DayCards>();

            var screen = NewRect("Screen", host);
            Stretch(screen);
            var backdrop = AddImage(screen.gameObject, Palette.Ink, true); // colour set per card at runtime

            var content = NewRect("Content", screen);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(1400, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 36;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewText("Title", content, 140, Palette.ByteCream, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;

            var tag = NewRect("SpeakerTag", content);
            AddImage(tag.gameObject, Palette.NeonCyan, false);
            AddOutline(tag.gameObject, 6);
            var tagLayout = tag.gameObject.AddComponent<HorizontalLayoutGroup>();
            tagLayout.padding = new RectOffset(40, 40, 10, 10);
            tagLayout.childControlWidth = true;
            tagLayout.childControlHeight = true;
            tagLayout.childForceExpandWidth = false; // hug the name instead of filling the row
            tagLayout.childForceExpandHeight = false;
            var speaker = NewText("Name", tag, 40, Palette.Ink, TextAlignmentOptions.Center);

            var line = NewText("Line", content, 56, Palette.ByteCream, TextAlignmentOptions.Center);
            line.fontStyle = FontStyles.Italic;
            line.textWrappingMode = TextWrappingModes.Normal;
            var lineElement = line.gameObject.AddComponent<LayoutElement>();
            lineElement.preferredWidth = 1400;

            var buttonRow = NewRect("ButtonRow", content);
            var rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 24, 0);
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var button = NewButton("Button", buttonRow, Palette.AlertPink, out var label);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = 520;
            element.preferredWidth = 520;
            element.preferredHeight = 120;
            label.fontSize = 48;

            var so = new SerializedObject(cards);
            so.FindProperty("screen").objectReferenceValue = screen.gameObject;
            so.FindProperty("backdrop").objectReferenceValue = backdrop;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("speakerTag").objectReferenceValue = tag.gameObject;
            so.FindProperty("speakerText").objectReferenceValue = speaker;
            so.FindProperty("lineText").objectReferenceValue = line;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("buttonLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Collapse ----------

        static void BuildCollapseScreen(Transform parent)
        {
            var host = NewRect("CollapseScreen", parent);
            Stretch(host);
            var collapse = host.gameObject.AddComponent<CollapseScreen>();

            // Dim mood (PRD 6.6): the whole town greys out.
            var screen = NewRect("Screen", host);
            Stretch(screen);
            AddImage(screen.gameObject, Palette.GridDim[1], true);

            var content = NewRect("Content", screen);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(1400, 0);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 64;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var line = NewText("Line", content, 64, Palette.GridDim[4], TextAlignmentOptions.Center);
            line.textWrappingMode = TextWrappingModes.Normal;
            line.gameObject.AddComponent<LayoutElement>().preferredWidth = 1400;

            var buttonRow = NewRect("ButtonRow", content);
            var rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var button = NewButton("Button", buttonRow, Palette.AlertPink, out var label);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = 520;
            element.preferredWidth = 520;
            element.preferredHeight = 120;
            label.fontSize = 48;

            var so = new SerializedObject(collapse);
            so.FindProperty("screen").objectReferenceValue = screen.gameObject;
            so.FindProperty("lineText").objectReferenceValue = line;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("buttonLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Intro panels ----------

        static void BuildIntroScreen(Transform parent)
        {
            var host = NewRect("IntroScreen", parent);
            Stretch(host);
            var intro = host.gameObject.AddComponent<IntroScreen>();

            var screen = NewRect("Screen", host);
            Stretch(screen);
            AddImage(screen.gameObject, Palette.Ink, true);

            var text = NewText("PanelText", screen, 76, Palette.ByteCream, TextAlignmentOptions.Center);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = new Vector2(0, 60);
            text.rectTransform.sizeDelta = new Vector2(1500, 500);
            text.textWrappingMode = TextWrappingModes.Normal;

            var dots = NewRect("Dots", screen);
            dots.anchorMin = dots.anchorMax = new Vector2(0.5f, 0);
            dots.pivot = new Vector2(0.5f, 0);
            dots.anchoredPosition = new Vector2(0, 120);
            dots.sizeDelta = new Vector2(600, 40);
            var dotsLayout = dots.gameObject.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 24;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = false;
            dotsLayout.childControlHeight = false;
            dotsLayout.childForceExpandWidth = false;
            dotsLayout.childForceExpandHeight = false;

            var next = NewRect("NextButton", screen);
            next.anchorMin = next.anchorMax = new Vector2(1, 0);
            next.pivot = new Vector2(1, 0);
            next.anchoredPosition = new Vector2(-96, 80);
            next.sizeDelta = new Vector2(360, 120);
            var nextImage = AddImage(next.gameObject, Palette.AlertPink, true);
            var nextOutline = next.gameObject.AddComponent<Outline>();
            nextOutline.effectColor = Palette.ByteCream;
            nextOutline.effectDistance = new Vector2(6, -6);
            var nextButton = next.gameObject.AddComponent<Button>();
            nextButton.targetGraphic = nextImage;
            var nextLabel = NewText("Label", next, 52, Palette.ByteCream, TextAlignmentOptions.Center);
            Stretch(nextLabel.rectTransform);

            // Skip: a quiet text button in the corner (still a big tap area for phones).
            var skip = NewRect("SkipButton", screen);
            skip.anchorMin = skip.anchorMax = new Vector2(1, 1);
            skip.pivot = new Vector2(1, 1);
            skip.anchoredPosition = new Vector2(-64, -48);
            skip.sizeDelta = new Vector2(240, 96);
            var skipImage = AddImage(skip.gameObject, new Color(0, 0, 0, 0), true);
            var skipButton = skip.gameObject.AddComponent<Button>();
            skipButton.targetGraphic = skipImage;
            var skipLabel = NewText("Label", skip, 40, Palette.NeonCyan, TextAlignmentOptions.Center);
            Stretch(skipLabel.rectTransform);

            var so = new SerializedObject(intro);
            so.FindProperty("screen").objectReferenceValue = screen.gameObject;
            so.FindProperty("panelText").objectReferenceValue = text;
            so.FindProperty("dots").objectReferenceValue = dots;
            so.FindProperty("nextButton").objectReferenceValue = nextButton;
            so.FindProperty("nextLabel").objectReferenceValue = nextLabel;
            so.FindProperty("skipButton").objectReferenceValue = skipButton;
            so.FindProperty("skipLabel").objectReferenceValue = skipLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Day 3 build mode ----------

        static void BuildBuildView(RectTransform parent)
        {
            var host = NewRect("BuildMode", parent);
            Stretch(host);
            var manager = host.gameObject.AddComponent<BuildManager>();

            var view = NewRect("View", host);
            Stretch(view);
            // Blueprint backdrop: blocks taps to anything behind and sets the build-day mood.
            AddImage(view.gameObject, Palette.CliffDark, true);

            // The board keeps the map's 16:9 shape and scales to fit; slots are placed by map coordinates.
            var board = NewRect("Board", view);
            Stretch(board);
            var fitter = board.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = BuildManager.MapWidth / BuildManager.MapHeight;
            var boardTint = (Color)Palette.SignalBlue;
            boardTint.a = 0.35f;
            AddImage(board.gameObject, boardTint, false);

            var cables = NewRect("Cables", board);
            Stretch(cables);
            var slots = NewRect("Slots", board);
            Stretch(slots);

            // Tray of pieces to place, bottom left.
            var tray = NewRect("Tray", view);
            tray.anchorMin = tray.anchorMax = new Vector2(0, 0);
            tray.pivot = new Vector2(0, 0);
            tray.anchoredPosition = new Vector2(48, 48);
            var trayColor = (Color)Palette.Ink;
            trayColor.a = 0.8f;
            AddImage(tray.gameObject, trayColor, true);
            AddOutline(tray.gameObject, 6);
            var trayLayout = tray.gameObject.AddComponent<HorizontalLayoutGroup>();
            trayLayout.padding = new RectOffset(36, 48, 36, 36);
            trayLayout.spacing = 40;
            trayLayout.childAlignment = TextAnchor.MiddleLeft;
            trayLayout.childControlWidth = true;
            trayLayout.childControlHeight = true;
            trayLayout.childForceExpandWidth = false;
            trayLayout.childForceExpandHeight = false;
            var trayFit = tray.gameObject.AddComponent<ContentSizeFitter>();
            trayFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            trayFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var so = new SerializedObject(manager);
            so.FindProperty("view").objectReferenceValue = view.gameObject;
            so.FindProperty("board").objectReferenceValue = board;
            so.FindProperty("cableLayer").objectReferenceValue = cables;
            so.FindProperty("slotLayer").objectReferenceValue = slots;
            so.FindProperty("tray").objectReferenceValue = tray;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Top bar ----------

        static void BuildTopBar(RectTransform parent)
        {
            var host = NewRect("TopBar", parent);
            Stretch(host);
            var topBar = host.gameObject.AddComponent<TopBar>();

            var bar = NewRect("Bar", host);
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, 110);
            var barColor = (Color)Palette.Ink;
            barColor.a = 0.85f;
            AddImage(bar.gameObject, barColor, false);

            var dayLabel = NewText("DayLabel", bar, 44, Palette.ByteCream, TextAlignmentOptions.MidlineLeft);
            dayLabel.rectTransform.anchorMin = new Vector2(0, 0);
            dayLabel.rectTransform.anchorMax = new Vector2(0.5f, 1);
            dayLabel.rectTransform.offsetMin = new Vector2(48, 0);
            dayLabel.rectTransform.offsetMax = Vector2.zero;

            var right = NewRect("Right", bar);
            right.anchorMin = new Vector2(1, 0);
            right.anchorMax = new Vector2(1, 1);
            right.pivot = new Vector2(1, 0.5f);
            right.anchoredPosition = new Vector2(-48, 0);
            right.sizeDelta = new Vector2(0, 0);
            var rightLayout = right.gameObject.AddComponent<HorizontalLayoutGroup>();
            ConfigureRow(rightLayout, 64, TextAnchor.MiddleRight);
            right.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildMeter(right, "Backups", out var backupsLabel, out var backupPips);
            var complaints = BuildMeter(right, "Complaints", out var complaintsLabel, out var complaintPips);

            var so = new SerializedObject(topBar);
            so.FindProperty("bar").objectReferenceValue = bar.gameObject;
            so.FindProperty("dayLabel").objectReferenceValue = dayLabel;
            so.FindProperty("backupsLabel").objectReferenceValue = backupsLabel;
            so.FindProperty("backupPips").objectReferenceValue = backupPips;
            so.FindProperty("complaintsGroup").objectReferenceValue = complaints.gameObject;
            so.FindProperty("complaintsLabel").objectReferenceValue = complaintsLabel;
            so.FindProperty("complaintPips").objectReferenceValue = complaintPips;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static RectTransform BuildMeter(RectTransform parent, string name, out TMP_Text label, out RectTransform pips)
        {
            var group = NewRect(name, parent);
            ConfigureRow(group.gameObject.AddComponent<HorizontalLayoutGroup>(), 18, TextAnchor.MiddleLeft);

            label = NewText("Label", group, 32, Palette.ByteCream, TextAlignmentOptions.MidlineRight);

            pips = NewRect("Pips", group);
            var pipLayout = pips.gameObject.AddComponent<HorizontalLayoutGroup>();
            pipLayout.spacing = 14;
            pipLayout.childAlignment = TextAnchor.MiddleLeft;
            pipLayout.childControlWidth = false;
            pipLayout.childControlHeight = false;
            pipLayout.childForceExpandWidth = false;
            pipLayout.childForceExpandHeight = false;
            return group;
        }

        static void ConfigureRow(HorizontalLayoutGroup layout, float spacing, TextAnchor alignment)
        {
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        // ---------- Inspect panel ----------

        static void BuildInspectPanel(RectTransform parent)
        {
            var host = NewRect("InspectPanel", parent);
            Stretch(host);
            var panel = host.gameObject.AddComponent<InspectPanel>();

            // Card on the right, under the top bar; grows downwards with its content.
            var card = NewRect("Card", host);
            card.anchorMin = card.anchorMax = new Vector2(1, 1);
            card.pivot = new Vector2(1, 1);
            card.anchoredPosition = new Vector2(-60, -150);
            card.sizeDelta = new Vector2(760, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, 8);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(44, 44, 40, 40);
            layout.spacing = 22;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewText("Title", card, 50, Palette.Ink, TextAlignmentOptions.TopLeft);
            title.fontStyle = FontStyles.Bold;
            title.textWrappingMode = TextWrappingModes.Normal;

            var so = new SerializedObject(panel);
            var rows = so.FindProperty("fieldRows");
            var labels = so.FindProperty("fieldLabels");
            var values = so.FindProperty("fieldValues");
            rows.arraySize = labels.arraySize = values.arraySize = InspectPanel.MaxFields;
            for (var i = 0; i < InspectPanel.MaxFields; i++)
            {
                var row = NewRect($"Field{i}", card);
                var rowLayout = row.gameObject.AddComponent<VerticalLayoutGroup>();
                rowLayout.spacing = 4;
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = true;
                rowLayout.childForceExpandWidth = true;
                rowLayout.childForceExpandHeight = false;

                var label = NewText("Label", row, 30, Palette.SignalBlue, TextAlignmentOptions.TopLeft);
                var value = NewText("Value", row, 42, Palette.Ink, TextAlignmentOptions.TopLeft);
                value.textWrappingMode = TextWrappingModes.Normal;

                rows.GetArrayElementAtIndex(i).objectReferenceValue = row.gameObject;
                labels.GetArrayElementAtIndex(i).objectReferenceValue = label;
                values.GetArrayElementAtIndex(i).objectReferenceValue = value;
            }

            var buttons = NewRect("Buttons", card);
            var buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 28;
            buttonLayout.padding = new RectOffset(0, 0, 16, 0);
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = false;

            // Same weight for both choices: the colour must not hint which one is right.
            var primary = NewButton("Primary", buttons, Palette.SignalBlue, out var primaryLabel);
            var secondary = NewButton("Secondary", buttons, Palette.SignalBlue, out var secondaryLabel);

            so.FindProperty("card").objectReferenceValue = card.gameObject;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("primaryButton").objectReferenceValue = primary;
            so.FindProperty("primaryLabel").objectReferenceValue = primaryLabel;
            so.FindProperty("secondaryButton").objectReferenceValue = secondary;
            so.FindProperty("secondaryLabel").objectReferenceValue = secondaryLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Button NewButton(string name, Transform parent, Color color, out TextMeshProUGUI label)
        {
            var rect = NewRect(name, parent);
            var image = AddImage(rect.gameObject, color, true);
            AddOutline(rect.gameObject, 6);
            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 104;
            element.preferredHeight = 104;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = NewText("Label", rect, 38, Palette.ByteCream, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12, 0);
            label.rectTransform.offsetMax = new Vector2(-12, 0);
            label.textWrappingMode = TextWrappingModes.Normal;
            return button;
        }

        // ---------- Dialogue bubble ----------

        static void BuildDialogueBubble(RectTransform parent)
        {
            var host = NewRect("DialogueBubble", parent);
            Stretch(host);
            var bubble = host.gameObject.AddComponent<DialogueBubble>();

            var catcher = NewRect("TapCatcher", host);
            Stretch(catcher);
            AddImage(catcher.gameObject, new Color(0, 0, 0, 0), true);

            var panel = NewRect("Panel", host);
            // Stretches with the screen, 120 px margin each side, so it fits any landscape aspect.
            panel.anchorMin = new Vector2(0, 0);
            panel.anchorMax = new Vector2(1, 0);
            panel.pivot = new Vector2(0.5f, 0);
            panel.anchoredPosition = new Vector2(0, 40);
            panel.sizeDelta = new Vector2(-240, 280);
            AddImage(panel.gameObject, Palette.ByteCream, true);
            AddOutline(panel.gameObject, 8);

            var nameTag = NewRect("NameTag", panel);
            nameTag.anchorMin = nameTag.anchorMax = new Vector2(0, 1);
            nameTag.pivot = new Vector2(0, 0.5f);
            nameTag.anchoredPosition = new Vector2(40, 0);
            nameTag.sizeDelta = new Vector2(380, 68);
            AddImage(nameTag.gameObject, Palette.SignalBlue, false);
            AddOutline(nameTag.gameObject, 6);
            var nameText = NewText("Name", nameTag, 36, Palette.ByteCream, TextAlignmentOptions.Center);
            Stretch(nameText.rectTransform);

            var body = NewText("Body", panel, 44, Palette.Ink, TextAlignmentOptions.TopLeft);
            Stretch(body.rectTransform);
            body.rectTransform.offsetMin = new Vector2(56, 100);
            body.rectTransform.offsetMax = new Vector2(-56, -52);
            body.textWrappingMode = TextWrappingModes.Normal;

            var cont = NewRect("ContinueIcon", panel);
            cont.anchorMin = cont.anchorMax = new Vector2(1, 0);
            cont.pivot = new Vector2(0.5f, 0.5f);
            cont.anchoredPosition = new Vector2(-40, 36);
            cont.sizeDelta = new Vector2(28, 28);
            cont.localRotation = Quaternion.Euler(0, 0, 45);
            AddImage(cont.gameObject, Palette.SignalBlue, false);

            var retry = NewRect("RetryButton", panel);
            retry.anchorMin = retry.anchorMax = new Vector2(1, 0);
            retry.pivot = new Vector2(1, 0);
            retry.anchoredPosition = new Vector2(-32, 24);
            retry.sizeDelta = new Vector2(320, 84);
            var retryImage = AddImage(retry.gameObject, Palette.AlertPink, true);
            AddOutline(retry.gameObject, 6);
            var button = retry.gameObject.AddComponent<Button>();
            button.targetGraphic = retryImage;
            var retryLabel = NewText("Label", retry, 36, Palette.ByteCream, TextAlignmentOptions.Center);
            Stretch(retryLabel.rectTransform);

            var pointer = NewRect("Pointer", host);
            pointer.anchorMin = pointer.anchorMax = new Vector2(0.5f, 0.5f);
            pointer.pivot = new Vector2(0.5f, 0.5f);
            pointer.sizeDelta = new Vector2(44, 44);
            pointer.localRotation = Quaternion.Euler(0, 0, 45);
            AddImage(pointer.gameObject, Palette.AlertPink, false);
            AddOutline(pointer.gameObject, 6);

            var so = new SerializedObject(bubble);
            so.FindProperty("tapCatcher").objectReferenceValue = catcher.gameObject;
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("nameTag").objectReferenceValue = nameTag.gameObject;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("continueIcon").objectReferenceValue = cont.gameObject;
            so.FindProperty("retryButton").objectReferenceValue = button;
            so.FindProperty("retryLabel").objectReferenceValue = retryLabel;
            so.FindProperty("pointer").objectReferenceValue = pointer;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Helpers ----------

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static Image AddImage(GameObject go, Color color, bool raycast)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        static void AddOutline(GameObject go, float size)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(size, -size);
        }

        static TextMeshProUGUI NewText(string name, Transform parent, float size, Color color, TextAlignmentOptions align)
        {
            var rect = NewRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.text = "";
            return text;
        }
    }
}
