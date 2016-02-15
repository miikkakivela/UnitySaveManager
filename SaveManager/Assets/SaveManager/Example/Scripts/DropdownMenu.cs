using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SaveManager.Example
{
    public enum DropdownOption
    {
        SerializationFormat,
        Async,
        SavedGamesSortMode
    }

    public class DropdownMenu : MonoBehaviour
    {
        [SerializeField]
        private DropdownOption dropdownOption;
        [SerializeField]
        private GameObject dropdownButtonsPanel;
        [SerializeField]
        private GameObject dropdownButton;
        [SerializeField]
        private Text dropdownActivatorButtonText;

        void Start()
        {
            switch (dropdownOption)
            {
                case DropdownOption.SerializationFormat:
                    var serializationFormats = Enum.GetValues(typeof(SerializationFormat))
                        .Cast<SerializationFormat>();

                    foreach (var serializationFormat in serializationFormats)
                    {
                        var serializationFormatButton = Instantiate(dropdownButton);
                        serializationFormatButton.GetComponent<DropdownButton>().Initialize(serializationFormat.ToString(), (string value) =>
                        {
                            DropdownButtonPressed(value);
                        });
                        serializationFormatButton.transform.SetParent(dropdownButtonsPanel.transform, false);
                    }

                    dropdownActivatorButtonText.text = SaveManager.savedGamesSerializationFormat
                        .ToString();

                    break;
                case DropdownOption.SavedGamesSortMode:
                    var savedGamesSortModes = Enum.GetValues(typeof(SavedGamesSortMode))
                        .Cast<SavedGamesSortMode>();

                    foreach (var savedGamesSortMode in savedGamesSortModes)
                    {
                        var savedGamesSortModeButton = Instantiate(dropdownButton);
                        savedGamesSortModeButton.GetComponent<DropdownButton>().Initialize(savedGamesSortMode.ToString(), (string value) =>
                        {
                            DropdownButtonPressed(value);
                        });
                        savedGamesSortModeButton.transform.SetParent(dropdownButtonsPanel.transform, false);
                    }

                    dropdownActivatorButtonText.text = SaveManager.savedGameSortMode.ToString();

                    break;
                case DropdownOption.Async:
                    var boolOptions = new string[2] { true.ToString(), false.ToString() };

                    foreach (var boolOption in boolOptions)
                    {
                        var asyncButton = Instantiate(dropdownButton);
                        asyncButton.GetComponent<DropdownButton>().Initialize(boolOption, (string value) =>
                        {
                            DropdownButtonPressed(value);
                        });
                        asyncButton.transform.SetParent(dropdownButtonsPanel.transform, false);
                    }

                    dropdownActivatorButtonText.text = SaveManager.Async.ToString();

                    break;
            }
        }

        public void DropdownButtonPressed(string value)
        {
            switch (dropdownOption)
            {
                case DropdownOption.SerializationFormat:
                    SaveManager.savedGamesSerializationFormat =
                        (SerializationFormat)Enum.Parse(typeof(SerializationFormat), value);

                    break;
                case DropdownOption.SavedGamesSortMode:
                    SaveManager.savedGameSortMode =
                        (SavedGamesSortMode)Enum.Parse(typeof(SavedGamesSortMode), value);

                    break;
                case DropdownOption.Async:
                    SaveManager.Async = Convert.ToBoolean(value);

                    break;
            }

            dropdownButtonsPanel.SetActive(false);
            dropdownActivatorButtonText.text = value;
        }
    }
}