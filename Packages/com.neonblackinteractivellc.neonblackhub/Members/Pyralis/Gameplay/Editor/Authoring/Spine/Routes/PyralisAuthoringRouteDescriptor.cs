using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringRouteDescriptor
    {
        private PyralisAuthoringRouteDescriptor(PyralisSetupRouteAnalysis analysis)
        {
            Analysis = analysis ?? PyralisSetupRouteAnalysis.Build((GameSetupProfile)null);
        }

        public PyralisSetupRouteAnalysis Analysis { get; }
        public SessionDefinition Session => Analysis.Session;
        public GameModeDefinition Mode => Analysis.Mode;
        public GameSetupProfile SetupProfile => Analysis.SetupProfile;
        public RuntimePatternDefinition[] Patterns => Analysis.Patterns ?? System.Array.Empty<RuntimePatternDefinition>();
        public RuntimeCapabilityFamily[] CapabilityFamilies => Analysis.CapabilityFamilies ?? System.Array.Empty<RuntimeCapabilityFamily>();
        public string[] RequiredRuntimeSystems => Analysis.RequiredRuntimeSystems ?? System.Array.Empty<string>();
        public PyralisAuthoringRouteFact[] RouteFacts => Analysis.RouteFacts ?? System.Array.Empty<PyralisAuthoringRouteFact>();
        public PyralisAuthoringRouteFact PrimaryRouteFact => Analysis.PrimaryRouteFact;
        public bool HasAssignedPatterns => Analysis.HasAssignedPatterns;
        public bool HasValidPatterns => Analysis.HasValidPatterns;
        public bool RequiresPawn => Analysis.RequiresPawn;
        public bool HasParticipants => Analysis.HasParticipants;
        public bool HasAnyDefaultPawn => Analysis.HasAnyDefaultPawn;
        public string ParticipantPawnIssue => Analysis.ParticipantPawnIssue;
        public PyralisParticipantPawnIssueKind ParticipantPawnIssueKind => Analysis.ParticipantPawnIssueKind;
        public string RouteName => Analysis.RouteName;

        public bool HasPawn => HasFamily(RuntimeCapabilityFamily.CharacterPawnGameplay);
        public bool HasCombat => HasFamily(RuntimeCapabilityFamily.Combat);
        public bool HasProjectiles => HasFamily(RuntimeCapabilityFamily.GunsProjectiles);
        public bool HasActions => HasFamily(RuntimeCapabilityFamily.ActionTargeting);
        public bool HasTabletop => HasFamily(RuntimeCapabilityFamily.BoardCardTabletop);
        public bool HasCamera => HasFamily(RuntimeCapabilityFamily.CameraInput);
        public bool HasAnimation => HasFamily(RuntimeCapabilityFamily.AnimationPresentation);
        public bool HasScoring => HasFamily(RuntimeCapabilityFamily.ScoringObjectives);
        public bool HasProcedural => HasFamily(RuntimeCapabilityFamily.ProceduralGeneration);
        public bool HasNetworking => HasFamily(RuntimeCapabilityFamily.Networking);
        public bool HasPlatformCore => HasFamily(RuntimeCapabilityFamily.PlatformCore);
        public bool HasSelectedCapabilities => CapabilityFamilies.Length > 0 || HasValidPatterns;

        public bool UsesWorld => HasSelectedCapabilities && (Analysis.UsesPlayfield() || Analysis.UsesPawnGameplay() || Analysis.UsesProjectileCombat());
        public bool UsesCamera => HasSelectedCapabilities && Analysis.UsesCamera();
        public bool UsesUi => HasSelectedCapabilities && (Analysis.UsesActionSelection() || Analysis.UsesScoring() || Analysis.UsesTabletopContract() || Analysis.UsesPawnGameplay());
        public bool UsesScoring => HasSelectedCapabilities && Analysis.UsesScoring();
        public bool UsesActionOrTabletop => HasSelectedCapabilities && (Analysis.UsesActionSelection() || Analysis.UsesTabletopContract());
        public bool UsesHazardsOrPickups => HasSelectedCapabilities && (Analysis.UsesPawnGameplay() || Analysis.UsesProjectileCombat() || Analysis.UsesScoring());
        public bool LikelyUsesInputManager => HasSelectedCapabilities && Analysis.LikelyUsesInputManager();

        public static PyralisAuthoringRouteDescriptor Build(Object activeSetup)
        {
            GameplaySessionBootstrap bootstrap = PyralisAuthoringWindow.GetSelectedBootstrap(activeSetup);
            SessionDefinition session = PyralisAuthoringWindow.GetSelectedSession(activeSetup, bootstrap);
            GameModeDefinition mode = PyralisAuthoringWindow.GetSelectedMode(activeSetup, session);
            GameSetupProfile setupProfile = PyralisAuthoringWindow.GetSelectedSetupProfile(activeSetup, mode);
            return Build(setupProfile, session, mode);
        }

        public static PyralisAuthoringRouteDescriptor Build(GameSetupProfile setupProfile, SessionDefinition session = null, GameModeDefinition mode = null)
        {
            return Build(PyralisSetupRouteAnalysis.Build(setupProfile, session, mode));
        }

        public static PyralisAuthoringRouteDescriptor Build(PyralisSetupRouteAnalysis analysis)
        {
            return new PyralisAuthoringRouteDescriptor(analysis);
        }

        public bool HasFamily(RuntimeCapabilityFamily family)
        {
            for (int i = 0; i < CapabilityFamilies.Length; i++)
            {
                if (CapabilityFamilies[i] == family)
                    return true;
            }

            for (int i = 0; i < Patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = Patterns[i];
                if (IsValidPattern(pattern) && pattern.capabilityFamily == family)
                    return true;
            }

            return false;
        }

        public bool SupportsSurface(RuntimeControlSurface surface)
        {
            for (int i = 0; i < Patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = Patterns[i];
                if (IsValidPattern(pattern) && pattern.SupportsControlSurface(surface))
                    return true;
            }

            return false;
        }

        public bool RequiresRuntimeSystem(string token)
        {
            return Analysis.RequiresRuntimeSystem(token);
        }

        private static bool IsValidPattern(RuntimePatternDefinition pattern)
        {
            return pattern != null && pattern.GetValidationIssues().Count == 0;
        }
    }
}
