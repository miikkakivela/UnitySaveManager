# Save Manager for Unity

An extension for Unity that brings support for saving, loading and deleting persistent game data both asynchronously and synchronously.

Currently supported formats are:

* Binary

## Overview

In this extension single saved game is technically constructed from two different pieces, which are game specific data and metadata that is related to the game specific data. Game specific data is unstructured data which is provided by the developer (like the name of the character that player creates and level of that character). This data is best provided as a class that holds all the data that needs to be saved. Metadata on the other hand is structured data that is partly provided from the developer as well. Metadata contains following information:

* File name for the file that holds the saved game and which are written on the persistent storage. File name is supplied by the developer and should not include the file name extension.
* Slot which is a number that can separate save files from one another in a game that uses save slots.
* Total time that the player has played the save.
* Time when the save was written on a file last time.

Game specific data and metadata are separated because instead of loading and deserializing all the game specific data (this could take pretty long if you have many saves), only small metadata files are loaded and after that one can choose to load game specific data files based on the metadata.

This extension also provides possibility to save, load and delete game data without the metadata.

All the functionality is tested on the following platforms and software:

* Unity 5.3.0f4
* PC Standalone, Windows 8.1
* Android 4.4.4, Samsung Galaxy S3 smartphone
* iOS 8.4, iPad 2 tablet

## Installing

Clone this repository or download it as a .zip-file. Inside Unity, import the SaveManager.unitypackage file into your Unity project. Use static methods found in SaveManager class to access functionality. 

## About the test scene

When imported into an Unity project, test scene can be found in the following directory:

`SaveManager/Example/Scenes/`

When running the scene, one can configure the SaveManager class from the GUI. Pressing the Run-button will present all the saved games in a scroll view and all the functionality related to them. Selecting one saved game element in the scroll view loads up the game specific data associated with the selected metadata.

GUI in the test scene is build for portrait resolutions only.

## Configuring SaveManager class

SaveManager class has a few settings that can be changed according to one's needs:

* Should the SaveManager class' methods work asynchronously or synchronously (`SaveManager.Async` property).
* How the metadata should be sorted when it is loaded. Metadata can be sorted by the time when the save was last written, by the slot and by the total time played that save (`SaveManager.savedGameSortMode` variable).
* What format of serialization SaveManager should use (`SaveManager.savedGamesSerializationFormat` variable). Look up the supported formats above.

## Using the Save Manager

### Defining data structure for the game specific data

Before saving games, you must define data that you want to save. This can be virtually anything. Save manager supports all the types that are supported by the serializers. Here we have a class that holds a few variables that we want to save. Note that the data object must be marked with `[Serializable]` attribute.

```csharp
[Serializable]
public class SaveData
{
    public string characterName = "Test";
    public bool hasSomeUpgrade = true;
}
```

### Saving a new game

```csharp
// Create a new object specific data.
SaveData saveData = new SaveData();
// Create a new metadata for the saved game. Pass a file name without the file name extension to the constructor.
SavedGameMetadata metadata = new SavedGameMetadata("SavedGame");
    
SaveManager.SaveGame(metadata, saveData, () =>
{
    Debug.Log("Game saved!");
});
```

### Loading metadata for all saved games

In order to load a previously saved game completely, one must first load up the metadata.

```csharp
SaveManager.LoadMetadata((List<SavedGameMetadata> metadata) =>
{
    // Iterate through all the metadata loaded.
    foreach (var singleMetadata in metadata)
    {
        Debug.Log("Saved game file name: " + singleMetadata.savedGameFileName);
        Debug.Log("Save slot: " + singleMetadata.slot);
    }
});
```

### Loading single game specific data for a saved game

After you have loaded metadata for saved games, you can use that metadata to load game specific data associated with the metadata. After loading game specific data hold up to the metadata passed in SavedGame object to save that existing game in the future.

```csharp
SaveManager.LoadSavedGame(metadata, (SavedGame savedGame) =>
{
    // Cast the loaded game data into data structure that holds your game specific data.
    var saveData = (SaveData)savedGame.gameSpecificData;
        
    Debug.Log("Character name: " + saveData.characterName);
});
```

### Saving existing game

When saving existing game, you have to have metadata loaded for previously saved games first. Choose the right metadata that is associated with the game specific data and then pass it along with game specific data to save the existing game.

```csharp
SaveManager.SaveGame(metadata, saveData, () =>
{
    Debug.Log("Game saved!");
});
```

### Deleting a game

Metadata for the saved games must be loaded before one can delete any saves. Load up the metadata and use that metadata to delete a game you want.

```csharp
SaveManager.DeleteSave(metadata, () =>
{
    Debug.Log("Save deleted!");
});
```

### Deleting ALL the saved games

```csharp
SaveManager.DeleteAllSavedGames(() =>
{
    Debug.Log("All saves deleted!");
});
```

### Saving game data without metadata

Saves data to the path without having to pass the metadata as well.

```csharp
// Create a new object specific data.
var saveData = new SaveData();
// Create a path for the file that holds the data. Notice that now you need to supply the
// full path in Application.persistentDataPath.
var path = Path.DirectorySeparatorChar + "Example.data";

SaveManager.SaveData(saveData, path, () =>
{
    Debug.Log("Data saved!");
});
```
    
### Loading game data without metadata

Loads up the game data straight from a file without having to pass the metadata.

```csharp
SaveManager.LoadData<SaveData>(path, (SaveData data) =>
{
    Debug.Log("Character name: " + saveData.characterName);
});
```
    
### Loading multiple files of game data without metadata

There is also a possibility to load multiple files of game data simultaneously using a search pattern.

```csharp
SaveManager.LoadDataMultiple<SaveData>(path, "*.data", (List<SaveData> data) =>
{
    foreach (var singleData in data)
    {
        Debug.Log("Character name: " + singleData.characterName);
    }
});
```
    
### Deleting game data without metadata

Delete any file straight with a file name.

```csharp
var paths = new string[1] { Path.DirectorySeparatorChar + "Example.data" };

SaveManager.DeleteData(paths, () =>
{
    Debug.Log("Paths deleted!");
});
```
