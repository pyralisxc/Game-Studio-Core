using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class HubHudPromptList
    {
        private readonly List<HubPromptPayload> _prompts = new List<HubPromptPayload>();
        private int _selectedIndex = -1;

        public IReadOnlyList<HubPromptPayload> Prompts => _prompts;
        public int SelectedIndex => _selectedIndex;
        public bool HasPrompt => _selectedIndex >= 0 && _selectedIndex < _prompts.Count;
        public HubPromptPayload SelectedPrompt => HasPrompt ? _prompts[_selectedIndex] : default;

        public void ApplyPrompts(IEnumerable<HubPromptPayload> prompts)
        {
            _prompts.Clear();

            if (prompts != null)
            {
                foreach (HubPromptPayload prompt in prompts)
                {
                    if (!string.IsNullOrWhiteSpace(prompt.InteractableId))
                        _prompts.Add(prompt);
                }
            }

            _prompts.Sort(ComparePrompts);
            _selectedIndex = _prompts.Count > 0 ? 0 : -1;
        }

        public void Clear()
        {
            _prompts.Clear();
            _selectedIndex = -1;
        }

        public bool SelectPrompt(string interactableId)
        {
            if (string.IsNullOrWhiteSpace(interactableId))
                return false;

            string normalizedId = interactableId.Trim();
            for (int i = 0; i < _prompts.Count; i++)
            {
                if (!string.Equals(_prompts[i].InteractableId, normalizedId, StringComparison.OrdinalIgnoreCase))
                    continue;

                _selectedIndex = i;
                return true;
            }

            return false;
        }

        public void SelectNext()
        {
            if (_prompts.Count == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % _prompts.Count;
        }

        public void SelectPrevious()
        {
            if (_prompts.Count == 0)
                return;

            _selectedIndex = _selectedIndex <= 0 ? _prompts.Count - 1 : _selectedIndex - 1;
        }

        private static int ComparePrompts(HubPromptPayload left, HubPromptPayload right)
        {
            int lockedCompare = left.Locked.CompareTo(right.Locked);
            if (lockedCompare != 0)
                return lockedCompare;

            int priorityCompare = left.Priority.CompareTo(right.Priority);
            if (priorityCompare != 0)
                return priorityCompare;

            return string.Compare(left.Text, right.Text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
