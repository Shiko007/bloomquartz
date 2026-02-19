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

            // ── CENTRE BOARD IN THE AREA BELOW THE HUD ───────────
            // The HUD occupies ~210 px at the top of a 1920 px canvas.
            // Shift the board down so it is visually centred in the
            // remaining playable area instead of the raw screen centre.
            if (boardGO != null)
            {
                var boardCam = Object.FindObjectOfType<Camera>();
                float orthoHeight = (boardCam != null && boardCam.orthographic)
                    ? boardCam.orthographicSize * 2f
                    : 10f; // sensible fallback for a typical puzzle board camera
                const float hudPx     = 210f;
                const float canvasPx  = 1920f;
                float yOffset = -(orthoHeight * (hudPx / canvasPx)) / 2f;
                boardGO.transform.position = new Vector3(0f, yOffset, 0f);
            }

            // Remove old objects to rebuild cleanly
            DestroyIfExists("HUDCanvas");
            DestroyIfExists("WinLoseController");
            DestroyIfExists("EventSystem");
            DestroyIfExists("FloatingTextPool");
            DestroyIfExists("AudioManager");

            // EventSystem — required for all UI button clicks
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // ── CANVAS ────────────────────────────────────────────
            // Reference 390×844 ≈ iPhone logical-point size → fontSize values map
            // directly to screen points (no 40% shrinkage from the old 1080×1920 ref).
            var hudCanvas = new GameObject("HUDCanvas");
            var canvas    = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = hudCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.matchWidthOrHeight  = 1f;   // match height → consistent vertical layout
            hudCanvas.AddComponent<GraphicRaycaster>();

            // ── SAFE-AREA PANEL ───────────────────────────────────
            // SafeAreaFitter pushes everything below the Dynamic Island at runtime.
            var safePanel = new GameObject("SafeAreaPanel");
            safePanel.transform.SetParent(hudCanvas.transform, false);
            var spRt = safePanel.AddComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero; spRt.anchorMax = Vector2.one;
            spRt.offsetMin = Vector2.zero; spRt.offsetMax = Vector2.zero;
            safePanel.AddComponent<Bloomquartz.UI.SafeAreaFitter>();

            // ── HUD ELEMENTS (all inside SafeAreaPanel, positions in logical pt) ──
            // Row 1 (y -26): Score (left, 10pt inset) | Moves (right, 10pt inset)
            // Row 2 (y -58): Level | Goal (center)
            // Row 3 (y -84): Gems (center-left) | < Menu (right, 10pt inset)
            // Row 4 (y-112): [+5 Moves] [Bomb] [Shuffle]

            // Score — top left, symmetric inset
            // anchoredPosition.x = width/2 + margin = 50+10 = 60 so the LEFT EDGE is 10pt inset
            // (with pivot (0.5,0.5), using x=10 would place the center 10pt from edge → left edge at -40)
            var scoreGO = CreateAnchoredText(safePanel, "ScoreText", "Score\n<b>0</b>",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(60, -26), new Vector2(100, 48), 13);
            // Left-align so the "S" of "Score" sits flush with the 10pt inset — matches
            // the mirror right-align on Moves (right edge flush with 10pt inset on the right).
            scoreGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Moves — top right, same inset: x = -(width/2 + margin) = -(50+10) = -60
            var movesGO = CreateAnchoredText(safePanel, "MovesText", "Moves\n<b>30</b>",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-60, -26), new Vector2(100, 48), 13);
            movesGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

            // Goal — top center
            var goalGO = CreateAnchoredText(safePanel, "GoalText", "Level 1 | Goal: 800",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -58), new Vector2(220, 28), 12);

            // Gem count — centered in row 3 (Menu button moved to row 1 so x=0 now)
            var gemCountGO = CreateAnchoredText(safePanel, "GemCountText", "Gems: 0",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -84), new Vector2(165, 24), 11);
            gemCountGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.88f, 0.3f);

            // Combo — mid screen
            var comboGO = CreateAnchoredText(safePanel, "ComboText", "x2 COMBO!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 80), new Vector2(200, 42), 18);
            comboGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
            comboGO.SetActive(false);

            // HUDController
            var hudCtrl = safePanel.AddComponent<Bloomquartz.UI.HUDController>();
            var hudSO   = new SerializedObject(hudCtrl);
            hudSO.FindProperty("scoreText").objectReferenceValue    = scoreGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("movesText").objectReferenceValue    = movesGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("goalText").objectReferenceValue     = goalGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("gemCountText").objectReferenceValue = gemCountGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("comboText").objectReferenceValue    = comboGO.GetComponent<TextMeshProUGUI>();
            hudSO.FindProperty("comboRect").objectReferenceValue    = comboGO.GetComponent<RectTransform>();
            hudSO.ApplyModifiedProperties();

            // ── BLOCKER ───────────────────────────────────────────
            var blocker = new GameObject("Blocker");
            blocker.transform.SetParent(hudCanvas.transform, false);
            var brt = blocker.AddComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            var bImg = blocker.AddComponent<Image>();
            bImg.color = new Color(0, 0, 0, 0.55f);
            blocker.AddComponent<Button>();
            blocker.SetActive(false);

            // ── WIN PANEL (280×320 pt) ────────────────────────────
            var winPanel = CreateCenteredPanel(hudCanvas, "WinPanel",
                new Color(0.05f, 0.35f, 0.1f, 0.97f), new Vector2(300, 330));

            CreateAnchoredText(winPanel, "WinTitle", "YOU WIN!",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-42), new Vector2(270,52), 22)
                .GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.95f, 0.3f);

            var winScore = CreateAnchoredText(winPanel, "WinScore", "Score: 0",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-102), new Vector2(250,34), 14);

            var starSprite = PrefabSetup.CreateStarSprite();
            var starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var sGO = new GameObject($"Star_{i}");
                sGO.transform.SetParent(winPanel.transform, false);
                var rt = sGO.AddComponent<RectTransform>();
                rt.anchorMin        = new Vector2(0.5f, 1f);
                rt.anchorMax        = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2((i - 1) * 56f, -148);
                rt.sizeDelta        = new Vector2(44, 44);
                var img = sGO.AddComponent<Image>();
                img.sprite = starSprite;
                img.color  = new Color(1f, 1f, 1f, 0.2f);
                starImages[i] = img;
            }

            // Buttons use anchor=(0.5,1) so y is measured from the PANEL TOP.
            // With panel height=330 (top at +165 from center):
            //   NextLevel center = 165-210 = -45 → bottom = -69  (within ±165) ✓
            //   Retry/Garden center = 165-267 = -102 → bottom = -123 (within ±165) ✓
            CreateButton(winPanel, "NextLevelButton", "NEXT LEVEL >>",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -210), new Vector2(240, 48),
                new Color(0.12f, 0.55f, 0.20f));
            CreateButton(winPanel, "RetryButton",  "RETRY",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(-68, -267), new Vector2(118, 42));
            CreateButton(winPanel, "NextButton",   "GARDEN →",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2( 68, -267), new Vector2(118, 42));
            winPanel.SetActive(false);

            // ── LOSE PANEL (280×240 pt) ───────────────────────────
            var losePanel = CreateCenteredPanel(hudCanvas, "LosePanel",
                new Color(0.38f, 0.04f, 0.04f, 0.97f), new Vector2(300, 250));

            CreateAnchoredText(losePanel, "LoseTitle", "OUT OF MOVES",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-42), new Vector2(270,46), 18)
                .GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.35f, 0.2f);

            var loseScore = CreateAnchoredText(losePanel, "LoseScore", "Score: 0",
                new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-100), new Vector2(250,34), 14);

            // anchor=(0.5,1): panel height=250 (top at +125 from center)
            //   Buttons center = 125-154 = -29 → bottom = -50 (within ±125) ✓
            CreateButton(losePanel, "RetryButton2", "TRY AGAIN",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(-68, -154), new Vector2(118, 42));
            CreateButton(losePanel, "MenuButton",   "MENU",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2( 68, -154), new Vector2(118, 42));
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

            WireButton(winPanel,  "NextLevelButton", wlCtrl, "OnNextLevelPressed");
            WireButton(winPanel,  "RetryButton",     wlCtrl, "OnRetryPressed");
            WireButton(winPanel,  "NextButton",      wlCtrl, "OnNextPressed");
            WireButton(losePanel, "RetryButton2",    wlCtrl, "OnRetryPressed");
            WireButton(losePanel, "MenuButton",      wlCtrl, "OnMenuPressed");

            // ── POWER-UP PANEL (row 4) ────────────────────────────
            var powerupHandler = safePanel.AddComponent<Bloomquartz.UI.PowerupHandler>();

            var powerupPanel = new GameObject("PowerupPanel");
            powerupPanel.transform.SetParent(safePanel.transform, false);
            var prt = powerupPanel.AddComponent<RectTransform>();
            prt.anchorMin        = new Vector2(0.5f, 1f);
            prt.anchorMax        = new Vector2(0.5f, 1f);
            prt.anchoredPosition = new Vector2(0f, -112f);
            prt.sizeDelta        = new Vector2(376f, 40f);

            // 3 buttons × 112pt + 2 × 20pt gap = 376pt panel
            // centres: -132, 0, +132
            var movesBtn   = CreateButton(powerupPanel, "BuyMovesBtn",  "+5 Moves 200g", new Vector2(-132f, 0f), new Vector2(112f, 36f), new Color(0.15f, 0.45f, 0.15f));
            var bombBtn    = CreateButton(powerupPanel, "BombBtn",      "Bomb 250g",     new Vector2(   0f, 0f), new Vector2(112f, 36f), new Color(0.50f, 0.15f, 0.10f));
            var shuffleBtn = CreateButton(powerupPanel, "ShuffleBtn",   "Shuffle 150g",  new Vector2( 132f, 0f), new Vector2(112f, 36f), new Color(0.15f, 0.25f, 0.50f));

            WireButtonToComponent(movesBtn,   powerupHandler, "BuyMoves");
            WireButtonToComponent(bombBtn,    powerupHandler, "BombPowerUp");
            WireButtonToComponent(shuffleBtn, powerupHandler, "ShufflePowerUp");

            // < Menu — row 1 CENTER, between Score (x:10-110) and Moves (x:280-380).
            // Button box at center = x:155-235 → no overlap with either stat box.
            // This keeps it visually above Goal/Gems/Powerups and well away from Shuffle.
            var menuBtn = CreateButton(safePanel, "MenuButton", "< Menu",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -26f), new Vector2(80f, 28f), new Color(0.15f, 0.15f, 0.35f));
            WireButtonToComponent(menuBtn, powerupHandler, "GoToMenu");

            // ── AUDIO MANAGER (fallback if not carried from MainMenu) ────
            var amGO = new GameObject("AudioManager");
            amGO.AddComponent<Bloomquartz.Audio.AudioManager>();

            // ── CAMERA SHAKER + PHYSICS2D RAYCASTER ──────────────
            // Physics2DRaycaster is required so tiles can use IPointerClickHandler,
            // which is properly blocked by UI buttons (unlike OnMouseDown).
            var cam = GameObject.Find("Main Camera");
            if (cam != null)
            {
                if (cam.GetComponent<Bloomquartz.Juice.CameraShaker>() == null)
                    cam.AddComponent<Bloomquartz.Juice.CameraShaker>();
                if (cam.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
                    cam.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            }

            // ── FLOATING TEXT POOL ────────────────────────────────
            var ftpGO = new GameObject("FloatingTextPool");
            ftpGO.AddComponent<Bloomquartz.Juice.FloatingTextPool>();

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

        // Overload: explicit anchor min/max for corner-pinned buttons
        private static GameObject CreateButton(GameObject parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size, Color? bgColor = null)
        {
            var go = CreateButton(parent, name, label, anchoredPos, size, bgColor);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            return go;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label,
            Vector2 anchoredPos, Vector2 size, Color? bgColor = null)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            var img = go.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.18f, 0.18f, 0.32f);
            go.AddComponent<Button>();

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static void WireButtonToComponent(GameObject btnGO,
            UnityEngine.Component ctrl, string methodName)
        {
            var btn = btnGO?.GetComponent<Button>();
            if (btn == null) return;
            var so    = new SerializedObject(btn);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            calls.arraySize++;
            var call = calls.GetArrayElementAtIndex(calls.arraySize - 1);
            call.FindPropertyRelative("m_Target").objectReferenceValue = ctrl;
            call.FindPropertyRelative("m_MethodName").stringValue      = methodName;
            call.FindPropertyRelative("m_Mode").intValue               = 1;
            call.FindPropertyRelative("m_CallState").intValue          = 2;
            so.ApplyModifiedProperties();
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
