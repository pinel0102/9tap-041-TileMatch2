using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelEditor;

public static partial class TileSearch
{
    private const float TILE_WIDTH_EDITOR = 160f;
    private const float TILE_HEIGHT_EDITOR = 160f;
    private const float TILE_WIDTH_GAME = 88f;
    private const float TILE_HEIGHT_GAME = 92f;

    private static readonly List<Vector2> offset_Editor = new List<Vector2>()
    {
        new Vector2(-TILE_WIDTH_EDITOR, 0), // Left
        new Vector2(TILE_WIDTH_EDITOR, 0), // Right
        new Vector2(0, TILE_HEIGHT_EDITOR), // Top
        new Vector2(0, -TILE_HEIGHT_EDITOR), // Bottom
    };

    private static readonly List<Vector2> offset_Game = new List<Vector2>()
    {
        new Vector2(-TILE_WIDTH_GAME, 0), // Left
        new Vector2(TILE_WIDTH_GAME, 0), // Right
        new Vector2(0, TILE_HEIGHT_GAME), // Top
        new Vector2(0, -TILE_HEIGHT_GAME), // Bottom
    };

#region [Editor] Tile

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

#endregion [Editor] Tile


#region [Editor] TileInfo

    private static List<Vector2> Offset(this TileInfo tile)
    {
        return offset_Editor;
    }

    private static Vector2 OffsetLeft(this TileInfo tile)
    {
        return tile.Offset()[0];
    }

    private static Vector2 OffsetRight(this TileInfo tile)
    {
        return tile.Offset()[1];
    }

    private static Vector2 OffsetTop(this TileInfo tile)
    {
        return tile.Offset()[2];
    }

    private static Vector2 OffsetBottom(this TileInfo tile)
    {
        return tile.Offset()[3];
    }

    private static Vector2 PositionLeft(this TileInfo tile)
    {
        return tile.Position + tile.OffsetLeft();
    }

    private static Vector2 PositionRight(this TileInfo tile)
    {
        return tile.Position + tile.OffsetRight();
    }

    private static Vector2 PositionTop(this TileInfo tile)
    {
        return tile.Position + tile.OffsetTop();
    }

    private static Vector2 PositionBottom(this TileInfo tile)
    {
        return tile.Position + tile.OffsetBottom();
    }

#endregion [Editor] TileInfo


#region [Game] TileItemModel

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

#endregion [Game] TileItemModel


#region [Game] TileItem

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

#endregion [Game] TileItem

}
