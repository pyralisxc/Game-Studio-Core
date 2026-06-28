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
    /// Scene navigation service that handles transitions with a generated fade canvas.
    /// </summary>
    [AuthoringContract(
        Category = "Setup",
        CapabilityPath = "Core Setup/Navigation/Scene Loader",
        Surface = AuthoringSurface.Service,
        Summary = "Simple ISceneNavigator implementation that fades with a generated runtime canvas. Use SceneFader for menu/game-shell flows that need loading-screen routing.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/navigation",
        RequiredFields = new[] { nameof(fadeDuration) },
        RequiredInterfaces = new[] { typeof(ISceneNavigator) },
        SetupSteps = new[] 
        { 
            "Add to a Bootstrap child GameObject or assign to GameplaySessionBootstrap.",
            "Configure Fade Duration.",
            "Prefer one navigation owner per menu flow. SceneFader is the current game-shell route; SceneLoader remains a lightweight generated-canvas fallback."
        },
        SuccessChecks = new[] { "Transitioning between scenes triggers a smooth fade out and fade in." },
        Tags = new[] { "capability:Setup" },
        Selectable = false
    )]
    public class SceneLoader : MonoBehaviour, ISceneNavigator
    {
        [Header("Fade")]
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup _fadeCanvas;

        private void Awake()
        {
            BuildFadeCanvas();
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }

        public void LoadScene(int buildIndex)
        {
            StartCoroutine(FadeAndLoad(buildIndex));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            StartCoroutine(FadeAndQuit());
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            yield return FadeOut();
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return FadeIn();
        }

        private IEnumerator FadeAndLoad(int buildIndex)
        {
            yield return FadeOut();
            yield return SceneManager.LoadSceneAsync(buildIndex);
            yield return FadeIn();
        }

        private IEnumerator FadeAndQuit()
        {
            yield return FadeOut();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator FadeOut()
        {
            _fadeCanvas.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                _fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            _fadeCanvas.alpha = 1f;
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                _fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            _fadeCanvas.alpha = 0f;
            _fadeCanvas.blocksRaycasts = false;
        }

        private void BuildFadeCanvas()
        {
            GameObject canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform);

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            _fadeCanvas = canvasGO.AddComponent<CanvasGroup>();
            _fadeCanvas.alpha = 0f;
            _fadeCanvas.blocksRaycasts = false;
            _fadeCanvas.interactable = false;

            GameObject imgGO = new GameObject("FadeImage");
            imgGO.transform.SetParent(canvasGO.transform, false);

            Image img = imgGO.AddComponent<Image>();
            img.color = Color.black;

            RectTransform rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
