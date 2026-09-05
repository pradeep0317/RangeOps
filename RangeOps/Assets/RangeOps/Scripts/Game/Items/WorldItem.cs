using UnityEngine;
using Game.Interfaces;
using Game.Inventory;

namespace Game.Items
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        public string GetInteractionPrompt()
        {
            if (quantity > 1)
                return $"Press E to pick up {itemData.displayName} x{quantity}";

            return $"Press E to pick up {itemData.displayName}";
        }

        public void Interact(GameObject interactor)
        {
            if (itemData.category == ItemCategory.Primary || itemData.category == ItemCategory.Secondary)
            {
                var equipment = interactor.GetComponent<EquipmentController>();
                if (equipment == null) return;

                equipment.Equip(itemData, gameObject);
                return;
            }

            if (itemData.category == ItemCategory.Attachment)
            {
                var equipment = interactor.GetComponent<EquipmentController>();
                if (equipment == null) return;

                bool equipped = equipment.EquipAttachment(itemData);
                if (equipped)
                {
                    var rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.detectCollisions = false;
                    }

                    gameObject.SetActive(false);
                }
                return;
            }

            var inventory = interactor.GetComponent<PlayerInventoryHolder>()?.Inventory;
            if (inventory == null) return;

            bool added = inventory.TryAddItem(itemData, quantity);
            if (added)
            {
                gameObject.SetActive(false);
            }
        }
    }
}