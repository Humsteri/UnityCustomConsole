using TMPro;
using UnityEngine;
using System;
namespace CustomConsolePackage
{
    public class CopyText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        public void CopyContents()
        {
            TextEditor te = new TextEditor();
            te.text = text.text;
            te.SelectAll();
            te.Copy();
        }
    }
}

