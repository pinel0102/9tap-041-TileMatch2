using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

partial class LevelEditor
{
	public enum UpdateType
	{
		NONE,
		ALL, // 편집할 레벨 변경 시 모든 데이터를 바꾼다.
		NUMBER_OF_TILE_TYPES, // 타일 종류 개수 변경
		LAYER, // 편집할 레이어 변경
		BOARD, // 편집할 보드 변경
	}

	public record InternalState
	(
		UpdateType UpdateType,
		int Level,
		int NumberOfTileTypes,
		IReadOnlyList<(Vector2 position, float size)> Tiles,
		int BoardIndex = 0,
		int LayerIndex = 0,
		int TileCountInLayer = 0, // Todo
		int TileCountAll = 0 // Todo
	)
	{

		public static InternalState Empty = new InternalState(
			UpdateType: UpdateType.NONE,
			Level: 1,
			NumberOfTileTypes: 1,
			BoardIndex: 0,
			LayerIndex: 0,
			TileCountInLayer: 0,
			TileCountAll: 0,
			Tiles: Array.Empty<(Vector2, float)>()
		);

		public static InternalState ToInternalInfo(LevelData data, float size)
		{
			return new InternalState(
				UpdateType: UpdateType.ALL,
				Level: data.Level,
				NumberOfTileTypes: data.NumberOfTileTypes,
				Tiles: data.GetLayer(0, 0).Tiles.Select(tile => (tile.Position, size)).ToArray()
			);
		}

		public CurrentState ToCurrentState()
		{
			return UpdateType switch {
				UpdateType.ALL => new CurrentState.AllUpdated(
					Level: Level,
					NumberOfTileTypes: NumberOfTileTypes,
					BoardIndex: BoardIndex,
					LayerIndex: LayerIndex,
					TileCountInLayer: TileCountInLayer,
					TileCountAll: TileCountAll,
					Tiles: Tiles
				),
				UpdateType.NUMBER_OF_TILE_TYPES => new CurrentState.NumberOfTileTypesUpdated(
					NumberOfTileTypes: NumberOfTileTypes
				),
				_=> new CurrentState.NotUpdated()
			};
		}
	}

	public abstract record CurrentState
	{
		public record NotUpdated : CurrentState;
		public record AllUpdated(
			int Level,
			int NumberOfTileTypes,
			int BoardIndex,
			int LayerIndex,
			int TileCountInLayer,
			int TileCountAll,
			IReadOnlyList<(Vector2 position, float size)> Tiles
		): CurrentState;
		public record NumberOfTileTypesUpdated(int NumberOfTileTypes): CurrentState;
	}
}