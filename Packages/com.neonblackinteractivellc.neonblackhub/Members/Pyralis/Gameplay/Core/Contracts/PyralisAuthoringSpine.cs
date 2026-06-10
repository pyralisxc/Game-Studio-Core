using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// The formal vocabulary for Pyralis Authoring. 
    /// This defines the "Spine" of the engine's capabilities.
    /// Use Flags to support compositional intent (e.g., Combat | Puzzle).
    /// </summary>
    [Flags]
    public enum AuthoringCapability
{
        None = 0,
        
        // Core & Shell
        Setup = 1 << 0,
        Session = 1 << 1,
        Input = 1 << 2,
        UI = 1 << 3,
        
        // Actor & Action
        Movement = 1 << 4,
        Combat = 1 << 5,
        Animation = 1 << 6,
        VFX = 1 << 7,
        
        // Strategy & Board
        Tabletop = 1 << 8,
        Grid = 1 << 9,
        TurnBased = 1 << 10,
        
        // RPG & Narrative
        Stats = 1 << 11,
        Inventory = 1 << 12,
        Dialogue = 1 << 13,
        Puzzle = 1 << 14,
        
        // World & Meta
        Camera = 1 << 15,
        Environment = 1 << 16,
        Audio = 1 << 17,
        Networking = 1 << 18
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
            { AuthoringCapability.Session, new CapabilityMetadata("Session", "High-level game rules and participant definitions.") },
            { AuthoringCapability.Input, new CapabilityMetadata("Input", "Human and AI control schemes and event routing.") },
            { AuthoringCapability.UI, new CapabilityMetadata("UI", "User interface, menus, and HUD presentation.") },
            
            { AuthoringCapability.Movement, new CapabilityMetadata("Movement", "Pawn locomotion, physics, and pathfinding.") },
            { AuthoringCapability.Combat, new CapabilityMetadata("Combat", "Health, damage, weapons, and reaction systems.") },
            { AuthoringCapability.Animation, new CapabilityMetadata("Animation", "Visual state machines and skeletal deformation.") },
            { AuthoringCapability.VFX, new CapabilityMetadata("VFX", "Particle systems, post-processing, and shader effects.") },
            
            { AuthoringCapability.Tabletop, new CapabilityMetadata("Tabletop", "Board game logic, piece management, and move policies.") },
            { AuthoringCapability.Grid, new CapabilityMetadata("Grid", "Coordinate systems, cell properties, and spatial queries.") },
            { AuthoringCapability.TurnBased, new CapabilityMetadata("Turn Based", "Phase management, action queues, and initiative.") },
            
            { AuthoringCapability.Stats, new CapabilityMetadata("Stats", "Attributes, modifiers, and character progression systems.") },
            { AuthoringCapability.Inventory, new CapabilityMetadata("Inventory", "Item storage, equipment, and resource management.") },
            { AuthoringCapability.Dialogue, new CapabilityMetadata("Dialogue", "Narrative flow, branching conversations, and event nodes.") },
            { AuthoringCapability.Puzzle, new CapabilityMetadata("Puzzle", "Logic gates, triggers, and state-based world interactions.") },
            
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
