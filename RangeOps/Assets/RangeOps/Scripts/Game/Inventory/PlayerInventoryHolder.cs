using UnityEngine;

namespace Game.Inventory
{
    /// <summary>
    /// Bridges Unity's component world with the plain-C# InventorySystem.
    /// This is the ONLY MonoBehaviour that owns an InventorySystem instance.
    /// </summary>
    public class PlayerInventoryHolder : MonoBehaviour
    {
        [SerializeField] private int slotCount = 8;

        public InventorySystem Inventory { get; private set; }

        private void Awake()
        {
            Inventory = new InventorySystem(slotCount);
        }
    }
}
