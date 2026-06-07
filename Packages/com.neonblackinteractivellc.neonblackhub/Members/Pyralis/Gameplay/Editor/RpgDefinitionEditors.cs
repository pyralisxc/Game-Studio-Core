using NeonBlack.Gameplay.Data.Definitions.Rpg;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(StatDefinition))]
    public class StatDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            StatDefinition definition = (StatDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Stat Definition",
                "A stat definition names one reusable RPG value that participants, actors, pawns, board pieces, NPCs, factions, or project-owned surfaces can read.",
                whenToUse: new[]
                {
                    "Use this for strength, speed, wisdom, health, defense, charisma, resources, or custom progression values.",
                    "Create one stat definition per stable gameplay meaning, not one per character."
                },
                createBefore: new[]
                {
                    "ProgressionCurveDefinition when the stat is part of a level or XP route.",
                    "Future equipment, skill tree, quest, or inventory effects that will modify this stat."
                },
                assignFirst: new[]
                {
                    "Set Stat Id to a stable id such as stat.wisdom.",
                    "Set Display Name for creator-facing UI.",
                    "Set Category so future RPG tools can group related stats."
                },
                validation: new[]
                {
                    "Stat Id is stable and non-empty.",
                    "Display Name is readable.",
                    "Category is not empty."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG stat definition is ready for progression, equipment, and skill-tree use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ProgressionCurveDefinition))]
    public class ProgressionCurveDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ProgressionCurveDefinition definition = (ProgressionCurveDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Progression Curve",
                "A progression curve maps total XP to levels and skill point grants for participant-owned or actor-owned RPG state.",
                whenToUse: new[]
                {
                    "Use this when a game mode awards XP, levels, skill points, talent points, or RPG-style unlock currency.",
                    "The same curve can serve brawlers, tabletop heroes, survival perks, action RPG pawns, or hub progression."
                },
                createBefore: new[]
                {
                    "StatDefinition assets for stats affected by level, equipment, or skills.",
                    "Future SkillTreeDefinition assets that spend the granted skill points."
                },
                assignFirst: new[]
                {
                    "Set Curve Id and Display Name.",
                    "Set Level Experience Thresholds with Level 1 at 0 XP.",
                    "Set Skill Point Grants for levels that should award points."
                },
                validation: new[]
                {
                    "Level 1 starts at 0 XP.",
                    "Thresholds never descend as level increases.",
                    "Skill point grants are zero or positive."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG progression curve is ready for ProgressionService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ItemDefinition definition = (ItemDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Item Definition",
                "An item definition names one reusable inventory entry for loot, resources, quest items, relics, keys, consumables, or future equipment.",
                whenToUse: new[]
                {
                    "Use this for brawler loot, tabletop relics, survival resources, hub keys, quest items, and upgrade materials.",
                    "Create one item definition per stable item meaning, not one per inventory stack."
                },
                createBefore: new[]
                {
                    "ItemCatalogDefinition that collects the items available to a game mode or project.",
                    "Future equipment, quest, vendor, reward, or skill-tree assets that reference this item."
                },
                assignFirst: new[]
                {
                    "Set Item Id to a stable id such as item.potion.",
                    "Set Display Name for creator-facing UI.",
                    "Set Category, Rarity, Max Stack Size, and Tags."
                },
                validation: new[]
                {
                    "Item Id is stable and non-empty.",
                    "Display Name is readable.",
                    "Max Stack Size is at least 1.",
                    "Tags are trimmed and empty tags are removed."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG item definition is ready for item catalog assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ItemCatalogDefinition))]
    public class ItemCatalogDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ItemCatalogDefinition definition = (ItemCatalogDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Item Catalog",
                "An item catalog gathers item definitions so inventory services, quests, vendors, rewards, hubs, and future save/load systems can validate item ids.",
                whenToUse: new[]
                {
                    "Use this when a game mode awards, removes, checks, shops, equips, or persists items.",
                    "A small prototype can use one catalog; larger projects may split catalogs by campaign, hub, biome, or mode."
                },
                createBefore: new[]
                {
                    "ItemDefinition assets for every item this catalog should expose.",
                    "Future quest, equipment, vendor, and reward definitions that will reference catalog items."
                },
                assignFirst: new[]
                {
                    "Set Catalog Id and Display Name.",
                    "Assign ItemDefinition assets.",
                    "Fix duplicate item ids before using the catalog at runtime."
                },
                validation: new[]
                {
                    "Catalog Id is stable and non-empty.",
                    "No item reference is null.",
                    "No two item definitions share the same Item Id.",
                    "Every referenced item passes its own validation."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG item catalog is ready for InventoryService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(EquipmentSlotDefinition))]
    public class EquipmentSlotDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EquipmentSlotDefinition definition = (EquipmentSlotDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Equipment Slot",
                "An equipment slot names one loadout position that participant-owned or actor-owned RPG state can equip into.",
                whenToUse: new[]
                {
                    "Use this for weapon, armor, cape, charm, tool, key item, or project-owned slots.",
                    "Slots are game-rule concepts; they do not need to match one exact pawn bone or UI panel."
                },
                createBefore: new[]
                {
                    "EquippableItemDefinition assets that list this slot in Allowed Slot Ids.",
                    "Future hub loadout stations, vendors, rewards, and skill unlocks that grant or require equipment."
                },
                assignFirst: new[]
                {
                    "Set Slot Id to a stable id such as slot.weapon.",
                    "Set Display Name for creator-facing UI.",
                    "Set Slot Family for grouping related slots."
                },
                validation: new[]
                {
                    "Slot Id is stable and non-empty.",
                    "Display Name is readable.",
                    "Slot Family is not empty."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG equipment slot is ready for equippable item setup.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(EquippableItemDefinition))]
    public class EquippableItemDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EquippableItemDefinition definition = (EquippableItemDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Equippable Item",
                "An equippable item extends a normal item with allowed slots and reusable stat modifiers.",
                whenToUse: new[]
                {
                    "Use this for weapons, armor, capes, charms, tools, relics, or key items that change gameplay.",
                    "The same equippable item can support brawlers, action RPG pawns, tabletop heroes, survival perks, and hub loadouts."
                },
                createBefore: new[]
                {
                    "EquipmentSlotDefinition assets for every slot this item can occupy.",
                    "StatDefinition assets for every stat this item modifies."
                },
                assignFirst: new[]
                {
                    "Set Item Id and Display Name.",
                    "Add at least one Allowed Slot Id.",
                    "Add Stat Modifiers only for stats this item changes."
                },
                validation: new[]
                {
                    "Item Id and Display Name are valid.",
                    "At least one Allowed Slot Id is assigned.",
                    "Every Stat Modifier has a Stat Id."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG equippable item is ready for EquipmentService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(SkillTreeDefinition))]
    public class SkillTreeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SkillTreeDefinition definition = (SkillTreeDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Skill Tree",
                "A skill tree spends progression skill points on reusable unlock nodes that can grant stats now and route to abilities, traversal powers, hub access, quests, or dialogue flags later.",
                whenToUse: new[]
                {
                    "Use this for brawler combos, gun upgrades, fantasy tactics abilities, survival perks, gliding powers, spells, and hub progression.",
                    "Create one tree per stable progression route, such as class, weapon, faction, dream world, or campaign track."
                },
                createBefore: new[]
                {
                    "ProgressionCurveDefinition assets that grant skill points.",
                    "StatDefinition assets for stat modifiers granted by nodes."
                },
                assignFirst: new[]
                {
                    "Set Tree Id and Display Name.",
                    "Add nodes with stable Node Id values.",
                    "Use prerequisite node ids to shape the unlock path.",
                    "Mark nodes Repeatable only when buying the same upgrade more than once is intended."
                },
                validation: new[]
                {
                    "Tree Id and Display Name are valid.",
                    "Node ids are unique and non-empty.",
                    "Prerequisites point to nodes in the same tree.",
                    "Costs are zero or positive.",
                    "Every Stat Modifier has a Stat Id."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG skill tree is ready for SkillTreeService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(QuestDefinition))]
    public class QuestDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            QuestDefinition definition = (QuestDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Quest",
                "A quest tracks authored objectives for one RPG owner and grants rewards when the objective list is complete.",
                whenToUse: new[]
                {
                    "Use this for NPC errands, brawler challenges, board goals, survival tasks, hub unlocks, open-zone events, or project-owned milestones.",
                    "Objectives can listen to external gameplay events such as collected items, defeated actors, reached zones, NPC talks, actions, board moves, score, or custom project events."
                },
                createBefore: new[]
                {
                    "ItemDefinition assets for item rewards.",
                    "ProgressionCurveDefinition assets if XP or skill-point rewards should feed leveling and skill trees.",
                    "Future NPC, dialogue, hub, or portal definitions that will start or complete this quest."
                },
                assignFirst: new[]
                {
                    "Set Quest Id and Display Name.",
                    "Add at least one objective with a stable Objective Id.",
                    "Set each objective Target Id to the item, NPC, zone, action, board move, score track, or event it listens for.",
                    "Add at least one reward."
                },
                validation: new[]
                {
                    "Quest Id and Display Name are valid.",
                    "Objective ids are unique and non-empty.",
                    "Objective target ids are assigned.",
                    "Required quantities are at least 1.",
                    "Rewards are present and non-negative."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG quest is ready for QuestService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(VendorDefinition))]
    public class VendorDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VendorDefinition definition = (VendorDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Vendor",
                "A vendor definition lists buyable and sellable offers that exchange inventory-owned currency items for inventory items.",
                whenToUse: new[]
                {
                    "Use this for shopkeepers, vending machines, upgrade benches, survival traders, reward kiosks, and hub merchants.",
                    "The first vendor economy uses a currency item id such as item.gold so projects can ship shops before a richer economy service exists."
                },
                createBefore: new[]
                {
                    "ItemDefinition assets for every item sold, bought back, or used as currency.",
                    "A HubDefinition interactable with PlayerPanelRoute.Vendor that points at this vendor id."
                },
                assignFirst: new[]
                {
                    "Set Vendor Id to a stable id such as vendor.apothecary.",
                    "Set Display Name for UI.",
                    "Add offers with stable Offer Id, Item Id, Currency Item Id, buy price, sell price, and buy/sell toggles."
                },
                validation: new[]
                {
                    "Vendor Id and Display Name are valid.",
                    "Offer ids are unique and non-empty.",
                    "Each offer has an Item Id.",
                    "Currency Item Id is assigned when buy or sell prices are greater than zero.",
                    "Each offer is buyable, sellable, or both."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG vendor is ready for VendorService and Vendor panel use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(NpcDefinition))]
    public class NpcDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NpcDefinition definition = (NpcDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG NPC",
                "An NPC definition gives a stable identity to a character, vendor, trainer, quest giver, portal keeper, board opponent, or project-owned interaction surface.",
                whenToUse: new[]
                {
                    "Use this for hub characters, brawler mission givers, fantasy RPG villagers, tabletop opponents, survival traders, lore readers, and mind-readable characters.",
                    "NPC ids are gameplay-facing references used by dialogue, quests, hubs, and project-owned systems."
                },
                createBefore: new[]
                {
                    "DialogueGraphDefinition assets that use this NPC as a speaker.",
                    "QuestDefinition assets that should be started, advanced, or completed through NPC dialogue."
                },
                assignFirst: new[]
                {
                    "Set NPC Id to a stable id such as npc.elder.",
                    "Set Display Name for creator-facing UI.",
                    "Set Role, Tags, Faction Id, and optional Actor Link Id when they matter to your game."
                },
                validation: new[]
                {
                    "NPC Id and Display Name are valid.",
                    "Tags are trimmed and unique.",
                    "Faction and actor links are stable ids when assigned."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG NPC is ready for dialogue graph assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(DialogueGraphDefinition))]
    public class DialogueGraphDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DialogueGraphDefinition definition = (DialogueGraphDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Dialogue Graph",
                "A dialogue graph defines speaker lines, player choices, conditions, and effects that can read and change Pyralis RPG state.",
                whenToUse: new[]
                {
                    "Use this when an NPC should talk, branch, remember flags, start quests, grant rewards, or route to future vendors, trainers, portals, and custom events.",
                    "This is the native Pyralis authoring model that a future visual narrative editor will edit directly."
                },
                createBefore: new[]
                {
                    "NpcDefinition assets for speakers.",
                    "QuestDefinition, ItemDefinition, ProgressionCurveDefinition, and SkillTreeDefinition assets referenced by conditions or effects."
                },
                assignFirst: new[]
                {
                    "Set Graph Id and Start Node Id.",
                    "Add a start node with matching Node Id.",
                    "Add line or choice nodes with speaker ids and text.",
                    "Attach conditions to choices and effects to choices or nodes."
                },
                validation: new[]
                {
                    "Graph Id and Start Node Id are valid.",
                    "Node ids are unique.",
                    "Next-node references point at existing nodes.",
                    "Condition and effect targets are assigned.",
                    "Line nodes include speaker id and text."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG dialogue graph is ready for DialogueService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(HubDefinition))]
    public class HubDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HubDefinition definition = (HubDefinition)target;
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: RPG Hub",
                "A hub definition gathers player-facing interactions such as NPC dialogue, quest boards, vendors, trainers, portals, loadout stations, lore readers, and minigame entrances.",
                whenToUse: new[]
                {
                    "Use this for villages, brawler mission rooms, survival bases, tabletop campaign rooms, dream-world lobbies, and hybrid minigame hubs.",
                    "Hub interactions are gameplay contracts first and UI/HUD payloads second, so projects can swap the visual presenter without rewriting hub rules."
                },
                createBefore: new[]
                {
                    "NpcDefinition and DialogueGraphDefinition assets for NPC dialogue interactions.",
                    "QuestDefinition, ItemDefinition, SkillTreeDefinition, and scene-flow assets referenced by conditions or effects."
                },
                assignFirst: new[]
                {
                    "Set Hub Id, Display Name, Scene Id, and Default Return Point Id.",
                    "Add interactables with stable ids, prompt text, icon ids, interaction kind, panel route, and target ids.",
                    "Use Locked Visible when the player should see a locked prompt, and Hidden Until Available for secret interactions."
                },
                validation: new[]
                {
                    "Hub and interactable ids are stable and unique.",
                    "Portal and minigame interactions include scene ids.",
                    "NPC dialogue interactions include dialogue graph ids.",
                    "Conditions and effects include required target ids."
                },
                manualPath: "Docs/RPG_SYSTEMS_ROADMAP.md"));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "RPG hub is ready for HubInteractionService use.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
