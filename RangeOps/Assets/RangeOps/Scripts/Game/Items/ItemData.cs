using UnityEngine;

namespace Game.Items
{
    public enum ItemCategory
    {
        Primary,
        Secondary,
        Ammo,
        Attachment
    }

    public enum AttachmentType
    {
        None,
        Magazine,
        Grip,
        Scope
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        public Sprite icon;
        public ItemCategory category;

        [Header("World")]
        public GameObject worldPrefab;

        [Header("Stacking")]
        public bool isStackable;
        public int maxStackSize = 1;

        [Header("Weapon")]
        public string ammoType;
        public int magazineCapacity = 30;

        [Header("Attachment")]
        public AttachmentType attachmentType;

        [Tooltip("Weapons that can use this attachment.")]
        public ItemData[] compatibleWeapons;

        [Header("Ammo Compatibility")]
        [Tooltip("Weapons that can use this ammo.")]
        public ItemData[] compatibleAmmoWeapons;

        public bool CanAttachTo(ItemData weapon)
        {
            if (category != ItemCategory.Attachment || weapon == null)
                return false;

            foreach (ItemData compatibleWeapon in compatibleWeapons)
            {
                if (compatibleWeapon == weapon)
                    return true;
            }

            return false;
        }

        public bool CanUseWithWeapon(ItemData weapon)
        {
            if (category != ItemCategory.Ammo || weapon == null)
                return false;

            foreach (ItemData compatibleWeapon in compatibleAmmoWeapons)
            {
                if (compatibleWeapon == weapon)
                    return true;
            }

            return false;
        }
    }
}