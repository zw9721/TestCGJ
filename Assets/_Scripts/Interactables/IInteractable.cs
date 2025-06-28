using UnityEngine;

namespace Game
{
    public interface IInteractable
    {
        void Interact(PlayerInteraction interactor);
    }
}