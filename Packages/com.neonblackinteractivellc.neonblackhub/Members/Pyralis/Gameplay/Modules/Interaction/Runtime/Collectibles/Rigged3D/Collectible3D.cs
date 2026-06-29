using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    [AuthoringContract(
        Category = "Inventory",
        CapabilityPath = "Interaction/Collectibles/Collectible 3D",
        Surface = AuthoringSurface.Goal,
        Summary = "3D collectible item that awards points and bobs in world space.",
        RequiredFields = new[] { nameof(bobSpeed), nameof(bobHeight) },
        SetupSteps = new[] 
        { 
            "Add to a 3D prefab with a Collider (Is Trigger).",
            "Tune collectible bobbing and score value."
        },
        SuccessChecks = new[] { "Walk an actor into the collectible and verify it disappears and awards points." },
        Tags = new[] { "capability:Inventory", "axiom:Dimensions3D" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Collectibles/Collectible 3D")]
    [RequireComponent(typeof(Collider))]
    public class Collectible3D : GameplayTickBehaviour, IPickupCollectible, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<Collider>() == null)
                yield return PyralisRuntimeValidationIssue.Required("Collider is required for collection detection.");
            else if (!GetComponent<Collider>().isTrigger)
                yield return PyralisRuntimeValidationIssue.Required("Collider must be set to Is Trigger.");
        }
        public int FeedbackScoreValue => 1;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.05f;
        [SerializeField, Tooltip("Optional award sink override. When empty, the collectible resolves an IPickupAwardSink from parents or active gameplay services.")]
        private MonoBehaviour awardSinkSource;

        private Vector3 _originPos;
        private bool _alive;
        private float _localTime;
        private IPickupAwardSink _awardSink;

        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Interaction;
        protected override bool UsesGameplayTick => true;

        private void OnEnable()
        {
            _originPos = transform.position;
            _alive = true;
            _localTime = Random.Range(0f, Mathf.PI * 2f);
            _awardSink ??= ResolveAwardSink();
        }

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (!_alive)
                return;

            _localTime += context.DeltaTime;
            float yOffset = Mathf.Sin(_localTime * bobSpeed) * bobHeight;
            transform.position = new Vector3(_originPos.x, _originPos.y + yOffset, _originPos.z);
        }

        public void CollectBy(GameObject collector)
        {
            if (!_alive)
                return;

            _alive = false;
            _awardSink ??= ResolveAwardSink();
            _awardSink?.ApplyAward(new PickupAwardPayload(collector, transform.position, FeedbackScoreValue, PickupAwardOutcome.Collected));
            gameObject.SetActive(false);
        }

        public bool RemoveFromPlay()
        {
            if (!_alive)
                return false;

            _alive = false;
            gameObject.SetActive(false);
            return true;
        }

        private IPickupAwardSink ResolveAwardSink()
        {
            if (awardSinkSource is IPickupAwardSink configuredSink)
                return configuredSink;

            IPickupAwardSink parentSink = GetComponentInParent<IPickupAwardSink>();
            if (parentSink != null)
                return parentSink;

            return null;
        }
    }
}
