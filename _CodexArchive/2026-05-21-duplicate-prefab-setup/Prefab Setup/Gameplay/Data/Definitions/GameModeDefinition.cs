using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Data-authored game mode composition and session rules.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Definitions/Game Mode Definition", fileName = "GameModeDefinition")]
    public class GameModeDefinition : ScriptableObject
    {
        [Header("Scenes")]
        public string mainMenuScene = "MainMenu";
        public string gameplayScene = "Opening";

        [Header("Profiles")]
        public GameSetupProfile setupProfile;
        public PlayfieldProfile playfieldProfile;
        public CameraRigProfile cameraRigProfile;

        [Header("Systems")]
        public FeatureModuleDefinition[] requiredFeatureModules;
        public bool enableCombat = true;
        public bool enablePickups = false;
        public bool enableHazards = false;
        public bool enableScore = false;
        public bool enableRespawn = true;

        [Header("Rules")]
        public float respawnDelay = 3f;
        public int startingLives = 0;
        public int maxParticipantsOverride = 0;

        public void Sanitize()
        {
            respawnDelay = Mathf.Max(0f, respawnDelay);
            startingLives = Mathf.Max(0, startingLives);
            maxParticipantsOverride = Mathf.Max(0, maxParticipantsOverride);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (playfieldProfile == null)
                issues.Add("Assign a Playfield Profile so movement-space rules stay separate from camera behavior.");
            if (cameraRigProfile == null)
                issues.Add("Assign a Camera Rig Profile for shared or split participant presentation.");
            if (!enableRespawn && startingLives > 0)
                issues.Add("Starting lives are only meaningful when respawn is enabled.");

            if (setupProfile != null)
            {
                List<string> setupIssues = setupProfile.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < setupIssues.Count; issueIndex++)
                    issues.Add($"Setup profile `{setupProfile.setupName}`: {setupIssues[issueIndex]}");
            }

            HashSet<string> moduleIds = new HashSet<string>();
            if (requiredFeatureModules == null)
                return issues;

            for (int i = 0; i < requiredFeatureModules.Length; i++)
            {
                FeatureModuleDefinition module = requiredFeatureModules[i];
                if (module == null)
                {
                    issues.Add($"Required Feature Modules[{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                    issues.Add($"Required feature module `{module.moduleId}` is assigned more than once.");

                List<string> moduleIssues = module.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                    issues.Add($"Required feature `{module.moduleId}`: {moduleIssues[issueIndex]}");
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
