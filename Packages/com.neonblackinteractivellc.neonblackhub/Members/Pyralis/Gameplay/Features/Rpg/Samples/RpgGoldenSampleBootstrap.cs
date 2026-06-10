using UnityEngine;
using VContainer;
using VContainer.Unity;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Features.Rpg.Samples
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/Samples/RPG Golden Sample Bootstrap")]
    public sealed class RpgGoldenSampleBootstrap : MonoBehaviour, IStartable
    {
        private RpgGoldenSampleRuntime _runtime;
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(RpgGoldenSampleRuntime runtime, IObjectResolver resolver)
        {
            _runtime = runtime;
            _resolver = resolver;
        }

        public void Start()
        {
            InitializeSampleData();
            Debug.Log("[RpgGoldenSampleBootstrap] Golden Proof initialized.");
        }

        private void InitializeSampleData()
        {
            if (_runtime == null) return;

            // Seed initial state like the factory does
            _runtime.Inventory.TryAddItem(_runtime.Owner, RpgGoldenSampleIds.GoldItemId, 10, out _);
            _runtime.Dialogue.RegisterQuest(_runtime.Quest);
            _runtime.ZoneState.RegisterZone(_runtime.Meadow);
            
            if (_runtime.HubInteractions is HubInteractionService hubInteractions)
            {
                hubInteractions.RegisterQuest(_runtime.Quest);
                hubInteractions.RegisterSkillTree(RpgGoldenSampleIds.SkillTreeId, _runtime.SkillTree);
            }
        }
    }
}