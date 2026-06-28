using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Interactions
{
    public sealed class ActorInteractionContext
    {
        public ActorInteractionContext(
            GameObject actorObject,
            ParticipantHandle participant = null,
            IActorAnimationController animation = null)
        {
            ActorObject = actorObject;
            ActorTransform = actorObject != null ? actorObject.transform : null;
            Participant = participant;
            Animation = animation;
        }

        public GameObject ActorObject { get; }
        public Transform ActorTransform { get; }
        public ParticipantHandle Participant { get; }
        public IActorAnimationController Animation { get; }

        public static ActorInteractionContext FromActor(GameObject actorObject)
        {
            if (actorObject == null)
                return null;

            IPawnParticipantStateReader participantState = actorObject.GetComponent<IPawnParticipantStateReader>();
            IActorAnimationController animation = actorObject.GetComponent<IActorAnimationController>();
            return new ActorInteractionContext(actorObject, participantState?.Participant, animation);
        }
    }
}
