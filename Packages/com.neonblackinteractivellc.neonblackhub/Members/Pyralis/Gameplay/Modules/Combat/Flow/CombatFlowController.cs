using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    /// <summary>
    /// Coordinates group-level combat logic, such as attack tokens and group positioning.
    /// </summary>
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Battle Manager",
        Surface = AuthoringSurface.Goal,
        Summary = "Manages attack tokens to prevent all enemies from attacking the player simultaneously.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        SetupSteps = new[] { "Add to a scene coordinator or core services object." },
        SuccessChecks = new[] { "Verify that enemies request and return tokens when starting/finishing attacks." },
        Tags = new[] { "capability:Combat", "axiom:Realtime" }
    )]
    public sealed class CombatFlowController : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Attack Token Settings")]
        [SerializeField] private int maxMeleeTokens = 2;
        [SerializeField] private int maxRangedTokens = 2;

        private readonly List<GameObject> _participants = new List<GameObject>();
        private int _availableMeleeTokens;
        private int _availableRangedTokens;

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (maxMeleeTokens < 0)
                yield return RuntimeValidationIssue.Required("Max Melee Tokens cannot be negative.");
            if (maxRangedTokens < 0)
                yield return RuntimeValidationIssue.Required("Max Ranged Tokens cannot be negative.");
        }

        private void Awake()
        {
            _availableMeleeTokens = maxMeleeTokens;
            _availableRangedTokens = maxRangedTokens;
        }

        public void RegisterParticipant(GameObject participant)
        {
            if (!_participants.Contains(participant))
                _participants.Add(participant);
        }

        public void UnregisterParticipant(GameObject participant)
        {
            _participants.Remove(participant);
        }

        public bool TryRequestAttackToken(GameObject requester, bool isMelee)
        {
            if (isMelee)
            {
                if (_availableMeleeTokens > 0)
                {
                    _availableMeleeTokens--;
                    return true;
                }
            }
            else
            {
                if (_availableRangedTokens > 0)
                {
                    _availableRangedTokens--;
                    return true;
                }
            }

            return false;
        }

        public void ReturnAttackToken(bool isMelee)
        {
            if (isMelee)
                _availableMeleeTokens = Mathf.Min(_availableMeleeTokens + 1, maxMeleeTokens);
            else
                _availableRangedTokens = Mathf.Min(_availableRangedTokens + 1, maxRangedTokens);
        }

        public int GetParticipantCount() => _participants.Count;
    }
}
