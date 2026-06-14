using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Categorized priority bounds for Pyralis Authoring contracts.
    /// Used to rank features and enforce hygiene rules (e.g. Primary duplication checks).
    /// </summary>
    public enum AuthoringPriority
    {
        /// <summary>No priority assigned; bypassed in conflict checks.</summary>
        Unspecified = 0,

        /// <summary>Secondary provider; coexists with others without warnings (default base for supporting modules).</summary>
        AuxiliaryDefault = 50,

        /// <summary>Canonical provider; strict enforcement of single-primary rule (100).</summary>
        Primary = 100,

        /// <summary>Obsolete contract; surfaces orange hygiene warnings and enforces expiration gates (999+).</summary>
        Deprecated = 999
    }

    /// <summary>
    /// The formal vocabulary for Pyralis Authoring. 
    /// This defines the "Spine" of the engine's capabilities.
    /// Use Flags to support compositional intent (e.g., Combat | Puzzle).
    /// </summary>
    [Flags]
    public enum AuthoringCapability : long
    {
        None = 0,
        
        // Core & Shell
        Setup = 1L << 0,
        Session = 1L << 1,
        Input = 1L << 2,
        UI = 1L << 3,
        
        // Actor & Action
        Movement = 1L << 4,
        Combat = 1L << 5,
        Animation = 1L << 6,
        VFX = 1L << 7,
        
        // Strategy & Board
        Tabletop = 1L << 8,
        Grid = 1L << 9,
        TurnBased = 1L << 10,
        
        // RPG & Narrative
        Stats = 1L << 11,
        Inventory = 1L << 12,
        Dialogue = 1L << 13,
        Puzzle = 1L << 14,
        Rpg = 1L << 19,
        Quests = 1L << 20,
        Vendors = 1L << 21,
        SkillTree = 1L << 22,
        Progression = 1L << 23,
        
        // World & Meta
        Camera = 1L << 15,
        Environment = 1L << 16,
        Audio = 1L << 17,
        Networking = 1L << 18,

        // Specialized Logic Roles (Hierarchical)
        CombatState = 1L << 24,
        CombatSensors = 1L << 25,

        // Session & Lifecycle Roles
        Rules = 1L << 27,
        Scoring = 1L << 28,
        Participants = 1L << 29,

        // Movement & Physics Roles (Granular)
        KineticMotor2D = 1L << 30,
        KineticMotor3D = 1L << 31,
        Steering2D = 1L << 32,
        Steering3D = 1L << 33,
        Traversal = 1L << 34,

        // Combat Behavioral Roles (Granular)
        MeleeFlow = 1L << 35,
        RangedFlow = 1L << 36,
        TacticsAggressive = 1L << 37,
        TacticsDefensive = 1L << 38
    }

    /// <summary>
    /// Registry providing metadata (tooltips, names) for Authoring Capabilities.
    /// This ensures a professional, non-duplicated vocabulary across the engine.
    /// </summary>
    public static class AuthoringCapabilityRegistry
    {
#if UNITY_EDITOR
        private static readonly Dictionary<AuthoringCapability, CapabilityMetadata> _metadata = new Dictionary<AuthoringCapability, CapabilityMetadata>
        {
            { AuthoringCapability.Setup, new CapabilityMetadata("Setup", "Foundational scene and bootstrap configuration.") },
            { AuthoringCapability.Session, new CapabilityMetadata("Session", "High-level session orchestration and network authority.") },
            { AuthoringCapability.Rules, new CapabilityMetadata("Rules", "Game-mode specific rulesets, win/loss conditions, and timers.") },
            { AuthoringCapability.Participants, new CapabilityMetadata("Participants", "Player seats, AI slots, and input ownership.") },
            { AuthoringCapability.Scoring, new CapabilityMetadata("Scoring", "Points, resources, leaderboards, and objective tracking.") },
            
            { AuthoringCapability.Input, new CapabilityMetadata("Input", "Human and AI control schemes and event routing.") },
            { AuthoringCapability.UI, new CapabilityMetadata("UI", "User interface, menus, and HUD presentation.") },
            
            { AuthoringCapability.Movement, new CapabilityMetadata("Movement", "General movement archetype and intent definitions.") },
            { AuthoringCapability.KineticMotor2D, new CapabilityMetadata("2D Kinetic Motor", "Low-level 2D physical motor implementation (Rigidbody2D).") },
            { AuthoringCapability.KineticMotor3D, new CapabilityMetadata("3D Kinetic Motor", "Low-level 3D physical motor implementation (CharacterController).") },
            { AuthoringCapability.Steering2D, new CapabilityMetadata("2D Steering", "Pathfinding and navigation for 2D actors.") },
            { AuthoringCapability.Steering3D, new CapabilityMetadata("3D Steering", "Pathfinding and navigation for 3D actors.") },
            { AuthoringCapability.Traversal, new CapabilityMetadata("Traversal", "World interaction features like ledge-climb, ladders, and jumping.") },

            { AuthoringCapability.Combat, new CapabilityMetadata("Combat", "General combat systems and weapon logic.") },
            { AuthoringCapability.CombatState, new CapabilityMetadata("Combat State", "Health, damage tracking, and actor life-cycle state.") },
            { AuthoringCapability.CombatSensors, new CapabilityMetadata("Combat Sensors", "Hitboxes, hurtboxes, and collision-based event triggers.") },
            { AuthoringCapability.MeleeFlow, new CapabilityMetadata("Melee Flow", "Attack sequencing, combos, and melee state management.") },
            { AuthoringCapability.RangedFlow, new CapabilityMetadata("Ranged Flow", "Projectile sequencing, reloading, and targeting logic.") },
            { AuthoringCapability.TacticsAggressive, new CapabilityMetadata("Aggressive Tactics", "AI decision trees for charging, flanking, and attacking.") },
            { AuthoringCapability.TacticsDefensive, new CapabilityMetadata("Defensive Tactics", "AI decision trees for guarding, retreating, and kiting.") },
            
            { AuthoringCapability.Animation, new CapabilityMetadata("Animation", "Visual state machines and skeletal deformation.") },
            { AuthoringCapability.VFX, new CapabilityMetadata("VFX", "Particle systems, post-processing, and shader effects.") },

            { AuthoringCapability.Tabletop, new CapabilityMetadata("Tabletop", "Board game logic, piece management, and move policies.") },
            { AuthoringCapability.Grid, new CapabilityMetadata("Grid", "Coordinate systems, cell properties, and spatial queries.") },
            { AuthoringCapability.TurnBased, new CapabilityMetadata("Turn Based", "Phase management, action queues, and initiative.") },
            
            { AuthoringCapability.Stats, new CapabilityMetadata("Stats", "Attributes, modifiers, and character progression systems.") },
            { AuthoringCapability.Inventory, new CapabilityMetadata("Inventory", "Item storage, equipment, and resource management.") },
            { AuthoringCapability.Dialogue, new CapabilityMetadata("Dialogue", "Narrative flow, branching conversations, and event nodes.") },
            { AuthoringCapability.Puzzle, new CapabilityMetadata("Puzzle", "Logic gates, triggers, and state-based world interactions.") },
            { AuthoringCapability.Rpg, new CapabilityMetadata("RPG", "General role-playing systems.") },
            { AuthoringCapability.Quests, new CapabilityMetadata("Quests", "Quest tracking, objective management, and reward systems.") },
            { AuthoringCapability.Vendors, new CapabilityMetadata("Vendors", "Shop logic, trading interfaces, and currency exchange.", "https://docs.neonblack.com/pyralis/vendors") },
            { AuthoringCapability.SkillTree, new CapabilityMetadata("Skill Tree", "Abilities, unlock paths, and specialized talent trees.") },
            { AuthoringCapability.Progression, new CapabilityMetadata("Progression", "Experience points, leveling, and milestone tracking.") },
            
            { AuthoringCapability.Camera, new CapabilityMetadata("Camera", "Framing, following, and world containment boundaries.") },
            { AuthoringCapability.Environment, new CapabilityMetadata("Environment", "World geometry, lighting, and static decoration.") },
            { AuthoringCapability.Audio, new CapabilityMetadata("Audio", "Soundscapes, spatial audio, and music management.") },
            { AuthoringCapability.Networking, new CapabilityMetadata("Networking", "State synchronization, authority, and multiplayer connectivity.") }
        };
#endif

        public static string GetDisplayName(AuthoringCapability capability)
        {
            if (capability == AuthoringCapability.None) return "General";
#if UNITY_EDITOR
            return _metadata.TryGetValue(capability, out var meta) ? meta.DisplayName : capability.ToString();
#else
            return capability.ToString();
#endif
        }

        public static string GetTooltip(AuthoringCapability capability)
        {
#if UNITY_EDITOR
            return _metadata.TryGetValue(capability, out var meta) ? meta.Tooltip : "A reflective engine capability.";
#else
            return string.Empty;
#endif
        }

        public static string GetDocumentationURL(AuthoringCapability capability)
        {
#if UNITY_EDITOR
            return _metadata.TryGetValue(capability, out var meta) ? meta.DocumentationURL : string.Empty;
#else
            return string.Empty;
#endif
        }

        public static string GetExpertAdvice(AuthoringCapability capability)
        {
#if UNITY_EDITOR
            return _metadata.TryGetValue(capability, out var meta) ? meta.ExpertAdvice : string.Empty;
#else
            return string.Empty;
#endif
        }

        public static string GetHygieneAdvice(AuthoringCapability capability)
        {
#if UNITY_EDITOR
            return _metadata.TryGetValue(capability, out var meta) ? meta.HygieneAdvice : $"Ensure your scripts are tagged with [AuthoringContract(Capability = AuthoringCapability.{capability})].";
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Converts a camelCase or PascalCase type name into a space-separated display name,
        /// removing common prefixes like 'I' for interfaces.
        /// </summary>
        public static string PrettifyTypeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Remove 'I' prefix from interfaces
            if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
                name = name.Substring(1);

            // Add spaces before capitals
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                    sb.Append(' ');
                sb.Append(name[i]);
            }
            return sb.ToString();
        }

        public static IEnumerable<AuthoringCapability> GetAllIndividualCapabilities()
        {
            foreach (AuthoringCapability val in Enum.GetValues(typeof(AuthoringCapability)))
            {
                if (val != AuthoringCapability.None)
                    yield return val;
            }
        }

#if UNITY_EDITOR
        private struct CapabilityMetadata
        {
            public string DisplayName;
            public string Tooltip;
            public string DocumentationURL;
            public string ExpertAdvice;
            public string HygieneAdvice;

            public CapabilityMetadata(string displayName, string tooltip, string documentationURL = "", string expertAdvice = "", string hygieneAdvice = "")
            {
                DisplayName = displayName;
                Tooltip = tooltip;
                DocumentationURL = documentationURL;
                ExpertAdvice = expertAdvice;
                HygieneAdvice = hygieneAdvice;
            }
        }
#endif
    }
}
