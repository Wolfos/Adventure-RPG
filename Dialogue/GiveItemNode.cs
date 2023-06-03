using Items;
using XNode;
using Utility;
using WolfRPG.Core;

namespace Dialogue
{
    public class GiveItemNode : Node
    {
        public bool giveItem;
        public RPGObjectReference item;
        public bool giveMoney;
        public int money;
        [Input(ShowBackingValue.Never)] public Node previous;
        [Output] public Node next;
        
        public void Execute()
        {
            if (giveItem)
            {
                SystemContainer.GetSystem<Player.PlayerCharacter>().Inventory.AddItem(item.GetObject());
            }
            else if (giveMoney)
            {
                SystemContainer.GetSystem<Player.PlayerCharacter>().Inventory.Money += money;
            }
        }
    }
}