using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NineTap.Common;

[Serializable]
public record User
(
    // GDPR
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
    int PuzzleOpenPopupIndex,

    // Review Popup
    bool IsRated,
    string ReviewPopupDate,
    int ReviewPopupCount,

    // RemoveAds Popup
    string RemoveAdsPopupDate,
    int RemoveAdsPopupCount,

    // 자동 팝업 띄움 여부.
    bool ShowedPopupBeginner,
    bool ShowedPopupWeekend1,
    bool ShowedPopupWeekend2,

    // 팝업이 마지막으로 뜬 시각.
    string LastPopupDateBeginner,
    string LastPopupDateWeekend1,
    string LastPopupDateWeekend2,
    string LastPopupDateHard,
    string LastPopupDateCheerup1,
    string LastPopupDateCheerup2,

    // 번들 구매 여부.
    bool PurchasedBeginner,
    bool PurchasedWeekend1,
    bool PurchasedWeekend2,
    bool PurchasedHard,
    bool PurchasedCheerup1,
    bool PurchasedCheerup2,

    // 번들 구매 시각.
    string PurchasedDateBeginner,
    string PurchasedDateWeekend1,
    string PurchasedDateWeekend2,
    string PurchasedDateHard,
    string PurchasedDateCheerup1,
    string PurchasedDateCheerup2,
    
    // 주말 번들 유효 기간.
    string WeekendStartDate,
    string WeekendEndDate,

    // 팝업이 다음에 뜰 시각.
    string NextPopupDateHard,
    string NextPopupDateCheerup,

    // 이벤트.
    int Event_SweetHolic_TotalExp,
    bool Event_SweetHolic_ShowedPopup,
    string Event_SweetHolic_StartDate,
    string Event_SweetHolic_EndDate,
    string Event_SweetHolic_BoosterEndDate,

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
        RemoveAdsPopupDate: GlobalDefine.dateDefault_HHmmss,
        ShowedPopupBeginner: false,
        ShowedPopupWeekend1: false,
        ShowedPopupWeekend2: false,
        LastPopupDateBeginner: GlobalDefine.dateDefault_HHmmss,
        LastPopupDateWeekend1: GlobalDefine.dateDefault_HHmmss,
        LastPopupDateWeekend2: GlobalDefine.dateDefault_HHmmss,
        LastPopupDateHard: GlobalDefine.dateDefault_HHmmss,
        LastPopupDateCheerup1: GlobalDefine.dateDefault_HHmmss,
        LastPopupDateCheerup2: GlobalDefine.dateDefault_HHmmss,
        PurchasedBeginner: false,
        PurchasedWeekend1: false,
        PurchasedWeekend2: false,
        PurchasedHard: false,
        PurchasedCheerup1: false,
        PurchasedCheerup2: false,
        PurchasedDateBeginner: GlobalDefine.dateDefault_HHmmss,
        PurchasedDateWeekend1: GlobalDefine.dateDefault_HHmmss,
        PurchasedDateWeekend2: GlobalDefine.dateDefault_HHmmss,
        PurchasedDateHard: GlobalDefine.dateDefault_HHmmss,
        PurchasedDateCheerup1: GlobalDefine.dateDefault_HHmmss,
        PurchasedDateCheerup2: GlobalDefine.dateDefault_HHmmss,
        WeekendStartDate: GlobalDefine.dateDefault_HHmmss,
        WeekendEndDate: GlobalDefine.dateDefault_HHmmss,
        NextPopupDateHard: GlobalDefine.dateDefault_HHmmss,
        NextPopupDateCheerup: GlobalDefine.dateDefault_HHmmss,
        Event_SweetHolic_TotalExp: 0,
        Event_SweetHolic_ShowedPopup: false,
        Event_SweetHolic_StartDate: GlobalDefine.dateDefault_HHmmss,
        Event_SweetHolic_EndDate: GlobalDefine.dateDefault_HHmmss,
        Event_SweetHolic_BoosterEndDate: GlobalDefine.dateDefault_HHmmss
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
        in Optional<string> removeAdsPopupDate = default,
        in Optional<bool> showedPopupBeginner = default,
        in Optional<bool> showedPopupWeekend1 = default,
        in Optional<bool> showedPopupWeekend2 = default,
        in Optional<bool> purchasedBeginner = default,
        in Optional<bool> purchasedWeekend1 = default,
        in Optional<bool> purchasedWeekend2 = default,
        in Optional<bool> purchasedHard = default,
        in Optional<bool> purchasedCheerup1 = default,
        in Optional<bool> purchasedCheerup2 = default,
        in Optional<string> lastPopupDateBeginner = default,
        in Optional<string> lastPopupDateWeekend1 = default,
        in Optional<string> lastPopupDateWeekend2 = default,
        in Optional<string> lastPopupDateHard = default,
        in Optional<string> lastPopupDateCheerup1 = default,
        in Optional<string> lastPopupDateCheerup2 = default,
        in Optional<string> purchasedDateBeginner = default,
        in Optional<string> purchasedDateWeekend1 = default,
        in Optional<string> purchasedDateWeekend2 = default,
        in Optional<string> purchasedDateHard = default,
        in Optional<string> purchasedDateCheerup1 = default,
        in Optional<string> purchasedDateCheerup2 = default,
        in Optional<string> weekendStartDate = default,
        in Optional<string> weekendEndDate = default,
        in Optional<string> nextPopupDateHard = default,
        in Optional<string> nextPopupDateCheerup = default,
        in Optional<int> event_SweetHolic_TotalExp = default,
        in Optional<bool> event_SweetHolic_ShowedPopup = default,
        in Optional<string> event_SweetHolic_StartDate = default,
        in Optional<string> event_SweetHolic_EndDate = default,
        in Optional<string> event_SweetHolic_BoosterEndDate = default
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
            RemoveAdsPopupDate: removeAdsPopupDate.GetValueOrDefault(RemoveAdsPopupDate),
            ShowedPopupBeginner: showedPopupBeginner.GetValueOrDefault(ShowedPopupBeginner),
            ShowedPopupWeekend1: showedPopupWeekend1.GetValueOrDefault(ShowedPopupWeekend1),
            ShowedPopupWeekend2: showedPopupWeekend2.GetValueOrDefault(ShowedPopupWeekend2),
            LastPopupDateBeginner: lastPopupDateBeginner.GetValueOrDefault(LastPopupDateBeginner),
            LastPopupDateWeekend1: lastPopupDateWeekend1.GetValueOrDefault(LastPopupDateWeekend1),
            LastPopupDateWeekend2: lastPopupDateWeekend2.GetValueOrDefault(LastPopupDateWeekend2),
            LastPopupDateHard: lastPopupDateHard.GetValueOrDefault(LastPopupDateHard),
            LastPopupDateCheerup1: lastPopupDateCheerup1.GetValueOrDefault(LastPopupDateCheerup1),
            LastPopupDateCheerup2: lastPopupDateCheerup2.GetValueOrDefault(LastPopupDateCheerup2),
            PurchasedBeginner: purchasedBeginner.GetValueOrDefault(PurchasedBeginner),
            PurchasedWeekend1: purchasedWeekend1.GetValueOrDefault(PurchasedWeekend1),
            PurchasedWeekend2: purchasedWeekend2.GetValueOrDefault(PurchasedWeekend2),
            PurchasedHard: purchasedHard.GetValueOrDefault(PurchasedHard),
            PurchasedCheerup1: purchasedCheerup1.GetValueOrDefault(PurchasedCheerup1),
            PurchasedCheerup2: purchasedCheerup2.GetValueOrDefault(PurchasedCheerup2),
            PurchasedDateBeginner: purchasedDateBeginner.GetValueOrDefault(PurchasedDateBeginner),
            PurchasedDateWeekend1: purchasedDateWeekend1.GetValueOrDefault(PurchasedDateWeekend1),
            PurchasedDateWeekend2: purchasedDateWeekend2.GetValueOrDefault(PurchasedDateWeekend2),
            PurchasedDateHard: purchasedDateHard.GetValueOrDefault(PurchasedDateHard),
            PurchasedDateCheerup1: purchasedDateCheerup1.GetValueOrDefault(PurchasedDateCheerup1),
            PurchasedDateCheerup2: purchasedDateCheerup2.GetValueOrDefault(PurchasedDateCheerup2),
            WeekendStartDate: weekendStartDate.GetValueOrDefault(WeekendStartDate),
            WeekendEndDate: weekendEndDate.GetValueOrDefault(WeekendEndDate),
            NextPopupDateHard: nextPopupDateHard.GetValueOrDefault(NextPopupDateHard),
            NextPopupDateCheerup: nextPopupDateCheerup.GetValueOrDefault(NextPopupDateCheerup),
            Event_SweetHolic_TotalExp: event_SweetHolic_TotalExp.GetValueOrDefault(Event_SweetHolic_TotalExp),
            Event_SweetHolic_ShowedPopup: event_SweetHolic_ShowedPopup.GetValueOrDefault(Event_SweetHolic_ShowedPopup),
            Event_SweetHolic_StartDate: event_SweetHolic_StartDate.GetValueOrDefault(Event_SweetHolic_StartDate),
            Event_SweetHolic_EndDate: event_SweetHolic_EndDate.GetValueOrDefault(Event_SweetHolic_EndDate),
            Event_SweetHolic_BoosterEndDate: event_SweetHolic_BoosterEndDate.GetValueOrDefault(Event_SweetHolic_BoosterEndDate)
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
    public bool IsEventBoosterTime(GameEventType type)
    {
        switch(type)
        {
            case GameEventType.SweetHolic:
                return !GlobalDefine.IsExpired(GlobalDefine.ToDateTime(Event_SweetHolic_BoosterEndDate));
            default:
                return false;
        }
    }
    
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
