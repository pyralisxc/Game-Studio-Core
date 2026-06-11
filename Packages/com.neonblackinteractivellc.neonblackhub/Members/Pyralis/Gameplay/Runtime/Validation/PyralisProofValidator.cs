using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Runtime.Validation
{
    /// <summary>
    /// Visual and logical validator for Pyralis Proof scenes.
    /// Flags issues with setup, connectivity, and baseline readiness.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Automated scene-readiness checker for Proof of Concept scenes.",
        ExpertAdvice = "Add this component to any proof scene to verify the bootstrap and core services are healthy."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Validation/Pyralis Proof Validator")]
    public class PyralisProofValidator : MonoBehaviour
    {
        public bool runOnStart = true;
        public bool logSuccess = true;

        private void Start()
        {
            if (runOnStart)
            {
                Validate();
            }
        }

        [ContextMenu("Run Validation")]
        public void Validate()
        {
            bool hasErrors = false;
            
            // Check Bootstrap
            var bootstrap = Object.FindAnyObjectByType<NeonBlack.Gameplay.Characters.GameplaySessionBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError("[ProofValidator] CRITICAL: No GameplaySessionBootstrap found in scene.", this);
                hasErrors = true;
            }
            else
            {
                if (logSuccess) Debug.Log("[ProofValidator] ✓ Found Bootstrap.", bootstrap);
            }

            // Check Lifetime Scope
            var scope = Object.FindAnyObjectByType<NeonBlack.Gameplay.Core.Runtime.PyralisGameplayLifetimeScope>();
            if (scope == null)
            {
                Debug.LogError("[ProofValidator] CRITICAL: No PyralisGameplayLifetimeScope found. Session dependencies will not be injected.", this);
                hasErrors = true;
            }
            else
            {
                if (logSuccess) Debug.Log("[ProofValidator] ✓ Lifetime Scope Active.", scope);
            }

            // Check Environment
            var light = Object.FindAnyObjectByType<Light>();
            if (light == null || light.type != LightType.Directional)
            {
                Debug.LogWarning("[ProofValidator] Warning: No Directional Light found. Proof may be dark.", this);
            }

            if (!hasErrors && logSuccess)
            {
                Debug.Log("[ProofValidator] SUCCESS: Scene baseline is healthy.", this);
            }
        }
    }
}