using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Input,
        Relevance = "Forwards 2D guard input into an installed Actor Guard feature on ActorFeatureHost.",
        NativeSetup = new[] 
        { 
            "Add ActorFeatureHost to the same GameObject.",
            "Install a module providing IActorGuardController.",
            "Route input from an adapter into this bridge."
        },
        Proof = "Verify the guard feature activates when the guard input is triggered.",
        ExpertAdvice = "Bridge only forwards input; it does not block damage by itself. Ensure the Guard feature is installed in PawnDefinition.",
        CapabilityPath = "Combat/Actions/Actor Guard Input Bridge2D"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Modules/Combat/Pawn/Sprite2D/Actor Guard Input Bridge 2D")]
    public class ActorGuardInputBridge2D : MonoBehaviour, IActorGuardInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<ActorFeatureHost>() == null)
                yield return PyralisRuntimeValidationIssue.Required("ActorFeatureHost is missing. Feature input bridges need it.");
        }
        private ActorFeatureHost _featureHost;
        private IActorGuardController _guardFeature;

        private void Awake()
        {
            _featureHost = GetComponent<ActorFeatureHost>();
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
            _featureHost ??= GetComponent<ActorFeatureHost>();
            if (_featureHost == null)
                return;

            _guardFeature ??= _featureHost.TryGetInstalledFeature(out IActorGuardController feature)
                ? feature
                : null;
        }
    }
}
