using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Items;

namespace Game.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private PlayerInventoryHolder inventoryHolder;

        [Header("Primary Weapon")]
        [SerializeField] private Image primaryWeaponImage;
        [SerializeField] private TMP_Text primaryAmmoText;

        [Header("Secondary Weapon")]
        [SerializeField] private Image secondaryWeaponImage;

        [Header("Attachment Panel")]
        [SerializeField] private GameObject attachmentPanel;
        [SerializeField] private Image magazineImage;
        [SerializeField] private Image gripImage;
        [SerializeField] private Image scopeImage;

        [Header("Dynamic Component Area")]
        [SerializeField] private Transform componentParent;
        [SerializeField] private GameObject componentPrefab;

        [Header("Visual Split")]
        [SerializeField] private int visualChunkSize = 10;

        private InventorySystem inventory;
        private EquipmentController equipment;

        private void Start()
        {
            inventory = inventoryHolder.Inventory;

            equipment =
                inventoryHolder.GetComponent<EquipmentController>();

            inventory.OnInventoryChanged += RefreshUI;
            inventory.OnAddFailed += ShowFullWarning;

            if (equipment != null)
                equipment.OnEquipmentChanged += RefreshUI;

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= RefreshUI;
                inventory.OnAddFailed -= ShowFullWarning;
            }

            if (equipment != null)
                equipment.OnEquipmentChanged -= RefreshUI;
        }

        public void RefreshUI()
        {
            RefreshEquipment();
            RefreshComponents();
        }

        private void RefreshEquipment()
        {
            // PRIMARY WEAPON
            if (equipment != null &&
                equipment.CurrentPrimary != null)
            {
                primaryWeaponImage.gameObject.SetActive(true);

                primaryWeaponImage.sprite =
                    equipment.CurrentPrimary.icon;

                primaryAmmoText.text =
                    $"{equipment.CurrentAmmo}/" +
                    $"{equipment.CurrentPrimary.magazineCapacity}";
            }
            else
            {
                primaryWeaponImage.gameObject.SetActive(false);

                if (primaryAmmoText != null)
                    primaryAmmoText.text = "";
            }

            // SECONDARY WEAPON
            if (equipment != null &&
                equipment.CurrentSecondary != null)
            {
                secondaryWeaponImage.gameObject.SetActive(true);

                secondaryWeaponImage.sprite =
                    equipment.CurrentSecondary.icon;
            }
            else
            {
                secondaryWeaponImage.gameObject.SetActive(false);
            }

            RefreshAttachments();
        }
        
        private void RefreshAttachments()
        {
            if (equipment == null ||
                equipment.CurrentPrimary == null)
            {
                attachmentPanel.SetActive(false);
                return;
            }

            bool hasAttachment =
                equipment.CurrentMagazine != null ||
                equipment.CurrentGrip != null ||
                equipment.CurrentScope != null;

            attachmentPanel.SetActive(hasAttachment);

            // MAGAZINE
            if (equipment.CurrentMagazine != null)
            {
                magazineImage.gameObject.SetActive(true);

                magazineImage.sprite =
                    equipment.CurrentMagazine.icon;
            }
            else
            {
                magazineImage.gameObject.SetActive(false);
            }

            // GRIP
            if (equipment.CurrentGrip != null)
            {
                gripImage.gameObject.SetActive(true);

                gripImage.sprite =
                    equipment.CurrentGrip.icon;
            }
            else
            {
                gripImage.gameObject.SetActive(false);
            }

            // SCOPE
            if (equipment.CurrentScope != null)
            {
                scopeImage.gameObject.SetActive(true);

                scopeImage.sprite =
                    equipment.CurrentScope.icon;
            }
            else
            {
                scopeImage.gameObject.SetActive(false);
            }
        }

       

        private void RefreshComponents()
        {
            if (componentParent == null ||
                componentPrefab == null ||
                inventory == null)
                return;

            // Remove old generated components
            for (int i = componentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(
                    componentParent.GetChild(i).gameObject
                );
            }

            // Read inventory
            foreach (InventorySlot slot in inventory.Slots)
            {
                if (slot.IsEmpty)
                    continue;

                int remaining = slot.Quantity;

                while (remaining > 0)
                {
                    int amount;

                    // Stackable item
                    if (slot.Item.isStackable)
                    {
                        amount = Mathf.Min(
                            remaining,
                            visualChunkSize
                        );
                    }
                    else
                    {
                        amount = 1;
                    }

                    CreateComponent(
                        slot.Item,
                        amount
                    );

                    remaining -= amount;
                }
            }
        }

        private void CreateComponent(
            ItemData item,
            int quantity)
        {
            GameObject component =
                Instantiate(
                    componentPrefab,
                    componentParent
                );

            // Find elements inside the newly created prefab
            Image image =
                component.transform
                    .Find("Component_Image")
                    ?.GetComponent<Image>();

            TMP_Text count =
                component.transform
                    .Find("Component_Count_Text")
                    ?.GetComponent<TMP_Text>();

            TMP_Text name =
                component.transform
                    .Find("Component_Name_Text")
                    ?.GetComponent<TMP_Text>();

            // IMAGE
            if (image != null)
            {
                image.sprite = item.icon;
                image.gameObject.SetActive(true);
            }

            // COUNT
            if (count != null)
            {
                count.text = quantity.ToString();
            }

            // NAME
            if (name != null)
            {
                name.text = item.displayName;
            }
        }

      

        private void ShowFullWarning(ItemData item)
        {
            Debug.Log(
                $"Backpack Full - Can't pick up {item.displayName}"
            );
        }

      

        public void OnDropButtonPressed(int slotIndex)
        {
            if (inventory.TryRemoveFromSlot(
                slotIndex,
                out ItemData item,
                out int quantity))
            {
                DropHandler.SpawnDroppedItem(
                    item,
                    quantity,
                    inventoryHolder.transform
                );
            }
        }
    }
}