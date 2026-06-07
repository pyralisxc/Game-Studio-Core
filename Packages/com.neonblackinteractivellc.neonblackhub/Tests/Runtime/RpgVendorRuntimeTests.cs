using System;
using NeonBlack.Gameplay.Core.Rpg;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgVendorRuntimeTests
    {
        [Test]
        public void VendorService_TryBuy_RemovesCurrencyAndAddsItem()
        {
            InventoryService inventory = new InventoryService();
            VendorService service = new VendorService(inventory);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            TestVendor vendor = new TestVendor();

            Assert.That(inventory.TryAddItem(owner, "item.gold", 10, out string addIssue), Is.True, addIssue);

            Assert.That(service.TryBuy(owner, vendor, "offer.potion", 2, out VendorTransactionResult result, out string issue), Is.True, issue);

            Assert.That(inventory.GetItemCount(owner, "item.gold"), Is.EqualTo(4));
            Assert.That(inventory.GetItemCount(owner, "item.potion"), Is.EqualTo(2));
            Assert.That(result.Quantity, Is.EqualTo(2));
            Assert.That(result.TotalPrice, Is.EqualTo(6));
        }

        [Test]
        public void VendorService_TrySell_RemovesItemAndAddsCurrency()
        {
            InventoryService inventory = new InventoryService();
            VendorService service = new VendorService(inventory);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            TestVendor vendor = new TestVendor();

            Assert.That(inventory.TryAddItem(owner, "item.potion", 2, out string addIssue), Is.True, addIssue);

            Assert.That(service.TrySell(owner, vendor, "offer.potion", 1, out VendorTransactionResult result, out string issue), Is.True, issue);

            Assert.That(inventory.GetItemCount(owner, "item.potion"), Is.EqualTo(1));
            Assert.That(inventory.GetItemCount(owner, "item.gold"), Is.EqualTo(1));
            Assert.That(result.TotalPrice, Is.EqualTo(1));
        }

        private sealed class TestVendor : IVendorDefinition
        {
            public string VendorId => "vendor.apothecary";
            public string DisplayName => "Apothecary";
            public VendorOffer[] Offers => new[]
            {
                new VendorOffer("offer.potion", "Potion", "item.potion", "item.gold", 3, 1, true, true)
            };

            public bool TryGetOffer(string offerId, out VendorOffer offer)
            {
                if (offerId == "offer.potion")
                {
                    offer = Offers[0];
                    return true;
                }

                offer = default;
                return false;
            }
        }
    }
}
