using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Coordinates group-level combat logic, such as attack tokens and group positioning.
    /// </summary>
    public sealed class BattleManager : MonoBehaviour
    {
        [Header("Attack Token Settings")]
        [SerializeField] private int maxMeleeTokens = 2;
        [SerializeField] private int maxRangedTokens = 2;

        private readonly List<GameObject> _participants = new List<GameObject>();
        private int _availableMeleeTokens;
        private int _availableRangedTokens;

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