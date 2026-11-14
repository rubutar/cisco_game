// Stream B. Makes the three TMP font assets (PRD-v2 6.6) from the TTFs in Art/Fonts and saves them in
// Resources/UI/Fonts, where UiStyle loads them. Dynamic atlases, pre-filled with the characters the game
// uses so the WebGL build never has to rasterise mid-game. Re-running replaces the assets and rebuilds UIRoot.
// Menu: Byte World > UI > Create Font Assets
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ByteWorld.EditorTools
{
    public static class UiFontsSetup
    {
        const string SourceFolder = "Assets/_Project/Art/Fonts/";
        const string TargetFolder = "Assets/_Project/Resources/" + UiStyle.FontFolder;

        // ASCII plus the punctuation and letters the JSON content uses (·, →, ×, quotes, dashes, accents).
        const string Prefill =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
            "·→←↑↓×✓✔…–—‘’“”•°€éèàùìòçÉ";

        [MenuItem("Byte World/UI/Create Font Assets")]
        public static void CreateAll()
        {
            Directory.CreateDirectory(TargetFolder);
            var title = Create("PixelifySans-SemiBold.ttf", UiStyle.TitleFontName);
            var body = Create("Nunito-SemiBold.ttf", UiStyle.BodyFontName);
            var mono = Create("JetBrainsMono-Medium.ttf", UiStyle.MonoFontName);
            // Pixelify and Nunito have no arrows (→ in "without → with"): borrow them from the mono font.
            foreach (var asset in new[] { title, body })
            {
                if (asset == null || mono == null) continue;
                asset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { mono };
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // New font assets get new GUIDs, so the UI prefab must point at them again.
            UIRootBuilder.Build();
        }

        static TMP_FontAsset Create(string file, string assetName)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(SourceFolder + file);
            if (font == null)
            {
                Debug.LogError($"[UiFontsSetup] Missing {SourceFolder}{file}");
                return null;
            }

            var path = $"{TargetFolder}{assetName}.asset";
            AssetDatabase.DeleteAsset(path);

            var asset = TMP_FontAsset.CreateFontAsset(font, 64, 7, GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            asset.name = assetName;
            asset.TryAddCharacters(Prefill, out var missing);

            AssetDatabase.CreateAsset(asset, path);
            foreach (var texture in asset.atlasTextures)
            {
                if (texture == null) continue;
                texture.name = $"{assetName} Atlas";
                AssetDatabase.AddObjectToAsset(texture, asset);
            }
            asset.material.name = $"{assetName} Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            EditorUtility.SetDirty(asset);

            Debug.Log($"[UiFontsSetup] Saved {path}" + (string.IsNullOrEmpty(missing) ? "" : $" (not in font: {missing})"));
            return asset;
        }
    }
}
