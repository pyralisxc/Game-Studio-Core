using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.progression",
        Capability = AuthoringCapability.Stats,
        Relevance = "Manages character progression, including experience points, leveling up, and skill point grants.",
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IProgressionCurve) },
        NativeSetup = new[]
        {
            "define progression curves for Level/XP",
            "assign curves to ProgressionService",
            "configure StatSheets for classes"
        },
        FirstProof = "Grant experience to an actor and verify they level up according to the progression curve."
    )]
    public sealed class ProgressionService : IProgressionService
{
        private readonly IProgressionCurve _curve;
        private readonly Dictionary<RpgOwnerKey, ProgressionState> _states = new Dictionary<RpgOwnerKey, ProgressionState>();

        public ProgressionService(IProgressionCurve curve)
        {
            _curve = curve;
        }

        public ProgressionState GetState(RpgOwnerKey owner)
        {
            return _states.TryGetValue(owner, out ProgressionState state)
                ? state
                : new ProgressionState(0, 1, 0);
        }

        public RpgProgressionSnapshot GetSnapshot(RpgOwnerKey owner)
        {
            ProgressionState state = GetState(owner);
            return new RpgProgressionSnapshot(state.Experience, state.Level, state.SkillPoints);
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgProgressionSnapshot snapshot)
        {
            if (!owner.IsValid)
                return;

            _states[owner] = snapshot.ToState();
        }

        public ProgressionState AddExperience(RpgOwnerKey owner, int amount)
        {
            if (!owner.IsValid)
                return new ProgressionState(0, 1, 0);

            ProgressionState current = GetState(owner);
            int added = amount < 0 ? 0 : amount;
            int experience = current.Experience + added;
            int level = _curve != null ? _curve.ResolveLevel(experience) : 1;
            int skillPoints = current.SkillPoints + ResolveNewSkillPoints(current.Level, level);
            ProgressionState next = current.WithExperienceLevelAndSkillPoints(experience, level, skillPoints);
            _states[owner] = next;
            return next;
        }

        public bool TrySpendSkillPoints(RpgOwnerKey owner, int amount, out string issue)
        {
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (amount <= 0)
            {
                issue = "Skill point spend amount must be positive.";
                return false;
            }

            ProgressionState state = GetState(owner);
            if (state.SkillPoints < amount)
            {
                issue = "Not enough skill points.";
                return false;
            }

            _states[owner] = state.WithExperienceLevelAndSkillPoints(
                state.Experience,
                state.Level,
                state.SkillPoints - amount);
            issue = string.Empty;
            return true;
        }

        public ProgressionState GrantSkillPoints(RpgOwnerKey owner, int amount)
        {
            if (!owner.IsValid)
                return new ProgressionState(0, 1, 0);

            ProgressionState state = GetState(owner);
            int added = amount < 0 ? 0 : amount;
            ProgressionState next = state.WithExperienceLevelAndSkillPoints(
                state.Experience,
                state.Level,
                state.SkillPoints + added);
            _states[owner] = next;
            return next;
        }

        private int ResolveNewSkillPoints(int previousLevel, int nextLevel)
        {
            if (_curve == null || nextLevel <= previousLevel)
                return 0;

            int points = 0;
            for (int level = previousLevel + 1; level <= nextLevel; level++)
                points += _curve.GetSkillPointGrantForLevel(level);

            return points;
        }
    }
}
