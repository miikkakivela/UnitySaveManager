using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SaveManager
{
    public enum SavedGamesSortMode
    {
        None,
        LastPlayedFirst,
        LongestPlaytimeFirst,
        SmallestSlotFirst
    }

    /// <summary>
    /// Manages saving and loading game data from persistent storage.
    /// </summary>
    public static class SaveManager
    {
        /// <summary>
        /// If the saving and loading should happen asynchronously.
        /// </summary>
        public static bool Async
        {
            get
            {
                return async;
            }

            set
            {
                if (value)
                {
                    if (GameObject.FindObjectOfType<AsyncActionHelper>() == null)
                    {
                        var obj = new GameObject(typeof(AsyncActionHelper).Name);
                        obj.AddComponent<AsyncActionHelper>();
                        GameObject.DontDestroyOnLoad(obj);
                    }

                    async = value;
                }
                else
                {
                    var obj = GameObject.FindObjectOfType<AsyncActionHelper>();

                    if (obj != null)
                    {
                        obj.gameObject.SetActive(false);
                        GameObject.Destroy(obj.gameObject);
                    }

                    async = value;
                }
            }
        }

        private static bool async = true;

        /// <summary>
        /// How the saved games should be sorted after they are loaded.
        /// </summary>
        public static SavedGamesSortMode savedGameSortMode = SavedGamesSortMode.None;
        /// <summary>
        /// The format for serializing game data.
        /// </summary>
        public static SerializationFormat savedGamesSerializationFormat =
            SerializationFormat.Binary;

        /// <summary>
        /// File name extension for the game specific data file.
        /// </summary>
        private static string savedGameFileSuffix = ".save";
        /// <summary>
        /// File name extension for the metadata file.
        /// </summary>
        private static string metadataFileSuffix = ".meta";

        /// <summary>
        /// Root path for the saves located in persistent storage.
        /// </summary>
        private static string SavedGameRootPath
        {
            get
            {
                return Application.persistentDataPath +
                    Path.DirectorySeparatorChar +
                    "Saved Games" +
                    Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Background worker that handles all the SaveManager's async actions.
        /// </summary>
        private static BackgroundWorker backgroundWorker;
        /// <summary>
        /// Queue for background actions. If for some reason multiple SaveManager
        /// async actions are run at the same time, they queue up in this object
        /// and instead are run one after another to prevent handling the same 
        /// file from multiple threads.
        /// </summary>
        private static Queue<Action> backgroundWorkQueue;

        static SaveManager()
        {
            // Creates the root path unless it exists.
            Directory.CreateDirectory(SavedGameRootPath);

            if (async)
            {
                if (GameObject.FindObjectOfType<AsyncActionHelper>() == null)
                {
                    var obj = new GameObject(typeof(AsyncActionHelper).Name);
                    obj.AddComponent<AsyncActionHelper>();
                    GameObject.DontDestroyOnLoad(obj);
                }
            }
        }

        #region Saving Game
        /// <summary>
        /// Saves the game on persistent storage.
        /// </summary>
        /// <param name="metadata">Metadata for the saved game.</param>
        /// <param name="gameData">Game specific data that is to be saved.</param>
        /// <param name="callback">Callback which is called when the saving has finished.</param>
        public static void SaveGame(SavedGameMetadata metadata, object gameData, Action callback)
        {
            SaveGame(new SavedGame(metadata, gameData), callback);
        }

        /// <summary>
        /// Saves the game on persistent storage.
        /// </summary>
        /// <param name="savedGame">Combination of metadata and game specific data
        /// that is to be saved.</param>
        /// <param name="callback">Callback which is called when the saving has finished.</param>
        public static void SaveGame(SavedGame savedGame, Action callback)
        {
            if (savedGame.metadata == null)
            {
                throw new ArgumentNullException(
                    "savedGame",
                    "Metadata on SavedGame can't be null."
                );
            }

            if (savedGame.gameSpecificData == null)
            {
                throw new ArgumentNullException(
                    "savedGame",
                    "Game specific data on SavedGame can't be null."
                );
            }

            savedGame.metadata.timeStamp = DateTime.Now;

            var gameDataPath = SavedGameRootPath + savedGame.metadata.savedGameFileName +
                savedGameFileSuffix;
            var metadataPath = SavedGameRootPath + savedGame.metadata.savedGameFileName +
                metadataFileSuffix;

            if (Async)
            {
                SaveGameAsync(savedGame, gameDataPath, metadataPath, callback);
            }
            else
            {
                SerializeAndWrite(savedGame.gameSpecificData, gameDataPath);
                SerializeAndWrite(savedGame.metadata, metadataPath);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Used for saving data on persistent storage.
        /// </summary>
        /// <param name="data">Data to be saved.</param>
        /// <param name="path">Full path of the file on Application.persistentDataPath.</param>
        /// <param name="callback">Callback which is called when the saving has finished.</param>
        public static void SaveData(object data, string path, Action callback)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "Data can't be null.");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path can't be null or empty.", "path");
            }

            var realPath = Application.persistentDataPath + path;

            if (Async)
            {
                SaveDataAsync(data, realPath, callback);
            }
            else
            {
                SerializeAndWrite(data, realPath);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        private static void SaveDataAsync(object data, string path, Action callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    SerializeAndWrite(data, path);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (callback != null)
                    {
                        AsyncActionHelper.RunInGameThread(() =>
                        {
                            callback();
                        });
                    }

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    SaveDataAsync(data, path, callback);
                });
            }
        }

        private static void SaveGameAsync(SavedGame savedGame, string gameDataPath, string metadataPath,
            Action callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    SerializeAndWrite(savedGame.gameSpecificData, gameDataPath);
                    SerializeAndWrite(savedGame.metadata, metadataPath);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (callback != null)
                    {
                        AsyncActionHelper.RunInGameThread(() =>
                        {
                            callback();
                        });
                    }

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    SaveGameAsync(savedGame, gameDataPath, metadataPath, callback);
                });
            }
        }

        private static void SerializeAndWrite(object data, string path)
        {
            switch (savedGamesSerializationFormat)
            {
                case SerializationFormat.Binary:
                default:
                    var binarySerializedData = Serialization.SerializeToBytes(data);
                    File.WriteAllBytes(path, binarySerializedData);

                    break;
            }
        }
        #endregion

        #region Loading Game
        /// <summary>
        /// Loads up all the metadata associated with the saved games.
        /// </summary>
        /// <param name="callback">Callback which is called when the loading is done.</param>
        public static void LoadMetadata(Action<List<SavedGameMetadata>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback", "Callback can't be null.");
            }

            var searchPattern = "*" + metadataFileSuffix;

            if (Async)
            {
                ReadFilesAsync<SavedGameMetadata>(
                    SavedGameRootPath,
                    searchPattern,
                    (List<SavedGameMetadata> metadata) =>
                    {
                        var sorted = SortMetadata(metadata);
                        callback(sorted);
                    }
                );
            }
            else
            {
                var metadata = ReadAndDeserializeMultiple<SavedGameMetadata>(SavedGameRootPath,
                    searchPattern);
                var sorted = SortMetadata(metadata);
                callback(sorted);
            }
        }

        /// <summary>
        /// Loads up single game specific data.
        /// </summary>
        /// <param name="metadata">Metadata that is associated with the game data.</param>
        /// <param name="callback">Callback which is called when the loading is done.</param>
        public static void LoadSavedGame(SavedGameMetadata metadata, Action<SavedGame> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback", "Callback can't be null.");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException("metadata", "Metadata can't be null.");
            }

            var gameDataPath = SavedGameRootPath + metadata.savedGameFileName + savedGameFileSuffix;

            if (Async)
            {
                ReadFileAsync<object>(gameDataPath, (object gameSpecificData) =>
                {
                    var savedGame = new SavedGame(metadata, gameSpecificData);
                    callback(savedGame);
                });
            }
            else
            {
                var gameSpecificData = ReadAndDeserialize<object>(gameDataPath);
                callback(new SavedGame(metadata, gameSpecificData));
            }
        }

        /// <summary>
        /// Used for loading data from the persistent storage.
        /// </summary>
        /// <typeparam name="T">Type of the object that is to be loaded and deserialized.</typeparam>
        /// <param name="path">Full path of the file on Application.persistentDataPath.</param>
        /// <param name="callback">Callback which is called when the loading is done.</param>
        public static void LoadData<T>(string path, Action<T> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path can't be null or empty.", "path");
            }

            if (callback == null)
            {
                throw new ArgumentException("Callback can't be null.", "callback");
            }

            var realPath = Application.persistentDataPath + Path.DirectorySeparatorChar + path;

            if (Async)
            {
                ReadFileAsync<T>(realPath, callback);
            }
            else
            {
                var data = ReadAndDeserialize<T>(realPath);
                callback(data);
            }
        }

        /// <summary>
        /// Used for loading multiple data from the persistent storage.
        /// </summary>
        /// <typeparam name="T">Type of the object that is to be loaded and deserialized.</typeparam>
        /// <param name="rootPath">Full path of the root on Application.persistentDataPath.</param>
        /// <param name="searchPattern">Pattern that is used to recognize files.</param>
        /// <param name="callback">Callback which is called when the loading is done.</param>
        public static void LoadDataMultiple<T>(string rootPath, string searchPattern, Action<List<T>> callback)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentException("Root path can't be null.", "rootPath");
            }

            if (callback == null)
            {
                throw new ArgumentException("Callback can't be null.", "callback");
            }

            var realPath = Application.persistentDataPath + rootPath;

            if (Async)
            {
                ReadFilesAsync<T>(realPath, searchPattern, callback);
            }
            else
            {
                var data = ReadAndDeserializeMultiple<T>(realPath, searchPattern);
                callback(data);
            }
        }

        private static void ReadFilesAsync<T>(string rootPath, string searchPattern, Action<List<T>> callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    args.Result = ReadAndDeserializeMultiple<T>(rootPath, searchPattern);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    var result = (List<T>)args.Result;

                    AsyncActionHelper.RunInGameThread(() =>
                    {
                        callback(result);
                    });

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    ReadFilesAsync(rootPath, searchPattern, callback);
                });
            }
        }

        private static void ReadFileAsync<T>(string path, Action<T> callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    args.Result = ReadAndDeserialize<T>(path);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    var result = (T)args.Result;

                    AsyncActionHelper.RunInGameThread(() =>
                    {
                        callback(result);
                    });

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    ReadFileAsync<T>(path, callback);
                });
            }
        }

        private static T ReadAndDeserialize<T>(string path)
        {
            switch (savedGamesSerializationFormat)
            {
                case SerializationFormat.Binary:
                default:
                    var binarySerializedData = File.ReadAllBytes(path);
                    return Serialization.DeserializeFromBytes<T>(binarySerializedData);
            }
        }

        private static List<T> ReadAndDeserializeMultiple<T>(string rootPath, string searchPattern)
        {
            var filePaths = Directory.GetFiles(rootPath, searchPattern);
            var files = new List<T>(filePaths.Count());

            if (filePaths.Count() > 0)
            {
                switch (savedGamesSerializationFormat)
                {
                    case SerializationFormat.Binary:
                    default:
                        foreach (var filePath in filePaths)
                        {
                            var data = File.ReadAllBytes(filePath);
                            var deserializedBinaryData = Serialization.DeserializeFromBytes<T>(data);
                            files.Add(deserializedBinaryData);
                        }

                        break;
                }
            }

            return files;
        }
        #endregion

        #region Deleting Game
        /// <summary>
        /// Deletes all the saved games permanently.
        /// </summary>
        /// <param name="callback">Callback which is called when deleting is done.</param>
        public static void DeleteAllSavedGames(Action callback)
        {
            if (Async)
            {
                DeleteSaveDirectoryAsync(SavedGameRootPath, callback);
            }
            else
            {
                DeleteFilesInDirectory(SavedGameRootPath);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Deletes a single saved game permanently.
        /// </summary>
        /// <param name="savedGame">Combination of metadata and game specific data
        /// that is to be saved.</param>
        /// <param name="callback">Callback which is called when the deleting is done.</param>
        public static void DeleteSave(SavedGame savedGame, Action callback)
        {
            DeleteSave(savedGame.metadata, callback);
        }

        /// <summary>
        /// Deletes a single saved game permanently.
        /// </summary>
        /// <param name="metadata">Metadata that is associated with the game data.</param>
        /// <param name="callback">Callback which is called when the deleting is done.</param>
        public static void DeleteSave(SavedGameMetadata metadata, Action callback)
        {
            var paths = new string[] {
                SavedGameRootPath + metadata.savedGameFileName + savedGameFileSuffix,
                SavedGameRootPath + metadata.savedGameFileName + metadataFileSuffix
            };

            if (Async)
            {
                DeleteFilesAsync(paths, callback);
            }
            else
            {
                DeleteFiles(paths);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Used for deleting data from the persistent storage.
        /// </summary>
        /// <param name="paths">Full paths for the files that should be deleted in
        /// Application.persistentDataPath.</param>
        /// <param name="callback">Callback which is called when the deleting is done.</param>
        public static void DeleteData(string[] paths, Action callback)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths", "Paths array can't be null.");
            }

            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = Application.persistentDataPath + paths[i];
            }

            if (Async)
            {
                DeleteFilesAsync(paths, callback);
            }
            else
            {
                DeleteFiles(paths);

                if (callback != null)
                {
                    callback();
                }
            }
        }

        private static void DeleteSaveDirectoryAsync(string rootPath, Action callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    DeleteFilesInDirectory(rootPath);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (callback != null)
                    {
                        AsyncActionHelper.RunInGameThread(() =>
                        {
                            callback();
                        });
                    }

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    DeleteSaveDirectoryAsync(rootPath, callback);
                });
            }
        }

        private static void DeleteFilesInDirectory(string path)
        {
            var filePaths = Directory.GetFiles(path);

            foreach (var filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }

        private static void DeleteFilesAsync(string[] paths, Action callback)
        {
            if (backgroundWorker == null)
            {
                backgroundWorker = InitializeBackgroundWorker();

                backgroundWorker.DoWork += (sender, args) =>
                {
                    DeleteFiles(paths);
                };

                backgroundWorker.RunWorkerCompleted += (sender, args) =>
                {
                    if (callback != null)
                    {
                        AsyncActionHelper.RunInGameThread(() =>
                        {
                            callback();
                        });
                    }

                    backgroundWorker = null;
                };

                backgroundWorker.RunWorkerAsync();
            }
            else if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                EnqueueAction(() =>
                {
                    DeleteFilesAsync(paths, callback);
                });
            }
        }

        private static void DeleteFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                File.Delete(path);
            }
        }
        #endregion

        private static List<SavedGameMetadata> SortMetadata(List<SavedGameMetadata> metadata)
        {
            switch (savedGameSortMode)
            {
                case SavedGamesSortMode.None:
                default:
                    return metadata;
                case SavedGamesSortMode.LongestPlaytimeFirst:
                    return metadata.OrderByDescending(savedGameMetadata => savedGameMetadata.timePlayed).
                        ToList();
                case SavedGamesSortMode.LastPlayedFirst:
                    return metadata.OrderByDescending(savedGameMetadata => savedGameMetadata.timeStamp).
                        ToList();
                case SavedGamesSortMode.SmallestSlotFirst:
                    return metadata.OrderBy(savedGameMetadata => savedGameMetadata.slot).
                        ToList();
            }
        }

        private static BackgroundWorker InitializeBackgroundWorker()
        {
            var newBackgroundWorker = new BackgroundWorker();

            newBackgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    AsyncActionHelper.RunInGameThread(() =>
                    {
                        throw args.Error;
                    });
                }

                if (backgroundWorkQueue != null && backgroundWorkQueue.Count > 0)
                {
                    backgroundWorkQueue.Dequeue().Invoke();
                }
            };

            return newBackgroundWorker;
        }

        private static void EnqueueAction(Action action)
        {
            if (backgroundWorkQueue == null)
            {
                backgroundWorkQueue = new Queue<Action>();
            }

            backgroundWorkQueue.Enqueue(action);
        }
    }
}