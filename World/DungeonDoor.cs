using Character;
using Interface;
using Player;
using UI;
using UnityEngine;

namespace World
{
    public class DungeonDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private int destination;
        
        public void OnCanInteract(CharacterBase characterBase)
        {
            // TODO: Localize
            Tooltip.Activate("Open");
        }

        public void OnInteract(CharacterBase characterBase)
        {
            if (characterBase is PlayerCharacter)
            {
                var tpDestination = TeleportDestinations.GetDestination(destination);
                PlayerCharacter.Teleport(tpDestination);
            }
        }

        public void OnEndInteract(CharacterBase characterBase)
        {
            Tooltip.DeActivate();
        }
    }
}