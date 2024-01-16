#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;

using NineTap.Common;
using Coffee.UIEffects;

[ResourcePath("UI/Widgets/JigsawPuzzlePiece")]
public class JigsawPuzzlePiece : CachedBehaviour
{
    [SerializeField]
	private Button m_attachButton = null!;
	[SerializeField]
	private Image m_image = null!;
    [SerializeField]
	private Image m_ssuImage = null!;
    [SerializeField]
	private Image m_attachedImage = null!;
    [SerializeField]
	private Image m_attachedSsuImage = null!;
    [SerializeField]
	private GameObject m_touchLock = null!;
    public UIShiny shiny = null!;

    public int Index;
    public bool isAttached = false;

    private PuzzlePieceItemData itemData = null!;
	private bool m_draggable = false;
	private Action<Vector2> m_onCheck = null!;

	public void OnSetup(PuzzlePieceItemData _itemData, bool _isAttached)
	{
        itemData = _itemData;
        Index = itemData.Index;
        isAttached = _isAttached;

		m_image.sprite = itemData.Sprite;
		m_ssuImage.sprite = itemData.Sprite;
        m_attachedImage.sprite = itemData.SpriteAttached;
        m_attachedSsuImage.sprite = itemData.SpriteAttached;

        m_image.rectTransform.SetSize(itemData.Size);
        m_attachedImage.rectTransform.SetSize(itemData.Size);
        m_draggable = false;
        shiny.Stop();

        RefreshAttached();
	}

    public void SetAttached(bool attached)
    {
        isAttached = attached;
        RefreshAttached();
    }

    public void RefreshAttached()
    {
        m_touchLock.SetActive(isAttached);
        m_attachedImage.gameObject.SetActive(isAttached);
    }

    public void OnClick_Unlock()
    {
        if (itemData.IsLocked)
        {
            itemData.OnTryUnlock?.Invoke(this, itemData, m_image.transform.position, SetAttached);
        }
        else
        {
            //Select Piece
            itemData.MovePiece?.Invoke(this, itemData, m_image.transform.position, SetAttached);
        }
    }

    /*public void OnSetup(PuzzlePieceItemData itemData, PointerEventData eventData, Action onCheck)
	{
		m_image.sprite = itemData.Sprite;
		m_image.rectTransform.SetSize(itemData.Size);
		m_draggable = true;
		m_onCheck = pos => onCheck?.Invoke();
		eventData.pointerDrag = CachedGameObject;

		OnDrag(eventData);
	}

    public void OnDrag(PointerEventData eventData)
	{
		if (m_draggable && UIManager.SceneCanvas != null)
		{	
			if (CachedRectTransform == null)
			{
				return;
			}

			float factor = UIManager.SceneCanvas.scaleFactor;
			CachedRectTransform.anchoredPosition += eventData.delta / factor;
			m_onCheck?.Invoke(CachedRectTransform.position);
		}
	}*/

	public void Attached()
	{
		m_draggable = false;
        shiny.Play();
	}
}
