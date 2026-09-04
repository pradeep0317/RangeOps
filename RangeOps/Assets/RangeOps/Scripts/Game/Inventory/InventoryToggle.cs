using UnityEngine;
using StarterAssets;

namespace Game.Inventory
{
    public class InventoryToggle : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject backpackPanel;
        [SerializeField] private GameObject inventoryUI;

        [Header("Player")]
        [SerializeField] private StarterAssetsInputs playerInput;

        private void Start()
        {
            // Initial state
            backpackPanel.SetActive(true);
            inventoryUI.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OpenInventory()
        {
            backpackPanel.SetActive(false);
            inventoryUI.SetActive(true);

            if (playerInput != null)
                playerInput.SetInputEnabled(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseInventory()
        {
            inventoryUI.SetActive(false);
            backpackPanel.SetActive(true);

            if (playerInput != null)
                playerInput.SetInputEnabled(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}