using System;
using System.Collections.Generic;
using Game.Items;

namespace Game.Inventory
{
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

        public void AddQuantity(int amount)
        {
            Quantity += amount;
        }

        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }
    }

    public class InventorySystem
    {
        private readonly InventorySlot[] slots;

        public event Action OnInventoryChanged;
        public event Action<ItemData> OnAddFailed;

        public InventorySystem(int slotCount)
        {
            slots = new InventorySlot[slotCount];

            for (int i = 0; i < slotCount; i++)
                slots[i] = new InventorySlot();
        }

        public IReadOnlyList<InventorySlot> Slots => slots;

        public bool TryAddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;

            int remaining = quantity;

            // Fill existing stacks first
            if (item.isStackable)
            {
                foreach (InventorySlot slot in slots)
                {
                    if (slot.IsEmpty)
                        continue;

                    if (slot.Item.itemId != item.itemId)
                        continue;

                    if (slot.Quantity >= item.maxStackSize)
                        continue;

                    int space = item.maxStackSize - slot.Quantity;
                    int amount = Math.Min(space, remaining);

                    slot.AddQuantity(amount);
                    remaining -= amount;

                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            // Create new stacks / slots
            while (remaining > 0)
            {
                InventorySlot emptySlot = FindEmptySlot();

                if (emptySlot == null)
                {
                    OnAddFailed?.Invoke(item);
                    return false;
                }

                int amountForSlot = item.isStackable
                    ? Math.Min(remaining, item.maxStackSize)
                    : 1;

                emptySlot.Set(item, amountForSlot);

                remaining -= amountForSlot;

                // Non-stackable item occupies exactly one slot
                if (!item.isStackable)
                    remaining = 0;
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool TryRemoveFromSlot(
            int slotIndex,
            out ItemData removedItem,
            out int removedQuantity)
        {
            removedItem = null;
            removedQuantity = 0;

            if (slotIndex < 0 ||
                slotIndex >= slots.Length ||
                slots[slotIndex].IsEmpty)
            {
                return false;
            }

            InventorySlot slot = slots[slotIndex];

            removedItem = slot.Item;
            removedQuantity = slot.Quantity;

            slot.Clear();

            OnInventoryChanged?.Invoke();

            return true;
        }

        public bool IsFull()
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty)
                    return false;
            }

            return true;
        }

        private InventorySlot FindEmptySlot()
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty)
                    return slot;
            }

            return null;
        }
    }
}