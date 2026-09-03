using UnityEngine;
using UnityEngine.UI;
using Game.Interfaces;

namespace Game.Interaction
{
    /// <summary>
    /// Listens to Interactor.OnTargetChanged only. Knows nothing about
    /// inventory, items, or pickup logic — purely reflects "what am I looking at".
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Interactor interactor;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText; // or TMP_Text if using TextMeshPro

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
