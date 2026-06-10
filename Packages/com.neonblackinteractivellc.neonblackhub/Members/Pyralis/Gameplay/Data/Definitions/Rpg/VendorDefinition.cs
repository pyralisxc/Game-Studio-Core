using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.vendor.definition",
        Capability = AuthoringCapability.Inventory,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(vendorId), nameof(displayName), nameof(offers) },
        FirstProof = "Proof that the vendor offers valid items and prices are correctly defined."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Vendor", fileName = "VendorDefinition")]
    public class VendorDefinition : ScriptableObject, IVendorDefinition
{
        public string vendorId = "vendor.new";
        public string displayName = "New Vendor";
        public VendorOfferDefinition[] offers = Array.Empty<VendorOfferDefinition>();

        public string VendorId => Normalize(vendorId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? VendorId : displayName.Trim();
        public VendorOfferDefinition[] OfferDefinitions => offers ?? Array.Empty<VendorOfferDefinition>();
        public VendorOffer[] Offers => OfferDefinitions.Select(offer => offer.CreateRuntimeOffer()).ToArray();

        public void Sanitize()
        {
            vendorId = VendorId;
            displayName = DisplayName;
            offers = OfferDefinitions;
            for (int i = 0; i < offers.Length; i++)
                offers[i].Sanitize();
        }

        public bool TryGetOffer(string offerId, out VendorOffer offer)
        {
            string normalizedOfferId = Normalize(offerId);
            VendorOffer[] runtimeOffers = Offers;
            for (int i = 0; i < runtimeOffers.Length; i++)
            {
                if (runtimeOffers[i].OfferId == normalizedOfferId)
                {
                    offer = runtimeOffers[i];
                    return true;
                }
            }

            offer = default;
            return false;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();
            if (string.IsNullOrWhiteSpace(vendorId))
                issues.Add("Vendor id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            VendorOfferDefinition[] definitions = OfferDefinitions;
            if (definitions.Length == 0)
                issues.Add("At least one vendor offer is required.");

            HashSet<string> offerIds = new HashSet<string>();
            for (int i = 0; i < definitions.Length; i++)
            {
                VendorOfferDefinition offer = definitions[i];
                if (string.IsNullOrWhiteSpace(offer.OfferId))
                    issues.Add($"Offers[{i}] Offer id is required.");
                else if (!offerIds.Add(offer.OfferId))
                    issues.Add($"Vendor offer `{offer.OfferId}` is assigned more than once.");

                if (string.IsNullOrWhiteSpace(offer.ItemId))
                    issues.Add($"Vendor offer `{offer.OfferId}` Item id is required.");

                if ((offer.buyPrice > 0 || offer.sellPrice > 0) && string.IsNullOrWhiteSpace(offer.CurrencyItemId))
                    issues.Add($"Vendor offer `{offer.OfferId}` Currency item id is required when prices are greater than zero.");

                if (!offer.canBuy && !offer.canSell)
                    issues.Add($"Vendor offer `{offer.OfferId}` should be buyable, sellable, or both.");
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
