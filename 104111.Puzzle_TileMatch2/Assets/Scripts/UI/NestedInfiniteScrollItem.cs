using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Gpm.Ui;

public class NestedInfiniteScrollItem : InfiniteScrollItem, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
   	private ScrollRect m_horizontal;

	[SerializeField]
	private ScrollRect m_vertical;

	private bool m_dragging = false;
	private bool m_isVertical = true;

	public override void Initalize(InfiniteScroll infiniteScroll, int itemIndex)
	{
		base.Initalize(infiniteScroll, itemIndex);

		ScrollRect inner = infiniteScroll.ScrollRect;
		ScrollRect outer = infiniteScroll.transform.parent.GetComponentInParent<ScrollRect>();

		(m_horizontal, m_vertical) = inner.vertical switch {
			true => (outer, inner),
			_ => (inner, outer)
		};
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_horizontal?.OnBeginDrag(eventData);
		m_vertical?.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		var (x, y) = eventData.delta;
		
		if (!m_dragging)
		{
			m_dragging = true;
			if (Mathf.Abs(x) > Mathf.Abs(y))
			{
				m_horizontal?.OnDrag(eventData);
				m_isVertical = false;
			}
			else
			{
				m_vertical?.OnDrag(eventData);
				m_isVertical = true;
			}
		}
		else
		{
			if (m_isVertical)
			{
				m_vertical?.OnDrag(eventData);
			}
			else
			{
				m_horizontal?.OnDrag(eventData);
			}
		}	
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_horizontal?.OnEndDrag(eventData);
		m_vertical?.OnEndDrag(eventData);
		m_dragging = false;
	}
}
