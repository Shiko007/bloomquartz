using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace Bloomquartz.Editor
{
    public static class BloomquartzSetup
    {
        [MenuItem("Bloomquartz/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            CreateMainMenuScene();
            CreatePuzzleBoardScene();
            CreateGardenScene();
            CreateWorldMapScene();
            AddScenesToBuildSettings();
            Debug.Log("[Bloomquartz] All scenes created and added to Build Settings.");
            EditorUtility.DisplayDialog("Bloomquartz Setup", "All scenes created successfully!\n\nCheck File > Build Settings to confirm.", "OK");
        }

        // ── MAIN MENU ──────────────────────────────────────────────
        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            Camera cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.05f, 0.15f);
                cam.orthographic = true;
                cam.orthographicSize = 5f;
            }

            // Canvas
            var canvasGO = CreateCanvas("MainMenuCanvas");

            // Title label
            var title = CreateUIText(canvasGO, "TitleText", "BLOOMQUARTZ",
                new Vector2(0, 150), new Vector2(600, 100), 64);

            // Play button
            var playBtn = CreateUIButton(canvasGO, "PlayButton", "PLAY",
                new Vector2(0, 0), new Vector2(260, 80));

            // Garden button
            var gardenBtn = CreateUIButton(canvasGO, "GardenButton", "GARDEN",
                new Vector2(0, -100), new Vector2(260, 60));

            // Managers root
            var managers = new GameObject("Managers");
            AddComponent<Bloomquartz.Core.GameManager>(managers);
            AddComponent<Bloomquartz.Core.SaveSystem>(managers);
            AddComponent<Bloomquartz.Idle.IdleManager>(managers);
            AddComponent<Bloomquartz.Meta.DailyChallenge>(managers);

            var audioGO = new GameObject("AudioManager");
            audioGO.transform.SetParent(managers.transform);
            AddComponent<Bloomquartz.Audio.AudioManager>(audioGO);

            SaveScene(scene, "Assets/_Game/Scenes/MainMenu.unity");
        }

        // ── PUZZLE BOARD ───────────────────────────────────────────
        private static void CreatePuzzleBoardScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.02f, 0.12f);
                cam.orthographic = true;
                cam.orthographicSize = 6f;
            }

            // Board
            var boardGO = new GameObject("Board");
            AddComponent<Bloomquartz.Puzzle.Board>(boardGO);

            // Juice & FX
            var juiceGO = new GameObject("JuiceManager");
            AddComponent<Bloomquartz.Juice.JuiceManager>(juiceGO);

            // Gem collector
            var collectorGO = new GameObject("GemCollector");
            AddComponent<Bloomquartz.Gems.GemCollector>(collectorGO);

            // HUD Canvas
            var hud = CreateCanvas("HUDCanvas");
            CreateUIText(hud, "GemCountText", "Gems: 0",
                new Vector2(-300, 480), new Vector2(300, 60), 32);
            CreateUIText(hud, "MovesText", "Moves: 30",
                new Vector2(300, 480), new Vector2(300, 60), 32);
            CreateUIButton(hud, "BackButton", "< BACK",
                new Vector2(-430, 480), new Vector2(120, 50));

            SaveScene(scene, "Assets/_Game/Scenes/PuzzleBoard.unity");
        }

        // ── GARDEN ─────────────────────────────────────────────────
        private static void CreateGardenScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.06f, 0.12f, 0.06f);
                cam.orthographic = true;
                cam.orthographicSize = 5f;
            }

            // Garden slots (6 slots in 2 rows)
            var gardenRoot = new GameObject("Garden");
            AddComponent<Bloomquartz.Plants.PlantGarden>(gardenRoot);

            var slotsRoot = new GameObject("GardenSlots");
            slotsRoot.transform.SetParent(gardenRoot.transform);

            Vector3[] slotPositions = new Vector3[]
            {
                new Vector3(-3f,  1f, 0), new Vector3(0f,  1f, 0), new Vector3(3f,  1f, 0),
                new Vector3(-3f, -1.5f, 0), new Vector3(0f, -1.5f, 0), new Vector3(3f, -1.5f, 0)
            };

            for (int i = 0; i < slotPositions.Length; i++)
            {
                var slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(slotsRoot.transform);
                slot.transform.position = slotPositions[i];
            }

            // Gem collector
            var collectorGO = new GameObject("GemCollector");
            AddComponent<Bloomquartz.Gems.GemCollector>(collectorGO);

            // Juice
            var juiceGO = new GameObject("JuiceManager");
            AddComponent<Bloomquartz.Juice.JuiceManager>(juiceGO);

            // UI
            var canvas = CreateCanvas("GardenCanvas");
            CreateUIText(canvas, "GemCountText", "Gems: 0",
                new Vector2(0, 460), new Vector2(400, 60), 32);
            CreateUIButton(canvas, "PuzzleButton", "PUZZLE",
                new Vector2(-200, -470), new Vector2(200, 70));
            CreateUIButton(canvas, "MapButton", "WORLD MAP",
                new Vector2(200, -470), new Vector2(200, 70));

            SaveScene(scene, "Assets/_Game/Scenes/Garden.unity");
        }

        // ── WORLD MAP ──────────────────────────────────────────────
        private static void CreateWorldMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.08f, 0.2f);
                cam.orthographic = true;
                cam.orthographicSize = 5f;
            }

            var mapGO = new GameObject("WorldMap");
            AddComponent<Bloomquartz.Meta.WorldMap>(mapGO);

            var canvas = CreateCanvas("MapCanvas");
            CreateUIText(canvas, "TitleText", "WORLD MAP",
                new Vector2(0, 460), new Vector2(400, 60), 36);
            CreateUIButton(canvas, "BackButton", "< GARDEN",
                new Vector2(-380, 460), new Vector2(160, 50));

            SaveScene(scene, "Assets/_Game/Scenes/WorldMap.unity");
        }

        // ── BUILD SETTINGS ─────────────────────────────────────────
        private static void AddScenesToBuildSettings()
        {
            string[] scenePaths = new[]
            {
                "Assets/_Game/Scenes/MainMenu.unity",
                "Assets/_Game/Scenes/PuzzleBoard.unity",
                "Assets/_Game/Scenes/Garden.unity",
                "Assets/_Game/Scenes/WorldMap.unity"
            };

            var scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);

            EditorBuildSettings.scenes = scenes;
        }

        // ── HELPERS ────────────────────────────────────────────────
        private static void SaveScene(Scene scene, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
            go.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
                UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return go;
        }

        private static GameObject CreateUIText(GameObject parent, string name, string text,
            Vector2 anchoredPos, Vector2 size, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static GameObject CreateUIButton(GameObject parent, string name, string label,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.3f, 0.1f, 0.5f);

            go.AddComponent<UnityEngine.UI.Button>();

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var trt = textGO.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go;
        }

        private static T AddComponent<T>(GameObject go) where T : Component
        {
            return go.AddComponent<T>();
        }
    }
}
