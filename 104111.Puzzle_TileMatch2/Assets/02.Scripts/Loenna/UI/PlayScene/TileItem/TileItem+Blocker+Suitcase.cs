using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public partial class TileItem
{
    [Header("★ [Reference] Suitcase")]
    public bool isActivatedSuitcaseTile;
    public TileItem m_parentTile = null;
    public List<TileItem> m_childTiles = new List<TileItem>();

#region Suitcase FX

    private void SetParentTile(TileItem parentTile)
    {
        //Debug.Log(CodeManager.GetMethodName());
        m_parentTile = parentTile;
    }

    private void RefreshSubTiles(Func<TileItem, bool> searchPattern)
    {
        //Debug.Log(CodeManager.GetMethodName());

        ClearSubTiles();

        var(existChild, childTiles) = this.FindBottomTileList();
        if(existChild)
        {
            m_childTiles.AddRange(
                childTiles.Where(searchPattern)
                        .Select(tile => {
                            tile.ClearSubTiles();
                            tile.SetParentTile(this);
                            return tile;
                        })
                        .OrderBy(item => item.blockerICD)
            );
        }
    }

    private void ClearSubTiles()
    {
        m_parentTile = null;
        m_childTiles.Clear();
    }

    public void RefreshSuitcaseState(int parentICD, bool overlapped, bool skipAnimation = false)
    {
        if(Current.Location == LocationType.BOARD)
        {
            if(blockerICD >= parentICD && !overlapped && CanOpenSuitcase())
            {
                ShowSuitcaseTile(skipAnimation);
            }
            else
            {
                HideSuitcaseTile(skipAnimation);
            }
        }
        else
        {
            isActivatedSuitcaseTile = false;
        }
    }

    private void HideSuitcaseTile(bool skipAnimation = false)
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);
        m_blockerRect.localPosition = Vector2.zero;

        if(isActivatedSuitcaseTile)
        {
            isActivatedSuitcaseTile = false;
            PlaySuitcaseHideAnimation(skipAnimation);
        }
    }

    private void ShowSuitcaseTile(bool skipAnimation = false)
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_SHOW_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);
        m_blockerRect.localPosition = Constant.Game.SUITCASE_HEIGHT_POSITION;

        if(!isActivatedSuitcaseTile)
        {
            isActivatedSuitcaseTile = true;
            PlaySuitcaseShowAnimation(skipAnimation);
        }
    }

    private void PlaySuitcaseHideAnimation(bool skipAnimation = false)
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);

        if(skipAnimation)
        {
            CachedRectTransform.SetLocalPosition(childTilePosition);
        }
        else
        {
            CachedRectTransform.SetLocalPosition(childTilePosition);
        }
    }

    private void PlaySuitcaseShowAnimation(bool skipAnimation = false)
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_SHOW_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);
        
        if(skipAnimation)
        {
            CachedRectTransform.SetLocalPosition(childTilePosition);
        }
        else
        {
            CachedRectTransform.SetLocalPosition(Current.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION);

            m_SuitcaseTween?.OnChangeValue(childTilePosition, GlobalDefine.SuitcaseFX_Duration);
            PlayBlockerEffect(blockerType, blockerICD, CachedRectTransform.position);
        }
    }

    private bool CanShowSuitcase()
    {
        return !isMoving && !IsUndoMoving;
    }

    public bool CanOpenSuitcase()
    {
        var(existTopTile, topTile) = this.FindTopTile();
        if(existTopTile)
        {
            return !topTile.Current?.Overlapped ?? false;
        }
        return !m_parentTile?.Current?.Overlapped ?? false;
    }

#endregion Suitcase FX

}