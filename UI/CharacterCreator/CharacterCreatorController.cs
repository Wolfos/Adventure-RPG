using System;
using Character;
using UnityEngine;

namespace UI.CharacterCreator
{
    public class CharacterCreatorController : MonoBehaviour
    {
        [SerializeField] private CharacterPartPicker partPicker;

        private CharacterCustomizationData _data;
        private CharacterCreatorSelector[] _selectors;

        private void Start()
        {
            _selectors = GetComponentsInChildren<CharacterCreatorSelector>();
            
            SetDefaults();
            
            CharacterCustomizationController.SetData(_data, partPicker);

            foreach (var selector in _selectors)
            {
                selector.OnLeftButton += s => { ButtonPressed(s, true); };
                selector.OnRightButton += s => { ButtonPressed(s, false); };
            }
        }

        private void SetDefaults()
        {
            _data = new()
            {
                // Lets put some clothes on
                Torso = 1,
                ArmUpperRight = 1,
                ArmUpperLeft = 1,
                ArmLowerRight = 1,
                ArmLowerLeft = 1,
                Hips = 1,
                LegRight = 1,
                LegLeft = 1
            };
            
            foreach (var s in _selectors)
            {
                s.GenderUpdated(_data.Gender); 
            }
        }

        private void ButtonPressed(CharacterCreatorSelector selector, bool isLeft)
        {
            var availableOptions = partPicker.GetNumAvailableOptions(_data, selector.partToCustomize);

            // Increase / decrease
            if (isLeft) selector.Index--;
            else selector.Index++;
            
            // Clamp
            if (selector.Index < 0) selector.Index = availableOptions - 1;
            if (selector.Index >= availableOptions) selector.Index = 0;
            
            switch (selector.partToCustomize)
            {
                case CharacterCustomizationPart.Gender:
                    _data.Gender = isLeft ? Gender.Female : Gender.Male;
                    
                    foreach (var s in _selectors)
                    {
                       s.GenderUpdated(_data.Gender); 
                    }
                    break;
                case CharacterCustomizationPart.Hair:
                    _data.Hair = selector.Index;
                    break;
                case CharacterCustomizationPart.Head:
                    _data.Head = selector.Index;
                    break;
                case CharacterCustomizationPart.Eyebrows:
                    _data.Eyebrows = selector.Index;
                    break;
                case CharacterCustomizationPart.FacialHair:
                    _data.FacialHair = selector.Index;
                    break;
                case CharacterCustomizationPart.Torso:
                    _data.Torso = selector.Index;
                    break;
                case CharacterCustomizationPart.ArmUpperRight:
                    _data.ArmUpperRight = selector.Index;
                    break;
                case CharacterCustomizationPart.ArmUpperLeft:
                    _data.ArmUpperLeft = selector.Index;
                    break;
                case CharacterCustomizationPart.ArmLowerRight:
                    _data.ArmLowerRight = selector.Index;
                    break;
                case CharacterCustomizationPart.ArmLowerLeft:
                    _data.ArmLowerLeft = selector.Index;
                    break;
                case CharacterCustomizationPart.HandRight:
                    _data.HandRight = selector.Index;
                    break;
                case CharacterCustomizationPart.HandLeft:
                    _data.HandLeft = selector.Index;
                    break;
                case CharacterCustomizationPart.Hips:
                    _data.Hips = selector.Index;
                    break;
                case CharacterCustomizationPart.LegRight:
                    _data.LegRight = selector.Index;
                    break;
                case CharacterCustomizationPart.LegLeft:
                    _data.LegLeft = selector.Index;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            CharacterCustomizationController.SetData(_data, partPicker);
        }
    }
}