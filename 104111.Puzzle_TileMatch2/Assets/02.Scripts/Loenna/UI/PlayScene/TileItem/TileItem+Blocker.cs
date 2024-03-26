using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public partial class TileItem
{
    [Header("★ [Reference] Blocker")]
    [SerializeField]	private RectTransform m_blockerRect;
    [SerializeField]	private Image m_blockerImage;
    [SerializeField]	private Image m_blockerImageSub;
    [SerializeField]	private TMP_Text m_blockerText;

    [Header("★ [Reference] Suitcase")]
    [SerializeField]	private RectTransform m_subTileParent;
    [SerializeField]	private TileItem m_subTileItem;
    
#region Blocker State

    private void RefreshBlockerState(BlockerType type, int currentICD)
    {
        if (currentLocation != LocationType.BOARD)
        {
            SetBlockerObject(Vector3.zero, null, activeMain:false);
            return;
        }

        switch(type)
        {
            case BlockerType.Glue_Right:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD));
                break;
            case BlockerType.Bush:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Suitcase:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), GlobalDefine.GetBlockerSubSprite(type, currentICD), currentICD.ToString(), true, true, true);
                RefreshSubTiles();
                break;
            case BlockerType.Jelly:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Chain:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), activeMain:currentICD > 0);
                break;
            case BlockerType.Glue_Left:
            case BlockerType.None:
            default:
                SetBlockerObject(GetBlockerRectPosition(type), null, activeMain:false);
                break;
        }

        CheckBlockerEffect(type, currentICD);
    }

    private Vector3 GetBlockerRectPosition(BlockerType type)
    {
        switch(type)
        {
            case BlockerType.Glue_Right:
                return -Constant.Game.TILE_WIDTH_HALF_POSITION;
        }

        return Vector3.zero;
    }

    private void CheckBlockerEffect(BlockerType type, int newICD)
    {
        bool isDecreased = newICD < oldICD;
        if(!isDecreased)
            return;
        
        switch(type)
        {
            case BlockerType.Bush:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Jelly:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Chain:
                PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Suitcase:
                //PlayBlockerEffect(type, newICD, m_blockerRect.position);
                break;
            case BlockerType.Glue_Left:
            case BlockerType.Glue_Right:
            case BlockerType.None:
            default:
                break;
        }
    }

    private bool IsNeedEffectBeforeMove(BlockerType type)
    {
        switch(type)
        {
            case BlockerType.Glue_Left:
            case BlockerType.Glue_Right:
                return true;
        }

        return false;
    }

    private void SetBlockerObject(Vector3 rectPosition, Sprite mainSprite, Sprite subSprite = null, string text = null, bool activeMain = true, bool activeSub = false, bool activeText = false)
    {
        m_blockerRect.localPosition = rectPosition;
        m_blockerImage.sprite = mainSprite;
        m_blockerImageSub.sprite = subSprite;
        m_blockerText.SetText(text);
        m_blockerImage.gameObject.SetActive(activeMain);
        m_blockerImageSub.gameObject.SetActive(activeSub);
        m_blockerText.gameObject.SetActive(activeText);
    }

    private void PlayBlockerEffect(BlockerType type, int newICD, Vector3 worldPosition)
    {
        globalData.playScene.LoadFX(type, newICD, worldPosition);
    }

#endregion Blocker State


#region Blocker Check

    /// <summary>
    /// 자신의 이동 가능 여부를 체크.
    /// </summary>
    public void MovableCheck()
    {
        bool oldValue = m_movable;

        if (m_interactable)
        {
            m_movable = BlockerMoveCheck(blockerICD);
        }
        else
        {
            m_movable = false;
        }

        if (oldValue != m_movable)
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("Movable Changed : {0} ({1}) : {2}", name, currentLocation, m_movable));
            
            if (currentLocation == LocationType.BOARD)
            {
                m_dimTween?.OnChangeValue(IsReallyMovable(m_movable) ? 0 : 1f, 0f);
            }
            
            AroundMovableCheck();
        }
    }

    /// <summary>
    /// 자신이 Blocker인 경우 인접 타일의 이동 가능 여부를 체크.
    /// </summary>
    private void AroundMovableCheck()
    {
        switch(blockerType)
        {
            case BlockerType.Glue_Left:
                var (_, rightTile) = this.FindRightTile();
                rightTile?.SetDim(IsReallyMovable(m_movable) ? 0 : 1f);
                break;
            case BlockerType.Glue_Right:
                var (_, leftTile) = this.FindLeftTile();
                leftTile?.SetDim(IsReallyMovable(m_movable) ? 0 : 1f);
                break;
        }
    }

    /// <summary>
    /// 자신이 Blocker인 경우 본인의 이동 가능 여부를 체크.
    /// </summary>
    private bool BlockerMoveCheck(int currentICD)
    {
        switch(blockerType)
        {
            case BlockerType.Glue_Left:
                var(existRight, rightTile) = this.FindRightTile();
                return existRight && rightTile.IsInteractable;
            case BlockerType.Glue_Right:
                var(existLeft, leftTile) = this.FindLeftTile();
                return existLeft && leftTile.IsInteractable;
            case BlockerType.Bush:
            case BlockerType.Chain:
            case BlockerType.Jelly:
                return currentICD <= 0;
            case BlockerType.Suitcase:
                return currentICD <= 1;
        }
        return true;
    }

    /// <summary>
    /// 실제 이동 가능 여부를 체크.
    /// </summary>
    /// <param name="tileMoveable"></param>
    /// <returns></returns>
    private bool IsReallyMovable(bool tileMoveable)
    {
        switch(currentLocation, blockerType)
        {
            case (LocationType.BOARD, BlockerType.Glue_Left):
                var(existRight, rightTile) = this.FindRightTile();
                return tileMoveable && existRight && rightTile.IsInteractable && rightTile.IsMovable;
            case (LocationType.BOARD, BlockerType.Glue_Right):
                var(existLeft, leftTile) = this.FindLeftTile();
                return tileMoveable && existLeft && leftTile.IsInteractable && leftTile.IsMovable;
        }

        return tileMoveable;
    }

#endregion Blocker Check


#region Glue FX

    public async UniTask WaitGlueAnimation(bool existPair, TileItem pairTile, Action onFinished)
    {
        if (existPair)
        {
            bool aniFinished = false;

            RefreshBlockerState(blockerType, blockerICD);
            pairTile.RefreshBlockerState(pairTile.blockerType, pairTile.blockerICD);

            SetBasketParent();
            pairTile.SetBasketParent();

            SetDim(0);
            pairTile.SetDim(0);

            SetMoving(true);
            pairTile.SetMoving(true);

            switch(blockerType)
            {
                case BlockerType.Glue_Left:
                    pairTile.m_blockerImage.gameObject.SetActive(false);
                    pairTile.PlayBlockerEffect(pairTile.blockerType, pairTile.blockerICD, pairTile.m_blockerRect.position);
                    await DoGlueMove(transform, pairTile.transform, GlobalDefine.GlueFX_Duration, OnAniFinished);
                    break;
                case BlockerType.Glue_Right:
                    m_blockerImage.gameObject.SetActive(false);
                    PlayBlockerEffect(blockerType, blockerICD, m_blockerRect.position);
                    await DoGlueMove(pairTile.transform, transform, GlobalDefine.GlueFX_Duration, OnAniFinished);
                    break;
                default:
                    aniFinished = true;
                    break;
            }

            await UniTask.WaitUntil(() => aniFinished);

            void OnAniFinished()
            {
                RollBackParent();
                pairTile.RollBackParent();

                aniFinished = true;
            }
        }

        onFinished?.Invoke();
    }

    private UniTask DoGlueMove(Transform left, Transform right, float duration, Action onComplete = null)
    {
        Sequence seqGlue = DOTween.Sequence();

        if(globalData.glueSimple)
        {
            var (leftPathLast, rightPathLast) = GlobalDefine.GluePathLast(left.transform.localPosition, right.transform.localPosition);
            var (leftRotationLast, rightRotationLast) = GlobalDefine.GlueRotationLast();

            seqGlue
                .Append(left.DOLocalMove(leftPathLast, duration))
                .Join(left.DOLocalRotate(leftRotationLast, duration))
                .Join(right.DOLocalMove(rightPathLast, duration))
                .Join(right.DOLocalRotate(rightRotationLast, duration));
        }
        else
        {
            var (leftPath, rightPath) = GlobalDefine.GluePathArray(left.transform.localPosition, right.transform.localPosition);
            var (leftRotation, rightRotation) = GlobalDefine.GlueRotationArray();

            float eDuration = duration * GlobalDefine.GlueFX_Count_Inverse;

            for(int i = 0; i < GlobalDefine.GlueFX_Count; i++)
            {
                seqGlue
                    .Append(left.DOLocalMove(leftPath[i], eDuration))
                    .Join(left.DOLocalRotate(leftRotation[i], eDuration))
                    .Join(right.DOLocalMove(rightPath[i], eDuration))
                    .Join(right.DOLocalRotate(rightRotation[i], eDuration));
            }
        }

        return seqGlue
            .SetEase(globalData.glueEase)
            .SetAutoKill()
            .OnComplete(() => {
                left.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => left.transform.localRotation = Quaternion.identity);
                right.DOLocalRotate(Vector3.zero, Constant.Game.TWEENTIME_TILE_DEFAULT)
                    .OnComplete(() => right.transform.localRotation = Quaternion.identity);
                onComplete?.Invoke();
            })
            .Play()
            .ToUniTask();
    }

#endregion Glue FX


#region Suitcase FX

    private void RefreshSubTiles()
    {
        Debug.Log(CodeManager.GetMethodName());

        ClearSubTiles();
        
        m_subTileItem = globalData.playScene.TileItemPool.Get();
        m_subTileItem.transform.SetParentReset(m_subTileParent);
        m_subTileItem.OnUpdateUI(CreateSubTileItemModel(), true, out _);
    }

    private void ClearSubTiles()
    {
        Debug.Log(CodeManager.GetMethodName());

        GlobalDefine.ClearChild(m_subTileParent);
        m_subTileItem = null;
    }

    private TileItemModel CreateSubTileItemModel()
    {
        Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 checkPosition = (Current.Position / Constant.Game.RESIZE_TILE_RATIOS) - new Vector2(0, Constant.Game.TILE_SIZE_EDITOR);
        Vector2 subTilePosition = new Vector2(0, checkPosition.y);
        
        var overlaps = globalData.playScene.TileItems
            .Where(
                item =>
                {
                    var resizePosition = checkPosition * Constant.Game.RESIZE_TILE_RATIOS;
                    return item.Current.LayerIndex > layerIndex && Vector2.SqrMagnitude(resizePosition - item.Current.Position) < 7700f;
                }
            ).Select(item => {
                    var resizePosition = checkPosition * Constant.Game.RESIZE_TILE_RATIOS;
                    return (item.Current.Guid, true, Vector2.SqrMagnitude(resizePosition - item.Current.Position));
                }
            )
            .ToList();

        int icon = iconList[Mathf.Max(0, blockerICD - 1)];

        return new TileItemModel(
            layerIndex,
            siblingIndex,
            LocationType.BOARD,
            Guid.NewGuid(),
            icon,
            iconList,
            subTilePosition * Constant.Game.RESIZE_TILE_RATIOS,
            BlockerType.Suitcase_Tile,
            0,
            -1,
            overlaps
        );
    }

#endregion Suitcase FX

}
