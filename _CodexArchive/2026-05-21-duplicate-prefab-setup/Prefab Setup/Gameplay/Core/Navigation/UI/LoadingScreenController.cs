using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Core.Navigation
{
    /// <summary>
    /// Drives the optional LoadingScreen intermediate scene.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _label;

        private void Start()
        {
            string target = SceneFader.PendingScene;
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogWarning("[LoadingScreen] No pending scene set - falling back to MainMenu.");
                target = SceneNames.MainMenu;
            }

            StartCoroutine(LoadRoutine(target));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            if (_progressBar != null)
            {
                _progressBar.minValue = 0f;
                _progressBar.maxValue = 1f;
                _progressBar.value = 0f;
            }

            SetLabel("Loading...");

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                if (_progressBar != null)
                {
                    _progressBar.value = progress;
                }

                SetLabel($"Loading... {(int)(progress * 100f)}%");
                yield return null;
            }

            if (_progressBar != null)
            {
                _progressBar.value = 1f;
            }

            SetLabel("Ready!");
            yield return new WaitForSeconds(0.2f);
            op.allowSceneActivation = true;
        }

        private void SetLabel(string text)
        {
            if (_label != null)
            {
                _label.text = text;
            }
        }
    }
}
