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
    /// Core contract helpers for capability flags. Editor-facing labels, tooltips,
    /// hygiene advice, and documentation links live in Editor/Authoring/Vocabulary.
    /// </summary>
    public static class AuthoringCapabilityRegistry
    {
        public static string GetDisplayName(AuthoringCapability capability)
        {
            return capability == AuthoringCapability.None
                ? "General"
                : PrettifyTypeName(capability.ToString());
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

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char previousWritten = '\0';
            for (int i = 0; i < name.Length; i++)
            {
                char current = name[i];
                if (current == '_' || current == '-' || current == '/')
                    current = ' ';

                if (char.IsWhiteSpace(current))
                {
                    if (sb.Length > 0 && previousWritten != ' ')
                    {
                        sb.Append(' ');
                        previousWritten = ' ';
                    }
                    continue;
                }

                if (sb.Length > 0 && ShouldInsertDisplaySpace(name, i, current, previousWritten))
                {
                    sb.Append(' ');
                    previousWritten = ' ';
                }

                sb.Append(current);
                previousWritten = current;
            }

            return sb.ToString();
        }

        private static bool ShouldInsertDisplaySpace(string value, int index, char current, char previousWritten)
        {
            if (previousWritten == ' ' || !char.IsUpper(current))
                return false;

            char previous = index > 0 ? value[index - 1] : '\0';
            char next = index + 1 < value.Length ? value[index + 1] : '\0';
            if (previous == '_' || previous == '-' || previous == '/' || char.IsWhiteSpace(previous))
                return false;

            if (char.IsLower(previous))
                return true;

            return char.IsUpper(previous)
                && next != '\0'
                && char.IsLower(next);
        }

        public static IEnumerable<AuthoringCapability> GetAllIndividualCapabilities()
        {
            foreach (AuthoringCapability val in Enum.GetValues(typeof(AuthoringCapability)))
            {
                if (val != AuthoringCapability.None)
                    yield return val;
            }
        }

    }
}
