using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class TileSearch
{
    private const float TILE_WIDTH_EDITOR = 160f;
    private const float TILE_HEIGHT_EDITOR = 160f;
    private const float TILE_WIDTH_GAME = 88f;
    private const float TILE_HEIGHT_GAME = 92f;

    private static readonly List<Vector2> offset_Editor = new List<Vector2>()
    {
        new Vector2(0, -TILE_HEIGHT_EDITOR), // Left
        new Vector2(0, TILE_HEIGHT_EDITOR), // Right
        new Vector2(TILE_WIDTH_EDITOR, 0), // Top
        new Vector2(-TILE_WIDTH_EDITOR, 0) // Bottom
    };

    private static readonly List<Vector2> offset_Game = new List<Vector2>()
    {
        new Vector2(0, -TILE_HEIGHT_GAME), // Left
        new Vector2(0, TILE_HEIGHT_GAME), // Right
        new Vector2(TILE_WIDTH_GAME, 0), // Top
        new Vector2(-TILE_WIDTH_GAME, 0) // Bottom
    };

#region Tile

    private static List<Vector2> Offset(this Tile tile)
    {
        return offset_Editor;
    }

    private static Vector2 OffsetLeft(this Tile tile)
    {
        return tile.Offset()[0];
    }

    private static Vector2 OffsetRight(this Tile tile)
    {
        return tile.Offset()[1];
    }

    private static Vector2 OffsetTop(this Tile tile)
    {
        return tile.Offset()[2];
    }

    private static Vector2 OffsetBottom(this Tile tile)
    {
        return tile.Offset()[3];
    }

    private static Vector2 PositionLeft(this Tile tile)
    {
        return tile.Position + tile.OffsetLeft();
    }

    private static Vector2 PositionRight(this Tile tile)
    {
        return tile.Position + tile.OffsetRight();
    }

    private static Vector2 PositionTop(this Tile tile)
    {
        return tile.Position + tile.OffsetTop();
    }

    private static Vector2 PositionBottom(this Tile tile)
    {
        return tile.Position + tile.OffsetBottom();
    }

#endregion Tile


#region TileItemModel

    private static List<Vector2> Offset(this TileItemModel tile)
    {
        return offset_Game;
    }

    private static Vector2 OffsetLeft(this TileItemModel tile)
    {
        return tile.Offset()[0];
    }

    private static Vector2 OffsetRight(this TileItemModel tile)
    {
        return tile.Offset()[1];
    }

    private static Vector2 OffsetTop(this TileItemModel tile)
    {
        return tile.Offset()[2];
    }

    private static Vector2 OffsetBottom(this TileItemModel tile)
    {
        return tile.Offset()[3];
    }

    private static Vector2 PositionLeft(this TileItemModel tile)
    {
        return tile.Position + tile.OffsetLeft();
    }

    private static Vector2 PositionRight(this TileItemModel tile)
    {
        return tile.Position + tile.OffsetRight();
    }

    private static Vector2 PositionTop(this TileItemModel tile)
    {
        return tile.Position + tile.OffsetTop();
    }

    private static Vector2 PositionBottom(this TileItemModel tile)
    {
        return tile.Position + tile.OffsetBottom();
    }

#endregion TileItemModel


#region TileItem

    private static List<Vector2> Offset(this TileItem tile)
    {
        return tile.Current.Offset();
    }

    private static Vector2 OffsetLeft(this TileItem tile)
    {
        return tile.Current.OffsetLeft();
    }

    private static Vector2 OffsetRight(this TileItem tile)
    {
        return tile.Current.OffsetRight();
    }

    private static Vector2 OffsetTop(this TileItem tile)
    {
        return tile.Current.OffsetTop();
    }

    private static Vector2 OffsetBottom(this TileItem tile)
    {
        return tile.Current.OffsetBottom();
    }

    private static Vector2 PositionLeft(this TileItem tile)
    {
        return tile.Current.PositionLeft();
    }

    private static Vector2 PositionRight(this TileItem tile)
    {
        return tile.Current.PositionRight();
    }

    private static Vector2 PositionTop(this TileItem tile)
    {
        return tile.Current.PositionTop();
    }

    private static Vector2 PositionBottom(this TileItem tile)
    {
        return tile.Current.PositionBottom();
    }

#endregion TileItem

}
