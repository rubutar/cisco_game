using UnityEditor;
using UnityEngine;

// Applies pixel-art defaults to textures the first time they are imported
// under Assets/_Project/Art: point filtering, no compression, 16 pixels per unit.
public class PixelArtImporter : AssetPostprocessor
{
    const int PixelsPerUnit = 16;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/_Project/Art/") || !assetImporter.importSettingsMissing)
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
    }
}
