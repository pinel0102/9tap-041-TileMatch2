using UnityEngine;
using UnityEngine.EventSystems;

/// <author>Pinelia Luna</author>
/// <Summary>DragObject v1.3
/// <para>오브젝트 드래그 클래스.</para>
/// </Summary>
public class DebugDragObject : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("★ [Parameter] Live")]
    public bool isDragging;

    [Header("★ [Reference] Nullable")]
    public Canvas rootCanvas;
    public Transform transformToDrag;
    public DebugDragZoom dragZoom;

    [Header("★ [Settings] Initialize")]
    public bool initOnEnable = true;
    
    [Header("★ [Settings] Manual Position")]
    public bool isManualInitPosition;
    public bool isLocalPosition = true;
    public Vector2 manualInitPosition = Vector2.zero;
    
    private Vector3 _initPosition = Vector3.zero;
    private Vector3 _offset;
    private bool _isOrthographicCamera;
    private int _pointerId;

    private void Awake()
    {
        isDragging = false;
        
        if(!transformToDrag)
            transformToDrag = transform;

        _isOrthographicCamera = Camera.main.orthographic;
        _initPosition = transformToDrag.position;
    }

    private void OnEnable()
    {
        if (initOnEnable)
        {
            if (isManualInitPosition)
            {
                if (isLocalPosition)
                    transformToDrag.localPosition = manualInitPosition;
                else
                    transformToDrag.position = manualInitPosition;
            }
            else
                transformToDrag.position = _initPosition;
        }

        isDragging = false;
    }

    private void OnDisable()
    {
        isDragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerId = eventData.pointerId;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragZoom != null && dragZoom.isScaling) return;
        if (eventData.pointerId != _pointerId) return;

        _offset = _isOrthographicCamera ? 
            (rootCanvas ? (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? eventData.position : rootCanvas.transform.localScale.x * Camera.main.ScreenToWorldPoint(eventData.position)) : Camera.main.ScreenToWorldPoint(eventData.position)) - transformToDrag.position : 
            //(Camera.main.ScreenToWorldPoint(eventData.position) * (rootCanvas ? rootCanvas.transform.localScale.x : 1)) - transformToDrag.position : 
            Input.mousePosition - transformToDrag.position;

        isDragging = true;

        if (transformToDrag.position.z != _initPosition.z)
            transformToDrag.position = new Vector3(transformToDrag.position.x, transformToDrag.position.y, _initPosition.z);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", eventData.position, Camera.main.ScreenToWorldPoint(eventData.position)));
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToDrag.localScale, transformToDrag.localPosition));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragZoom != null && dragZoom.isScaling) return;
        if (eventData.pointerId != _pointerId) return;

        transformToDrag.position = _isOrthographicCamera ? 
            (rootCanvas ? (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? eventData.position : rootCanvas.transform.localScale.x * Camera.main.ScreenToWorldPoint(eventData.position)) : Camera.main.ScreenToWorldPoint(eventData.position)) - _offset : 
            //(Camera.main.ScreenToWorldPoint(eventData.position) * (rootCanvas ? rootCanvas.transform.localScale.x : 1)) - _offset : 
            Input.mousePosition - _offset;

        if (transformToDrag.position.z != _initPosition.z)
            transformToDrag.position = new Vector3(transformToDrag.position.x, transformToDrag.position.y, _initPosition.z);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", eventData.position, Camera.main.ScreenToWorldPoint(eventData.position)));
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToDrag.localScale, transformToDrag.localPosition));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragZoom != null && dragZoom.isScaling) return;
        if (eventData.pointerId != _pointerId) return;

        _offset = Vector3.zero;
        isDragging = false;

        if (transformToDrag.position.z != _initPosition.z)
            transformToDrag.position = new Vector3(transformToDrag.position.x, transformToDrag.position.y, _initPosition.z);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", transformToDrag.localScale, transformToDrag.localPosition));
    }
}