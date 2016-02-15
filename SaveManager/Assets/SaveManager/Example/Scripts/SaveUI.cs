using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SaveManager.Example
{
    public class SaveUI : MonoBehaviour
    {
        [SerializeField]
        private Text statusText;
        [SerializeField]
        private GameObject savesGroup;
        [SerializeField]
        private GameObject savePrefab;
        [SerializeField]
        private ToggleGroup toggleGroup;
        [SerializeField]
        private CanvasGroup canvasGroup;
        private int largestSlot;

        void OnEnable()
        {
            LoadGamesButtonPressed();
        }

        public void LoadGamesButtonPressed()
        {
            statusText.text = "Loading metadata...";

            canvasGroup.interactable = false;

            ClearSaveElements();

            // Loads up the metadata and populate the scroll view with the retrieved data.
            SaveManager.LoadMetadata((List<SavedGameMetadata> metadata) =>
            {
                foreach (var md in metadata)
                {
                    var save = CreateSaveElement(md);
                    save.transform.SetParent(savesGroup.transform, false);

                    if (md.slot > largestSlot)
                    {
                        largestSlot = md.slot;
                    }
                }

                if (metadata.Count == 0)
                {
                    statusText.text = "No save metadata found.";
                }
                else
                {
                    statusText.text = string.Format("Loaded {0} metadata files!", metadata.Count);
                }

                canvasGroup.interactable = true;
            });
        }

        public void CreateSavedGameButtonPressed()
        {
            statusText.text = "Saving...";

            canvasGroup.interactable = false;

            largestSlot++;

            var savedGameFileName = "SavedGame" + largestSlot;

            // Creates a new SavedGame object.
            var savedGame = new SavedGame(
                new SavedGameMetadata(savedGameFileName, largestSlot),
                new SaveData()
            );

            // Saves the SavedGame object and adds it to the scroll view.
            SaveManager.SaveGame(savedGame, () =>
            {
                var save = CreateSaveElement(savedGame.metadata);
                save.transform.SetParent(savesGroup.transform, false);

                statusText.text = "Game saved successfully!";

                canvasGroup.interactable = true;
            });
        }

        public void ClearSavedGamesButtonPressed()
        {
            statusText.text = "Deleting all saves...";

            canvasGroup.interactable = false;

            // Delete all saves and clears the scroll view.
            SaveManager.DeleteAllSavedGames(() =>
            {
                ClearSaveElements();
                largestSlot = 0;
                statusText.text = "All saves deleted!";

                canvasGroup.interactable = true;
            });
        }

        public void DeleteSaveButtonPressed()
        {
            statusText.text = "Deleting save...";

            canvasGroup.interactable = false;

            var activeToggles = toggleGroup.ActiveToggles();

            if (activeToggles.Count() != 0)
            {
                var toggleElement = activeToggles.First();
                var metadata = toggleElement.GetComponent<SaveToggle>().SavedGameMetadata;

                // Deletes a single save and removes it from the scroll view.
                SaveManager.DeleteSave(metadata, () =>
                {
                    statusText.text = string.Format("Save on slot {0} deleted!", metadata.slot);

                    var obj = toggleElement.gameObject;
                    obj.SetActive(false);
                    Destroy(obj);

                    canvasGroup.interactable = true;
                });
            }
            else
            {
                statusText.text = "No save element selected!";
                canvasGroup.interactable = true;
            }
        }

        public void ToggleSelected(SavedGameMetadata metadata)
        {
            statusText.text = "Loading save data...";

            canvasGroup.interactable = false;

            // Loads up the game data when save element on the scroll view is selected.
            SaveManager.LoadSavedGame(metadata, (SavedGame savedGame) =>
            {
                statusText.text = "Save data loaded!";

                var saveData = (SaveData)savedGame.gameSpecificData;

                // Log some dummy data from the loaded game data.
                Debug.Log("Character name: " + saveData.characterName);
                Debug.Log("Has that upgrade: " + saveData.upgrades.hasThatUpgrade);
                Debug.Log("Inventory:");

                foreach (var item in saveData.inventory)
                {
                    Debug.Log(item);
                }

                canvasGroup.interactable = true;
            });
        }

        private void ClearSaveElements()
        {
            foreach (Transform child in savesGroup.transform)
            {
                var obj = child.gameObject;
                obj.SetActive(false);
                Destroy(obj);
            }
        }

        private GameObject CreateSaveElement(SavedGameMetadata metadata)
        {
            var save = Instantiate(savePrefab);

            save.GetComponent<SaveToggle>().Initialize(
                this,
                metadata,
                toggleGroup
            );

            return save;
        }
    }
}

