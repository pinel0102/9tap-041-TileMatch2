using UnityEngine;

/// <author>Pinelia Luna</author>
/// <Summary>NotchScreen v1.0
/// <para>노치 스크린 보정용 클래스.</para>
/// <para>Initialize(canvasHeight)로 초기화시 RectTransform Array에 등록된 오브젝트들이 보정됩니다.</para>
/// <para>public 함수들을 이용해 추가 오브젝트들을 보정할 수 있습니다.</para>
/// </Summary>
public class NotchScreen : SingletonMono<NotchScreen>
{
    [Header("★ [Settings] Manual Notch Size")]
    public bool useManualNotchSize;
    public float manualNotchSize = 50;
    
    [Header("★ [Live] Notch Screen")]
    public bool isNotchScreen;

    [Header("★ [Live] Device")]
    public float screenHeightDevice;
    public float safeHeightDevice;
    public float notchSizeTopDevice;
    public float notchSizeBottomDevice;

    [Header("★ [Live] Canvas")]
    public float screenHeightCanvas;
    public float safeHeightCanvas;
    public float notchSizeTopCanvas;
    public float notchSizeBottomCanvas;

    [Header("★ [Reference] Notch Adjust : Top")]
    public RectTransform[] notchTopDownArray;
    public RectTransform[] notchTopSpanArray;
    public RectTransform[] notchTopReduceArray;
    public RectTransform[] notchTopOffsetArray;

    [Header("★ [Reference] Notch Adjust : Bottom")]
    public RectTransform[] notchBottomUpArray;
    public RectTransform[] notchBottomSpanArray;
    public RectTransform[] notchBottomReduceArray;
    public RectTransform[] notchBottomOffsetArray;

    [Header("★ [Reference] Notch Adjust : Manual")]
    public RectTransform[] notchManualOffsetBottom;
    public float[] notchManualOffsetBottomValue;
    public RectTransform[] notchManualOffsetTop;
    public float[] notchManualOffsetTopValue;

    private bool isInitialized;


#region Initialize

    public void Initialize(float canvasHeight)
    {
        isNotchScreen = false;

        GetNotchSize_Device();
        GetNotchSize_Canvas(canvasHeight);

        isInitialized = true;

#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
    	
		if (!Mathf.Approximately(screenHeightDevice, safeHeightDevice)) 
        {
            isNotchScreen = true;

            AdjustTop_Down(notchTopDownArray);
            AdjustTop_Span(notchTopSpanArray);
            AdjustTop_Reduce(notchTopReduceArray);
            AdjustTop_Offset(notchTopOffsetArray);

            AdjustBottom_Up(notchBottomUpArray);
            AdjustBottom_Span(notchBottomSpanArray);
            AdjustBottom_Reduce(notchBottomReduceArray);
            AdjustBottom_Offset(notchBottomOffsetArray);

            AdjustManual_OffsetBottom(notchManualOffsetBottom, notchManualOffsetBottomValue);
            AdjustManual_OffsetTop(notchManualOffsetTop, notchManualOffsetTopValue);
		}
#endif
    }

    private void GetNotchSize_Device()
    {
        var safeRect = Screen.safeArea;

        // [iPhone X] (1125 x 2436)
        // Screen.safeArea = Rect (0, 102, 1125, 2202)
        // Debug.Log(CodeManager.GetMethodName() + string.Format("{0}, {1}, {2}, {3}", safeRect.x, safeRect.y, safeRect.width, safeRect.height));

		screenHeightDevice = Screen.height; // [iPhone X] 2436
        safeHeightDevice = safeRect.height; // [iPhone X] 2202
        notchSizeTopDevice = screenHeightDevice - (safeRect.y + safeRect.height); // [iPhone X] 132
        notchSizeBottomDevice = safeRect.y; // [iPhone X] 102
    }

    private void GetNotchSize_Canvas(float canvasHeight)
    {
        float ratio = (canvasHeight / screenHeightDevice);

        screenHeightCanvas = screenHeightDevice * ratio;
        safeHeightCanvas = safeHeightDevice * ratio;
        notchSizeTopCanvas = notchSizeTopDevice * ratio;
        notchSizeBottomCanvas = notchSizeBottomDevice * ratio;

        if (useManualNotchSize)
            notchSizeTopCanvas = manualNotchSize;
    }

#endregion Initialize


#region Public Functions

    ///<Summary>notchSizeTopCanvas만큼 위치를 내린다.</Summary>
    public void AdjustTop_Down(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newPos = array[i].anchoredPosition;
                newPos.y -= notchSizeTopCanvas;
                array[i].anchoredPosition = newPos;
            }
        }
    }

    ///<Summary>notchSizeTopCanvas만큼 사이즈를 늘린다.</Summary>
    public void AdjustTop_Span(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newSize = array[i].sizeDelta;
                newSize.y += notchSizeTopCanvas;
                array[i].sizeDelta = newSize;
            }
        }
    }

    ///<Summary>notchSizeTopCanvas만큼 사이즈를 줄인다.</Summary>
    public void AdjustTop_Reduce(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newSize = array[i].sizeDelta;
                newSize.y -= notchSizeTopCanvas;
                array[i].sizeDelta = newSize;
            }
        }
    }

    ///<Summary>notchSizeTopCanvas만큼 Offset Top을 내려서 영역을 줄인다.</Summary>
    public void AdjustTop_Offset(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newOffset = array[i].offsetMax;
                newOffset.y -= notchSizeTopCanvas;
                array[i].offsetMax = newOffset;
            }
        }
    }

    ///<Summary>notchSizeBottomCanvas만큼 위치를 올린다.</Summary>
    public void AdjustBottom_Up(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newPos = array[i].anchoredPosition;
                newPos.y += notchSizeBottomCanvas;
                array[i].anchoredPosition = newPos;
            }
        }
    }

    ///<Summary>notchSizeBottomCanvas만큼 사이즈를 늘린다.</Summary>
    public void AdjustBottom_Span(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newSize = array[i].sizeDelta;
                newSize.y += notchSizeBottomCanvas;
                array[i].sizeDelta = newSize;
            }
        }
    }

    ///<Summary>notchSizeBottomCanvas만큼 사이즈를 줄인다.</Summary>
    public void AdjustBottom_Reduce(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newSize = array[i].sizeDelta;
                newSize.y -= notchSizeBottomCanvas;
                array[i].sizeDelta = newSize;
            }
        }
    }

    ///<Summary>notchSizeBottomCanvas만큼 Offset Bottom을 올려서 영역을 줄인다.</Summary>
    public void AdjustBottom_Offset(RectTransform[] array)
    {
        if (!isInitialized) return;

        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newOffset = array[i].offsetMin;
                newOffset.y += notchSizeBottomCanvas;
                array[i].offsetMin = newOffset;
            }
        }
    }

    ///<Summary>
    ///<para>offsetValue가 (+)면 Offset Bottom을 내려서 영역을 늘린다.</para>
    ///<para>offsetValue가 (-)면 Offset Bottom을 올려서 영역을 줄인다.</para>
    ///</Summary>
    public void AdjustManual_OffsetBottom(RectTransform[] array, float[] offsetValue)
    {
        if (array.Length != offsetValue.Length)
            return;
        
        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newOffset = array[i].offsetMin;
                newOffset.y -= offsetValue[i];
                array[i].offsetMin = newOffset;
            }
        }
    }

    ///<Summary>
    ///<para>offsetValue가 (+)면 Offset Top을 올려서 영역을 늘린다.</para>
    ///<para>offsetValue가 (-)면 Offset Top을 내려서 영역을 줄인다.</para>
    ///</Summary>
    public void AdjustManual_OffsetTop(RectTransform[] array, float[] offsetValue)
    {
        if (array.Length != offsetValue.Length)
            return;
        
        for (int i=0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                Vector2 newOffset = array[i].offsetMax;
                newOffset.y += offsetValue[i];
                array[i].offsetMax = newOffset;
            }
        }
    }

#endregion Public Functions

}
