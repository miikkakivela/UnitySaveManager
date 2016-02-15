using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveManager
{
    ///////// <summary>
    /// This is a helper class that is used to invoke actions in Unity's main 
    /// thread from another thread.
    ///
    /// This MonoBehavior must be included in the scene before using it.
    /// </summary>
    public class AsyncActionHelper : MonoBehaviour
    {
        /// <summary>
        /// A list of actions that are awaiting to get invoked in Unity's main 
        /// thread.
        /// </summary>
        private static List<Action> gameThreadActions = new List<Action>();
        /// <summary>
        /// Tells if there are any actions awaiting to be invoked in Unity's
        /// main thread.
        /// </summary>
        private static volatile bool actionsAwaiting;

        void Update()
        {
            if (actionsAwaiting)
            {
                List<Action> tempActionList = new List<Action>();

                lock (gameThreadActions)
                {
                    tempActionList.AddRange(gameThreadActions);
                    gameThreadActions.Clear();
                    actionsAwaiting = false;
                }

                foreach (var action in tempActionList)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// Adds an action to be invoked in Unity's main thread.
        /// </summary>
        /// <param name="action">Action to be invoked in Unity's main thread.
        /// </param>
        public static void RunInGameThread(Action action)
        {
            lock (gameThreadActions)
            {
                gameThreadActions.Add(action);
                actionsAwaiting = true;
            }
        }
    }
}
