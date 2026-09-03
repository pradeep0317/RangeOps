using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// Pure view layer. Only listens to InventorySystem events and redraws.
    /// Never calls TryAddItem/TryRemoveFromSlot directly except in response
    /// to a user action like clicking "Drop" on a slot widget.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventoryHolder inventoryHolder;
        [SerializeField] private GameObject fullInventoryWarning; // e.g. a small toast/text object

        private InventorySystem inventory;

        private void Start()
        {
            inventory = inventoryHolder.Inventory;
            inventory.OnInventoryChanged += RefreshSlots;
            inventory.OnAddFailed += ShowFullWarning;

            RefreshSlots();
        }

        private void OnDestroy()
        {
            if (inventory == null) return;
            inventory.OnInventoryChanged -= RefreshSlots;
            inventory.OnAddFailed -= ShowFullWarning;
        }

        private void RefreshSlots()
        {
            // for each slot in inventory.Slots, update the matching UI slot widget
            // (icon, quantity text, empty-state) — implementation depends on your
            // UI toolkit (uGUI/UI Toolkit), omitted here since it's pure presentation
        }

        private void ShowFullWarning(Items.ItemData attemptedItem)
        {
            if (fullInventoryWarning != null)
                fullInventoryWarning.SetActive(true);
            // hide again after a short delay, e.g. via a coroutine
        }

        /// <summary>Called by a slot widget's "Drop" button.</summary>
        public void OnDropButtonPressed(int slotIndex)
        {
            if (inventory.TryRemoveFromSlot(slotIndex, out var item, out var qty))
            {
                DropHandler.SpawnDroppedItem(item, qty, inventoryHolder.transform);
            }
        }
    }
}
