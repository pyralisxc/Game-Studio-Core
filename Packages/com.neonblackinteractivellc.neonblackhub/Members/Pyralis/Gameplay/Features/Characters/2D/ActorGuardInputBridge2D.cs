using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Input,
        Relevance = "Forwards 2D guard input into an installed Actor Guard feature on ActorFeatureHost.",
        NativeSetup = new[] 
        { 
            "Add ActorFeatureHost to the same GameObject.",
            "Install a module providing IActorGuardFeature.",
            "Route input from an adapter into this bridge."
        },
        FirstProof = "Verify the guard feature activates when the guard input is triggered.",
        ExpertAdvice = "Bridge only forwards input; it does not block damage by itself. Ensure the Guard feature is installed in PawnDefinition."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Actor Guard Input Bridge 2D")]
    public class ActorGuardInputBridge2D : MonoBehaviour, IActorGuardInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (GetComponent<ActorFeatureHost>() == null)
                yield return "ActorFeatureHost is missing. Feature input bridges need it.";
        }
        private ActorFeatureHost _featureHost;
        private IActorGuardFeature _guardFeature;

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

            _guardFeature ??= _featureHost.TryGetInstalledFeature(out IActorGuardFeature feature)
                ? feature
                : null;
        }
    }
}
