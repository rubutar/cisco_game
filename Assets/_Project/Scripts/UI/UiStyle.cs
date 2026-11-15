// Stream B. Every UI size, colour and font in one place (PRD-v2 6.6). Colours come from Palette only.
//   Pixelify Sans  titles and buttons
//   Nunito         body text
//   JetBrains Mono inspected values and log lines (distinct zero, so 0 and O never mix)
// Main buttons are Packet Yellow with ink text, secondary buttons white with an ink border, Alert Pink only
// for danger and alert states. No text under 34 at 1920 × 1080.
using TMPro;
using UnityEngine;

namespace ByteWorld
{
    public enum UiFont { Body, Title, Mono }

    public enum ButtonStyle { Main, Secondary, Danger }

    public static class UiStyle
    {
        // ---------- Fonts (TMP font assets in Resources/UI/Fonts, made by Byte World > UI > Create Font Assets) ----------

        public const string FontFolder = "UI/Fonts/";
        public const string TitleFontName = "PixelifySans SDF";
        public const string BodyFontName = "Nunito SDF";
        public const string MonoFontName = "JetBrainsMono SDF";

        static TMP_FontAsset title, body, mono;

        public static TMP_FontAsset Font(UiFont font) => font switch
        {
            UiFont.Title => Load(ref title, TitleFontName),
            UiFont.Mono => Load(ref mono, MonoFontName),
            _ => Load(ref body, BodyFontName),
        };

        static TMP_FontAsset Load(ref TMP_FontAsset cache, string name)
        {
            if (cache == null) cache = Resources.Load<TMP_FontAsset>(FontFolder + name);
            return cache != null ? cache : TMP_Settings.defaultFontAsset;
        }

        /// <summary>Applies a font role to a text made at runtime.</summary>
        public static void Apply(TMP_Text text, UiFont font)
        {
            var asset = Font(font);
            if (asset != null) text.font = asset;
        }

        // ---------- Text sizes (1920 × 1080 reference) ----------

        public const float MinText = 34;
        public const float Small = 34;
        public const float Body = 40;
        public const float BodyLarge = 46;
        public const float Button = 44;
        public const float Heading = 56;
        public const float Title = 72;
        public const float Display = 140;

        // ---------- Colours ----------

        public static Color White => Color.white;
        public static Color Ink => Palette.Ink;
        public static Color Panel => Palette.ByteCream;
        public static Color Accent => Palette.SignalBlue;
        public static Color Highlight => Palette.NeonCyan;
        public static Color Danger => Palette.AlertPink;
        public static Color Done => Palette.LeavesDark;
        public static Color Locked => Palette.GridDim[3];
        public static Color LockedDark => Palette.GridDim[2];

        public static Color Dim(float alpha = 0.75f)
        {
            Color c = Palette.Ink;
            c.a = alpha;
            return c;
        }

        /// <summary>Webex avatar tile colour per speaker key.</summary>
        public static Color AvatarFill(string speaker) => speaker switch
        {
            "scout" => Palette.SignalBlue,
            "gigi" => Palette.PodOrange,
            "twins" => Palette.NeonCyan,
            "patch" => Palette.GrassDark,
            _ => Palette.CliffLight, // the town / Splunk alert bot
        };

        public static Color AvatarText(string speaker) => speaker == "twins" || speaker == "patch" || speaker == "gigi"
            ? (Color)Palette.Ink : (Color)Palette.ByteCream;

        public static Color ButtonFill(ButtonStyle style) => style switch
        {
            ButtonStyle.Secondary => White,
            ButtonStyle.Danger => Palette.AlertPink,
            _ => Palette.PacketYellow,
        };

        public static Color ButtonText(ButtonStyle style) => style == ButtonStyle.Danger ? (Color)Palette.ByteCream : (Color)Palette.Ink;

        // ---------- Shapes ----------

        public const float Border = 6;          // ink border on buttons and cards
        public const float CardBorder = 8;
        public const float Shadow = 8;          // ink drop shadow under buttons
        public const float PressedShadow = 2;   // shadow while pressed (the button "sinks")
        public const float ButtonHeight = 104;
        public const float PanelSlideSeconds = 0.24f;
        public const float IrisSeconds = 0.45f;
    }
}
