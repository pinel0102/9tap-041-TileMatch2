using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;

using Gpm.Ui;

using TMPro;

public class PuzzlePieceItemData : InfiniteScrollData
{
	public int Index;
    public int Cost;
	public Sprite Sprite;
	public float Size;
	
	public bool IsLocked;

    //public Action<PuzzlePieceItemData, PointerEventData> OnMovePiece;
	public Action<PuzzlePieceItemData, Vector2, Action> OnTryUnlock;
    public Action<PuzzlePieceItemData, Vector2, Action> MovePiece;
}

public class PuzzlePieceScrollItem : InfiniteScrollItem, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private const float RESIZE_RATIO = 0.8f;
	private const float THRESHOLD = 0.25f;

	[SerializeField]
	private Image m_image;
    [SerializeField]
	private Image m_ssuImage;

	[SerializeField]
	private GameObject m_locked;

	[SerializeField]
	private TMP_Text m_text;
    private int cost;
	private ScrollRect m_scrollRect;
    public int Index;
    public CanvasGroup canvasGroup;
    private bool m_interacteble = false;
    private bool m_moving = false;
	private bool m_dragging = false;
	private bool m_scrolling = false;

	public override void Initalize(InfiniteScroll scroll, int itemIndex)
	{
		base.Initalize(scroll, itemIndex);
		
		m_scrollRect = scroll.GetComponent<ScrollRect>();
        m_moving = false;
	}

	public override void UpdateData(InfiniteScrollData scrollData)
	{
		m_text.text = string.Empty;
		base.UpdateData(scrollData);

		if (scrollData is PuzzlePieceItemData itemData)
		{
            Index = itemData.Index;
            cost = itemData.Cost;
            m_text.text = itemData.IsLocked ? cost.ToString() : string.Empty;
			m_locked.SetActive(itemData.IsLocked);
			m_image.sprite = itemData.Sprite;
			m_image.rectTransform.SetSize(itemData.Size * RESIZE_RATIO);
            m_ssuImage.sprite = itemData.Sprite;
            
            ShowItem();
			return;
		}

		m_image.color = Color.clear;
	}

    public void ShowItem()
    {
        //Debug.Log(CodeManager.GetMethodName() + Index);
        canvasGroup.alpha = 1f;
        m_interacteble = true;
    }

    public void HideItem()
    {
        //Debug.Log(CodeManager.GetMethodName() + Index);
        canvasGroup.alpha = 0.3f;
        m_interacteble = false;
    }

    public void OnPointerDown()
    {
        if (!m_interacteble || m_dragging || m_scrolling)
            return;
        
        if (scrollData is PuzzlePieceItemData itemData)
        {
            if (itemData.IsLocked)
            {
                itemData.OnTryUnlock?.Invoke(itemData, m_image.transform.position, MoveComplete);
                MoveStart();
            }
            else
            {
                //Select Piece
                itemData.MovePiece?.Invoke(itemData, m_image.transform.position, MoveComplete);
                MoveStart();
            }
        }
    }

    private void MoveStart()
    {
        //m_scrollRect.StopMovement();
        m_moving = true;
    }

    private void MoveComplete()
    {
        m_moving = false;
    }

    public void OnPointerUp()
    {
        //if (m_dragging)
        //    return;

        m_dragging = false;
		m_scrolling = false;

        /*if (scrollData is PuzzlePieceItemData itemData)
        {
            if (itemData.IsLocked)
            {
                itemData.OnTryUnlock?.Invoke(itemData);
            }
        }*/
    }

    public void OnBeginDrag(PointerEventData eventData)
	{
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);
        m_dragging = false;
		m_scrolling = false;

		m_scrollRect?.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);
        var (x, y) = eventData.delta.normalized;

		if (m_dragging)
		{
			if (m_scrolling)
			{
				m_scrollRect?.OnDrag(eventData);
			}
			return;
		}

		m_dragging = true;

		if (scrollData is not PuzzlePieceItemData itemData)
		{
			return;
		}

		if (itemData.IsLocked || Mathf.Abs(x) >= Mathf.Abs(y) + THRESHOLD)
		{
			m_scrollRect?.OnDrag(eventData);
			m_scrolling = true;
		}
		else
		{
			//itemData.OnMovePiece?.Invoke(itemData, eventData);

			m_scrolling = false;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
        //Debug.Log(CodeManager.GetMethodName() + gameObject.name);
        
		if (m_dragging && m_scrolling)
		{		
			m_scrollRect?.OnEndDrag(eventData);
		}

		m_dragging = false;
		m_scrolling = false;

		/*if (eventData.delta.magnitude < 1f && scrollData is PuzzlePieceItemData itemData)
		{
			if (itemData.IsLocked)
			{
				itemData.OnTryUnlock?.Invoke(itemData);
			}
		}*/
	}
}
