using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Interaction;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorFeatureHost))]
    public sealed class ActorFeatureHostEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                title: "Inspector Field Guide: Actor Feature Host",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "ActorFeatureHost installs runtime feature prefabs declared by PawnDefinition, EnemyFeatureProfile, or other actor authoring assets. Use this Inspector for host-local runtime evidence and validation.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Feature modules are assigned by the actor definition or profile that initializes this host.",
                            "Runtime feature prefabs should contain one or more IFeatureModuleRuntime components.",
                            "An actor bootstrap such as PawnRoot, Motor3D, EnemyAI, or custom code must call InitializeFeatures."
                        }),
                    new PyralisGuideSection(
                        "Customize Here",
                        null,
                        new[]
                        {
                            "Keep one host on the actor root.",
                            "Use Authoring to decide which actor definition/profile owns the feature list."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetHostMessages((ActorFeatureHost)target), "ActorFeatureHost is ready for actor feature initialization.");
            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetHostMessages(ActorFeatureHost host)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = host != null ? host.gameObject : null;

            if (root != null && root.GetComponents<ActorFeatureHost>().Length > 1)
                messages.Add(PyralisGuideIssue.Required("This actor has multiple ActorFeatureHost components. Keep one host on the actor root."));

            if (root != null
                && root.GetComponent("PawnRoot") == null
                && root.GetComponent("Motor3D") == null
                && root.GetComponent("EnemyAI") == null)
            {
                messages.Add(PyralisGuideIssue.Optional("No known actor bootstrap component found. This is fine for custom actors, but something must call InitializeFeatures at runtime."));
            }

            if (Application.isPlaying && host != null && host.InstalledModules.Count == 0)
                messages.Add(PyralisGuideIssue.Optional("No feature modules are currently installed. Check the actor definition/profile feature list if features were expected."));

            return messages;
        }
    }

    [CustomEditor(typeof(ActorCombatReactionFeatureRuntime))]
    public sealed class ActorCombatReactionFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ActorFeatureRuntimeEditorUtility.DrawFeatureRuntimeGuide(
                "Inspector Field Guide: Combat Reaction Runtime",
                "ActorCombatReactionFeatureRuntime adds guard, parry, damage modification, hurt/stagger locks, and combat reaction feedback for an actor.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorCombatReactionFeatureRuntime)target),
                "Pair the actor root with HealthComponent, a movement/reaction responder, KnockbackReceiver when knockback is used, ActorAnimationDriver for guard/hurt/stagger signals, and impact feedback sinks when hit pause or camera shake is enabled.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(ActorFeatureRuntimeEditorUtility.GetReactionMessages(serializedObject), "Combat reaction runtime is ready for FeatureModuleDefinition wiring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorStatusEffectFeatureRuntime))]
    public sealed class ActorStatusEffectFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ActorFeatureRuntimeEditorUtility.DrawFeatureRuntimeGuide(
                "Inspector Field Guide: Status Effect Runtime",
                "ActorStatusEffectFeatureRuntime applies timed status effects, damage/heal ticks, action locks, movement modifiers, combat multipliers, and shield-style damage modifiers.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorStatusEffectFeatureRuntime)target),
                "Pair the actor root with HealthComponent plus movement, combat, and health modifier receivers when the selected statuses need those effects.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(ActorFeatureRuntimeEditorUtility.GetProfileRuntimeMessages(serializedObject, "statusProfile", PyralisAuthoringContractGuideText.RequiredProfileName((ActorStatusEffectFeatureRuntime)target, "ActorStatusEffectProfile")), "Status effect runtime is ready for FeatureModuleDefinition wiring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorInteractionFeatureRuntime))]
    public sealed class ActorInteractionFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ActorFeatureRuntimeEditorUtility.DrawFeatureRuntimeGuide(
                "Inspector Field Guide: Interaction Runtime",
                "ActorInteractionFeatureRuntime receives interaction input and delegates it to IActorInteractionHandler components on the runtime prefab.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorInteractionFeatureRuntime)target),
                "Sprite2D actors usually also need ActorInteractionInputBridge2D on the actor root so pawn input can reach this feature.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(ActorFeatureRuntimeEditorUtility.GetInteractionMessages(serializedObject, (ActorInteractionFeatureRuntime)target), "Interaction runtime is ready for FeatureModuleDefinition wiring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorFeedbackFeatureRuntime))]
    public sealed class ActorFeedbackFeatureRuntimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ActorFeatureRuntimeEditorUtility.DrawFeatureRuntimeGuide(
                "Inspector Field Guide: Feedback Runtime",
                "ActorFeedbackFeatureRuntime listens to actor health and publishes damage, heal, death, status, score, combo, parry, stagger, guard-break, and finisher events to feedback receivers.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorFeedbackFeatureRuntime)target),
                "Add at least one IActorFeedbackReceiver, such as ActorFloatingFeedbackReceiver or ParticipantFeedbackRelay, in the actor hierarchy.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(ActorFeatureRuntimeEditorUtility.GetFeedbackMessages(serializedObject, (ActorFeedbackFeatureRuntime)target), "Feedback runtime is ready for FeatureModuleDefinition wiring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorFloatingFeedbackReceiver))]
    public sealed class ActorFloatingFeedbackReceiverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                title: "Inspector Field Guide: Floating Feedback Receiver",
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        "ActorFloatingFeedbackReceiver renders world-space damage, heal, score, combo, status, parry, stagger, guard-break, and finisher popups from actor feedback events. Use this Inspector for popup categories, sinks, camera, offsets, and timing.",
                        manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Feedback_Setup.md")),
                    new PyralisGuideSection(
                        "Required Fields",
                        null,
                        new[]
                        {
                            "Assign Damage Number Sink to DamageNumberSpawner or another IDamageNumberSink when damage/heal number categories are enabled.",
                            "Assign Popup Camera when world-space popups should face a specific gameplay camera.",
                            "Tune offsets and popup timing after the actor scale and camera framing are stable."
                        }),
                    new PyralisGuideSection(
                        "Customize Here",
                        null,
                        new[]
                        {
                            "Enable at least one feedback category.",
                            "Use shorter popup lifetimes for actors that take frequent damage.",
                            "For HUD-only games, prefer participant HUD presenters over world-space popups."
                        })
                });

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(ActorFeatureRuntimeEditorUtility.GetFloatingFeedbackMessages(serializedObject), "Floating feedback receiver is ready for actor feedback events.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class ActorFeatureRuntimeEditorUtility
    {
        public static void DrawFeatureRuntimeGuide(string title, string whatThisIs, string featureModuleSetup, string actorSetup)
        {
            PyralisInspectorGuide.DrawFieldGuide(
                title: title,
                defaultOpen: false,
                sections: new[]
                {
                    new PyralisGuideSection(
                        "What This Is",
                        whatThisIs,
                        manualPath: PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                    new PyralisGuideSection(
                        "Feature Module Fields",
                        null,
                        new[]
                        {
                            featureModuleSetup,
                            "Set Runtime Prefab to a prefab containing this runtime component.",
                            "Keep Enabled By Default on unless a setup route or game mode toggles the feature manually.",
                            "Use Supported Presentation Modes to prevent 2D-only or 3D-only features from installing on the wrong actor type."
                        }),
                    new PyralisGuideSection(
                        "Actor Fields",
                        null,
                        new[] { actorSetup }),
                    new PyralisGuideSection(
                        "Customize Here",
                        null,
                        new[]
                        {
                            "Use Authoring to decide which FeatureModuleDefinition belongs to the actor route.",
                            "Keep the FeatureModuleDefinition profile type matched with this runtime's expected profile.",
                            "Keep actor-root components available when this runtime resolves them from ActorFeatureContext."
                        })
                });
        }

        public static List<PyralisGuideIssue> GetProfileRuntimeMessages(SerializedObject serializedObject, string profileFieldName, string expectedProfileName)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty(profileFieldName);

            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional(profile.displayName + " is empty. This is expected when FeatureModuleDefinition provides the " + expectedProfileName + " at runtime."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetReactionMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = GetProfileRuntimeMessages(serializedObject, "reactionProfile", "EnemyReactionProfile or ActorCombatReactionProfile");
            RequireOptionalInterface<IHitPauseSink>(serializedObject, messages, "hitPauseSink", "Hit Pause Sink", "IHitPauseSink");
            RequireOptionalInterface<ICameraShakeSink>(serializedObject, messages, "cameraShakeSink", "Camera Shake Sink", "ICameraShakeSink");
            return messages;
        }

        public static List<PyralisGuideIssue> GetInteractionMessages(SerializedObject serializedObject, ActorInteractionFeatureRuntime runtime)
        {
            List<PyralisGuideIssue> messages = GetProfileRuntimeMessages(serializedObject, "interactionProfile", "InteractionFeatureProfile");
            if (runtime != null && !HasHandlerBesideSelf(runtime.gameObject, runtime))
                messages.Add(PyralisGuideIssue.Recommended("No separate IActorInteractionHandler was found on this runtime prefab. Add a handler when interaction should do more than trigger animation."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetFeedbackMessages(SerializedObject serializedObject, ActorFeedbackFeatureRuntime runtime)
        {
            List<PyralisGuideIssue> messages = GetProfileRuntimeMessages(serializedObject, "feedbackProfile", "ActorFeedbackProfile");
            if (runtime != null && !HasComponentImplementing(runtime.gameObject, "NeonBlack.Gameplay.Features.Feedback.IActorFeedbackReceiver"))
                messages.Add(PyralisGuideIssue.Optional("No IActorFeedbackReceiver was found on this runtime prefab. This is fine if receivers live on the actor root or child visuals."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetFloatingFeedbackMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            bool anyCategory =
                serializedObject.FindProperty("showDamageNumbers")?.boolValue == true
                || serializedObject.FindProperty("showHealNumbers")?.boolValue == true
                || serializedObject.FindProperty("showScorePopups")?.boolValue == true
                || serializedObject.FindProperty("showComboPopups")?.boolValue == true
                || serializedObject.FindProperty("showStatusPopups")?.boolValue == true
                || serializedObject.FindProperty("showCombatAlertPopups")?.boolValue == true;

            if (!anyCategory)
                messages.Add(PyralisGuideIssue.Required("Every feedback category is disabled, so this receiver will never show feedback."));

            SerializedProperty popupLifetime = serializedObject.FindProperty("popupLifetime");
            SerializedProperty popupRiseSpeed = serializedObject.FindProperty("popupRiseSpeed");
            SerializedProperty popupFontSize = serializedObject.FindProperty("popupFontSize");

            if (popupLifetime != null && popupLifetime.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Popup Lifetime must be greater than zero."));

            if (popupRiseSpeed != null && popupRiseSpeed.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Popup Rise Speed cannot be negative."));

            if (popupFontSize != null && popupFontSize.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Popup Font Size must be greater than zero."));

            SerializedProperty popupCamera = serializedObject.FindProperty("popupCamera");
            if (popupCamera != null && popupCamera.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Popup Camera is empty. Assign a gameplay camera or set it at runtime so popups billboard correctly."));

            SerializedProperty damageNumberSink = serializedObject.FindProperty("damageNumberSink");
            bool damageNumbersEnabled =
                serializedObject.FindProperty("showDamageNumbers")?.boolValue == true
                || serializedObject.FindProperty("showHealNumbers")?.boolValue == true;

            if (damageNumbersEnabled && damageNumberSink != null && damageNumberSink.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Damage Number Sink is empty. Assign DamageNumberSpawner or another IDamageNumberSink to show damage/heal numbers."));

            if (damageNumberSink != null
                && damageNumberSink.objectReferenceValue is Component sinkComponent
                && sinkComponent.GetComponent<IDamageNumberSink>() == null)
            {
                messages.Add(PyralisGuideIssue.Required("Damage Number Sink must reference a component that implements IDamageNumberSink."));
            }

            return messages;
        }

        private static bool HasHandlerBesideSelf(GameObject root, MonoBehaviour self)
        {
            MonoBehaviour[] behaviours = root.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null || ReferenceEquals(behaviours[i], self))
                    continue;

                if (ImplementsTypeName(behaviours[i], "NeonBlack.Gameplay.Features.Interaction.IActorInteractionHandler"))
                    return true;
            }

            return false;
        }

        private static bool HasComponentImplementing(GameObject root, string interfaceName)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (ImplementsTypeName(behaviours[i], interfaceName))
                    return true;
            }

            return false;
        }

        private static bool ImplementsTypeName(MonoBehaviour behaviour, string interfaceName)
        {
            if (behaviour == null)
                return false;

            System.Type[] interfaces = behaviour.GetType().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (string.Equals(interfaces[i].FullName, interfaceName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void RequireOptionalInterface<T>(SerializedObject serializedObject, List<PyralisGuideIssue> messages, string propertyName, string displayName, string interfaceName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                return;

            if (!(property.objectReferenceValue is T))
                messages.Add(PyralisGuideIssue.Required(displayName + " must reference a component that implements " + interfaceName + "."));
        }
    }
}
