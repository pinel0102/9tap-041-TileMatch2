using System.Collections.Generic;

public abstract record CommandResource
{
	public record PlayScene
	(
		CommandType.PlayScene CommandType, 
		TileItemModel TileItemModel,
		LocationType Location,
        BlockerType BlockerType,
        int BlockerICD
	) : CommandResource
	{
		public void Deconstruct(out CommandType.PlayScene type, out TileItemModel tileItemModel)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
		}

		public void Deconstruct(out CommandType.PlayScene type, out TileItemModel tileItemModel, out LocationType location, out BlockerType blockerType, out int blockerICD)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
			location = Location;
            blockerType = BlockerType;
            blockerICD = BlockerICD;
		}

		public static PlayScene CreateCommand(CommandType.PlayScene type, TileItemModel tileItemModel, LocationType location, BlockerType blockerType, int blockerICD)
		{
			return new PlayScene(type, tileItemModel, location, blockerType, blockerICD);
		}

		public PlayScene Undo => new PlayScene(CommandType.GetUndoType(), TileItemModel, Location, BlockerType, BlockerICD);
	}
}