using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Loadout Panel Presenter")]
    public sealed class RpgLoadoutPanelPresenter : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Route")]
        [SerializeField] private RpgPanelRoutePresenter routePresenter;

        [Header("Definitions")]
        [SerializeField] private EquipmentSlotDefinition[] slots = Array.Empty<EquipmentSlotDefinition>();
        [SerializeField] private EquippableItemDefinition[] items = Array.Empty<EquippableItemDefinition>();

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI loadoutLabel;
        [SerializeField] private TextMeshProUGUI selectedItemLabel;
        [SerializeField] private TextMeshProUGUI equippedSlotsLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Controls")]
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Copy")]
        [SerializeField] private string emptyLoadoutText = "No equippable items available.";

        private EquipmentService _equipmentService;
        private IEquipmentSlot[] _runtimeSlots = Array.Empty<IEquipmentSlot>();
        private IEquippableItem[] _runtimeItems = Array.Empty<IEquippableItem>();
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;
        private int _selectedIndex;

        public RpgLoadoutEntry[] Entries { get; private set; } = Array.Empty<RpgLoadoutEntry>();
        public string LastIssue { get; private set; } = string.Empty;
        public int SelectedIndex => _selectedIndex;
        public RpgLoadoutEntry SelectedEntry => Entries.Length > 0 && _selectedIndex >= 0 && _selectedIndex < Entries.Length ? Entries[_selectedIndex] : default;

        [Inject]
        private void Construct(EquipmentService equipment = null)
        {
            if (_equipmentService == null)
                _equipmentService = equipment ?? new EquipmentService();
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureService();
        }

        private void OnEnable()
        {
            BindRoutePresenter();
            BindButtons();
            RefreshEntries();
        }

        private void OnDisable()
        {
            UnbindRoutePresenter();
            UnbindButtons();
        }

        public void ConfigureForTests(RpgOwnerKey owner, EquipmentService service, IEquipmentSlot[] equipmentSlots, IEquippableItem[] equippableItems)
        {
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _equipmentService = service ?? new EquipmentService();
            _runtimeSlots = equipmentSlots ?? Array.Empty<IEquipmentSlot>();
            _runtimeItems = equippableItems ?? Array.Empty<IEquippableItem>();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute != PlayerPanelRoute.Loadout)
                return false;

            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public void SelectNextItem()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % Entries.Length;
            Render();
        }

        public void SelectPreviousItem()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex - 1 + Entries.Length) % Entries.Length;
            Render();
        }

        public bool SelectItem(string itemId)
        {
            string normalizedItemId = Normalize(itemId);
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].ItemId != normalizedItemId)
                    continue;

                _selectedIndex = i;
                Render();
                return true;
            }

            return Fail($"Item `{normalizedItemId}` is not in this loadout list.");
        }

        public bool EquipSelectedItem()
        {
            RpgLoadoutEntry selected = SelectedEntry;
            if (string.IsNullOrEmpty(selected.ItemId))
                return Fail("No item is selected.");

            if (!TryGetItem(selected.ItemId, out IEquippableItem item))
                return Fail($"Item `{selected.ItemId}` could not be found.");

            if (!TryGetFirstCompatibleSlot(item, out IEquipmentSlot slot))
                return Fail($"Item `{selected.ItemId}` has no compatible configured slot.");

            EnsureService();
            if (!_equipmentService.TryEquip(ResolveOwner(), slot, item, out string issue))
                return Fail(issue);

            LastIssue = string.Empty;
            RefreshEntries();
            SelectItem(selected.ItemId);
            return true;
        }

        public bool UnequipSelectedItem()
        {
            RpgLoadoutEntry selected = SelectedEntry;
            if (string.IsNullOrEmpty(selected.ItemId))
                return Fail("No item is selected.");

            if (!TryGetEquippedSlotForItem(selected.ItemId, out string slotId))
                return Fail($"Item `{selected.ItemId}` is not currently equipped.");

            EnsureService();
            if (!_equipmentService.TryUnequip(ResolveOwner(), slotId, out _))
                return Fail($"Slot `{slotId}` could not be unequipped.");

            LastIssue = string.Empty;
            RefreshEntries();
            SelectItem(selected.ItemId);
            return true;
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (routePresenter == null)
                yield return "`RpgLoadoutPanelPresenter` should reference the Loadout route presenter or live below one.";

            if ((slots == null || slots.Length == 0) && (_runtimeSlots == null || _runtimeSlots.Length == 0))
                yield return "`RpgLoadoutPanelPresenter` should reference at least one Equipment Slot Definition.";

            if ((items == null || items.Length == 0) && (_runtimeItems == null || _runtimeItems.Length == 0))
                yield return "`RpgLoadoutPanelPresenter` should reference at least one Equippable Item Definition.";

            if (loadoutLabel == null && selectedItemLabel == null)
                yield return "`RpgLoadoutPanelPresenter` should reference a loadout list or selected item label.";

            if (equipButton == null && unequipButton == null)
                yield return "`RpgLoadoutPanelPresenter` needs Equip Button, Unequip Button, or a project input bridge calling EquipSelectedItem()/UnequipSelectedItem().";
        }

        private void HandlePanelOpened(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void RefreshEntries()
        {
            IEquippableItem[] equippableItems = GetItems();
            List<RpgLoadoutEntry> entries = new List<RpgLoadoutEntry>();
            for (int i = 0; i < equippableItems.Length; i++)
            {
                IEquippableItem item = equippableItems[i];
                if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                    continue;

                entries.Add(new RpgLoadoutEntry(
                    item.ItemId,
                    GetItemTitle(item),
                    BuildSlotSummary(item),
                    IsItemEquipped(item.ItemId)));
            }

            Entries = entries.ToArray();
            if (_selectedIndex >= Entries.Length)
                _selectedIndex = Math.Max(0, Entries.Length - 1);

            Render();
        }

        private void Render()
        {
            if (loadoutLabel != null)
                loadoutLabel.text = BuildLoadoutText();

            RpgLoadoutEntry selected = SelectedEntry;
            if (selectedItemLabel != null)
                selectedItemLabel.text = string.IsNullOrEmpty(selected.ItemId) ? string.Empty : selected.Title + " - " + selected.SlotSummary;

            if (equippedSlotsLabel != null)
                equippedSlotsLabel.text = BuildEquippedSlotsText();

            if (issueLabel != null)
                issueLabel.text = LastIssue;

            bool hasSelection = !string.IsNullOrEmpty(selected.ItemId);
            if (equipButton != null)
                equipButton.interactable = hasSelection && !selected.IsEquipped;

            if (unequipButton != null)
                unequipButton.interactable = hasSelection && selected.IsEquipped;

            bool hasMultiple = Entries.Length > 1;
            if (nextButton != null)
                nextButton.interactable = hasMultiple;
            if (previousButton != null)
                previousButton.interactable = hasMultiple;
        }

        private string BuildLoadoutText()
        {
            if (Entries.Length == 0)
                return emptyLoadoutText;

            string[] lines = new string[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                string marker = i == _selectedIndex ? "> " : "  ";
                string state = Entries[i].IsEquipped ? "equipped" : "available";
                lines[i] = marker + Entries[i].Title + " - " + Entries[i].SlotSummary + " - " + state;
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private string BuildEquippedSlotsText()
        {
            IEquipmentSlot[] equipmentSlots = GetSlots();
            if (equipmentSlots.Length == 0)
                return string.Empty;

            string[] lines = new string[equipmentSlots.Length];
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                IEquipmentSlot slot = equipmentSlots[i];
                string slotId = slot != null ? slot.SlotId : string.Empty;
                string slotTitle = GetSlotTitle(slot);
                string equippedItemId = _equipmentService != null ? _equipmentService.GetEquippedItemId(ResolveOwner(), slotId) : string.Empty;
                lines[i] = slotTitle + ": " + (string.IsNullOrEmpty(equippedItemId) ? "empty" : GetItemTitleById(equippedItemId));
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private string BuildSlotSummary(IEquippableItem item)
        {
            IEquipmentSlot[] equipmentSlots = GetSlots();
            List<string> titles = new List<string>();
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                IEquipmentSlot slot = equipmentSlots[i];
                if (slot != null && item.CanEquipInSlot(slot.SlotId))
                    titles.Add(GetSlotTitle(slot));
            }

            return titles.Count > 0 ? string.Join(", ", titles) : "No compatible slots";
        }

        private bool TryGetItem(string itemId, out IEquippableItem item)
        {
            string normalizedItemId = Normalize(itemId);
            IEquippableItem[] equippableItems = GetItems();
            for (int i = 0; i < equippableItems.Length; i++)
            {
                if (equippableItems[i] != null && equippableItems[i].ItemId == normalizedItemId)
                {
                    item = equippableItems[i];
                    return true;
                }
            }

            item = null;
            return false;
        }

        private bool TryGetFirstCompatibleSlot(IEquippableItem item, out IEquipmentSlot slot)
        {
            IEquipmentSlot[] equipmentSlots = GetSlots();
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                if (equipmentSlots[i] != null && item.CanEquipInSlot(equipmentSlots[i].SlotId))
                {
                    slot = equipmentSlots[i];
                    return true;
                }
            }

            slot = null;
            return false;
        }

        private bool TryGetEquippedSlotForItem(string itemId, out string slotId)
        {
            string normalizedItemId = Normalize(itemId);
            IEquipmentSlot[] equipmentSlots = GetSlots();
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                IEquipmentSlot slot = equipmentSlots[i];
                if (slot == null)
                    continue;

                if (_equipmentService != null && _equipmentService.GetEquippedItemId(ResolveOwner(), slot.SlotId) == normalizedItemId)
                {
                    slotId = slot.SlotId;
                    return true;
                }
            }

            slotId = string.Empty;
            return false;
        }

        private bool IsItemEquipped(string itemId)
        {
            return TryGetEquippedSlotForItem(itemId, out _);
        }

        private IEquipmentSlot[] GetSlots()
        {
            if (_runtimeSlots != null && _runtimeSlots.Length > 0)
                return _runtimeSlots;

            EquipmentSlotDefinition[] definitions = slots ?? Array.Empty<EquipmentSlotDefinition>();
            IEquipmentSlot[] result = new IEquipmentSlot[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
                result[i] = definitions[i];

            return result;
        }

        private IEquippableItem[] GetItems()
        {
            if (_runtimeItems != null && _runtimeItems.Length > 0)
                return _runtimeItems;

            EquippableItemDefinition[] definitions = items ?? Array.Empty<EquippableItemDefinition>();
            IEquippableItem[] result = new IEquippableItem[definitions.Length];
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
            if (_equipmentService == null)
                _equipmentService = new EquipmentService();
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
            equipButton?.onClick.AddListener(EquipSelectedItemFromButton);
            unequipButton?.onClick.AddListener(UnequipSelectedItemFromButton);
            nextButton?.onClick.AddListener(SelectNextItem);
            previousButton?.onClick.AddListener(SelectPreviousItem);
        }

        private void UnbindButtons()
        {
            equipButton?.onClick.RemoveListener(EquipSelectedItemFromButton);
            unequipButton?.onClick.RemoveListener(UnequipSelectedItemFromButton);
            nextButton?.onClick.RemoveListener(SelectNextItem);
            previousButton?.onClick.RemoveListener(SelectPreviousItem);
        }

        private void EquipSelectedItemFromButton()
        {
            EquipSelectedItem();
        }

        private void UnequipSelectedItemFromButton()
        {
            UnequipSelectedItem();
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            Render();
            return false;
        }

        private string GetItemTitleById(string itemId)
        {
            if (TryGetItem(itemId, out IEquippableItem item))
                return GetItemTitle(item);

            return itemId;
        }

        private static string GetItemTitle(IEquippableItem item)
        {
            EquippableItemDefinition definition = item as EquippableItemDefinition;
            if (definition != null && !string.IsNullOrWhiteSpace(definition.displayName))
                return definition.displayName.Trim();

            return item != null ? item.ItemId : string.Empty;
        }

        private static string GetSlotTitle(IEquipmentSlot slot)
        {
            EquipmentSlotDefinition definition = slot as EquipmentSlotDefinition;
            if (definition != null && !string.IsNullOrWhiteSpace(definition.displayName))
                return definition.displayName.Trim();

            return slot != null ? slot.SlotId : string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
