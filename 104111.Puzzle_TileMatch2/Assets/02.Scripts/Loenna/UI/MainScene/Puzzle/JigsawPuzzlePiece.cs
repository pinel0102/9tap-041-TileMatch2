#nullable enable

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using NineTap.Common;
using Coffee.UIEffects;
using DG.Tweening;
using System.Linq;

[ResourcePath("UI/Widgets/JigsawPuzzlePiece")]
public class JigsawPuzzlePiece : CachedBehaviour
{
    [SerializeField]	private Button m_attachButton = null!;
	[SerializeField]	private Image m_image = null!;
    [SerializeField]	private Image m_ssuImage = null!;
    [SerializeField]	private Image m_filter = null!;
    [SerializeField]	private GameObject m_blur = null!;
    [SerializeField]    private Outline m_outline = null!;
    [SerializeField]    private UIShiny m_shiny = null!;
    [SerializeField]    private List<PuzzleCurveType> m_puzzleCurveTypes = new List<PuzzleCurveType>();
    private Action<JigsawPuzzlePiece, Action> OnTryUnlock = null!;

    public int Index;
    public bool Placed = false;
    
    public void OnSetup(PuzzlePieceItemData _itemData, bool _placed)
	{
        Index = _itemData.Index;
        Placed = _placed;
        m_puzzleCurveTypes = _itemData.PuzzleCurveTypes.ToList();
        OnTryUnlock = _itemData.OnTryUnlock;
		m_image.sprite = m_ssuImage.sprite = _itemData.Sprite;
        m_filter.sprite = _itemData.Filter;
        m_image.rectTransform.SetSize(_itemData.Size);
        m_shiny.Stop();
        
        RefreshState();
	}

    public void RefreshState()
    {
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0:00} : {1}", Index, Placed));
        m_image.transform.SetLocalScale(1f);
        m_outline.enabled = Placed;
        m_filter.gameObject.SetActive(Placed);
        m_blur.SetActive(!Placed);
    }

    public void Attached()
	{
        //Debug.Log(CodeManager.GetMethodName() + Index);
        Placed = true;
        m_outline.enabled = Placed;
        m_filter.gameObject.SetActive(Placed);
        m_blur.SetActive(!Placed);
	}

    public void PlaceEffect(Action onComplete)
    {
        m_image.transform.SetLocalScale(0.5f);
        m_image.gameObject.SetActive(true);
        m_image.transform.DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                onComplete?.Invoke();
            });
    }

    public void ShinyEffect()
    {
        m_shiny.Play();
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

    public void OnClick_Unlock_MustCallback(Action onComplete)
    {
        if(Placed) 
        {
            onComplete.Invoke();
            return;
        }

        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", Index, Placed));
        OnTryUnlock?.Invoke(this, onComplete);
    }
}
