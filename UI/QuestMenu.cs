using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class QuestMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private Text title;
        [SerializeField] private Text stages;
        private int minVisible;
        private int maxVisible;
        private GameObject lastSelectedObject;
        private List<Button> buttons;
        private List<Quest> quests;

        private void OnEnable()
        {
            buttons = new List<Button>();
            quests = SystemContainer.GetSystem<Player.Player>().data.quests;
            CreateQuestButtons();
        }

        private void OnDisable()
        {
            foreach (var button in buttons)
            {
                Destroy(button.gameObject);
            }
        }

        

        private void CreateQuestButtons()
        {
            foreach (var quest in quests)
            {
                var go = Instantiate(buttonPrefab, content);
                go.SetActive(true);
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => ButtonPressed(go));
                buttons.Add(button);
                go.GetComponentInChildren<Text>().text = quest.questName;
            }
            if(InputMapper.usingController && buttons.Count > 0) buttons[0].Select();
        }
        
        private void ButtonPressed(GameObject button)
        {
            SelectedObject(button);
        }
        
        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject != lastSelectedObject)
            {
                lastSelectedObject = EventSystem.current.currentSelectedGameObject;
                SelectedObject(lastSelectedObject);
            }
        }
        
        private void SelectedObject(GameObject selected)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (selected != buttons[i].gameObject) continue;
                
                var quest = quests[i];
                title.text = quest.questName.ToUpper();
                string stageText = "";
                foreach (var stage in quest.stages)
                {
                    if (stage.complete)
                    {
                        stageText += "◉ ";
                        stageText += stage.description + "\n";
                    }
                    else
                    {
                        stageText += "○ ";
                        stageText += stage.description;
                        break;
                    }
                }

                stages.text = stageText;
                
                // Scrolling the list for gamepads
                if(!InputMapper.usingController) continue;
                
                var scrollRectTransform = scrollRect.transform as RectTransform;
                var buttonTransform = buttons[0].transform as RectTransform;
                var buttonSize = buttonTransform.sizeDelta.y;
                maxVisible = Mathf.RoundToInt(scrollRectTransform.sizeDelta.y / buttonSize);
                    
                Debug.Log($"Button{i} was selected");
                Debug.Log(maxVisible);
                    
                if (i >= maxVisible)
                {
                    minVisible++;
                    maxVisible++;
                    scrollRect.verticalNormalizedPosition -= GetNormalizedButtonSize();
                }
                if (i < minVisible)
                {
                    minVisible--;
                    maxVisible--;
                    scrollRect.verticalNormalizedPosition += GetNormalizedButtonSize();
                }

                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }

        private float GetNormalizedButtonSize()
        {
            var buttonTransform = buttons[0].transform as RectTransform;
            var scrollRectTransform = scrollRect.transform as RectTransform;
            var contentSize = content.sizeDelta.y;
            var buttonSize = buttonTransform.sizeDelta.y;
            var overflowSize = contentSize - scrollRectTransform.sizeDelta.y;
            return buttonSize / overflowSize;
        }
    }
}