using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bloomquartz.Editor
{
    public static class DebugTools
    {
        [MenuItem("Bloomquartz/Clear Save Data")]
        private static void ClearSaveData()
        {
            string path = Path.Combine(Application.persistentDataPath, "bloomquartz.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[Bloomquartz] Save data deleted: {path}");
                EditorUtility.DisplayDialog("Bloomquartz", "Save data cleared!\nAll progress has been reset.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Bloomquartz", "No save file found — nothing to clear.", "OK");
            }
        }

        [MenuItem("Bloomquartz/Show Save Path")]
        private static void ShowSavePath()
        {
            string path = Path.Combine(Application.persistentDataPath, "bloomquartz.json");
            Debug.Log($"[Bloomquartz] Save path: {path}");
            EditorUtility.DisplayDialog("Bloomquartz", $"Save file location:\n{path}", "OK");
        }
    }
}
