using System.Collections.Generic;

public abstract record CommandResource
{
	public record PlayScene
	(
		CommandType.PlayScene CommandType, 
		TileItemModel TileItemModel,
		LocationType Location,
        BlockerType BlockerType,
        int BlockerICD,
        bool ForceMove
	) : CommandResource
	{
		public void Deconstruct(out CommandType.PlayScene type, out TileItemModel tileItemModel, out bool forceMove)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
            forceMove = ForceMove;
		}

		public void Deconstruct(out CommandType.PlayScene type, out TileItemModel tileItemModel, out LocationType location, out BlockerType blockerType, out int blockerICD, out bool forceMove)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
			location = Location;
            blockerType = BlockerType;
            blockerICD = BlockerICD;
            forceMove = ForceMove;
		}

		public static PlayScene CreateCommand(CommandType.PlayScene type, TileItemModel tileItemModel, LocationType location, BlockerType blockerType, int blockerICD, bool forceMove)
		{
			return new PlayScene(type, tileItemModel, location, blockerType, blockerICD, forceMove);
		}

		public PlayScene Undo => new PlayScene(CommandType.GetUndoType(), TileItemModel, Location, BlockerType, BlockerICD, ForceMove);
	}
}