#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System;
using NineTap.Common;
using Coffee.UIEffects;
using DG.Tweening;

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
    private UIShiny shiny = null!;
    //[SerializeField]
	//private GameObject m_placeEffect = null!;
    private Action<JigsawPuzzlePiece, Action> OnTryUnlock = null!;

    public int Index;
    public bool Placed = false;
    
    public void OnSetup(PuzzlePieceItemData _itemData, bool _placed)
	{
        Index = _itemData.Index;
        Placed = _placed;

        OnTryUnlock = _itemData.OnTryUnlock;

		m_image.sprite = _itemData.Sprite;
		m_ssuImage.sprite = _itemData.Sprite;
        m_attachedImage.sprite = _itemData.SpriteAttached;
        m_attachedSsuImage.sprite = _itemData.SpriteAttached;

        m_image.rectTransform.SetSize(_itemData.Size);
        m_attachedImage.rectTransform.SetSize(_itemData.Size);

        shiny.Stop();
        //m_placeEffect.SetActive(false);

        RefreshState();
	}

    public void RefreshState()
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0:00} : {1}", Index, Placed));
        m_attachedImage.transform.SetLocalScale(1f);
        m_attachedImage.gameObject.SetActive(Placed);
    }

    public void Attached()
	{
        //Debug.Log(CodeManager.GetMethodName() + Index);
        Placed = true;
	}

    public void PlaceEffect(Action onComplete)
    {
        //m_placeEffect.SetActive(true);

        m_attachedImage.transform.SetLocalScale(0.5f);
        m_attachedImage.gameObject.SetActive(true);
        m_attachedImage.transform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                onComplete?.Invoke();
            });
    }

    public void ShinyEffect()
    {
        shiny.Play();
    }

    public void OnClick_Unlock()
    {
        OnClick_Unlock(onComplete: () => {});
    }

    public void OnClick_Unlock(Action onComplete)
    {
        if(Placed) return;

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", Index, Placed));
        OnTryUnlock?.Invoke(this, onComplete);
    }
}
