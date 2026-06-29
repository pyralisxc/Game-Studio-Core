using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Data.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        StableId = "feature.rpg.npc",
        Category = "Dialogue",
        CapabilityPath = "RPG/Dialogue/Definitions/Npc Definition",
        Surface = AuthoringSurface.Goal,
        RequiredFields = new[] { nameof(npcId), nameof(displayName), nameof(role), nameof(tags), nameof(factionId), nameof(actorLinkId) },
        SuccessChecks = new[] { "Proof that the NPC can initiate dialogue and has a valid profile." },
        Tags = new[] { "capability:Dialogue", "runtime:CharacterPawnGameplay", "lane:RPG" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/NPC", fileName = "NpcDefinition")]
    public class NpcDefinition : ScriptableObject, INpcProfile, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return RuntimeValidationIssueUtility.FromLocalValidationMessages(GetValidationIssues(), this);
        }

        public string npcId = "npc.new";
        public string displayName = "New NPC";
        public string role = "npc";
        public string[] tags = Array.Empty<string>();
        public string factionId = string.Empty;
        public string actorLinkId = string.Empty;
        public DialogueGraphDefinition defaultGraph;

        public string NpcId => Normalize(npcId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? NpcId : displayName.Trim();
        public string Role => Normalize(role);
        public string[] Tags => SanitizeTags(tags);
        public string FactionId => Normalize(factionId);
        public string ActorLinkId => Normalize(actorLinkId);

        public void Sanitize()
        {
            npcId = NpcId;
            displayName = DisplayName;
            role = string.IsNullOrWhiteSpace(Role) ? "npc" : Role;
            tags = Tags;
            factionId = FactionId;
            actorLinkId = ActorLinkId;
        }

        public NpcProfile CreateRuntimeProfile()
        {
            return new NpcProfile(NpcId, DisplayName, Role, Tags, FactionId, ActorLinkId);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();
            if (string.IsNullOrWhiteSpace(npcId))
                issues.Add("NPC id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            string[] rawTags = tags ?? Array.Empty<string>();
            HashSet<string> seenTags = new HashSet<string>();
            for (int i = 0; i < rawTags.Length; i++)
            {
                string tag = Normalize(rawTags[i]);
                if (string.IsNullOrEmpty(tag))
                    issues.Add($"Tags[{i}] cannot be empty.");
                else if (!seenTags.Add(tag))
                    issues.Add($"Tag `{tag}` is assigned more than once.");
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static string[] SanitizeTags(string[] values)
        {
            return (values ?? Array.Empty<string>())
                .Select(Normalize)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .ToArray();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
