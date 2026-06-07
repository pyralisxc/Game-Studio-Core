using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    /// <summary>
    /// Verifies that PawnRoot.ApplyProfiles dispatches to each pawn-module interface
    /// when the matching component is present as a child MonoBehaviour.
    /// </summary>
    public class PawnSystemTests
    {
        // ── Stub MonoBehaviours ────────────────────────────────────────────── //

        private class StubMotor : MonoBehaviour, IPawnMotor
        {
            public PawnMovementProfile LastProfile { get; private set; }
            public int CallCount { get; private set; }

            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
                LastProfile = movementProfile;
                CallCount++;
            }
        }

        private class StubTraversal : MonoBehaviour, IPawnTraversalModule
        {
            public PawnTraversalProfile LastProfile { get; private set; }
            public int CallCount { get; private set; }

            public void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile traversalProfile)
            {
                LastProfile = traversalProfile;
                CallCount++;
            }
        }

        private class StubPresentation : MonoBehaviour, IPawnPresentationModule
        {
            public PawnPresentationProfile LastProfile { get; private set; }
            public int CallCount { get; private set; }

            public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
            {
                LastProfile = presentationProfile;
                CallCount++;
            }
        }

        private class StubCombat : MonoBehaviour, IPawnCombatModule
        {
            public PawnCombatProfile LastProfile { get; private set; }
            public int CallCount { get; private set; }

            public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile combatProfile)
            {
                LastProfile = combatProfile;
                CallCount++;
            }
        }

        // ── Helper ─────────────────────────────────────────────────────────── //

        private ParticipantHandle BuildParticipant(
            PawnDefinition pawnDefinition,
            out GameObject rosterGo,
            out SessionDefinition session,
            out ParticipantDefinition participantDef)
        {
            participantDef = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participantDef.defaultPawn = pawnDefinition;

            rosterGo = new GameObject("Roster");
            ParticipantRosterService roster = rosterGo.AddComponent<ParticipantRosterService>();
            session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 1;
            roster.SetSessionDefinition(session);

            return roster.RegisterParticipant(null, participantDef, 0);
        }

        // ── Tests ──────────────────────────────────────────────────────────── //

        [Test]
        public void PawnRoot_WithIPawnMotorChild_CallsApplyMovementProfile()
        {
            PawnMovementProfile movementProfile = ScriptableObject.CreateInstance<PawnMovementProfile>();
            movementProfile.walkSpeed = 7f;

            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            definition.movementProfile = movementProfile;
            definition.featureModules = new FeatureModuleDefinition[0];

            ParticipantHandle participant = BuildParticipant(definition, out GameObject rosterGo, out SessionDefinition session, out ParticipantDefinition participantDef);

            GameObject go = new GameObject("PawnRoot");
            PawnRoot root = go.AddComponent<PawnRoot>();
            StubMotor motor = go.AddComponent<StubMotor>();

            root.InitializeForParticipant(participant, null);

            Assert.That(motor.CallCount, Is.EqualTo(1), "IPawnMotor.ApplyMovementProfile should be called once");
            Assert.That(motor.LastProfile, Is.EqualTo(movementProfile));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(rosterGo);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(movementProfile);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participantDef);
        }

        [Test]
        public void PawnRoot_WithIPawnTraversalModuleChild_CallsApplyTraversalProfile()
        {
            PawnTraversalProfile traversalProfile = ScriptableObject.CreateInstance<PawnTraversalProfile>();
            traversalProfile.jumpHeight = 5f;

            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            definition.traversalProfile = traversalProfile;
            definition.featureModules = new FeatureModuleDefinition[0];

            ParticipantHandle participant = BuildParticipant(definition, out GameObject rosterGo, out SessionDefinition session, out ParticipantDefinition participantDef);

            GameObject go = new GameObject("PawnRoot");
            PawnRoot root = go.AddComponent<PawnRoot>();
            StubTraversal stub = go.AddComponent<StubTraversal>();

            root.InitializeForParticipant(participant, null);

            Assert.That(stub.CallCount, Is.EqualTo(1), "IPawnTraversalModule.ApplyTraversalProfile should be called once");
            Assert.That(stub.LastProfile, Is.EqualTo(traversalProfile));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(rosterGo);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(traversalProfile);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participantDef);
        }

        [Test]
        public void PawnRoot_WithIPawnPresentationModuleChild_CallsApplyPresentationProfile()
        {
            PawnPresentationProfile presentationProfile = ScriptableObject.CreateInstance<PawnPresentationProfile>();

            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            definition.presentationProfile = presentationProfile;
            definition.featureModules = new FeatureModuleDefinition[0];

            ParticipantHandle participant = BuildParticipant(definition, out GameObject rosterGo, out SessionDefinition session, out ParticipantDefinition participantDef);

            GameObject go = new GameObject("PawnRoot");
            PawnRoot root = go.AddComponent<PawnRoot>();
            StubPresentation stub = go.AddComponent<StubPresentation>();

            root.InitializeForParticipant(participant, null);

            Assert.That(stub.CallCount, Is.EqualTo(1), "IPawnPresentationModule.ApplyPresentationProfile should be called once");
            Assert.That(stub.LastProfile, Is.EqualTo(presentationProfile));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(rosterGo);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(presentationProfile);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participantDef);
        }

        [Test]
        public void PawnRoot_WithIPawnCombatModuleChild_CallsApplyCombatProfile()
        {
            PawnCombatProfile combatProfile = ScriptableObject.CreateInstance<PawnCombatProfile>();
            combatProfile.baseDamage = 25f;

            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            definition.combatProfile = combatProfile;
            definition.featureModules = new FeatureModuleDefinition[0];

            ParticipantHandle participant = BuildParticipant(definition, out GameObject rosterGo, out SessionDefinition session, out ParticipantDefinition participantDef);

            GameObject go = new GameObject("PawnRoot");
            PawnRoot root = go.AddComponent<PawnRoot>();
            StubCombat stub = go.AddComponent<StubCombat>();

            root.InitializeForParticipant(participant, null);

            Assert.That(stub.CallCount, Is.EqualTo(1), "IPawnCombatModule.ApplyCombatProfile should be called once");
            Assert.That(stub.LastProfile, Is.EqualTo(combatProfile));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(rosterGo);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(combatProfile);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participantDef);
        }

        [Test]
        public void PawnRoot_WithNullPawnDefinition_DoesNotCallModules()
        {
            GameObject go = new GameObject("PawnRoot");
            PawnRoot root = go.AddComponent<PawnRoot>();
            StubMotor motor = go.AddComponent<StubMotor>();

            // Pass null participant — PawnRoot guards pawnDefinition == null in ApplyProfiles
            root.InitializeForParticipant(null, null);

            Assert.That(motor.CallCount, Is.EqualTo(0), "No module should be called when PawnDefinition is null");

            Object.DestroyImmediate(go);
        }
    }
}
