using System;
using System.Collections.Generic;
using Game.Items;

namespace Game.Inventory
{
    /// <summary>One occupied or empty slot.</summary>
    public class InventorySlot
    {
        public ItemData Item { get; private set; }
        public int Quantity { get; private set; }
        public bool IsEmpty => Item == null;

        public void Set(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public void AddQuantity(int amount) => Quantity += amount;

        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }
    }

    /// <summary>
    /// Pure data + logic. No MonoBehaviour, no UI reference. Fires events so any
    /// number of UI views (or nothing at all, e.g. in a unit test) can react.
    /// This separation is what lets the UI be swapped or tested independently.
    /// </summary>
    public class InventorySystem
    {
        private readonly InventorySlot[] slots;

        public event Action OnInventoryChanged;
        public event Action<ItemData> OnAddFailed; // e.g. inventory full

        public InventorySystem(int slotCount)
        {
            slots = new InventorySlot[slotCount];
            for (int i = 0; i < slotCount; i++) slots[i] = new InventorySlot();
        }

        public IReadOnlyList<InventorySlot> Slots => slots;

        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item.isStackable)
            {
                // try to merge into an existing stack of the same item first
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty && slot.Item.itemId == item.itemId && slot.Quantity < item.maxStackSize)
                    {
                        int spaceLeft = item.maxStackSize - slot.Quantity;
                        int amountToAdd = Math.Min(spaceLeft, quantity);
                        slot.AddQuantity(amountToAdd);
                        quantity -= amountToAdd;

                        if (quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            // non-stackable, or stackable with leftover quantity: needs a fresh slot
            var emptySlot = FindEmptySlot();
            if (emptySlot == null)
            {
                OnAddFailed?.Invoke(item);
                return false;
            }

            int qtyForNewSlot = item.isStackable ? Math.Min(quantity, item.maxStackSize) : 1;
            emptySlot.Set(item, qtyForNewSlot);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool TryRemoveFromSlot(int slotIndex, out ItemData removedItem, out int removedQuantity)
        {
            removedItem = null;
            removedQuantity = 0;

            if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex].IsEmpty)
                return false;

            var slot = slots[slotIndex];
            removedItem = slot.Item;
            removedQuantity = slot.Quantity;
            slot.Clear();

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool IsFull()
        {
            foreach (var slot in slots)
                if (slot.IsEmpty) return false;
            return true;
        }

        private InventorySlot FindEmptySlot()
        {
            foreach (var slot in slots)
                if (slot.IsEmpty) return slot;
            return null;
        }
    }
}
