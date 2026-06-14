using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringRouteDescriptor
    {
        private PyralisAuthoringRouteDescriptor(PyralisSetupRouteAnalysis analysis)
        {
            Analysis = analysis ?? PyralisSetupRouteAnalysis.Build((Object)null);
        }

        public PyralisSetupRouteAnalysis Analysis { get; }
        public SessionDefinition Session => Analysis.Session;
        public GameModeDefinition Mode => Analysis.Mode;
        public RuntimeCapabilityFamily[] CapabilityFamilies => Analysis.CapabilityFamilies ?? System.Array.Empty<RuntimeCapabilityFamily>();
        public PyralisAuthoringRouteFact[] RouteFacts => Analysis.RouteFacts ?? System.Array.Empty<PyralisAuthoringRouteFact>();
        public PyralisAuthoringRouteFact PrimaryRouteFact => Analysis.PrimaryRouteFact;
        public bool HasSelectedCapabilities => Analysis.HasSelectedCapabilities;
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

        public bool UsesWorld => HasSelectedCapabilities && (Analysis.UsesPlayfield() || Analysis.UsesPawnGameplay() || Analysis.UsesProjectileCombat());
        public bool UsesCamera => HasSelectedCapabilities && Analysis.UsesCamera();
        public bool UsesUi => HasSelectedCapabilities && (Analysis.UsesActionSelection() || Analysis.UsesScoring() || Analysis.UsesTabletopContract() || Analysis.UsesPawnGameplay());
        public bool UsesScoring => HasSelectedCapabilities && Analysis.UsesScoring();
        public bool UsesActionOrTabletop => HasSelectedCapabilities && (Analysis.UsesActionSelection() || Analysis.UsesTabletopContract());
        public bool UsesHazardsOrPickups => HasSelectedCapabilities && (Analysis.UsesPawnGameplay() || Analysis.UsesProjectileCombat() || Analysis.UsesScoring());
        public bool LikelyUsesInputManager => HasSelectedCapabilities && Analysis.LikelyUsesInputManager();

        public static PyralisAuthoringRouteDescriptor Build(Object activeSetup)
        {
            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(activeSetup, bootstrap);
            GameModeDefinition mode = PyralisAuthoringSetupContextResolver.GetSelectedMode(activeSetup, session);
            return Build(PyralisSetupRouteAnalysis.Build(mode, session));
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

            return false;
        }
    }
}
