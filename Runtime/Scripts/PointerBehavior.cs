using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomConsolePackage
{
    public class PointerBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        GameObject hover;
        TextMeshProUGUI hoversText;
        public string hoverText;
        [SerializeField] Vector2 offSet;
        private void Start()
        {
            hover = CustomConsole.Instance.hoverInfo;
            hoversText = hover.GetComponentInChildren<TextMeshProUGUI>();
            hover.SetActive(false);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverText == "") return;
            hover.SetActive(true);
            hoversText.text = hoverText;
            hover.transform.position = eventData.position + offSet;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover.SetActive(false);
        }
    }
}

