using Character;
using Items;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using WolfRPG.Core;

public class CharacterEditorComponent : MonoBehaviour
{
    public CharacterPartPicker partPicker;
    public CharacterDataObject dataObject;
    public StartingInventory[] startingInventory;
}
