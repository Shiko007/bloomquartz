using UnityEngine;
using Bloomquartz.Core;
using Bloomquartz.Gems;

namespace Bloomquartz.Plants
{
    public class PlantCreature : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlantData data;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer plantRenderer;
        [SerializeField] private Animator plantAnimator;
        [SerializeField] private ParticleSystem evolutionParticles;

        private PlantSaveEntry _saveEntry;
        private GemProducer _producer;
        private int _evolutionLevel;

        public PlantData Data => data;
        public int EvolutionLevel => _evolutionLevel;
        public int GetSlotIndex() => _saveEntry?.slotIndex ?? -1;

        public void Init(PlantSaveEntry entry)
        {
            _saveEntry = entry;
            _evolutionLevel = entry.evolutionLevel;
            _producer = GetComponent<GemProducer>();
            _producer.Init(entry);

            ApplyEvolutionVisuals();
        }

        private void ApplyEvolutionVisuals()
        {
            if (data == null) return;
            if (_evolutionLevel < data.evolutions.Length)
            {
                plantRenderer.sprite = data.evolutions[_evolutionLevel].sprite;
            }
        }

        public bool CanEvolve()
        {
            if (data == null) return false;
            return _evolutionLevel < data.evolutions.Length - 1;
        }

        public void Evolve()
        {
            if (!CanEvolve()) return;
            _evolutionLevel++;
            _saveEntry.evolutionLevel = _evolutionLevel;
            SaveSystem.Instance.Save();

            ApplyEvolutionVisuals();
            PlayEvolutionEffect();
        }

        private void PlayEvolutionEffect()
        {
            if (evolutionParticles != null)
                evolutionParticles.Play();

            if (plantAnimator != null)
                plantAnimator.SetTrigger("Evolve");

            Juice.HapticFeedback.Heavy();
        }
    }
}
