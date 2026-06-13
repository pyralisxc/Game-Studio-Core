using NeonBlack.Gameplay.Core.Contracts;
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
            AddRequireComponentFacts<Pawn2DMovementComponent>(
                facts,
                "reflection.require-component.pawn-2d-movement-component",
                "Pawn 2D Movement Component Requirements",
                "2D pawn movement requires Unity physics body and collider components on the prefab.",
                new[] { "capability.2d-pawn-movement", "proof.1p-pawn-movement" });
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
                requiredUnitySurfaces: kind == PyralisAuthoringFactKind.UnitySurface ? new[] { typeName } : null,
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
                PyralisAuthoringFactKind.UnitySurface,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Explicit,
                typeof(T).Name + " declares required Unity components through RequireComponent metadata.",
                routeRelevance,
                string.Empty,
                requiredUnitySurfaces: requiredComponents.ToArray(),
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

}
