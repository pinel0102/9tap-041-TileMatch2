using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

partial class LevelEditor
{
	public record LayerInfo(Color Color, IReadOnlyList<TileInfo> Tiles)
	{
		public LayerInfo(IEnumerable<TileInfo> tiles) 
			: this(Color: UnityEngine.Random.ColorHSV(), Tiles: tiles?.ToArray() ?? Array.Empty<TileInfo>())
		{

		}
	}
	public record TileInfo(Vector2 Position, float Size);

	public enum UpdateType
	{
		NONE,
		ALL, // 편집할 레벨 변경 시 모든 데이터를 바꾼다.
		BOARD, // 편집할 보드 변경
		LAYER, // 편집할 레이어 변경
		TILE, // 타일 변경
		NUMBER_OF_TILE_TYPES, // 타일 종류 개수 변경
	}

	/// <summary>
	/// 레벨 에디터에서 편집하는 데이터들을 이곳에 보관
	/// </summary>
	public record InternalState
	(
		UpdateType UpdateType,
		int LastLevel,
		int CurrentLevel,
		int NumberOfTileTypes,
		List<LayerInfo> Layers,
		int BoardIndex = 0,
		int BoardCount = 0,
		int LayerIndex = 0,
		int TileCountInBoard = 0,
		int TileCountAll = 0
	)
	{

		public static InternalState Empty = new InternalState(
			UpdateType: UpdateType.NONE,
			LastLevel: 1,
			CurrentLevel: 1,
			NumberOfTileTypes: 1,
			BoardIndex: 0,
			BoardCount: 1,
			LayerIndex: 0,
			TileCountInBoard: 0,
			TileCountAll: 0,
			Layers: new()
		);

		public static InternalState ToInternalInfo(LevelData data, int lastLevel, float size)
		{
			return new InternalState(
				UpdateType: UpdateType.ALL,
				LastLevel: lastLevel,
				CurrentLevel: data.Level,
				NumberOfTileTypes: data.NumberOfTileTypes,
				BoardIndex: 0,
				LayerIndex: 0,
				BoardCount: data.Boards.Count,
				TileCountInBoard: data[0].Layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll: data.TileCountAll,
				Layers: data[0]
					.Layers
					.Select(
						layer => {
							return new LayerInfo(
								layer?
								.Tiles?
								.Select(
									tile => new TileInfo(tile.Position, size)
								)
							);
						}
					).ToList()
			);
		}

		public CurrentState ToCurrentState()
		{
			return UpdateType switch {
				UpdateType.ALL => new CurrentState.AllUpdated(
					LastLevel: LastLevel,
					CurrentLevel: CurrentLevel,
					NumberOfTileTypes: NumberOfTileTypes,
					BoardCount: BoardCount,
					BoardIndex: BoardIndex,
					LayerIndex: LayerIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Layers: Layers
				),
				UpdateType.BOARD => new CurrentState.BoardUpdated(
					BoardCount: BoardCount,
					BoardIndex: BoardIndex,
					LayerIndex: LayerIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Layers: Layers
				),
				UpdateType.NUMBER_OF_TILE_TYPES => new CurrentState.NumberOfTileTypesUpdated(
					NumberOfTileTypes: NumberOfTileTypes
				),
				UpdateType.LAYER => new CurrentState.LayerUpdated(
					LayerIndex: LayerIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Layers: Layers
				),
				UpdateType.TILE => new CurrentState.TileUpdated(
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll
				),
				_=> new CurrentState.NotUpdated()
			};
		}
	}

	public record CurrentState
	{
		public record NotUpdated : CurrentState;

		public record AllUpdated(
			int CurrentLevel,
			int LastLevel,
			int NumberOfTileTypes,
			int BoardIndex,
			int BoardCount,
			int LayerIndex,
			int TileCountInBoard,
			int TileCountAll,
			List<LayerInfo> Layers
		): LayerUpdated(LayerIndex, TileCountInBoard, TileCountAll, Layers);

		public record BoardUpdated(
			int BoardCount,
			int BoardIndex,
			int LayerIndex,
			int TileCountInBoard,
			int TileCountAll,
			List<LayerInfo> Layers
		) : LayerUpdated(LayerIndex, TileCountInBoard, TileCountAll, Layers);

		public record LayerUpdated(
			int LayerIndex,
			int TileCountInBoard,
			int TileCountAll,
			List<LayerInfo> Layers
		): CurrentState
		{
			public Color[] LayerColors = Layers.Select(layer => layer.Color)?.ToArray() ?? Array.Empty<Color>();
			public IReadOnlyList<TileInfo> GetTileInfos(int index)
			{
				if (Layers.TryGetValue(index, out var layer))
				{
					return layer.Tiles;
				}

				return Array.Empty<TileInfo>();
			}
		}

		public record TileUpdated(
			int TileCountInBoard,
			int TileCountAll
		) : CurrentState;

		public record NumberOfTileTypesUpdated(int NumberOfTileTypes): CurrentState;
	}
}