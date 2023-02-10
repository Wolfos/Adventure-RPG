using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DialogueResponseDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonContainer;

        private List<GameObject> buttons;

        public void Activate(List<string> options, Action<int> callback)
        {
            gameObject.SetActive(true);
            buttons = new List<GameObject>();

            for (var i = 0; i < options.Count; i++)
            {
                var s = options[i];
                var button = Instantiate(buttonPrefab, buttonContainer, true);
                button.SetActive(true);
                button.GetComponentInChildren<Text>().text = s;
                int iterator = i;
                var b = button.GetComponent<Button>();
                b.onClick.AddListener(delegate { callback(iterator); });
                buttons.Add(button);

                if (InputMapper.UsingController && i == 0)
                {
                    b.Select();
                }
            }
        }

        public void DeActivate()
        {
            if (buttons != null)
                foreach (var b in buttons)
                    Destroy(b);
            gameObject.SetActive(false);
        }
    }
}