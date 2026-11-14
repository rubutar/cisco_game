using UnityEditor;
using UnityEngine;

// Applies pixel-art defaults to textures the first time they are imported under Assets/_Project/Art or
// Assets/_Project/Resources/Sprites (written by Tools/sprites/make_sprites.py): sprite, point filtering,
// no compression, no mipmaps, 16 pixels per unit. Sprites in a Characters folder get their pivot at the feet.
public class PixelArtImporter : AssetPostprocessor
{
    const int PixelsPerUnit = 16;

    void OnPreprocessTexture()
    {
        var pixelArt = assetPath.StartsWith("Assets/_Project/Art/") || assetPath.StartsWith("Assets/_Project/Resources/Sprites/");
        if (!pixelArt || !assetImporter.importSettingsMissing)
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        if (assetPath.Contains("/Characters/"))
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
        }
    }
}
