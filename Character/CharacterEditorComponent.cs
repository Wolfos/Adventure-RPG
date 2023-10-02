using Character;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using WolfRPG.Core;

public class CharacterEditorComponent : MonoBehaviour
{
    public CharacterPartPicker partPicker;
    [FormerlySerializedAs("RpgObjectReference")] [ObjectReference((int)DatabaseCategory.Characters)]
    public RPGObjectReference rpgObjectReference;

    [ObjectReference(1)] public RPGObjectReference[] StartingEquipment;
}
