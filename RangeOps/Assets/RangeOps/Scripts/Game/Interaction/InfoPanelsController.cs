using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Interaction
{
    /// <summary>
    /// Shows a Welcome panel on start (dismissed by any key), and toggles a
    /// separate Instructions panel on a dedicated key press. Pure UI state -
    /// no gameplay logic, kept isolated from Interactor/EquipmentController.
    /// </summary>
    public class InfoPanelsController : MonoBehaviour
    {
        [Header("Welcome Panel")]
        [SerializeField] private GameObject welcomePanel;

        [Header("Instructions Panel")]
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private Key instructionsKey = Key.H;

        private void Start()
        {
            if (welcomePanel != null)
                welcomePanel.SetActive(true);

            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // dismiss welcome panel on any key press
            if (welcomePanel != null && welcomePanel.activeSelf && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                welcomePanel.SetActive(false);
            }

            // toggle instructions panel on its dedicated key
            if (instructionsPanel != null && Keyboard.current[instructionsKey].wasPressedThisFrame)
            {
                instructionsPanel.SetActive(!instructionsPanel.activeSelf);
            }
        }
    }
}