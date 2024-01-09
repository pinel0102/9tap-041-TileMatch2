using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class DebugButton : MonoBehaviour
{
    public string buttonText;

    public void Initialize(Action onClick = null)
    {
        if(TryGetComponent(out Button button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { onClick?.Invoke(); });

            if(!buttonText.Equals(string.Empty))
            {
                if(TryGetComponentInChildren(out TMP_Text _text))
                {
                    _text.SetText(buttonText);
                }
            }
        }
    }

    private bool TryGetComponentInChildren<T>(out T component)
    {
        for(int i=0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).TryGetComponent(out component))
            {
                return true;
            }
        }

        component = default;
        return false;
    }
}