using UnityEngine;

namespace Game.Interfaces
{
    /// <summary>
    /// Anything the player can interact with (guns, ammo crates, medkit crate, etc.)
    /// implements this. The Interactor never needs to know the concrete type.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Text shown in the prompt, e.g. "Press E to pick up Pistol".</summary>
        string GetInteractionPrompt();

        /// <summary>Called when the player presses the interact key while targeting this object.</summary>
        void Interact(GameObject interactor);
    }
}
