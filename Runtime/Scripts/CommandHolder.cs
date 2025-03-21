using System;
using UnityEngine;

namespace CustomConsolePackage
{
    public class CommandHolder : MonoBehaviour
    {
        [CustomCommand("Expects: GameObject ID")]
        public void DestroyCommand(int id)
        {
            UnityEngine.Object[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject go in allObjects)
            {
                //Debug.Log(go + " is an active object " + go.GetInstanceID());
                if (go.GetInstanceID() == id)
                {
                    Destroy(go);
                    Debug.Log($"Destroyed GameObject: {go.name} which ID was: {id}");
                    return;
                }
            }
            Debug.LogError($"No GameObject found with ID: {id}.");
        }
        [CustomCommand("Clears Console")]
        public void Clear()
        {
            CustomConsole.Instance.ClearConsole();
        }
    }
}

