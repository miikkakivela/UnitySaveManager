using System;

namespace SaveManager
{
    /// <summary>
    /// Represents the combination of saveable game specific data and metadata
    /// associated with it.
    /// </summary>
    [Serializable]
    public class SavedGame
    {
        public SavedGameMetadata metadata;
        public object gameSpecificData;

        public SavedGame(SavedGameMetadata metadata, object gameSpecificData)
        {
            this.metadata = metadata;
            this.gameSpecificData = gameSpecificData;
        }
    }
}
