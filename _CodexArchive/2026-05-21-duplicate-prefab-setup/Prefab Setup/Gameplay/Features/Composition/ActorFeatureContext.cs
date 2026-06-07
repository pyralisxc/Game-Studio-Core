using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Shared runtime context for actor feature modules across pawns, enemies, and future actor types.
    /// </summary>
    public sealed class ActorFeatureContext
    {
        private readonly List<ScriptableObject> _profiles = new List<ScriptableObject>(8);
        public GameObject ActorObject { get; }
        public Transform ActorTransform { get; }
        public ParticipantHandle Participant { get; }
        public PawnDefinition PawnDefinition { get; }
        public GameModeDefinition GameMode { get; }
        public IActorHealthState Health { get; }
        public IActorAnimationController Animation { get; }
        public IActorKnockbackController Knockback { get; }
        public IEnemyActorState EnemyActorState { get; }
        public ActorPresentationMode PresentationMode { get; }
        public Faction Faction => Health != null ? Health.Faction : Faction.Neutral;

        public IReadOnlyList<ScriptableObject> AuthoredProfiles => _profiles;

        public ActorFeatureContext(
            GameObject actorObject,
            ParticipantHandle participant = null,
            PawnDefinition pawnDefinition = null,
            GameModeDefinition gameMode = null,
            IActorHealthState health = null,
            IActorAnimationController animation = null,
            IActorKnockbackController knockback = null,
            IEnemyActorState enemyActorState = null,
            ActorPresentationMode presentationMode = ActorPresentationMode.Sprite2D,
            IEnumerable<ScriptableObject> authoredProfiles = null)
        {
            ActorObject = actorObject;
            ActorTransform = actorObject != null ? actorObject.transform : null;
            Participant = participant;
            PawnDefinition = pawnDefinition;
            GameMode = gameMode;
            Health = health;
            Animation = animation;
            Knockback = knockback;
            EnemyActorState = enemyActorState;
            PresentationMode = presentationMode;

            if (authoredProfiles == null)
                return;

            foreach (ScriptableObject profile in authoredProfiles)
            {
                if (profile != null)
                    _profiles.Add(profile);
            }
        }

        public T GetProfile<T>(ScriptableObject preferred = null) where T : ScriptableObject
        {
            if (preferred is T preferredProfile)
                return preferredProfile;

            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i] is T profile)
                    return profile;
            }

            return null;
        }
    }
}
