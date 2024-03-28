using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class TileItem
{
    [Header("★ [Reference] Blocker")]
    [SerializeField]	private RectTransform m_blockerRect;
    [SerializeField]	private Image m_blockerImage;
    [SerializeField]	private Image m_blockerImageSub;
    [SerializeField]	private TMP_Text m_blockerText;
    
#region Blocker State

    public void RefreshBlockerState(BlockerType type, int currentICD)
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
                m_view.SetLocalScale(0);
                break;
            case BlockerType.Suitcase_Tile:
                SetBlockerObject(GetBlockerRectPosition(type), GlobalDefine.GetBlockerSprite(type, currentICD), GlobalDefine.GetBlockerSubSprite(type, currentICD), currentICD.ToString(), 
                    CanShowSuitcase(), 
                    CanShowSuitcase() && !CanOpenSuitcase(), 
                    CanShowSuitcase());
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
                return Constant.Game.TILE_WIDTH_HALF_POSITION;
            case BlockerType.Suitcase_Tile:
                return isActivatedSuitcaseTile ? Constant.Game.SUITCASE_HEIGHT_POSITION : Vector3.zero;
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

}
