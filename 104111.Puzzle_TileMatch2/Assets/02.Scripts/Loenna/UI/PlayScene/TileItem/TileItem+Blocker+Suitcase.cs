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
    [SerializeField]	private RectTransform m_subTileParent;
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
        //GlobalDefine.ClearChild(m_subTileParent);
        m_parentTile = null;
        m_childTiles.Clear();
    }

    private void RefreshSuitcaseState()
    {
        if(Current.Location == LocationType.BOARD)
        {
            if(blockerICD < m_parentTile.blockerICD)
            {
                HideSuitcaseTile();
            }
            else
            {
                ShowSuitcaseTile();
            }
        }
        else
        {
            isActivatedSuitcaseTile = false;
        }
    }

    private void HideSuitcaseTile()
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);

        if(isActivatedSuitcaseTile)
        {
            isActivatedSuitcaseTile = false;
            PlaySuitcaseHideAnimation();
        }
    }

    private void ShowSuitcaseTile()
    {
        //Debug.Log(CodeManager.GetMethodName() + tileName);

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_SHOW_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);

        if(!isActivatedSuitcaseTile)
        {
            isActivatedSuitcaseTile = true;
            PlaySuitcaseShowAnimation();
        }
    }

    private void PlaySuitcaseHideAnimation()
    {
        Debug.Log(CodeManager.GetMethodName() + tileName);
        
        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);
        CachedRectTransform.SetLocalPosition(childTilePosition);
    }

    private void PlaySuitcaseShowAnimation()
    {
        Debug.Log(CodeManager.GetMethodName() + tileName);
        //

        Vector2 childTilePosition = Current.Position + Constant.Game.SUITCASE_TILE_SHOW_POSITION;
        m_originWorldPosition = _parentLayer.TransformPoint(childTilePosition);
        CachedRectTransform.SetLocalPosition(childTilePosition);
    }

#endregion Suitcase FX

}