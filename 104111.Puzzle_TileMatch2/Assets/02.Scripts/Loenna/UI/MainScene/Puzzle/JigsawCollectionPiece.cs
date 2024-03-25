using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NineTap.Common;

[ResourcePath("UI/Widgets/JigsawCollectionPiece")]
public class JigsawCollectionPiece : CachedBehaviour
{
    [SerializeField] private Image pieceImage;
    [SerializeField] private Image filterImage;

    private bool m_isAttached;
    public bool IsAttached => m_isAttached;

    public void OnSetup(int index, Sprite sprite, Sprite filter, bool _isAttached, Vector2 position)
    {
        CachedGameObject.name = $"piece[{index}]";
        CachedRectTransform.anchoredPosition = position;
        pieceImage.sprite = sprite;
        filterImage.sprite = filter;
        pieceImage.SetNativeSize();
        RefreshAttached(_isAttached);
    }

    public void RefreshAttached(bool _isAttached)
    {
        m_isAttached = _isAttached;
        CachedGameObject.SetActive(m_isAttached);
    }
}
