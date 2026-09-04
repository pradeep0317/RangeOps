using System;
using UnityEngine;
using Game.Interfaces;
using UnityEngine.InputSystem;


namespace Game.Interaction
{
    /// <summary>
    /// Lives on the player/camera. Casts a ray forward each frame, checks range,
    /// and raises events when the current interactable target changes. UI listens
    /// to these events — this class never touches UI directly.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        public event Action<IInteractable> OnTargetChanged; // null when nothing valid is targeted
        public event Action<IInteractable> OnInteracted;

        private IInteractable currentTarget;

        private void Update()
        {
            IInteractable newTarget = FindTarget();

            if (newTarget != currentTarget)
            {
                currentTarget = newTarget;
                OnTargetChanged?.Invoke(currentTarget); // UI shows/hides prompt here
            }

            if (currentTarget != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentTarget.Interact(gameObject);
                OnInteracted?.Invoke(currentTarget);
                Debug.Log("E pressed, target exists");
            }
        }

        private IInteractable FindTarget()
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayers))
            {
                // range is already enforced by the raycast distance itself,
                // both "looking at" and "within range" are satisfied together
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                {
                    return interactable;
                }
            }
            return null;
        }
    }
}
