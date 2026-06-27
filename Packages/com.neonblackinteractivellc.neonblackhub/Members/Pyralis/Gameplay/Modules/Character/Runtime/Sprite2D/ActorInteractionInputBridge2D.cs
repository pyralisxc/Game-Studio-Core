using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using NeonBlack.Gameplay.Modules.Interaction;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Input,
        Relevance = "Forwards interact input into an installed Actor Interaction feature on ActorFeatureHost.",
        NativeSetup = new[] 
        { 
            "Add ActorFeatureHost to the same GameObject.",
            "Install a module providing IActorInteractionFeature."
        },
        Proof = "Verify interaction triggers the installed feature.",
        ExpertAdvice = "Bridge only forwards input. Ensure the Interaction feature is installed in PawnDefinition.",
        CapabilityPath = "Input/Pawn/Actor Interaction Input Bridge2D"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Interaction/Actor Interaction Input Bridge 2D")]
    public class ActorInteractionInputBridge2D : MonoBehaviour, IActorInteractionInputReceiver2D, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (GetComponent<ActorFeatureHost>() == null)
                yield return PyralisRuntimeValidationIssue.Required("ActorFeatureHost is missing. Feature input bridges need it.");
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
