using NeonBlack.Gameplay.Core.Contracts;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Drives the company splash/intro scene.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.UI,
        Relevance = "Handles the timed or video-backed splash screen flow.",
        AssignmentFields = new[] { "splashVideo", "splashDuration" }
    )]
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Video (optional - leave empty for static image)")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private RawImage _videoDisplay;
        [SerializeField] private VideoClip _videoClip;

        [Header("Timing")]
        [SerializeField] private float _fallbackDisplaySeconds = 2f;
        [SerializeField] private float _fadeOutSeconds = 0.5f;

        [Header("Next Scene")]
        [SerializeField] private string _nextSceneName = SceneNames.MainMenu;

        [Header("Skip")]
        [SerializeField] private bool _allowSkip = true;
        [SerializeField] private float _skipLockSeconds = 0.5f;

        [Header("Fade")]
        [SerializeField] private Image _blackOverlay;

        private RenderTexture _rt;
        private bool _skipRequested;
        private bool _done;
        private bool _videoFinished;

        private void Start()
        {
            if (_blackOverlay != null)
            {
                Color color = _blackOverlay.color;
                color.a = 0f;
                _blackOverlay.color = color;
            }

            if (_videoPlayer != null && _videoClip != null)
            {
                _rt = new RenderTexture((int)_videoClip.width, (int)_videoClip.height, 0);
                _videoPlayer.clip = _videoClip;
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                _videoPlayer.targetTexture = _rt;
                _videoPlayer.isLooping = false;
                _videoPlayer.playOnAwake = false;
                _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                if (_videoDisplay != null)
                {
                    _videoDisplay.texture = _rt;
                }

                _videoPlayer.loopPointReached += OnVideoFinished;
                _videoPlayer.Play();
            }

            StartCoroutine(SplashRoutine());
        }

        private void Update()
        {
            if (!_allowSkip || _done || Time.realtimeSinceStartup < _skipLockSeconds)
            {
                return;
            }

            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
            {
                _skipRequested = true;
            }

            if (UnityEngine.InputSystem.Touchscreen.current != null &&
                UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                _skipRequested = true;
            }
        }

        private void OnDestroy()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnVideoFinished;
            }

            if (_rt != null)
            {
                _rt.Release();
                _rt = null;
            }
        }

        private IEnumerator SplashRoutine()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(_nextSceneName);
            load.allowSceneActivation = false;

            _videoFinished = _videoPlayer == null || _videoClip == null;

            float elapsed = 0f;
            float minimumSeconds = _videoClip != null ? (float)_videoClip.length : _fallbackDisplaySeconds;

            while ((!_videoFinished && elapsed < minimumSeconds) || load.progress < 0.9f)
            {
                if (_skipRequested)
                {
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _done = true;
            yield return StartCoroutine(FadeOutRoutine());

            if (_videoPlayer != null)
            {
                _videoPlayer.Stop();
                _videoPlayer.targetTexture = null;
            }

            if (_videoDisplay != null)
            {
                _videoDisplay.texture = null;
            }

            if (_rt != null)
            {
                _rt.Release();
                _rt = null;
            }

            yield return null;
            load.allowSceneActivation = true;
        }

        private IEnumerator FadeOutRoutine()
        {
            if (_blackOverlay == null || _fadeOutSeconds <= 0f)
            {
                yield break;
            }

            float elapsed = 0f;
            Color color = _blackOverlay.color;
            while (elapsed < _fadeOutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Clamp01(elapsed / _fadeOutSeconds);
                _blackOverlay.color = color;
                yield return null;
            }

            color.a = 1f;
            _blackOverlay.color = color;
        }

        private void OnVideoFinished(VideoPlayer _)
        {
            _videoFinished = true;
        }
    }
}
