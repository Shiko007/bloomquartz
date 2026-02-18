using UnityEngine;
using UnityEditor;

namespace Bloomquartz.Editor
{
    public static class ParticleSetup
    {
        [MenuItem("Bloomquartz/Create Particle Prefabs")]
        public static void CreateParticlePrefabs()
        {
            CreateGemPopPrefab();
            CreateGemSparklePrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Bloomquartz Setup",
                "Particle prefabs created!\n\nAssign them to the JuiceManager in the PuzzleBoard scene:\n• GemPop → Gem Pop Prefab\n• GemSparkle → Gem Sparkle Prefab", "OK");
        }

        private static void CreateGemPopPrefab()
        {
            var go = new GameObject("GemPop");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = Color.white;
            main.gravityModifier = 0.3f;
            main.maxParticles = 20;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 12)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 10;

            PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Game/Prefabs/Particles/GemPop.prefab");
            Object.DestroyImmediate(go);
        }

        private static void CreateGemSparklePrefab()
        {
            var go = new GameObject("GemSparkle");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(1f, 0.95f, 0.6f);
            main.gravityModifier = -0.1f;
            main.maxParticles = 15;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 8)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 10;

            PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Game/Prefabs/Particles/GemSparkle.prefab");
            Object.DestroyImmediate(go);
        }
    }
}
