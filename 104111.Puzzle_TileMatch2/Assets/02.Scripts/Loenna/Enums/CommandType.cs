public static class CommandType
{
	public enum PlayScene
	{
		NOTHING,
		MOVE_TILE_IN_BOARD_TO_BASKET,
		MOVE_TILE_IN_STASH_TO_BASKET,
        CHANGE_BLOCKER_ICD,
        DO_NOT_MOVE_DISAPPEAR,
        ROLLBACK_TILE_TO_BOARD,
		ROLLBACK_TILE_TO_STASH,
        ROLLBACK_CHANGE_BLOCKER_ICD,
        DO_NOT_MOVE_APPEAR,
	}
}

public static class PlaySceneCommandTypeExtensions
{
	public static CommandType.PlayScene GetUndoType(this CommandType.PlayScene type)
	{
		return type switch {
			CommandType.PlayScene.MOVE_TILE_IN_BOARD_TO_BASKET => CommandType.PlayScene.ROLLBACK_TILE_TO_BOARD,
			CommandType.PlayScene.MOVE_TILE_IN_STASH_TO_BASKET => CommandType.PlayScene.ROLLBACK_TILE_TO_STASH,
            CommandType.PlayScene.CHANGE_BLOCKER_ICD => CommandType.PlayScene.ROLLBACK_CHANGE_BLOCKER_ICD,
            CommandType.PlayScene.DO_NOT_MOVE_DISAPPEAR => CommandType.PlayScene.DO_NOT_MOVE_APPEAR,
			_ => CommandType.PlayScene.NOTHING		
		};
	}

	public static LocationType GetLocationType(this CommandType.PlayScene type)
	{
		return type switch {
			CommandType.PlayScene.MOVE_TILE_IN_BOARD_TO_BASKET or CommandType.PlayScene.MOVE_TILE_IN_STASH_TO_BASKET => LocationType.BASKET,
            CommandType.PlayScene.CHANGE_BLOCKER_ICD => LocationType.BOARD,
            CommandType.PlayScene.DO_NOT_MOVE_DISAPPEAR => LocationType.POOL,
			CommandType.PlayScene.ROLLBACK_TILE_TO_BOARD => LocationType.BOARD,
			CommandType.PlayScene.ROLLBACK_TILE_TO_STASH => LocationType.STASH,
            CommandType.PlayScene.ROLLBACK_CHANGE_BLOCKER_ICD => LocationType.BOARD,
            CommandType.PlayScene.DO_NOT_MOVE_APPEAR => LocationType.BOARD,
			_ => LocationType.POOL		
		};
	}
}
