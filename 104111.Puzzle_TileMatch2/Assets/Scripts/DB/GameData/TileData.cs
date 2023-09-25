public record TileData
(
	int Index,
	string Name,
	string Path,
	int MinimumLevel,
	string CountryCode
) : TableRowData<int>(Index);