using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameSetupProfile))]
    public class GameSetupProfileEditor : UnityEditor.Editor
    {
        private int _capabilityToAddIndex;
        private static readonly RuntimeCapabilityFamily[] AddableCapabilityFamilies =
        {
            RuntimeCapabilityFamily.CharacterPawnGameplay,
            RuntimeCapabilityFamily.Combat,
            RuntimeCapabilityFamily.GunsProjectiles,
            RuntimeCapabilityFamily.ActionTargeting,
            RuntimeCapabilityFamily.BoardCardTabletop,
            RuntimeCapabilityFamily.CameraInput,
            RuntimeCapabilityFamily.AnimationPresentation,
            RuntimeCapabilityFamily.ScoringObjectives,
            RuntimeCapabilityFamily.ProceduralGeneration,
            RuntimeCapabilityFamily.Networking,
            RuntimeCapabilityFamily.PlatformCore,
            RuntimeCapabilityFamily.Custom
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("setupName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("summary"));
            DrawRuntimeCapabilities(serializedObject.FindProperty("runtimeCapabilities"));

            bool changed = serializedObject.ApplyModifiedProperties();
            GameSetupProfile profile = (GameSetupProfile)target;
            profile.Sanitize();
            if (changed)
                EditorUtility.SetDirty(profile);

            serializedObject.UpdateIfRequiredOrScript();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("runtimePatterns"), includeChildren: true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("setupNotes"));

            DrawValidation(profile);
            PyralisInspectorHandoff.DrawAuthoringButton();
            if (serializedObject.ApplyModifiedProperties())
            {
                profile.Sanitize();
                EditorUtility.SetDirty(profile);
            }
        }

        private void DrawRuntimeCapabilities(SerializedProperty capabilities)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Capabilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Follow the Intent tab's selected guidance, then choose the runtime capability this setup should support. Each row can link to an existing RuntimePatternDefinition recipe so Authoring can explain the Definitions, Profiles, Components, and scene objects needed for this route.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Capability To Add", "Pick the next route capability this setup should support."), GUILayout.Width(120));
                _capabilityToAddIndex = EditorGUILayout.Popup(_capabilityToAddIndex, BuildCapabilityPopupLabels());
                RuntimeCapabilityFamily? selectedFamily = GetCapabilityToAdd();
                bool alreadySelected = selectedFamily.HasValue && HasCapabilityFamily(capabilities, selectedFamily.Value);
                using (new EditorGUI.DisabledScope(!selectedFamily.HasValue || alreadySelected))
                {
                    string buttonLabel = alreadySelected ? "Already Added" : "Add Capability";
                    if (GUILayout.Button(new GUIContent(buttonLabel, "Adds the chosen capability family as a creator-facing row. Pattern recipes stay manual so the profile follows the selected Intent guidance without preselecting a setup."), GUILayout.Width(130)))
                        AddCapability(capabilities, selectedFamily.Value);
                }
            }

            if (GetCapabilityToAdd() is RuntimeCapabilityFamily familyToPreview)
            {
                string preview = GetCapabilityPreview(familyToPreview);
                int matchingPatterns = CountPatterns(familyToPreview);
                EditorGUILayout.HelpBox($"{preview}\n\nExisting recipe assets for this capability: {matchingPatterns}", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Read the Intent tab's current guidance, then choose the capability family that matches the route you are authoring. This inspector will not pick a route or recipe for the creator.", MessageType.None);
            }

            if (capabilities == null || capabilities.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No runtime capabilities are selected yet. Use the Intent tab to decide the first capability, then add only the route pieces the project is actually ready to customize.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4f);
            for (int i = 0; i < capabilities.arraySize; i++)
                DrawCapabilityRow(capabilities, i);
        }

        private static void DrawCapabilityRow(SerializedProperty capabilities, int index)
        {
            SerializedProperty element = capabilities.GetArrayElementAtIndex(index);
            SerializedProperty familyProperty = element.FindPropertyRelative("capabilityFamily");
            SerializedProperty patternProperty = element.FindPropertyRelative("patternDefinition");
            SerializedProperty requiredProperty = element.FindPropertyRelative("requiredForFirstProof");
            SerializedProperty notesProperty = element.FindPropertyRelative("creatorNotes");

            RuntimeCapabilityFamily family = (RuntimeCapabilityFamily)familyProperty.enumValueIndex;
            RuntimePatternDefinition pattern = patternProperty.objectReferenceValue as RuntimePatternDefinition;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Capability {index + 1}: {GetCapabilityLabel(family)}", EditorStyles.boldLabel);
                if (GUILayout.Button(new GUIContent("Remove", "Removes this capability from the setup profile. The RuntimePatternDefinition asset remains in the project."), GUILayout.Width(72)))
                {
                    capabilities.DeleteArrayElementAtIndex(index);
                    EditorGUILayout.EndVertical();
                    return;
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(familyProperty, new GUIContent("Route Capability", "The kind of runtime behavior this setup should guide."));
            if (EditorGUI.EndChangeCheck())
            {
                family = (RuntimeCapabilityFamily)familyProperty.enumValueIndex;
                pattern = patternProperty.objectReferenceValue as RuntimePatternDefinition;
                if (pattern == null || pattern.capabilityFamily != family)
                    patternProperty.objectReferenceValue = null;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(patternProperty, new GUIContent("Pattern Recipe", "Existing RuntimePatternDefinition asset that describes the concrete Unity assets, components, and scene objects for this capability."));
            if (EditorGUI.EndChangeCheck())
            {
                pattern = patternProperty.objectReferenceValue as RuntimePatternDefinition;
                if (pattern != null)
                    familyProperty.enumValueIndex = (int)pattern.capabilityFamily;
            }

            pattern = patternProperty.objectReferenceValue as RuntimePatternDefinition;
            family = (RuntimeCapabilityFamily)familyProperty.enumValueIndex;
            DrawPatternStatus(family, pattern);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(requiredProperty, new GUIContent("First Proof Gate", "Treat this capability as required for the first live proof of the route."));
                using (new EditorGUI.DisabledScope(pattern == null))
                {
                    if (GUILayout.Button(new GUIContent("Inspect Pattern", "Selects the assigned recipe asset so its inspector can show missing fields and setup notes. Assign the recipe through Pattern Recipe first."), GUILayout.Width(112)))
                    {
                        Selection.activeObject = pattern;
                        EditorGUIUtility.PingObject(pattern);
                    }
                }
            }

            EditorGUILayout.PropertyField(notesProperty, new GUIContent("Creator Notes", "Optional notes about how this project wants to use this capability."));
            EditorGUILayout.EndVertical();
        }

        private static void DrawPatternStatus(RuntimeCapabilityFamily family, RuntimePatternDefinition pattern)
        {
            if (pattern == null)
            {
                int count = CountPatterns(family);
                MessageType type = count > 0 ? MessageType.Warning : MessageType.Info;
                string message = count > 0
                    ? "Matching RuntimePatternDefinition assets exist. Assign one in Pattern Recipe only if it describes this route; otherwise create a proof-local RuntimePatternDefinition for the route you are authoring."
                    : "No matching RuntimePatternDefinition asset exists yet. Create one only when the existing capability recipes cannot describe this route.";
                EditorGUILayout.HelpBox(message, type);
                return;
            }

            if (pattern.capabilityFamily != family)
            {
                EditorGUILayout.HelpBox($"This recipe is tagged {pattern.capabilityFamily}, but the row is {family}. Pick a matching recipe or change the route capability.", MessageType.Warning);
                return;
            }

            string description = !string.IsNullOrWhiteSpace(pattern.description)
                ? pattern.description
                : RuntimePatternAuthoringText.GetSuggestedDescription(pattern);
            string setupNotes = !string.IsNullOrWhiteSpace(pattern.setupNotes)
                ? pattern.setupNotes
                : RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);

            EditorGUILayout.HelpBox($"{GetPatternLabel(pattern)}\n{description}", MessageType.Info);
            if (!string.IsNullOrWhiteSpace(setupNotes))
                EditorGUILayout.HelpBox(setupNotes, MessageType.None);
        }

        private static string GetCapabilityPreview(RuntimeCapabilityFamily family)
        {
            switch (family)
            {
                case RuntimeCapabilityFamily.CharacterPawnGameplay:
                    return "Adds a pawn/player route: participant definition, pawn definition, input profile, pawn prefab, spawn point, and camera guidance.";
                case RuntimeCapabilityFamily.ActionTargeting:
                    return "Adds an action-selection route: available actions, targeting/selection rules, and UI or world selection guidance.";
                case RuntimeCapabilityFamily.Combat:
                    return "Adds a combat route: action definitions, hit/damage rules, pawn combat profiles, health/hitbox objects, and feedback guidance.";
                case RuntimeCapabilityFamily.GunsProjectiles:
                    return "Adds a projectile route: projectile definitions, fire modes, launch surfaces, impact feedback, and scene or prefab handoffs.";
                case RuntimeCapabilityFamily.ProceduralGeneration:
                    return "Adds a procedural route: generation definitions, placement profiles, scene evidence, and validation handoffs.";
                case RuntimeCapabilityFamily.BoardCardTabletop:
                    return "Adds a board/card/tabletop route: seats, board or card state, turns, legal actions, selection surfaces, and no-pawn guidance.";
                case RuntimeCapabilityFamily.AnimationPresentation:
                    return "Adds an animation/presentation route: animation definitions, presentation profiles, animator signals, visual feedback, and prefab handoffs.";
                case RuntimeCapabilityFamily.ScoringObjectives:
                    return "Adds a scoring/objectives route: score services, timers, resources, objectives, win/loss state, and HUD handoffs.";
                case RuntimeCapabilityFamily.CameraInput:
                    return "Adds a camera/cursor/world route: camera rig profile, cursor/world targeting, bounds, and scene framing guidance.";
                case RuntimeCapabilityFamily.Networking:
                    return "Adds a networking/ownership route: authority rules, participant ownership, spawn ownership, and network validation guidance.";
                case RuntimeCapabilityFamily.PlatformCore:
                    return "Adds platform-core setup guidance for shared session infrastructure and baseline authoring expectations.";
                case RuntimeCapabilityFamily.Custom:
                    return "Adds a custom runtime route. Use this when the creator will define the required assets, scene objects, and proof steps.";
                default:
                    return "Adds the selected runtime route so Authoring can explain the assets and scene objects needed for that style of game.";
            }
        }

        private static void AddCapability(SerializedProperty capabilities, RuntimeCapabilityFamily family)
        {
            int index = capabilities.arraySize;
            capabilities.InsertArrayElementAtIndex(index);
            SerializedProperty element = capabilities.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("capabilityFamily").enumValueIndex = (int)family;
            element.FindPropertyRelative("patternDefinition").objectReferenceValue = null;
            element.FindPropertyRelative("requiredForFirstProof").boolValue = false;
            element.FindPropertyRelative("creatorNotes").stringValue = string.Empty;
        }

        private RuntimeCapabilityFamily? GetCapabilityToAdd()
        {
            if (_capabilityToAddIndex <= 0)
                return null;

            int familyIndex = _capabilityToAddIndex - 1;
            if (familyIndex < 0 || familyIndex >= AddableCapabilityFamilies.Length)
                return null;

            return AddableCapabilityFamilies[familyIndex];
        }

        private static string[] BuildCapabilityPopupLabels()
        {
            string[] labels = new string[AddableCapabilityFamilies.Length + 1];
            labels[0] = "Read Intent, Then Choose";
            for (int i = 0; i < AddableCapabilityFamilies.Length; i++)
                labels[i + 1] = GetCapabilityLabel(AddableCapabilityFamilies[i]);

            return labels;
        }

        private static bool HasCapabilityFamily(SerializedProperty capabilities, RuntimeCapabilityFamily family)
        {
            if (capabilities == null)
                return false;

            for (int i = 0; i < capabilities.arraySize; i++)
            {
                SerializedProperty element = capabilities.GetArrayElementAtIndex(i);
                SerializedProperty familyProperty = element.FindPropertyRelative("capabilityFamily");
                if (familyProperty.enumValueIndex == (int)family)
                    return true;
            }

            return false;
        }

        private static int CountPatterns(RuntimeCapabilityFamily family)
        {
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:RuntimePatternDefinition");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RuntimePatternDefinition pattern = AssetDatabase.LoadAssetAtPath<RuntimePatternDefinition>(path);
                if (pattern != null && pattern.capabilityFamily == family)
                    count++;
            }

            return count;
        }

        private static string GetCapabilityLabel(RuntimeCapabilityFamily family)
        {
            switch (family)
            {
                case RuntimeCapabilityFamily.CharacterPawnGameplay:
                    return "Character Pawn Gameplay";
                case RuntimeCapabilityFamily.GunsProjectiles:
                    return "Guns / Projectiles";
                case RuntimeCapabilityFamily.ActionTargeting:
                    return "Action Targeting";
                case RuntimeCapabilityFamily.BoardCardTabletop:
                    return "Board / Card / Tabletop";
                case RuntimeCapabilityFamily.CameraInput:
                    return "Camera / Input";
                case RuntimeCapabilityFamily.AnimationPresentation:
                    return "Animation / Presentation";
                case RuntimeCapabilityFamily.ScoringObjectives:
                    return "Scoring / Objectives";
                case RuntimeCapabilityFamily.ProceduralGeneration:
                    return "Procedural Generation";
                default:
                    return family.ToString();
            }
        }

        private static string GetPatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "<None>";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;

            return pattern.name;
        }

        private static void DrawValidation(GameSetupProfile profile)
        {
            List<string> issues = profile.GetValidationIssues();
            PyralisInspectorGuide.DrawValidationIssues(issues, "Game setup profile has runtime capabilities and recipe patterns.");
        }
    }
}
