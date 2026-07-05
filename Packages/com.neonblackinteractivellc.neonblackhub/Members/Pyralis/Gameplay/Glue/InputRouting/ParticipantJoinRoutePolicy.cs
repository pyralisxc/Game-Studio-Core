using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Glue.InputRouting
{
    internal readonly struct ParticipantJoinRouteDecision
    {
        public ParticipantJoinRouteDecision(bool shouldDeferAutoRegistration, int autoJoinParticipantCount)
        {
            ShouldDeferAutoRegistration = shouldDeferAutoRegistration;
            AutoJoinParticipantCount = autoJoinParticipantCount;
        }

        public bool ShouldDeferAutoRegistration { get; }
        public int AutoJoinParticipantCount { get; }

        public string WarningMessage =>
            "Multiple default participants are marked Auto Join while PlayerInputManager is assigned. "
            + "Skipping automatic registration so Unity PlayerInputManager can pair each controller with one participant.";
    }

    internal static class ParticipantJoinRoutePolicy
    {
        public static ParticipantJoinRouteDecision Evaluate(SessionDefinition sessionDefinition, bool hasPlayerInputManager)
        {
            int autoJoinCount = CountAutoJoinDefaultParticipants(sessionDefinition);
            return new ParticipantJoinRouteDecision(hasPlayerInputManager && autoJoinCount > 1, autoJoinCount);
        }

        private static int CountAutoJoinDefaultParticipants(SessionDefinition sessionDefinition)
        {
            if (sessionDefinition == null || sessionDefinition.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < sessionDefinition.defaultParticipants.Length; i++)
            {
                ParticipantDefinition definition = sessionDefinition.defaultParticipants[i];
                if (definition != null && definition.autoJoin)
                    count++;
            }

            return count;
        }
    }
}
