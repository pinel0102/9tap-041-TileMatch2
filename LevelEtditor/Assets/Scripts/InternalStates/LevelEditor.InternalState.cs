using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

partial class LevelEditor
{
	public record TileInfo(Vector2 Position, float Size);

	public enum UpdateType
	{
		NONE,
		ALL, // 편집할 레벨 변경 시 모든 데이터를 바꾼다.
		NUMBER_OF_TILE_TYPES, // 타일 종류 개수 변경
		LAYER, // 편집할 레이어 변경
		BOARD, // 편집할 보드 변경
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
		List<IReadOnlyList<TileInfo>> Layers,
		int BoardIndex = 0,
		int LayerIndex = 0,
		int TileCountInLayer = 0, // Todo
		int TileCountAll = 0 // Todo
	)
	{

		public static InternalState Empty = new InternalState(
			UpdateType: UpdateType.NONE,
			LastLevel: 1,
			CurrentLevel: 1,
			NumberOfTileTypes: 1,
			BoardIndex: 0,
			LayerIndex: 0,
			TileCountInLayer: 0,
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
				Layers: data[0]
					.Layers
					.Select(
						layer => {
							return layer?
							.Tiles?
							.Select(
								tile => new TileInfo(tile.Position, size)
							).AsReadOnlyList() ?? Array.Empty<TileInfo>();
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
					BoardIndex: BoardIndex,
					LayerIndex: LayerIndex,
					TileCountInLayer: TileCountInLayer,
					TileCountAll: TileCountAll,
					Layers: Layers
				),
				UpdateType.NUMBER_OF_TILE_TYPES => new CurrentState.NumberOfTileTypesUpdated(
					NumberOfTileTypes: NumberOfTileTypes
				),
				UpdateType.LAYER => new CurrentState.LayerUpdated(
					LayerIndex: LayerIndex,
					TileCountInLayer: TileCountInLayer,
					TileCountAll: TileCountAll,
					Layers: Layers
				),
				_=> new CurrentState.NotUpdated()
			};
		}
	}

	public abstract record CurrentState
	{
		public record NotUpdated : CurrentState;
		public record AllUpdated(
			int CurrentLevel,
			int LastLevel,
			int NumberOfTileTypes,
			int BoardIndex,
			int LayerIndex,
			int TileCountInLayer,
			int TileCountAll,
			List<IReadOnlyList<TileInfo>> Layers
		): CurrentState;
		public record NumberOfTileTypesUpdated(int NumberOfTileTypes): CurrentState;
		public record LayerUpdated(
			int LayerIndex,
			int TileCountInLayer,
			int TileCountAll,
			List<IReadOnlyList<TileInfo>> Layers
		): CurrentState;
	}
}