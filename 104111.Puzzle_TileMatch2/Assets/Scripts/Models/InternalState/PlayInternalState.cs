#nullable enable

using System;
using System.Collections.Generic;

partial class GameManager
{
	public record InternalState (
		InternalState.Type StateType,
		int Level,
		bool HardMode,
		int MissionTileCount,
		Queue<BoardItemModel> StandByBoards,
		int BoardCount,
		int CurrentBoardIndex,
		TileItemModel[] SelectedTiles,
		BoardItemModel CurrentBoard,
		bool HasNext,
		List<Guid> Stash,
		List<(Guid Guid, int Icon)> Basket
	)
	{
		public enum Type
		{
			NotUpdated,
			Initialized,
			CurrentUpdated,
			BoardChanged,
			Finished,
		}

		public static InternalState Empty = new InternalState(
			StateType: Type.NotUpdated,
			Level: 0,
			MissionTileCount: 0,
			HardMode: false,
			StandByBoards: new(),
			Stash: new(),
			Basket: new(),
			BoardCount: 0,
			SelectedTiles: Array.Empty<TileItemModel>(),
			CurrentBoardIndex: -1,
			CurrentBoard: BoardItemModel.Empty,
			HasNext: false
		);

		public BoardItemModel? Dequeue()
		{
			return StandByBoards.Count > 0? StandByBoards.Dequeue() : null;
		}

		public CurrentPlayState ToCurrentState()
		{
			return StateType switch {
				Type.Initialized => new CurrentPlayState.Initialized(Level, HardMode, MissionTileCount, BoardCount, CurrentBoardIndex, Array.Empty<TileItemModel>(), CurrentBoard, HasNext),
				Type.BoardChanged => new CurrentPlayState.BoardChanged(CurrentBoardIndex, CurrentBoard, HasNext),
				Type.CurrentUpdated => new CurrentPlayState.CurrentUpdated(CurrentBoard, Basket, Stash, SelectedTiles),
				Type.Finished => new CurrentPlayState.Finished(Result: Basket.Count <= 0? CurrentPlayState.Finished.State.CLEAR : CurrentPlayState.Finished.State.OVER),
				_ => new CurrentPlayState.NotUpdated()
			};
		}

		public int RemainedTileCount => CurrentBoard?.TileCount ?? 0;
	}

}

public abstract record CurrentPlayState
{
	public record NotUpdated : CurrentPlayState;

	public record Initialized
	(
		int Level,
		bool HardMode,
		int MissionTileCount,
		int BoardCount,
		int CurrentBoardIndex,
		TileItemModel[] SelectedTiles,
		BoardItemModel CurrentBoard,
		bool HasNext
	) : CurrentPlayState;

	public record CurrentUpdated(BoardItemModel CurrentBoard, List<(Guid Guid, int Icon)> Basket, List<Guid> Stash, TileItemModel[] SelectedTiles) : CurrentPlayState
	{
	}

	public record BoardChanged
	(
		int CurrentBoardIndex,
		BoardItemModel CurrentBoard, 
		bool HasNext
	) : CurrentPlayState;

	public record Finished
	(
		Finished.State Result
	) : CurrentPlayState
	{
		public enum State
		{
			CLEAR,
			OVER
		}
	}
}