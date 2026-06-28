using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        Relevance = "Forwards interact input into a sibling actor interaction receiver.",
        NativeSetup = new[] 
        { 
            "Add a component that implements IActorInteractionRequestReceiver to the same GameObject.",
            "Route input from an adapter into this bridge."
        },
        Proof = "Verify interaction triggers the installed feature.",
        ExpertAdvice = "Bridge only forwards input. Add ActorInteractionComponent or another interaction receiver directly to the pawn root.",
        CapabilityPath = "Input/Pawn/Actor Interaction Input Bridge2D"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Input Bridge 2D")]
    public class ActorInteractionInputBridge2D : MonoBehaviour, IActorInteractionInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<IActorInteractionRequestReceiver>() == null)
                yield return PyralisRuntimeValidationIssue.Required("No sibling IActorInteractionRequestReceiver is available for interact input.");
        }
        private IActorInteractionRequestReceiver _interactionRequests;

        private void Awake()
        {
            _interactionRequests = GetComponent<IActorInteractionRequestReceiver>();
        }

        public void HandleInteractionInput()
        {
            _interactionRequests ??= GetComponent<IActorInteractionRequestReceiver>();
            _interactionRequests?.TryHandleInteraction();
        }
    }
}
