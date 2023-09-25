public static class CommandType
{
	public enum PlayScene
	{
		NOTHING,
		MOVE_TILE_IN_BOARD_TO_BASKET,
		MOVE_TILE_IN_STASH_TO_BASKET,
		ROLLBACK_TILE_TO_BOARD,
		ROLLBACK_TILE_TO_STASH,
	}
}

public static class PlaySceneCommandTypeExtensions
{
	public static CommandType.PlayScene GetUndoType(this CommandType.PlayScene type)
	{
		return type switch {
			CommandType.PlayScene.MOVE_TILE_IN_BOARD_TO_BASKET => CommandType.PlayScene.ROLLBACK_TILE_TO_BOARD,
			CommandType.PlayScene.MOVE_TILE_IN_STASH_TO_BASKET => CommandType.PlayScene.ROLLBACK_TILE_TO_STASH,
			_ => CommandType.PlayScene.NOTHING		
		};
	}

	public static LocationType GetLocationType(this CommandType.PlayScene type)
	{
		return type switch {
			CommandType.PlayScene.MOVE_TILE_IN_BOARD_TO_BASKET or CommandType.PlayScene.MOVE_TILE_IN_STASH_TO_BASKET => LocationType.BASKET,
			CommandType.PlayScene.ROLLBACK_TILE_TO_BOARD => LocationType.BOARD,
			CommandType.PlayScene.ROLLBACK_TILE_TO_STASH => LocationType.STASH,
			_ => LocationType.POOL		
		};
	}
}
