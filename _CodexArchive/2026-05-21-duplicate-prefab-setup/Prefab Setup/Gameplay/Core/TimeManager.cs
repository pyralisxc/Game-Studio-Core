using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Singleton manager for game time effects.
    /// </summary>
    public class TimeManager : MonoBehaviour, IGameService
    {
        public static TimeManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private Coroutine _freezeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Shutdown();
            }
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
