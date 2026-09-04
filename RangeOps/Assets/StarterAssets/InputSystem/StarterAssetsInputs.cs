using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = false;
        public bool cursorInputForLook = true;

        [Header("Input Control")]
        public bool inputEnabled = true;

#if ENABLE_INPUT_SYSTEM

        public void OnMove(InputValue value)
        {
            if (!inputEnabled)
            {
                move = Vector2.zero;
                return;
            }

            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (!inputEnabled)
            {
                look = Vector2.zero;
                return;
            }

            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            if (!inputEnabled)
            {
                jump = false;
                return;
            }

            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (!inputEnabled)
            {
                sprint = false;
                return;
            }

            SprintInput(value.isPressed);
        }

#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;

            if (!enabled)
            {
                move = Vector2.zero;
                look = Vector2.zero;
                jump = false;
                sprint = false;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}