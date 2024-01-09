using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DebugPanel : MonoBehaviour
{
    public List<DebugButton> buttonList = new List<DebugButton>();
    public List<DebugInput> inputListInt = new List<DebugInput>();
    public List<DebugInput> inputListString = new List<DebugInput>();

    private void Awake()
    {   
        Initialize();
    }

    private void Initialize()
    {
        ClearList();
        SetupButtonListeners();

        for(int i=0; i < buttonList.Count; i++)
        {
            if (buttonList[i] != null && buttonActionList[i] != null)
                buttonList[i].Initialize(buttonActionList[i]);
        }

        for(int i=0; i < inputListInt.Count; i++)
        {
            if (inputListInt[i] != null && inputActionListInt[i] != null)
                inputListInt[i].Initialize(inputActionListInt[i]);
        }

        for(int i=0; i < inputListString.Count; i++)
        {
            if (inputListString[i] != null && inputActionListString[i] != null)
                inputListString[i].Initialize(inputActionListString[i]);
        }
    }
}
