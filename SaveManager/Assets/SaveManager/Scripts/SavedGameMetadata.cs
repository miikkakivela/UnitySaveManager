using System;

namespace SaveManager
{
    /// <summary>
    /// Metadata that is associated with saveable game specific data.
    /// </summary>
    [Serializable]
    public class SavedGameMetadata
    {
        /// <summary>
        /// Developer supplied required file name for the save that should be 
        /// unique for each unique save. File name should NOT contain file name
        /// extension.
        /// </summary>
        public string savedGameFileName;
        /// <summary>
        /// Developer supplied optional number that can separate save files 
        /// from one another in a game that uses save slots.
        /// </summary>
        public int slot;
        /// <summary>
        /// Developer supplied optional time that the player has played the
        /// save.
        /// </summary>
        public TimeSpan timePlayed;
        /// <summary>
        /// The last time the save was written on a file.
        /// </summary>
        public DateTime timeStamp;

        /// <summary>
        /// Creates a new SavedGameMetadata object.
        /// </summary>
        /// <param name="savedGameFileName">File name for the save. Should NOT
        /// contain file name extension.</param>
        public SavedGameMetadata(string savedGameFileName)
        {
            if (string.IsNullOrEmpty(savedGameFileName))
            {
                throw new ArgumentException(
                    "File name for the saved game is required!",
                    "savedGameFileName"
                );
            }

            this.savedGameFileName = savedGameFileName;
        }

        /// <summary>
        /// Creates a new SavedGameMetadata object.
        /// </summary>
        /// <param name="savedGameFileName">File name for the save. Should NOT
        /// contain file name extension.</param>
        /// <param name="timePlayed">Time that the player has played the save.
        /// </param>
        public SavedGameMetadata(string savedGameFileName, TimeSpan timePlayed)
        {
            if (string.IsNullOrEmpty(savedGameFileName))
            {
                throw new ArgumentException(
                    "File name for the saved game is required!",
                    "savedGameFileName"
                );
            }

            this.savedGameFileName = savedGameFileName;
            this.timePlayed = timePlayed;
        }

        /// <summary>
        /// Creates a new SavedGameMetadata object.
        /// </summary>
        /// <param name="savedGameFileName">File name for the save. Should NOT
        /// contain file name extension.</param>
        /// <param name="slot">Slot of the save.</param>
        public SavedGameMetadata(string savedGameFileName, int slot)
        {
            if (string.IsNullOrEmpty(savedGameFileName))
            {
                throw new ArgumentException(
                    "File name for the saved game is required!",
                    "savedGameFileName"
                );
            }

            this.savedGameFileName = savedGameFileName;
            this.slot = slot;
        }

        /// <summary>
        /// Creates a new SavedGameMetadata object.
        /// </summary>
        /// <param name="savedGameFileName">File name for the save. Should NOT
        /// contain file name extension.</param>
        /// <param name="timePlayed">Time that the player has played the save.
        /// </param>
        /// <param name="slot">Slot of the save.</param>
        public SavedGameMetadata(string savedGameFileName, TimeSpan timePlayed, int slot)
        {
            if (string.IsNullOrEmpty(savedGameFileName))
            {
                throw new ArgumentException(
                    "File name for the saved game is required!",
                    "savedGameFileName"
                );
            }

            this.savedGameFileName = savedGameFileName;
            this.timePlayed = timePlayed;
            this.slot = slot;
        }
    }
}

