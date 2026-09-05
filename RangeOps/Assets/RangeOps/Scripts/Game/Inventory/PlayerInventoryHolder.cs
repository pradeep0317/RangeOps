using UnityEngine;

namespace Game.Inventory
{
    public class PlayerInventoryHolder : MonoBehaviour
    {
        [SerializeField] private int slotCount = 6; 

        public InventorySystem Inventory { get; private set; }

        private void Awake()
        {
            Inventory = new InventorySystem(slotCount);
        }
    }
}