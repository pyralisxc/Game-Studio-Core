using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Glue.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class PawnSystemTests
    {
        [Test]
        public void PawnRoot_InitializesAvailablePawnModulesFromDefinitionProfiles()
        {
            GameObject rosterObject = null;
            GameObject pawnObject = null;
            SessionDefinition session = null;
            ParticipantDefinition participantDefinition = null;
            PawnDefinition pawnDefinition = null;
            PawnMovementProfile movementProfile = null;
            PawnTraversalProfile traversalProfile = null;
            PawnPresentationProfile presentationProfile = null;
            PawnCombatProfile combatProfile = null;

            try
            {
                movementProfile = ScriptableObject.CreateInstance<PawnMovementProfile>();
                traversalProfile = ScriptableObject.CreateInstance<PawnTraversalProfile>();
                presentationProfile = ScriptableObject.CreateInstance<PawnPresentationProfile>();
                combatProfile = ScriptableObject.CreateInstance<PawnCombatProfile>();
                pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();
                pawnDefinition.movementProfile = movementProfile;
                pawnDefinition.traversalProfile = traversalProfile;
                pawnDefinition.presentationProfile = presentationProfile;
                pawnDefinition.combatProfile = combatProfile;

                ParticipantHandle participant = BuildParticipant(
                    pawnDefinition,
                    out rosterObject,
                    out session,
                    out participantDefinition);

                pawnObject = new GameObject("PawnRoot");
                PawnRoot root = pawnObject.AddComponent<PawnRoot>();
                StubMotor motor = pawnObject.AddComponent<StubMotor>();
                StubTraversal traversal = pawnObject.AddComponent<StubTraversal>();
                StubPresentation presentation = pawnObject.AddComponent<StubPresentation>();
                StubCombat combat = pawnObject.AddComponent<StubCombat>();

                root.InitializeForParticipant(participant, null);

                Assert.That(motor.LastProfile, Is.EqualTo(movementProfile));
                Assert.That(traversal.LastProfile, Is.EqualTo(traversalProfile));
                Assert.That(presentation.LastProfile, Is.EqualTo(presentationProfile));
                Assert.That(combat.LastProfile, Is.EqualTo(combatProfile));
            }
            finally
            {
                DestroyAll(pawnObject, rosterObject, session, participantDefinition, pawnDefinition, movementProfile, traversalProfile, presentationProfile, combatProfile);
            }
        }

        [Test]
        public void ParticipantRoster_AttachAndClearPawn_UpdateParticipantAndRaiseEvents()
        {
            GameObject rosterObject = new GameObject("Roster");
            GameObject pawn = new GameObject("Pawn");
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            ParticipantDefinition participantDefinition = ScriptableObject.CreateInstance<ParticipantDefinition>();

            try
            {
                ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
                session.defaultParticipants = new[] { participantDefinition };
                roster.SetSessionDefinition(session);
                ParticipantHandle participant = roster.RegisterParticipant(null, participantDefinition, 0);
                ParticipantHandle assignedParticipant = null;
                GameObject assignedPawn = null;
                ParticipantHandle clearedParticipant = null;
                GameObject clearedPawn = null;

                roster.ParticipantPawnAssigned += (handle, instance) =>
                {
                    assignedParticipant = handle;
                    assignedPawn = instance;
                };
                roster.ParticipantPawnCleared += (handle, instance) =>
                {
                    clearedParticipant = handle;
                    clearedPawn = instance;
                };

                roster.AttachPawn(participant, pawn);

                Assert.That(participant.PawnInstance, Is.EqualTo(pawn));
                Assert.That(assignedParticipant, Is.EqualTo(participant));
                Assert.That(assignedPawn, Is.EqualTo(pawn));

                roster.ClearPawn(participant);

                Assert.That(participant.PawnInstance, Is.Null);
                Assert.That(clearedParticipant, Is.EqualTo(participant));
                Assert.That(clearedPawn, Is.EqualTo(pawn));
            }
            finally
            {
                DestroyAll(pawn, rosterObject, session, participantDefinition);
            }
        }

        [Test]
        public void ParticipantInputProfileUtility_PreservesJoinedRuntimeActions()
        {
            GameObject playerObject = new GameObject("Joined Player");
            InputActionAsset joinedRuntimeActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionAsset profileTemplateActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();

            try
            {
                PlayerInput playerInput = playerObject.AddComponent<PlayerInput>();
                joinedRuntimeActions.AddActionMap("Player");
                profileTemplateActions.AddActionMap("Player");
                profile.actions = profileTemplateActions;
                profile.primaryActionMap = "Player";
                playerInput.actions = joinedRuntimeActions;

                ParticipantInputProfileUtility.ApplyToPlayerInput(playerInput, profile);

                Assert.That(playerInput.actions, Is.EqualTo(joinedRuntimeActions));
            }
            finally
            {
                DestroyAll(playerObject, joinedRuntimeActions, profileTemplateActions, profile);
            }
        }

        private static ParticipantHandle BuildParticipant(
            PawnDefinition pawnDefinition,
            out GameObject rosterObject,
            out SessionDefinition session,
            out ParticipantDefinition participantDefinition)
        {
            participantDefinition = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participantDefinition.defaultPawn = pawnDefinition;

            rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 1;
            roster.SetSessionDefinition(session);

            return roster.RegisterParticipant(null, participantDefinition, 0);
        }

        private static void DestroyAll(params Object[] objects)
        {
            foreach (Object obj in objects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }

        private sealed class StubMotor : MonoBehaviour, IPawnMotor
        {
            public PawnMovementProfile LastProfile { get; private set; }

            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
                LastProfile = movementProfile;
            }
        }

        private sealed class StubTraversal : MonoBehaviour, IPawnTraversalModule
        {
            public PawnTraversalProfile LastProfile { get; private set; }
            public float ShimmyVelocityX => 0f;

            public void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile traversalProfile)
            {
                LastProfile = traversalProfile;
            }

            public bool HandleHangFrame(FrameInput frameInput, float deltaTime) => false;
            public void ProbeLedge() { }
            public void HandleInteract() { }
            public void TriggerClimbUp() { }
            public void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f) { }
            public void SetClimbZone(IClimbZone zone) { }
            public void ClearClimbZone() { }
        }

        private sealed class StubPresentation : MonoBehaviour, IPawnPresentationModule
        {
            public PawnPresentationProfile LastProfile { get; private set; }

            public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
            {
                LastProfile = presentationProfile;
            }
        }

        private sealed class StubCombat : MonoBehaviour, IPawnCombatModule
        {
            public PawnCombatProfile LastProfile { get; private set; }

            public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile combatProfile)
            {
                LastProfile = combatProfile;
            }
        }
    }
}
