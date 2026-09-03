using UnityEngine;
using Game.Interfaces;
using Game.Inventory;

namespace Game.Items
{
    /// <summary>
    /// Sits on any pickup-able object in the world (gun on the rack, ammo box,
    /// medkit). Knows nothing about inventory internals — it just asks the
    /// player's InventorySystem to add itself and reports success/fail.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        public string GetInteractionPrompt()
        {
            return quantity > 1
                ? $"Press E to pick up {itemData.displayName} x{quantity}"
                : $"Press E to pick up {itemData.displayName}";
        }

        public void Interact(GameObject interactor)
        {
            var inventory = interactor.GetComponent<PlayerInventoryHolder>()?.Inventory;
            if (inventory == null) return;

            bool added = inventory.TryAddItem(itemData, quantity);
            if (added)
            {
                gameObject.SetActive(false); // remove from world once picked up
            }
            // if not added (inventory full), the UI layer shows feedback via
            // InventorySystem's OnAddFailed event — WorldItem doesn't need to know why
        }
    }
}
