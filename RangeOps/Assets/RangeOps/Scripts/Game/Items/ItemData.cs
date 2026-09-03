using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// Data-driven definition for an item type. Create one asset per item
    /// (Pistol, Rifle, Ammo, Medkit...). Adding a new item = new asset,
    /// zero code changes to Inventory or Interaction systems.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;          // unique key, e.g. "pistol", "ammo_9mm"
        public string displayName;     // e.g. "Pistol", "9mm Ammo"
        public Sprite icon;

        [Header("World Representation")]
        public GameObject worldPrefab; // spawned when the item is dropped

        [Header("Stacking")]
        public bool isStackable;
        public int maxStackSize = 1;   // ignored if isStackable is false
    }
}
