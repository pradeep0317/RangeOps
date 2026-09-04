using UnityEngine;
using Game.Items;

namespace Game.Inventory
{
    public static class DropHandler
    {
        private const float DropDistance = 1.2f;

        public static void SpawnDroppedItem(ItemData item, int quantity, Transform playerTransform)
        {
            if (item.worldPrefab == null) return;

            Vector3 spawnPos = playerTransform.position + playerTransform.forward * DropDistance;
            GameObject spawned = Object.Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);
        }
    }
}