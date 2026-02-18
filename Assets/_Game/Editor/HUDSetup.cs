using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Bloomquartz.Editor
{
    public static class HUDSetup
    {
        [MenuItem("Bloomquartz/Setup HUD & Win-Lose")]
        public static void SetupHUDAndWinLose()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/PuzzleBoard.unity");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            // ScoreManager on Board
            var boardGO = GameObject.Find("Board");
            if (boardGO != null && boardGO.GetComponent<Bloomquartz.Puzzle.ScoreManager>() == null)
                boardGO.AddComponent<Bloomquartz.Puzzle.ScoreManager>();

            // Remove old objects to rebuild cleanly
            DestroyIfExists("HUDCanvas");
            DestroyIfExists("WinLoseController");
            DestroyIfExists("EventSystem");

            // EventSystem — required for all UI button clicks
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // ── CANVAS ────────────────────────────────────────────
            var hudCanvas = new GameObject("HUDCanvas");
            var canvas    = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = hudCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
            hudCanvas.AddComponent<GraphicRaycaster>();

            // ── HUD ELEMENTS (anchored to screen edges) ───────────

            // Score — top left
            var scoreGO = CreateAnchoredText(hudCanvas, "ScoreText", "Score\n<b>0</b>",
                new Vector2(0, 1), new Vector2(0, 1),   // top-left anchor
                new Vector2(160, -80), new Vector2(260, 90), 24);

            // Moves — top right
            var movesGO = CreateAnchoredText(hudCanvas, "MovesText", "Moves\n<b>30</b>",
                new Vector2(1, 1), new Vector2(1, 1),   // top-right anchor
                new Vector2(-160, -80), new Vector2(260, 90), 24);
            movesGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

            // Goal — top center
            var goalGO = CreateAnchoredText(hudCanvas, "GoalText", "Goal: 2,000",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), // top center
                new Vector2(0, -60), new Vector2(340, 60), 20);

            // Combo — mid screen
            var comboGO = CreateAnchoredText(hudCanvas, "ComboText", "x2 COMBO!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 120), new Vector2(440, 70), 38);
            comboGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
            comboGO.SetActive(false);

            // HUDController
            var hudCtrl = hudCanvas.AddComponent<Bloomquartz.UI.HUDController>();
            var hudSO   = new SerializedObject(hudCtrl);
            hudSO.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("movesText").objectReferenceValue = movesGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("goalText").objectReferenceValue  = goalGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("comboText").objectReferenceValue = comboGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("comboRect").objectReferenceValue = comboGO.GetComponent<RectTransform>();
            hudSO.ApplyModifiedProperties();

            // ── BLOCKER (fullscreen, sits behind panels, blocks board clicks) ──
            var blocker = new GameObject("Blocker");
            blocker.transform.SetParent(hudCanvas.transform, false);
            var brt = blocker.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            var bImg = blocker.AddComponent<Image>();
            bImg.color = new Color(0, 0, 0, 0.55f);
            blocker.AddComponent<Button>(); // swallows clicks
            blocker.SetActive(false);

            // ── WIN PANEL ─────────────────────────────────────────
            var winPanel = CreateCenteredPanel(hudCanvas, "WinPanel",
                new Color(0.05f, 0.35f, 0.1f, 0.97f), new Vector2(560, 480));

            CreateAnchoredText(winPanel, "WinTitle", "YOU WIN!",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-70), new Vector2(460,80), 52)
                .GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.3f);

            var winScore = CreateAnchoredText(winPanel, "WinScore", "Score: 0",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-150), new Vector2(420,55), 28);

            // Stars as Image components using generated star sprite
            var starSprite = PrefabSetup.CreateStarSprite();
            var starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var sGO = new GameObject($"Star_{i}");
                sGO.transform.SetParent(winPanel.transform, false);
                var rt = sGO.AddComponent<RectTransform>();
                rt.anchorMin        = new Vector2(0.5f, 1f);
                rt.anchorMax        = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2((i - 1) * 90f, -220);
                rt.sizeDelta        = new Vector2(72, 72);
                var img = sGO.AddComponent<Image>();
                img.sprite = starSprite;
                img.color  = new Color(1f, 1f, 1f, 0.2f);
                starImages[i] = img;
            }

            CreateButton(winPanel, "RetryButton",  "RETRY",    new Vector2(-130, -310), new Vector2(200, 65));
            CreateButton(winPanel, "NextButton",   "GARDEN →", new Vector2(130,  -310), new Vector2(200, 65));
            winPanel.SetActive(false);

            // ── LOSE PANEL ────────────────────────────────────────
            var losePanel = CreateCenteredPanel(hudCanvas, "LosePanel",
                new Color(0.38f, 0.04f, 0.04f, 0.97f), new Vector2(560, 380));

            CreateAnchoredText(losePanel, "LoseTitle", "OUT OF MOVES",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-70), new Vector2(460,70), 38)
                .GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.35f, 0.2f);

            var loseScore = CreateAnchoredText(losePanel, "LoseScore", "Score: 0",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-150), new Vector2(420,55), 28);

            CreateButton(losePanel, "RetryButton2", "TRY AGAIN", new Vector2(-130, -250), new Vector2(200, 65));
            CreateButton(losePanel, "MenuButton",   "MENU",      new Vector2(130,  -250), new Vector2(200, 65));
            losePanel.SetActive(false);

            // ── WinLoseController ─────────────────────────────────
            var wlGO   = new GameObject("WinLoseController");
            var wlCtrl = wlGO.AddComponent<Bloomquartz.UI.WinLoseController>();
            var wlSO   = new SerializedObject(wlCtrl);
            wlSO.FindProperty("blocker").objectReferenceValue       = blocker;
            wlSO.FindProperty("winPanel").objectReferenceValue      = winPanel;
            wlSO.FindProperty("winScoreText").objectReferenceValue  = winScore.GetComponent<TextMeshProUGUI>();
            wlSO.FindProperty("losePanel").objectReferenceValue     = losePanel;
            wlSO.FindProperty("loseScoreText").objectReferenceValue = loseScore.GetComponent<TextMeshProUGUI>();
            var starsArr = wlSO.FindProperty("starImages");
            starsArr.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starsArr.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
            wlSO.ApplyModifiedProperties();

            WireButton(winPanel,  "RetryButton",  wlCtrl, "OnRetryPressed");
            WireButton(winPanel,  "NextButton",   wlCtrl, "OnNextPressed");
            WireButton(losePanel, "RetryButton2", wlCtrl, "OnRetryPressed");
            WireButton(losePanel, "MenuButton",   wlCtrl, "OnMenuPressed");

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log("[Bloomquartz] HUD & Win/Lose setup complete.");
            EditorUtility.DisplayDialog("Bloomquartz", "HUD & Win/Lose rebuilt!\n\nPress Play to test.", "OK");
        }

        // ── Helpers ────────────────────────────────────────────────

        private static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static GameObject CreateCenteredPanel(GameObject parent, string name, Color color, Vector2 size)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateAnchoredText(GameObject parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, int fontSize)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin        = anchorMin;
            rt.anchorMax        = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return go;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label,
            Vector2 anchoredPos, Vector2 size)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.32f);
            go.AddComponent<Button>();

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static void WireButton(GameObject panel, string btnName,
            Bloomquartz.UI.WinLoseController ctrl, string methodName)
        {
            var btnGO = panel.transform.Find(btnName)?.gameObject;
            if (btnGO == null) return;
            var btn = btnGO.GetComponent<Button>();
            if (btn == null) return;
            var so     = new SerializedObject(btn);
            var calls  = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            calls.arraySize++;
            var call = calls.GetArrayElementAtIndex(calls.arraySize - 1);
            call.FindPropertyRelative("m_Target").objectReferenceValue = ctrl;
            call.FindPropertyRelative("m_MethodName").stringValue      = methodName;
            call.FindPropertyRelative("m_Mode").intValue               = 1;
            call.FindPropertyRelative("m_CallState").intValue          = 2;
            so.ApplyModifiedProperties();
        }
    }
}
