using System;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgVendorPanelPresenterRuntimeTests
    {
        [Test]
        public void RpgVendorPanelPresenter_ShowInteractionResult_ListsVendorOffers()
        {
            GameObject root = new GameObject("Vendor Panel");
            try
            {
                RpgVendorPanelPresenter presenter = root.AddComponent<RpgVendorPanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, new VendorService(new InventoryService()), new IVendorDefinition[] { new TestVendor() });

                Assert.That(presenter.ShowInteractionResult(Result("vendor.apothecary")), Is.True, presenter.LastIssue);

                Assert.That(presenter.Entries.Length, Is.EqualTo(1));
                Assert.That(presenter.Entries[0].OfferId, Is.EqualTo("offer.potion"));
                Assert.That(presenter.Entries[0].Title, Is.EqualTo("Potion"));
                Assert.That(presenter.Entries[0].PriceText, Is.EqualTo("3 item.gold"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgVendorPanelPresenter_BuySelectedOffer_UpdatesInventory()
        {
            GameObject root = new GameObject("Vendor Panel");
            try
            {
                InventoryService inventory = new InventoryService();
                VendorService service = new VendorService(inventory);
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                Assert.That(inventory.TryAddItem(owner, "item.gold", 5, out string addIssue), Is.True, addIssue);

                RpgVendorPanelPresenter presenter = root.AddComponent<RpgVendorPanelPresenter>();
                presenter.ConfigureForTests(owner, service, new IVendorDefinition[] { new TestVendor() });
                presenter.ShowInteractionResult(Result("vendor.apothecary"));

                Assert.That(presenter.BuySelectedOffer(), Is.True, presenter.LastIssue);

                Assert.That(inventory.GetItemCount(owner, "item.gold"), Is.EqualTo(2));
                Assert.That(inventory.GetItemCount(owner, "item.potion"), Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static HubInteractionResult Result(string vendorId)
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                PlayerPanelRoute.Vendor,
                string.Empty,
                string.Empty,
                vendorId,
                Array.Empty<HubNotificationPayload>());
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
