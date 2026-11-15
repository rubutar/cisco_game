// Stream B. The doors and rules panel on the build board (PRD-v2 4.7, B-12), right side:
//   Doors: one row per door, the port and its setting; tap the setting to cycle Everyone → Friends only → Closed.
//   Rules: build.json rules[] in the order they apply; tap a rule to move it up one place (top wraps to the bottom).
// Display only: BuildManager changes DoorRules. Shown only for puzzles with doors or rules.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class RulesPanel : MonoBehaviour
    {
        [SerializeField] BuildManager manager;
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform doorList;
        [SerializeField] RectTransform ruleList;
        [SerializeField] GameObject ruleHeader;
        [SerializeField] RectTransform boardArea; // shrinks to the left of the panel while it shows

        public const float Width = 620f;
        const float RowHeight = 62f;

        readonly List<(int port, Image fill, TMP_Text label)> doorRows = new();
        readonly List<(Image fill, TMP_Text label)> ruleRows = new();

        void Awake() => panel.SetActive(false);

        void OnEnable()
        {
            manager.Loaded += Rebuild;
            manager.Changed += Redraw;
            manager.Ended += Hide;
        }

        void OnDisable()
        {
            manager.Loaded -= Rebuild;
            manager.Changed -= Redraw;
            manager.Ended -= Hide;
        }

        void Hide() => Show(false);

        void Show(bool on)
        {
            panel.SetActive(on);
            if (boardArea != null) boardArea.offsetMax = new Vector2(on ? -(Width + 48) : 0, boardArea.offsetMax.y);
        }

        void Rebuild()
        {
            foreach (Transform c in doorList) Destroy(c.gameObject);
            foreach (Transform c in ruleList) Destroy(c.gameObject);
            doorRows.Clear();
            ruleRows.Clear();

            var rules = manager.Rules;
            var puzzle = manager.Puzzle;
            if (rules == null || (!rules.HasDoors && !rules.HasRules))
            {
                Show(false);
                return;
            }

            if (rules.HasDoors)
                foreach (var door in puzzle.doors)
                {
                    var port = door.port;
                    var row = Row(doorList, out var portText, out var fill, out var label, () => manager.CycleDoor(port));
                    portText.text = port.ToString();
                    doorRows.Add((port, fill, label));
                }
            doorList.gameObject.SetActive(rules.HasDoors);

            if (rules.HasRules)
                for (var i = 0; i < puzzle.rules.Length; i++)
                {
                    var position = i;
                    Row(ruleList, out var number, out var fill, out var label, () => manager.MoveRuleUp(position));
                    number.text = (i + 1).ToString();
                    ruleRows.Add((fill, label));
                }
            ruleList.gameObject.SetActive(rules.HasRules);
            ruleHeader.SetActive(rules.HasRules);

            Show(true);
            Redraw();
        }

        void Redraw()
        {
            var rules = manager.Rules;
            if (rules == null || !panel.activeSelf) return;

            foreach (var (port, fill, label) in doorRows)
            {
                var options = rules.Options;
                label.text = options != null && options.Length > 0 ? options[rules.OptionIndex(port) % options.Length] : "";
                fill.color = rules.SettingOf(port) switch
                {
                    DoorSetting.Everyone => UiStyle.Done,
                    DoorSetting.Friends => Palette.PacketYellow,
                    _ => UiStyle.Danger
                };
                label.color = rules.SettingOf(port) == DoorSetting.Friends ? UiStyle.Ink : (Color)Palette.ByteCream;
            }

            for (var i = 0; i < ruleRows.Count && i < rules.Order.Count; i++)
            {
                var rule = rules.RuleAt(i);
                var allow = string.Equals(rule.action, "allow", System.StringComparison.OrdinalIgnoreCase);
                ruleRows[i].label.text = $"{rule.match}  →  {rule.action}";
                ruleRows[i].fill.color = allow ? UiStyle.White : (Color)Palette.AlertBelly;
                ruleRows[i].label.color = UiStyle.Ink;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
        }

        // One tappable row: a small ink tag on the left (port or position), then a filled button.
        RectTransform Row(RectTransform parent, out TMP_Text tag, out Image fill, out TMP_Text label, UnityEngine.Events.UnityAction onTap)
        {
            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var rect = (RectTransform)row.transform;
            rect.SetParent(parent, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            row.GetComponent<LayoutElement>().preferredHeight = RowHeight;

            var tagGo = new GameObject("Tag", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            ((RectTransform)tagGo.transform).SetParent(rect, false);
            tagGo.GetComponent<Image>().color = Palette.Ink;
            tagGo.GetComponent<Image>().raycastTarget = false;
            var tagElement = tagGo.GetComponent<LayoutElement>();
            tagElement.minWidth = tagElement.preferredWidth = 104;
            tag = NewText(tagGo.transform, Palette.ByteCream);

            var buttonGo = new GameObject("Setting", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button), typeof(LayoutElement));
            ((RectTransform)buttonGo.transform).SetParent(rect, false);
            fill = buttonGo.GetComponent<Image>();
            var outline = buttonGo.GetComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(4, -4);
            var button = buttonGo.GetComponent<Button>();
            button.targetGraphic = fill;
            button.onClick.AddListener(onTap);
            buttonGo.GetComponent<LayoutElement>().flexibleWidth = 1;
            label = NewText(buttonGo.transform, Palette.Ink);
            return rect;
        }

        static TMP_Text NewText(Transform parent, Color color)
        {
            var text = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(parent, false);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(12, 0);
            text.rectTransform.offsetMax = new Vector2(-12, 0);
            UiStyle.Apply(text, UiFont.Mono);
            text.fontSize = UiStyle.Small;
            text.enableAutoSizing = true;
            text.fontSizeMin = UiStyle.MinText - 6; // long rules shrink a little rather than wrap
            text.fontSizeMax = UiStyle.Small;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap; // shrink long rules instead of wrapping
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
