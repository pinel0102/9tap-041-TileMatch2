using UnityEngine;

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using NineTap.Common;

[Serializable]
public record User
(
	// 재화
	long Coin, // 보유 코인
	int Life, // 보유 하트
	int Puzzle, // 보유 미션조각

	long ExpiredLifeBoosterTime, // 하트 부스터 끝나는 시간
	long EndChargeLifeTime, // 하트가 모두 충전되는 시간

	// 게임 현황
	int Level, // 플레이할 레벨
	int CurrentPlayingPuzzleIndex, // 플레이 중인 퍼즐
	List<int> ClearedPuzzleCollection, // 완료한 퍼즐
	Dictionary<int, uint> PlayingPuzzleCollection, // 진행중인 퍼즐
	Dictionary<int, uint> UnlockedPuzzlePieceDic, // 별조각으로 언락한 조각들
	Dictionary<SkillItemType, int> OwnSkillItems, // 가지고 있는 스킬 아이템

	Dictionary<SettingsType, bool> Settings
)
{	
	public static User NewUser = new User (
		Coin: 0,
		Life: Constant.User.MAX_LIFE_COUNT,
		Puzzle: 0,
		ExpiredLifeBoosterTime: 0L,
		EndChargeLifeTime: 0L,
		Level: 1,
		ClearedPuzzleCollection: new(),
		CurrentPlayingPuzzleIndex: 1001, //퍼즐 하나는 무조건 언락되어 있는 상태
		UnlockedPuzzlePieceDic: new Dictionary<int, uint>(),
		PlayingPuzzleCollection: new Dictionary<int, uint>{{1001, 0}}, //퍼즐 하나는 무조건 언락되어 있는 상태
		OwnSkillItems: new Dictionary<SkillItemType, int>{
			{SkillItemType.Stash, 1},
			{SkillItemType.Undo, 1},
			{SkillItemType.Shuffle, 1}
		},
		Settings: new Dictionary<SettingsType, bool>{
			{SettingsType.Fx, true},
			{SettingsType.Bgm, true},
			{SettingsType.Vibration, true},
		}
	);

	[JsonIgnore]
	public DateTimeOffset ExpiredLifeBoosterAt => DateTimeOffset.FromUnixTimeMilliseconds(ExpiredLifeBoosterTime);
	[JsonIgnore]
	public DateTimeOffset EndChargeLifeAt => DateTimeOffset.FromUnixTimeMilliseconds(EndChargeLifeTime);

	public User Update
	(
	  	in Optional<long> coin = default, 
		in Optional<int> life = default,
		in Optional<int> puzzle = default,
		in Optional<DateTimeOffset> expiredLifeBoosterAt = default,
		in Optional<DateTimeOffset> endChargeLifeAt = default,
		in Optional<int> level = default,
		in Optional<int> currentPlayingPuzzleIndex = default,
		in Optional<(int, uint)> unlockedPuzzlePiece = default,
		in Optional<List<int>> clearedPuzzleCollection = default,
		in Optional<(int, uint)> playingPuzzle = default,
		in Optional<Dictionary<SkillItemType, int>> ownSkillItems = default,
		in Optional<Dictionary<SettingsType, bool>> settings = default
	)
	{
		DateTimeOffset now = DateTimeOffset.Now;
		var endChargeAtValue = endChargeLifeAt.GetValueOrDefault(EndChargeLifeAt);

		TimeSpan chargeTimeSpan = now > endChargeAtValue? TimeSpan.Zero : endChargeAtValue - now;

		var remain = Mathf.CeilToInt((float)(chargeTimeSpan / TimeSpan.FromMilliseconds(Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS)));

		int maxLife = Constant.User.MAX_LIFE_COUNT;
		var modifiedLife = Mathf.Clamp(maxLife - remain, 0, maxLife);

		var (playingPuzzleIndex, placedPieces) = playingPuzzle.GetValueOrDefault((0, 0));
		var (index, pieces) = unlockedPuzzlePiece.GetValueOrDefault((0, 0));
		var currentPlayingCollection = PlayingPuzzleCollection ?? new();
		var unlockedPuzzlePieceDic = UnlockedPuzzlePieceDic ?? new();

		if (playingPuzzleIndex > 0)
		{
			if (!currentPlayingCollection.ContainsKey(playingPuzzleIndex))
			{
				currentPlayingCollection.Add(playingPuzzleIndex, 0);
			}


			currentPlayingCollection[playingPuzzleIndex] = placedPieces;
		}

		if (index > 0)
		{
			if (!unlockedPuzzlePieceDic.ContainsKey(index))
			{
				unlockedPuzzlePieceDic.Add(index, 0);
			}
			unlockedPuzzlePieceDic[index] = pieces;
		}

		return new User(
			Coin: coin.GetValueOrDefault(Coin),
			Life: modifiedLife,
			Puzzle: puzzle.GetValueOrDefault(Puzzle),
			ExpiredLifeBoosterTime: expiredLifeBoosterAt.GetValueOrDefault(ExpiredLifeBoosterAt).ToUnixTimeMilliseconds(),
			EndChargeLifeTime: endChargeLifeAt.GetValueOrDefault(EndChargeLifeAt).ToUnixTimeMilliseconds(),
			Level: level.GetValueOrDefault(Level),
			CurrentPlayingPuzzleIndex: currentPlayingPuzzleIndex.GetValueOrDefault(CurrentPlayingPuzzleIndex),
			UnlockedPuzzlePieceDic: unlockedPuzzlePieceDic,
			PlayingPuzzleCollection: currentPlayingCollection,
			ClearedPuzzleCollection: clearedPuzzleCollection.GetValueOrDefault(ClearedPuzzleCollection),
			OwnSkillItems: ownSkillItems.GetValueOrDefault(OwnSkillItems),
			Settings: settings.GetValueOrDefault(Settings)
		);
	}

	public (bool coin, bool life, bool puzzle) Valid
	(
		in Optional<long> requireCoin = default,
		in Optional<int> requirePuzzle = default
	)
	{
		return 
		(
			coin: Coin >= requireCoin.GetValueOrDefault(0),
			life: DateTimeOffset.FromUnixTimeMilliseconds(ExpiredLifeBoosterTime) - DateTimeOffset.Now > TimeSpan.FromSeconds(0) || Life > 0,
			puzzle: Puzzle >= requirePuzzle.GetValueOrDefault(0)
		);
	}

	public bool IsFullLife() => Life >= Constant.User.MAX_LIFE_COUNT;

	public string GetLifeString()
	{
		DateTimeOffset expiredLifeBoosterAt = DateTimeOffset.FromUnixTimeMilliseconds(ExpiredLifeBoosterTime);

		//Debug.Log(expiredLifeBoosterAt);

		if (DateTime.Now <= expiredLifeBoosterAt)
		{
			TimeSpan span = expiredLifeBoosterAt - DateTime.Now;
			return span.ToString(@"mm\:ss");
		}

		if (Life > 0)
		{
			return Life.ToString();
		}

		DateTimeOffset endChargeLifeAt = DateTimeOffset.FromUnixTimeMilliseconds(EndChargeLifeTime);

		//Debug.Log(endChargeLifeAt);
		TimeSpan chargeTimeSpan = endChargeLifeAt - DateTime.Now;
		var millionSec = Math.Max(0, chargeTimeSpan.TotalMilliseconds % Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS);

		return TimeSpan.FromMilliseconds(millionSec).ToString(@"mm\:ss");
	}
}
