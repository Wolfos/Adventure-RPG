using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MeleeWeaponData = Items.MeleeWeaponData;

namespace UI
{
    public class ItemDescription : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI properties;
        [SerializeField] private Image itemSprite;

        public void SetItem(Items.ItemData item)
        {
            if (item == null)
            {
                itemName.text = "";
                description.text = "";
                properties.text = "";
                itemSprite.enabled = false;
            }
            else
            {
                itemName.text = item.Name.Get();
                description.text = item.Description.Get();

                if (item.sprite != null)
                {
                    var sprite = item.sprite;
                    if (sprite != null)
                    {
                        itemSprite.sprite = sprite;
                        itemSprite.enabled = true;
                    }
                    else
                    {
                        itemSprite.enabled = false;
                    }
                }
                else
                {
                    itemSprite.enabled = false;
                }

                var propertyText = "";
                switch (item.Type)
                {
                    case Items.ItemType.Consumable:
                        break;
                    case Items.ItemType.Weapon:
                        var meleeWeaponData = item as MeleeWeaponData;
                        if (meleeWeaponData != null)
                        {
                            propertyText += $"Damage: {meleeWeaponData.BaseDamage}\n"; // TODO: Localize
                        }
                        break;
                    case Items.ItemType.Equipment:
                        break;
                }
                // TODO: Localize
                propertyText += $"Weight: {item.Weight}";

                properties.text = propertyText;
            }
            
        }
    }
}