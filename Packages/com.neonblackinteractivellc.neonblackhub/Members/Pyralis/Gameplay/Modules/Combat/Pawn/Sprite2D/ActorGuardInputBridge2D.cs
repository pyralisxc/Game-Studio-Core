using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat, Input",
        CapabilityPath = "Combat/Actions/Actor Guard Input Bridge2D",
        Surface = AuthoringSurface.Goal,
        Summary = "Forwards 2D guard input into a sibling actor guard controller.",
        SetupSteps = new[] 
        { 
            "Add a component that implements IActorGuardController to the same GameObject.",
            "Route input from an adapter into this bridge."
        },
        SuccessChecks = new[] { "Verify the guard feature activates when the guard input is triggered." },
        Tags = new[] { "capability:Combat", "capability:Input" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Combat/Pawn/Sprite2D/Actor Guard Input Bridge 2D")]
    public class ActorGuardInputBridge2D : MonoBehaviour, IActorGuardInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<IActorGuardController>() == null)
                yield return PyralisRuntimeValidationIssue.Required("No sibling IActorGuardController is available for guard input.");
        }
        private IActorGuardController _guardFeature;

        private void Awake()
        {
            _guardFeature = GetComponent<IActorGuardController>();
        }

        public void HandleGuardStartInput()
        {
            ResolveGuardFeature();
            _guardFeature?.BeginGuard();
        }

        public void HandleGuardEndInput()
        {
            ResolveGuardFeature();
            _guardFeature?.EndGuard();
        }

        private void ResolveGuardFeature()
        {
            _guardFeature ??= GetComponent<IActorGuardController>();
        }
    }
}
