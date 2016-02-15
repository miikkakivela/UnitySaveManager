using System;
using UnityEngine;
using UnityEngine.UI;

namespace SaveManager.Example
{
    public class DropdownButton : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        private Action<string> buttonPressed;

        public void Initialize(string value, Action<string> buttonPressed)
        {
            text.text = value;
            this.buttonPressed = buttonPressed;
        }

        public void DropdownButtonPressed()
        {
            buttonPressed(text.text);
        }
    }
}