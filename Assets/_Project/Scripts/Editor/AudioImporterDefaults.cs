using UnityEditor;
using UnityEngine;

// Import defaults for Assets/_Project/Resources/Audio (written by Tools/audio/make_audio.py), first import only:
// music is compressed (Vorbis) and stays compressed in memory; sound effects decompress on load so they play
// instantly. Mono, no preloading of music, for a small WebGL build.
public class AudioImporterDefaults : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/_Project/Resources/Audio/") || !assetImporter.importSettingsMissing)
            return;

        var importer = (AudioImporter)assetImporter;
        var isMusic = assetPath.Contains("/Music/");
        importer.forceToMono = true;
        importer.loadInBackground = isMusic;
        importer.defaultSampleSettings = new AudioImporterSampleSettings
        {
            loadType = isMusic ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad,
            compressionFormat = AudioCompressionFormat.Vorbis,
            quality = isMusic ? 0.5f : 0.7f,
            sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate,
            preloadAudioData = !isMusic,
        };
    }
}
