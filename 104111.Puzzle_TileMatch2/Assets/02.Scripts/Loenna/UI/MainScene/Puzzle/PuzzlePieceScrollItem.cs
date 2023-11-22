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

	public Action<PuzzlePieceItemData, PointerEventData> OnMovePiece;
	public Action<PuzzlePieceItemData> OnTryUnlock;
}

public class PuzzlePieceScrollItem : InfiniteScrollItem, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private const float RESIZE_RATIO = 0.8f;
	private const float THRESHOLD = 0.25f;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private GameObject m_locked;

	[SerializeField]
	private TMP_Text m_text;
    private int cost;

	private ScrollRect m_scrollRect;

	private bool m_dragging = false;
	private bool m_scrolling = false;

	public override void Initalize(InfiniteScroll scroll, int itemIndex)
	{
		base.Initalize(scroll, itemIndex);
		
		m_scrollRect = scroll.GetComponent<ScrollRect>();
	}

	public override void UpdateData(InfiniteScrollData scrollData)
	{
		m_text.text = string.Empty;
		base.UpdateData(scrollData);

		if (scrollData is PuzzlePieceItemData itemData)
		{
            cost = itemData.Cost;
            m_text.text = itemData.IsLocked ? cost.ToString() : string.Empty;
			m_locked.SetActive(itemData.IsLocked);
			m_image.sprite = itemData.Sprite;
			m_image.rectTransform.SetSize(itemData.Size * RESIZE_RATIO);
			return;
		}

		m_image.color = Color.clear;
	}

    public void OnPointerDown()
    {
        if (m_dragging || m_scrolling)
            return;
        
        if (scrollData is PuzzlePieceItemData itemData)
        {
            if (itemData.IsLocked)
            {
                itemData.OnTryUnlock?.Invoke(itemData);
            }
            else
            {
                //Select Piece
            }
        }
    }

    public void OnPointerUp()
    {
        if (m_dragging)
            return;

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
			itemData.OnMovePiece?.Invoke(itemData, eventData);

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
