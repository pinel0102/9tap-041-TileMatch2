using UnityEngine;

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using NineTap.Common;
using System.Linq;

[Serializable]
public record User
(
    bool AgreePrivacy,
    bool AgreeService,
    bool AgreeCMP,
    bool AgreeATT,
    bool AgreeCMPState,
    bool AgreeATTState,

    // Log
    string InstallDate,
    string UserGroup,
    int AppOpenCount,
    int LevelPlayCount,
    uint TotalPlayTime, // 누적 플레이 타임 (초).
    float TotalPayment,
    int InterstitalViewCount,
    int RewardViewCount,
    int FailedLevel,
    int FailedCount,
    bool NoAD,
    bool SendAppOpenCount,
    bool SendInterstitalViewCount,
    bool SendRewardViewCount,

    // 재화
	long Coin, // 보유 코인
	int Life, // 보유 하트
	int Puzzle, // 보유 미션조각
    int GoldPiece,
	long ExpiredLifeBoosterTime, // 하트 부스터 끝나는 시간
	long EndChargeLifeTime, // 하트가 모두 충전되는 시간
    string DailyRewardDate, // 마지막으로 출석체크 보상을 받은 시각.
    int DailyRewardIndex, // 받을 수 있는 출석체크 보상의 인덱스. == 이번 주기에서 수령한 누적 횟수.

    bool IsRated,
    string ReviewPopupDate,
    int ReviewPopupCount,
    int PuzzleOpenPopupIndex,

    string RemoveAdsPopupDate,
    int RemoveAdsPopupCount,
    
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
        AgreeService: false,
        AgreePrivacy: false,
        AgreeCMP: false,
        AgreeATT: false,
        AgreeCMPState: false,
        AgreeATTState: false,
		Coin: 0,
		Life: Constant.User.MAX_LIFE_COUNT,
		Puzzle: 0,
        GoldPiece: 0,
		ExpiredLifeBoosterTime: 0L,
		EndChargeLifeTime: 0L,
        IsRated: false,
        ReviewPopupCount: 0,
        Level: 1,
		ClearedPuzzleCollection: new(),
		CurrentPlayingPuzzleIndex: 1001, //퍼즐 하나는 무조건 언락되어 있는 상태
		PlayingPuzzleCollection: new Dictionary<int, uint>{{1001, 0}}, //퍼즐 하나는 무조건 언락되어 있는 상태
        UnlockedPuzzlePieceDic: new Dictionary<int, uint>(),
		OwnSkillItems: new Dictionary<SkillItemType, int>{
			{SkillItemType.Stash, 1},
			{SkillItemType.Undo, 1},
			{SkillItemType.Shuffle, 1}
		},
		Settings: new Dictionary<SettingsType, bool>{
			{SettingsType.Fx, true},
			{SettingsType.Bgm, true},
			{SettingsType.Vibration, true},
            {SettingsType.Notification, true},
		},
        InstallDate: GlobalDefine.dateDefault_HHmmss,
        DailyRewardDate: GlobalDefine.dateDefault_HHmmss,
        ReviewPopupDate: GlobalDefine.dateDefault_HHmmss,
        DailyRewardIndex: 0,
        UserGroup: "A",
        AppOpenCount: 0,
        LevelPlayCount: 0,
        TotalPlayTime: 0,
        TotalPayment: 0,
        InterstitalViewCount: 0,
        RewardViewCount: 0,
        PuzzleOpenPopupIndex: -1,
        FailedLevel: 0,
        FailedCount: 0,
        NoAD: false,
        SendAppOpenCount: false,
        SendInterstitalViewCount: false,
        SendRewardViewCount: false,
        RemoveAdsPopupCount: 0,
        RemoveAdsPopupDate: GlobalDefine.dateDefault_HHmmss
	);

	[JsonIgnore]
	public DateTimeOffset ExpiredLifeBoosterAt => DateTimeOffset.FromUnixTimeMilliseconds(ExpiredLifeBoosterTime);
	[JsonIgnore]
	public DateTimeOffset EndChargeLifeAt => DateTimeOffset.FromUnixTimeMilliseconds(EndChargeLifeTime);

	public User Update
	(
        in Optional<bool> agreeService = default,
        in Optional<bool> agreePrivacy = default,
        in Optional<bool> agreeCMP = default,
        in Optional<bool> agreeATT = default,
        in Optional<bool> agreeCMPState = default,
        in Optional<bool> agreeATTState = default,
	  	in Optional<long> coin = default, 
		in Optional<int> life = default,
		in Optional<int> puzzle = default,
        in Optional<int> goldPiece = default,
		in Optional<DateTimeOffset> expiredLifeBoosterAt = default,
		in Optional<DateTimeOffset> endChargeLifeAt = default,
        in Optional<int> level = default,
        in Optional<bool> isRated = default,
        in Optional<int> reviewPopupCount = default,
        in Optional<string> reviewPopupDate = default,
        in Optional<(int, uint)> unlockedPuzzlePiece = default,
		in Optional<List<int>> clearedPuzzleCollection = default,
		in Optional<(int, uint)> playingPuzzle = default,
		in Optional<Dictionary<SkillItemType, int>> ownSkillItems = default,
		in Optional<Dictionary<SettingsType, bool>> settings = default,
        in Optional<string> installDate = default,
        in Optional<string> dailyRewardDate = default,
        in Optional<int> dailyRewardIndex = default,
        in Optional<int> puzzleOpenPopupIndex = default,
        in Optional<string> userGroup = default,
        in Optional<int> appOpenCount = default,
        in Optional<int> levelPlayCount = default,
        in Optional<uint> totalPlayTime = default,
        in Optional<float> totalPayment = default,
        in Optional<int> interstitalViewCount = default,
        in Optional<int> rewardViewCount = default,
        in Optional<int> failedLevel = default,
        in Optional<int> failedCount = default,
        in Optional<bool> noAD = default,
        in Optional<bool> sendAppOpenCount = default,
        in Optional<bool> sendInterstitalViewCount = default,
        in Optional<bool> sendRewardViewCount = default,
        in Optional<int> removeAdsPopupCount = default,
        in Optional<string> removeAdsPopupDate = default
	)
	{
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset endChargeAtValue = endChargeLifeAt.GetValueOrDefault(EndChargeLifeAt);
		TimeSpan chargeTimeSpan = now > endChargeAtValue ? TimeSpan.Zero : endChargeAtValue.Subtract(now);        
		
        var remain = Mathf.CeilToInt((float)(chargeTimeSpan / TimeSpan.FromMilliseconds(Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS)));
        int maxLife = Constant.User.MAX_LIFE_COUNT;
        var modifiedLife = IsFullLife() ? Life : Mathf.Clamp(maxLife - remain, 0, maxLife);
        int newLife = life.HasValue ? life.Value : modifiedLife;
        
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

        User user = new User(
            AgreeService: agreeService.GetValueOrDefault(AgreeService),
            AgreePrivacy: agreePrivacy.GetValueOrDefault(AgreePrivacy),
            AgreeCMP: agreeCMP.GetValueOrDefault(AgreeCMP),
            AgreeATT: agreeATT.GetValueOrDefault(AgreeATT),
            AgreeCMPState: agreeCMPState.GetValueOrDefault(AgreeCMPState),
            AgreeATTState: agreeATTState.GetValueOrDefault(AgreeATTState),
			Coin: coin.GetValueOrDefault(Coin),
			Life: newLife,
			Puzzle: puzzle.GetValueOrDefault(Puzzle),
            GoldPiece: goldPiece.GetValueOrDefault(GoldPiece),
			ExpiredLifeBoosterTime: expiredLifeBoosterAt.GetValueOrDefault(ExpiredLifeBoosterAt).ToUnixTimeMilliseconds(),
			EndChargeLifeTime: endChargeLifeAt.GetValueOrDefault(EndChargeLifeAt).ToUnixTimeMilliseconds(),
            Level: level.GetValueOrDefault(Level),
            IsRated: isRated.GetValueOrDefault(IsRated),
            ReviewPopupCount: reviewPopupCount.GetValueOrDefault(ReviewPopupCount),
            ReviewPopupDate: reviewPopupDate.GetValueOrDefault(ReviewPopupDate),
            PuzzleOpenPopupIndex: puzzleOpenPopupIndex.GetValueOrDefault(PuzzleOpenPopupIndex),
            CurrentPlayingPuzzleIndex: playingPuzzleIndex > 0 ? playingPuzzleIndex : CurrentPlayingPuzzleIndex,
			UnlockedPuzzlePieceDic: unlockedPuzzlePieceDic,
			PlayingPuzzleCollection: currentPlayingCollection,
			ClearedPuzzleCollection: clearedPuzzleCollection.GetValueOrDefault(ClearedPuzzleCollection),
			OwnSkillItems: ownSkillItems.GetValueOrDefault(OwnSkillItems),
			Settings: settings.GetValueOrDefault(Settings),
            InstallDate: installDate.GetValueOrDefault(InstallDate),
            DailyRewardDate: dailyRewardDate.GetValueOrDefault(DailyRewardDate),
            DailyRewardIndex: dailyRewardIndex.GetValueOrDefault(DailyRewardIndex),
            UserGroup: userGroup.GetValueOrDefault(UserGroup),
            AppOpenCount: appOpenCount.GetValueOrDefault(AppOpenCount),
            LevelPlayCount: levelPlayCount.GetValueOrDefault(LevelPlayCount),
            TotalPlayTime: totalPlayTime.GetValueOrDefault(TotalPlayTime),
            TotalPayment: totalPayment.GetValueOrDefault(TotalPayment),
            InterstitalViewCount: interstitalViewCount.GetValueOrDefault(InterstitalViewCount),
            RewardViewCount: rewardViewCount.GetValueOrDefault(RewardViewCount),
            FailedLevel: failedLevel.GetValueOrDefault(FailedLevel),
            FailedCount: failedCount.GetValueOrDefault(FailedCount),
            NoAD: noAD.GetValueOrDefault(NoAD),
            SendAppOpenCount: sendAppOpenCount.GetValueOrDefault(SendAppOpenCount),
            SendInterstitalViewCount: sendInterstitalViewCount.GetValueOrDefault(SendInterstitalViewCount),
            SendRewardViewCount: sendRewardViewCount.GetValueOrDefault(SendRewardViewCount),
            RemoveAdsPopupCount: removeAdsPopupCount.GetValueOrDefault(RemoveAdsPopupCount),
            RemoveAdsPopupDate: removeAdsPopupDate.GetValueOrDefault(RemoveAdsPopupDate)
		);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("Life : {0} / EndChargeLifeAt : {1}", user.Life, user.EndChargeLifeAt.LocalDateTime));

		return user;
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
			life: ExpiredLifeBoosterAt - DateTimeOffset.Now > TimeSpan.FromSeconds(0) || Life > 0,
			puzzle: Puzzle >= requirePuzzle.GetValueOrDefault(0)
		);
	}

	public bool IsFullLife() => Life >= Constant.User.MAX_LIFE_COUNT;
    public bool IsBoosterTime() => ExpiredLifeBoosterAt >= DateTime.Now;
    
    private const string timeFormat_d = @"%d'd'";
    private const string timeFormat_hhmmss = @"hh\:mm\:ss";
    private const string timeFormat_hmmss = @"h\:mm\:ss";
    private const string timeFormat_mmss = @"mm\:ss";

    /// <summary>
    /// 라이프 상태를 갱신한다.
    /// </summary>
    /// <returns>
    /// <para>Item1 : 부스터 여부.</para>
    /// <para>Item2 : 라이프 개수.</para>
    /// <para>Item3 : (Item1 == true) ? 부스터 남은 시간 : 라이프 충전 시간.)</para>
    /// </returns>
    public (bool, string, string, bool) GetLifeStatus()
    {
        var (isBoosterTime, boosterRemainTime) = GetBoosterStatus();
        string heartString, timeString;

        if (isBoosterTime)
        {
            heartString = string.Empty;
            timeString = boosterRemainTime;
        }
        else
        {
            heartString = Mathf.Max(0, Life).ToString();
            
            if (IsFullLife())
            {
                timeString = Constant.User.MAX_LIFE_TEXT;
            }
            else
            {
                TimeSpan chargeTimeSpan = EndChargeLifeAt - DateTime.Now;
                var millionSec = Math.Max(0, chargeTimeSpan.TotalMilliseconds % Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS);
                TimeSpan span = TimeSpan.FromMilliseconds(millionSec);

                timeString = span.Hours > 9 ? span.ToString(timeFormat_hhmmss)
                           : span.Hours > 0 ? span.ToString(timeFormat_hmmss)
                           : span.ToString(timeFormat_mmss);
            }
        }

        return (isBoosterTime, heartString, timeString, Life >= Constant.User.MAX_LIFE_COUNT);
    }

    private (bool, string) GetBoosterStatus()
    {
        if (IsBoosterTime())
        {
            TimeSpan span = ExpiredLifeBoosterAt - DateTime.Now;
            return (true, span.Days > 0 ? string.Format("{0} {1}", span.ToString(timeFormat_d), span.ToString(timeFormat_hhmmss)) 
                        : span.Hours > 9 ? span.ToString(timeFormat_hhmmss) 
                        : span.Hours > 0 ? span.ToString(timeFormat_hmmss)
                        : span.ToString(timeFormat_mmss));
        }
        else
        {
            return (false, string.Empty);
        }
    }
}
