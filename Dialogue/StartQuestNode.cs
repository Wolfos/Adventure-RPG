using Character;
using Data;
using UnityEngine;
using Utility;
using WolfRPG.Core;
using XNode;

namespace Dialogue
{
    public class StartQuestNode : Node
    {
        [SerializeField, ObjectReference(5)] private RPGObjectReference questReference;

        
        [Input(ShowBackingValue.Never)] public Node previous;
        [Output] public Node next;
        
        public void Execute(CharacterBase character)
        {
            character.StartQuest(questReference.Guid);
        }
        
    }
}