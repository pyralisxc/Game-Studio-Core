using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/Hub Interaction Scene Controller")]
    public sealed class HubInteractionSceneController : MonoBehaviour, IActorInteractionHandler, IRuntimeValidationProvider
    {
        [Header("Hub")]
        [SerializeField] private HubDefinition hubDefinition;

        [Header("HUD")]
        [SerializeField] private HubInteractionHudPresenter hudPresenter;

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Behavior")]
        [SerializeField] private bool refreshOnEnable;
        [SerializeField] private bool clearOnDisable = true;
        [SerializeField] private bool refreshOnTriggerEnter = true;
        [SerializeField] private bool clearOnTriggerExit = true;

        private HubInteractionService _interactionService;
        private IHubDefinition _runtimeHub;
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;

        public HubInteractionResult LastResult { get; private set; } = HubInteractionResult.Invalid("No hub interaction has been selected yet.");

        [Inject]
        private void Construct(
            HubInteractionService interactionService,
            HubInteractionHudPresenter injectedPresenter = null)
        {
            _interactionService = interactionService;
            if (hudPresenter == null)
                hudPresenter = injectedPresenter;
        }

        private void Awake()
        {
            ResolveHudPresenter();
        }

        private void OnEnable()
        {
            BindPresenter();

            if (refreshOnEnable)
                RefreshAvailableInteractions();
        }

        private void OnDisable()
        {
            UnbindPresenter();

            if (clearOnDisable)
                hudPresenter?.ClearPrompts();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (refreshOnTriggerEnter)
                RefreshAvailableInteractions();
        }

        private void OnTriggerExit(Collider other)
        {
            if (clearOnTriggerExit)
                hudPresenter?.ClearPrompts();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (refreshOnTriggerEnter)
                RefreshAvailableInteractions();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (clearOnTriggerExit)
                hudPresenter?.ClearPrompts();
        }

        public void ConfigureForTests(IHubDefinition hub, HubInteractionHudPresenter presenter, RpgOwnerKey owner, HubInteractionService service)
        {
            UnbindPresenter();
            _runtimeHub = hub;
            hudPresenter = presenter;
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _interactionService = service ?? new HubInteractionService();
            BindPresenter();
        }

        public bool TryHandleInteraction(ActorFeatureContext context)
        {
            if (context != null && context.Participant != null && string.IsNullOrWhiteSpace(ownerStableId) && !_hasRuntimeOwner)
                _runtimeOwner = new RpgOwnerKey(RpgOwnerKind.Participant, $"seat-{context.Participant.SeatIndex + 1}");

            HubInteractionResult[] results = RefreshAvailableInteractions();
            return results.Any(result => result.Status == HubInteractionStatus.Available || result.Status == HubInteractionStatus.Locked);
        }

        public HubInteractionResult[] RefreshAvailableInteractions()
        {
            ResolveHudPresenter();
            EnsureInteractionService();

            IHubDefinition hub = ResolveHub();
            RpgOwnerKey owner = ResolveOwner();
            HubInteractionResult[] results = _interactionService.GetAvailableInteractions(owner, hub);
            hudPresenter?.ShowPrompts(results
                .Where(result => result.Status == HubInteractionStatus.Available || result.Status == HubInteractionStatus.Locked)
                .Select(result => result.Prompt));
            return results;
        }

        public HubInteractionResult ConfirmInteractable(string interactableId)
        {
            EnsureInteractionService();

            IHubDefinition hub = ResolveHub();
            RpgOwnerKey owner = ResolveOwner();
            LastResult = _interactionService.SelectInteraction(owner, hub, interactableId);
            hudPresenter?.ShowInteractionResult(LastResult);
            RefreshAvailableInteractions();
            return LastResult;
        }

        public System.Collections.Generic.IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (hubDefinition == null && _runtimeHub == null)
                yield return "`HubInteractionSceneController` should reference a Hub Definition.";

            if (hudPresenter == null && GetComponentInChildren<HubInteractionHudPresenter>(true) == null)
                yield return "`HubInteractionSceneController` should reference a Hub Interaction HUD Presenter or have one as a child.";

            if (!_hasRuntimeOwner && string.IsNullOrWhiteSpace(ownerStableId))
                yield return "`HubInteractionSceneController` needs Owner Stable Id unless a project input bridge supplies participant context.";
        }

        private void HandlePromptConfirmed(HubPromptPayload prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt.InteractableId))
                return;

            ConfirmInteractable(prompt.InteractableId);
        }

        private IHubDefinition ResolveHub()
        {
            return _runtimeHub ?? hubDefinition;
        }

        private RpgOwnerKey ResolveOwner()
        {
            if (_hasRuntimeOwner)
                return _runtimeOwner;

            return new RpgOwnerKey(ownerKind, ownerStableId);
        }

        private void ResolveHudPresenter()
        {
            if (hudPresenter == null)
                hudPresenter = GetComponentInChildren<HubInteractionHudPresenter>(true);
        }

        private void EnsureInteractionService()
        {
            if (_interactionService == null)
                _interactionService = new HubInteractionService();
        }

        private void BindPresenter()
        {
            ResolveHudPresenter();
            if (hudPresenter != null)
                hudPresenter.PromptConfirmed += HandlePromptConfirmed;
        }

        private void UnbindPresenter()
        {
            if (hudPresenter != null)
                hudPresenter.PromptConfirmed -= HandlePromptConfirmed;
        }
    }
}
