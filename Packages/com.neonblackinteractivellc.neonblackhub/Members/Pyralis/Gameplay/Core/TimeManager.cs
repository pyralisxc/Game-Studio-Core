using System;
using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Singleton manager for game time effects.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Manages global time scale effects such as hit-pause and game freeze.",
        Axioms = AuthoringWorldAxiom.Realtime,
        RequiredInterfaces = new[] { typeof(IHitPauseSink) },
        NativeSetup = new[] { "Add to a Bootstrap child GameObject or assign to GameplaySessionBootstrap." },
        FirstProof = "Calling Freeze(duration) pauses the game for the specified time.",
        ExpertAdvice = "Use TimeManager to create dramatic pauses during combat or UI events. It manages the global Unity Time.timeScale safely and resets it on disable.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/time"
    )]
public class TimeManager : MonoBehaviour, IHitPauseSink
    {
        private Coroutine _freezeCoroutine;

        private void OnDisable()
        {
            ResetTimeScale();
        }

        public void Freeze(float duration)
        {
            if (_freezeCoroutine != null)
            {
                StopCoroutine(_freezeCoroutine);
            }

            _freezeCoroutine = StartCoroutine(FreezeCoroutine(duration));
        }

        private void ResetTimeScale()
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
            
            if (_freezeCoroutine != null)
            {
                StopCoroutine(_freezeCoroutine);
                _freezeCoroutine = null;
            }
        }

        private IEnumerator FreezeCoroutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _freezeCoroutine = null;
        }
    }
}
