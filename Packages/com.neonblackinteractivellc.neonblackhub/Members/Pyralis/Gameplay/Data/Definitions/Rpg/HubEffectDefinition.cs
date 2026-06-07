using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct HubEffectDefinition
    {
        public HubEffectKind kind;
        public string targetId;
        public string secondaryTargetId;
        public int quantity;
        public bool boolValue;
        public PlayerPanelRoute panelRoute;

        public string TargetId => Normalize(targetId);
        public string SecondaryTargetId => Normalize(secondaryTargetId);
        public int Quantity => quantity < 1 ? 1 : quantity;

        public void Sanitize()
        {
            targetId = TargetId;
            secondaryTargetId = SecondaryTargetId;
            quantity = Quantity;
        }

        public HubInteractionEffect CreateRuntimeEffect()
        {
            return new HubInteractionEffect(kind, TargetId, SecondaryTargetId, Quantity, boolValue, panelRoute);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
