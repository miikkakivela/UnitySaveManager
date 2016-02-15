using System;
using System.Collections.Generic;

namespace SaveManager.Example
{
    /// <summary>
    /// Holds some dummy data for testing purposes.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        [Serializable]
        public class Upgrades
        {
            public bool hasThisUpgrade = true;
            public bool hasThatUpgrade = false;
        }

        public string characterName = "Test";
        public int level = 50;
        public Upgrades upgrades = new Upgrades();
        public List<string> inventory = new List<string> { "Axe", "Shield" };
    }
}

