using UnityEngine;
using System;
using TMPro;

public class DebugInput : MonoBehaviour
{
    private string stringValue;
    private int intValue;

    public void Initialize(Action<int> onClick)
    {
        InitValue();

        if(TryGetComponentInChildren(out TMP_InputField inputField))
        {
            ResetListeners(inputField);

            if(TryGetComponentInChildren(out DebugButton debugButton))
            {
                debugButton.Initialize(() => {onClick?.Invoke(intValue);});
            }
        }
    }

    public void Initialize(Action<string> onClick)
    {
        InitValue();

        if(TryGetComponentInChildren(out TMP_InputField inputField))
        {
            ResetListeners(inputField);

            if(TryGetComponentInChildren(out DebugButton debugButton))
            {
                debugButton.Initialize(() => {onClick?.Invoke(stringValue);});
            }
        }
    }

    private void InitValue()
    {
        stringValue = string.Empty;
        intValue = 0;
    }

    private void ResetListeners(TMP_InputField inputField)
    {
        inputField.onValueChanged.RemoveAllListeners();
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onSelect.RemoveAllListeners();
        inputField.onDeselect.RemoveAllListeners();
        inputField.onValueChanged.AddListener(RefreshValue);
        inputField.onEndEdit.AddListener(RefreshValue);
        inputField.onSelect.AddListener(RefreshValue);
        inputField.onDeselect.AddListener(RefreshValue);
    }

    private void RefreshValue(string newValue)
    {
        stringValue = newValue;
        if(!int.TryParse(stringValue, out intValue))
            intValue = 0;
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
