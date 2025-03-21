using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace CustomConsolePackage
{
    public class ExecutingCustomCommand : MonoBehaviour
    {
        #region Singleton
        public static ExecutingCustomCommand Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }
        #endregion

        [Header("UI Components")]
        [SerializeField] TMP_InputField inputField;
        [SerializeField] TextMeshProUGUI suggestText;
        [SerializeField] TextMeshProUGUI suggestPrefab;
        [SerializeField] GameObject commandScrollView;
        [SerializeField] Button commandButton;
        [SerializeField] GameObject suggestArea;

        [Header("Settings")]
        [SerializeField] bool useMousePos = true;

        Dictionary<MethodInfo, Component> customMethods = new Dictionary<MethodInfo, Component>();
        List<string> typedWords = new List<string>();
        GameObject ObjectHit;
        string suggestCommand = "";
        int index = 0;
        private void Start()
        {
            GetMethods();
            ListOfCommands();
        }

        public void ExecuteCommand()
        {
            ExecuteCommand(inputField.text);
        }
        void GetGameObjectFromMousePos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit RayHit))
            {
                ObjectHit = RayHit.transform.gameObject;
            }
        }
        private void Update()
        {
            GetGameObjectFromMousePos();
            if (ObjectHit != null && useMousePos && CustomConsole.Instance.closed && Input.GetMouseButtonDown(1))
            {
                UnityEngine.Debug.Log($"Selected: {ObjectHit.name} which id is {ObjectHit.GetInstanceID()}. ID copied to clipboard.");
                TextEditor te = new TextEditor();
                te.text = ObjectHit.GetInstanceID().ToString();
                te.SelectAll();
                te.Copy();
            }
            if (suggestText.text != "" && Input.GetKeyDown(KeyCode.Tab))
            {
                inputField.text = suggestCommand;
                inputField.MoveTextEnd(false);
                suggestText.text = "";
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && inputField.isFocused)
            {
                if (typedWords.Count == 0) return;
                index--;
                if (index < 0)
                {
                    index = 0;
                    inputField.MoveTextEnd(false);
                    return;
                }
                inputField.text = typedWords[index];
                inputField.MoveTextEnd(false);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && inputField.isFocused)
            {
                if (typedWords.Count == 0) return;
                index++;
                if (index >= typedWords.Count)
                {
                    index = typedWords.Count;
                    inputField.text = "";
                    inputField.MoveTextEnd(false);
                    return;
                }
                inputField.text = typedWords[index];
                inputField.MoveTextEnd(false);
            }
            else if (!inputField.isFocused)
            {
                index = typedWords.Count;
            }
        }
        void GetMethods()
        {
            foreach (GameObject item in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                foreach (Component component in item.GetComponents<MonoBehaviour>())
                {
                    Type type = component.GetType();
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<CustomCommand>() != null)
                        {
                            if (!customMethods.ContainsKey(method))
                                customMethods.Add(method, component);
                        }
                    }
                }
            }
        }
        public void ExecuteCommand(string commandName)
        {
            if (commandName != "")
                typedWords.Add(commandName);
            string test = commandName.Split(new string[] { " " }, StringSplitOptions.None).First();
            foreach (KeyValuePair<MethodInfo, Component> entry in customMethods)
            {

                if (entry.Key.Name.Equals(test, StringComparison.OrdinalIgnoreCase))
                {
                    if (entry.Key.GetParameters().Count<ParameterInfo>() == 0)
                    {
                        try
                        {
                            entry.Key.Invoke(entry.Value, null);
                            UnityEngine.Debug.Log($"Executed Command: {entry.Key.Name}");
                            inputField.text = "";
                            return;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error invoking method: {ex.Message}");
                            print(entry.Value.gameObject.GetComponent<MonoBehaviour>());
                            inputField.text = "";
                        }
                    }
                    else
                    {
                        try
                        {
                            object[] words = commandName.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                                .Skip(1) // Skip the first word (command name)
                                .Cast<object>()
                                .ToArray();
                            ParameterInfo[] parameters = entry.Key.GetParameters();
                            if (words.Length != parameters.Length)
                            {
                                UnityEngine.Debug.LogError($"Error: Expected {parameters.Length} arguments but got {words.Length}.");
                                return;
                            }
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                //UnityEngine.Debug.Log($"Checking word {i}: {words[i]}, Expected Type: {parameters[i].ParameterType}");

                                if (parameters[i].ParameterType == typeof(string))
                                {
                                    words[i] = words[i].ToString();
                                }
                                else if (parameters[i].ParameterType == typeof(int))
                                {
                                    if (int.TryParse(words[i].ToString(), out int intValue))
                                    {
                                        words[i] = intValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to int.");
                                        return;
                                    }
                                }
                                else if (parameters[i].ParameterType == typeof(float))
                                {
                                    if (float.TryParse(words[i].ToString(), out float floatValue))
                                    {
                                        words[i] = floatValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to float.");
                                        return;
                                    }
                                }
                                else if (parameters[i].ParameterType == typeof(bool))
                                {
                                    if (bool.TryParse(words[i].ToString(), out bool boolValue))
                                    {
                                        words[i] = boolValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to bool.");
                                        return;
                                    }
                                }
                            }
                            entry.Key.Invoke(entry.Value, words);
                            UnityEngine.Debug.Log($"Executed Command: {entry.Key.Name} with parameters.");
                            inputField.text = "";
                            return;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error invoking method: {ex.Message}");
                            print(entry.Value.gameObject.GetComponent<MonoBehaviour>());
                            inputField.text = "";

                        }
                    }

                }
            }
            if (commandName == "") return;
            UnityEngine.Debug.LogError("No command found with: " + commandName);
            inputField.text = "";
        }
        public void ExecuteCommandFromButton()
        {
            string commandName = inputField.text;
            if (commandName != "")
                typedWords.Add(commandName);
            string test = commandName.Split(new string[] { " " }, StringSplitOptions.None).First();
            foreach (KeyValuePair<MethodInfo, Component> entry in customMethods)
            {

                if (entry.Key.Name.Equals(test, StringComparison.OrdinalIgnoreCase))
                {
                    if (entry.Key.GetParameters().Count<ParameterInfo>() == 0)
                    {
                        try
                        {
                            entry.Key.Invoke(entry.Value, null);
                            UnityEngine.Debug.Log($"Executed Command: {entry.Key.Name}");
                            inputField.text = "";
                            return;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error invoking method: {ex.Message}");
                            print(entry.Value.gameObject.GetComponent<MonoBehaviour>());
                            inputField.text = "";
                        }
                    }
                    else
                    {
                        try
                        {
                            object[] words = commandName.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                                .Skip(1) // Skip the first word (command name)
                                .Cast<object>()
                                .ToArray();
                            ParameterInfo[] parameters = entry.Key.GetParameters();
                            if (words.Length != parameters.Length)
                            {
                                UnityEngine.Debug.LogError($"Error: Expected {parameters.Length} arguments but got {words.Length}.");
                                return;
                            }
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                //UnityEngine.Debug.Log($"Checking word {i}: {words[i]}, Expected Type: {parameters[i].ParameterType}");

                                if (parameters[i].ParameterType == typeof(string))
                                {
                                    words[i] = words[i].ToString();
                                }
                                else if (parameters[i].ParameterType == typeof(int))
                                {
                                    if (int.TryParse(words[i].ToString(), out int intValue))
                                    {
                                        words[i] = intValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to int.");
                                        return;
                                    }
                                }
                                else if (parameters[i].ParameterType == typeof(float))
                                {
                                    if (float.TryParse(words[i].ToString(), out float floatValue))
                                    {
                                        words[i] = floatValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to float.");
                                        return;
                                    }
                                }
                                else if (parameters[i].ParameterType == typeof(bool))
                                {
                                    if (bool.TryParse(words[i].ToString(), out bool boolValue))
                                    {
                                        words[i] = boolValue;
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError($"Error: Could not convert '{words[i]}' to bool.");
                                        return;
                                    }
                                }
                            }
                            entry.Key.Invoke(entry.Value, words);
                            UnityEngine.Debug.Log($"Executed Command: {entry.Key.Name} with parameters.");
                            inputField.text = "";
                            return;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error invoking method: {ex.Message}");
                            print(entry.Value.gameObject.GetComponent<MonoBehaviour>());
                            inputField.text = "";

                        }
                    }

                }
            }
            if (commandName == "") return;
            UnityEngine.Debug.LogError("No command found with: " + commandName);
            inputField.text = "";
        }
        public void ListOfCommands()
        {
            if (suggestArea.transform.childCount > 0)
            {
                for (int i = 0; i < suggestArea.transform.childCount; i++)
                {
                    Destroy(suggestArea.transform.GetChild(i).gameObject);
                }
            }
            suggestArea.SetActive(true);
            foreach (KeyValuePair<MethodInfo, Component> entry in customMethods)
            {
                Button butt = Instantiate(commandButton, suggestArea.transform);
                butt.GetComponentInChildren<TextMeshProUGUI>().text = entry.Key.Name;
                butt.onClick.AddListener(() => WriteToConsole(entry.Key.Name));
                var attribute = entry.Key.GetCustomAttribute<CustomCommand>();
                butt.GetComponent<PointerBehavior>().hoverText = attribute.ToolTip;
            }
            if (CustomConsole.Instance.closed)
            {
                suggestArea.SetActive(true);
                commandScrollView.SetActive(true);
                UnityEngine.Debug.Log("Updated command list!");
            }
            else
            {
                suggestArea.SetActive(false);
                commandScrollView.SetActive(false);
            }

        }
        public void ShowCommands()
        {
            if (!CustomConsole.Instance.closed) return;
            suggestArea.SetActive(suggestArea.activeInHierarchy ? false : true);
            commandScrollView.SetActive(commandScrollView.activeInHierarchy ? false : true);
        }
        public void WriteToConsole(string text)
        {
            inputField.text = text.ToLower();
            inputField.MoveTextEnd(false);
            suggestText.text = "";

        }
        public void SuggestCommand()
        {
            string catchText = inputField.text;
            string currentText = inputField.text;
            if (currentText == "")
            {
                suggestText.text = "";
                suggestCommand = "";
                return;
            }
            foreach (KeyValuePair<MethodInfo, Component> entry in customMethods)
            {
                if (entry.Key.Name.Contains(currentText, StringComparison.OrdinalIgnoreCase))
                {
                    string suggestedCompletion = new String(' ', currentText.Length + 10) + "TAB => " + entry.Key.Name.ToLower();
                    suggestText.text = currentText + suggestedCompletion.ToLower();

                    suggestCommand = entry.Key.Name.ToLower();
                    inputField.text = catchText.ToLower();
                    return;
                }
            }
            suggestText.text = "";
            suggestCommand = "";
            inputField.text = catchText.ToLower();
        }
    }
}

