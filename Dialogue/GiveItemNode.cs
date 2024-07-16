using Character;
using Items;
using XNode;
using Utility;
using WolfRPG.Core;

namespace Dialogue
{
    public class GiveItemNode : Node
    {
        public bool giveItem;
        public ItemData item;
        public bool giveMoney;
        public int money;
        [Input(ShowBackingValue.Never)] public Node previous;
        [Output] public Node next;
        
        public void Execute(CharacterBase interactingCharacter)
        {
            if (giveItem)
            {
                interactingCharacter.Inventory.AddItem(item);
            }
            else if (giveMoney)
            {
                interactingCharacter.Inventory.Money += money;
            }
        }
    }
}