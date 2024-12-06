#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AudioBatchProcessor : EditorWindow
{
    [MenuItem("Tools/Batch Set WAVs to Decompressed and Accessible")]
    static void SetDecompressOnLoadForAllWavClips()
    {
        string[] audioClipGUIDs = AssetDatabase.FindAssets("t:AudioClip");
        foreach (string guid in audioClipGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(path);

            if (importer != null && path.EndsWith(".wav"))
            {
                // Update settings to make sure audio is PCM and fully decompressed in memory
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;

                // Set to PCM to maintain uncompressed form
                settings.compressionFormat = AudioCompressionFormat.PCM;

                // DecompressOnLoad to make sure all data is available for GetData()
                settings.loadType = AudioClipLoadType.DecompressOnLoad;

                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

                importer.defaultSampleSettings = settings;

                // Ensure the platform-specific sample settings are also set the same way
                importer.SetOverrideSampleSettings("WebGL", settings);

                // Force reimport to apply changes
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        Debug.Log("Batch Set WAV clips to PCM and DecompressOnLoad completed.");
    }
}
#endif
