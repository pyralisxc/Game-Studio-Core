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
    public class SceneLoader : MonoBehaviour, ISceneNavigator, IGameService
    {
        public static SceneLoader Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        [Header("Fade")]
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup _fadeCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            Initialize();
            BuildFadeCanvas();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Shutdown();
                Instance = null;
            }
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

        public void Initialize()
        {
        }

        public void Shutdown()
        {
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
