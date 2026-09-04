using UnityEngine;
using EasyUI.Toast;
using Game.Items;

namespace Game.Interaction
{
    public class EquipMessageUI : MonoBehaviour
    {
        [SerializeField] private EquipmentController equipmentController;

        private void OnEnable()
        {
            equipmentController.OnEquipBlocked += HandleBlocked;
        }

        private void OnDisable()
        {
            equipmentController.OnEquipBlocked -= HandleBlocked;
        }

        private void HandleBlocked(string message)
        {
            Toast.Show(message);
        }
    }
}