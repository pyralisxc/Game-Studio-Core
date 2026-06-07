using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Rpg.UI;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ParticipantFeedbackRelay))]
    public sealed class ParticipantFeedbackRelayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Participant Feedback Relay",
                new PyralisGuideSection(
                    "What This Is",
                    "ParticipantFeedbackRelay converts actor feedback events into participant-scoped HUD feedback messages for combo, score, status, damage, heal, and combat alerts.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Feedback_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a participant pawn or a child of the pawn.",
                        "Use ActorFeedbackFeatureRuntime or another IActorFeedbackPublisher to send actor feedback events.",
                        "Keep ParticipantFeedbackService registered in the gameplay session so HUD presenters can subscribe.",
                        "Add ParticipantFeedbackHudPresenter on the HUD canvas to display the relayed messages."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this on a global scene manager, because participant lookup expects a pawn hierarchy.",
                        "Do not use this without participant registration; it cannot publish a message without a ParticipantHandle.",
                        "Do not add duplicate relays under the same actor unless duplicate HUD messages are intended."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRelayMessages((ParticipantFeedbackRelay)target), "ParticipantFeedbackRelay is ready for actor-to-HUD feedback routing.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ParticipantHealthPanel))]
    public sealed class ParticipantHealthPanelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Participant Health Panel",
                new PyralisGuideSection(
                    "What This Is",
                    "ParticipantHealthPanel renders one health surface: a TextMeshPro health label, an Image fill bar, or both. ParticipantHealthHudBinder feeds it with the tracked pawn health state.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this under a HUD canvas or reusable participant HUD prefab.",
                        "Assign Health Label, Health Fill Image, or both.",
                        "Use Fill Image Type Filled when the image should behave like a health bar.",
                        "Let ParticipantHealthHudBinder discover this panel, or assign it directly in Health Panels."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave both health output fields empty.",
                        "Do not tint by health unless the fill image and gradient are tuned for readable colors.",
                        "Do not put gameplay health logic in this panel; it should only present health state."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetHealthPanelMessages(serializedObject), "ParticipantHealthPanel is ready for HUD health presentation.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ParticipantTimedTextPanel))]
    public sealed class ParticipantTimedTextPanelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Participant Timed Text Panel",
                new PyralisGuideSection(
                    "What This Is",
                    "ParticipantTimedTextPanel shows a short-lived TextMeshPro label for combo, status, score, or combat alert HUD messages.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this under a HUD canvas where temporary feedback should appear.",
                        "Assign Label to the TextMeshProUGUI object that should be shown and hidden.",
                        "Set Default Display Time to the fallback duration for direct ShowText calls.",
                        "Assign this panel to a ParticipantFeedbackHudPresenter panel array, or name it with combo/status/score/combat so the presenter can discover it."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Label empty.",
                        "Do not use long display times for frequent events like score pickups.",
                        "Do not use one panel for multiple categories if overlapping messages need to be visible at the same time."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetTimedTextPanelMessages(serializedObject), "ParticipantTimedTextPanel is ready for temporary HUD messages.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ParticipantFeedbackHudPresenter))]
    public sealed class ParticipantFeedbackHudPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            FeedbackHudEditorUtility.DrawParticipantHudGuide(
                "Inspector Field Guide: Participant Feedback HUD Presenter",
                "ParticipantFeedbackHudPresenter listens to ParticipantFeedbackService and displays participant-scoped combo, status, score, and combat alert messages.",
                new[]
                {
                    "Place this on a HUD canvas object for one participant view.",
                    "Assign direct labels or reusable ParticipantTimedTextPanel arrays for each feedback category you want visible.",
                    "Use Primary Participant for single-player HUDs; use Participant Seat for split-screen or shared multi-player HUDs.",
                    "Keep ParticipantFeedbackService registered by the gameplay session."
                },
                "Do not leave every label and panel array empty.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetFeedbackPresenterMessages(serializedObject), "ParticipantFeedbackHudPresenter is ready for participant feedback messages.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ParticipantHealthHudBinder))]
    public sealed class ParticipantHealthHudBinderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            FeedbackHudEditorUtility.DrawParticipantHudGuide(
                "Inspector Field Guide: Participant Health HUD Binder",
                "ParticipantHealthHudBinder tracks a participant pawn and pushes its IActorHealthState into health labels, fill images, and ParticipantHealthPanel children.",
                new[]
                {
                    "Place this on a HUD canvas object for one participant view.",
                    "Assign a direct Health Label, direct Health Fill Image, reusable Health Panels, or let it discover child ParticipantHealthPanel components.",
                    "Use Primary Participant for single-player HUDs; use Participant Seat for split-screen or shared multi-player HUDs.",
                    "Make sure the tracked pawn has a component implementing IActorHealthState, such as HealthComponent."
                },
                "Do not leave every health label, fill image, and health panel empty.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetHealthHudBinderMessages(serializedObject), "ParticipantHealthHudBinder is ready for participant health HUD updates.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(HubInteractionHudPresenter))]
    public sealed class HubInteractionHudPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hub Interaction HUD Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "HubInteractionHudPresenter displays RPG hub prompts from HubInteractionService results, lets input/UI select a prompt, and shows the routed panel, dialogue, scene, issue, or notification result.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the HUD canvas object that owns RPG prompt UI.",
                        "Assign Prompt Label so nearby hub actions can be shown.",
                        "Assign Select Button for click/touch confirmation, or call ConfirmSelectedPrompt from a project input bridge.",
                        "Assign notification, route, or issue labels so selected interactions give visible feedback.",
                        "Have the hub interaction driver call ShowPrompts with available HubPromptPayload values and ShowInteractionResult after selection."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put this on every NPC or vendor; it is a HUD surface for the player view.",
                        "Do not leave Prompt Label empty unless another component is rendering prompts.",
                        "Do not directly load scenes from the presenter; consume the routed scene id from HubInteractionResult."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetHubInteractionHudMessages(serializedObject), "HubInteractionHudPresenter is ready for RPG hub prompts.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(HubInteractionSceneController))]
    public sealed class HubInteractionSceneControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Hub Interaction Scene Controller",
                new PyralisGuideSection(
                    "What This Is",
                    "HubInteractionSceneController is the scene bridge between a Hub Definition, HubInteractionService, and HubInteractionHudPresenter. It can be driven by trigger volumes, Actor Interaction input, buttons, or custom project input.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Hub Definition for the town, camp, level-select room, or safe-zone being presented.",
                        "Assign Hub Interaction HUD Presenter, or place the presenter as a child so it can be discovered.",
                        "Set Owner Kind and Owner Stable Id to the RPG owner whose inventory, quests, dialogue flags, and skill unlocks should be checked.",
                        "For trigger-driven hubs, add a 2D or 3D trigger collider and keep Refresh On Trigger Enter enabled.",
                        "For input-driven hubs, place this beside an ActorInteractionFeatureRuntime so TryHandleInteraction can refresh prompts."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put hub rule logic in this component; rules belong in HubDefinition and HubInteractionService.",
                        "Do not leave Owner Stable Id blank unless a project-owned input bridge supplies participant context.",
                        "Do not directly load scenes from this controller; consume Last Result or the presenter's routed state from a scene-flow bridge."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetHubInteractionSceneControllerMessages(serializedObject, (HubInteractionSceneController)target), "HubInteractionSceneController is ready to bridge hub prompts into the scene.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgHubPanelRouter))]
    public sealed class RpgHubPanelRouterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Hub Panel Router",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgHubPanelRouter listens to selected hub interaction results and opens the matching RPG panel route, such as Dialogue, QuestBoard, Vendor, Loadout, SkillTree, or Trainer.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the same Canvas hierarchy as HubInteractionHudPresenter.",
                        "Assign Hub Interaction HUD Presenter, or keep it in the same parent/child hierarchy for discovery.",
                        "Create one RpgPanelRoutePresenter per route surface you want to support.",
                        "Assign all route presenters in Route Presenters.",
                        "Keep Close Panels When Route Missing enabled unless another screen manager owns panel lifetime."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not create one router per panel; one router should coordinate the group.",
                        "Do not leave every route presenter empty.",
                        "Do not use the router for scene loading; scene ids should still route through scene-flow systems."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgHubPanelRouterMessages(serializedObject, (RpgHubPanelRouter)target), "RpgHubPanelRouter is ready to open routed RPG panels.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgPanelRoutePresenter))]
    public sealed class RpgPanelRoutePresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Panel Route Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgPanelRoutePresenter owns one panel surface for a PlayerPanelRoute. It shows and hides the panel and can fill optional title, body, and context labels from the selected hub result.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Choose the Route this panel represents.",
                        "Assign Panel Root, or place this component directly on the panel root object.",
                        "Assign optional Title, Body, and Context labels for automatic copy.",
                        "Leave the panel inactive by default so the router opens it when selected.",
                        "Add this presenter to RpgHubPanelRouter.Route Presenters."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Route set to None.",
                        "Do not put gameplay service mutation in this presenter; it is a display surface.",
                        "Do not use the same panel root for multiple active route presenters unless they intentionally share one screen."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgPanelRoutePresenterMessages(serializedObject), "RpgPanelRoutePresenter is ready for routed panel display.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgDialoguePanelPresenter))]
    public sealed class RpgDialoguePanelPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Dialogue Panel Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgDialoguePanelPresenter is the rich body for the Dialogue panel route. It starts native DialogueService sessions from hub results, renders the active node, and lets Continue or Choice buttons advance the conversation.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this inside the Dialogue panel root that has RpgPanelRoutePresenter set to Dialogue.",
                        "Assign Route Presenter, or keep this component under the Dialogue route presenter for discovery.",
                        "Assign Dialogue Graphs and optional NPC Profiles for the conversations this panel can open.",
                        "Assign Speaker, Line, Choice Summary, or Issue labels as needed for your layout.",
                        "Assign Continue Button and/or Choice Buttons so players can advance the conversation."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not point this at quest, vendor, or loadout route presenters; it only consumes Dialogue hub results.",
                        "Do not leave Dialogue Graphs empty unless a test or project bridge supplies runtime graphs.",
                        "Do not use one Choice Button for several simultaneous choices; add enough buttons for the largest expected choice hub."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgDialoguePanelPresenterMessages(serializedObject, (RpgDialoguePanelPresenter)target), "RpgDialoguePanelPresenter is ready for native dialogue playback.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgQuestBoardPanelPresenter))]
    public sealed class RpgQuestBoardPanelPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Quest Board Panel Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgQuestBoardPanelPresenter is the rich body for the QuestBoard panel route. It lists authored quests, shows owner-specific status, and starts the selected quest through QuestService.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this inside the QuestBoard panel root that has RpgPanelRoutePresenter set to QuestBoard.",
                        "Assign Route Presenter, or keep this component under the QuestBoard route presenter for discovery.",
                        "Assign Quest Definition assets for every quest this board can offer.",
                        "Assign board, selected quest, status, and issue labels as needed for your layout.",
                        "Assign Accept, Next, and Previous buttons, or call the public methods from a custom input bridge."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put all world quests on one board unless that is the intended design.",
                        "Do not leave Quest Definitions empty unless a test or project bridge supplies runtime quests.",
                        "Do not expect this first body to track map pins or turn-in staging yet; it starts quests and reflects QuestService status."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgQuestBoardPanelPresenterMessages(serializedObject, (RpgQuestBoardPanelPresenter)target), "RpgQuestBoardPanelPresenter is ready for quest board playback.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgVendorPanelPresenter))]
    public sealed class RpgVendorPanelPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Vendor Panel Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgVendorPanelPresenter is the rich body for the Vendor panel route. It lists authored vendor offers and performs buy/sell transactions through VendorService and InventoryService.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this inside the Vendor panel root that has RpgPanelRoutePresenter set to Vendor.",
                        "Assign Route Presenter, or keep this component under the Vendor route presenter for discovery.",
                        "Assign Vendor Definition assets for every shop this panel can open.",
                        "Assign vendor, offer list, selected offer, and issue labels as needed for your layout.",
                        "Assign Buy, Sell, Next, and Previous buttons, or call the public methods from a custom input bridge."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Vendors empty unless a test or project bridge supplies runtime vendors.",
                        "Do not forget that prices use an inventory item id as currency, such as item.gold.",
                        "Do not point the hub Vendor interaction at an NPC id unless that id matches a VendorDefinition.VendorId."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgVendorPanelPresenterMessages(serializedObject, (RpgVendorPanelPresenter)target), "RpgVendorPanelPresenter is ready for vendor transactions.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgLoadoutPanelPresenter))]
    public sealed class RpgLoadoutPanelPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Loadout Panel Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgLoadoutPanelPresenter is the rich body for the Loadout panel route. It lists authored equippable items, shows compatible slots, and equips or unequips the selected item through EquipmentService.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this inside the Loadout panel root that has RpgPanelRoutePresenter set to Loadout.",
                        "Assign Route Presenter, or keep this component under the Loadout route presenter for discovery.",
                        "Assign Equipment Slot Definition assets for every visible slot.",
                        "Assign Equippable Item Definition assets for the gear this panel can equip.",
                        "Assign loadout list, selected item, equipped slots, and issue labels as needed for your layout.",
                        "Assign Equip, Unequip, Next, and Previous buttons, or call the public methods from a custom input bridge."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Slots empty; the panel needs configured slots to know where an item can be equipped.",
                        "Do not leave Items empty unless a test or project bridge supplies runtime equippables.",
                        "Do not expect inventory ownership checks in this first body; it presents the authored loadout catalog and writes to EquipmentService."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgLoadoutPanelPresenterMessages(serializedObject, (RpgLoadoutPanelPresenter)target), "RpgLoadoutPanelPresenter is ready for loadout editing.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(RpgSkillTreePanelPresenter))]
    public sealed class RpgSkillTreePanelPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: RPG Skill Tree Panel Presenter",
                new PyralisGuideSection(
                    "What This Is",
                    "RpgSkillTreePanelPresenter is the rich body for the SkillTree and Trainer panel routes. It lists authored skill nodes, shows skill points, and unlocks the selected node through SkillTreeService.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this inside a SkillTree or Trainer panel root.",
                        "Assign Route Presenter, or keep this component under the matching route presenter for discovery.",
                        "Assign Skill Tree Definition assets for the trees this panel can present.",
                        "Assign tree, skill point, node list, selected node, and issue labels as needed for your layout.",
                        "Assign Unlock, Next Node, Previous Node, Next Tree, and Previous Tree buttons, or call the public methods from a custom input bridge."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not leave Skill Trees empty unless a test or project bridge supplies runtime trees.",
                        "Do not forget to provide a ProgressionService when nodes cost skill points.",
                        "Do not use the Trainer route for a different tree unless the hub interaction NPC Id or Interactable Id intentionally matches a SkillTreeDefinition tree id."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(FeedbackHudEditorUtility.GetRpgSkillTreePanelPresenterMessages(serializedObject, (RpgSkillTreePanelPresenter)target), "RpgSkillTreePanelPresenter is ready for skill unlock playback.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class FeedbackHudEditorUtility
    {
        public static void DrawParticipantHudGuide(string title, string whatThisIs, string[] requiredSetup, string primaryMistake)
        {
            PyralisInspectorGuide.DrawFieldGuide(
                title,
                new PyralisGuideSection(
                    "What This Is",
                    whatThisIs,
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/UI_HUD_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    requiredSetup),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        primaryMistake,
                        "Do not use a non-primary participant seat until the session actually registers that seat.",
                        "Do not put participant-bound HUD presenters on world-space actor prefabs unless that is the intended UI layer."
                    }));
        }

        public static List<PyralisGuideIssue> GetRelayMessages(ParticipantFeedbackRelay relay)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            GameObject root = relay != null ? relay.gameObject : null;

            if (root != null && !HasComponentInParents(root, "PawnRoot"))
                messages.Add(PyralisGuideIssue.Recommended("This relay is not under a PawnRoot. Participant lookup expects the relay to live on the participant pawn or a child."));

            if (root != null && root.GetComponents<ParticipantFeedbackRelay>().Length > 1)
                messages.Add(PyralisGuideIssue.Optional("This GameObject has multiple ParticipantFeedbackRelay components. That can duplicate HUD messages."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetHealthPanelMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            bool hasLabel = HasObject(serializedObject, "healthLabel");
            bool hasFill = HasObject(serializedObject, "healthFillImage");

            if (!hasLabel && !hasFill)
                messages.Add(PyralisGuideIssue.Required("Health Label, Health Fill Image, or both should be assigned."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetTimedTextPanelMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "label"))
                messages.Add(PyralisGuideIssue.Required("Label should reference the TextMeshProUGUI object to show and hide."));

            SerializedProperty displayTime = serializedObject.FindProperty("defaultDisplayTime");
            if (displayTime != null && displayTime.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Default Display Time must be greater than zero."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetFeedbackPresenterMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = GetParticipantFilterMessages(serializedObject);
            bool hasDirectLabels =
                HasObject(serializedObject, "comboLabel")
                || HasObject(serializedObject, "statusLabel")
                || HasObject(serializedObject, "scorePopupLabel")
                || HasObject(serializedObject, "combatAlertLabel");

            bool hasPanels =
                HasArrayItems(serializedObject, "comboPanels")
                || HasArrayItems(serializedObject, "statusPanels")
                || HasArrayItems(serializedObject, "scorePanels")
                || HasArrayItems(serializedObject, "combatAlertPanels");

            if (!hasDirectLabels && !hasPanels)
                messages.Add(PyralisGuideIssue.Required("Assign at least one feedback label or ParticipantTimedTextPanel array."));

            AddPositiveFloatIssue(messages, serializedObject, "comboDisplayTime", "Combo Display Time");
            AddPositiveFloatIssue(messages, serializedObject, "statusDisplayTime", "Status Display Time");
            AddPositiveFloatIssue(messages, serializedObject, "scoreDisplayTime", "Score Display Time");
            AddPositiveFloatIssue(messages, serializedObject, "combatAlertDisplayTime", "Combat Alert Display Time");

            return messages;
        }

        public static List<PyralisGuideIssue> GetHealthHudBinderMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = GetParticipantFilterMessages(serializedObject);
            bool hasDirectHudSurface = HasObject(serializedObject, "healthLabel") || HasObject(serializedObject, "healthFillImage");
            bool hasPanels = HasArrayItems(serializedObject, "healthPanels");

            if (!hasDirectHudSurface && !hasPanels)
                messages.Add(PyralisGuideIssue.Required("Assign a health label, fill image, or ParticipantHealthPanel array."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetHubInteractionHudMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "promptLabel"))
                messages.Add(PyralisGuideIssue.Required("Prompt Label should reference the TextMeshProUGUI object used for hub prompts."));

            if (!HasObject(serializedObject, "selectButton"))
                messages.Add(PyralisGuideIssue.Recommended("Select Button is empty. This is okay only when a project input bridge calls ConfirmSelectedPrompt()."));

            bool hasResultSurface =
                HasObject(serializedObject, "notificationTitleLabel")
                || HasObject(serializedObject, "notificationBodyLabel")
                || HasObject(serializedObject, "routeLabel")
                || HasObject(serializedObject, "issueLabel");

            if (!hasResultSurface)
                messages.Add(PyralisGuideIssue.Recommended("Add a notification, route, or issue label so selected hub interactions visibly respond."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetHubInteractionSceneControllerMessages(SerializedObject serializedObject, HubInteractionSceneController controller)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "hubDefinition"))
                messages.Add(PyralisGuideIssue.Required("Hub Definition should be assigned."));

            if (!HasObject(serializedObject, "hudPresenter") && (controller == null || controller.GetComponentInChildren<HubInteractionHudPresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Hub Interaction HUD Presenter should be assigned or placed as a child."));

            SerializedProperty ownerStableId = serializedObject.FindProperty("ownerStableId");
            if (ownerStableId != null && string.IsNullOrWhiteSpace(ownerStableId.stringValue))
                messages.Add(PyralisGuideIssue.Recommended("Owner Stable Id is empty. This is okay only if a project input bridge supplies participant context."));

            SerializedProperty triggerEnter = serializedObject.FindProperty("refreshOnTriggerEnter");
            if (triggerEnter != null && triggerEnter.boolValue && controller != null)
            {
                bool hasTriggerCollider = false;
                Collider[] colliders = controller.GetComponents<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                    hasTriggerCollider |= colliders[i] != null && colliders[i].isTrigger;

                Collider2D[] colliders2D = controller.GetComponents<Collider2D>();
                for (int i = 0; i < colliders2D.Length; i++)
                    hasTriggerCollider |= colliders2D[i] != null && colliders2D[i].isTrigger;

                if (!hasTriggerCollider)
                    messages.Add(PyralisGuideIssue.Recommended("Refresh On Trigger Enter is enabled, but no trigger Collider or Collider2D was found on this GameObject."));
            }

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgHubPanelRouterMessages(SerializedObject serializedObject, RpgHubPanelRouter router)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "hudPresenter") && (router == null || router.GetComponentInParent<HubInteractionHudPresenter>() == null && router.GetComponentInChildren<HubInteractionHudPresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Hub Interaction HUD Presenter should be assigned or discoverable in the same hierarchy."));

            if (!HasArrayItems(serializedObject, "routePresenters"))
                messages.Add(PyralisGuideIssue.Required("Route Presenters should include at least one RpgPanelRoutePresenter."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgPanelRoutePresenterMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty route = serializedObject.FindProperty("route");
            if (route != null && route.enumValueIndex == 0)
                messages.Add(PyralisGuideIssue.Required("Route should not be None."));

            if (!HasObject(serializedObject, "panelRoot"))
                messages.Add(PyralisGuideIssue.Recommended("Panel Root is empty. The presenter will use its own GameObject as the panel root."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgDialoguePanelPresenterMessages(SerializedObject serializedObject, RpgDialoguePanelPresenter presenter)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "routePresenter") && (presenter == null || presenter.GetComponentInParent<RpgPanelRoutePresenter>() == null && presenter.GetComponentInChildren<RpgPanelRoutePresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Route Presenter should be assigned or discoverable in the Dialogue panel hierarchy."));

            if (!HasArrayItems(serializedObject, "dialogueGraphs"))
                messages.Add(PyralisGuideIssue.Required("Dialogue Graphs should include at least one DialogueGraphDefinition."));

            if (!HasObject(serializedObject, "lineLabel"))
                messages.Add(PyralisGuideIssue.Required("Line Label should reference the TextMeshProUGUI object used for dialogue text."));

            if (!HasObject(serializedObject, "continueButton") && !HasArrayItems(serializedObject, "choiceButtons"))
                messages.Add(PyralisGuideIssue.Required("Assign Continue Button, Choice Buttons, or both so players can advance dialogue."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgQuestBoardPanelPresenterMessages(SerializedObject serializedObject, RpgQuestBoardPanelPresenter presenter)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "routePresenter") && (presenter == null || presenter.GetComponentInParent<RpgPanelRoutePresenter>() == null && presenter.GetComponentInChildren<RpgPanelRoutePresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Route Presenter should be assigned or discoverable in the QuestBoard panel hierarchy."));

            if (!HasArrayItems(serializedObject, "quests"))
                messages.Add(PyralisGuideIssue.Required("Quests should include at least one QuestDefinition."));

            if (!HasObject(serializedObject, "boardLabel") && !HasObject(serializedObject, "selectedQuestLabel"))
                messages.Add(PyralisGuideIssue.Required("Assign Board Label or Selected Quest Label so quests are visible."));

            if (!HasObject(serializedObject, "acceptButton"))
                messages.Add(PyralisGuideIssue.Recommended("Accept Button is empty. This is okay only when a project input bridge calls StartSelectedQuest()."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgVendorPanelPresenterMessages(SerializedObject serializedObject, RpgVendorPanelPresenter presenter)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "routePresenter") && (presenter == null || presenter.GetComponentInParent<RpgPanelRoutePresenter>() == null && presenter.GetComponentInChildren<RpgPanelRoutePresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Route Presenter should be assigned or discoverable in the Vendor panel hierarchy."));

            if (!HasArrayItems(serializedObject, "vendors"))
                messages.Add(PyralisGuideIssue.Required("Vendors should include at least one VendorDefinition."));

            if (!HasObject(serializedObject, "offerListLabel") && !HasObject(serializedObject, "selectedOfferLabel"))
                messages.Add(PyralisGuideIssue.Required("Assign Offer List Label or Selected Offer Label so vendor offers are visible."));

            if (!HasObject(serializedObject, "buyButton") && !HasObject(serializedObject, "sellButton"))
                messages.Add(PyralisGuideIssue.Recommended("Buy Button and Sell Button are empty. This is okay only when a project input bridge calls BuySelectedOffer() or SellSelectedOffer()."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgLoadoutPanelPresenterMessages(SerializedObject serializedObject, RpgLoadoutPanelPresenter presenter)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "routePresenter") && (presenter == null || presenter.GetComponentInParent<RpgPanelRoutePresenter>() == null && presenter.GetComponentInChildren<RpgPanelRoutePresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Route Presenter should be assigned or discoverable in the Loadout panel hierarchy."));

            if (!HasArrayItems(serializedObject, "slots"))
                messages.Add(PyralisGuideIssue.Required("Slots should include at least one EquipmentSlotDefinition."));

            if (!HasArrayItems(serializedObject, "items"))
                messages.Add(PyralisGuideIssue.Required("Items should include at least one EquippableItemDefinition."));

            if (!HasObject(serializedObject, "loadoutLabel") && !HasObject(serializedObject, "selectedItemLabel"))
                messages.Add(PyralisGuideIssue.Required("Assign Loadout Label or Selected Item Label so gear is visible."));

            if (!HasObject(serializedObject, "equipButton") && !HasObject(serializedObject, "unequipButton"))
                messages.Add(PyralisGuideIssue.Recommended("Equip Button and Unequip Button are empty. This is okay only when a project input bridge calls EquipSelectedItem() or UnequipSelectedItem()."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetRpgSkillTreePanelPresenterMessages(SerializedObject serializedObject, RpgSkillTreePanelPresenter presenter)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!HasObject(serializedObject, "routePresenter") && (presenter == null || presenter.GetComponentInParent<RpgPanelRoutePresenter>() == null && presenter.GetComponentInChildren<RpgPanelRoutePresenter>(true) == null))
                messages.Add(PyralisGuideIssue.Required("Route Presenter should be assigned or discoverable in the SkillTree or Trainer panel hierarchy."));

            if (!HasArrayItems(serializedObject, "skillTrees"))
                messages.Add(PyralisGuideIssue.Required("Skill Trees should include at least one SkillTreeDefinition."));

            if (!HasObject(serializedObject, "nodeListLabel") && !HasObject(serializedObject, "selectedNodeLabel"))
                messages.Add(PyralisGuideIssue.Required("Assign Node List Label or Selected Node Label so skill nodes are visible."));

            if (!HasObject(serializedObject, "skillPointLabel"))
                messages.Add(PyralisGuideIssue.Recommended("Skill Point Label is empty. Players can still unlock skills, but they will not see remaining points on this panel."));

            if (!HasObject(serializedObject, "unlockButton"))
                messages.Add(PyralisGuideIssue.Recommended("Unlock Button is empty. This is okay only when a project input bridge calls UnlockSelectedNode()."));

            return messages;
        }

        private static List<PyralisGuideIssue> GetParticipantFilterMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty usePrimary = serializedObject.FindProperty("usePrimaryParticipant");
            SerializedProperty seat = serializedObject.FindProperty("participantSeat");

            if (usePrimary != null && !usePrimary.boolValue && seat != null && seat.intValue < 0)
                messages.Add(PyralisGuideIssue.Required("Participant Seat must be zero or greater when Use Primary Participant is disabled."));

            return messages;
        }

        private static void AddPositiveFloatIssue(List<PyralisGuideIssue> messages, SerializedObject serializedObject, string propertyName, string displayName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required(displayName + " must be greater than zero."));
        }

        private static bool HasObject(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.objectReferenceValue != null;
        }

        private static bool HasArrayItems(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.isArray && property.arraySize > 0;
        }

        private static bool HasComponentInParents(GameObject root, string componentTypeName)
        {
            Transform current = root != null ? root.transform : null;
            while (current != null)
            {
                if (current.GetComponent(componentTypeName) != null)
                    return true;

                current = current.parent;
            }

            return false;
        }
    }
}
