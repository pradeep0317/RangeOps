using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EasyUI.Toast;
using Game.Items;

namespace Game.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private PlayerInventoryHolder inventoryHolder;

        [Header("Equipment (assign directly - do not rely on GetComponent)")]
        [SerializeField] private EquipmentController equipment;

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

        [Header("Dynamic Component Slots")]
        [SerializeField] private Transform componentParent;
        [SerializeField] private GameObject componentPrefab;
        [SerializeField] private RectTransform dropZone;
        [SerializeField] private int chunkSize = 10;

        private InventorySystem inventory;

        private void Start()
        {
            // InventoryUI.Start()-
           
            inventory = inventoryHolder.Inventory;

            inventory.OnInventoryChanged += RefreshUI;
            inventory.OnAddFailed += ShowFullWarning;

            if (equipment != null)
                equipment.OnEquipmentChanged += RefreshUI;
            
            Debug.Log($"InventoryUI watching: {inventoryHolder.gameObject.name}, instance ID: {inventory.GetHashCode()}");

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
            if (equipment != null && equipment.CurrentPrimary != null)
            {
                primaryWeaponImage.gameObject.SetActive(true);
                primaryWeaponImage.sprite = equipment.CurrentPrimary.icon;

                int totalAmmo = string.IsNullOrEmpty(equipment.CurrentPrimary.ammoType)
                    ? 0
                    : inventory.GetQuantityForItemId(equipment.CurrentPrimary.ammoType);

                if (primaryAmmoText != null)
                    primaryAmmoText.text = totalAmmo.ToString();
            }
            else
            {
                primaryWeaponImage.gameObject.SetActive(false);
                if (primaryAmmoText != null) primaryAmmoText.text = "";
            }

            if (equipment != null && equipment.CurrentSecondary != null)
            {
                secondaryWeaponImage.gameObject.SetActive(true);
                secondaryWeaponImage.sprite = equipment.CurrentSecondary.icon;
            }
            else
            {
                secondaryWeaponImage.gameObject.SetActive(false);
            }

            RefreshAttachments();
        }

        private void RefreshAttachments()
        {
            if (equipment == null || equipment.CurrentPrimary == null)
            {
                attachmentPanel.SetActive(false);
                return;
            }

            bool hasAttachment = equipment.CurrentMagazine != null || equipment.CurrentGrip != null || equipment.CurrentScope != null;
            attachmentPanel.SetActive(hasAttachment);

            if (equipment.CurrentMagazine != null)
            {
                magazineImage.gameObject.SetActive(true);
                magazineImage.sprite = equipment.CurrentMagazine.icon;
            }
            else magazineImage.gameObject.SetActive(false);

            if (equipment.CurrentGrip != null)
            {
                gripImage.gameObject.SetActive(true);
                gripImage.sprite = equipment.CurrentGrip.icon;
            }
            else gripImage.gameObject.SetActive(false);

            if (equipment.CurrentScope != null)
            {
                scopeImage.gameObject.SetActive(true);
                scopeImage.sprite = equipment.CurrentScope.icon;
            }
            else scopeImage.gameObject.SetActive(false);
        }

        private void RefreshComponents()
        {
            if (componentParent == null || componentPrefab == null || inventory == null)
                return;

            for (int i = componentParent.childCount - 1; i >= 0; i--)
                Destroy(componentParent.GetChild(i).gameObject);

            var slots = inventory.Slots;

            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                InventorySlot slot = slots[slotIndex];
                if (slot.IsEmpty) continue;

                int remaining = slot.Quantity;

                while (remaining > 0)
                {
                    int amount = slot.Item.isStackable ? Mathf.Min(remaining, chunkSize) : remaining;
                    CreateComponent(slot.Item, amount, slotIndex);
                    remaining -= amount;
                }
            }
        }
        private void CreateComponent(ItemData item, int amount, int slotIndex)
        {
            GameObject go = Instantiate(componentPrefab, componentParent);

            // lists every component on every object named Component_Count_Text
            var allTransforms = go.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                if (t.name == "Component_Count_Text")
                {
                    var comps = t.GetComponents<Component>();
                    Debug.Log($"Components on '{t.name}': {string.Join(", ", System.Array.ConvertAll(comps, c => c.GetType().Name))}");
                }
            }

            var images = go.GetComponentsInChildren<Image>(true);
            Image iconImage = System.Array.Find(images, img => img.gameObject.name == "Component_Image");
            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                iconImage.gameObject.SetActive(true);
            }

            SetTextByName(go, "Component_Name_Text", item.displayName);
            SetTextByName(go, "Component_Count_Text", amount > 1 ? amount.ToString() : "");

            DraggableSlot drag = go.GetComponent<DraggableSlot>();
            if (drag == null) drag = go.AddComponent<DraggableSlot>();

            drag.Configure(DragSlotType.Component, slotIndex, amount, this, dropZone);
        }
                
                        /// <summary>Finds a TMP_Text or legacy Text component anywhere under root by GameObject
                        /// name, using GetComponentsInChildren (reliable regardless of nesting depth) instead
                        /// of Transform.Find/manual recursion.</summary>
                        private static void SetTextByName(GameObject root, string childName, string value)
                        {
                            var tmps = root.GetComponentsInChildren<TMP_Text>(true);
                            var tmp = System.Array.Find(tmps, t => t.gameObject.name == childName);
                            if (tmp != null)
                            {
                                tmp.text = value;
                                tmp.gameObject.SetActive(true);
                                return;
                            }
                
                            var texts = root.GetComponentsInChildren<Text>(true);
                            var legacy = System.Array.Find(texts, t => t.gameObject.name == childName);
                            if (legacy != null)
                            {
                                legacy.text = value;
                                legacy.gameObject.SetActive(true);
                                return;
                            }
                
                            Debug.LogWarning($"No TMP_Text or Text component found on any child named '{childName}'");
                        }

        private void ShowFullWarning(ItemData item)
        {
            Toast.Show($"Bag Full - can't pick up {item.displayName}");
        }

        public void OnDropButtonPressed(int slotIndex)
        {
            if (inventory.TryRemoveFromSlot(slotIndex, out ItemData item, out int quantity))
            {
                DropHandler.SpawnDroppedItem(item, quantity, inventoryHolder.transform);
            }
        }

        public void HandleSlotDropped(DragSlotType type, int slotIndex, int chunkAmount, AttachmentType attachmentType = AttachmentType.None)
        {
            switch (type)
            {
                case DragSlotType.Primary:
                    equipment?.DropEquipped(ItemCategory.Primary);
                    break;
                case DragSlotType.Secondary:
                    equipment?.DropEquipped(ItemCategory.Secondary);
                    break;
                case DragSlotType.Component:
                    if (inventory.TryRemoveQuantity(slotIndex, chunkAmount, out ItemData removedItem))
                    {
                        DropHandler.SpawnDroppedItem(removedItem, chunkAmount, inventoryHolder.transform);
                    }
                    break;
                case DragSlotType.Attachment:
                    equipment?.DropAttachment(attachmentType);
                    break;
            }
        }
    }
}