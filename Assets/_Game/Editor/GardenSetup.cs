using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Bloomquartz.Editor
{
    public static class GardenSetup
    {
        [MenuItem("Bloomquartz/Setup Garden")]
        public static void SetupGarden()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Garden.unity");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            DestroyIfExists("GardenCanvas");
            DestroyIfExists("EventSystem");
            DestroyIfExists("Garden");
            DestroyIfExists("GemCollector");
            DestroyIfExists("JuiceManager");

            // Camera
            var cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags      = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.1f, 0.04f);
                cam.orthographic    = true;
                // CameraFitter adjusts ortho size to the real device aspect ratio at runtime.
                // Garden uses a 3-row × 2-col portrait layout: slots at x=±1.5, y=±2.8 and 0.
                // Half-extents: width=2.5 (±1.5 + ~1 glow), height=3.8 (±2.8 + ~1 glow).
                // Always remove + re-add so values stay in sync if Setup is run multiple times.
                var existingFitter = cam.GetComponent<Bloomquartz.Juice.CameraFitter>();
                if (existingFitter != null) Object.DestroyImmediate(existingFitter);
                {
                    var fitter   = cam.gameObject.AddComponent<Bloomquartz.Juice.CameraFitter>();
                    var fitterSO = new SerializedObject(fitter);
                    fitterSO.FindProperty("worldHalfWidth").floatValue  = 2.5f;
                    fitterSO.FindProperty("worldHalfHeight").floatValue = 3.8f;
                    fitterSO.FindProperty("padding").floatValue         = 1.1f;
                    fitterSO.ApplyModifiedProperties();
                }

                // Set editor preview size to match the runtime value (~6 for portrait iPhone)
                cam.orthographicSize = 6.0f;

                // Required for IPointerClickHandler on world-space GameObjects
                if (cam.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
                    cam.gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            }

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Managers
            new GameObject("GemCollector").AddComponent<Bloomquartz.Gems.GemCollector>();
            var juiceGO = new GameObject("JuiceManager");
            var juice   = juiceGO.AddComponent<Bloomquartz.Juice.JuiceManager>();
            var juiceSO = new SerializedObject(juice);
            var sparklePrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(
                "Assets/_Game/Prefabs/Particles/GemSparkle.prefab");
            var popPrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(
                "Assets/_Game/Prefabs/Particles/GemPop.prefab");
            var burstPrefab = AssetDatabase.LoadAssetAtPath<ParticleSystem>(
                "Assets/_Game/Prefabs/Particles/GemSparkle.prefab");
            juiceSO.FindProperty("gemSparklePrefab").objectReferenceValue    = sparklePrefab;
            juiceSO.FindProperty("gemPopPrefab").objectReferenceValue        = popPrefab;
            juiceSO.FindProperty("evolutionBurstPrefab").objectReferenceValue = burstPrefab;
            juiceSO.ApplyModifiedProperties();

            // ── PLANT SLOTS (2 rows of 3) ─────────────────────────
            var gardenRoot = new GameObject("Garden");
            var pg = gardenRoot.AddComponent<Bloomquartz.Plants.PlantGarden>();

            var slotsParent = new GameObject("GardenSlots");
            slotsParent.transform.SetParent(gardenRoot.transform);

            // 3 rows × 2 columns — portrait layout that fits narrow iPhone screens.
            // Columns at x = ±1.5,  rows at y = +2.8, 0, -2.8.
            Vector3[] positions = {
                new Vector3(-1.5f,  2.8f, 0), new Vector3(1.5f,  2.8f, 0),
                new Vector3(-1.5f,  0.0f, 0), new Vector3(1.5f,  0.0f, 0),
                new Vector3(-1.5f, -2.8f, 0), new Vector3(1.5f, -2.8f, 0)
            };

            var slotTransforms = new Transform[6];
            for (int i = 0; i < 6; i++)
            {
                var slotGO = new GameObject($"Slot_{i}");
                slotGO.transform.SetParent(slotsParent.transform);
                slotGO.transform.position = positions[i];
                slotTransforms[i] = slotGO.transform;

                // Background circle
                var bgGO = new GameObject("SlotBG");
                bgGO.transform.SetParent(slotGO.transform, false);
                var bgSr = bgGO.AddComponent<SpriteRenderer>();
                bgSr.sprite       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                bgSr.color        = new Color(0.12f, 0.25f, 0.12f, 0.8f);
                bgSr.sortingOrder = 0;
                bgGO.transform.localScale = Vector3.one * 1.8f;

                // Glow ring
                var glowGO = new GameObject("Glow");
                glowGO.transform.SetParent(slotGO.transform, false);
                var glowSr = glowGO.AddComponent<SpriteRenderer>();
                glowSr.sprite       = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                glowSr.color        = new Color(0.5f, 0.5f, 0.5f, 0.15f);
                glowSr.sortingOrder = 1;
                glowGO.transform.localScale = Vector3.one * 2f;

                // Plus label for empty slots
                var plusGO = new GameObject("PlusLabel");
                plusGO.transform.SetParent(slotGO.transform, false);
                plusGO.transform.localPosition = Vector3.zero;
                var plusTMP = plusGO.AddComponent<TextMeshPro>();
                plusTMP.text      = "+";
                plusTMP.fontSize  = 3f;
                plusTMP.alignment = TextAlignmentOptions.Center;
                plusTMP.color     = new Color(1f, 1f, 1f, 0.4f);
                plusTMP.sortingOrder = 2;

                // Collider for tapping
                var col = slotGO.AddComponent<CircleCollider2D>();
                col.radius = 0.9f;

                // GardenSlot script
                var gs  = slotGO.AddComponent<Bloomquartz.Plants.GardenSlot>();
                var gsSO = new SerializedObject(gs);
                gsSO.FindProperty("slotIndex").intValue                = i;
                gsSO.FindProperty("glowRenderer").objectReferenceValue = glowSr;
                gsSO.FindProperty("plusLabel").objectReferenceValue    = plusTMP;
                gsSO.ApplyModifiedProperties();
            }

            // Wire PlantGarden slots
            var pgSO = new SerializedObject(pg);
            var arr  = pgSO.FindProperty("gardenSlots");
            arr.arraySize = 6;
            for (int i = 0; i < 6; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = slotTransforms[i];
            pgSO.ApplyModifiedProperties();

            // ── CANVAS ────────────────────────────────────────────
            var canvasGO = new GameObject("GardenCanvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Gem count — top center
            var gemText = CreateText(canvasGO, "GemCountText", "Gems: 0",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -70), new Vector2(400, 80), 36);
            gemText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.92f, 0.3f);

            // Garden title
            var titleText = CreateText(canvasGO, "TitleText", "GARDEN",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -130), new Vector2(400, 55), 22);
            titleText.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 1f, 0.6f);

            // Bottom nav buttons
            var puzzleBtn = CreateButton(canvasGO, "PuzzleButton", "PUZZLE",
                new Vector2(160, 70), new Vector2(260, 80), 26,
                new Color(0.4f, 0.1f, 0.7f));
            var puzzleRT = puzzleBtn.GetComponent<RectTransform>();
            puzzleRT.anchorMin = new Vector2(0, 0);
            puzzleRT.anchorMax = new Vector2(0, 0);

            var menuBtn = CreateButton(canvasGO, "MenuButton", "MENU",
                new Vector2(-160, 70), new Vector2(220, 80), 26,
                new Color(0.2f, 0.2f, 0.35f));
            var menuRT = menuBtn.GetComponent<RectTransform>();
            menuRT.anchorMin = new Vector2(1, 0);
            menuRT.anchorMax = new Vector2(1, 0);

            // ── SLOT ACTION PANEL ──────────────────────────────────
            var actionPanel = CreatePanel(canvasGO, "SlotActionPanel",
                new Color(0.05f, 0.15f, 0.07f, 0.97f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -200), new Vector2(500, 320));

            var panelTitle = CreateText(actionPanel, "PanelTitle", "Empty Slot",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -50), new Vector2(440, 60), 30);

            // Production rate — shown when a plant is selected
            var rateText = CreateText(actionPanel, "RateText", "1 gem every 30s",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -105), new Vector2(420, 42), 20);
            rateText.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 1f, 0.7f);
            rateText.SetActive(false);

            var plantBtn   = CreateButton(actionPanel, "PlantButton",   "PLANT HERE",
                new Vector2(0, -160), new Vector2(380, 75), 26, new Color(0.2f, 0.55f, 0.2f));
            var evolveBtn  = CreateButton(actionPanel, "EvolveButton",  "EVOLVE",
                new Vector2(0, -160), new Vector2(380, 75), 26, new Color(0.5f, 0.2f, 0.8f));
            var unlockBtn  = CreateButton(actionPanel, "UnlockButton",  "UNLOCK",
                new Vector2(0, -160), new Vector2(380, 75), 26, new Color(0.6f, 0.45f, 0.05f));
            var closeBtn   = CreateButton(actionPanel, "CloseButton",   "CLOSE",
                new Vector2(0, -250), new Vector2(200, 55), 22, new Color(0.3f, 0.1f, 0.1f));

            actionPanel.SetActive(false);

            // ── OFFLINE REWARD PANEL ───────────────────────────────
            var offlinePanel = CreatePanel(canvasGO, "OfflinePanel",
                new Color(0.08f, 0.08f, 0.2f, 0.97f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(580, 200));

            var offlineText = CreateText(offlinePanel, "OfflineText",
                "+0 gems while away!",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -55), new Vector2(520, 65), 26);
            offlineText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

            var claimBtn = CreateButton(offlinePanel, "ClaimButton", "CLAIM",
                new Vector2(0, -140), new Vector2(240, 65), 26, new Color(0.3f, 0.5f, 0.1f));
            offlinePanel.SetActive(false);

            // ── Wire GardenUI ──────────────────────────────────────
            var ui   = canvasGO.AddComponent<Bloomquartz.UI.GardenUI>();
            var uiSO = new SerializedObject(ui);
            uiSO.FindProperty("gemCountText").objectReferenceValue     = gemText.GetComponent<TextMeshProUGUI>();
            uiSO.FindProperty("slotActionPanel").objectReferenceValue  = actionPanel;
            uiSO.FindProperty("slotPanelTitle").objectReferenceValue   = panelTitle.GetComponent<TextMeshProUGUI>();
            uiSO.FindProperty("slotRateText").objectReferenceValue     = rateText.GetComponent<TextMeshProUGUI>();
            uiSO.FindProperty("plantButton").objectReferenceValue      = plantBtn.GetComponent<Button>();
            uiSO.FindProperty("evolveButton").objectReferenceValue     = evolveBtn.GetComponent<Button>();
            uiSO.FindProperty("unlockButton").objectReferenceValue     = unlockBtn.GetComponent<Button>();
            uiSO.FindProperty("closeButton").objectReferenceValue      = closeBtn.GetComponent<Button>();
            uiSO.FindProperty("offlinePanel").objectReferenceValue     = offlinePanel;
            uiSO.FindProperty("offlineText").objectReferenceValue      = offlineText.GetComponent<TextMeshProUGUI>();
            uiSO.ApplyModifiedProperties();

            WireButton(puzzleBtn, ui, "OnPuzzlePressed");
            WireButton(menuBtn,   ui, "OnMenuPressed");
            WireButton(plantBtn,  ui, "OnPlantPressed");
            WireButton(evolveBtn, ui, "OnEvolvePressed");
            WireButton(unlockBtn, ui, "OnUnlockPressed");
            WireButton(closeBtn,  ui, "OnClosePanel");
            WireButton(claimBtn,  ui, "OnClaimOfflineReward");

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log("[Bloomquartz] Garden setup complete.");
            EditorUtility.DisplayDialog("Bloomquartz", "Garden scene built!\n\nPress Play to test.", "OK");
        }

        // ── Helpers ────────────────────────────────────────────────

        private static void DestroyIfExists(string n)
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static GameObject CreateText(GameObject parent, string name, string text,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, int fs)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fs;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static GameObject CreatePanel(GameObject parent, string name, Color color,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label,
            Vector2 pos, Vector2 size, int fs, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            go.AddComponent<Button>();
            var lGO = new GameObject("Label");
            lGO.transform.SetParent(go.transform, false);
            var lrt = lGO.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.sizeDelta = Vector2.zero;
            var tmp = lGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fs;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static void WireButton(GameObject btnGO,
            Bloomquartz.UI.GardenUI ctrl, string method)
        {
            var btn = btnGO.GetComponent<Button>();
            if (btn == null) return;
            var so    = new SerializedObject(btn);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            calls.arraySize++;
            var call = calls.GetArrayElementAtIndex(calls.arraySize - 1);
            call.FindPropertyRelative("m_Target").objectReferenceValue = ctrl;
            call.FindPropertyRelative("m_MethodName").stringValue      = method;
            call.FindPropertyRelative("m_Mode").intValue               = 1;
            call.FindPropertyRelative("m_CallState").intValue          = 2;
            so.ApplyModifiedProperties();
        }
    }
}
