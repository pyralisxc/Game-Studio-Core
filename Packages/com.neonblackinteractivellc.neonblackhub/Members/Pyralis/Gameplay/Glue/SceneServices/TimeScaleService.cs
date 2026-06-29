using System;
using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneServices
{
    /// <summary>
    /// Runtime service for global time-scale effects such as hit pause.
    /// </summary>
    [AuthoringContract(
        Category = "Setup",
        CapabilityPath = "Core Setup/Runtime/Time Scale Service",
        Surface = AuthoringSurface.Service,
        Summary = "Manages global time scale effects such as hit-pause and game freeze.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/time",
        RequiredInterfaces = new[] { typeof(IHitPauseSink) },
        SetupSteps = new[] { "Add to a Bootstrap child GameObject or assign to GameplaySessionBootstrap." },
        SuccessChecks = new[] { "Calling Freeze(duration) pauses the game for the specified time." },
        Tags = new[] { "capability:Setup", "axiom:Realtime" },
        Selectable = false
    )]
    public class TimeScaleService : MonoBehaviour, ITimeScaleService
    {
        private Coroutine _freezeCoroutine;
        private bool _isPaused;
        private bool _isHitPaused;

        public bool IsGameplayActive => CanGameplayTick;
        public bool IsPaused => _isPaused;
        public bool IsHitPaused => _isHitPaused;
        public bool CanGameplayTick => !_isPaused && !_isHitPaused;
        public float TimeScale => Time.timeScale;

        private void OnDisable()
        {
            ResetTimeScale();
        }

        public void Freeze(float duration)
        {
            if (duration <= 0f)
                return;

            if (_freezeCoroutine != null)
            {
                StopCoroutine(_freezeCoroutine);
            }

            _freezeCoroutine = StartCoroutine(FreezeCoroutine(duration));
        }

        private void ResetTimeScale()
        {
            _isPaused = false;
            _isHitPaused = false;

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
            _isHitPaused = true;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            _isHitPaused = false;
            Time.timeScale = 1f;
            _freezeCoroutine = null;
        }
    }
}
