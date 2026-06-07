using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Game-specific wiring point for scenes and key service prefabs.
    /// Participant and pawn authoring now lives in <see cref="SessionDefinition"/>
    /// and <see cref="PawnDefinition"/>; prefer those for all new game setup.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Core/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Session")]
        [Tooltip("Preferred entry point for new projects. Drives participants, mode, camera, and settings.")]
        public SessionDefinition sessionDefinition;
        [Header("Scenes")]
        public string mainMenuScene = "MainMenu";
        public string gameplayScene = "";

        [Header("Service Prefabs")]
        [Tooltip("Optional prefab to use for SceneLoader service creation.")]
        public GameObject sceneLoaderPrefab;
        [Tooltip("Optional prefab to use for TimeManager service creation.")]
        public GameObject timeManagerPrefab;
        [Tooltip("Optional prefab to use for CameraShake service creation.")]
        public GameObject cameraShakePrefab;

    }
}
