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
    public Action<PuzzlePieceItemData, Vector2, Action<bool>> OnTryUnlock;
    public Action<PuzzlePieceItemData, Vector2, Action<bool>> MovePiece;
}

public class PuzzlePieceScrollItem : InfiniteScrollItem
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
	public int Index;
    public CanvasGroup canvasGroup;
    private bool m_interacteble = false;

	public override void Initalize(InfiniteScroll scroll, int itemIndex)
	{
		base.Initalize(scroll, itemIndex);
		
		SetInteractable(true);
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

    public void SetInteractable(bool interactable)
    {
        Debug.Log(CodeManager.GetMethodName() + interactable);
        m_interacteble = interactable;
    }

    public void OnPointerDown()
    {
        if (!m_interacteble)
            return;
        
        if (scrollData is PuzzlePieceItemData itemData)
        {
            SetInteractable(false);

            if (itemData.IsLocked)
            {
                itemData.OnTryUnlock?.Invoke(itemData, m_image.transform.position, SetInteractable);
            }
            else
            {
                //Select Piece
                itemData.MovePiece?.Invoke(itemData, m_image.transform.position, SetInteractable);
            }
        }
    }

    public void OnPointerUp()
    {
        //m_dragging = false;
		//m_scrolling = false;
    }
}
