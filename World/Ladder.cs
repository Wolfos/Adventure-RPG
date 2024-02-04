using Character;
using Interface;
using UI;
using UnityEngine;

namespace World
{
    public class Ladder : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform top;
        [SerializeField] private Transform bottom;
        public void OnCanInteract(CharacterBase characterBase)
        {
            // TODO: Localize
            Tooltip.Activate("Use", transform, new (1, 1));
        }

        public void OnInteract(CharacterBase characterBase)
        {
            var characterPos = characterBase.transform.position;
            if (Vector3.SqrMagnitude(characterPos - top.position) >
                Vector3.SqrMagnitude(characterPos - bottom.position))
            {
                characterBase.Teleport(top.position, top.rotation);
            }
            else
            {
                characterBase.Teleport(bottom.position, bottom.rotation);
            }
        }

        public void OnEndInteract(CharacterBase characterBase)
        {
            Tooltip.DeActivate();
        }
    }
}