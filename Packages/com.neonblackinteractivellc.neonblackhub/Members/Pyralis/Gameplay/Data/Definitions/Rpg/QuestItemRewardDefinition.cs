using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct QuestItemRewardDefinition
    {
        public string itemId;
        public int quantity;

        public QuestItemRewardDefinition(string itemId, int quantity)
        {
            this.itemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            this.quantity = quantity < 1 ? 1 : quantity;
        }

        public string ItemId => string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
        public int Quantity => quantity < 1 ? 1 : quantity;
        public bool IsValid => !string.IsNullOrWhiteSpace(ItemId) && Quantity > 0;

        public void Sanitize()
        {
            itemId = ItemId;
            quantity = Quantity;
        }

        public QuestItemReward CreateRuntimeReward()
        {
            return new QuestItemReward(ItemId, Quantity);
        }
    }
}
