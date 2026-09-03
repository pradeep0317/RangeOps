using UnityEngine;
using Game.Items;

namespace Game.Inventory
{
    /// <summary>
    /// Spawns the world prefab for a dropped item just in front of the player.
    /// Kept separate so both InventoryUI and any future drop trigger (e.g. a
    /// hotkey) can reuse the same logic.
    /// </summary>
    public static class DropHandler
    {
        private const float DropDistance = 1.2f;

        public static void SpawnDroppedItem(ItemData item, int quantity, Transform playerTransform)
        {
            if (item.worldPrefab == null) return;

            Vector3 spawnPos = playerTransform.position + playerTransform.forward * DropDistance;
            GameObject spawned = Object.Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

            // if the world prefab needs to know the quantity (e.g. ammo box showing "x30"),
            // set it here via a WorldItem field/setter
        }
    }
}
