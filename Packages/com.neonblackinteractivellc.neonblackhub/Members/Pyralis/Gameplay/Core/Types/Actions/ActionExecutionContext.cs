using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Participants;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Actions
{
    public sealed class ActionExecutionContext
    {
        public string ActionId { get; }
        public GameObject SourceObject { get; }
        public GameObject OwnerObject { get; }
        public ParticipantId ParticipantId { get; }
        public Faction SourceFaction { get; }
        public ActionTargetDescriptor[] Targets { get; }
        public object CustomPayload { get; }
        public Transform SourceTransform => SourceObject != null ? SourceObject.transform : null;

        public ActionExecutionContext(
            string actionId,
            GameObject sourceObject = null,
            GameObject ownerObject = null,
            ParticipantId participantId = default,
            Faction sourceFaction = Faction.Neutral,
            ActionTargetDescriptor[] targets = null,
            object customPayload = null)
        {
            ActionId = actionId ?? string.Empty;
            SourceObject = sourceObject;
            OwnerObject = ownerObject;
            ParticipantId = participantId;
            SourceFaction = sourceFaction;
            Targets = targets ?? System.Array.Empty<ActionTargetDescriptor>();
            CustomPayload = customPayload;
        }
    }
}
