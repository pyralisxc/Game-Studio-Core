using System;
using System.Collections;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Core.Runtime
{
    /// <summary>
    /// Singleton that handles all scene transitions with a fade.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Handles all scene transitions with a fade by creating its own fade canvas at runtime.",
        Axioms = AuthoringWorldAxiom.None,
        RequiredInterfaces = new[] { typeof(ISceneNavigator) },
        NativeSetup = new[] 
        { 
            "Add to a Bootstrap child GameObject or assign to GameplaySessionBootstrap.",
            "Configure Fade Duration.",
            "Prefer one navigation owner per menu flow: SceneLoader or SceneFader."
        },
        AssignmentFields = new[] { nameof(fadeDuration) },
        FirstProof = "Transitioning between scenes triggers a smooth fade out and fade in.",
        ExpertAdvice = "Inject ISceneNavigator to trigger transitions. Keep Fade Duration non-negative; zero gives an instant cut with the generated fade canvas.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/navigation"
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
