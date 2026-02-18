using UnityEditor;
using UnityEditor.SceneManagement;

namespace Bloomquartz.Editor
{
    /// When "Always Start From Main Menu" is checked (default: on), pressing
    /// Play from any scene other than MainMenu will automatically open
    /// MainMenu.unity first — matching the real device launch experience.
    [InitializeOnLoad]
    public static class PlayFromMainMenu
    {
        private const string MenuPath = "Bloomquartz/Always Start From Main Menu";
        private const string PrefsKey = "Bloomquartz_PlayFromMainMenu";

        static PlayFromMainMenu()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            bool next = !EditorPrefs.GetBool(PrefsKey, true);
            EditorPrefs.SetBool(PrefsKey, next);
            Menu.SetChecked(MenuPath, next);
        }

        [MenuItem(MenuPath, isValidateFunction: true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefsKey, true));
            return true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            if (!EditorPrefs.GetBool(PrefsKey, true)) return;

            var active = EditorSceneManager.GetActiveScene();
            if (active.name == "MainMenu") return;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
        }
    }
}
