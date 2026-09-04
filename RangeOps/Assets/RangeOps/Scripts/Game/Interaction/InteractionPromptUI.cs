using UnityEngine;
using UnityEngine.UI;
using Game.Interfaces;
using TMPro;

namespace Game.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Interactor interactor;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;

        private void OnEnable()
        {
            interactor.OnTargetChanged += HandleTargetChanged;
        }

        private void OnDisable()
        {
            interactor.OnTargetChanged -= HandleTargetChanged;
        }

        private void HandleTargetChanged(IInteractable target)
        {
            if (target == null)
            {
                promptRoot.SetActive(false);
                return;
            }

            promptText.text = target.GetInteractionPrompt();
            promptRoot.SetActive(true);
        }
    }
}