using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringValidationCategory
    {
        SessionSetup,
        GameRules,
        SetupProfile,
        PlayersSeats,
        PawnsActors,
        SceneObjects,
        CodeContract,
        Other
    }

    public sealed class PyralisAuthoringValidationIssue
    {
        public PyralisAuthoringValidationIssue(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string problem,
            string whyItMatters,
            string inspectionHint,
            string affectedMember,
            Object target,
            string primaryActionLabel,
            string guidanceActionLabel,
            string expected = "",
            string found = "",
            string successLooksLike = "",
            PyralisAuthoringIssue typedIssue = null)
        {
            IssueCode = issueCode;
            Category = category;
            Problem = problem;
            WhyItMatters = whyItMatters;
            InspectionHint = inspectionHint;
            AffectedMember = affectedMember;
            Target = target;
            PrimaryActionLabel = primaryActionLabel;
            GuidanceActionLabel = guidanceActionLabel;
            Expected = expected;
            Found = found;
            SuccessLooksLike = successLooksLike;
            TypedIssue = typedIssue ?? PyralisAuthoringIssueAdapter.Create(
                issueCode,
                category,
                problem,
                affectedMember,
                primaryActionLabel,
                expected,
                found,
                successLooksLike);
        }

        public string IssueCode { get; }
        public PyralisAuthoringValidationCategory Category { get; }
        public string Problem { get; }
        public string WhyItMatters { get; }
        public string InspectionHint { get; }
        public string AffectedMember { get; }
        public Object Target { get; }
        public string PrimaryActionLabel { get; }
        public string GuidanceActionLabel { get; }
        public string Expected { get; }
        public string Found { get; }
        public string SuccessLooksLike { get; }
        public PyralisAuthoringIssue TypedIssue { get; }
        public bool CanInspectTarget => Target != null;
        public bool HasGuidanceAction => !string.IsNullOrWhiteSpace(GuidanceActionLabel);
        public bool HasAuditEvidence => !string.IsNullOrWhiteSpace(Expected)
            || !string.IsNullOrWhiteSpace(Found)
            || !string.IsNullOrWhiteSpace(SuccessLooksLike);
    }

    public sealed class PyralisAuthoringValidationModel
    {
        private PyralisAuthoringValidationModel(string routeName, string nextStep, List<PyralisAuthoringValidationIssue> issues)
        {
            RouteName = routeName;
            NextStep = nextStep;
            Issues = issues ?? new List<PyralisAuthoringValidationIssue>();
        }

        public string RouteName { get; }
        public string NextStep { get; }
        public IReadOnlyList<PyralisAuthoringValidationIssue> Issues { get; }
        public IReadOnlyList<PyralisAuthoringIssue> TypedIssues
        {
            get
            {
                List<PyralisAuthoringIssue> typedIssues = new List<PyralisAuthoringIssue>();
                for (int i = 0; i < Issues.Count; i++)
                {
                    if (Issues[i]?.TypedIssue != null)
                        typedIssues.Add(Issues[i].TypedIssue);
                }

                return typedIssues;
            }
        }

        public bool HasIssues => Issues.Count > 0;

        public static PyralisAuthoringValidationModel Build(PyralisAuthoringRouteReport report)
        {
            return Build(null, report);
        }

        public static PyralisAuthoringValidationModel Build(Object target)
        {
            return Build(target, PyralisAuthoringRouteReport.Build(target));
        }

        public static PyralisAuthoringValidationModel Build(Object target, PyralisAuthoringRouteReport report)
        {
            if (report == null)
                return new PyralisAuthoringValidationModel("No selection", "Select a Pyralis setup item.", new List<PyralisAuthoringValidationIssue>());

            List<PyralisAuthoringValidationIssue> structuredIssues = BuildStructuredIssues(target);
            if (structuredIssues.Count > 0)
                return new PyralisAuthoringValidationModel(report.RouteName, report.NextStep, structuredIssues);

            var issues = new List<PyralisAuthoringValidationIssue>();
            for (int i = 0; i < report.ValidationIssues.Count; i++)
            {
                string issue = report.ValidationIssues[i];
                if (string.IsNullOrWhiteSpace(issue))
                    continue;

                PyralisAuthoringValidationCategory category = GetCategory(issue);
                issues.Add(new PyralisAuthoringValidationIssue(
                    "route.keyword",
                    category,
                    issue,
                    GetWhyItMatters(category),
                    GetInspectionHint(category),
                    GetAffectedMember(issue, category),
                    target,
                    GetPrimaryActionLabel(category, target),
                    string.Empty));
            }

            return new PyralisAuthoringValidationModel(report.RouteName, report.NextStep, issues);
        }

        private static List<PyralisAuthoringValidationIssue> BuildStructuredIssues(Object target)
        {
            return target switch
            {
                GameplaySessionBootstrap bootstrap => BuildBootstrapIssues(bootstrap),
                SessionDefinition session => BuildSessionIssues(session),
                GameModeDefinition mode => BuildGameModeIssues(mode),
                GameSetupProfile setup => BuildSetupProfileIssues(setup),
                PawnDefinition pawn => BuildPawnIssues(pawn),
                _ => new List<PyralisAuthoringValidationIssue>()
            };
        }

        private static List<PyralisAuthoringValidationIssue> BuildBootstrapIssues(GameplaySessionBootstrap bootstrap)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (bootstrap == null)
                return issues;

            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(bootstrap, bootstrap);
            if (session == null)
            {
                issues.Add(CreateIssue(
                    "bootstrap.sessionDefinition.missing",
                    PyralisAuthoringValidationCategory.SessionSetup,
                    "Session Definition is not assigned. Create or assign the SessionDefinition this scene should start.",
                    "GameplaySessionBootstrap.sessionDefinition",
                    bootstrap));
            }
            else
            {
                issues.AddRange(BuildSessionIssues(session));
            }

            issues.AddRange(BuildPrefabReadinessIssues(bootstrap));
            issues.AddRange(BuildSceneSurfaceIssues(bootstrap));
            return issues;
        }

        private static List<PyralisAuthoringValidationIssue> BuildPrefabReadinessIssues(GameplaySessionBootstrap bootstrap)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (bootstrap == null)
                return issues;

            PyralisSceneReadinessReport readiness = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            AppendPrefabReadinessIssues(readiness.RequiredIssues, true, bootstrap, issues);
            AppendPrefabReadinessIssues(readiness.RecommendedIssues, false, bootstrap, issues);
            return issues;
        }

        private static void AppendPrefabReadinessIssues(
            IReadOnlyList<string> readinessIssues,
            bool required,
            GameplaySessionBootstrap bootstrap,
            List<PyralisAuthoringValidationIssue> output)
        {
            if (readinessIssues == null)
                return;

            int issueIndex = 0;
            for (int i = 0; i < readinessIssues.Count; i++)
            {
                string issue = readinessIssues[i];
                if (!ShouldSurfacePrefabReadinessIssue(issue))
                    continue;

                output.Add(CreatePrefabReadinessIssue(issue, required, issueIndex, bootstrap));
                issueIndex++;
            }
        }

        private static bool ShouldSurfacePrefabReadinessIssue(string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                return false;

            string lower = issue.ToLowerInvariant();
            return lower.Contains("prefab")
                || lower.Contains("missing script")
                || lower.Contains("pawnroot")
                || lower.Contains("ipawnmotor")
                || lower.Contains("inputprofile")
                || lower.Contains("input action")
                || lower.Contains("input module")
                || lower.Contains("ipawninputmodule")
                || lower.Contains("move action")
                || lower.Contains("ipawnpresentationmodule")
                || lower.Contains("iprojectileruntimebody")
                || lower.Contains("networkobject");
        }

        private static List<PyralisAuthoringValidationIssue> BuildSceneSurfaceIssues(GameplaySessionBootstrap bootstrap)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row == null || !row.Recommended || row.Present)
                    continue;

                issues.Add(CreateSceneSurfaceIssue(row, bootstrap));
            }

            return issues;
        }

        private static List<PyralisAuthoringValidationIssue> BuildSessionIssues(SessionDefinition session)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (session == null)
                return issues;

            if (string.IsNullOrWhiteSpace(session.sessionName))
                issues.Add(CreateIssue("session.name.required", PyralisAuthoringValidationCategory.SessionSetup, "Session name is required.", "SessionDefinition.sessionName", session));

            if (session.maxParticipants < 1)
                issues.Add(CreateIssue("session.maxParticipants.minimum", PyralisAuthoringValidationCategory.PlayersSeats, "Max participants must be at least 1.", "SessionDefinition.maxParticipants", session));

            if (session.defaultGameMode == null)
            {
                issues.Add(CreateIssue("session.defaultGameMode.missing", PyralisAuthoringValidationCategory.SessionSetup, "Default game mode is not assigned.", "SessionDefinition.defaultGameMode", session));
            }
            else
            {
                issues.AddRange(BuildGameModeIssues(session.defaultGameMode));
            }

            if (session.networkMode != GameplayNetworkMode.LocalOnly && session.localFirst)
                issues.Add(CreateIssue("session.network.localFirst.conflict", PyralisAuthoringValidationCategory.SessionSetup, "Networked sessions should set Local First to false so setup tooling treats NGO as the authority path.", "SessionDefinition.localFirst", session));

            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                issues.Add(CreateIssue("session.defaultParticipants.missing", PyralisAuthoringValidationCategory.PlayersSeats, "At least one default participant should be assigned.", "SessionDefinition.defaultParticipants", session));
            }
            else
            {
                for (int i = 0; i < session.defaultParticipants.Length; i++)
                {
                    if (session.defaultParticipants[i] == null)
                        issues.Add(CreateIssue("session.defaultParticipants.slot.empty", PyralisAuthoringValidationCategory.PlayersSeats, $"Default participant slot {i} is empty.", $"SessionDefinition.defaultParticipants[{i}]", session));
                }
            }

            return issues;
        }

        private static List<PyralisAuthoringValidationIssue> BuildGameModeIssues(GameModeDefinition mode)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (mode == null)
                return issues;

            if (!mode.enableRespawn && mode.startingLives > 0)
                issues.Add(CreateIssue("gameMode.startingLives.respawnDisabled", PyralisAuthoringValidationCategory.GameRules, "Starting lives are only meaningful when respawn is enabled.", "GameModeDefinition.startingLives", mode));

            if (mode.setupProfile == null)
            {
                issues.Add(CreateIssue("gameMode.setupProfile.missing", PyralisAuthoringValidationCategory.SetupProfile, "Setup profile is not assigned.", "GameModeDefinition.setupProfile", mode));
            }
            else
            {
                issues.AddRange(BuildSetupProfileIssues(mode.setupProfile));
            }

            if (mode.requiredFeatureModules != null)
            {
                var moduleIds = new HashSet<string>();
                for (int i = 0; i < mode.requiredFeatureModules.Length; i++)
                {
                    FeatureModuleDefinition module = mode.requiredFeatureModules[i];
                    if (module == null)
                    {
                        issues.Add(CreateIssue("gameMode.requiredFeatureModules.slot.empty", PyralisAuthoringValidationCategory.GameRules, $"Required Feature Modules[{i}] is null.", $"GameModeDefinition.requiredFeatureModules[{i}]", mode));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                        issues.Add(CreateIssue("gameMode.requiredFeatureModules.duplicate", PyralisAuthoringValidationCategory.GameRules, $"Required feature module `{module.moduleId}` is assigned more than once.", "GameModeDefinition.requiredFeatureModules", mode));
                }
            }

            return issues;
        }

        private static List<PyralisAuthoringValidationIssue> BuildSetupProfileIssues(GameSetupProfile setup)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (setup == null)
                return issues;

            if (string.IsNullOrWhiteSpace(setup.setupName))
                issues.Add(CreateIssue("setupProfile.name.required", PyralisAuthoringValidationCategory.SetupProfile, "Setup name is required.", "GameSetupProfile.setupName", setup));

            if ((setup.runtimeCapabilities == null || setup.runtimeCapabilities.Length == 0)
                && (setup.runtimePatterns == null || setup.runtimePatterns.Length == 0))
            {
                issues.Add(CreateIssue("setupProfile.runtimeCapabilities.missing", PyralisAuthoringValidationCategory.SetupProfile, "At least one runtime capability family should be selected.", "GameSetupProfile.runtimeCapabilities", setup));
                return issues;
            }

            var capabilityFamilies = new HashSet<RuntimeCapabilityFamily>();
            if (setup.runtimeCapabilities != null)
            {
                for (int i = 0; i < setup.runtimeCapabilities.Length; i++)
                {
                    RuntimeCapabilitySelection selection = setup.runtimeCapabilities[i];
                    if (selection == null)
                    {
                        issues.Add(CreateIssue("setupProfile.runtimeCapabilities.slot.empty", PyralisAuthoringValidationCategory.SetupProfile, $"Runtime Capabilities[{i}] is empty.", $"GameSetupProfile.runtimeCapabilities[{i}]", setup));
                        continue;
                    }

                    if (!capabilityFamilies.Add(selection.capabilityFamily))
                        issues.Add(CreateIssue("setupProfile.runtimeCapabilities.duplicate", PyralisAuthoringValidationCategory.SetupProfile, $"Runtime capability `{selection.capabilityFamily}` is selected more than once.", "GameSetupProfile.runtimeCapabilities", setup));

                    if (selection.patternDefinition != null && selection.patternDefinition.capabilityFamily != selection.capabilityFamily)
                        issues.Add(CreateIssue("setupProfile.runtimeCapabilities.patternFamilyMismatch", PyralisAuthoringValidationCategory.SetupProfile, $"Runtime capability `{selection.capabilityFamily}` references pattern `{selection.patternDefinition.name}` with family `{selection.patternDefinition.capabilityFamily}`.", $"GameSetupProfile.runtimeCapabilities[{i}]", setup));
                }
            }

            if (setup.runtimePatterns == null || setup.runtimePatterns.Length == 0)
                return issues;

            var patternIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < setup.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setup.runtimePatterns[i];
                if (pattern == null)
                {
                    issues.Add(CreateIssue("setupProfile.runtimePatterns.slot.empty", PyralisAuthoringValidationCategory.SetupProfile, $"Optional Runtime Patterns[{i}] is null.", $"GameSetupProfile.runtimePatterns[{i}]", setup));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(pattern.patternId) && !patternIds.Add(pattern.patternId))
                    issues.Add(CreateIssue("setupProfile.runtimePatterns.duplicate", PyralisAuthoringValidationCategory.SetupProfile, $"Runtime pattern `{pattern.patternId}` is assigned more than once.", "GameSetupProfile.runtimePatterns", setup));
            }

            return issues;
        }

        private static List<PyralisAuthoringValidationIssue> BuildPawnIssues(PawnDefinition pawn)
        {
            var issues = new List<PyralisAuthoringValidationIssue>();
            if (pawn == null)
                return issues;

            if (pawn.pawnPrefab == null)
                issues.Add(CreateIssue("pawn.pawnPrefab.missing", PyralisAuthoringValidationCategory.PawnsActors, "Assign a pawn prefab. PawnDefinition is the primary authored unit for runtime-controlled entities.", "PawnDefinition.pawnPrefab", pawn));
            else
                AppendPawnPrefabReadinessIssues(pawn, issues);

            if (pawn.featureModules != null)
            {
                var moduleIds = new HashSet<string>();
                for (int i = 0; i < pawn.featureModules.Length; i++)
                {
                    FeatureModuleDefinition module = pawn.featureModules[i];
                    if (module == null)
                    {
                        issues.Add(CreateIssue("pawn.featureModules.slot.empty", PyralisAuthoringValidationCategory.PawnsActors, $"Feature Modules[{i}] is null.", $"PawnDefinition.featureModules[{i}]", pawn));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                        issues.Add(CreateIssue("pawn.featureModules.duplicate", PyralisAuthoringValidationCategory.PawnsActors, $"Feature module `{module.moduleId}` is assigned more than once.", "PawnDefinition.featureModules", pawn));
                }
            }

            return issues;
        }

        private static void AppendPawnPrefabReadinessIssues(PawnDefinition pawn, List<PyralisAuthoringValidationIssue> issues)
        {
            AppendPawnPrefabReadinessIssue(
                pawn,
                issues,
                "pawn.prefab.rigidbody2D.gravity",
                GetPawnPrefabGravityIssue(pawn),
                "PawnDefinition.pawnPrefab Rigidbody2D.gravityScale");

            AppendPawnPrefabReadinessIssue(
                pawn,
                issues,
                "pawn.prefab.rigidbody2D.rotation",
                GetPawnPrefabRotationIssue(pawn),
                "PawnDefinition.pawnPrefab Rigidbody2D.constraints");

            AppendPawnPrefabReadinessIssue(
                pawn,
                issues,
                "pawn.prefab.sprite.environmentSized",
                GetPawnPrefabSpriteScaleIssue(pawn),
                "PawnDefinition.pawnPrefab SpriteRenderer.sprite");

            AppendPawnPrefabReadinessIssue(
                pawn,
                issues,
                "pawn.prefab.input.duplicate2DHandler",
                GetPawnPrefabInputAdapterIssue(pawn),
                "PawnDefinition.pawnPrefab input components");
        }

        private static void AppendPawnPrefabReadinessIssue(
            PawnDefinition pawn,
            List<PyralisAuthoringValidationIssue> issues,
            string issueCode,
            string problem,
            string affectedMember)
        {
            if (string.IsNullOrWhiteSpace(problem))
                return;

            issues.Add(CreateIssue(issueCode, PyralisAuthoringValidationCategory.PawnsActors, problem, affectedMember, pawn));
        }

        internal static List<string> BuildPawnRouteValidationIssues(PawnDefinition pawn)
        {
            List<string> issues = pawn.GetValidationIssues();
            AddIfPresent(issues, GetPawnPrefabGravityIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabRotationIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabSpriteScaleIssue(pawn));
            AddIfPresent(issues, GetPawnPrefabInputAdapterIssue(pawn));
            return issues;
        }

        private static string GetPawnPrefabGravityIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out _, out Rigidbody2D body, out _))
                return null;

            return body != null && Mathf.Abs(body.gravityScale) > 0.001f
                ? "Rigidbody2D gravity is non-zero on a Pawn2DMovementComponent prefab. Set Rigidbody2D > Gravity Scale to 0 for this native 2D pawn movement stack."
                : null;
        }

        private static string GetPawnPrefabRotationIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out _, out Rigidbody2D body, out _))
                return null;

            return body != null && (body.constraints & RigidbodyConstraints2D.FreezeRotation) == 0
                ? "Rigidbody2D rotation is not frozen on a Pawn2DMovementComponent prefab. Set Rigidbody2D > Constraints > Freeze Rotation so collision nudges do not spin the pawn."
                : null;
        }

        private static string GetPawnPrefabSpriteScaleIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out _, out Pawn2DMovementComponent movement2D, out _, out SpriteRenderer spriteRenderer))
                return null;

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return null;

            float largestVisualExtent = Mathf.Max(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
            float spriteRadius = GetSerializedFloat(movement2D, "spriteRadius", 0.32f);
            float expectedPawnExtent = Mathf.Max(6f, spriteRadius * 8f);
            return largestVisualExtent > expectedPawnExtent
                ? "The pawn prefab SpriteRenderer uses an environment-sized sprite. Drag a character sprite from Project onto SpriteRenderer > Sprite, and keep floor/map art on separate scene objects."
                : null;
        }

        private static string GetPawnPrefabInputAdapterIssue(PawnDefinition pawn)
        {
            if (!TryGet2DPawnPrefabParts(pawn, out GameObject prefab, out _, out _, out _))
                return null;

            Motor2DInputAdapter inputAdapter = prefab.GetComponent<Motor2DInputAdapter>();
            PlayerInputHandler[] inputHandlers = prefab.GetComponents<PlayerInputHandler>();
            if (inputAdapter == null && inputHandlers.Length == 0)
                return "The player-owned 2D pawn prefab has no input module. Use Inspector > Add Component on the pawn prefab root and add Motor2DInputAdapter so InputProfile actions can reach movement.";

            if (inputAdapter != null && inputHandlers.Length > 1)
                return "The pawn prefab has both Motor2DInputAdapter and an extra PlayerInputHandler. Keep Motor2DInputAdapter for the supported 2D player-input bridge, and remove the duplicate 2D Player Input Handler before the movement proof.";

            return null;
        }

        private static bool TryGet2DPawnPrefabParts(
            PawnDefinition pawn,
            out GameObject prefab,
            out Pawn2DMovementComponent movement2D,
            out Rigidbody2D body,
            out SpriteRenderer spriteRenderer)
        {
            prefab = pawn != null ? pawn.pawnPrefab : null;
            movement2D = prefab != null ? prefab.GetComponent<Pawn2DMovementComponent>() : null;
            body = prefab != null ? prefab.GetComponent<Rigidbody2D>() : null;
            spriteRenderer = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>(true) : null;
            return prefab != null && movement2D != null;
        }

        private static void AddIfPresent(List<string> issues, string issue)
        {
            if (!string.IsNullOrWhiteSpace(issue))
                issues.Add(issue);
        }

        private static float GetSerializedFloat(Object target, string propertyName, float fallback)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.floatValue : fallback;
        }

        private static PyralisAuthoringValidationIssue CreateIssue(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string problem,
            string affectedMember,
            Object target)
        {
            return new PyralisAuthoringValidationIssue(
                issueCode,
                category,
                problem,
                GetWhyItMatters(category),
                GetInspectionHint(category),
                affectedMember,
                target,
                GetPrimaryActionLabel(category, target),
                GetGuidanceActionLabel(issueCode));
        }

        private static PyralisAuthoringValidationIssue CreateSceneSurfaceIssue(PyralisAuthoringSceneSurfaceRow row, GameplaySessionBootstrap bootstrap)
        {
            string issueCode = $"sceneSurface.{GetSceneSurfaceIssueToken(row.Surface)}.missing";
            return new PyralisAuthoringValidationIssue(
                issueCode,
                PyralisAuthoringValidationCategory.SceneObjects,
                $"{row.Surface} is a proof enhancer for the selected intent but was not detected.",
                $"Validate found: {row.Current}. This scene surface can make the proof easier to read, but it should not stop a narrow Play Mode attempt when the selected intent's Do Now setup is clear.",
                row.NextFix,
                $"Scene surface: {row.Surface}",
                bootstrap,
                GetPrimaryActionLabel(PyralisAuthoringValidationCategory.SceneObjects, bootstrap),
                "Open Map",
                GetSceneSurfaceExpected(row.Surface),
                row.Current,
                GetSceneSurfaceSuccess(row.Surface));
        }

        private static PyralisAuthoringValidationIssue CreatePrefabReadinessIssue(string readinessIssue, bool required, int issueIndex, GameplaySessionBootstrap bootstrap)
        {
            PyralisAuthoringValidationCategory category = GetPrefabReadinessCategory(readinessIssue);
            string severity = required ? "Required" : "Recommended";
            string issueCode = $"prefabReadiness.{(required ? "required" : "recommended")}.{GetIssueToken(readinessIssue, issueIndex)}";
            Object target = ResolvePrefabReadinessTarget(bootstrap, readinessIssue) ?? bootstrap;
            return new PyralisAuthoringValidationIssue(
                issueCode,
                category,
                $"{severity} prefab readiness issue: {readinessIssue}",
                "Validate found this through the scene and prefab readiness audit. Pyralis can point to the contract, but the developer should inspect and edit the prefab or scene object in Unity because project presets, art, physics lanes, networking, and component choices are design-owned.",
                GetPrefabReadinessInspectionHint(readinessIssue),
                GetPrefabReadinessAffectedMember(readinessIssue, category),
                target,
                GetPrimaryActionLabel(category, target),
                "Open Map",
                GetPrefabReadinessExpected(readinessIssue),
                readinessIssue,
                GetPrefabReadinessSuccess(readinessIssue));
        }

        private static Object ResolvePrefabReadinessTarget(GameplaySessionBootstrap bootstrap, string readinessIssue)
        {
            if (bootstrap == null || string.IsNullOrWhiteSpace(readinessIssue))
                return null;

            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(bootstrap, bootstrap);
            if (session == null)
                return bootstrap;

            string lower = readinessIssue.ToLowerInvariant();
            IReadOnlyList<string> tokens = ExtractBacktickTokens(readinessIssue);

            Object matchedTarget = TryResolveSceneReferenceTarget(bootstrap, tokens, lower);
            if (matchedTarget != null)
                return matchedTarget;

            if (session.defaultParticipants == null)
                return bootstrap;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                PawnDefinition pawn = participant != null ? participant.defaultPawn : null;
                if (pawn == null)
                    continue;

                matchedTarget = TryResolvePawnReadinessTarget(pawn, tokens, lower);
                if (matchedTarget != null)
                    return matchedTarget;
            }

            return bootstrap;
        }

        private static Object TryResolveSceneReferenceTarget(GameplaySessionBootstrap bootstrap, IReadOnlyList<string> tokens, string lowerIssue)
        {
            if (bootstrap == null)
                return null;

            if (TokenMatchesObject(tokens, bootstrap.gameObject) || lowerIssue.Contains("gameplay root"))
                return bootstrap.gameObject;

            return null;
        }

        private static Object TryResolvePawnReadinessTarget(PawnDefinition pawn, IReadOnlyList<string> tokens, string lowerIssue)
        {
            if (pawn == null)
                return null;

            if (pawn.pawnPrefab != null
                && (TokenMatchesObject(tokens, pawn.pawnPrefab)
                    || lowerIssue.Contains("pawn prefab") && lowerIssue.Contains(pawn.pawnPrefab.name.ToLowerInvariant())))
            {
                return pawn.pawnPrefab;
            }

            Object featureTarget = TryResolveFeatureModuleReadinessTarget(pawn, tokens, lowerIssue);
            if (featureTarget != null)
                return featureTarget;

            Object projectileTarget = TryResolveProjectileReadinessTarget(pawn.combatProfile, tokens, lowerIssue);
            if (projectileTarget != null)
                return projectileTarget;

            return null;
        }

        private static Object TryResolveFeatureModuleReadinessTarget(PawnDefinition pawn, IReadOnlyList<string> tokens, string lowerIssue)
        {
            if (pawn.featureModules == null)
                return null;

            for (int i = 0; i < pawn.featureModules.Length; i++)
            {
                FeatureModuleDefinition module = pawn.featureModules[i];
                if (module == null)
                    continue;

                if (TokenMatches(tokens, module.moduleId) || TokenMatches(tokens, module.displayName) || TokenMatchesObject(tokens, module))
                    return module;

                if (module.runtimePrefab != null
                    && (TokenMatchesObject(tokens, module.runtimePrefab)
                        || lowerIssue.Contains("runtime prefab") && lowerIssue.Contains(module.runtimePrefab.name.ToLowerInvariant())))
                {
                    return module.runtimePrefab;
                }
            }

            return null;
        }

        private static Object TryResolveProjectileReadinessTarget(PawnCombatProfile combatProfile, IReadOnlyList<string> tokens, string lowerIssue)
        {
            if (combatProfile == null || !combatProfile.enableCombat)
                return null;

            Object target = TryResolveWeaponProjectileTarget(combatProfile.attackWeapon, tokens, lowerIssue);
            if (target != null)
                return target;

            target = TryResolveWeaponProjectileTarget(combatProfile.kickWeapon, tokens, lowerIssue);
            if (target != null)
                return target;

            target = TryResolveWeaponProjectileTarget(combatProfile.aerialWeapon, tokens, lowerIssue);
            if (target != null)
                return target;

            target = TryResolveSequenceProjectileTarget(combatProfile.primarySequence, tokens, lowerIssue);
            if (target != null)
                return target;

            target = TryResolveSequenceProjectileTarget(combatProfile.secondarySequence, tokens, lowerIssue);
            if (target != null)
                return target;

            return TryResolveSequenceProjectileTarget(combatProfile.aerialSequence, tokens, lowerIssue);
        }

        private static Object TryResolveSequenceProjectileTarget(CombatSequenceDefinition sequence, IReadOnlyList<string> tokens, string lowerIssue)
        {
            if (sequence == null || sequence.actions == null)
                return null;

            for (int i = 0; i < sequence.actions.Length; i++)
            {
                CombatActionDefinition action = sequence.actions[i];
                Object target = action != null ? TryResolveWeaponProjectileTarget(action.weapon, tokens, lowerIssue) : null;
                if (target != null)
                    return target;
            }

            return null;
        }

        private static Object TryResolveWeaponProjectileTarget(WeaponData weapon, IReadOnlyList<string> tokens, string lowerIssue)
        {
            ProjectileDefinition projectile = weapon != null ? weapon.projectileDefinition : null;
            if (projectile == null)
                return null;

            if (projectile.projectilePrefab != null
                && (TokenMatchesObject(tokens, projectile.projectilePrefab)
                    || lowerIssue.Contains("projectile prefab") && lowerIssue.Contains(projectile.projectilePrefab.name.ToLowerInvariant())))
            {
                return projectile.projectilePrefab;
            }

            if (TokenMatches(tokens, projectile.displayName)
                || TokenMatches(tokens, projectile.projectileId)
                || TokenMatchesObject(tokens, projectile))
            {
                return projectile;
            }

            return null;
        }

        private static IReadOnlyList<string> ExtractBacktickTokens(string text)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return tokens;

            int searchIndex = 0;
            while (searchIndex < text.Length)
            {
                int start = text.IndexOf('`', searchIndex);
                if (start < 0)
                    break;

                int end = text.IndexOf('`', start + 1);
                if (end < 0)
                    break;

                string token = text.Substring(start + 1, end - start - 1);
                if (!string.IsNullOrWhiteSpace(token))
                    tokens.Add(token.Trim());

                searchIndex = end + 1;
            }

            return tokens;
        }

        private static bool TokenMatchesObject(IReadOnlyList<string> tokens, Object target)
        {
            return target != null && TokenMatches(tokens, target.name);
        }

        private static bool TokenMatches(IReadOnlyList<string> tokens, string value)
        {
            if (tokens == null || string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (string.Equals(tokens[i], value, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static PyralisAuthoringValidationCategory GetPrefabReadinessCategory(string readinessIssue)
        {
            if (string.IsNullOrWhiteSpace(readinessIssue))
                return PyralisAuthoringValidationCategory.Other;

            string lower = readinessIssue.ToLowerInvariant();
            if (lower.Contains("pawn prefab")
                || lower.Contains("feature runtime prefab")
                || lower.Contains("projectile prefab")
                || lower.Contains("pawnroot")
                || lower.Contains("ipawnmotor")
                || lower.Contains("ipawnpresentationmodule")
                || lower.Contains("iprojectileruntimebody")
                || lower.Contains("networkobject"))
            {
                return PyralisAuthoringValidationCategory.PawnsActors;
            }

            if (lower.Contains("missing script"))
                return PyralisAuthoringValidationCategory.SceneObjects;

            return GetCategory(readinessIssue);
        }

        private static string GetPrefabReadinessAffectedMember(string readinessIssue, PyralisAuthoringValidationCategory category)
        {
            if (string.IsNullOrWhiteSpace(readinessIssue))
                return "Scene and prefab readiness audit";

            string lower = readinessIssue.ToLowerInvariant();
            if (lower.Contains("pawn prefab"))
                return "PawnDefinition.pawnPrefab";

            if (lower.Contains("feature runtime prefab"))
                return "FeatureModuleDefinition.runtimePrefab";

            if (lower.Contains("projectile prefab"))
                return "ProjectileDefinition.projectilePrefab";

            if (lower.Contains("networkobject"))
                return "Networked pawn prefab components";

            if (lower.Contains("missing script"))
                return "Referenced prefab or scene hierarchy";

            return GetAffectedMember(readinessIssue, category);
        }

        private static string GetPrefabReadinessInspectionHint(string readinessIssue)
        {
            if (string.IsNullOrWhiteSpace(readinessIssue))
                return "Inspect the Bootstrap scene references and the prefabs linked from its session route.";

            string lower = readinessIssue.ToLowerInvariant();
            if (lower.Contains("pawn prefab") || lower.Contains("pawnroot") || lower.Contains("ipawnmotor") || lower.Contains("ipawnpresentationmodule"))
                return "Open the ParticipantDefinition and PawnDefinition, then inspect the assigned pawn prefab root and child components.";

            if (lower.Contains("feature module") || lower.Contains("feature runtime prefab"))
                return "Open the FeatureModuleDefinition and inspect its Runtime Prefab for the required runtime contract components.";

            if (lower.Contains("projectile"))
                return "Open the ProjectileDefinition, then inspect its Projectile Prefab for runtime body and one physics lane.";

            if (lower.Contains("networkobject") || lower.Contains("networkmanager"))
                return "Inspect the networked pawn prefab and the scene NetworkManager Network Prefabs list.";

            if (lower.Contains("missing script"))
                return "Select the named prefab or scene hierarchy and remove or replace missing MonoBehaviour references.";

            return "Inspect the Bootstrap scene references and the prefabs linked from its session route.";
        }

        private static string GetPrefabReadinessExpected(string readinessIssue)
        {
            if (string.IsNullOrWhiteSpace(readinessIssue))
                return "Referenced scene objects and prefabs should have valid scripts and route-owned runtime components.";

            string lower = readinessIssue.ToLowerInvariant();
            if (lower.Contains("pawn prefab") || lower.Contains("pawnroot") || lower.Contains("ipawnmotor") || lower.Contains("ipawnpresentationmodule"))
                return "A pawn prefab with PawnRoot on the root plus a movement component and presentation module appropriate for its 2D, 2.5D, or 3D lane.";

            if (lower.Contains("feature module") || lower.Contains("feature runtime prefab"))
                return "A feature module asset whose enabled runtime prefab contains the feature runtime interfaces promised by the module.";

            if (lower.Contains("projectile"))
                return "A projectile definition and prefab whose runtime body can receive projectile data, move, and detect hits through either 2D or 3D physics.";

            if (lower.Contains("networkobject"))
                return "A networked pawn prefab with NetworkObject and registration appropriate for the selected network session.";

            if (lower.Contains("missing script"))
                return "No missing MonoBehaviour references on scene roots, linked prefabs, feature runtime prefabs, or projectile prefabs.";

            return "Scene and prefab references should satisfy the contracts claimed by the selected setup profile.";
        }

        private static string GetPrefabReadinessSuccess(string readinessIssue)
        {
            if (string.IsNullOrWhiteSpace(readinessIssue))
                return "Pressing Play reaches the first proof without prefab-contract failures.";

            string lower = readinessIssue.ToLowerInvariant();
            if (lower.Contains("pawn prefab") || lower.Contains("pawnroot") || lower.Contains("ipawnmotor") || lower.Contains("ipawnpresentationmodule"))
                return "Pressing Play can spawn or control one pawn, move it through the selected lane, and show its presentation without setup exceptions.";

            if (lower.Contains("projectile"))
                return "One shot spawns or resolves, travels or applies hitscan behavior, and reaches hit/miss handling without prefab contract errors.";

            if (lower.Contains("networkobject"))
                return "The networked proof can spawn/own the pawn through NGO without prefab registration or authority setup blocking the session.";

            if (lower.Contains("missing script"))
                return "The referenced scene or prefab hierarchy loads with no missing-script warnings that undermine the route proof.";

            return "The route's first playable proof can run with the referenced prefabs and scene objects intact.";
        }

        private static string GetSceneSurfaceExpected(string surface)
        {
            return PyralisAuthoringSceneSurfaceGuidance.GetExpected(surface);
        }

        private static string GetSceneSurfaceSuccess(string surface)
        {
            return PyralisAuthoringSceneSurfaceGuidance.GetSuccess(surface);
        }

        private static string GetSceneSurfaceIssueToken(string surface)
        {
            return GetIssueToken(surface, 0);
        }

        private static string GetIssueToken(string text, int fallbackIndex)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "unknown-" + fallbackIndex;

            char[] chars = text.ToCharArray();
            var builder = new System.Text.StringBuilder(chars.Length);
            bool lastWasSeparator = false;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = char.ToLowerInvariant(chars[i]);
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                    lastWasSeparator = false;
                    continue;
                }

                if (!lastWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                    lastWasSeparator = true;
                }
            }

            if (builder.Length > 0 && builder[builder.Length - 1] == '-')
                builder.Length--;

            const int maxTokenLength = 72;
            if (builder.Length > maxTokenLength)
                builder.Length = maxTokenLength;

            return builder.Length > 0 ? builder.ToString() : "unknown-" + fallbackIndex;
        }

        public static PyralisAuthoringValidationCategory GetCategory(string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                return PyralisAuthoringValidationCategory.Other;

            string lower = issue.ToLowerInvariant();

            if (lower.Contains("session") || lower.Contains("game mode") || lower.Contains("network") || lower.Contains("participant slot"))
                return PyralisAuthoringValidationCategory.SessionSetup;

            if (lower.Contains("rule") || lower.Contains("mode"))
                return PyralisAuthoringValidationCategory.GameRules;

            if (lower.Contains("setup") || lower.Contains("runtime pattern") || lower.Contains("pattern"))
                return PyralisAuthoringValidationCategory.SetupProfile;

            if (lower.Contains("participant") || lower.Contains("player") || lower.Contains("seat") || lower.Contains("hand") || lower.Contains("faction"))
                return PyralisAuthoringValidationCategory.PlayersSeats;

            if (lower.Contains("pawn") || lower.Contains("prefab") || lower.Contains("actor") || lower.Contains("motor") || lower.Contains("presentation"))
                return PyralisAuthoringValidationCategory.PawnsActors;

            if (lower.Contains("spawn") || lower.Contains("camera") || lower.Contains("scene") || lower.Contains("bootstrap") || lower.Contains("playfield"))
                return PyralisAuthoringValidationCategory.SceneObjects;

            if (lower.Contains("contract") || lower.Contains("reflective") || lower.Contains("required types"))
                return PyralisAuthoringValidationCategory.CodeContract;

            return PyralisAuthoringValidationCategory.Other;
}

        public static string GetCategoryTitle(PyralisAuthoringValidationCategory category)
        {
            return category switch
            {
                PyralisAuthoringValidationCategory.SessionSetup => "Session Setup",
                PyralisAuthoringValidationCategory.GameRules => "Game Rules",
                PyralisAuthoringValidationCategory.SetupProfile => "Setup Profile",
                PyralisAuthoringValidationCategory.PlayersSeats => "Players / Seats",
                PyralisAuthoringValidationCategory.PawnsActors => "Pawns & Actors",
                PyralisAuthoringValidationCategory.SceneObjects => "Scene Objects",
                PyralisAuthoringValidationCategory.CodeContract => "Code Contract",
                _ => "Other"
            };
        }

        private static string GetWhyItMatters(PyralisAuthoringValidationCategory category)
        {
            return category switch
            {
                PyralisAuthoringValidationCategory.SessionSetup => "The session chain decides which game mode, participants, and ownership rules the bootstrap can start.",
                PyralisAuthoringValidationCategory.GameRules => "Game rules define the playable loop the route is trying to prove.",
                PyralisAuthoringValidationCategory.SetupProfile => "The setup profile tells Pyralis which capability surfaces this route needs before scene wiring starts.",
                PyralisAuthoringValidationCategory.PlayersSeats => "Participants describe who can act in the session, including players, seats, hands, factions, and AI.",
                PyralisAuthoringValidationCategory.PawnsActors => "Pawn-backed routes cannot spawn or drive actor bodies until pawn definitions and prefabs are coherent.",
                PyralisAuthoringValidationCategory.SceneObjects => "Scene objects are the runtime anchors that let the authored route exist when Play starts.",
                PyralisAuthoringValidationCategory.CodeContract => "Code logic or metadata inconsistencies detected. This is a development bug that must be fixed in the codebase.",
                _ => "This issue still blocks confidence in the selected authoring asset."
            };
        }

        private static string GetInspectionHint(PyralisAuthoringValidationCategory category)
        {
            return category switch
            {
                PyralisAuthoringValidationCategory.SessionSetup => "Inspect the GameplaySessionBootstrap, SessionDefinition, and Default Game Mode assignments.",
                PyralisAuthoringValidationCategory.GameRules => "Inspect the GameModeDefinition and its rule assets.",
                PyralisAuthoringValidationCategory.SetupProfile => "Inspect the GameSetupProfile runtime capability selections and any optional RuntimePatternDefinition contracts assigned to it.",
                PyralisAuthoringValidationCategory.PlayersSeats => "Inspect SessionDefinition participants and each ParticipantDefinition.",
                PyralisAuthoringValidationCategory.PawnsActors => "Inspect ParticipantDefinition pawn links, PawnDefinition fields, and pawn prefab roots.",
                PyralisAuthoringValidationCategory.SceneObjects => "Inspect Bootstrap scene references, spawn points, camera, playfield, and scene-root helpers.",
                _ => "Inspect the selected asset in the normal Inspector, then return to this Validate page."
            };
        }

        private static string GetAffectedMember(string issue, PyralisAuthoringValidationCategory category)
        {
            if (string.IsNullOrWhiteSpace(issue))
                return "Selected object";

            string lower = issue.ToLowerInvariant();

            if (lower.Contains("default game mode") || lower.Contains("game mode"))
                return "SessionDefinition.defaultGameMode";

            if (lower.Contains("setup profile"))
                return "GameModeDefinition.setupProfile";

            if (lower.Contains("runtime pattern") || lower.Contains("pattern"))
                return "GameSetupProfile.runtimePatterns";

            if (lower.Contains("participant") || lower.Contains("player") || lower.Contains("seat"))
                return "SessionDefinition.defaultParticipants";

            if (lower.Contains("pawn prefab"))
                return "PawnDefinition.pawnPrefab";

            if (lower.Contains("pawn"))
                return "ParticipantDefinition.defaultPawn";

            if (lower.Contains("feature module"))
                return "PawnDefinition.featureModules";

            if (lower.Contains("spawn"))
                return "GameplaySessionBootstrap.spawnPoints";

            if (lower.Contains("camera"))
                return "GameModeDefinition.cameraRigProfile or scene camera root";

            if (lower.Contains("playfield"))
                return "GameModeDefinition.playfieldProfile or scene playfield root";

            return category switch
            {
                PyralisAuthoringValidationCategory.SessionSetup => "Session setup chain",
                PyralisAuthoringValidationCategory.GameRules => "GameModeDefinition rules",
                PyralisAuthoringValidationCategory.SetupProfile => "GameSetupProfile runtime capabilities",
                PyralisAuthoringValidationCategory.PlayersSeats => "Participant definitions",
                PyralisAuthoringValidationCategory.PawnsActors => "Pawn or actor setup",
                PyralisAuthoringValidationCategory.SceneObjects => "Scene object setup",
                _ => "Selected object"
            };
        }

        private static string GetPrimaryActionLabel(PyralisAuthoringValidationCategory category, Object target)
        {
            if (target == null)
                return "Inspect Target";

            return category switch
            {
                PyralisAuthoringValidationCategory.SessionSetup => "Inspect Session Chain",
                PyralisAuthoringValidationCategory.GameRules => "Inspect Rule Asset",
                PyralisAuthoringValidationCategory.SetupProfile => "Inspect Setup Profile",
                PyralisAuthoringValidationCategory.PlayersSeats => "Inspect Participant Setup",
                PyralisAuthoringValidationCategory.PawnsActors => "Inspect Pawn Setup",
                PyralisAuthoringValidationCategory.SceneObjects => "Inspect Scene Target",
                _ => "Inspect Target"
            };
        }

        private static string GetGuidanceActionLabel(string issueCode)
        {
            return issueCode switch
            {
                "bootstrap.sessionDefinition.missing" => "Open Bootstrap Guide",
                "session.defaultGameMode.missing" => "Open Session Guide",
                "session.defaultParticipants.missing" => "Open Participant Guide",
                "session.defaultParticipants.slot.empty" => "Open Participant Guide",
                "gameMode.setupProfile.missing" => "Open Game Rules Guide",
                "setupProfile.runtimeCapabilities.missing" => "Open Setup Profile",
                "setupProfile.runtimeCapabilities.slot.empty" => "Open Setup Profile",
                "setupProfile.runtimeCapabilities.duplicate" => "Open Setup Profile",
                "setupProfile.runtimeCapabilities.patternFamilyMismatch" => "Open Setup Profile",
                "setupProfile.runtimePatterns.missing" => "Open Setup Profile",
                "setupProfile.runtimePatterns.slot.empty" => "Open Setup Profile",
                "setupProfile.runtimePatterns.duplicate" => "Open Setup Profile",
                "pawn.pawnPrefab.missing" => "Open Pawn Guide",
                _ => string.Empty
            };
        }
    }

}
