using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public readonly struct PyralisRuntimeSystemClaimContext
    {
        public PyralisRuntimeSystemClaimContext(
            string participantPawnIssue,
            bool hasProjectileLauncher,
            bool hasScoreService,
            bool scoringEnabled)
        {
            ParticipantPawnIssue = participantPawnIssue;
            HasProjectileLauncher = hasProjectileLauncher;
            HasScoreService = hasScoreService;
            ScoringEnabled = scoringEnabled;
        }

        public string ParticipantPawnIssue { get; }
        public bool HasProjectileLauncher { get; }
        public bool HasScoreService { get; }
        public bool ScoringEnabled { get; }
    }

    public sealed class PyralisRuntimeSystemClaimReport
    {
        private readonly List<string> _declared;
        private readonly List<string> _unverified;

        public PyralisRuntimeSystemClaimReport(IEnumerable<string> declared, IEnumerable<string> unverified)
        {
            _declared = new List<string>(declared ?? System.Array.Empty<string>());
            _unverified = new List<string>(unverified ?? System.Array.Empty<string>());
        }

        public IReadOnlyList<string> Declared => _declared;
        public IReadOnlyList<string> Unverified => _unverified;
        public bool HasDeclaredClaims => _declared.Count > 0;
        public bool HasUnverifiedClaims => _unverified.Count > 0;
        public string UnverifiedSummary => string.Join(", ", _unverified);
    }

    public static class PyralisRuntimeSystemClaimResolver
    {
        public static PyralisRuntimeSystemClaimReport BuildReport(string[] requiredRuntimeSystems, PyralisRuntimeSystemClaimContext context)
        {
            List<string> declared = new List<string>();
            List<string> unverified = new List<string>();

            if (requiredRuntimeSystems == null || requiredRuntimeSystems.Length == 0)
                return new PyralisRuntimeSystemClaimReport(declared, unverified);

            for (int i = 0; i < requiredRuntimeSystems.Length; i++)
            {
                string claim = requiredRuntimeSystems[i];
                if (string.IsNullOrWhiteSpace(claim))
                    continue;

                declared.Add(claim);
                if (!IsVerified(claim, context))
                    unverified.Add(claim);
            }

            return new PyralisRuntimeSystemClaimReport(declared, unverified);
        }

        public static bool IsVerified(string claim, PyralisRuntimeSystemClaimContext context)
        {
            if (ContainsClaim(claim, "ParticipantScoreService") || ContainsClaim(claim, "ISessionScoreService"))
                return context.HasScoreService && context.ScoringEnabled;

            if (ContainsClaim(claim, "project-owned"))
                return false;

            if (ContainsClaim(claim, "ParticipantRosterService")
                || ContainsClaim(claim, "ParticipantSpawnService")
                || ContainsClaim(claim, "SessionStateService")
                || ContainsClaim(claim, "ParticipantInputRouter")
                || ContainsClaim(claim, "ProjectileFirePlanner")
                || ContainsStandaloneClaim(claim, "ActionDefinition")
                || ContainsClaim(claim, "ActionQueueService")
                || ContainsClaim(claim, "BoardDefinition")
                || ContainsClaim(claim, "BoardMovePolicyDefinition")
                || ContainsClaim(claim, "BoardTerminalConditionDefinition")
                || ContainsClaim(claim, "BoardMoveActionResolver")
                || ContainsClaim(claim, "TurnOrderDefinition"))
            {
                return true;
            }

            if (ContainsClaim(claim, "PawnRoot"))
                return string.IsNullOrWhiteSpace(context.ParticipantPawnIssue);

            if (ContainsClaim(claim, "ProjectileLauncher"))
                return context.HasProjectileLauncher;

            return false;
        }

        public static bool ContainsClaim(string claim, string token)
        {
            return !string.IsNullOrWhiteSpace(claim)
                && !string.IsNullOrWhiteSpace(token)
                && claim.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsStandaloneClaim(string claim, string token)
        {
            if (string.IsNullOrWhiteSpace(claim) || string.IsNullOrWhiteSpace(token))
                return false;

            int index = claim.IndexOf(token, System.StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int before = index - 1;
                int after = index + token.Length;
                bool startsCleanly = before < 0 || !IsIdentifierCharacter(claim[before]);
                bool endsCleanly = after >= claim.Length || !IsIdentifierCharacter(claim[after]);
                if (startsCleanly && endsCleanly)
                    return true;

                index = claim.IndexOf(token, index + token.Length, System.StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsIdentifierCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}
