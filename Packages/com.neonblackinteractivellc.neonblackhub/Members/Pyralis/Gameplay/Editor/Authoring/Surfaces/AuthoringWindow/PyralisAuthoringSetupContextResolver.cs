using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringSetupContextResolver
    {
        public static GameplaySessionBootstrap GetSelectedBootstrap(Object selection)
        {
            if (selection is GameplaySessionBootstrap bootstrap)
                return bootstrap;

            if (selection is GameObject gameObject)
                return gameObject.GetComponent<GameplaySessionBootstrap>() ?? gameObject.GetComponentInParent<GameplaySessionBootstrap>();

            if (selection is Component component)
                return component.GetComponent<GameplaySessionBootstrap>() ?? component.GetComponentInParent<GameplaySessionBootstrap>();

            return null;
        }

        public static Object ResolveActiveSetup(Object selection, Object selectionSetup, Object sceneFallbackSetup, Object pinnedActiveSetup, Object rememberedActiveSetup)
        {
            if (CanUseAsActiveSetup(pinnedActiveSetup))
                return GetSetupContext(pinnedActiveSetup);

            Object rememberedSetup = CanUseAsActiveSetup(rememberedActiveSetup)
                ? GetSetupContext(rememberedActiveSetup)
                : null;
            if (rememberedSetup != null && ShouldKeepRememberedSetupForLooseSelection(selection, rememberedSetup))
                return rememberedSetup;

            GameplaySessionBootstrap sceneBootstrap = GetOnlySceneBootstrap();
            if (sceneBootstrap != null && ShouldKeepRememberedSetupForLooseSelection(selection, sceneBootstrap))
                return sceneBootstrap;

            if (selectionSetup != null)
                return selectionSetup;

            if (sceneFallbackSetup != null)
                return sceneFallbackSetup;

            if (rememberedSetup != null)
                return rememberedSetup;

            return sceneBootstrap;
        }

        private static bool ShouldKeepRememberedSetupForLooseSelection(Object selection, Object rememberedSetup)
        {
            if (selection == null || rememberedSetup == null)
                return false;

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(rememberedSetup);
            SessionDefinition rememberedSession = GetSelectedSession(rememberedSetup, bootstrap);
            if (selection is SessionDefinition && bootstrap != null)
                return rememberedSession == null;

            if (selection is GameModeDefinition && rememberedSession != null)
                return rememberedSession.defaultGameMode == null;

            return false;
        }

        public static Object GetSceneFallbackSetup(Object selection, Object selectionSetup)
        {
            if (selection != null || selectionSetup != null)
                return null;

            return GetOnlySceneBootstrap();
        }

        public static bool CanUseAsActiveSetup(Object selection)
        {
            return GetSetupContext(selection) != null;
        }

        public static Object GetSetupContext(Object selection)
        {
            if (selection == null)
                return null;

            if (selection is GameplaySessionBootstrap)
                return selection;

            GameplaySessionBootstrap linkedBootstrap = GetBootstrapReferencingSelectedAsset(selection);
            if (linkedBootstrap != null)
                return linkedBootstrap;

            if (selection is SessionDefinition
                || selection is GameModeDefinition)
                return selection;

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(selection);
            if (bootstrap != null)
                return bootstrap;

            GameplaySessionBootstrap referencedBootstrap = GetBootstrapReferencingSelectedTransform(selection);
            if (referencedBootstrap != null)
                return referencedBootstrap;

            return null;
        }

        private static GameplaySessionBootstrap GetBootstrapReferencingSelectedAsset(Object selection)
        {
            if (selection == null)
                return null;

            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                if (bootstrap == null || !bootstrap.gameObject.scene.IsValid() || !bootstrap.gameObject.scene.isLoaded)
                    continue;

                SessionDefinition session = GetSelectedSession(null, bootstrap);
                if (SessionReferencesSelection(session, selection))
                    return bootstrap;
            }

            return null;
        }

        private static bool SessionReferencesSelection(SessionDefinition session, Object selection)
        {
            if (session == null)
                return false;

            if (selection == session
                || selection == session.defaultGameMode
                || selection == session.defaultInputProfile
                || selection == session.settingsProfile)
                return true;

            if (GameModeReferencesSelection(session.defaultGameMode, selection))
                return true;

            if (session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (ParticipantReferencesSelection(participant, selection))
                    return true;
            }

            return false;
        }

        private static bool GameModeReferencesSelection(GameModeDefinition mode, Object selection)
        {
            if (mode == null)
                return false;

            if (selection == mode
                || selection == mode.playfieldProfile
                || selection == mode.cameraRigProfile
                || selection == mode.turnOrderDefinition
                || selection == mode.boardDefinition)
                return true;

            if (mode.requiredFeatureModules != null)
            {
                for (int i = 0; i < mode.requiredFeatureModules.Length; i++)
                {
                    if (selection == mode.requiredFeatureModules[i])
                        return true;
                }
            }

            if (mode.boardTerminalConditions != null)
            {
                for (int i = 0; i < mode.boardTerminalConditions.Length; i++)
                {
                    if (selection == mode.boardTerminalConditions[i])
                        return true;
                }
            }

            return false;
        }

        private static bool ParticipantReferencesSelection(ParticipantDefinition participant, Object selection)
        {
            if (participant == null)
                return false;

            if (selection == participant
                || selection == participant.defaultPawn
                || selection == participant.inputProfile)
                return true;

            PawnDefinition pawn = participant.defaultPawn;
            return pawn != null
                && (selection == pawn.pawnPrefab
                    || selection == pawn.defaultInputProfile
                    || selection == pawn.movementProfile
                    || selection == pawn.combatProfile
                    || selection == pawn.traversalProfile
                    || selection == pawn.presentationProfile
                    || selection == pawn.animationProfile
                    || PawnReferencesFeatureModule(pawn, selection));
        }

        private static bool PawnReferencesFeatureModule(PawnDefinition pawn, Object selection)
        {
            if (pawn == null || pawn.featureModules == null)
                return false;

            for (int i = 0; i < pawn.featureModules.Length; i++)
            {
                if (selection == pawn.featureModules[i])
                    return true;
            }

            return false;
        }

        private static GameplaySessionBootstrap GetBootstrapReferencingSelectedTransform(Object selection)
        {
            Transform selectedTransform = GetSelectedTransform(selection);
            if (selectedTransform == null)
                return null;

            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
                SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
                if (spawnPoints == null || !spawnPoints.isArray)
                    continue;

                for (int i = 0; i < spawnPoints.arraySize; i++)
                {
                    if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue == selectedTransform)
                        return bootstrap;
                }
            }

            return null;
        }

        private static Transform GetSelectedTransform(Object selection)
        {
            if (selection is GameObject gameObject)
                return gameObject.transform;

            if (selection is Component component)
                return component.transform;

            return null;
        }

        private static GameplaySessionBootstrap GetOnlySceneBootstrap()
        {
            GameplaySessionBootstrap onlyBootstrap = null;
            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                if (bootstrap == null || !bootstrap.gameObject.scene.IsValid() || !bootstrap.gameObject.scene.isLoaded)
                    continue;

                if (onlyBootstrap != null)
                    return null;

                onlyBootstrap = bootstrap;
            }

            return onlyBootstrap;
        }

        public static SessionDefinition GetSelectedSession(Object selection, GameplaySessionBootstrap bootstrap)
        {
            if (selection is SessionDefinition session)
                return session;

            if (bootstrap == null)
                return null;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
        }

        public static GameModeDefinition GetSelectedMode(Object selection, SessionDefinition session)
        {
            if (selection is GameModeDefinition mode)
                return mode;

            return session != null ? session.defaultGameMode : null;
        }

    }
}
