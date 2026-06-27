using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Actions
{
    public sealed class ActionExecutionContext
    {
        public string ActionId { get; }
        public GameObject SourceObject { get; }
        public GameObject OwnerObject { get; }
        public object Participant { get; }
        public Faction SourceFaction { get; }
        public ActionTargetDescriptor[] Targets { get; }
        public object CustomPayload { get; }
        public Transform SourceTransform => SourceObject != null ? SourceObject.transform : null;

        public ActionExecutionContext(
            string actionId,
            GameObject sourceObject = null,
            GameObject ownerObject = null,
            object participant = null,
            Faction sourceFaction = Faction.Neutral,
            ActionTargetDescriptor[] targets = null,
            object customPayload = null)
        {
            ActionId = actionId ?? string.Empty;
            SourceObject = sourceObject;
            OwnerObject = ownerObject;
            Participant = participant;
            SourceFaction = sourceFaction;
            Targets = targets ?? System.Array.Empty<ActionTargetDescriptor>();
            CustomPayload = customPayload;
        }
    }
}
