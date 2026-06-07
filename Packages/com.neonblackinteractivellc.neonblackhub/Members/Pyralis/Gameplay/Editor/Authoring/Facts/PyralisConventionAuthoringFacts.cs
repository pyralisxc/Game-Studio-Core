using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Spawning;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Features.Zones;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisConventionAuthoringFacts
    {
        internal const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();

            AddCreateAssetMenuFact<SessionDefinition>(
                facts,
                "reflection.create-asset-menu.session-definition",
                "SessionDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for the top-level session asset.",
                "Core setup chain",
                new[] { "setup.assign-session-definition", "inspector.gameplay-session-bootstrap.session-definition" });
            AddCreateAssetMenuFact<GameModeDefinition>(
                facts,
                "reflection.create-asset-menu.game-mode-definition",
                "GameModeDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for the game rules asset.",
                "Core setup chain",
                new[] { "setup.assign-default-game-mode", "inspector.session-definition.default-game-mode" });
            AddCreateAssetMenuFact<GameSetupProfile>(
                facts,
                "reflection.create-asset-menu.game-setup-profile",
                "GameSetupProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for the runtime pattern setup profile.",
                "Core setup chain",
                new[] { "setup.assign-setup-profile", "inspector.game-mode-definition.setup-profile" });
            AddCreateAssetMenuFact<RuntimePatternDefinition>(
                facts,
                "reflection.create-asset-menu.runtime-pattern-definition",
                "RuntimePatternDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for reusable runtime pattern assets.",
                "Capability setup",
                new[] { "setup.add-runtime-patterns", "inspector.game-setup-profile.runtime-patterns" });
            AddCreateAssetMenuFact<CameraRigProfile>(
                facts,
                "reflection.create-asset-menu.camera-rig-profile",
                "CameraRigProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for camera framing, follow, zoom, and 2D orthographic route choices.",
                "world/camera route",
                new[] { "inspector.game-mode-definition.camera-and-playfield", "inspector.cinemachine-camera-rig-controller.camera-fields", "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds" });
            AddCreateAssetMenuFact<PlayfieldProfile>(
                facts,
                "reflection.create-asset-menu.playfield-profile",
                "PlayfieldProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for movement space, bounds, wrap, and arena-depth rules.",
                "world/camera route",
                new[] { "inspector.game-mode-definition.camera-and-playfield", "inspector.cinemachine-camera-rig-controller.camera-fields", "route.world-camera", "proof.camera-cursor-world" });
            AddCreateAssetMenuFact<SettingsProfile>(
                facts,
                "reflection.create-asset-menu.settings-profile",
                "SettingsProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for settings and menu defaults.",
                "UI/HUD/menu route",
                new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "capability.ui-scoring-feedback" });
            AddCreateAssetMenuFact<FeatureModuleDefinition>(
                facts,
                "reflection.create-asset-menu.feature-module-definition",
                "FeatureModuleDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for custom object, pickup, hazard, NPC, UI, feedback, and networking feature modules.",
                "custom object/feature route",
                new[] { "inspector.game-mode-definition.required-feature-modules", "inspector.feature-module-definition.profile-runtime-network", "route.custom-object-feature", "proof.custom-object-effect" });
            AddCreateAssetMenuFact<ActionDefinition>(
                facts,
                "reflection.create-asset-menu.action-definition",
                "ActionDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for one selectable command or resolver-backed action.",
                "action selection route",
                new[] { "route.custom-object-feature", "route.ui-hud-menu", "route.tabletop-card", "proof.action-selection", "proof.board-card-action", "capability.interaction-action-selection" });
            AddCreateAssetMenuFact<BoardDefinition>(
                facts,
                "reflection.create-asset-menu.board-definition",
                "BoardDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for tabletop board layouts and starting pieces.",
                "tabletop board/card route",
                new[] { "inspector.game-mode-definition.board-and-turn-rules", "inspector.tabletop-board-grid-presenter.board-fields", "route.tabletop-card", "proof.board-card-action" });
            AddCreateAssetMenuFact<BoardMovePolicyDefinition>(
                facts,
                "reflection.create-asset-menu.board-move-policy-definition",
                "BoardMovePolicyDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for tabletop legal-move policy.",
                "tabletop board/card route",
                new[] { "inspector.tabletop-board-grid-presenter.board-fields", "route.tabletop-card", "proof.board-card-action" });
            AddCreateAssetMenuFact<TurnOrderDefinition>(
                facts,
                "reflection.create-asset-menu.turn-order-definition",
                "TurnOrderDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for tabletop and turn/menu action order.",
                "tabletop board/card route",
                new[] { "inspector.game-mode-definition.board-and-turn-rules", "route.tabletop-card", "proof.board-card-action" });
            AddCreateAssetMenuFact<PhaseDefinition>(
                facts,
                "reflection.create-asset-menu.phase-definition",
                "PhaseDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for turn phase rules.",
                "tabletop board/card route",
                new[] { "route.tabletop-card", "proof.board-card-action" });
            AddCreateAssetMenuFact<BoardPieceDefinition>(
                facts,
                "reflection.create-asset-menu.board-piece-definition",
                "BoardPieceDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for tabletop board pieces.",
                "tabletop board/card route",
                new[] { "reflection.create-asset-menu.board-definition", "route.tabletop-card", "proof.board-card-action" });
            AddCreateAssetMenuFact<ProjectileDefinition>(
                facts,
                "reflection.create-asset-menu.projectile-definition",
                "ProjectileDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for projectile behavior.",
                "combat/projectile route",
                new[] { "capability.combat-projectile-proof", "proof.custom-object-effect", "proof.npc-enemy-behavior" });
            AddCreateAssetMenuFact<FireModeDefinition>(
                facts,
                "reflection.create-asset-menu.fire-mode-definition",
                "FireModeDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for firing cadence, burst, and spread behavior.",
                "combat/projectile route",
                new[] { "capability.combat-projectile-proof", "proof.custom-object-effect", "proof.npc-enemy-behavior" });
            AddCreateAssetMenuFact<CombatActionDefinition>(
                facts,
                "reflection.create-asset-menu.combat-action-definition",
                "CombatActionDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for one combat action.",
                "combat route",
                new[] { "capability.combat-projectile-proof", "proof.npc-enemy-behavior", "proof.custom-object-effect" });
            AddCreateAssetMenuFact<ActorFeedbackProfile>(
                facts,
                "reflection.create-asset-menu.actor-feedback-profile",
                "ActorFeedbackProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for actor feedback and route-readable reaction polish.",
                "UI/HUD/menu and combat routes",
                new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "proof.npc-enemy-behavior", "capability.ui-scoring-feedback" });
            AddCreateAssetMenuFact<ActorCombatReactionProfile>(
                facts,
                "reflection.create-asset-menu.actor-combat-reaction-profile",
                "ActorCombatReactionProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for combat reaction behavior.",
                "combat/NPC route",
                new[] { "route.npc-enemy-actor", "proof.npc-enemy-behavior", "capability.combat-projectile-proof" });
            AddCreateAssetMenuFact<EnemyFeatureProfile>(
                facts,
                "reflection.create-asset-menu.enemy-feature-profile",
                "EnemyFeatureProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for enemy feature setup.",
                "NPC/enemy actor route",
                new[] { "route.npc-enemy-actor", "proof.npc-enemy-behavior" });
            AddCreateAssetMenuFact<PickupFeatureProfile>(
                facts,
                "reflection.create-asset-menu.pickup-feature-profile",
                "PickupFeatureProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for pickup feature setup.",
                "custom object/feature route",
                new[] { "route.custom-object-feature", "proof.custom-object-effect" });

            AddAddComponentMenuFact<GameplaySessionBootstrap>(
                facts,
                "reflection.add-component-menu.gameplay-session-bootstrap",
                "GameplaySessionBootstrap Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for the scene setup root.",
                "Core setup chain",
                new[] { "setup.scene-bootstrap", "inspector.gameplay-session-bootstrap.session-definition" });
            AddAddComponentMenuFact<PyralisGameplayLifetimeScope>(
                facts,
                "reflection.add-component-menu.pyralis-gameplay-lifetime-scope",
                "PyralisGameplayLifetimeScope Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for the visible Pyralis runtime composition scope.",
                "Core setup chain",
                new[] { "setup.scene-bootstrap", "setup.runtime-service-ownership", "proof.1p-pawn-movement" });
            AddAddComponentMenuFact<CinemachineCameraRigController>(
                facts,
                "reflection.add-component-menu.cinemachine-camera-rig-controller",
                "CinemachineCameraRigController Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for the shared camera route controller.",
                "world/camera route",
                new[] { "inspector.cinemachine-camera-rig-controller.camera-fields", "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds" });
            AddAddComponentMenuFact<TabletopBoardGridPresenter>(
                facts,
                "reflection.add-component-menu.tabletop-board-grid-presenter",
                "TabletopBoardGridPresenter Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for a board presenter that can build selectable tabletop spaces.",
                "tabletop board/card route",
                new[] { "inspector.tabletop-board-grid-presenter.board-fields", "route.tabletop-card", "proof.board-card-action" });
            AddAddComponentMenuFact<TabletopBoardSelectionBridge>(
                facts,
                "reflection.add-component-menu.tabletop-board-selection-bridge",
                "TabletopBoardSelectionBridge Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for a board action selection bridge.",
                "tabletop board/card route",
                new[] { "route.tabletop-card", "proof.board-card-action", "capability.interaction-action-selection" });
            AddAddComponentMenuFact<TabletopTurnStatusPresenter>(
                facts,
                "reflection.add-component-menu.tabletop-turn-status-presenter",
                "TabletopTurnStatusPresenter Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for a small proof HUD label that shows which tabletop seat acts next.",
                "tabletop board/card route",
                new[] { "inspector.tabletop-turn-status-presenter.fields", "route.tabletop-card", "proof.board-card-action", "capability.ui-scoring-feedback" });
            AddAddComponentMenuFact<ActorFeatureHost>(
                facts,
                "reflection.add-component-menu.actor-feature-host",
                "ActorFeatureHost Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for installing custom feature modules on an actor.",
                "custom object/feature route",
                new[] { "inspector.feature-module-definition.profile-runtime-network", "route.custom-object-feature", "proof.custom-object-effect" });
            AddAddComponentMenuFact<ParticipantFeedbackHudPresenter>(
                facts,
                "reflection.add-component-menu.participant-feedback-hud-presenter",
                "ParticipantFeedbackHudPresenter Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for participant feedback HUD output.",
                "UI/HUD/menu route",
                new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "capability.ui-scoring-feedback" });
            AddAddComponentMenuFact<ParticipantHealthHudBinder>(
                facts,
                "reflection.add-component-menu.participant-health-hud-binder",
                "ParticipantHealthHudBinder Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for participant health HUD binding.",
                "UI/HUD/menu route",
                new[] { "route.ui-hud-menu", "proof.ui-hud-menu", "capability.ui-scoring-feedback" });
            AddAddComponentMenuFact<EnemyAI>(
                facts,
                "reflection.add-component-menu.enemy-ai",
                "EnemyAI Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for enemy AI behavior.",
                "NPC/enemy actor route",
                new[] { "route.npc-enemy-actor", "proof.npc-enemy-behavior" });
            AddAddComponentMenuFact<EnemySpawner>(
                facts,
                "reflection.add-component-menu.enemy-spawner",
                "EnemySpawner Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for scene-authored enemy spawning.",
                "NPC/enemy actor route",
                new[] { "route.npc-enemy-actor", "proof.npc-enemy-behavior" });
            AddAddComponentMenuFact<DamageZone2D>(
                facts,
                "reflection.add-component-menu.damage-zone-2d",
                "DamageZone2D Add Component Menu",
                PyralisAuthoringFactKind.SceneComponent,
                "Inspector Add Component path for a 2D hazard or damage trigger.",
                "custom object/feature route",
                new[] { "route.custom-object-feature", "proof.custom-object-effect" });
            AddAddComponentMenuFact<HealthComponent>(
                facts,
                "reflection.add-component-menu.health-component",
                "HealthComponent Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for any damageable player, enemy, prop, or custom object.",
                "combat/NPC/custom object routes",
                new[] { "route.npc-enemy-actor", "route.custom-object-feature", "proof.npc-enemy-behavior", "proof.custom-object-effect", "capability.combat-projectile-proof" });
            AddAddComponentMenuFact<ActorAnimationDriver>(
                facts,
                "reflection.add-component-menu.actor-animation-driver",
                "ActorAnimationDriver Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for actor animation and presentation mapping.",
                "pawn/NPC actor route",
                new[] { "route.pawn-actor", "route.npc-enemy-actor", "proof.1p-pawn-movement", "proof.npc-enemy-behavior" });

            AddSerializedFieldFact<GameModeDefinition>(
                facts,
                "convention.serialized-field.game-mode-definition.board-definition",
                "GameModeDefinition Board Definition Field",
                "boardDefinition",
                "The board definition field is a native Inspector assignment convention for tabletop routes.",
                "tabletop board/card route",
                "GameModeDefinition.boardDefinition -> BoardDefinition",
                new[] { "inspector.game-mode-definition.board-and-turn-rules", "route.tabletop-card", "proof.board-card-action" });
            AddSerializedFieldFact<GameModeDefinition>(
                facts,
                "convention.serialized-field.game-mode-definition.turn-order-definition",
                "GameModeDefinition Turn Order Field",
                "turnOrderDefinition",
                "The turn order field is a native Inspector assignment convention for tabletop and turn/menu routes.",
                "tabletop board/card route",
                "GameModeDefinition.turnOrderDefinition -> TurnOrderDefinition",
                new[] { "inspector.game-mode-definition.board-and-turn-rules", "route.tabletop-card", "proof.board-card-action" });
            AddSerializedFieldFact<GameModeDefinition>(
                facts,
                "convention.serialized-field.game-mode-definition.required-feature-modules",
                "GameModeDefinition Required Feature Modules Field",
                "requiredFeatureModules",
                "The required feature modules field is a native Inspector assignment convention for custom object and feature routes.",
                "custom object/feature route",
                "GameModeDefinition.requiredFeatureModules -> FeatureModuleDefinition[]",
                new[] { "inspector.game-mode-definition.required-feature-modules", "route.custom-object-feature", "proof.custom-object-effect" });
            AddSerializedFieldFact<GameModeDefinition>(
                facts,
                "convention.serialized-field.game-mode-definition.camera-rig-profile",
                "GameModeDefinition Camera Rig Profile Field",
                "cameraRigProfile",
                "The camera rig profile field is a native Inspector assignment convention for camera/world routes.",
                "world/camera route",
                "GameModeDefinition.cameraRigProfile -> CameraRigProfile",
                new[] { "inspector.game-mode-definition.camera-and-playfield", "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds" });
            AddSerializedFieldFact<FeatureModuleDefinition>(
                facts,
                "convention.serialized-field.feature-module-definition.profile-asset",
                "FeatureModuleDefinition Profile Asset Field",
                "profileAsset",
                "The profile asset field is a native Inspector assignment convention for feature modules.",
                "custom object/feature route",
                "FeatureModuleDefinition.profileAsset -> route-specific profile asset",
                new[] { "inspector.feature-module-definition.profile-runtime-network", "route.custom-object-feature", "proof.custom-object-effect" });
            AddSerializedFieldFact<FeatureModuleDefinition>(
                facts,
                "convention.serialized-field.feature-module-definition.runtime-prefab",
                "FeatureModuleDefinition Runtime Prefab Field",
                "runtimePrefab",
                "The runtime prefab field is a native Inspector assignment convention for feature modules.",
                "custom object/feature route",
                "FeatureModuleDefinition.runtimePrefab -> runtime feature prefab",
                new[] { "inspector.feature-module-definition.profile-runtime-network", "route.custom-object-feature", "proof.custom-object-effect" });
            AddSerializedFieldFact<CinemachineCameraRigController>(
                facts,
                "convention.serialized-field.cinemachine-camera-rig-controller.camera-rig-profile",
                "CinemachineCameraRigController Camera Rig Profile Field",
                "cameraRigProfile",
                "The camera rig profile field is a native Inspector assignment convention on the scene camera controller.",
                "world/camera route",
                "CinemachineCameraRigController.cameraRigProfile -> CameraRigProfile",
                new[] { "inspector.cinemachine-camera-rig-controller.camera-fields", "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds" });

            return facts;
        }

        internal static void AddCreateAssetMenuFact<T>(
            List<PyralisAuthoringFact> facts,
            string stableId,
            string displayName,
            PyralisAuthoringFactKind kind,
            string summary,
            string routeRelevance,
            string[] relatedStableIds) where T : ScriptableObject
        {
            CreateAssetMenuAttribute attribute = typeof(T).GetCustomAttribute<CreateAssetMenuAttribute>();
            if (attribute == null || string.IsNullOrWhiteSpace(attribute.menuName))
                return;

            string typeName = typeof(T).Name;
            string fileName = string.IsNullOrWhiteSpace(attribute.fileName) ? typeName : attribute.fileName;
            string createPath = "Assets/Create/" + attribute.menuName;

            facts.Add(new PyralisAuthoringFact(
                stableId,
                displayName,
                kind,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                string.Empty,
                requiredDefinitions: kind == PyralisAuthoringFactKind.Definition ? new[] { typeName } : null,
                requiredProfiles: kind == PyralisAuthoringFactKind.Profile ? new[] { typeName } : null,
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        createPath,
                        fileName,
                        typeName + " asset exists in the chosen project folder")
                },
                workIntent: "NativeCreatePath",
                relatedStableIds: relatedStableIds));
        }

        internal static void AddAddComponentMenuFact<T>(
            List<PyralisAuthoringFact> facts,
            string stableId,
            string displayName,
            PyralisAuthoringFactKind kind,
            string summary,
            string routeRelevance,
            string[] relatedStableIds) where T : Component
        {
            AddComponentMenu attribute = typeof(T).GetCustomAttribute<AddComponentMenu>();
            if (attribute == null || string.IsNullOrWhiteSpace(attribute.componentMenu))
                return;

            string typeName = typeof(T).Name;
            facts.Add(new PyralisAuthoringFact(
                stableId,
                displayName,
                kind,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                string.Empty,
                requiredSceneComponents: kind == PyralisAuthoringFactKind.SceneComponent ? new[] { typeName } : null,
                requiredPrefabComponents: kind == PyralisAuthoringFactKind.PrefabComponent ? new[] { typeName } : null,
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Add Component",
                        PyralisAuthoringActionSurface.Inspector,
                        typeName,
                        attribute.componentMenu,
                        typeName + " is present on the selected scene object or prefab")
                },
                workIntent: "NativeComponentMenu",
                relatedStableIds: relatedStableIds));
        }

        internal static void AddRequireComponentFacts<T>(
            List<PyralisAuthoringFact> facts,
            string stableId,
            string displayName,
            string routeRelevance,
            string[] relatedStableIds) where T : Component
        {
            object[] attributes = typeof(T).GetCustomAttributes(typeof(RequireComponent), false);
            List<string> requiredComponents = new List<string>();
            for (int i = 0; i < attributes.Length; i++)
                AddRequireComponentTypes((RequireComponent)attributes[i], requiredComponents);

            if (requiredComponents.Count == 0)
                return;

            facts.Add(new PyralisAuthoringFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.PrefabComponent,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Explicit,
                typeof(T).Name + " declares required Unity components through RequireComponent metadata.",
                routeRelevance,
                string.Empty,
                requiredPrefabComponents: requiredComponents.ToArray(),
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        typeof(T).Name,
                        string.Join(", ", requiredComponents),
                        "Unity can satisfy or preserve the required component stack")
                },
                workIntent: "RequiredComponentContract",
                relatedStableIds: relatedStableIds));
        }

        internal static void AddSerializedFieldFact<T>(
            List<PyralisAuthoringFact> facts,
            string stableId,
            string displayName,
            string fieldName,
            string summary,
            string routeRelevance,
            string fieldDescription,
            string[] relatedStableIds)
        {
            FieldInfo field = typeof(T).GetField(fieldName, SerializedFieldFlags);
            if (field == null)
                return;

            bool isUnitySerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
            if (!isUnitySerialized)
                return;

            facts.Add(new PyralisAuthoringFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.AssignmentField,
                PyralisAuthoringFactSourceKind.Convention,
                PyralisAuthoringConfidence.ConventionDerived,
                summary,
                routeRelevance,
                string.Empty,
                assignmentFields: new[] { fieldDescription },
                nativeActions: new[]
                {
                    new PyralisAuthoringNativeAction(
                        "Assign",
                        PyralisAuthoringActionSurface.Inspector,
                        typeof(T).Name,
                        field.Name,
                        "the serialized Inspector field holds the user's authored value")
                },
                workIntent: "InspectorFieldConvention",
                relatedStableIds: relatedStableIds));
        }

        private static void AddRequireComponentTypes(RequireComponent attribute, List<string> requiredComponents)
        {
            AddRequireComponentType(attribute, "m_Type0", requiredComponents);
            AddRequireComponentType(attribute, "m_Type1", requiredComponents);
            AddRequireComponentType(attribute, "m_Type2", requiredComponents);
        }

        private static void AddRequireComponentType(RequireComponent attribute, string fieldName, List<string> requiredComponents)
        {
            FieldInfo field = typeof(RequireComponent).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return;

            System.Type type = field.GetValue(attribute) as System.Type;
            if (type == null)
                return;

            string typeName = type.Name;
            if (!requiredComponents.Contains(typeName))
                requiredComponents.Add(typeName);
        }
    }

    public sealed class PyralisSprite2DConventionAuthoringFactProvider : IAuthoringConventionFactProvider
    {
        public IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();

            PyralisConventionAuthoringFacts.AddCreateAssetMenuFact<ParticipantDefinition>(
                facts,
                "reflection.create-asset-menu.participant-definition",
                "ParticipantDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for player, NPC, seat, or participant defaults.",
                "2D pawn movement route",
                new[] { "setup.assign-participant-pawn", "inspector.participant-definition.default-pawn", "capability.2d-pawn-movement" });
            PyralisConventionAuthoringFacts.AddCreateAssetMenuFact<PawnDefinition>(
                facts,
                "reflection.create-asset-menu.pawn-definition",
                "PawnDefinition Create Menu",
                PyralisAuthoringFactKind.Definition,
                "Project-window creation path for authored pawn defaults.",
                "2D pawn movement route",
                new[] { "setup.assign-participant-pawn", "inspector.pawn-definition.pawn-prefab", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddCreateAssetMenuFact<InputProfile>(
                facts,
                "reflection.create-asset-menu.input-profile",
                "InputProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for input role/action-name translation profiles. Beginner setups usually point this profile at Assets/InputSystem_Actions.inputactions.",
                "2D pawn movement route",
                new[] { "setup.assign-input-profile", "inspector.input-profile.gameplay-action-names", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddCreateAssetMenuFact<PawnMovementProfile>(
                facts,
                "reflection.create-asset-menu.pawn-movement-profile",
                "PawnMovementProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for pawn movement feel, speed, acceleration, dash, and jump tuning.",
                "2D pawn movement route",
                new[] { "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddCreateAssetMenuFact<PawnPresentationProfile>(
                facts,
                "reflection.create-asset-menu.pawn-presentation-profile",
                "PawnPresentationProfile Create Menu",
                PyralisAuthoringFactKind.Profile,
                "Project-window creation path for pawn presentation lane and visual setup choices.",
                "2D pawn movement route",
                new[] { "setup.tune-pawn-visuals-and-collision", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });

            PyralisConventionAuthoringFacts.AddAddComponentMenuFact<PawnRoot>(
                facts,
                "reflection.add-component-menu.pawn-root",
                "PawnRoot Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for the participant-owned pawn root.",
                "2D pawn movement route",
                new[] { "setup.assign-participant-pawn", "inspector.pawn-definition.pawn-prefab", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddAddComponentMenuFact<Motor2D>(
                facts,
                "reflection.add-component-menu.motor-2d",
                "Motor2D Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for the 2D pawn motor coordinator.",
                "2D pawn movement route",
                new[] { "setup.assign-participant-pawn", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddAddComponentMenuFact<Motor2DInputAdapter>(
                facts,
                "reflection.add-component-menu.motor-2d-input-adapter",
                "Motor2DInputAdapter Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for the supported neutral 2D pawn input adapter.",
                "2D pawn movement route",
                new[] { "setup.assign-input-profile", "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddAddComponentMenuFact<Pawn2DMovementComponent>(
                facts,
                "reflection.add-component-menu.pawn-2d-movement-component",
                "Pawn2DMovementComponent Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for the tunable 2D movement module.",
                "2D pawn movement route",
                new[] { "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddAddComponentMenuFact<Pawn2DPresentationComponent>(
                facts,
                "reflection.add-component-menu.pawn-2d-presentation-component",
                "Pawn2DPresentationComponent Add Component Menu",
                PyralisAuthoringFactKind.PrefabComponent,
                "Inspector Add Component path for the 2D pawn visual and presentation module.",
                "2D pawn movement route",
                new[] { "setup.tune-pawn-visuals-and-collision", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });

            PyralisConventionAuthoringFacts.AddRequireComponentFacts<Motor2D>(
                facts,
                "reflection.require-component.motor-2d",
                "Motor2D Required Components",
                "2D pawn movement route",
                new[] { "setup.assign-participant-pawn", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddRequireComponentFacts<Pawn2DMovementComponent>(
                facts,
                "reflection.require-component.pawn-2d-movement-component",
                "Pawn2DMovementComponent Required Components",
                "2D pawn movement route",
                new[] { "setup.tune-pawn-visuals-and-collision", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });

            PyralisConventionAuthoringFacts.AddSerializedFieldFact<PawnDefinition>(
                facts,
                "convention.serialized-field.pawn-definition.pawn-prefab",
                "PawnDefinition Pawn Prefab Field",
                "pawnPrefab",
                "The public pawn prefab field is a native Inspector assignment convention.",
                "2D pawn movement route",
                "PawnDefinition.pawnPrefab -> pawn prefab",
                new[] { "inspector.pawn-definition.pawn-prefab", "setup.assign-participant-pawn", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
            PyralisConventionAuthoringFacts.AddSerializedFieldFact<InputProfile>(
                facts,
                "convention.serialized-field.input-profile.gameplay-actions",
                "InputProfile Gameplay Action Rows",
                "actionBindings",
                "The gameplay action binding rows are native Inspector customization conventions layered on top of Unity's Input Actions asset.",
                "2D pawn movement route",
                "InputProfile actionBindings -> built-in/custom gameplay roles -> stock or custom project Input Action names",
                new[] { "inspector.input-profile.gameplay-action-names", "setup.assign-input-profile", "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" });

            return facts;
        }
    }
}
