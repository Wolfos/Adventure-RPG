using System;
using Character;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CharacterCreator
{
    public class CharacterCreatorSelector : MonoBehaviour
    {
        public CharacterCustomizationPart partToCustomize;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Text indexText;
        [SerializeField] private bool onlyEnabledForFemales;
        [SerializeField] private bool onlyEnabledForMales;

        private int _index;

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                indexText.text = (_index + 1).ToString();
            }
        }

        public Action<CharacterCreatorSelector> OnLeftButton { get; set; }
        public Action<CharacterCreatorSelector> OnRightButton { get; set; }

        private void Awake()
        {
            leftButton.onClick.AddListener(() =>
            {
                OnLeftButton?.Invoke(this);
            });
            rightButton.onClick.AddListener(() =>
            {
                OnRightButton?.Invoke(this);
            });
        }

        public void GenderUpdated(Gender newGender)
        {
            bool enabled = true;
            if (onlyEnabledForFemales) enabled = newGender == Gender.Female;
            if (onlyEnabledForMales) enabled = newGender == Gender.Male;

            if (enabled)
            {
                leftButton.interactable = true;
                rightButton.interactable = true;
            }
            else
            {
                leftButton.interactable = false;
                rightButton.interactable = false;
            }
        }
    }
}