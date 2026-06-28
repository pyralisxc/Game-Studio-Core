using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    [AuthoringContract(
        Category = "Puzzle, Input",
        CapabilityPath = "Input/Pawn/Actor Interaction Input Bridge2D",
        Surface = AuthoringSurface.Goal,
        Summary = "Forwards interact input into a sibling actor interaction receiver.",
        SetupSteps = new[] 
        { 
            "Add a component that implements IActorInteractionRequestReceiver to the same GameObject.",
            "Route input from an adapter into this bridge."
        },
        SuccessChecks = new[] { "Verify interaction triggers the installed feature." },
        Tags = new[] { "capability:Puzzle", "capability:Input" }
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
