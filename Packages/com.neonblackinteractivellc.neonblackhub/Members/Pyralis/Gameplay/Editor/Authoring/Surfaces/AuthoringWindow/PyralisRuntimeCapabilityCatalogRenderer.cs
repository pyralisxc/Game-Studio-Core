using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisRuntimeCapabilityCatalogRenderer
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

        public static void Draw(GameSetupProfile setupProfile)
        {
            EditorGUILayout.LabelField("Runtime Capability Catalog", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringWindowText.DrawSemanticMiniLabel("Browse Pyralis-supported runtime setup by game goal or runtime lane. No asset or component creation happens here; each card points back to native Project, Hierarchy, Inspector, Add Component, assignment, customization, and Play Mode proof steps.");

                DrawRuntimeCapabilityCatalogByGoal(setupProfile);
                DrawRuntimeCapabilityCatalogByLane(setupProfile);
            }
        }

        private static void DrawRuntimeCapabilityCatalogByGoal(GameSetupProfile setupProfile)
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
                        setupProfile, 
                        AuthoringCapabilityRegistry.GetTooltip(cap));
                }
            }
        }

        private static void DrawRuntimeCapabilityCatalogByLane(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Browse By Runtime Lane", EditorStyles.miniBoldLabel);
            for (int i = 0; i < GuidedCapabilityLaneTags.Length; i++)
            {
                RuntimeCapabilityLaneTag tag = GuidedCapabilityLaneTags[i];
                string laneName = tag.ToString();
                IReadOnlyList<PyralisAuthoringFact> facts =
                    PyralisAuthoringSetupGraphProjection.BuildRuntimeCapabilityFactsForLane(null, tag);
                DrawRuntimeCapabilityGroup(GetLaneTagLabel(tag), "Lane", facts, setupProfile, laneName, tag);
            }
        }

        private static void DrawRuntimeCapabilityGroup(
            string title,
            string groupKind,
            IReadOnlyList<PyralisAuthoringFact> facts,
            GameSetupProfile setupProfile,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneTag = null)
        {
            string key = "Pyralis.AuthoringWindow.RuntimeCapabilityCatalog." + groupKind + "." + keySuffix;
            bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
            int count = facts != null ? facts.Count : 0;
            isOpen = EditorGUILayout.Foldout(isOpen, $"{title} ({count})", true);
            Foldouts[key] = isOpen;

            if (!isOpen || facts == null)
                return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < facts.Count; i++)
                DrawRuntimeCapabilityCard(facts[i], setupProfile, keySuffix + "." + i, laneTag);
            EditorGUI.indentLevel--;
        }

        private static void DrawRuntimeCapabilityCard(
            PyralisAuthoringFact fact,
            GameSetupProfile setupProfile,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneContext)
        {
            if (fact == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string status = GetRuntimeCapabilityStatus(fact, setupProfile, laneContext);
                EditorGUILayout.LabelField(fact.DisplayName, status, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(fact.Summary, EditorStyles.wordWrappedMiniLabel);

                string key = "Pyralis.AuthoringWindow.RuntimeCapabilityCard." + fact.StableId + "." + keySuffix;
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

        private static string GetRuntimeCapabilityStatus(PyralisAuthoringFact fact, GameSetupProfile setupProfile, RuntimeCapabilityLaneTag? laneContext)
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

            if (PyralisReflectiveContractSolver.IsSatisfied(fact, out string message, out _))
            {
                return "[Ready] " + message;
            }

            if (fact.RequiredSceneComponents != null && fact.RequiredSceneComponents.Length > 0)
            {
                return "[Needs Scene Setup] " + message;
            }

            return "Guide-only option";
        }

        public static bool HasAnyRuntimeCapability(GameSetupProfile setupProfile)
        {
            return setupProfile != null
                && setupProfile.runtimeCapabilities != null
                && setupProfile.runtimeCapabilities.Length > 0;
        }

        public static RuntimeCapabilitySelection GetCapabilitySelection(GameSetupProfile setupProfile, RuntimeCapabilityFamily family)
        {
            if (setupProfile == null || setupProfile.runtimeCapabilities == null)
                return null;

            for (int i = 0; i < setupProfile.runtimeCapabilities.Length; i++)
            {
                RuntimeCapabilitySelection selection = setupProfile.runtimeCapabilities[i];
                if (selection != null && selection.capabilityFamily == family)
                    return selection;
            }

            return null;
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
