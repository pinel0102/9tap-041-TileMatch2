public abstract record CommandResource
{
	public record PlayScene
	(
		CommandType.PlayScene CommandType, 
		TileItemModel TileItemModel,
		LocationType Location
	) : CommandResource
	{
		public void Deconstruct(out CommandType.PlayScene type, out TileItemModel tileItemModel)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
		}

		public void Deconstruct(out CommandType.PlayScene type,out TileItemModel tileItemModel, out LocationType location)
		{
			type = CommandType;
			tileItemModel = TileItemModel;
			location = Location;
		}

		public static PlayScene CreateCommand(CommandType.PlayScene type, TileItemModel tileItemModel, LocationType location)
		{
			return new PlayScene(type, tileItemModel, location);
		}

		public PlayScene Undo => new PlayScene(CommandType.GetUndoType(), TileItemModel, Location);
	}
}