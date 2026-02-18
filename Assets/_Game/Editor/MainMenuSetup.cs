using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Bloomquartz.Editor
{
    public static class MainMenuSetup
    {
        [MenuItem("Bloomquartz/Setup Main Menu")]
        public static void SetupMainMenu()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            // Clear old objects
            DestroyIfExists("MainMenuCanvas");
            DestroyIfExists("EventSystem");
            DestroyIfExists("Managers");
            DestroyIfExists("BackgroundParticles");

            // Camera
            var cam = Object.FindObjectOfType<Camera>();
            if (cam != null)
            {
                cam.clearFlags      = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.055f, 0.03f, 0.13f);
                cam.orthographic    = true;
                cam.orthographicSize = 5f;
            }

            // EventSystem
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Managers — each must be a ROOT GameObject for DontDestroyOnLoad to work
            new GameObject("GameManager").AddComponent<Bloomquartz.Core.GameManager>();
            new GameObject("SaveSystem").AddComponent<Bloomquartz.Core.SaveSystem>();
            new GameObject("IdleManager").AddComponent<Bloomquartz.Idle.IdleManager>();
            new GameObject("DailyChallenge").AddComponent<Bloomquartz.Meta.DailyChallenge>();
            new GameObject("AudioManager").AddComponent<Bloomquartz.Audio.AudioManager>();

            // Background particle effect (floating gems)
            CreateBackgroundParticles();

            // ── CANVAS ────────────────────────────────────────────
            var canvasGO = new GameObject("MainMenuCanvas");
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // ── TITLE ──────────────────────────────────────────
            var titleGO = CreateText(canvasGO, "TitleText", "BLOOMQUARTZ",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -280), new Vector2(800, 160), 82);
            var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
            titleTMP.fontStyle  = FontStyles.Bold;
            titleTMP.color      = new Color(0.95f, 0.85f, 1f);
            titleTMP.enableVertexGradient = true;
            titleTMP.colorGradient = new VertexGradient(
                new Color(1f, 0.8f, 1f),
                new Color(1f, 0.8f, 1f),
                new Color(0.6f, 0.4f, 1f),
                new Color(0.6f, 0.4f, 1f));

            // Subtitle
            var subGO = CreateText(canvasGO, "SubtitleText", "Match • Grow • Harvest",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -390), new Vector2(700, 60), 28);
            subGO.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.6f, 1f);

            // ── BUTTONS GROUP ──────────────────────────────────
            var btnsGO = new GameObject("ButtonsGroup");
            btnsGO.transform.SetParent(canvasGO.transform, false);
            var btnsRT = btnsGO.AddComponent<RectTransform>();
            btnsRT.anchorMin        = new Vector2(0.5f, 0.5f);
            btnsRT.anchorMax        = new Vector2(0.5f, 0.5f);
            btnsRT.anchoredPosition = new Vector2(0, -100);
            btnsRT.sizeDelta        = new Vector2(400, 300);
            var btnsGroup = btnsGO.AddComponent<CanvasGroup>();

            // Play button (large, prominent)
            var playBtn = CreateButton(btnsGO, "PlayButton", "PLAY",
                new Vector2(0, 60), new Vector2(380, 110), 42,
                new Color(0.5f, 0.15f, 0.9f));

            // Garden button
            var gardenBtn = CreateButton(btnsGO, "GardenButton", "GARDEN",
                new Vector2(0, -60), new Vector2(380, 80), 30,
                new Color(0.15f, 0.45f, 0.2f));

            // ── DAILY CHALLENGE BADGE ──────────────────────────
            var badgeGO = CreatePanel(canvasGO, "DailyBadge",
                new Color(0.8f, 0.5f, 0.05f, 0.92f),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 200), new Vector2(500, 80));

            CreateText(badgeGO, "BadgeText", "Daily Challenge Available!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(460, 60), 26);

            // ── OFFLINE REWARD PANEL ───────────────────────────
            var offlinePanel = CreatePanel(canvasGO, "OfflineRewardPanel",
                new Color(0.1f, 0.1f, 0.3f, 0.97f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(580, 220));

            var offlineText = CreateText(offlinePanel, "OfflineText",
                "+0 gems while you were away!",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -50), new Vector2(520, 70), 28);
            offlineText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.4f);

            var claimBtn = CreateButton(offlinePanel, "ClaimButton", "CLAIM",
                new Vector2(0, -130), new Vector2(260, 70), 28,
                new Color(0.4f, 0.6f, 0.1f));
            offlinePanel.SetActive(false);

            // ── VERSION TEXT ───────────────────────────────────
            var verGO = CreateText(canvasGO, "VersionText", "v0.1 Alpha",
                new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-80, 40), new Vector2(200, 40), 18);
            verGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.3f);

            // ── WIRE MainMenuController ────────────────────────
            var ctrl   = canvasGO.AddComponent<Bloomquartz.UI.MainMenuController>();
            var ctrlSO = new SerializedObject(ctrl);
            ctrlSO.FindProperty("titleRect").objectReferenceValue =
                titleGO.GetComponent<RectTransform>();
            ctrlSO.FindProperty("buttonsGroup").objectReferenceValue = btnsGroup;
            ctrlSO.FindProperty("offlineRewardText").objectReferenceValue =
                offlineText.GetComponent<TextMeshProUGUI>();
            ctrlSO.FindProperty("offlineRewardPanel").objectReferenceValue = offlinePanel;
            ctrlSO.ApplyModifiedProperties();

            // Wire buttons
            WireButton(playBtn,   ctrl, "OnPlayPressed");
            WireButton(gardenBtn, ctrl, "OnGardenPressed");
            WireButton(claimBtn,  ctrl, "OnClaimOfflineReward");

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Debug.Log("[Bloomquartz] Main Menu setup complete.");
            EditorUtility.DisplayDialog("Bloomquartz", "Main Menu built!\n\nPress Play to see the intro animation.", "OK");
        }

        private static void CreateBackgroundParticles()
        {
            var go = new GameObject("BackgroundParticles");
            go.transform.position = new Vector3(0, 0, 1);
            var ps   = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop             = true;
            main.startLifetime    = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed       = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
            main.startSize        = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
            main.maxParticles     = 60;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.startColor       = new ParticleSystem.MinMaxGradient(
                new Color(0.8f, 0.4f, 1f, 0.6f),
                new Color(0.4f, 0.7f, 1f, 0.4f));

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale     = new Vector3(12f, 14f, 0f);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.5f, 0.2f),
                    new GradientAlphaKey(0.5f, 0.8f),
                    new GradientAlphaKey(0f, 1f) });
            colorOverLife.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 0;
        }

        // ── Helpers ────────────────────────────────────────────────

        private static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static GameObject CreateText(GameObject parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, int fontSize)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static GameObject CreatePanel(GameObject parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private static GameObject CreateButton(GameObject parent, string name, string label,
            Vector2 pos, Vector2 size, int fontSize, Color color)
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
            tmp.text = label; tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return go;
        }

        private static void WireButton(GameObject btnGO,
            Bloomquartz.UI.MainMenuController ctrl, string method)
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
