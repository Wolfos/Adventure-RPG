using Data;
using Utility;
using XNode;

namespace Dialogue
{
    public class StartQuestNode : Node
    {
        public Quest quest;
        
        [Input(ShowBackingValue.Never)] public Node previous;
        [Output] public Node next;
        
        public void Execute()
        {
            SystemContainer.GetSystem<Player.PlayerCharacter>().StartQuest(quest);
        }
        
    }
}