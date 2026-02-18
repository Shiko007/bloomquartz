using System.Collections.Generic;
using UnityEngine;
using Bloomquartz.Gems;

namespace Bloomquartz.Juice
{
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem gemPopPrefab;
        [SerializeField] private ParticleSystem gemSparklePrefab;
        [SerializeField] private ParticleSystem evolutionBurstPrefab;

        [Header("Gem Colors")]
        [SerializeField] private Color rubyColor    = new Color(1f, 0.15f, 0.15f);
        [SerializeField] private Color sapphireColor = new Color(0.15f, 0.4f, 1f);
        [SerializeField] private Color emeraldColor  = new Color(0.1f, 0.9f, 0.3f);
        [SerializeField] private Color amethystColor = new Color(0.7f, 0.2f, 1f);
        [SerializeField] private Color topazColor    = new Color(1f, 0.75f, 0.1f);
        [SerializeField] private Color diamondColor  = new Color(0.9f, 0.95f, 1f);

        private Queue<ParticleSystem> _popPool = new Queue<ParticleSystem>();

        private void Awake()
        {
            Instance = this;
        }

        public void PlayGemPop(Vector3 position, GemType gemType)
        {
            if (gemPopPrefab == null) return;

            ParticleSystem ps = GetPooled(_popPool, gemPopPrefab);
            ps.transform.position = position;

            var main = ps.main;
            main.startColor = GetGemColor(gemType);
            ps.Play();

            StartCoroutine(ReturnToPool(ps, _popPool, main.duration + main.startLifetime.constantMax));
        }

        public void PlayGemSparkle(Vector3 position)
        {
            if (gemSparklePrefab == null) return;
            ParticleSystem ps = Instantiate(gemSparklePrefab, position, Quaternion.identity);
            Destroy(ps.gameObject, 2f);
            ps.Play();
        }

        public void PlayEvolutionBurst(Vector3 position)
        {
            if (evolutionBurstPrefab == null) return;
            ParticleSystem ps = Instantiate(evolutionBurstPrefab, position, Quaternion.identity);
            Destroy(ps.gameObject, 3f);
            ps.Play();
        }

        private Color GetGemColor(GemType type) => type switch
        {
            GemType.Ruby      => rubyColor,
            GemType.Sapphire  => sapphireColor,
            GemType.Emerald   => emeraldColor,
            GemType.Amethyst  => amethystColor,
            GemType.Topaz     => topazColor,
            GemType.Diamond   => diamondColor,
            _                 => Color.white
        };

        private ParticleSystem GetPooled(Queue<ParticleSystem> pool, ParticleSystem prefab)
        {
            if (pool.Count > 0)
            {
                var ps = pool.Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }
            return Instantiate(prefab, transform);
        }

        private System.Collections.IEnumerator ReturnToPool(ParticleSystem ps, Queue<ParticleSystem> pool, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }
}
