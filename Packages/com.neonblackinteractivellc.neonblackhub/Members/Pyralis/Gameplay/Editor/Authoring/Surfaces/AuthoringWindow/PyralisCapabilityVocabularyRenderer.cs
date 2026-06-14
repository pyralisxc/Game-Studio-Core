using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisCapabilityVocabularyRenderer
    {
        private static readonly RuntimeCapabilityLaneTag[] GuidedCapabilityLaneTags =
        {
            RuntimeCapabilityLaneTag.Sprite2D,
            RuntimeCapabilityLaneTag.Billboard2_5D,
            RuntimeCapabilityLaneTag.ThirdPerson3D,
            RuntimeCapabilityLaneTag.TabletopBoard,
            RuntimeCapabilityLaneTag.UiMenuOnly,
            RuntimeCapabilityLaneTag.CameraCursor,
            RuntimeCapabilityLaneTag.Mixed
        };
        private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

        public static void Draw()
        {
            EditorGUILayout.LabelField("Runtime Capability Reference", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("Use this as a dictionary for capability ingredients. Intent filters this vocabulary; the graph reflects readiness from gameplay contracts, serialized references, validators, and scene evidence.");

                DrawRuntimeCapabilityVocabularyByGoal();
                DrawRuntimeCapabilityVocabularyByLane();
            }
        }

        private static void DrawRuntimeCapabilityVocabularyByGoal()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Browse By Engine Spine Capability", EditorStyles.miniBoldLabel);
            foreach (AuthoringCapability cap in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if (cap == AuthoringCapability.None) continue;
                
                IReadOnlyList<PyralisAuthoringFact> facts =
                    PyralisAuthoringSetupGraphProjection.BuildRuntimeCapabilityFactsForCapability(null, cap);

                if (facts.Count > 0)
                {
                    DrawRuntimeCapabilityGroup(
                        AuthoringCapabilityRegistry.GetDisplayName(cap), 
                        "Capability", 
                        facts, 
                        AuthoringCapabilityRegistry.GetTooltip(cap));
                }
            }
        }

        private static void DrawRuntimeCapabilityVocabularyByLane()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Browse By Runtime Lane", EditorStyles.miniBoldLabel);
            for (int i = 0; i < GuidedCapabilityLaneTags.Length; i++)
            {
                RuntimeCapabilityLaneTag tag = GuidedCapabilityLaneTags[i];
                string laneName = tag.ToString();
                IReadOnlyList<PyralisAuthoringFact> facts =
                    PyralisAuthoringSetupGraphProjection.BuildRuntimeCapabilityFactsForLane(null, tag);
                DrawRuntimeCapabilityGroup(GetLaneTagLabel(tag), "Lane", facts, laneName, tag);
            }
        }

        private static void DrawRuntimeCapabilityGroup(
            string title,
            string groupKind,
            IReadOnlyList<PyralisAuthoringFact> facts,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneTag = null)
        {
            string key = "Pyralis.AuthoringWindow.RuntimeCapabilityVocabulary." + groupKind + "." + keySuffix;
            bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
            int count = facts != null ? facts.Count : 0;
            isOpen = EditorGUILayout.Foldout(isOpen, $"{title} ({count})", true);
            Foldouts[key] = isOpen;

            if (!isOpen || facts == null)
                return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < facts.Count; i++)
                DrawPyralisCapabilityVocabularyCard(facts[i], keySuffix + "." + i, laneTag);
            EditorGUI.indentLevel--;
        }

        private static void DrawPyralisCapabilityVocabularyCard(
            PyralisAuthoringFact fact,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneContext)
        {
            if (fact == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string status = GetRuntimeCapabilityStatus(fact, laneContext);
                EditorGUILayout.LabelField(fact.DisplayName, status, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(fact.Summary, EditorStyles.wordWrappedMiniLabel);

                string key = "Pyralis.AuthoringWindow.PyralisCapabilityVocabularyCard." + fact.StableId + "." + keySuffix;
                bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Native Setup Guide", true);
                Foldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Relevance", fact.RouteRelevance);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Definitions", fact.RequiredDefinitions);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Profiles", fact.RequiredProfiles);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Scene Components", fact.RequiredSceneComponents);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Unity Surfaces", fact.RequiredUnitySurfaces);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Assignment Fields", fact.AssignmentFields);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Customization Moments", fact.CustomizationMoments);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Can Wait", fact.CanWait);
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Proof", fact.FirstProof);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Common Next Capabilities", fact.RelatedStableIds);
                EditorGUI.indentLevel--;
            }
        }

        private static string GetRuntimeCapabilityStatus(PyralisAuthoringFact fact, RuntimeCapabilityLaneTag? laneContext)
        {
            if (fact == null)
                return "Unknown";

            if (laneContext.HasValue)
            {
                string laneName = laneContext.Value.ToString();
                if (fact.IsExplicitlyUnsupported(laneName))
                    return "Explicitly unsupported for this lane";
                
                if (!fact.HasLane(laneName))
                    return "Available in Pyralis, but not explicitly relevant to this lane";
            }

            return "Vocabulary";
        }

        private static RuntimeCapabilityFamily[] BuildRuntimeFamilies(PyralisAuthoringFact fact)
        {
            if (fact == null)
                return System.Array.Empty<RuntimeCapabilityFamily>();

            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors = PyralisAuthoringCapabilityDescriptorRegistry.All;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null)
                    continue;

                bool sameStableId = string.Equals(descriptor.StableId, fact.StableId, System.StringComparison.Ordinal);
                bool sameCapability = fact.Capability != AuthoringCapability.None
                    && (descriptor.Capability & fact.Capability) != 0;

                if ((sameStableId || sameCapability) && !families.Contains(descriptor.Family))
                    families.Add(descriptor.Family);
            }

            return families.ToArray();
        }

        private static string GetLaneTagLabel(RuntimeCapabilityLaneTag tag)
        {
            return tag switch
            {
                RuntimeCapabilityLaneTag.Sprite2D => "Sprite2D",
                RuntimeCapabilityLaneTag.Billboard2_5D => "Billboard2_5D",
                RuntimeCapabilityLaneTag.ThirdPerson3D => "Rigged3D",
                RuntimeCapabilityLaneTag.TabletopBoard => "Tabletop / No Pawn",
                RuntimeCapabilityLaneTag.UiMenuOnly => "UI / Menu",
                RuntimeCapabilityLaneTag.CameraCursor => "Camera / Cursor",
                RuntimeCapabilityLaneTag.Mixed => "Networked",
                _ => tag.ToString()
            };
        }

    }
}
