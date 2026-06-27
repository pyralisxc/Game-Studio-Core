using System;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Actions
{
    [Serializable]
    public struct ActionTargetDescriptor
    {
        public ActionTargetKind targetKind;
        public GameObject targetObject;
        public Vector3 worldPosition;
        public Vector3 direction;
        public string targetId;
        public Faction targetFaction;
        public bool hasWorldPosition;

        [NonSerialized] public object customPayload;

        public static ActionTargetDescriptor None()
        {
            return new ActionTargetDescriptor { targetKind = ActionTargetKind.None };
        }

        public static ActionTargetDescriptor Self(GameObject source, Faction faction = Faction.Neutral)
        {
            return new ActionTargetDescriptor
            {
                targetKind = ActionTargetKind.Self,
                targetObject = source,
                worldPosition = source != null ? source.transform.position : Vector3.zero,
                targetFaction = faction,
                hasWorldPosition = source != null
            };
        }

        public static ActionTargetDescriptor Actor(GameObject actor, Faction faction = Faction.Neutral)
        {
            return new ActionTargetDescriptor
            {
                targetKind = ActionTargetKind.Actor,
                targetObject = actor,
                worldPosition = actor != null ? actor.transform.position : Vector3.zero,
                targetFaction = faction,
                hasWorldPosition = actor != null
            };
        }

        public static ActionTargetDescriptor WorldPoint(Vector3 position)
        {
            return new ActionTargetDescriptor
            {
                targetKind = ActionTargetKind.WorldPoint,
                worldPosition = position,
                hasWorldPosition = true
            };
        }

        public static ActionTargetDescriptor Direction(Vector3 value)
        {
            return new ActionTargetDescriptor
            {
                targetKind = ActionTargetKind.Direction,
                direction = value.sqrMagnitude > 0f ? value.normalized : Vector3.zero
            };
        }

        public static ActionTargetDescriptor Id(ActionTargetKind kind, string id)
        {
            return new ActionTargetDescriptor
            {
                targetKind = kind,
                targetId = id ?? string.Empty
            };
        }

        public static ActionTargetDescriptor Custom(object payload, string id = "")
        {
            return new ActionTargetDescriptor
            {
                targetKind = ActionTargetKind.Custom,
                targetId = id ?? string.Empty,
                customPayload = payload
            };
        }

        public bool TryGetPosition(out Vector3 position)
        {
            if (targetObject != null)
            {
                position = targetObject.transform.position;
                return true;
            }

            if (hasWorldPosition)
            {
                position = worldPosition;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
