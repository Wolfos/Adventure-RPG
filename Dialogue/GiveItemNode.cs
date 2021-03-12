using Items;
using XNode;
using Utility;

namespace Dialogue
{
    public class GiveItemNode : Node
    {
        public bool giveItem;
        public Item item;
        public bool giveMoney;
        public int money;
        [Input(ShowBackingValue.Never)] public Node previous;
        [Output] public Node next;
        
        public void Execute()
        {
            if (giveItem)
            {
                SystemContainer.GetSystem<Player.Player>().inventory.AddItem(item.id);
            }
            else if (giveMoney)
            {
                SystemContainer.GetSystem<Player.Player>().data.money += money;
            }
        }
    }
}