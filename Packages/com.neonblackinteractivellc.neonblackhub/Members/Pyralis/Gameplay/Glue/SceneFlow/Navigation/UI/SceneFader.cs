using System;
using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Glue.SceneFlow.Navigation
{
    /// <summary>
    /// Scene navigation service that fades the screen before and after loading scenes.
    /// </summary>
    [AuthoringContract(
        Category = "Setup",
        CapabilityPath = "Core Setup/Navigation/Scene Fader",
        Surface = AuthoringSurface.Service,
        Summary = "Persistent ISceneNavigator that fades through a runtime-owned overlay, optionally routes through the loading screen, and restores time scale.",
        SetupSteps = new[] 
        { 
            "Place one SceneFader in the bootstrap or first navigation scene.",
            "Tune fade durations when the defaults do not fit the project.",
            "The fade overlay is created at runtime by this service.",
            "Use FadeToSceneViaLoader when the LoadingScreen scene should show progress."
        },
        SuccessChecks = new[] { "Initiate a scene transition and verify the screen fades smoothly to black." },
        Tags = new[] { "capability:Setup" },
        Selectable = false
    )]
    [DefaultExecutionOrder(-40)]
    public class SceneFader : MonoBehaviour, ISceneNavigator
    {
        [SerializeField, Range(0.05f, 2f)] private float _fadeOutDuration = 0.35f;
        [SerializeField, Range(0.05f, 2f)] private float _fadeInDuration = 0.35f;

        private Image _overlay;
        private bool _busy;

        public static string PendingScene { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            PendingScene = null;
        }

        private void Awake()
        {
            BuildOverlay();
        }

        private void OnDestroy()
        {
            _busy = false;
        }

        public void FadeToSceneViaLoader(string sceneName)
        {
            if (_busy)
            {
                return;
            }

            PendingScene = sceneName;
            StartCoroutine(FadeRoutine(SceneNames.LoadingScreen));
        }

        public void FadeToScene(string sceneName)
        {
            if (_busy)
            {
                return;
            }

            StartCoroutine(FadeRoutine(sceneName));
        }

        public void FadeToScene(int buildIndex)
        {
            if (_busy)
            {
                return;
            }

            StartCoroutine(FadeRoutine(buildIndex));
        }

        public void LoadScene(string sceneName)
        {
            FadeToScene(sceneName);
        }

        public void LoadScene(int buildIndex)
        {
            FadeToScene(buildIndex);
        }

        public void ReloadCurrentScene()
        {
            FadeToScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            if (_busy)
            {
                return;
            }

            StartCoroutine(FadeAndQuitRoutine());
        }

        private IEnumerator FadeRoutine(string sceneName)
        {
            yield return StartCoroutine(FadeCore(() => SceneManager.LoadSceneAsync(sceneName)));
        }

        private IEnumerator FadeRoutine(int buildIndex)
        {
            yield return StartCoroutine(FadeCore(() => SceneManager.LoadSceneAsync(buildIndex)));
        }

        private IEnumerator FadeAndQuitRoutine()
        {
            _busy = true;
            Time.timeScale = 1f;
            yield return StartCoroutine(SetAlpha(0f, 1f, _fadeOutDuration));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            _busy = false;
        }

        private IEnumerator FadeCore(Func<AsyncOperation> loadFunc)
        {
            _busy = true;
            Time.timeScale = 1f;

            yield return StartCoroutine(SetAlpha(0f, 1f, _fadeOutDuration));

            AsyncOperation op = loadFunc();
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                yield return null;
            }

            op.allowSceneActivation = true;
            yield return null;
            yield return null;

            yield return StartCoroutine(SetAlpha(1f, 0f, _fadeInDuration));
            _busy = false;
        }

        private IEnumerator SetAlpha(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetOverlayAlpha(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetOverlayAlpha(to);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (_overlay == null)
            {
                return;
            }

            Color color = _overlay.color;
            color.a = alpha;
            _overlay.color = color;
        }

        private void BuildOverlay()
        {
            GameObject canvasGO = new GameObject("SceneFader_Canvas");
            canvasGO.transform.SetParent(transform, false);

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject imgGO = new GameObject("SceneFader_Overlay");
            imgGO.transform.SetParent(canvasGO.transform, false);
            _overlay = imgGO.AddComponent<Image>();
            _overlay.color = new Color(0f, 0f, 0f, 0f);
            _overlay.raycastTarget = false;

            RectTransform rt = imgGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
