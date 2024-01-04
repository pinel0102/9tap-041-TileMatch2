using UnityEngine;
using System;

/// <author>Pinelia Luna</author>
/// <Summary>DragZoom v1.1
/// <para>오브젝트 드래그 줌 클래스.</para>
/// </Summary>
public class DebugDragZoom : MonoBehaviour
{
    [Header("★ [Parameter] Live")]
    public bool isScaling;

    [Header("★ [Reference] Nullable")]
    public Transform transformToScale;
    public DebugDragObject dragObject;

    [Header("★ [Settings] Initialize")]
    public bool initOnEnable = true;
    
    [Header("★ [Settings] Zoom")]
    public float MinScale = 1f;
    public float MaxScale = 2f;
    public float ScaleIncreaseTouch = 0.005f;
    public float ScaleIncreaseArrow = 0.01f;

    private bool savedMultiTouchStatus;
    private Vector3 _initScale = Vector3.one;
    private Vector3 _currentScale;

    private void Awake()
    {
        isScaling = false;

        if(!transformToScale)
            transformToScale = transform;
        
        _initScale = transformToScale.localScale;
        _currentScale = _initScale;
    }

    private void OnEnable()
    {
        if (initOnEnable)
            transformToScale.localScale = _initScale;
        
        _currentScale = transformToScale.localScale;

        savedMultiTouchStatus = Input.multiTouchEnabled;
        Input.multiTouchEnabled = true;

        isScaling = false;
    }

    private void OnDisable()
    {
        Input.multiTouchEnabled = savedMultiTouchStatus;

        isScaling = false;
    }
 
    private void Update()
    {
        if (dragObject != null && dragObject.isDragging) return;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            AdjustScale(ScaleIncreaseArrow);
        }
        else if(Input.GetKey(KeyCode.DownArrow))
        {
            AdjustScale(-ScaleIncreaseArrow);
        }
        else
        {
            UpdateWithTouch();
        }

        if (Input.touchCount < 2)
        {
            isScaling = false;
        }
    }

    private void UpdateWithTouch()
    {
        if (Input.touchCount >= 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0); //첫번째 손가락 좌표
            Touch touchOne = Input.GetTouch(1); //두번째 손가락 좌표

            // deltaposition은 deltatime과 동일하게 delta만큼 시간동안 움직인 거리를 말한다.
            // 현재 position에서 이전 delta값을 빼주면 움직이기 전의 손가락 위치가 된다.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // 현재와 과거값의 움직임의 크기를 구한다.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // 두 값의 차이는 즉 확대/축소할때 얼만큼 많이 확대/축소가 진행되어야 하는지를 결정한다.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            AdjustScale(-deltaMagnitudeDiff * ScaleIncreaseTouch);
        }
        else
        {
            isScaling = false;
        }
    }

    private void AdjustScale(float scaleIncrement)
    {
        var scaleAdjustment = _currentScale.x + scaleIncrement;
        
        if (scaleAdjustment <= MinScale)
        {
            if (_currentScale.x != MinScale)
            {
                _currentScale.x = MinScale;
                _currentScale.y = MinScale;
                transformToScale.localScale = _currentScale;

                //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToScale.localScale, transformToScale.localPosition));
            }

            return;
        } 
        else if (scaleAdjustment >= MaxScale)
        {
            if (_currentScale.x != MaxScale)
            {
                _currentScale.x = MaxScale;
                _currentScale.y = MaxScale;
                transformToScale.localScale = _currentScale;

                //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToScale.localScale, transformToScale.localPosition));
            }

            return;
        }
 
        _currentScale.x = scaleAdjustment;
        _currentScale.y = scaleAdjustment; 
        transformToScale.localScale = _currentScale;

        isScaling = true;

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToScale.localScale, transformToScale.localPosition));
    }
}