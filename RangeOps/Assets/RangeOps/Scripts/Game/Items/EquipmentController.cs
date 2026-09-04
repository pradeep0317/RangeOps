using System;
using UnityEngine;

namespace Game.Items
{
    public class EquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform handHoldPoint;
        [SerializeField] private Transform playerTransform;

        [Header("Current Equipment")]
        public ItemData CurrentPrimary { get; private set; }
        public ItemData CurrentSecondary { get; private set; }

        public ItemData CurrentMagazine { get; private set; }
        public ItemData CurrentGrip { get; private set; }
        public ItemData CurrentScope { get; private set; }

        public int CurrentAmmo { get; private set; }

        public event Action<ItemCategory, ItemData> OnEquipped;
        public event Action<ItemData> OnAttachmentEquipped;
        public event Action<string> OnEquipBlocked;
        public event Action OnEquipmentChanged;

        private GameObject heldPrimaryObject;
        private GameObject heldSecondaryObject;

        public bool Equip(ItemData item, GameObject pickedObject)
        {
            if (item == null)
                return false;

            if (item.category == ItemCategory.Primary)
            {
                if (CurrentPrimary != null)
                {
                    OnEquipBlocked?.Invoke(
                        $"Drop the {CurrentPrimary.displayName} before picking up {item.displayName}"
                    );

                    return false;
                }

                CurrentPrimary = item;
                heldPrimaryObject = pickedObject;

                AttachToHand(pickedObject);

                CurrentAmmo = 0;

                OnEquipped?.Invoke(ItemCategory.Primary, item);
                OnEquipmentChanged?.Invoke();

                return true;
            }

            if (item.category == ItemCategory.Secondary)
            {
                if (CurrentSecondary != null)
                {
                    OnEquipBlocked?.Invoke(
                        $"Drop the {CurrentSecondary.displayName} before picking up {item.displayName}"
                    );

                    return false;
                }

                CurrentSecondary = item;
                heldSecondaryObject = pickedObject;

                AttachToHand(pickedObject);

                OnEquipped?.Invoke(ItemCategory.Secondary, item);
                OnEquipmentChanged?.Invoke();

                return true;
            }

            return false;
        }

        public bool EquipAttachment(ItemData attachment)
        {
            if (attachment == null ||
                attachment.category != ItemCategory.Attachment)
                return false;

            if (CurrentPrimary == null)
            {
                OnEquipBlocked?.Invoke("Equip a primary weapon first.");
                return false;
            }

            if (!attachment.CanAttachTo(CurrentPrimary))
            {
                OnEquipBlocked?.Invoke(
                    $"{attachment.displayName} is not compatible with {CurrentPrimary.displayName}"
                );

                return false;
            }

            switch (attachment.attachmentType)
            {
                case AttachmentType.Magazine:
                    CurrentMagazine = attachment;
                    break;

                case AttachmentType.Grip:
                    CurrentGrip = attachment;
                    break;

                case AttachmentType.Scope:
                    CurrentScope = attachment;
                    break;

                default:
                    return false;
            }

            OnAttachmentEquipped?.Invoke(attachment);
            OnEquipmentChanged?.Invoke();

            return true;
        }

        public bool UseAmmo(ItemData ammo)
        {
            if (CurrentPrimary == null)
                return false;

            if (!ammo.CanUseWithWeapon(CurrentPrimary))
                return false;

            return true;
        }

        public void SetCurrentAmmo(int amount)
        {
            if (CurrentPrimary == null)
                return;

            CurrentAmmo = Mathf.Clamp(
                amount,
                0,
                CurrentPrimary.magazineCapacity
            );

            OnEquipmentChanged?.Invoke();
        }

        public void DropEquipped(ItemCategory category)
        {
            GameObject obj = null;

            if (category == ItemCategory.Primary)
                obj = heldPrimaryObject;

            else if (category == ItemCategory.Secondary)
                obj = heldSecondaryObject;

            if (obj == null)
                return;

            obj.transform.SetParent(null);
            obj.SetActive(true);

            Collider col = obj.GetComponent<Collider>();

            if (col != null)
                col.enabled = true;

            if (playerTransform != null)
            {
                obj.transform.position =
                    playerTransform.position +
                    playerTransform.forward * 1.2f;

                obj.transform.rotation = Quaternion.identity;
            }

            if (category == ItemCategory.Primary)
            {
                CurrentPrimary = null;
                heldPrimaryObject = null;

                CurrentMagazine = null;
                CurrentGrip = null;
                CurrentScope = null;
                CurrentAmmo = 0;
            }
            else
            {
                CurrentSecondary = null;
                heldSecondaryObject = null;
            }

            OnEquipmentChanged?.Invoke();
        }

        private void AttachToHand(GameObject pickedObject)
        {
            if (pickedObject == null || handHoldPoint == null)
                return;

            Collider col = pickedObject.GetComponent<Collider>();

            if (col != null)
                col.enabled = false;

            pickedObject.transform.SetParent(handHoldPoint);
            pickedObject.transform.localPosition = Vector3.zero;
            pickedObject.transform.localRotation = Quaternion.identity;
        }
    }
}