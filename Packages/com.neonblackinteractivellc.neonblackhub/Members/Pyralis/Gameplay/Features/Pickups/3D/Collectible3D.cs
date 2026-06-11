using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Pickups
{
    [AuthoringContract(
        Capability = AuthoringCapability.Inventory,
        Relevance = "3D collectible item that awards points and bobs in world space.",
        Axioms = AuthoringWorldAxiom.Dimensions3D,
        NativeSetup = new[] 
        { 
            "Add to a 3D prefab with a Collider (Is Trigger).",
            "Assign an award sink if not using the global service."
        },
        FirstProof = "Walk an actor into the collectible and verify it disappears and awards points.",
        AssignmentFields = new[] { nameof(bobSpeed), nameof(bobHeight), nameof(awardSinkSource) }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Pickups/Collectible 3D")]
    [RequireComponent(typeof(Collider))]
    public class Collectible3D : MonoBehaviour, IPickupCollectible, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (GetComponent<Collider>() == null)
                yield return "Collider is required for collection detection.";
            else if (!GetComponent<Collider>().isTrigger)
                yield return "Collider must be set to Is Trigger.";
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

        private void OnEnable()
        {
            _originPos = transform.position;
            _alive = true;
            _localTime = Random.Range(0f, Mathf.PI * 2f);
            _awardSink ??= ResolveAwardSink();
        }

        private void Update()
        {
            if (!_alive)
                return;

            _localTime += Time.deltaTime;
            float yOffset = Mathf.Sin(_localTime * bobSpeed) * bobHeight;
            transform.position = new Vector3(_originPos.x, _originPos.y + yOffset, _originPos.z);
        }

        [Inject]
        private void Construct(IPickupAwardSink awardSink = null)
        {
            _awardSink = awardSink;
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
