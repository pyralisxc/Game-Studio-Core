using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;

namespace NeonBlack.Gameplay.Runtime.Validation
{
    /// <summary>
    /// Visual and logical validator for Pyralis proof scenes.
    /// Attach it to the proof root and use explicit references when the checked objects live elsewhere.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Automated scene-readiness checker for Proof of Concept scenes.",
        ExpertAdvice = "Add this component to any proof root to verify the bootstrap and core services are healthy."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Validation/Pyralis Proof Validator")]
    public class PyralisProofValidator : MonoBehaviour
    {
        public bool runOnStart = true;
        public bool logSuccess = true;
        [SerializeField] private GameplaySessionBootstrap bootstrap;
        [SerializeField] private PyralisGameplayLifetimeScope lifetimeScope;
        [SerializeField] private Light directionalLight;

        private void Start()
        {
            if (runOnStart)
                Validate();
        }

        [ContextMenu("Run Validation")]
        public void Validate()
        {
            bool hasErrors = false;

            GameplaySessionBootstrap foundBootstrap = bootstrap != null
                ? bootstrap
                : transform.root.GetComponentInChildren<GameplaySessionBootstrap>(true);
            if (foundBootstrap == null)
            {
                Debug.LogError("[ProofValidator] CRITICAL: No GameplaySessionBootstrap found under the proof root.", this);
                hasErrors = true;
            }
            else if (logSuccess)
            {
                Debug.Log("[ProofValidator] Found Bootstrap.", foundBootstrap);
            }

            PyralisGameplayLifetimeScope foundScope = lifetimeScope != null
                ? lifetimeScope
                : transform.root.GetComponentInChildren<PyralisGameplayLifetimeScope>(true);
            if (foundScope == null)
            {
                Debug.LogError("[ProofValidator] CRITICAL: No PyralisGameplayLifetimeScope found under the proof root. Session dependencies will not be injected.", this);
                hasErrors = true;
            }
            else if (logSuccess)
            {
                Debug.Log("[ProofValidator] Lifetime Scope Active.", foundScope);
            }

            Light foundLight = directionalLight != null
                ? directionalLight
                : transform.root.GetComponentInChildren<Light>(true);
            if (foundLight == null || foundLight.type != LightType.Directional)
                Debug.LogWarning("[ProofValidator] Warning: No Directional Light found under the proof root. Proof may be dark.", this);

            if (!hasErrors && logSuccess)
                Debug.Log("[ProofValidator] SUCCESS: Scene baseline is healthy.", this);
        }
    }
}
