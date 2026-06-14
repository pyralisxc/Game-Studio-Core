using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Runtime;
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public abstract class PyralisEditorTestSupport
    {
        protected static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        protected static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"Expected serialized property `{propertyName}` on {target}.");
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void AssertNoMojibake(string source, string path)
        {
            string[] mojibakeTokens =
            {
                "\u00C3\u00A2",
                "\u00C3\u0192",
                "\u00C3\u0082",
                "\u00E2",
                "\uFFFD",
                "\u00EF\u00BF\u00BD",
                "\u045E",
                "\u0459",
                "\u0412",
            };

            for (int i = 0; i < mojibakeTokens.Length; i++)
                Assert.That(source.Contains(mojibakeTokens[i]), Is.False, $"Unexpected mojibake token `{mojibakeTokens[i]}` in {path}");
        }

        protected static bool UsesSharedGuide(string source)
        {
            return source.Contains("PyralisInspectorGuide.DrawGuide")
                || source.Contains("PyralisInspectorGuide.DrawFieldGuide")
                || source.Contains("PyralisInspectorHandoff.DrawAuthoringButton");
        }

        protected static RuntimePatternDefinition CreateRuntimePattern(
            string patternId,
            string displayName,
            RuntimeCapabilityFamily family,
            ParticipantEmbodimentRequirement embodiment,
            params RuntimeControlSurface[] controlSurfaces)
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = patternId;
            pattern.displayName = displayName;
            pattern.description = displayName + " test description.";
            pattern.setupNotes = displayName + " test setup notes.";
            pattern.capabilityFamily = family;
            pattern.participantEmbodiment = embodiment;
            pattern.supportedControlSurfaces = controlSurfaces;
            return pattern;
        }

        protected static AnimatorController CreateTestAnimatorController(string assetName)
        {
            AnimatorController controller = new AnimatorController();
            controller.name = assetName;
            return controller;
        }

        protected static void DeleteTestAnimatorController(AnimatorController controller)
        {
            if (controller == null)
                return;

            Object.DestroyImmediate(controller);
        }

        protected sealed class TestFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime
        {
            public string ModuleId => "feature.test";

            public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
            {
            }

            public void ShutdownFeature()
            {
            }
        }
    }
}
