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
        SetupRecipe,
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

            SessionDefinition session = PyralisAuthoringWindow.GetSelectedSession(bootstrap, bootstrap);
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
                issues.Add(CreateIssue("gameMode.setupProfile.missing", PyralisAuthoringValidationCategory.SetupRecipe, "Setup profile is not assigned.", "GameModeDefinition.setupProfile", mode));
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
                issues.Add(CreateIssue("setupProfile.name.required", PyralisAuthoringValidationCategory.SetupRecipe, "Setup name is required.", "GameSetupProfile.setupName", setup));

            if (setup.runtimePatterns == null || setup.runtimePatterns.Length == 0)
            {
                issues.Add(CreateIssue("setupProfile.runtimePatterns.missing", PyralisAuthoringValidationCategory.SetupRecipe, "At least one runtime pattern should be assigned.", "GameSetupProfile.runtimePatterns", setup));
                return issues;
            }

            var patternIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < setup.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setup.runtimePatterns[i];
                if (pattern == null)
                {
                    issues.Add(CreateIssue("setupProfile.runtimePatterns.slot.empty", PyralisAuthoringValidationCategory.SetupRecipe, $"Runtime Patterns[{i}] is null.", $"GameSetupProfile.runtimePatterns[{i}]", setup));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(pattern.patternId) && !patternIds.Add(pattern.patternId))
                    issues.Add(CreateIssue("setupProfile.runtimePatterns.duplicate", PyralisAuthoringValidationCategory.SetupRecipe, $"Runtime pattern `{pattern.patternId}` is assigned more than once.", "GameSetupProfile.runtimePatterns", setup));
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
                $"{row.Surface} is a proof enhancer for this route but was not detected.",
                $"Validate found: {row.Current}. This scene surface can make the proof easier to read, but it should not stop a narrow Play Mode attempt when required setup is clear.",
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

            SessionDefinition session = PyralisAuthoringWindow.GetSelectedSession(bootstrap, bootstrap);
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

            return "Scene and prefab references should satisfy the contracts claimed by the selected setup recipe.";
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
                return PyralisAuthoringValidationCategory.SetupRecipe;

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
                PyralisAuthoringValidationCategory.SetupRecipe => "Setup Recipe",
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
                PyralisAuthoringValidationCategory.SetupRecipe => "The setup recipe tells Pyralis which capability surfaces this route needs before scene wiring starts.",
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
                PyralisAuthoringValidationCategory.SetupRecipe => "Inspect the GameSetupProfile and RuntimePatternDefinition assets assigned to it.",
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
                PyralisAuthoringValidationCategory.SetupRecipe => "GameSetupProfile runtime patterns",
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
                PyralisAuthoringValidationCategory.SetupRecipe => "Inspect Setup Recipe",
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
                "setupProfile.runtimePatterns.missing" => "Open Setup Recipe Picker",
                "setupProfile.runtimePatterns.slot.empty" => "Open Setup Recipe Picker",
                "setupProfile.runtimePatterns.duplicate" => "Open Setup Recipe Picker",
                "pawn.pawnPrefab.missing" => "Open Pawn Guide",
                _ => string.Empty
            };
        }
    }

    public static class PyralisAuthoringIssueAdapter
    {
        public static PyralisAuthoringIssue Create(PyralisAuthoringValidationIssue issue)
        {
            if (issue == null)
                return null;

            return Create(
                issue.IssueCode,
                issue.Category,
                issue.Problem,
                issue.AffectedMember,
                issue.PrimaryActionLabel,
                issue.Expected,
                issue.Found,
                issue.SuccessLooksLike);
        }

        public static PyralisAuthoringIssue Create(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string problem,
            string affectedMember,
            string primaryActionLabel,
            string expected = "",
            string found = "",
            string successLooksLike = "")
        {
            PyralisAuthoringNativeAction? nativeAction = CreateNativeAction(
                issueCode,
                category,
                affectedMember,
                primaryActionLabel,
                successLooksLike);

            return new PyralisAuthoringIssue(
                issueCode,
                GetSeverity(issueCode, category),
                GetWorkIntent(issueCode, category),
                GetEvidenceState(issueCode, expected, found),
                GetTargetObject(category),
                affectedMember,
                nativeAction,
                problem);
        }

        public static List<PyralisAuthoringIssue> CreateAll(PyralisAuthoringValidationModel model)
        {
            List<PyralisAuthoringIssue> issues = new List<PyralisAuthoringIssue>();
            if (model == null)
                return issues;

            for (int i = 0; i < model.Issues.Count; i++)
            {
                PyralisAuthoringIssue typedIssue = model.Issues[i]?.TypedIssue;
                if (typedIssue != null)
                    issues.Add(typedIssue);
            }

            return issues;
        }

        private static PyralisAuthoringIssueSeverity GetSeverity(string issueCode, PyralisAuthoringValidationCategory category)
        {
            if (string.IsNullOrWhiteSpace(issueCode))
                return PyralisAuthoringIssueSeverity.Recommended;

            if (issueCode.StartsWith("prefabReadiness.recommended.", System.StringComparison.Ordinal)
                || issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal))
            {
                return PyralisAuthoringIssueSeverity.Recommended;
            }

            if (issueCode.StartsWith("route.", System.StringComparison.Ordinal))
                return PyralisAuthoringIssueSeverity.Recommended;

            if (category == PyralisAuthoringValidationCategory.CodeContract)
                return PyralisAuthoringIssueSeverity.Bug;

            return category == PyralisAuthoringValidationCategory.Other
? PyralisAuthoringIssueSeverity.Recommended
                : PyralisAuthoringIssueSeverity.Required;
        }

        private static string GetWorkIntent(string issueCode, PyralisAuthoringValidationCategory category)
        {
            if (!string.IsNullOrWhiteSpace(issueCode)
                && (issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal)
                    || issueCode.StartsWith("prefabReadiness.recommended.", System.StringComparison.Ordinal)))
            {
                return PyralisSetupFlowWorkIntent.ProofEnhancer.ToString();
            }

            switch (category)
            {
                case PyralisAuthoringValidationCategory.SceneObjects:
                    return PyralisSetupFlowWorkIntent.ProofEnhancer.ToString();
                case PyralisAuthoringValidationCategory.Other:
                    return PyralisSetupFlowWorkIntent.FeatureCard.ToString();
                default:
                    return PyralisSetupFlowWorkIntent.RequiredSetup.ToString();
            }
        }

        private static PyralisAuthoringEvidenceState GetEvidenceState(string issueCode, string expected, string found)
        {
            if (!string.IsNullOrWhiteSpace(issueCode)
                && issueCode.StartsWith("prefabReadiness.", System.StringComparison.Ordinal))
            {
                return PyralisAuthoringEvidenceState.CandidateDetected;
            }

            if (!string.IsNullOrWhiteSpace(found))
                return PyralisAuthoringEvidenceState.Missing;

            if (!string.IsNullOrWhiteSpace(expected))
                return PyralisAuthoringEvidenceState.Missing;

            return PyralisAuthoringEvidenceState.Missing;
        }

        private static PyralisAuthoringNativeAction? CreateNativeAction(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string affectedMember,
            string primaryActionLabel,
            string successLooksLike)
        {
            PyralisAuthoringActionSurface surface = category == PyralisAuthoringValidationCategory.SceneObjects
                ? PyralisAuthoringActionSurface.Hierarchy
                : PyralisAuthoringActionSurface.Inspector;

            string verb = string.IsNullOrWhiteSpace(primaryActionLabel) ? "Inspect" : primaryActionLabel;
            string target = GetTargetObject(category);
            string field = string.IsNullOrWhiteSpace(affectedMember) ? "the affected field or component" : affectedMember;
            string success = string.IsNullOrWhiteSpace(successLooksLike) ? "the issue no longer appears in Validate" : successLooksLike;

            if (!string.IsNullOrWhiteSpace(issueCode)
                && issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal))
            {
                verb = "Create or link";
            }

            return new PyralisAuthoringNativeAction(verb, surface, target, field, success);
        }

        private static string GetTargetObject(PyralisAuthoringValidationCategory category)
        {
            switch (category)
            {
                case PyralisAuthoringValidationCategory.SessionSetup:
                    return "Session setup chain";
                case PyralisAuthoringValidationCategory.GameRules:
                    return "GameModeDefinition";
                case PyralisAuthoringValidationCategory.SetupRecipe:
                    return "GameSetupProfile";
                case PyralisAuthoringValidationCategory.PlayersSeats:
                    return "ParticipantDefinition or SessionDefinition";
                case PyralisAuthoringValidationCategory.PawnsActors:
                    return "PawnDefinition or pawn prefab";
                case PyralisAuthoringValidationCategory.SceneObjects:
                    return "scene Hierarchy";
                default:
                    return "selected authoring object";
            }
        }
    }

    public sealed class PyralisAuthoringRouteReport
    {
        private PyralisAuthoringRouteReport(string routeName, string nextStep, string routeGuidance, List<string> validationIssues)
        {
            RouteName = routeName;
            NextStep = nextStep;
            RouteGuidance = routeGuidance;
            ValidationIssues = validationIssues ?? new List<string>();
        }

        public string RouteName { get; }
        public string NextStep { get; }
        public string RouteGuidance { get; }
        public IReadOnlyList<string> ValidationIssues { get; }

        public static PyralisAuthoringRouteReport Build(Object selection)
        {
            if (selection == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No selection",
                    "Start in Unity: right-click Hierarchy -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component to add GameplaySessionBootstrap.",
                    "The Authoring Window guides the route, but the first scene object is still made with normal Unity workflow. After adding GameplaySessionBootstrap, select it here and wire the SessionDefinition in the Inspector.",
                    new List<string>());
            }

            if (selection is GameplaySessionBootstrap bootstrap)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(bootstrap), null, bootstrap);

            if (selection is GameObject gameObject && gameObject.GetComponent<GameplaySessionBootstrap>() == null)
            {
                if (!IsSceneSetupRootCandidate(gameObject))
                {
                    return new PyralisAuthoringRouteReport(
                        "Scene support object selected",
                        "Create or select a Gameplay Root first, then add GameplaySessionBootstrap there. Keep camera, lights, art, and playfield objects as scene support objects.",
                        "This object can support the route, but it should not become the composition root. Use Hierarchy -> Create Empty for Gameplay Root, add GameplaySessionBootstrap in the Inspector, then return to camera or playfield setup once the active setup exists.",
                        GetIssues(selection));
                }

                return new PyralisAuthoringRouteReport(
                    "Scene object selected",
                    $"Use Inspector -> Add Component search for GameplaySessionBootstrap on `{gameObject.name}`.",
                    "This is the right native Unity flow: create and name the scene root in Hierarchy first, then add Pyralis components through the Inspector so the setup object remains visible and editable.",
                    GetIssues(selection));
            }

            if (selection is SessionDefinition session)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(session), session.GetValidationIssues());

            if (selection is GameModeDefinition mode)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(mode), mode.GetValidationIssues());

            if (selection is GameSetupProfile setupProfile)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(setupProfile), setupProfile.GetValidationIssues());

            if (selection is ParticipantDefinition participant)
                return BuildFromParticipant(participant);

            if (selection is PawnDefinition pawn)
                return BuildFromPawn(pawn);

            return new PyralisAuthoringRouteReport(
                "Selected Context",
                "Inspect this asset in the context of the active setup route.",
                "This selected object may be part of setup, but route guidance should come from the active bootstrap, session, game mode, or setup profile. Pin or select one of those route anchors when you want whole-game validation.",
                GetIssues(selection));
        }

        private static bool IsSceneSetupRootCandidate(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            if (gameObject.GetComponent<Camera>() != null || gameObject.GetComponent<Light>() != null)
                return false;

            Component[] components = gameObject.GetComponents<Component>();
            return components.Length <= 1 || gameObject.name.Contains("Gameplay");
        }

        private static PyralisAuthoringRouteReport BuildFromAnalysis(PyralisSetupRouteAnalysis analysis, List<string> validationIssues = null, GameplaySessionBootstrap bootstrap = null)
        {
            if (analysis == null || analysis.Session == null && analysis.Mode == null && analysis.SetupProfile == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create a Session Definition asset in the Project window, then assign it to GameplaySessionBootstrap > Session Definition by drag/drop or the field's object picker circle.",
                    "Use normal Unity workflow first: Project window -> open the target folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Session Definition. The Inspector remains the source of truth for assigning it to the scene bootstrap.",
                    validationIssues ?? new List<string>());
            }

            List<string> issues = validationIssues ?? BuildValidationIssues(analysis);

            if (bootstrap != null)
            {
                PyralisSetupFlowReport reflectiveReport = PyralisReflectiveContractSolver.BuildReport(bootstrap);
                for (int i = 0; i < reflectiveReport.Steps.Count; i++)
                {
                    PyralisSetupFlowStep step = reflectiveReport.Steps[i];
                    if (step.Status != PyralisSetupFlowStepStatus.Ready)
                    {
                        if (!issues.Contains(step.Message))
                            issues.Add(step.Message);
                    }
                }
            }
PyralisAuthoringRouteDescriptor descriptor = PyralisAuthoringRouteDescriptor.Build(analysis);
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(descriptor);
            if (analysis.Session != null && analysis.Mode == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create a Game Mode Definition asset, then select/open the SessionDefinition asset and assign Default Game Mode by drag/drop or the field's object picker circle.",
                    "Use the SessionDefinition Inspector to wire the game rules asset. Pyralis explains the route, while native Unity creation and Inspector fields show which folder and field own the connection.",
                    issues);
            }

            if (analysis.SetupProfile == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create or choose a Game Setup Profile asset, then select/open the GameModeDefinition asset and assign Setup Profile by drag/drop or the field's object picker circle.",
                    "The setup profile is the route recipe. Create it from the Project window when wiring manually, then choose runtime patterns before adding scene extras.",
                    issues);
            }

            if (!analysis.HasAssignedPatterns)
            {
                bool hasExistingPatterns = AssetDatabase.FindAssets("t:RuntimePatternDefinition").Length > 0;
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    hasExistingPatterns
                        ? "Select/open the GameSetupProfile asset, choose a route family in Runtime Capabilities -> Capability To Add, then click Add Capability."
                        : "Create or import a Runtime Pattern Definition, then select/open the GameSetupProfile asset and add it through Runtime Capabilities -> Add Capability.",
                    hasExistingPatterns
                        ? "Use existing runtime patterns for first proofs; create new capability patterns only for advanced custom setup categories."
                        : "No Runtime Pattern Definition assets were found in the project. For a 1P movement proof, create a Runtime Pattern Definition, select it, and use Pawn Action defaults before participant and pawn wiring can become route-aware.",
                    issues);
            }

            if (!analysis.HasValidPatterns)
            {
                return new PyralisAuthoringRouteReport(
                    "Incomplete capability pattern",
                    "Select the assigned Runtime Pattern Definition and clear its Inspector validation issues before adding participants.",
                    "A Runtime Pattern Definition is assigned, but Pyralis cannot use default placeholder metadata as the route source of truth. Fill its id, display name, description, setup notes, capability family, embodiment, and control surfaces first.",
                    issues);
            }

            if (analysis.Session == null)
            {
                return new PyralisAuthoringRouteReport(
                    analysis.RouteName,
                    "Assign this setup profile through GameModeDefinition, then SessionDefinition, then check the Bootstrap Setup Flow.",
                    analysis.RequiresPawn
                        ? proof.Guidance
                        : "Pawn prefab can stay empty. " + proof.Guidance,
                    issues);
            }

            if (!analysis.HasParticipants)
            {
                return new PyralisAuthoringRouteReport(
                    analysis.RouteName,
                    "Add at least one player, seat, AI, cursor, hand, or other participant to the session.",
                    "Participants can be players, AI, seats, hands, factions, cameras, cursors, or turn owners.",
                    issues);
            }

            if (analysis.RequiresPawn)
            {
                string pawnIssue = analysis.ParticipantPawnIssue;
                if (!string.IsNullOrWhiteSpace(pawnIssue))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        pawnIssue,
                        "Pawn-backed routes need ParticipantDefinition > PawnDefinition > pawn prefab with PawnRoot before they can spawn actors.",
                        issues);
                }

                if (bootstrap == null)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Select the GameplaySessionBootstrap and assign Spawn Points before running the first proof.",
                        proof.Guidance,
                        issues);
                }

                int assignedSpawnPointCount = CountAssignedSpawnPoints(bootstrap);
                int assignedParticipantCount = CountAssignedParticipants(analysis.Session);
                if (assignedSpawnPointCount == 0)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Add a scene Transform to GameplaySessionBootstrap > Spawn Points, then run the first proof.",
                        proof.Guidance,
                        issues);
                }

                if (assignedParticipantCount > assignedSpawnPointCount)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        $"This scene has {assignedSpawnPointCount} assigned spawn point(s) for {assignedParticipantCount} default participant(s). Add one spawn point per participant, or remove extra participants for a clean 1P proof.",
                        "For the first movement proof, keep the route boring on purpose: one participant, one pawn, one assigned spawn point, then Play Mode.",
                        issues);
                }

                if (!HasAssignedCameraRig(bootstrap))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Create and assign a Camera Root before Play Mode.",
                        "The 2D pawn movement stack needs camera bounds for clamping and framing. Native path: keep or create exactly one enabled physical Unity Camera for this shared proof, usually the default Main Camera; do not delete it for the normal Cinemachine route. Create Camera Root, add CinemachineCameraRigController, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, verify the physical Main Camera is tagged MainCamera with Cinemachine Brain, assign that physical camera as Target Camera, disable or remove accidental extra physical Camera objects only when they were created by mistake, then drag the Camera Root object from Hierarchy into GameplaySessionBootstrap > Camera Rig Controller.",
                        issues);
                }

                if (analysis.Requires2DCameraBounds() && !HasUsable2DCameraBounds(bootstrap, analysis.Mode))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Make the camera rig usable for 2D bounds before Play Mode.",
                        "The 2D movement stack uses the assigned CinemachineCameraRigController as its bounds provider. Select/open the Camera Root and either assign an orthographic CameraRigProfile, or select the physical Target Camera and set Camera > Projection to Orthographic. If using a profile, also assign it to GameModeDefinition > Camera Rig Profile so the rig applies the same intent at runtime.",
                        issues);
                }

                if (analysis.LikelyUsesInputManager())
                {
                    if (!TryGetAssignedPlayerInputManager(bootstrap, out PlayerInputManager playerInputManager))
                    {
                        return new PyralisAuthoringRouteReport(
                            "Pawn-backed local-join route",
                            "For a 1P proof, set SessionDefinition > Max Participants to 1; use PlayerInputManager only for local join.",
                            "Multi-participant local join uses Unity PlayerInputManager to receive join/leave events. Single-player pawn movement should leave Bootstrap > Player Input Manager empty unless the route is intentionally testing local join.",
                            issues);
                    }

                    if (playerInputManager.playerPrefab == null)
                    {
                        return new PyralisAuthoringRouteReport(
                            "Pawn-backed local-join route",
                            "Configure PlayerInputManager > Player Prefab before Play Mode, or disable local join for this proof.",
                            "Unity PlayerInputManager logs runtime errors when join is enabled without a Player Prefab. Use a dedicated PlayerInput prefab for local join; do not use the spawned pawn prefab unless that prefab is intentionally the input-join prefab.",
                            issues);
                    }
                }

                PyralisSetupFlowReport setupFlowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
                PyralisSetupFlowStep inputProfileStep = setupFlowReport.GetStep("Assign Input Profile");
                if (inputProfileStep != null && inputProfileStep.IsRequiredIssue)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        inputProfileStep.Message,
                        "Before Play Mode, select/open the effective InputProfile from the participant, PawnDefinition, or SessionDefinition fallback. Assign Actions, verify Primary Action Map, and make Move Action match a movement action in the Input Action Asset.",
                        issues);
                }

                PyralisAuthoringSceneSurfaceRow playfieldSurface = GetSceneSurfaceRow(bootstrap, PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);
                if (playfieldSurface != null && playfieldSurface.Recommended && !playfieldSurface.Present)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Create a playable Environment / Playfield surface before Play Mode.",
                        "A spawn point only says where the pawn appears; it is not a movement proof surface. In the Hierarchy, create a Ground, Platform, Tilemap, Zone, or Playfield Root that matches the route, then use the Inspector to add an intentional Collider2D, TilemapCollider2D, bounds provider, or project-owned surface component before judging movement, jump, camera follow, or combat spacing in Play Mode.",
                        issues);
                }

                return new PyralisAuthoringRouteReport(
                    "Pawn-backed route",
                    "Enter Play Mode and confirm the first pawn spawns, receives input, and moves.",
                    proof.Guidance,
                    issues);
            }

            return new PyralisAuthoringRouteReport(
                analysis.RouteName,
                proof.FirstUnityFocus,
                "Pawn prefab can stay empty unless this setup later introduces actor bodies. " + proof.Guidance,
                issues);
        }

        private static PyralisAuthoringRouteReport BuildFromParticipant(ParticipantDefinition participant)
        {
            if (participant == null)
                return Build(null);

            string guidance = participant.defaultPawn == null
                ? "Default Pawn can stay empty for no-pawn routes. Assign one only when this participant owns an actor body."
                : "This participant points to a pawn definition. Validate the pawn next if this is a pawn-backed route.";

            return new PyralisAuthoringRouteReport(
                "Participant asset",
                participant.defaultPawn == null ? "Assign this participant to a SessionDefinition." : "Validate the assigned PawnDefinition.",
                guidance,
                new List<string>());
        }

        private static PyralisAuthoringRouteReport BuildFromPawn(PawnDefinition pawn)
        {
            if (pawn == null)
                return Build(null);

            string nextStep = pawn.pawnPrefab == null
                ? "Assign PawnDefinition > Pawn Prefab."
                : "Assign this PawnDefinition to pawn-backed participants.";

            string guidance = pawn.pawnPrefab == null
                ? "A pawn-backed route cannot spawn this pawn until it points to a prefab with PawnRoot on the root GameObject."
                : "Use this only for participants that need spawned or placed actor bodies.";

            return new PyralisAuthoringRouteReport("Pawn-backed asset", nextStep, guidance, PyralisAuthoringValidationModel.BuildPawnRouteValidationIssues(pawn));
        }

        private static PyralisAuthoringSceneSurfaceRow GetSceneSurfaceRow(GameplaySessionBootstrap bootstrap, string surface)
        {
            if (bootstrap == null || string.IsNullOrWhiteSpace(surface))
                return null;

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && row.Surface == surface)
                    return row;
            }

            return null;
        }

        private static List<string> BuildValidationIssues(PyralisSetupRouteAnalysis analysis)
        {
            if (analysis.Session != null)
                return analysis.Session.GetValidationIssues();

            if (analysis.Mode != null)
                return analysis.Mode.GetValidationIssues();

            if (analysis.SetupProfile != null)
                return analysis.SetupProfile.GetValidationIssues();

            return new List<string>();
        }

        private static List<string> GetIssues(Object selection)
        {
            return selection switch
            {
                SessionDefinition session => session.GetValidationIssues(),
                PawnDefinition pawn => pawn.GetValidationIssues(),
                GameModeDefinition mode => mode.GetValidationIssues(),
                FeatureModuleDefinition module => module.GetValidationIssues(),
                RuntimePatternDefinition pattern => pattern.GetValidationIssues(),
                GameSetupProfile setup => setup.GetValidationIssues(),
                _ => new List<string>()
            };
        }

        private static int CountAssignedSpawnPoints(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return 0;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            if (spawnPoints == null || !spawnPoints.isArray)
                return 0;

            int count = 0;
            for (int i = 0; i < spawnPoints.arraySize; i++)
            {
                if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
        }

        private static int CountAssignedParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] != null)
                    count++;
            }

            return count;
        }

        private static bool HasAssignedCameraRig(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue != null;
        }

        private static bool HasUsable2DCameraBounds(GameplaySessionBootstrap bootstrap, GameModeDefinition mode)
        {
            if (!TryGetAssignedCameraRig(bootstrap, out CinemachineCameraRigController rig))
                return false;

            SerializedObject serializedRig = new SerializedObject(rig);
            CameraRigProfile rigProfile = serializedRig.FindProperty("cameraRigProfile")?.objectReferenceValue as CameraRigProfile;
            if (rigProfile != null)
                return rigProfile.orthographic;

            if (mode != null && mode.cameraRigProfile != null && mode.cameraRigProfile.orthographic)
                return true;

            Camera targetCamera = serializedRig.FindProperty("targetCamera")?.objectReferenceValue as Camera;
            if (targetCamera != null)
                return targetCamera.orthographic;

            Camera childCamera = rig.GetComponentInChildren<Camera>(true);
            return childCamera != null && childCamera.orthographic;
        }

        private static bool TryGetAssignedCameraRig(GameplaySessionBootstrap bootstrap, out CinemachineCameraRigController rig)
        {
            rig = null;
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            rig = serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue as CinemachineCameraRigController;
            return rig != null;
        }

        private static bool TryGetAssignedPlayerInputManager(GameplaySessionBootstrap bootstrap, out PlayerInputManager playerInputManager)
        {
            playerInputManager = null;
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            playerInputManager = serializedBootstrap.FindProperty("playerInputManager")?.objectReferenceValue as PlayerInputManager;
            return playerInputManager != null;
        }
    }
}
