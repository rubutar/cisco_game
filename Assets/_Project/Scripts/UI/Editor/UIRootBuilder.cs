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
                // Map UI, bottom to top: station bubbles, the comms dock, the station popup and card,
                // then the materials panel over all of them.
                var materials = BuildMaterialsPanel(safe);
                var cardLayer = BuildCheckpointMap(safe, materials);
                BuildTownSpace(safe);
                BuildSplunkLog(safe);
                cardLayer.SetAsLastSibling();
                materials.transform.SetAsLastSibling();
                BuildInspectPanel(safe);
                BuildDialogueBubble(safe);
                BuildToolkit(safe);
                BuildPauseMenu(safe);
                BuildJobScreens(root.transform);
                BuildPropCard(root.transform);
                BuildBuildIntro(root.transform);
                BuildRevealCard(root.transform);
                BuildDayCards(root.transform);
                var incident = BuildIncidentReport(root.transform);
                BuildQuestCheck(root.transform, incident);
                BuildCollapseScreen(root.transform);
                BuildPeaceScreen(root.transform);
                BuildIris(root.transform);

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
            var logoShadow = NewText("LogoShadow", screen, 180, Palette.SignalBlue, TextAlignmentOptions.Center, UiFont.Title);
            logoShadow.rectTransform.anchorMin = logoShadow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            logoShadow.rectTransform.anchoredPosition = new Vector2(12, 158);
            logoShadow.rectTransform.sizeDelta = new Vector2(1600, 240);

            var logo = NewText("Logo", screen, 180, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            logo.rectTransform.anchorMin = logo.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            logo.rectTransform.anchoredPosition = new Vector2(0, 170);
            logo.rectTransform.sizeDelta = new Vector2(1600, 240);
            logo.raycastTarget = true; // tappable for the 5-tap debug menu
            logo.gameObject.AddComponent<SecretTaps>();

            var tagline = NewText("Tagline", screen, 52, Palette.NeonCyan, TextAlignmentOptions.Center);
            tagline.rectTransform.anchorMin = tagline.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            tagline.rectTransform.anchoredPosition = new Vector2(0, 20);
            tagline.rectTransform.sizeDelta = new Vector2(1600, 80);

            var button = NewButton("PlayButton", screen, ButtonStyle.Main, out var playLabel);
            Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -190), new Vector2(440, 130));
            playLabel.fontSize = 64;

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
            var place = NewText("Place", banner, 52, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);

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

            var title = NewText("Title", card, 68, Palette.SignalBlue, TextAlignmentOptions.Center, UiFont.Title);
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
            var button = NewButton("Button", buttonRow, ButtonStyle.Main, out var buttonLabel);
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
            so.FindProperty("cardPortrait").objectReferenceValue = NewPortrait(card, new Vector2(0, 1), new Vector2(-40, 40), 160);
            so.FindProperty("bannerPortrait").objectReferenceValue = NewPortrait(banner, new Vector2(0, 0.5f), new Vector2(-150, 0), 132);
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

            var title = NewText("Title", content, 140, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);

            var tag = NewRect("SpeakerTag", content);
            AddImage(tag.gameObject, Palette.NeonCyan, false);
            AddOutline(tag.gameObject, 6);
            var tagLayout = tag.gameObject.AddComponent<HorizontalLayoutGroup>();
            tagLayout.padding = new RectOffset(40, 40, 10, 10);
            tagLayout.childControlWidth = true;
            tagLayout.childControlHeight = true;
            tagLayout.childForceExpandWidth = false; // hug the name instead of filling the row
            tagLayout.childForceExpandHeight = false;
            var speaker = NewText("Name", tag, 40, Palette.Ink, TextAlignmentOptions.Center, UiFont.Title);

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
            var button = NewButton("Button", buttonRow, ButtonStyle.Main, out var label);
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
            var button = NewButton("Button", buttonRow, ButtonStyle.Main, out var label);
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

            var nextButton = NewButton("NextButton", screen, ButtonStyle.Main, out var nextLabel);
            Place((RectTransform)nextButton.transform, new Vector2(1, 0), new Vector2(-96, 80), new Vector2(360, 120));
            nextLabel.fontSize = 52;

            // Skip: a quiet text button in the corner (still a big tap area for phones).
            var skip = NewRect("SkipButton", screen);
            skip.anchorMin = skip.anchorMax = new Vector2(1, 1);
            skip.pivot = new Vector2(1, 1);
            skip.anchoredPosition = new Vector2(-64 - SettingsRoom, -48);
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

            // The board keeps the map's 16:9 shape and scales to fit between the top bar and the tray / Run row,
            // so no piece hides under a button; slots are placed by map coordinates.
            var area = NewRect("BoardArea", view);
            Stretch(area);
            area.offsetMin = new Vector2(0, 170);
            area.offsetMax = new Vector2(0, -BuildTop); // below the top bar and the goal banner
            var board = NewRect("Board", area);
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
            var packets = NewRect("Packets", board);
            Stretch(packets);

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

            BuildRunnerParts(host.gameObject.AddComponent<BuildRunner>(), view, packets);
            BuildRulesPanel(host.gameObject.AddComponent<RulesPanel>(), manager, view, area);
            BuildSiegeParts(host.gameObject.AddComponent<SiegeRunner>(), view);
            // The run result card sits over everything on the board, the rules panel included.
            view.Find("ResultCard")?.SetAsLastSibling();
        }

        // Doors and rules (Quests 3 and 4), top right under the top bar. Rows are made at runtime per puzzle.
        /// <summary>Build mode content starts this far below the top of the screen (top bar + goal banner).</summary>
        const float BuildTop = 206;

        // A right-hand column from under the goal banner down to just above the Run button; the board shrinks
        // to its left while the panel shows (RulesPanel moves the board area).
        static void BuildRulesPanel(RulesPanel rules, BuildManager manager, RectTransform view, RectTransform boardArea)
        {
            var panel = NewRect("RulesPanel", view);
            Place(panel, new Vector2(1, 1), new Vector2(-24, -BuildTop), new Vector2(RulesPanel.Width, 0));
            AddImage(panel.gameObject, UiStyle.Dim(0.9f), true);
            AddOutline(panel.gameObject, UiStyle.Border);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var doors = NewList("Doors", panel);
            var header = NewRect("RulesHeader", panel);
            AddImage(header.gameObject, Palette.NeonCyan, false);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 6;
            var list = NewList("Rules", panel);

            var so = new SerializedObject(rules);
            so.FindProperty("manager").objectReferenceValue = manager;
            so.FindProperty("panel").objectReferenceValue = panel.gameObject;
            so.FindProperty("doorList").objectReferenceValue = doors;
            so.FindProperty("ruleList").objectReferenceValue = list;
            so.FindProperty("ruleHeader").objectReferenceValue = header.gameObject;
            so.FindProperty("boardArea").objectReferenceValue = boardArea;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static RectTransform NewList(string name, Transform parent)
        {
            var list = NewRect(name, parent);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return list;
        }

        // Quest 4 siege: call indicator bottom left, Call in the middle, Priority and Run again bottom right.
        static void BuildSiegeParts(SiegeRunner siege, RectTransform view)
        {
            var indicator = NewRect("CallIndicator", view);
            // Bottom left, where the tray sits in other puzzles (the siege board is built for you, no tray).
            Place(indicator, new Vector2(0, 0), new Vector2(48, 48), new Vector2(0, 0));
            AddImage(indicator.gameObject, UiStyle.Dim(0.9f), false);
            AddOutline(indicator.gameObject, UiStyle.Border);
            var row = indicator.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(20, 24, 20, 20);
            ConfigureRow(row, 20, TextAnchor.MiddleLeft);
            FitBoth(indicator.gameObject);

            // Video tile with the caller's pod number; a pause mark when the picture freezes.
            var tile = NewRect("Video", row.transform);
            var tileImage = AddImage(tile.gameObject, Palette.SignalBlue, false);
            AddOutline(tile.gameObject, 4);
            var tileElement = tile.gameObject.AddComponent<LayoutElement>();
            tileElement.minWidth = tileElement.preferredWidth = 200;
            tileElement.minHeight = tileElement.preferredHeight = 120;
            var caller = NewText("Caller", tile, UiStyle.Heading, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            Stretch(caller.rectTransform);
            var frozen = NewRect("Frozen", tile);
            Stretch(frozen);
            AddImage(frozen.gameObject, UiStyle.Dim(0.55f), false);
            foreach (var x in new[] { -16f, 16f })
            {
                var bar = NewRect("Pause", frozen);
                bar.sizeDelta = new Vector2(16, 52);
                bar.anchoredPosition = new Vector2(x, 0);
                AddImage(bar.gameObject, Palette.ByteCream, false);
            }

            var column = NewRect("Signal", row.transform);
            var columnLayout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            columnLayout.spacing = 12;
            columnLayout.childControlWidth = true;
            columnLayout.childControlHeight = true;
            columnLayout.childForceExpandWidth = false;
            columnLayout.childForceExpandHeight = false;
            var barRow = NewRect("Bars", column);
            ConfigureRow(barRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 8, TextAnchor.LowerLeft);
            var so = new SerializedObject(siege);
            var bars = so.FindProperty("bars");
            bars.arraySize = 4;
            for (var i = 0; i < 4; i++)
            {
                var bar = NewRect($"Bar{i}", barRow);
                var image = AddImage(bar.gameObject, UiStyle.Done, false);
                var e = bar.gameObject.AddComponent<LayoutElement>();
                e.minWidth = e.preferredWidth = 18;
                e.minHeight = e.preferredHeight = 18 + i * 14;
                bars.GetArrayElementAtIndex(i).objectReferenceValue = image;
            }
            var stats = NewText("Stats", column, UiStyle.Small, Palette.NeonCyan, TextAlignmentOptions.MidlineLeft, UiFont.Mono);

            var call = NewButton("CallButton", view, ButtonStyle.Main, out var callLabel);
            Place((RectTransform)call.transform, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(720, 140));
            callLabel.fontSize = 52;

            var priority = NewButton("PriorityButton", view, ButtonStyle.Secondary, out var priorityLabel);
            Place((RectTransform)priority.transform, new Vector2(1, 0), new Vector2(-400, 48), new Vector2(460, 128));

            var runAgain = NewButton("RunAgainButton", view, ButtonStyle.Main, out var runAgainLabel);
            Place((RectTransform)runAgain.transform, new Vector2(1, 0), new Vector2(-48, 48), new Vector2(320, 128));
            runAgainLabel.fontSize = 60;

            so.FindProperty("indicator").objectReferenceValue = indicator.gameObject;
            so.FindProperty("videoTile").objectReferenceValue = tileImage;
            so.FindProperty("frozenMark").objectReferenceValue = frozen.gameObject;
            so.FindProperty("statsText").objectReferenceValue = stats;
            so.FindProperty("callerText").objectReferenceValue = caller;
            so.FindProperty("callButton").objectReferenceValue = call;
            so.FindProperty("callLabel").objectReferenceValue = callLabel;
            so.FindProperty("priorityButton").objectReferenceValue = priority;
            so.FindProperty("priorityFill").objectReferenceValue = priority.targetGraphic;
            so.FindProperty("priorityLabel").objectReferenceValue = priorityLabel;
            so.FindProperty("runAgainButton").objectReferenceValue = runAgain;
            so.FindProperty("runAgainLabel").objectReferenceValue = runAgainLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildRunnerParts(BuildRunner runner, RectTransform view, RectTransform packets)
        {
            // Run, bottom right.
            var runButton = NewButton("RunButton", view, ButtonStyle.Main, out var runLabel);
            Place((RectTransform)runButton.transform, new Vector2(1, 0), new Vector2(-48, 48), new Vector2(320, 128));
            runLabel.fontSize = 60;

            // Result card, centred.
            var card = NewRect("ResultCard", view);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1000, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, 10);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 48, 48);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var line = NewText("Line", card, 48, Palette.Ink, TextAlignmentOptions.Center);
            line.textWrappingMode = TextWrappingModes.Normal;
            var summary = NewText("Summary", card, 36, Palette.SignalBlue, TextAlignmentOptions.Center);
            summary.textWrappingMode = TextWrappingModes.Normal;
            var row = NewRect("ButtonRow", card);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var resultButton = NewButton("Button", row, ButtonStyle.Main, out var resultLabel);
            var element = resultButton.GetComponent<LayoutElement>();
            element.minWidth = 480;
            element.preferredWidth = 480;
            resultLabel.fontSize = 44;

            var so = new SerializedObject(runner);
            so.FindProperty("runButton").objectReferenceValue = runButton;
            so.FindProperty("runLabel").objectReferenceValue = runLabel;
            so.FindProperty("packetLayer").objectReferenceValue = packets;
            so.FindProperty("resultCard").objectReferenceValue = card.gameObject;
            so.FindProperty("resultLine").objectReferenceValue = line;
            so.FindProperty("resultSummary").objectReferenceValue = summary;
            so.FindProperty("resultButton").objectReferenceValue = resultButton;
            so.FindProperty("resultButtonLabel").objectReferenceValue = resultLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildBuildIntro(Transform parent)
        {
            var host = NewRect("BuildIntro", parent);
            Stretch(host);
            var intro = host.gameObject.AddComponent<BuildIntroScreen>();

            var button = NewButton("ButtonRoot", host, ButtonStyle.Main, out var label);
            var root = (RectTransform)button.transform;
            Place(root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 140));
            label.fontSize = 60;

            var so = new SerializedObject(intro);
            so.FindProperty("buttonRoot").objectReferenceValue = root.gameObject;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("buttonLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Pause ----------

        static void BuildPauseMenu(RectTransform parent)
        {
            var host = NewRect("PauseMenu", parent);
            Stretch(host);
            var pause = host.gameObject.AddComponent<PauseMenu>();

            // Pause button at the left of the top bar: two cream bars, no text.
            var button = NewRect("PauseButton", host);
            button.anchorMin = button.anchorMax = new Vector2(0, 1);
            button.pivot = new Vector2(0, 0.5f);
            button.anchoredPosition = new Vector2(24, -55);
            button.sizeDelta = new Vector2(88, 88);
            var buttonImage = AddImage(button.gameObject, Palette.SignalBlue, true);
            AddOutline(button.gameObject, 4);
            button.gameObject.AddComponent<Button>().targetGraphic = buttonImage;
            foreach (var x in new[] { -14f, 14f })
            {
                var bar = NewRect("Bar", button);
                bar.sizeDelta = new Vector2(14, 44);
                bar.anchoredPosition = new Vector2(x, 0);
                AddImage(bar.gameObject, Palette.ByteCream, false);
            }

            var menu = NewRect("Menu", host);
            Stretch(menu);
            var dim = (Color)Palette.Ink;
            dim.a = 0.8f;
            AddImage(menu.gameObject, dim, true);

            var card = NewRect("Card", menu);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(720, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, 10);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(64, 64, 56, 64);
            layout.spacing = 36;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = NewText("Title", card, 72, Palette.Ink, TextAlignmentOptions.Center, UiFont.Title);
            var resume = NewButton("Resume", card, ButtonStyle.Main, out var resumeLabel);
            resume.GetComponent<LayoutElement>().preferredHeight = 120;
            resumeLabel.fontSize = 48;
            // Restart is the risky choice, so it's the quieter one (ink, below Resume).
            var restart = NewButton("RestartDay", card, ButtonStyle.Secondary, out var restartLabel);
            restartLabel.fontSize = 40;

            var so = new SerializedObject(pause);
            so.FindProperty("pauseButton").objectReferenceValue = button.gameObject;
            so.FindProperty("menu").objectReferenceValue = menu.gameObject;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("resumeButton").objectReferenceValue = resume;
            so.FindProperty("resumeLabel").objectReferenceValue = resumeLabel;
            so.FindProperty("restartButton").objectReferenceValue = restart;
            so.FindProperty("restartLabel").objectReferenceValue = restartLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Reveal card and peace ----------

        static void BuildRevealCard(Transform parent)
        {
            var host = NewRect("RevealCard", parent);
            Stretch(host);
            var reveal = host.gameObject.AddComponent<RevealCard>();

            var overlay = NewRect("Overlay", host);
            Stretch(overlay);
            var dim = (Color)Palette.Ink;
            dim.a = 0.75f;
            AddImage(overlay.gameObject, dim, true);

            var card = NewRect("Card", overlay);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1200, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, 10);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(72, 72, 56, 56);
            layout.spacing = 32;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var tag = NewRect("SpeakerTag", card);
            AddImage(tag.gameObject, Palette.SignalBlue, false);
            AddOutline(tag.gameObject, 6);
            var tagLayout = tag.gameObject.AddComponent<HorizontalLayoutGroup>();
            tagLayout.padding = new RectOffset(36, 36, 8, 8);
            tagLayout.childControlWidth = true;
            tagLayout.childControlHeight = true;
            tagLayout.childForceExpandWidth = false;
            tagLayout.childForceExpandHeight = false;
            var speaker = NewText("Name", tag, 38, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);

            var headline = NewText("Headline", card, 42, Palette.CliffLight, TextAlignmentOptions.Center);
            headline.fontStyle = FontStyles.Italic;
            headline.textWrappingMode = TextWrappingModes.Normal;
            headline.gameObject.AddComponent<LayoutElement>().preferredWidth = 1056;

            var term = NewText("Term", card, 80, Palette.SignalBlue, TextAlignmentOptions.Center, UiFont.Title);
            term.textWrappingMode = TextWrappingModes.Normal;
            term.gameObject.AddComponent<LayoutElement>().preferredWidth = 1056;

            var body = NewText("Body", card, 44, Palette.Ink, TextAlignmentOptions.Center);
            body.textWrappingMode = TextWrappingModes.Normal;
            body.gameObject.AddComponent<LayoutElement>().preferredWidth = 1056;

            // Chips: the Cisco tool, the general concept, the career role.
            var chips = NewRect("Chips", card);
            ConfigureRow(chips.gameObject.AddComponent<HorizontalLayoutGroup>(), 20, TextAnchor.MiddleCenter);
            var tool = NewChip("Tool", chips, Palette.SignalBlue, Palette.ByteCream, UiFont.Title, out var toolText);
            var concept = NewChip("Concept", chips, Palette.NeonCyan, Palette.Ink, UiFont.Body, out var conceptText);
            var role = NewChip("Role", chips, Palette.PacketYellow, Palette.Ink, UiFont.Body, out var roleText);

            var row = NewRect("ButtonRow", card);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 12, 0);
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            var button = NewButton("Button", row, ButtonStyle.Main, out var label);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = 520;
            element.preferredWidth = 520;
            label.fontSize = 46;

            var so = new SerializedObject(reveal);
            so.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
            so.FindProperty("speakerTag").objectReferenceValue = tag.gameObject;
            so.FindProperty("speakerText").objectReferenceValue = speaker;
            so.FindProperty("headlineText").objectReferenceValue = headline;
            so.FindProperty("paxImage").objectReferenceValue = NewPortrait(card, new Vector2(0, 1), new Vector2(-40, 40), 168);
            so.FindProperty("chipRow").objectReferenceValue = chips.gameObject;
            so.FindProperty("toolChip").objectReferenceValue = tool;
            so.FindProperty("toolText").objectReferenceValue = toolText;
            so.FindProperty("conceptChip").objectReferenceValue = concept;
            so.FindProperty("conceptText").objectReferenceValue = conceptText;
            so.FindProperty("roleChip").objectReferenceValue = role;
            so.FindProperty("roleText").objectReferenceValue = roleText;
            so.FindProperty("termText").objectReferenceValue = term;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("nextButton").objectReferenceValue = button;
            so.FindProperty("nextLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildPeaceScreen(Transform parent)
        {
            var host = NewRect("PeaceScreen", parent);
            Stretch(host);
            var peace = host.gameObject.AddComponent<PeaceScreen>();

            var screen = NewRect("Screen", host);
            Stretch(screen);
            AddImage(screen.gameObject, Palette.SignalBlue, true);

            // Left: the celebration and Play again. Right: the Learner Card.
            var content = NewRect("Content", screen);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            var row = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            ConfigureRow(row, 80, TextAnchor.MiddleCenter);
            FitBoth(content.gameObject);

            var left = NewRect("Left", content);
            var layout = left.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 48;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            left.gameObject.AddComponent<LayoutElement>().preferredWidth = 720;

            var title = NewText("Title", left, 110, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            title.textWrappingMode = TextWrappingModes.Normal;
            title.gameObject.AddComponent<LayoutElement>().preferredWidth = 720;
            var line = NewText("Line", left, 48, Palette.NeonCyan, TextAlignmentOptions.Center);
            line.textWrappingMode = TextWrappingModes.Normal;
            line.gameObject.AddComponent<LayoutElement>().preferredWidth = 720;

            var buttonRow = NewRect("ButtonRow", left);
            ConfigureRow(buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 0, TextAnchor.MiddleCenter);
            var button = NewButton("Button", buttonRow, ButtonStyle.Main, out var label);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = 520;
            element.preferredWidth = 520;
            element.preferredHeight = 128;
            label.fontSize = 52;

            var card = BuildLearnerCard(content);

            var so = new SerializedObject(peace);
            so.FindProperty("screen").objectReferenceValue = screen.gameObject;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("lineText").objectReferenceValue = line;
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("buttonLabel").objectReferenceValue = label;
            so.FindProperty("learnerCard").objectReferenceValue = card;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Checkpoint UI (B-14 to B-17) ----------

        /// <summary>Returns the popup/card layer, which the caller places above the comms dock.</summary>
        static RectTransform BuildCheckpointMap(RectTransform parent, MaterialsPanel materials)
        {
            var host = NewRect("CheckpointMap", parent);
            Stretch(host);
            var map = host.gameObject.AddComponent<CheckpointMap>();
            var layer = NewRect("Layer", host);
            Stretch(layer);
            var cardLayer = NewRect("CheckpointCards", parent);
            Stretch(cardLayer);
            var so = new SerializedObject(map);
            so.FindProperty("layer").objectReferenceValue = layer;
            so.FindProperty("cardLayer").objectReferenceValue = cardLayer;
            so.FindProperty("materials").objectReferenceValue = materials;
            so.ApplyModifiedPropertiesWithoutUndo();
            return cardLayer;
        }

        static MaterialsPanel BuildMaterialsPanel(RectTransform parent)
        {
            var host = NewRect("MaterialsPanel", parent);
            Stretch(host);
            var root = NewRect("Root", host);
            Stretch(root);
            var panel = host.gameObject.AddComponent<MaterialsPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("root").objectReferenceValue = root;
            so.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        static void BuildToolkit(RectTransform parent)
        {
            var host = NewRect("Toolkit", parent);
            Stretch(host);
            var toolkit = host.gameObject.AddComponent<ToolkitPanel>();

            // Top-bar button in the top-right corner; the top bar leaves a gap for it on the map.
            var open = NewButton("ToolkitButton", host, ButtonStyle.Secondary, out var openLabel);
            Place((RectTransform)open.transform, new Vector2(1, 1), new Vector2(-28 - SettingsRoom, -16), new Vector2(ToolkitButtonWidth, 78));
            ((RectTransform)open.transform).pivot = new Vector2(1, 1);
            openLabel.fontSize = UiStyle.Small;

            // Panel: right half, under the top bar, scrolls.
            var panel = NewRect("Panel", host);
            panel.anchorMin = new Vector2(0.5f, 0);
            panel.anchorMax = new Vector2(1, 1);
            panel.pivot = new Vector2(1, 0.5f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = new Vector2(0, -110);
            AddImage(panel.gameObject, Palette.ByteCream, true);
            var edge = NewRect("Edge", panel);
            edge.anchorMin = Vector2.zero;
            edge.anchorMax = new Vector2(0, 1);
            edge.pivot = new Vector2(1, 0.5f);
            edge.sizeDelta = new Vector2(8, 0);
            AddImage(edge.gameObject, Palette.Ink, false);

            var scroll = NewRect("Scroll", panel);
            Stretch(scroll);
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scroll.gameObject.AddComponent<RectMask2D>();
            AddImage(scroll.gameObject, new Color(0, 0, 0, 0), true);
            var content = NewRect("Content", scroll);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 32, 48);
            layout.spacing = 20;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = content;
            scrollRect.viewport = scroll;

            var header = NewRect("Header", content);
            ConfigureRow(header.gameObject.AddComponent<HorizontalLayoutGroup>(), 20, TextAnchor.MiddleLeft);
            var portrait = NewRect("Pax", header);
            var portraitImage = AddImage(portrait.gameObject, Color.white, false);
            portraitImage.preserveAspect = true;
            var pe = portrait.gameObject.AddComponent<LayoutElement>();
            pe.minWidth = pe.preferredWidth = pe.minHeight = pe.preferredHeight = 144;
            var title = NewText("Title", header, UiStyle.Heading, Palette.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var close = NewButton("Close", header, ButtonStyle.Secondary, out var closeLabel);
            var ce = close.GetComponent<LayoutElement>();
            ce.minWidth = ce.preferredWidth = 72;
            ce.minHeight = ce.preferredHeight = 72;
            closeLabel.text = "×";
            closeLabel.font = UiStyle.Font(UiFont.Mono);

            // The Visitor Pass and its badges.
            var passRow = NewRect("Pass", content);
            ConfigureRow(passRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 24, TextAnchor.MiddleLeft);
            var pass = NewRect("VisitorPass", passRow);
            var passImage = AddImage(pass.gameObject, Color.white, false);
            passImage.preserveAspect = true;
            var passElement = pass.gameObject.AddComponent<LayoutElement>();
            passElement.minWidth = passElement.preferredWidth = 150;
            passElement.minHeight = passElement.preferredHeight = 200;
            var badges = NewRect("Badges", passRow);
            var grid = badges.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(64, 64);
            grid.spacing = new Vector2(14, 14);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            badges.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var list = NewRect("Stations", content);
            var listLayout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 12;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;

            var card = BuildLearnerCard(content);
            var group = card.gameObject.AddComponent<CanvasGroup>();

            // Toast, top centre (slides down under the top bar).
            var toast = NewRect("Toast", host);
            toast.anchorMin = toast.anchorMax = new Vector2(0.5f, 1);
            toast.pivot = new Vector2(0.5f, 1);
            AddImage(toast.gameObject, Palette.ByteCream, false);
            AddOutline(toast.gameObject, UiStyle.Border);
            var toastLayout = toast.gameObject.AddComponent<HorizontalLayoutGroup>();
            toastLayout.padding = new RectOffset(20, 32, 14, 14);
            ConfigureRow(toastLayout, 20, TextAnchor.MiddleLeft);
            FitBoth(toast.gameObject);
            var toastPax = NewRect("Pax", toast);
            var toastPaxImage = AddImage(toastPax.gameObject, Color.white, false);
            toastPaxImage.preserveAspect = true;
            var tpe = toastPax.gameObject.AddComponent<LayoutElement>();
            tpe.minWidth = tpe.preferredWidth = tpe.minHeight = tpe.preferredHeight = 96;
            var toastBadge = NewRect("Badge", toast);
            var toastBadgeImage = AddImage(toastBadge.gameObject, Color.white, false);
            toastBadgeImage.preserveAspect = true;
            var tbe = toastBadge.gameObject.AddComponent<LayoutElement>();
            tbe.minWidth = tbe.preferredWidth = tbe.minHeight = tbe.preferredHeight = 72;
            var toastText = NewText("Text", toast, 40, Palette.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Title);

            var so = new SerializedObject(toolkit);
            so.FindProperty("openButton").objectReferenceValue = open;
            so.FindProperty("openLabel").objectReferenceValue = openLabel;
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("pass").objectReferenceValue = passImage;
            so.FindProperty("paxPortrait").objectReferenceValue = portraitImage;
            so.FindProperty("badgeRow").objectReferenceValue = badges;
            so.FindProperty("stationList").objectReferenceValue = list;
            so.FindProperty("learnerCard").objectReferenceValue = card;
            so.FindProperty("learnerGroup").objectReferenceValue = group;
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("toast").objectReferenceValue = toast;
            so.FindProperty("toastBadge").objectReferenceValue = toastBadgeImage;
            so.FindProperty("toastPortrait").objectReferenceValue = toastPaxImage;
            so.FindProperty("toastText").objectReferenceValue = toastText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildIris(Transform parent)
        {
            var host = NewRect("Iris", parent);
            Stretch(host);
            var iris = host.gameObject.AddComponent<IrisTransition>();
            var root = NewRect("Root", host);
            Stretch(root);
            var so = new SerializedObject(iris);
            so.FindProperty("root").objectReferenceValue = root;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A Pax portrait in a card corner (ignores the card's layout).</summary>
        static Image NewPortrait(Transform card, Vector2 anchor, Vector2 position, float size)
        {
            var rect = NewRect("Pax", card);
            rect.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place(rect, anchor, position, new Vector2(size, size));
            var image = AddImage(rect.gameObject, Color.white, false);
            image.preserveAspect = true;
            return image;
        }

        // ---------- Learner Card (peace screen, later the toolkit panel) ----------

        static LearnerCard BuildLearnerCard(Transform parent)
        {
            var root = NewRect("LearnerCard", parent);
            AddImage(root.gameObject, Palette.ByteCream, false);
            AddOutline(root.gameObject, UiStyle.CardBorder);
            var card = root.gameObject.AddComponent<LearnerCard>();
            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 40, 40);
            layout.spacing = 14;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            root.gameObject.AddComponent<LayoutElement>().preferredWidth = 900;

            var title = NewText("Title", root, UiStyle.Heading, Palette.SignalBlue, TextAlignmentOptions.Left, UiFont.Title);
            title.textWrappingMode = TextWrappingModes.Normal;

            var so = new SerializedObject(card);
            var rows = so.FindProperty("outcomeRows");
            var ticks = so.FindProperty("outcomeTicks");
            var texts = so.FindProperty("outcomeTexts");
            rows.arraySize = ticks.arraySize = texts.arraySize = LearnerCard.MaxOutcomes;
            for (var i = 0; i < LearnerCard.MaxOutcomes; i++)
            {
                var row = NewRect($"Outcome{i}", root);
                ConfigureRow(row.gameObject.AddComponent<HorizontalLayoutGroup>(), 18, TextAnchor.MiddleLeft);
                var tick = NewRect("Tick", row);
                var tickImage = AddImage(tick.gameObject, UiStyle.Done, false);
                AddOutline(tick.gameObject, 3);
                var tickElement = tick.gameObject.AddComponent<LayoutElement>();
                tickElement.minWidth = tickElement.preferredWidth = 26;
                tickElement.minHeight = tickElement.preferredHeight = 26;
                var text = NewText("Text", row, UiStyle.Small, Palette.Ink, TextAlignmentOptions.MidlineLeft);
                text.textWrappingMode = TextWrappingModes.Normal;
                text.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
                rows.GetArrayElementAtIndex(i).objectReferenceValue = row.gameObject;
                ticks.GetArrayElementAtIndex(i).objectReferenceValue = tickImage;
                texts.GetArrayElementAtIndex(i).objectReferenceValue = text;
            }

            // Field Notes and the check pips on one line.
            var stats = NewRect("Stats", root);
            ConfigureRow(stats.gameObject.AddComponent<HorizontalLayoutGroup>(), 40, TextAnchor.MiddleLeft);
            var notes = NewText("FieldNotes", stats, UiStyle.Small, Palette.Ink, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            var pips = NewRect("CheckPips", stats);
            ConfigureRow(pips.gameObject.AddComponent<HorizontalLayoutGroup>(), 10, TextAnchor.MiddleLeft);
            var pipList = so.FindProperty("checkPips");
            pipList.arraySize = LearnerCard.MaxCheckPips;
            for (var i = 0; i < LearnerCard.MaxCheckPips; i++)
            {
                var pip = NewRect($"Pip{i}", pips);
                var pipImage = AddImage(pip.gameObject, UiStyle.Done, false);
                AddOutline(pip.gameObject, 3);
                var e = pip.gameObject.AddComponent<LayoutElement>();
                e.minWidth = e.preferredWidth = 30;
                e.minHeight = e.preferredHeight = 30;
                pipList.GetArrayElementAtIndex(i).objectReferenceValue = pipImage;
            }

            // QR code beside the call to action and the link.
            var bottom = NewRect("Bottom", root);
            ConfigureRow(bottom.gameObject.AddComponent<HorizontalLayoutGroup>(), 28, TextAnchor.MiddleLeft);
            var qr = NewRect("QR", bottom);
            var qrImage = AddImage(qr.gameObject, Color.white, false);
            qrImage.preserveAspect = true;
            var qrElement = qr.gameObject.AddComponent<LayoutElement>();
            qrElement.minWidth = qrElement.preferredWidth = 180;
            qrElement.minHeight = qrElement.preferredHeight = 180;
            var cta = NewRect("CallToAction", bottom);
            var ctaLayout = cta.gameObject.AddComponent<VerticalLayoutGroup>();
            ctaLayout.spacing = 8;
            ctaLayout.childControlWidth = true;
            ctaLayout.childControlHeight = true;
            ctaLayout.childForceExpandWidth = true;
            ctaLayout.childForceExpandHeight = false;
            cta.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            var ctaText = NewText("Text", cta, UiStyle.Small, Palette.Ink, TextAlignmentOptions.MidlineLeft);
            ctaText.textWrappingMode = TextWrappingModes.Normal;
            var link = NewRect("Link", cta);
            var linkImage = AddImage(link.gameObject, new Color(0, 0, 0, 0), true);
            var linkButton = link.gameObject.AddComponent<Button>();
            linkButton.targetGraphic = linkImage;
            link.gameObject.AddComponent<LayoutElement>().minHeight = 56;
            var linkText = NewText("Text", link, UiStyle.Small, Palette.SignalBlue, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
            Stretch(linkText.rectTransform);
            linkText.fontStyle = FontStyles.Underline;

            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("fieldNotesText").objectReferenceValue = notes;
            so.FindProperty("qrImage").objectReferenceValue = qrImage;
            so.FindProperty("ctaText").objectReferenceValue = ctaText;
            so.FindProperty("linkButton").objectReferenceValue = linkButton;
            so.FindProperty("linkText").objectReferenceValue = linkText;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        // ---------- Quest-end check (B-04) ----------

        static void BuildQuestCheck(Transform parent, IncidentReport incident)
        {
            var host = NewRect("QuestCheck", parent);
            Stretch(host);
            var check = host.gameObject.AddComponent<QuestCheck>();

            var overlay = NewRect("Overlay", host);
            Stretch(overlay);
            AddImage(overlay.gameObject, UiStyle.Dim(0.85f), true);

            var card = NewCard("Card", overlay, 1300);
            var progress = NewText("Progress", card, UiStyle.Small, Palette.CliffLight, TextAlignmentOptions.Center, UiFont.Mono);
            var question = NewText("Question", card, 50, Palette.Ink, TextAlignmentOptions.Center);
            question.textWrappingMode = TextWrappingModes.Normal;

            var grid = NewRect("Options", card);
            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(560, 120);
            gridLayout.spacing = new Vector2(28, 24);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            grid.gameObject.AddComponent<LayoutElement>().preferredHeight = 120 * 2 + 24;

            var so = new SerializedObject(check);
            var buttons = so.FindProperty("optionButtons");
            var images = so.FindProperty("optionImages");
            var labels = so.FindProperty("optionLabels");
            buttons.arraySize = images.arraySize = labels.arraySize = QuestCheck.OptionCount;
            for (var i = 0; i < QuestCheck.OptionCount; i++)
            {
                var option = NewButton($"Option{i}", grid, ButtonStyle.Secondary, out var label);
                label.font = UiStyle.Font(UiFont.Body);
                label.fontSize = 38;
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = option;
                images.GetArrayElementAtIndex(i).objectReferenceValue = option.targetGraphic;
                labels.GetArrayElementAtIndex(i).objectReferenceValue = label;
            }

            var explanation = NewText("Explanation", card, 40, Palette.SignalBlue, TextAlignmentOptions.Center);
            explanation.textWrappingMode = TextWrappingModes.Normal;
            var next = NewButtonRow(card, ButtonStyle.Main, 440, out var nextLabel);

            so.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
            so.FindProperty("progressText").objectReferenceValue = progress;
            so.FindProperty("questionText").objectReferenceValue = question;
            so.FindProperty("explanationText").objectReferenceValue = explanation;
            so.FindProperty("nextButton").objectReferenceValue = next;
            so.FindProperty("nextLabel").objectReferenceValue = nextLabel;
            so.FindProperty("incident").objectReferenceValue = incident;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Incident report (B-11) ----------

        static IncidentReport BuildIncidentReport(Transform parent)
        {
            var host = NewRect("IncidentReport", parent);
            Stretch(host);
            var report = host.gameObject.AddComponent<IncidentReport>();

            var overlay = NewRect("Overlay", host);
            Stretch(overlay);
            AddImage(overlay.gameObject, UiStyle.Dim(0.9f), true);

            var card = NewCard("Card", overlay, 1600);
            var prompt = NewText("Prompt", card, 48, Palette.Ink, TextAlignmentOptions.Center, UiFont.Title);
            prompt.textWrappingMode = TextWrappingModes.Normal;

            var so = new SerializedObject(report);
            // Timeline: numbered slots that fill in order.
            var slots = NewRect("Timeline", card);
            var slotsLayout = slots.gameObject.AddComponent<VerticalLayoutGroup>();
            slotsLayout.spacing = 10;
            slotsLayout.childControlWidth = true;
            slotsLayout.childControlHeight = true;
            slotsLayout.childForceExpandWidth = true;
            slotsLayout.childForceExpandHeight = false;
            var slotImages = so.FindProperty("slotImages");
            var slotLabels = so.FindProperty("slotLabels");
            slotImages.arraySize = slotLabels.arraySize = IncidentReport.MaxCards;
            for (var i = 0; i < IncidentReport.MaxCards; i++)
            {
                var slot = NewRect($"Slot{i}", slots);
                var image = AddImage(slot.gameObject, UiStyle.Locked, false);
                AddOutline(slot.gameObject, 4);
                slot.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
                var text = NewText("Text", slot, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.MidlineLeft, UiFont.Mono);
                Stretch(text.rectTransform);
                text.rectTransform.offsetMin = new Vector2(24, 0);
                slotImages.GetArrayElementAtIndex(i).objectReferenceValue = image;
                slotLabels.GetArrayElementAtIndex(i).objectReferenceValue = text;
            }

            // Pool: the shuffled evidence cards.
            var pool = NewRect("Pool", card);
            var poolLayout = pool.gameObject.AddComponent<GridLayoutGroup>();
            poolLayout.cellSize = new Vector2(460, 104);
            poolLayout.spacing = new Vector2(24, 20);
            poolLayout.childAlignment = TextAnchor.MiddleCenter;
            poolLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            poolLayout.constraintCount = 3;
            pool.gameObject.AddComponent<LayoutElement>().preferredHeight = 104 * 2 + 20;
            var poolButtons = so.FindProperty("poolButtons");
            var poolImages = so.FindProperty("poolImages");
            var poolLabels = so.FindProperty("poolLabels");
            poolButtons.arraySize = poolImages.arraySize = poolLabels.arraySize = IncidentReport.MaxCards;
            for (var i = 0; i < IncidentReport.MaxCards; i++)
            {
                var b = NewButton($"Card{i}", pool, ButtonStyle.Secondary, out var label);
                label.font = UiStyle.Font(UiFont.Mono);
                label.fontSize = UiStyle.Small;
                poolButtons.GetArrayElementAtIndex(i).objectReferenceValue = b;
                poolImages.GetArrayElementAtIndex(i).objectReferenceValue = b.targetGraphic;
                poolLabels.GetArrayElementAtIndex(i).objectReferenceValue = label;
            }

            var done = NewButtonRow(card, ButtonStyle.Main, 440, out var doneLabel);

            so.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("doneButton").objectReferenceValue = done;
            so.FindProperty("doneLabel").objectReferenceValue = doneLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
            return report;
        }

        // ---------- Prop card (B-10) ----------

        static void BuildPropCard(Transform parent)
        {
            var host = NewRect("PropCard", parent);
            Stretch(host);
            var prop = host.gameObject.AddComponent<PropCard>();

            var overlay = NewRect("Overlay", host);
            Stretch(overlay);
            var dimImage = AddImage(overlay.gameObject, UiStyle.Dim(0.6f), true);
            var dim = overlay.gameObject.AddComponent<Button>();
            dim.targetGraphic = dimImage;
            dim.transition = Selectable.Transition.None;

            var card = NewCard("Card", overlay, 1000);
            var name = NewText("Name", card, UiStyle.Heading, Palette.SignalBlue, TextAlignmentOptions.Center, UiFont.Title);
            name.textWrappingMode = TextWrappingModes.Normal;
            var place = NewText("Place", card, UiStyle.Small, Palette.CliffLight, TextAlignmentOptions.Center);
            var chipRow = NewRect("ChipRow", card);
            ConfigureRow(chipRow.gameObject.AddComponent<HorizontalLayoutGroup>(), 0, TextAnchor.MiddleCenter);
            var chip = NewChip("Concept", chipRow, Palette.NeonCyan, Palette.Ink, UiFont.Body, out var conceptText);
            var body = NewText("Body", card, 42, Palette.Ink, TextAlignmentOptions.Center);
            body.textWrappingMode = TextWrappingModes.Normal;
            var tryButton = NewButtonRow(card, ButtonStyle.Main, 480, out var tryLabel);
            var notes = NewText("Notes", card, UiStyle.Small, Palette.CliffLight, TextAlignmentOptions.Center, UiFont.Mono);

            var so = new SerializedObject(prop);
            so.FindProperty("overlay").objectReferenceValue = overlay.gameObject;
            so.FindProperty("dimButton").objectReferenceValue = dim;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("placeText").objectReferenceValue = place;
            so.FindProperty("conceptChip").objectReferenceValue = chip;
            so.FindProperty("conceptText").objectReferenceValue = conceptText;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("tryButton").objectReferenceValue = tryButton;
            so.FindProperty("tryLabel").objectReferenceValue = tryLabel;
            so.FindProperty("notesText").objectReferenceValue = notes;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Splunk ticker (B-09, map and stations, bottom right) ----------

        // ---------- Shared card helpers ----------

        /// <summary>A centred cream card with an ink border that grows with its content.</summary>
        static RectTransform NewCard(string name, Transform parent, float width)
        {
            var card = NewRect(name, parent);
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(width, 0);
            AddImage(card.gameObject, Palette.ByteCream, true);
            AddOutline(card.gameObject, UiStyle.CardBorder);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(64, 64, 52, 56);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return card;
        }

        /// <summary>A centred row holding one fixed-width button.</summary>
        static Button NewButtonRow(Transform parent, ButtonStyle style, float width, out TextMeshProUGUI label)
        {
            var row = NewRect("ButtonRow", parent);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 8, 0);
            ConfigureRow(rowLayout, 0, TextAnchor.MiddleCenter);
            var button = NewButton("Button", row, style, out label);
            var element = button.GetComponent<LayoutElement>();
            element.minWidth = width;
            element.preferredWidth = width;
            return button;
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

            var dayLabel = NewText("DayLabel", bar, 44, Palette.ByteCream, TextAlignmentOptions.MidlineLeft, UiFont.Title);
            dayLabel.rectTransform.anchorMin = new Vector2(0, 0);
            dayLabel.rectTransform.anchorMax = new Vector2(0.38f, 1);
            dayLabel.rectTransform.offsetMin = new Vector2(224, 0); // room for the pause button and Pax on the left

            // Pax's portrait next to the pause button, framed in the same cyan as the "you" arrow on the map.
            var pax = NewRect("Pax", bar);
            pax.anchorMin = pax.anchorMax = new Vector2(0, 0.5f);
            pax.pivot = new Vector2(0, 0.5f);
            pax.anchoredPosition = new Vector2(128, 0);
            pax.sizeDelta = new Vector2(80, 80);
            AddImage(pax.gameObject, Palette.Ink, false);
            AddOutline(pax.gameObject, 4);
            pax.GetComponent<Outline>().effectColor = Palette.NeonCyan;
            var paxFace = NewRect("Face", pax);
            Stretch(paxFace);
            var paxImage = AddImage(paxFace.gameObject, Color.white, false);
            paxImage.preserveAspect = true;
            dayLabel.rectTransform.offsetMax = Vector2.zero;
            // Quest titles are long: one line, shrinking a little if needed.
            dayLabel.textWrappingMode = TextWrappingModes.NoWrap;
            dayLabel.enableAutoSizing = true;
            dayLabel.fontSizeMin = UiStyle.MinText;
            dayLabel.fontSizeMax = 44;
            dayLabel.overflowMode = TextOverflowModes.Ellipsis;

            // Day 3 goal line: its own banner under the bar, so long goals read in full.
            var banner = NewRect("GoalBanner", host);
            banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 1);
            banner.pivot = new Vector2(0.5f, 1);
            banner.anchoredPosition = new Vector2(0, -122);
            AddImage(banner.gameObject, UiStyle.Dim(0.92f), false);
            AddOutline(banner.gameObject, UiStyle.Border);
            var bannerLayout = banner.gameObject.AddComponent<HorizontalLayoutGroup>();
            bannerLayout.padding = new RectOffset(32, 32, 12, 14);
            ConfigureRow(bannerLayout, 0, TextAnchor.MiddleCenter);
            FitBoth(banner.gameObject);
            var goal = NewText("Goal", banner, UiStyle.Body, Palette.PacketYellow, TextAlignmentOptions.Center);
            goal.textWrappingMode = TextWrappingModes.NoWrap;

            var right = NewRect("Right", bar);
            right.anchorMin = new Vector2(1, 0);
            right.anchorMax = new Vector2(1, 1);
            right.pivot = new Vector2(1, 0.5f);
            right.anchoredPosition = new Vector2(-48 - SettingsRoom, 0);
            right.sizeDelta = new Vector2(0, 0);
            var rightLayout = right.gameObject.AddComponent<HorizontalLayoutGroup>();
            ConfigureRow(rightLayout, 64, TextAnchor.MiddleRight);
            right.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // No backups meter: the team chose to keep the top bar to the day, complaints and the toolkit.
            var complaints = BuildMeter(right, "Complaints", out var complaintsLabel, out var complaintPips);
            // Room for the Cisco toolkit button (built with the toolkit, pinned to the top-right corner).
            var toolkitSpace = NewRect("ToolkitSpace", right);
            toolkitSpace.gameObject.AddComponent<LayoutElement>().preferredWidth = ToolkitButtonWidth - 40;

            var so = new SerializedObject(topBar);
            so.FindProperty("bar").objectReferenceValue = bar.gameObject;
            so.FindProperty("dayLabel").objectReferenceValue = dayLabel;
            so.FindProperty("goalText").objectReferenceValue = goal;
            so.FindProperty("goalBanner").objectReferenceValue = banner.gameObject;
            so.FindProperty("complaintsGroup").objectReferenceValue = complaints.gameObject;
            so.FindProperty("complaintsLabel").objectReferenceValue = complaintsLabel;
            so.FindProperty("complaintPips").objectReferenceValue = complaintPips;
            so.FindProperty("toolkitSpace").objectReferenceValue = toolkitSpace.gameObject;
            so.FindProperty("paxPortrait").objectReferenceValue = paxImage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        const float ToolkitButtonWidth = 400;
        // A's settings button (SettingsMenu on GameRoot) is a 78 px tile in the top-right corner of every screen;
        // what used to sit in that corner moves left by this much.
        const float SettingsRoom = 96;

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

        /// <summary>A small filled tag that hugs its text.</summary>
        static GameObject NewChip(string name, Transform parent, Color fill, Color textColor, UiFont font, out TextMeshProUGUI text)
        {
            var chip = NewRect(name, parent);
            AddImage(chip.gameObject, fill, false);
            AddOutline(chip.gameObject, 4);
            var layout = chip.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 8, 10);
            ConfigureRow(layout, 0, TextAnchor.MiddleCenter);
            text = NewText("Text", chip, UiStyle.Small, textColor, TextAlignmentOptions.Center, font);
            return chip.gameObject;
        }

        static void FitBoth(GameObject go)
        {
            var fit = go.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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

            var title = NewText("Title", card, 50, Palette.Ink, TextAlignmentOptions.TopLeft, UiFont.Title);
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
                var value = NewText("Value", row, 42, Palette.Ink, TextAlignmentOptions.TopLeft, UiFont.Mono);
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
            var primary = NewButton("Primary", buttons, ButtonStyle.Secondary, out var primaryLabel);
            var secondary = NewButton("Secondary", buttons, ButtonStyle.Secondary, out var secondaryLabel);

            so.FindProperty("card").objectReferenceValue = card.gameObject;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("primaryButton").objectReferenceValue = primary;
            so.FindProperty("primaryLabel").objectReferenceValue = primaryLabel;
            so.FindProperty("secondaryButton").objectReferenceValue = secondary;
            so.FindProperty("secondaryLabel").objectReferenceValue = secondaryLabel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Chunky button (PRD-v2 6.6): fill by style, ink border, ink drop shadow that shrinks when pressed.
        static Button NewButton(string name, Transform parent, ButtonStyle style, out TextMeshProUGUI label)
        {
            var rect = NewRect(name, parent);
            var image = AddImage(rect.gameObject, UiStyle.ButtonFill(style), true);
            AddOutline(rect.gameObject, UiStyle.Border);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Palette.Ink;
            shadow.effectDistance = new Vector2(UiStyle.Shadow, -UiStyle.Shadow);
            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.minHeight = UiStyle.ButtonHeight;
            element.preferredHeight = UiStyle.ButtonHeight;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            button.colors = colors;
            label = NewText("Label", rect, UiStyle.Button, UiStyle.ButtonText(style), TextAlignmentOptions.Center, UiFont.Title);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(16, 0);
            label.rectTransform.offsetMax = new Vector2(-16, 0);
            label.textWrappingMode = TextWrappingModes.Normal;

            var press = rect.gameObject.AddComponent<PressableButton>();
            var so = new SerializedObject(press);
            so.FindProperty("shadow").objectReferenceValue = shadow;
            so.FindProperty("face").objectReferenceValue = label.rectTransform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return button;
        }

        /// <summary>Anchors and pivots a free-standing rect at one point.</summary>
        static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // ---------- Webex Town Space strip (map, bottom left) ----------

        // Splunk log, bottom left inside a station (builds its parts at runtime).
        static void BuildSplunkLog(RectTransform parent)
        {
            var host = NewRect("SplunkLog", parent);
            Stretch(host);
            var log = host.gameObject.AddComponent<SplunkTicker>();
            var so = new SerializedObject(log);
            so.FindProperty("root").objectReferenceValue = host;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildTownSpace(RectTransform parent)
        {
            // Webex chat, bottom left on the map (builds its parts at runtime).
            var host = NewRect("TownSpace", parent);
            Stretch(host);
            var town = host.gameObject.AddComponent<TownSpace>();
            var so = new SerializedObject(town);
            so.FindProperty("root").objectReferenceValue = host;
            so.ApplyModifiedPropertiesWithoutUndo();
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
            // Webex message card: stretches with the screen, 120 px margin each side, any landscape aspect.
            panel.anchorMin = new Vector2(0, 0);
            panel.anchorMax = new Vector2(1, 0);
            panel.pivot = new Vector2(0.5f, 0);
            panel.anchoredPosition = new Vector2(0, 40);
            panel.sizeDelta = new Vector2(-240, 320);
            AddImage(panel.gameObject, UiStyle.White, true);
            AddOutline(panel.gameObject, UiStyle.CardBorder);

            // Town Space tab sitting on the top edge: "▪ Byte World".
            var space = NewRect("SpaceTab", panel);
            space.anchorMin = space.anchorMax = new Vector2(0, 1);
            space.pivot = new Vector2(0, 0);
            space.anchoredPosition = new Vector2(32, 6);
            AddImage(space.gameObject, Palette.SignalBlue, false);
            AddOutline(space.gameObject, UiStyle.Border);
            var spaceLayout = space.gameObject.AddComponent<HorizontalLayoutGroup>();
            spaceLayout.padding = new RectOffset(24, 28, 8, 8);
            ConfigureRow(spaceLayout, 14, TextAnchor.MiddleLeft);
            FitBoth(space.gameObject);
            var dot = NewRect("Dot", space);
            AddImage(dot.gameObject, Palette.NeonCyan, false);
            var dotElement = dot.gameObject.AddComponent<LayoutElement>();
            dotElement.preferredWidth = dotElement.preferredHeight = 16;
            var spaceText = NewText("Name", space, UiStyle.Small, Palette.ByteCream, TextAlignmentOptions.MidlineLeft, UiFont.Title);

            // Avatar tile with initials.
            var avatar = NewRect("Avatar", panel);
            Place(avatar, new Vector2(0, 1), Vector2.zero, new Vector2(120, 120));
            avatar.anchoredPosition = new Vector2(48, -52);
            var avatarImage = AddImage(avatar.gameObject, Palette.SignalBlue, false);
            AddOutline(avatar.gameObject, UiStyle.Border);
            var initials = NewText("Initials", avatar, 56, Palette.ByteCream, TextAlignmentOptions.Center, UiFont.Title);
            Stretch(initials.rectTransform);

            // Header: name and timestamp.
            var nameTag = NewRect("Header", panel);
            nameTag.anchorMin = nameTag.anchorMax = new Vector2(0, 1);
            nameTag.pivot = new Vector2(0, 1);
            nameTag.anchoredPosition = new Vector2(200, -40);
            ConfigureRow(nameTag.gameObject.AddComponent<HorizontalLayoutGroup>(), 24, TextAnchor.LowerLeft);
            FitBoth(nameTag.gameObject);
            var nameText = NewText("Name", nameTag, 42, Palette.SignalBlue, TextAlignmentOptions.BottomLeft, UiFont.Title);
            var timeText = NewText("Time", nameTag, UiStyle.Small, Palette.CliffLight, TextAlignmentOptions.BottomLeft, UiFont.Mono);

            var body = NewText("Body", panel, 44, Palette.Ink, TextAlignmentOptions.TopLeft);
            Stretch(body.rectTransform);
            body.rectTransform.offsetMin = new Vector2(200, 40);
            body.rectTransform.offsetMax = new Vector2(-96, -96);
            body.textWrappingMode = TextWrappingModes.Normal;

            var cont = NewRect("ContinueIcon", panel);
            cont.anchorMin = cont.anchorMax = new Vector2(1, 0);
            cont.pivot = new Vector2(0.5f, 0.5f);
            cont.anchoredPosition = new Vector2(-40, 36);
            cont.sizeDelta = new Vector2(28, 28);
            cont.localRotation = Quaternion.Euler(0, 0, 45);
            AddImage(cont.gameObject, Palette.SignalBlue, false);

            var button = NewButton("RetryButton", panel, ButtonStyle.Main, out var retryLabel);
            Place((RectTransform)button.transform, new Vector2(1, 0), new Vector2(-32, 24), new Vector2(340, 88));

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
            so.FindProperty("spaceText").objectReferenceValue = spaceText;
            so.FindProperty("avatar").objectReferenceValue = avatarImage;
            so.FindProperty("avatarText").objectReferenceValue = initials;
            so.FindProperty("timeText").objectReferenceValue = timeText;
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

        static TextMeshProUGUI NewText(string name, Transform parent, float size, Color color, TextAlignmentOptions align,
            UiFont font = UiFont.Body)
        {
            var rect = NewRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            UiStyle.Apply(text, font);
            text.fontSize = Mathf.Max(size, UiStyle.MinText); // never under 34 at 1920 × 1080
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.text = "";
            return text;
        }
    }
}
