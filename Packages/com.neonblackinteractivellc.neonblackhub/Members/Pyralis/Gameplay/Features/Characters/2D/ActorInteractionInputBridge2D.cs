using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        Relevance = "Forwards interact input into an installed Actor Interaction feature on ActorFeatureHost.",
        NativeSetup = new[] 
        { 
            "Add ActorFeatureHost to the same GameObject.",
            "Install a module providing IActorInteractionFeature."
        },
        FirstProof = "Verify interaction triggers the installed feature.",
        ExpertAdvice = "Bridge only forwards input. Ensure the Interaction feature is installed in PawnDefinition."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Input Bridge 2D")]
    public class ActorInteractionInputBridge2D : MonoBehaviour, IActorInteractionInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (GetComponent<ActorFeatureHost>() == null)
                yield return "ActorFeatureHost is missing. Feature input bridges need it.";
        }
        private ActorFeatureHost _featureHost;
        private IActorInteractionFeature _interactionFeature;

        private void Awake()
        {
            _featureHost = GetComponent<ActorFeatureHost>();
        }

        public void HandleInteractionInput()
        {
            _featureHost ??= GetComponent<ActorFeatureHost>();
            if (_featureHost == null)
                return;

            _interactionFeature ??= _featureHost.TryGetInstalledFeature(out IActorInteractionFeature feature)
                ? feature
                : null;
            _interactionFeature?.TryHandleInteraction();
        }
    }
}
