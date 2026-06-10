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
        RequiredInterfaces = new[] { typeof(IGameService), typeof(IHitPauseSink) },
        NativeSetup = new[] { "Add to a persistent Bootstrap or CoreServices GameObject" },
        AssignmentFields = new string[0],
        FirstProof = "Calling Freeze(duration) pauses the game for the specified time."
    ,
        ExpertAdvice = "Use TimeManager to create dramatic pauses during combat. It manages the global Unity Time.timeScale safely.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/time")]
    public class TimeManager : MonoBehaviour, IGameService, IHitPauseSink
{
        [Obsolete("Use IObjectResolver to inject TimeManager or resolve it from the active session scope.")]
        public static TimeManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private Coroutine _freezeCoroutine;

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void Freeze(float duration)
        {
            if (_freezeCoroutine != null)
            {
                StopCoroutine(_freezeCoroutine);
            }

            _freezeCoroutine = StartCoroutine(FreezeCoroutine(duration));
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }

        private IEnumerator FreezeCoroutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
}
