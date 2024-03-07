using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

partial class LevelEditor
{
	public record BoardInfo(
		int NumberOfTileTypes,
		int DifficultType,
		int MissionCount,
		int GoldTileIcon,
		IReadOnlyList<LayerInfo> Layers
	)
	{
		public static IReadOnlyList<BoardInfo> Create(LevelData data, float size)
		{
			int colorIndex = 0;
			return data.Boards.Select(
				board => {
					return new BoardInfo(
						NumberOfTileTypes: board.NumberOfTileTypes,
						DifficultType: board.Difficult,
						MissionCount: board.MissionTileCount,
						GoldTileIcon: board.GoldTileIcon,
						Layers: board.Layers.Select(
							layer => {
								return new LayerInfo(
									Color: ColorTableUtility.GetColor(colorIndex++),
									layer?.Tiles.Select(tile => new TileInfo(tile.Guid, tile.Position, size, tile.IncludeMission, tile.Blocker))
								);
							}
						).ToArray()
					);
				}
			).ToArray();
		}

		public int TileCountAll => Layers.Sum(layer => layer.TileCount);

        public Dictionary<BlockerType, int> GetBlockerDic()
        {
            Dictionary<BlockerType, int> dic = new Dictionary<BlockerType, int>();
            var includeList = Layers.Select(layer => layer.Tiles.Where(tile => IsIncludeBlockerCount(tile.blockerType)).ToList()).ToList();
            
            includeList.ForEach(tileInfoList => 
                tileInfoList.ForEach(
                    tileInfo => {
                        if(!dic.ContainsKey(tileInfo.blockerType))
                            dic.Add(tileInfo.blockerType, 0);
                        dic[tileInfo.blockerType] += 1;
                    }
                )
            );

            return dic;
        }

        private bool IsIncludeBlockerCount(BlockerType blockerType)
        {
            switch(blockerType)
            {
                case BlockerType.None:
                    return false;
                default:
                    return true;
            }
        }
	}

	public record LayerInfo(Color Color, IReadOnlyList<TileInfo> Tiles)
	{
		public LayerInfo(Color Color, IEnumerable<TileInfo> tiles) 
		: this(Color, Tiles: tiles?.ToArray() ?? Array.Empty<TileInfo>())
		{

		}

		public int TileCount => Tiles.Count();
	}
	
	public record TileInfo(Guid Guid, Vector2 Position, float Size, bool attachedMission, BlockerType blockerType);

	public enum UpdateType
	{
		NONE,
		ALL, // 편집할 레벨 변경 시 모든 데이터를 바꾼다.
		BOARD, // 편집할 보드 변경
		TILE, // 타일 변경
		NUMBER_OF_TILE_TYPES, // 타일 종류 개수 변경
		DIFFICULT, // 난이도 변경
	}

	/// <summary>
	/// 레벨 에디터에서 편집하는 데이터들을 이곳에 보관
	/// </summary>
	public record InternalState
	(
		UpdateType UpdateType,
		int LastLevel,
		int CurrentLevel,
		IReadOnlyList<BoardInfo> Boards,
		string CountryCode,

		bool HardMode = false,
		int BoardIndex = 0,
		int BoardCount = 0,
		int TileCountInBoard = 0,
		int TileCountAll = 0
	)
	{

		public static InternalState Empty = new InternalState(
			UpdateType: UpdateType.NONE,
			LastLevel: 1,
			CurrentLevel: 1,
			BoardIndex: 0,
			BoardCount: 1,
			TileCountInBoard: 0,
			TileCountAll: 0,
			Boards: Array.Empty<BoardInfo>(),
			CountryCode: CountryCodeDataTable.DEFAULT_CODE
		);

		public static InternalState ToInternalInfo(LevelData data, int lastLevel, float size)
		{
			return new InternalState(
				UpdateType: UpdateType.ALL,
				LastLevel: lastLevel,
				CurrentLevel: data.Key,
				BoardIndex: 0,
				BoardCount: data.Boards.Count,
				TileCountInBoard: data[0].Layers.Sum(layer => layer.Tiles.Count()),
				TileCountAll: data.TileCountAll,
				Boards: BoardInfo.Create(data, size),
				HardMode: data.HardMode,
				CountryCode: data.CountryCode
			);
		}
		
		public IReadOnlyList<LayerInfo> CurrentBoard => Boards?.ElementAtOrDefault(BoardIndex)?.Layers ?? Array.Empty<LayerInfo>();

		public CurrentState ToCurrentState()
		{
            return UpdateType switch {
				UpdateType.ALL => new CurrentState.AllUpdated(
					LastLevel: LastLevel,
					CurrentLevel: CurrentLevel,
					BoardCount: BoardCount,
					BoardIndex: BoardIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Boards: Boards,
					HardMode: HardMode,
					CountryCode: CountryCode
				),
				UpdateType.BOARD => new CurrentState.BoardUpdated(
					BoardCount: BoardCount,
					BoardIndex: BoardIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Boards: Boards,
					HardMode: HardMode
				),
				UpdateType.NUMBER_OF_TILE_TYPES => new CurrentState.NumberOfTileTypesUpdated(
                    Boards: Boards, 
                    BoardIndex: BoardIndex,
                    BoardCount: BoardCount,
                    TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll
                ),
				UpdateType.TILE => new CurrentState.TileUpdated(
					BoardIndex: BoardIndex,
					TileCountInBoard: TileCountInBoard,
					TileCountAll: TileCountAll,
					Boards: Boards,
					Layers: Boards?.ElementAtOrDefault(BoardIndex)?.Layers ?? Array.Empty<LayerInfo>()
				),
				UpdateType.DIFFICULT => new CurrentState.DifficultUpdated(Difficult: (DifficultType)Boards[BoardIndex].DifficultType, HardMode: HardMode ),
                _=> new CurrentState.NotUpdated()
			};
		}
	}

	public record CurrentState
	{
		public record NotUpdated : CurrentState;

        public record DifficultUpdated(
			DifficultType Difficult,
			bool HardMode
		) : CurrentState;

        public record AllUpdated(
			int CurrentLevel,
			int LastLevel,
			int BoardIndex,
			int BoardCount,
			int TileCountInBoard,
			int TileCountAll,
			IReadOnlyList<BoardInfo> Boards,
			bool HardMode,
			string CountryCode
		): BoardUpdated(BoardCount, BoardIndex, TileCountInBoard, TileCountAll, Boards, HardMode);

		public record BoardUpdated(
			int BoardCount,
			int BoardIndex,
			int TileCountInBoard,
			int TileCountAll,
			IReadOnlyList<BoardInfo> Boards,
			bool HardMode
		) : BoardState(Boards, BoardIndex);

		public record TileUpdated(
			int BoardIndex,
			int TileCountInBoard,
			int TileCountAll,
			IReadOnlyList<BoardInfo> Boards,
			IReadOnlyList<LayerInfo> Layers
		) : BoardState(Boards, BoardIndex)
		{
			public IReadOnlyList<Color> CurrentColors => Layers?.Select(layer => layer.Color)?.ToArray() ?? Array.Empty<Color>();

			public IReadOnlyList<TileInfo> GetTileInfos(int index)
			{
				if (Layers.TryGetValue(index, out var layer))
				{
					return layer.Tiles;
				}

				return Array.Empty<TileInfo>();
			}
		}

		public record NumberOfTileTypesUpdated(
            int BoardCount, 
            int BoardIndex,
            int TileCountInBoard,
			int TileCountAll,
            IReadOnlyList<BoardInfo> Boards
        ): BoardState(Boards, BoardIndex);

        public record BoardState(
            IReadOnlyList<BoardInfo> Boards, 
            int BoardIndex
        ) : CurrentState
        {
            public BoardInfo CurrentBoard = Boards?.ElementAtOrDefault(BoardIndex) ?? null;
            public IReadOnlyList<LayerInfo> CurrentLayers => CurrentBoard?.Layers ?? Array.Empty<LayerInfo>();
            public int GoldTileCount => CurrentBoard?.MissionCount ?? 0;
            public int NumberOfTileTypesCurrent => CurrentBoard?.NumberOfTileTypes ?? 1;
            public Dictionary<BlockerType, int> BlockerDic => CurrentBoard?.GetBlockerDic() ?? new Dictionary<BlockerType, int>();
        }
	}
}