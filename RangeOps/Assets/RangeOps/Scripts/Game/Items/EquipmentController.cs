using System;
using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// Owns Primary/Secondary equip state and which one is currently active
    /// in-hand. Only ONE held object is ever visible at a time — switching
    /// hides the other, dropping clears that slot and falls back to the
    /// other weapon if one exists.
    /// </summary>
    public class EquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform handHoldPoint;
        [SerializeField] private Transform playerTransform;

        public ItemData CurrentPrimary { get; private set; }
        public ItemData CurrentSecondary { get; private set; }
        public ItemData CurrentMagazine { get; private set; }
        public ItemData CurrentGrip { get; private set; }
        public ItemData CurrentScope { get; private set; }

        /// <summary>Which slot is currently visible in-hand. Null = nothing held.</summary>
        public ItemCategory? ActiveCategory { get; private set; }

        public event Action<ItemCategory, ItemData> OnEquipped;
        public event Action<ItemData> OnAttachmentEquipped;
        public event Action<string> OnEquipBlocked;
        public event Action OnEquipmentChanged;

        private GameObject heldPrimaryObject;
        private GameObject heldSecondaryObject;

        public bool Equip(ItemData item, GameObject pickedObject)
        {
            if (item == null) return false;

            if (item.category == ItemCategory.Primary)
            {
                if (CurrentPrimary != null)
                {
                    OnEquipBlocked?.Invoke($"Drop the {CurrentPrimary.displayName} before picking up {item.displayName}");
                    return false;
                }

                CurrentPrimary = item;
                heldPrimaryObject = pickedObject;
                PrepareHeldObject(pickedObject);

                if (ActiveCategory == null)
                    SetActiveWeapon(ItemCategory.Primary);
                else
                    pickedObject.SetActive(false); // owned but stowed until switched to

                OnEquipped?.Invoke(ItemCategory.Primary, item);
                OnEquipmentChanged?.Invoke();
                return true;
            }

            if (item.category == ItemCategory.Secondary)
            {
                if (CurrentSecondary != null)
                {
                    OnEquipBlocked?.Invoke($"Drop the {CurrentSecondary.displayName} before picking up {item.displayName}");
                    return false;
                }

                CurrentSecondary = item;
                heldSecondaryObject = pickedObject;
                PrepareHeldObject(pickedObject);

                if (ActiveCategory == null)
                    SetActiveWeapon(ItemCategory.Secondary);
                else
                    pickedObject.SetActive(false);

                OnEquipped?.Invoke(ItemCategory.Secondary, item);
                OnEquipmentChanged?.Invoke();
                return true;
            }

            return false;
        }

        public bool EquipAttachment(ItemData attachment)
        {
            if (attachment == null || attachment.category != ItemCategory.Attachment)
                return false;

            if (CurrentPrimary == null || heldPrimaryObject == null)
            {
                OnEquipBlocked?.Invoke("Equip a primary weapon first.");
                return false;
            }

            if (!attachment.CanAttachTo(CurrentPrimary))
            {
                OnEquipBlocked?.Invoke($"{attachment.displayName} is not compatible with {CurrentPrimary.displayName}");
                return false;
            }

            SetAttachmentSlotActive(attachment, true);

            switch (attachment.attachmentType)
            {
                case AttachmentType.Magazine: CurrentMagazine = attachment; break;
                case AttachmentType.Grip: CurrentGrip = attachment; break;
                case AttachmentType.Scope: CurrentScope = attachment; break;
                default: return false;
            }

            OnAttachmentEquipped?.Invoke(attachment);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        /// <summary>Toggles the visible weapon between Primary and Secondary. Requires both to be owned.</summary>
        public void SwitchWeapon()
        {
            if (CurrentPrimary == null || CurrentSecondary == null)
            {
                OnEquipBlocked?.Invoke("Need both a primary and secondary to switch.");
                return;
            }

            ItemCategory next = ActiveCategory == ItemCategory.Primary ? ItemCategory.Secondary : ItemCategory.Primary;
            SetActiveWeapon(next);
        }

        /// <summary>Drops whichever weapon is currently active in-hand.</summary>
        public void DropActiveWeapon()
        {
            if (ActiveCategory == null) return;
            DropEquipped(ActiveCategory.Value);
        }

        public void DropEquipped(ItemCategory category)
        {
            GameObject obj = category == ItemCategory.Primary ? heldPrimaryObject : heldSecondaryObject;
            if (obj == null) return;

            obj.transform.SetParent(null);
            obj.SetActive(true);

            var col = obj.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            if (playerTransform != null)
            {
                obj.transform.position = playerTransform.position + playerTransform.forward * 1.2f;
                obj.transform.rotation = Quaternion.identity;
            }

            if (category == ItemCategory.Primary)
            {
                CurrentPrimary = null;
                heldPrimaryObject = null;
                CurrentMagazine = null;
                CurrentGrip = null;
                CurrentScope = null;
            }
            else
            {
                CurrentSecondary = null;
                heldSecondaryObject = null;
            }

            if (ActiveCategory == category)
            {
                if (category == ItemCategory.Primary && CurrentSecondary != null)
                    SetActiveWeapon(ItemCategory.Secondary);
                else if (category == ItemCategory.Secondary && CurrentPrimary != null)
                    SetActiveWeapon(ItemCategory.Primary);
                else
                    ActiveCategory = null;
            }

            OnEquipmentChanged?.Invoke();
        }
        public void DropAttachment(AttachmentType type)
             {
                 ItemData dropped = null;
     
                 switch (type)
                 {
                     case AttachmentType.Magazine: dropped = CurrentMagazine; CurrentMagazine = null; break;
                     case AttachmentType.Grip: dropped = CurrentGrip; CurrentGrip = null; break;
                     case AttachmentType.Scope: dropped = CurrentScope; CurrentScope = null; break;
                     default: return;
                 }
     
                 if (dropped == null) return;
     
                 SetAttachmentSlotActive(dropped, false);
     
                 if (playerTransform != null)
                     Game.Inventory.DropHandler.SpawnDroppedItem(dropped, 1, playerTransform);
     
                 OnEquipmentChanged?.Invoke();
             }
        /// <summary>Toggles the visual attachment slot that's already modeled on the
        /// currently held primary gun (e.g. a disabled "Scope_Slot" child object).</summary>
        private void SetAttachmentSlotActive(ItemData attachment, bool active)
        {
            if (heldPrimaryObject == null || string.IsNullOrEmpty(attachment.attachmentSlotName))
                return;

            Transform slot = FindDeepChild(heldPrimaryObject.transform, attachment.attachmentSlotName);
            if (slot == null)
            {
                Debug.LogWarning($"Attachment slot '{attachment.attachmentSlotName}' not found on {heldPrimaryObject.name}");
                return;
            }

            slot.gameObject.SetActive(active);

            if (active)
            {
                // this mesh is now a child of the gun - it must not simulate physics
                // independently or it'll fight the gun's transform / fall off
                var rb = slot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.detectCollisions = false;
                }

                var col = slot.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;

                Transform result = FindDeepChild(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private void SetActiveWeapon(ItemCategory category)
        {
            if (ActiveCategory == ItemCategory.Primary && heldPrimaryObject != null)
                heldPrimaryObject.SetActive(false);
            else if (ActiveCategory == ItemCategory.Secondary && heldSecondaryObject != null)
                heldSecondaryObject.SetActive(false);

            ActiveCategory = category;

            if (category == ItemCategory.Primary && heldPrimaryObject != null)
                heldPrimaryObject.SetActive(true);
            else if (category == ItemCategory.Secondary && heldSecondaryObject != null)
                heldSecondaryObject.SetActive(true);

            OnEquipmentChanged?.Invoke();
        }

        private void PrepareHeldObject(GameObject obj)
        {
            var col = obj.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            obj.transform.SetParent(handHoldPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}