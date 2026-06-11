using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Game-specific wiring point for scenes and key service prefabs.
    /// Participant and pawn authoring now lives in <see cref="SessionDefinition"/>
    /// and <see cref="PawnDefinition"/>; prefer those for all new game setup.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "The master wiring point for the game project; defines the entry session and service prefabs.",
        NativeSetup = new[] { "Create Asset.", "Assign Session Definition.", "Set Scene names." },
        AssignmentFields = new[] { nameof(sessionDefinition), nameof(mainMenuScene) },
        FirstProof = "Verify the game boots into the specified main menu scene.",
        ExpertAdvice = "Use service prefabs only if you need custom logic for core services like Time or Scene Loading."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Core/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (sessionDefinition == null) yield return "Session Definition is missing.";
            if (string.IsNullOrWhiteSpace(mainMenuScene)) yield return "Main Menu Scene name is required.";
        }

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
