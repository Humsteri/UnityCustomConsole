using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using System.Collections;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using Unity.CodeEditor;
#endif
namespace CustomConsolePackage
{
    public class CustomConsole : MonoBehaviour
    {
        #region Singleton
        public static CustomConsole Instance { get; private set; }
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

        string output = "";
        string rawOutput = "";
        string stack = "";
        bool warning = true;
        bool normal = true;
        bool error = true;
        [Header("Logging type")]
        [SerializeField] StackTraceLength chosenType;
        [Header("UI Buttons")]
        [SerializeField] GameObject normalLogButton;
        [SerializeField] GameObject warningLogButton;
        [SerializeField] GameObject errorLogButton;

        [Header("UI Components")]
        [SerializeField] GameObject poolHolder;
        [SerializeField] GameObject commandHolder;
        [SerializeField] GameObject commandScrollView;
        [SerializeField] GameObject logArea;

        [Header("Other")]
        [SerializeField] TextMeshProUGUI logPrefab;
        [SerializeField] Animator animator;
        [HideInInspector] public bool closed = false;
        [SerializeField] KeyCode openConsoleKey;
        Color normalLogButtonStartColor;
        Color warningLogButtonStartColor;
        Color errorLogButtonStartColor;
        public GameObject hoverInfo;

        [Header("Pooling")]
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxCapacity = 100;
        [SerializeField] float waitTimeBeforePool = 30;
        ObjectPool<TextMeshProUGUI> pool;

        [Header("Custom Grid Layout Group sizes")]
        [Tooltip("Custom grid layout so few variables can be changed")]
        [SerializeField] GridLayoutGroupCustom gridLayoutGroup;
        [Tooltip("Cell Size Y when Stack Trace Length is set to short")]
        [SerializeField] float shortCellSizeY = 30f;
        [Tooltip("Cell Size Y when Stack Trace Length is set to normal")]
        [SerializeField] float normalCellSizeY = 100f;
        [Tooltip("Spacing Y when Stack Trace Length is set to short")]
        [SerializeField] float shortSpacingY = 3.5f;
        [Tooltip("Spacing Y when Stack Trace Length is set to normal")]
        [SerializeField] float normalSpacingY = 15f;
        enum StackTraceLength
        {
            None,
            Normal,
            Short
        }
        private void Start()
        {
            pool = new ObjectPool<TextMeshProUGUI>(() =>
            {
                return Instantiate(logPrefab, poolHolder.transform);
            }, logPrefab =>
            {
                logPrefab.gameObject.SetActive(true);

            }, logPrefab =>
            {
                logPrefab.gameObject.SetActive(false);
                logPrefab.transform.SetParent(poolHolder.transform);
            }, logPrefab =>
            {
                Destroy(logPrefab);
            }, false, defaultCapacity, maxCapacity);

            normalLogButtonStartColor = normalLogButton.GetComponent<Image>().color;
            warningLogButtonStartColor = warningLogButton.GetComponent<Image>().color;
            errorLogButtonStartColor = errorLogButton.GetComponent<Image>().color;
        }
        void OpenInEditor(string fileName, [CallerLineNumber] int lineNumber = 0)
        {
#if UNITY_EDITOR
            if (CodeEditor.Editor != null)
            {
                CodeEditor.Editor.CurrentCodeEditor.OpenProject(fileName, lineNumber, 1);
            }
            else
            {
                Debug.LogError("No external code editor is set in Unity preferences.");
            }
#elif !UNITY_EDITOR
        Debug.LogError("Only supported in editor!. Would open the path to error in code editor.");
#endif
        }
        private void Update()
        {
            if (ExecutingCustomCommand.Instance.commandInputField.isFocused) return;
            if(Input.GetKeyDown(openConsoleKey))
            {
                PlayAnimation();
            }
        }
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            Color chosenColor = new Color();
            string errorFilePath = "";
            int errorLine = 0;
            switch (type)
            {
                case LogType.Error:
                    if (!error) return;
                    chosenColor = Color.red;
                    errorFilePath = stackTrace.Split("(at ").Last().Split(')')[0];
                    string[] parts = errorFilePath.Split(':');
                    errorFilePath = string.Join(":", parts.Take(parts.Length - 1));
                    errorLine = int.Parse(parts.Last());
                    break;
                case LogType.Assert:
                    break;
                case LogType.Warning:
                    if (!warning) return;
                    errorFilePath = "";
                    errorLine = 0;
                    chosenColor = Color.yellow;
                    break;
                case LogType.Log:
                    if (!normal) return;
                    chosenColor = Color.white;
                    errorFilePath = "";
                    errorLine = 0;
                    break;
                case LogType.Exception:
                    break;
                default:
                    break;
            }
            rawOutput = logString;
            output = "[" + System.DateTime.Now.ToString("H:mm:ss") + "]" + " " + logString;
            stack = stackTrace;
            TextMeshProUGUI component = pool.Get();
            component.color = chosenColor;
            if (errorFilePath != "" && errorLine != 0)
                component.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OpenInEditor(errorFilePath, errorLine);
                });
            SetLogType(component, stackTrace);

            component.fontSize = 30;
            component.transform.SetParent(logArea.transform, false);
            StartCoroutine(ReleaseBackToPool(component, waitTimeBeforePool));
        }
        void SetLogType(TextMeshProUGUI component, string stackTrace)
        {
            switch (chosenType)
            {
                case StackTraceLength.None:
                    gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, shortCellSizeY);
                    gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, shortSpacingY);
                    component.text = output;
                    break;
                case StackTraceLength.Normal:
                    gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, normalCellSizeY);
                    gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, normalSpacingY);
                    component.text = output + ", " + stackTrace;
                    break;
                case StackTraceLength.Short:
                    gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, shortCellSizeY);
                    gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, shortSpacingY);
                    component.text = output + ", From: " + stackTrace.Split(new string[] { "(at " }, StringSplitOptions.None).Last();
                    break;
                default:
                    break;
            }
        }
        public void ClearConsole()
        {
            for (int i = logArea.transform.childCount - 1; i >= 0; i--)
            {
                pool.Release(logArea.transform.GetChild(i).GetComponent<TextMeshProUGUI>());
            }
        }
        public void PlayAnimation()
        {
            closed = !closed;
            if (commandHolder.activeInHierarchy) commandHolder.SetActive(false);
            if (commandScrollView.activeInHierarchy) commandScrollView.SetActive(false);
            animator.SetBool("Play", closed);
        }
        IEnumerator ReleaseBackToPool(TextMeshProUGUI logItem, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (logItem.IsActive())
                pool.Release(logItem);
        }
        public void NormalLog()
        {
            normal = normal ? false : true;
            normalLogButton.GetComponent<Image>().color = normal ? normalLogButtonStartColor : normalLogButtonStartColor - new Color(0, 0, 0, 0.8f);
        }
        public void ErrorLog()
        {
            error = error ? false : true;
            errorLogButton.GetComponent<Image>().color = error ? errorLogButtonStartColor : errorLogButtonStartColor - new Color(0, 0, 0, 0.8f);
        }
        public void WarningLog()
        {
            warning = warning ? false : true;
            warningLogButton.GetComponent<Image>().color = warning ? warningLogButtonStartColor : warningLogButtonStartColor - new Color(0, 0, 0, 0.8f);
        }
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }
        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}

