using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomConsolePackage
{
    public class PointerBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        GameObject hover;
        Transform hoverPos;
        TextMeshProUGUI hoversText;
        public string hoverText;
        [SerializeField] Vector2 offSet;
        private void Start()
        {
            hover = CustomConsole.Instance.hoverInfo;
            hoverPos = CustomConsole.Instance.hoverPos;
            hoversText = hover.GetComponentInChildren<TextMeshProUGUI>();
            hover.SetActive(false);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverText == "") return;
            hover.transform.position = new Vector2(hoverPos.position.x, eventData.position.y);
            hover.SetActive(true);
            hoversText.text = hoverText;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover.SetActive(false);
        }
    }
}

