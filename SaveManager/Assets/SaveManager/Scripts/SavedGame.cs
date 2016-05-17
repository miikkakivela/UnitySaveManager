using System;

namespace SaveManager
{
    /// <summary>
    /// Represents the combination of saveable game specific data and metadata
    /// associated with it.
    /// </summary>
    [Serializable]
    public class SavedGame<T>
    {
        public SavedGameMetadata metadata;
        public T gameSpecificData;

        public SavedGame(SavedGameMetadata metadata, T gameSpecificData)
        {
            this.metadata = metadata;
            this.gameSpecificData = gameSpecificData;
        }
    }
}
