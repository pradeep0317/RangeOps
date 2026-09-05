using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Interaction
{
    using Game.Items;

    /// <summary>
    /// Reads Switch/Drop key input for the currently held weapon and forwards
    /// it to EquipmentController. Kept separate from Interactor (which only
    /// handles the look+range pickup key) — one input concern per class.
    /// </summary>
    public class EquipmentInputController : MonoBehaviour
    {
        [SerializeField] private EquipmentController equipment;
        [SerializeField] private Key switchKey = Key.Q;
        [SerializeField] private Key dropKey = Key.G;

        private void Update()
        {
            if (Keyboard.current == null || equipment == null) return;

            if (Keyboard.current[switchKey].wasPressedThisFrame)
                equipment.SwitchWeapon();

            if (Keyboard.current[dropKey].wasPressedThisFrame)
                equipment.DropActiveWeapon();
        }
    }
}