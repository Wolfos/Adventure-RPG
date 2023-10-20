using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;
using WolfRPG.Core.Quests;

namespace UI
{
    public class QuestMenu : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private Text title;
        [SerializeField] private Text stages;
        private int _minVisible;
        private int _maxVisible;
        private GameObject _lastSelectedObject;
        private List<Button> _buttons;
        private List<QuestProgress> _questProgress;
        

        private void OnEnable()
        {
            _buttons = new();
            _questProgress = PlayerCharacter.GetAllQuestProgress();

            CreateQuestButtons();
        }

        private void OnDisable()
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }
        }

        

        private void CreateQuestButtons()
        {
            foreach (var quest in _questProgress)
            {
                var go = Instantiate(buttonPrefab, content);
                go.SetActive(true);
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => ButtonPressed(go));
                _buttons.Add(button);
                go.GetComponentInChildren<Text>().text = quest.GetQuest().QuestName.Get();
            }
            if(InputMapper.UsingController && _buttons.Count > 0) _buttons[0].Select();
        }
        
        private void ButtonPressed(GameObject button)
        {
            SelectedObject(button);
        }
        
        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject != _lastSelectedObject)
            {
                _lastSelectedObject = EventSystem.current.currentSelectedGameObject;
                SelectedObject(_lastSelectedObject);
            }
        }
        
        private void SelectedObject(GameObject selected)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (selected != _buttons[i].gameObject) continue;
                
                var questProgress = _questProgress[i];
                var quest = questProgress.GetQuest();
                title.text = quest.QuestName.Get().ToUpper();
                string stageText = "";
                for(int j = 0; j < quest.Stages.Length; j++)
                {
                    if (questProgress.CurrentStage > j || questProgress.Complete)
                    {
                        stageText += "◉ ";
                        stageText += quest.Stages[j].Description + "\n";
                    }
                    else
                    {
                        stageText += "○ ";
                        stageText += quest.Stages[j].Description + "\n";
                    }

                    if (j >= questProgress.CurrentStage)
                    {
                        break;
                    }
                }

                stages.text = stageText;
                
                // Scrolling the list for gamepads
                if(!InputMapper.UsingController) continue;
                
                var scrollRectTransform = scrollRect.transform as RectTransform;
                var buttonTransform = _buttons[0].transform as RectTransform;
                var buttonSize = buttonTransform.sizeDelta.y;
                _maxVisible = Mathf.RoundToInt(scrollRectTransform.sizeDelta.y / buttonSize);
                    
                Debug.Log($"Button{i} was selected");
                Debug.Log(_maxVisible);
                    
                if (i >= _maxVisible)
                {
                    _minVisible++;
                    _maxVisible++;
                    scrollRect.verticalNormalizedPosition -= GetNormalizedButtonSize();
                }
                if (i < _minVisible)
                {
                    _minVisible--;
                    _maxVisible--;
                    scrollRect.verticalNormalizedPosition += GetNormalizedButtonSize();
                }

                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
            }
        }

        private float GetNormalizedButtonSize()
        {
            var buttonTransform = _buttons[0].transform as RectTransform;
            var scrollRectTransform = scrollRect.transform as RectTransform;
            var contentSize = content.sizeDelta.y;
            var buttonSize = buttonTransform.sizeDelta.y;
            var overflowSize = contentSize - scrollRectTransform.sizeDelta.y;
            return buttonSize / overflowSize;
        }
    }
}