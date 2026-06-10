using System;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisDocGenerator
    {
        [MenuItem("NeonBlack/Gameplay/Pyralis/Generate Engine Documentation")]
        public static void Generate()
        {
            var facts = PyralisAuthoringFactRegistry.AllFacts;
            var sb = new StringBuilder();
            sb.AppendLine("# Pyralis Engine Capabilities & Authoring Contracts");
            sb.AppendLine();
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("This document is auto-generated from the [AuthoringContract] attributes and the central Authoring Registries. It serves as the singular source of truth for the engine's capabilities.");
            sb.AppendLine();

            // Handle Typed Capabilities (Flags)
            var allIndividualCapabilities = AuthoringCapabilityRegistry.GetAllIndividualCapabilities().ToList();
            
            foreach (var cap in allIndividualCapabilities)
            {
                var matchingFacts = facts.Where(f => (f.Capability & cap) != 0).OrderBy(f => f.DisplayName).ToList();
                if (matchingFacts.Count == 0) continue;

                sb.AppendLine($"## Capability: {AuthoringCapabilityRegistry.GetDisplayName(cap)}");
                sb.AppendLine($"> {AuthoringCapabilityRegistry.GetTooltip(cap)}");
                sb.AppendLine();
                
                var advice = AuthoringCapabilityRegistry.GetHygieneAdvice(cap);
                if (!string.IsNullOrEmpty(advice))
                {
                    sb.AppendLine($"**Implementation Goal**: {advice}");
                    sb.AppendLine();
                }

                foreach (var fact in matchingFacts)
                {
                    DrawFact(sb, fact);
                }
                
                sb.AppendLine("---");
                sb.AppendLine();
            }

            // Handle Legacy Goal-based contracts or General facts
            var legacyFacts = facts.Where(f => f.Capability == AuthoringCapability.None).OrderBy(f => f.DisplayName).ToList();
            if (legacyFacts.Count > 0)
            {
                sb.AppendLine("## General & Legacy Contracts");
                sb.AppendLine("> Contracts that have not yet been migrated to the typed Spine Capability system or represent general engine utilities.");
                sb.AppendLine();

                foreach (var fact in legacyFacts)
                {
                    DrawFact(sb, fact);
                }
                
                sb.AppendLine("---");
                sb.AppendLine();
            }

            string path = "Assets/Pyralis/PyralisEngineContracts.md";
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            
            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated Pyralis documentation at {path}");
        }

        private static void DrawFact(StringBuilder sb, PyralisAuthoringFact fact)
        {
            sb.AppendLine($"### {fact.DisplayName}");
            
            if (!string.IsNullOrEmpty(fact.Summary))
                sb.AppendLine($"- **Summary**: {fact.Summary}");
            
            if (!string.IsNullOrEmpty(fact.ExpertAdvice))
                sb.AppendLine($"- **Expert Advice**: {fact.ExpertAdvice}");
            
            if (!string.IsNullOrEmpty(fact.DocumentationURL))
                sb.AppendLine($"- **Docs**: [Technical Specification]({fact.DocumentationURL})");

            // Add Axioms if present
            if (fact.Axioms != AuthoringWorldAxiom.None)
                sb.AppendLine($"- **Axioms**: `{fact.Axioms}`");

            // Add Lane tags if present
            if (fact.LaneTags != null && fact.LaneTags.Length > 0)
                sb.AppendLine($"- **Lanes**: {string.Join(", ", fact.LaneTags.Select(l => $"`{l}`"))}");

            sb.AppendLine();
        }
    }
}
