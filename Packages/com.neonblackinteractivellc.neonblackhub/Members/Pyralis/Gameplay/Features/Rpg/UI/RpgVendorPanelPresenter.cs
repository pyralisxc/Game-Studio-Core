using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AuthoringContract(
        ModuleId = "rpg.vendor.ui",
        Capability = AuthoringCapability.Inventory,
        Lane = "RPG",
        RequiredComponentNames = new[] { "TMPro.TextMeshProUGUI" },
        FirstProof = "Verify that the vendor panel displays offers and correctly calculates total prices."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Vendor Panel Presenter")]
    public sealed class RpgVendorPanelPresenter : MonoBehaviour, IRuntimeValidationProvider
{
        [Header("Route")]
        [SerializeField] private RpgPanelRoutePresenter routePresenter;

        [Header("Definitions")]
        [SerializeField] private VendorDefinition[] vendors = Array.Empty<VendorDefinition>();

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI vendorLabel;
        [SerializeField] private TextMeshProUGUI offerListLabel;
        [SerializeField] private TextMeshProUGUI selectedOfferLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Controls")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Copy")]
        [SerializeField] private string emptyVendorText = "No vendor offers available.";

        private VendorService _vendorService;
        private IVendorDefinition[] _runtimeVendors = Array.Empty<IVendorDefinition>();
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;
        private IVendorDefinition _activeVendor;
        private int _selectedIndex;

        public RpgVendorEntry[] Entries { get; private set; } = Array.Empty<RpgVendorEntry>();
        public string LastIssue { get; private set; } = string.Empty;
        public int SelectedIndex => _selectedIndex;
        public RpgVendorEntry SelectedEntry => Entries.Length > 0 && _selectedIndex >= 0 && _selectedIndex < Entries.Length ? Entries[_selectedIndex] : default;

        [Inject]
        private void Construct(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            BindRoutePresenter();
            BindButtons();
            Render();
        }

        private void OnDisable()
        {
            UnbindRoutePresenter();
            UnbindButtons();
        }

        public void ConfigureForTests(RpgOwnerKey owner, VendorService service, IVendorDefinition[] vendorDefinitions)
        {
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _vendorService = service;
            _runtimeVendors = vendorDefinitions ?? Array.Empty<IVendorDefinition>();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute != PlayerPanelRoute.Vendor)
                return false;

            string vendorId = !string.IsNullOrWhiteSpace(result.NpcId) ? result.NpcId : result.Prompt.InteractableId;
            if (!TryGetVendor(vendorId, out IVendorDefinition vendor))
                return Fail($"Vendor `{vendorId}` could not be found.");

            _activeVendor = vendor;
            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public void SelectNextOffer()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % Entries.Length;
            Render();
        }

        public void SelectPreviousOffer()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex - 1 + Entries.Length) % Entries.Length;
            Render();
        }

        public bool BuySelectedOffer()
        {
            RpgVendorEntry selected = SelectedEntry;
            if (string.IsNullOrWhiteSpace(selected.OfferId))
                return Fail("No vendor offer is selected.");

            if (!_vendorService.TryBuy(ResolveOwner(), _activeVendor, selected.OfferId, 1, out _, out string issue))
                return Fail(issue);

            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public bool SellSelectedOffer()
        {
            RpgVendorEntry selected = SelectedEntry;
            if (string.IsNullOrWhiteSpace(selected.OfferId))
                return Fail("No vendor offer is selected.");

            if (!_vendorService.TrySell(ResolveOwner(), _activeVendor, selected.OfferId, 1, out _, out string issue))
                return Fail(issue);

            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (routePresenter == null)
                yield return "`RpgVendorPanelPresenter` should reference the Vendor route presenter or live below one.";

            if ((vendors == null || vendors.Length == 0) && (_runtimeVendors == null || _runtimeVendors.Length == 0))
                yield return "`RpgVendorPanelPresenter` should reference at least one Vendor Definition.";

            if (offerListLabel == null && selectedOfferLabel == null)
                yield return "`RpgVendorPanelPresenter` should reference an offer list or selected offer label.";

            if (buyButton == null && sellButton == null)
                yield return "`RpgVendorPanelPresenter` needs Buy Button, Sell Button, or a project input bridge calling BuySelectedOffer()/SellSelectedOffer().";
        }

        private void HandlePanelOpened(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void RefreshEntries()
        {
            if (_activeVendor == null)
            {
                Entries = Array.Empty<RpgVendorEntry>();
                Render();
                return;
            }

            VendorOffer[] offers = _activeVendor.Offers ?? Array.Empty<VendorOffer>();
            List<RpgVendorEntry> entries = new List<RpgVendorEntry>();
            for (int i = 0; i < offers.Length; i++)
            {
                if (!offers[i].IsValid)
                    continue;

                entries.Add(new RpgVendorEntry(
                    offers[i].OfferId,
                    offers[i].DisplayName,
                    offers[i].ItemId,
                    BuildPriceText(offers[i]),
                    offers[i].CanBuy,
                    offers[i].CanSell));
            }

            Entries = entries.ToArray();
            if (_selectedIndex >= Entries.Length)
                _selectedIndex = Math.Max(0, Entries.Length - 1);
            Render();
        }

        private void Render()
        {
            if (vendorLabel != null)
                vendorLabel.text = _activeVendor != null ? _activeVendor.DisplayName : string.Empty;

            if (offerListLabel != null)
                offerListLabel.text = BuildOfferListText();

            RpgVendorEntry selected = SelectedEntry;
            if (selectedOfferLabel != null)
                selectedOfferLabel.text = string.IsNullOrWhiteSpace(selected.OfferId) ? string.Empty : selected.Title + " - " + selected.PriceText;

            if (issueLabel != null)
                issueLabel.text = LastIssue;

            if (buyButton != null)
                buyButton.interactable = selected.CanBuy;

            if (sellButton != null)
                sellButton.interactable = selected.CanSell;

            bool hasMultiple = Entries.Length > 1;
            if (nextButton != null)
                nextButton.interactable = hasMultiple;
            if (previousButton != null)
                previousButton.interactable = hasMultiple;
        }

        private string BuildOfferListText()
        {
            if (Entries.Length == 0)
                return emptyVendorText;

            string[] lines = new string[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                string marker = i == _selectedIndex ? "> " : "  ";
                lines[i] = marker + Entries[i].Title + " - " + Entries[i].PriceText;
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private bool TryGetVendor(string vendorId, out IVendorDefinition vendor)
        {
            string normalizedVendorId = Normalize(vendorId);
            IVendorDefinition[] allVendors = GetVendors();
            for (int i = 0; i < allVendors.Length; i++)
            {
                if (allVendors[i] != null && allVendors[i].VendorId == normalizedVendorId)
                {
                    vendor = allVendors[i];
                    return true;
                }
            }

            vendor = null;
            return false;
        }

        private IVendorDefinition[] GetVendors()
        {
            if (_runtimeVendors != null && _runtimeVendors.Length > 0)
                return _runtimeVendors;

            VendorDefinition[] definitions = vendors ?? Array.Empty<VendorDefinition>();
            IVendorDefinition[] result = new IVendorDefinition[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
                result[i] = definitions[i];

            return result;
        }

        private void ResolveReferences()
        {
            if (routePresenter == null)
                routePresenter = GetComponentInParent<RpgPanelRoutePresenter>() ?? GetComponentInChildren<RpgPanelRoutePresenter>(true);
        }

        private void EnsureService()
        {
            if (_vendorService == null)
                _vendorService = new VendorService();
        }

        private RpgOwnerKey ResolveOwner()
        {
            if (_hasRuntimeOwner)
                return _runtimeOwner;

            return new RpgOwnerKey(ownerKind, ownerStableId);
        }

        private void BindRoutePresenter()
        {
            ResolveReferences();
            if (routePresenter != null)
                routePresenter.PanelOpened += HandlePanelOpened;
        }

        private void UnbindRoutePresenter()
        {
            if (routePresenter != null)
                routePresenter.PanelOpened -= HandlePanelOpened;
        }

        private void BindButtons()
        {
            buyButton?.onClick.AddListener(BuySelectedOfferFromButton);
            sellButton?.onClick.AddListener(SellSelectedOfferFromButton);
            nextButton?.onClick.AddListener(SelectNextOffer);
            previousButton?.onClick.AddListener(SelectPreviousOffer);
        }

        private void UnbindButtons()
        {
            buyButton?.onClick.RemoveListener(BuySelectedOfferFromButton);
            sellButton?.onClick.RemoveListener(SellSelectedOfferFromButton);
            nextButton?.onClick.RemoveListener(SelectNextOffer);
            previousButton?.onClick.RemoveListener(SelectPreviousOffer);
        }

        private void BuySelectedOfferFromButton()
        {
            BuySelectedOffer();
        }

        private void SellSelectedOfferFromButton()
        {
            SellSelectedOffer();
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            Render();
            return false;
        }

        private static string BuildPriceText(VendorOffer offer)
        {
            if (offer.CanBuy)
                return offer.BuyPrice + " " + offer.CurrencyItemId;
            if (offer.CanSell)
                return offer.SellPrice + " " + offer.CurrencyItemId;
            return string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
