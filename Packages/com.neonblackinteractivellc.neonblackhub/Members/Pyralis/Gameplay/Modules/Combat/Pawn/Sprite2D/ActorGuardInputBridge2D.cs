using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Input,
        Relevance = "Forwards 2D guard input into a sibling actor guard controller.",
        NativeSetup = new[] 
        { 
            "Add a component that implements IActorGuardController to the same GameObject.",
            "Route input from an adapter into this bridge."
        },
        Proof = "Verify the guard feature activates when the guard input is triggered.",
        ExpertAdvice = "Bridge only forwards input; it does not block damage by itself. Add ActorCombatReactionComponent or another guard controller directly to the pawn root.",
        CapabilityPath = "Combat/Actions/Actor Guard Input Bridge2D"
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
