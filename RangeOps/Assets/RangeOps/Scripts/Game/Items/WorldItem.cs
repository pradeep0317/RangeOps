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
            if (itemData == null)
                return;

            // Primary / Secondary
            if (itemData.category == ItemCategory.Primary ||
                itemData.category == ItemCategory.Secondary)
            {
                EquipmentController equipment =
                    interactor.GetComponent<EquipmentController>();

                if (equipment == null)
                    return;

                bool equipped =
                    equipment.Equip(itemData, gameObject);

                // Don't deactivate.
                // The same object becomes the held weapon.
                return;
            }

            // Attachment
            if (itemData.category == ItemCategory.Attachment)
            {
                EquipmentController equipment =
                    interactor.GetComponent<EquipmentController>();

                if (equipment == null)
                    return;

                bool equipped =
                    equipment.EquipAttachment(itemData);

                if (equipped)
                    gameObject.SetActive(false);

                return;
            }

            // Ammo
            PlayerInventoryHolder holder =
                interactor.GetComponent<PlayerInventoryHolder>();

            if (holder == null)
                return;

            bool added =
                holder.Inventory.TryAddItem(itemData, quantity);

            if (added)
                gameObject.SetActive(false);
        }
    }
}