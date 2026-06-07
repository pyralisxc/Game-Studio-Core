using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Authored ordered combo or action chain for one neutral combat lane.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Combat/Combat Sequence Definition", fileName = "CombatSequenceDefinition")]
    public class CombatSequenceDefinition : ScriptableObject
    {
        public string displayName = "Combat Sequence";
        public CombatInputType inputType = CombatInputType.Primary;
        public bool resetAfterFinalAction = true;
        public bool restartFromFirstActionWhenBranchFails = true;
        public CombatActionDefinition[] actions;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = name;
            }

            if (actions == null)
            {
                actions = Array.Empty<CombatActionDefinition>();
                return;
            }

            List<CombatActionDefinition> sanitized = new List<CombatActionDefinition>(actions.Length);
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                {
                    sanitized.Add(actions[i]);
                }
            }

            actions = sanitized.ToArray();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
