using UnityEngine;
using UnityEngine.UI;

namespace SaveManager.Example
{
    public class SaveToggle : MonoBehaviour
    {
        public SavedGameMetadata SavedGameMetadata { get; private set; }

        [SerializeField]
        private Color toggledColor;
        [SerializeField]
        private Image toggleImage;
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private Text toggleText;

        private SaveUI saveUI;
        private Color originalColor;

        public void Initialize(SaveUI saveUI, SavedGameMetadata savedGameMetadata, ToggleGroup toggleGroup)
        {
            this.saveUI = saveUI;
            SavedGameMetadata = savedGameMetadata;

            toggleText.text = string.Format(
                "Slot {0}: {1}",
                savedGameMetadata.slot,
                savedGameMetadata.timeStamp.ToString()
            );

            toggle.onValueChanged.AddListener(ToggleChanged);
            toggle.group = toggleGroup;
        }

        private void ToggleChanged(bool value)
        {
            if (value)
            {
                originalColor = toggleImage.color;
                toggleImage.color = toggledColor;

                saveUI.ToggleSelected(SavedGameMetadata);
            }
            else
            {
                toggleImage.color = originalColor;
            }
        }
    }
}

