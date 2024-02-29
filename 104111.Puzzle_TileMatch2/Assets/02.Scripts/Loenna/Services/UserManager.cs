using UnityEngine;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;

public class UserManager : IDisposable
{
	private readonly IDisposableAsyncReactiveProperty<User> m_user;
	private readonly TimeManager m_timeManager;
	
	public event Action<User> OnUpdated;
	public User Current => m_user.Value;

	public UserManager(TimeManager timeManager)
	{
        GlobalDefine.SetUserLoaded(false);

		m_timeManager = timeManager;
		m_user = new AsyncReactiveProperty<User>(User.NewUser).WithDispatcher();

		m_user.Subscribe(user => OnUpdated?.Invoke(user));
		m_timeManager.OnUpdateEverySecond += OnUpdateEverySecond;
	}

	public void Dispose()
	{
		OnUpdated = null;
	}

	public async UniTask<bool> LoadAsync(bool editorMode, GameObject waitPanel)
	{
        Debug.Log(CodeManager.GetAsyncName());

        if (editorMode)
		{
			int level = PlayerPrefs.GetInt(Constant.Editor.LATEST_LEVEL_KEY, 1);
			CreateDummy(level);
			return true;
		}

		string savedPath = Constant.User.DATA_PATH;

		if (File.Exists(savedPath))
		{
			using (FileStream fs = new FileStream(savedPath, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					string json = await sr.ReadToEndAsync().AsUniTask();
                    m_user.Value = JsonConvert.DeserializeObject<User>(json);
				}
			}
		}
		else
		{
			m_user.Value = User.NewUser;
		}

        if(Current.AppOpenCount == 0)
        {
            UpdateLog(installDate: GlobalDefine.TimeToString(DateTime.Now));
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>First Open : {0}</color>", Current.InstallDate));
        }

        if(string.IsNullOrEmpty(Current.UserGroup))
        {
            UpdateLog(userGroup: GlobalDefine.GetNewUserGroup());
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>User Group : {0}</color>", Current.UserGroup));
        }

        UpdateLog(appOpenCount: Current.AppOpenCount + 1);
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>App Open Count : {0}</color>", Current.AppOpenCount));

        GlobalData.Instance.CURRENT_SCENE = GlobalDefine.SCENE_MAIN;
        GlobalData.Instance.CURRENT_LEVEL = Current.Level;

        LogUserData();

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>LevelPlayCount : {0} / TotalPlayTime : {1} / TotalPayment : {2}</color>", m_user.Value.LevelPlayCount, m_user.Value.TotalPlayTime, m_user.Value.TotalPayment));
        
        SDKManager.Instance.Initialize(m_user.Value.AppOpenCount, m_user.Value.InstallDate, m_user.Value.UserGroup, m_user.Value.NoAD);
        PushManager.Initialize(ProjectManager.productName, GlobalDefine.ToDateTime(m_user.Value.InstallDate));

        GlobalDefine.Initialize();
        GlobalDefine.SetUserLoaded(true);

#if !UNITY_STANDALONE
        await CheckAgrees(waitPanel);
#endif
        waitPanel?.SetActive(false);

		return true;

		void CreateDummy(int level)
		{
			m_user.Value = new User(
                AgreePrivacy: true,
                AgreeService: true,
                AgreeCMP: true,
                AgreeATT: true,
                AgreeCMPState: true,
                AgreeATTState: true,
				Coin: 9999999,
				Life: Constant.User.MAX_LIFE_COUNT,
				Puzzle: 0,
                GoldPiece: 0,
				ExpiredLifeBoosterTime: 0L,
				EndChargeLifeTime: 0L,
                IsRated: false,
                ReviewPopupCount: 0,
				Level: level,
				CurrentPlayingPuzzleIndex: 1001,
				UnlockedPuzzlePieceDic: new(),
				ClearedPuzzleCollection: new(),
				PlayingPuzzleCollection: new(),
				OwnSkillItems: new Dictionary<SkillItemType, int>{
					{SkillItemType.Stash, int.MaxValue},
					{SkillItemType.Undo, int.MaxValue},
					{SkillItemType.Shuffle, int.MaxValue}
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
                PuzzleOpenPopupIndex: -1,
                UserGroup: "A",
                AppOpenCount: 1,
                LevelPlayCount: 0,
                TotalPlayTime: 0,
                TotalPayment: 0,
                InterstitalViewCount: 0,
                RewardViewCount: 0,
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
                ExpiredSweetHolicBoosterTime: 0L
			);
		}
	}
    
    private async UniTask CheckAgrees(GameObject waitPanel)
    {
        string region = PreciseLocale.GetRegion();

#if UNITY_EDITOR
        region = "KR";
#endif

        Debug.Log(CodeManager.GetMethodName() + region);

        if (!m_user.Value.AgreeService || !m_user.Value.AgreePrivacy)
        {
            waitPanel?.SetActive(true);
            GlobalData.Instance.ShowAgreePopup_GDPR(region);
        }

        await UniTask.WaitUntil(() => 
            m_user.Value.AgreeService && m_user.Value.AgreePrivacy 
        );

        if (UmpManager.useCMPCheck)
        {
            if (!m_user.Value.AgreeCMP)
            {
                waitPanel?.SetActive(true);
                GlobalData.Instance.ShowAgreePopup_CMP(region);
            }

            await UniTask.WaitUntil(() => 
                m_user.Value.AgreeCMP
            );
        }
        
#if (UNITY_IOS && !UNITY_STANDALONE) || UNITY_EDITOR
        if (!m_user.Value.AgreeATT)
        {
            waitPanel?.SetActive(true);
            GlobalData.Instance.ShowAgreePopup_ATT(region);
        }

        await UniTask.WaitUntil(() => 
            m_user.Value.AgreeATT
        );
#endif
    }

    public void LogUserData()
    {
        string json = JsonConvert.SerializeObject(Current);
        Debug.Log(CodeManager.GetMethodName() + string.Format("\n{0}", JsonHelper.Beautify(json)));
    }

	public void Save()
	{
		string mode = PlayerPrefs.GetString(Constant.Editor.DEVELOP_MODE_SCENE_KEY, Constant.Scene.CLIENT);

		if (mode == Constant.Scene.EDITOR)
		{
			return;
		}

        string path = Constant.User.DATA_PATH;

		if (File.Exists(path))
		{
			File.Delete(path);
			//File.Move(path, $"{path}.backup");
		}

		using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
		{
			using (StreamWriter sr = new StreamWriter(fs))
			{
				string json = JsonConvert.SerializeObject(Current);
				sr.Write(json);
			}
		}

        Debug.Log(CodeManager.GetMethodName());
	}

	public bool TryUpdate
	(
		bool requireLife = false,
		Optional<long> requireCoin = default,
		Optional<int> requirePuzzle = default
	)
	{
		if (m_user?.Value == null)
		{
			return false;
		}

		var valid = m_user.Value.Valid(requireCoin, requirePuzzle);

		if (requireCoin.HasValue && !valid.coin)
		{
			return false;
		}

		if (requireLife && !valid.life)
		{
			return false;
		}

		if (requirePuzzle.HasValue && !valid.puzzle)
		{
			return false;
		}

		bool onBooster = Current.IsBoosterTime();
        int oldLife = Current.Life;
        int newLife = onBooster ? Current.Life : Current.Life - 1;
        long oldCoin = Current.Coin;

		m_user.Update(user => 
			user.Update(
				coin: user.Coin - requireCoin.GetValueOrDefault(0),
				puzzle: user.Puzzle - requirePuzzle.GetValueOrDefault(0),
				life: newLife,
				endChargeLifeAt: CalcualteChargeLifeAt(user.EndChargeLifeTime, newLife, requireLife && !onBooster)
			)
		);

        if (requireLife)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Life] {0} - {1} = {2} / Full : {3}</color=yellow>", oldLife, onBooster ? 0 : 1, Current.Life, Current.EndChargeLifeAt.LocalDateTime));
        if (requireCoin.HasValue)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Coin] {0} - {1} = {2}</color=yellow>", oldCoin, requireCoin.Value, Current.Coin));

		DateTimeOffset CalcualteChargeLifeAt(long _oldEndChargeLifeTime, int _newLife, bool _requireLife)
		{
            DateTimeOffset at = DateTimeOffset.FromUnixTimeMilliseconds(_oldEndChargeLifeTime);
			DateTimeOffset chargeAt = at > DateTimeOffset.Now? at : DateTimeOffset.Now;

			return _newLife >= Constant.User.MAX_LIFE_COUNT ? DateTimeOffset.Now :
                _requireLife ? chargeAt.Add(TimeSpan.FromMilliseconds(Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS)) : chargeAt;
		}
		return true;
	}

	public void UpdateRewards(Dictionary<ProductType, long> rewards, int updateLevel = -1)
	{
		if (m_user?.Value == null || rewards?.Count <= 0)
		{
			return;
		}

		TimeSpan addBooster = TimeSpan.Zero;
        TimeSpan sweetHolicBooster = TimeSpan.Zero;

		if (rewards.TryGetValue(ProductType.HeartBooster, out var minutes))
		{
			addBooster = TimeSpan.FromMinutes(minutes);
		}
        if (rewards.TryGetValue(ProductType.DoubleTime, out var eventMinutes))
		{
			sweetHolicBooster = TimeSpan.FromMinutes(eventMinutes);
		}

		m_user.Update(
			user => user.Update(
                level: updateLevel > 0 ? GlobalData.Instance.GetEnableLevel(updateLevel) : m_user.Value.Level,
				coin: user.Coin + GetValue(ProductType.Coin),
				puzzle: user.Puzzle + GetValue(ProductType.PuzzlePiece),
				ownSkillItems: user.OwnSkillItems.Select(
					pair => {
						int value = pair.Value + GetValue(pair.Key.GetProductType());
						return KeyValuePair.Create(pair.Key, value);
					}
				)
				.ToDictionary(keySelector: pair => pair.Key, elementSelector: pair => pair.Value),
				expiredLifeBoosterAt: Current.IsBoosterTime() ? user.ExpiredLifeBoosterAt + addBooster
                                                              : DateTimeOffset.Now + addBooster,
                expiredSweetHolicBoosterAt: Current.IsEventBoosterTime(GameEventType.SweetHolic)
                                                              ? user.ExpiredSweetHolicBoosterAt + sweetHolicBooster
                                                              : DateTimeOffset.Now + sweetHolicBooster
			)
		);

        LogGetItems(rewards);

        int GetValue(ProductType type)
		{
			if (rewards.TryGetValue(type, out long value))
			{
				return (int)value;
			}

			return 0;
		}
	}

    public void Update
	(
	 	Optional<long> coin = default, 
		Optional<int> life = default,
		Optional<int> puzzle = default,
        Optional<int> goldPiece = default,
		Optional<DateTimeOffset> endChargeLifeAt = default,
        Optional<int> level = default,
        Optional<(int, uint)> unlockedPuzzlePiece = default,
		Optional<List<int>> clearedPuzzleCollection = default,
		Optional<(int, uint)> playingPuzzle = default,
		Optional<Dictionary<SkillItemType, int>> ownSkillItems = default
	)
	{
		if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				coin: coin,
				life: life,
				puzzle: puzzle,
                goldPiece: goldPiece,
				endChargeLifeAt: endChargeLifeAt,
				level: level,
                unlockedPuzzlePiece: unlockedPuzzlePiece,
				clearedPuzzleCollection: clearedPuzzleCollection,
				playingPuzzle: playingPuzzle,
				ownSkillItems: ownSkillItems
			)
		);
	}

    public void UpdateLog
	(
	 	Optional<string> installDate = default, 
        Optional<string> userGroup = default, 
		Optional<int> appOpenCount = default,
        Optional<int> levelPlayCount = default,
        Optional<uint> totalPlayTime = default,
        Optional<float> totalPayment = default,
		Optional<int> interstitalViewCount = default,
        Optional<int> rewardViewCount = default,
        Optional<int> failedLevel = default,
        Optional<int> failedCount = default,
        Optional<bool> noAD = default,
        Optional<bool> sendAppOpenCount = default,
        Optional<bool> sendInterstitalViewCount = default,
        Optional<bool> sendRewardViewCount = default
	)
	{
		if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				installDate: installDate,
                userGroup: userGroup,
                appOpenCount: appOpenCount,
                levelPlayCount: levelPlayCount,
                totalPlayTime: totalPlayTime,
                totalPayment: totalPayment,
                interstitalViewCount: interstitalViewCount,
                rewardViewCount: rewardViewCount,
                failedLevel: failedLevel,
                failedCount: failedCount,
                noAD: noAD,
                sendAppOpenCount: sendAppOpenCount,
                sendInterstitalViewCount: sendInterstitalViewCount,
                sendRewardViewCount: sendRewardViewCount
			)
		);
	}

    public void UpdateAgree
	(
	 	Optional<bool> agreePrivacy = default,
        Optional<bool> agreeService = default,
        Optional<bool> agreeCMP = default,
        Optional<bool> agreeATT = default,
        Optional<bool> agreeCMPState = default,
        Optional<bool> agreeATTState = default
	)
	{
		if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				agreePrivacy: agreePrivacy,
                agreeService: agreeService,
                agreeCMP: agreeCMP,
                agreeATT: agreeATT,
                agreeCMPState: agreeCMPState,
                agreeATTState: agreeATTState
			)
		);
	}

    public void UpdateReviewPopup(
        Optional<bool> isRated = default,
        Optional<int> reviewPopupCount = default,
        Optional<string> reviewPopupDate = default
    )
    {
        if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				isRated: isRated,
                reviewPopupCount: reviewPopupCount,
				reviewPopupDate: reviewPopupDate
			)
		);
    }

    public void UpdateDailyReward
	(
	 	Optional<string> dailyRewardDate = default,
        Optional<int> dailyRewardIndex = default
	)
	{
		if (m_user?.Value == null)
		{
			return;
		}

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1}", dailyRewardDate, dailyRewardIndex));

		m_user.Update(
			user => user.Update(
				dailyRewardDate: dailyRewardDate,
                dailyRewardIndex: dailyRewardIndex
			)
		);
	}

    public void UpdateRemoveAdsPopup(
        Optional<int> RemoveAdsPopupCount = default,
        Optional<string> RemoveAdsPopupDate = default
    )
    {
        if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				removeAdsPopupCount: RemoveAdsPopupCount,
				removeAdsPopupDate: RemoveAdsPopupDate
			)
		);
    }

    public void UpdateBundle(
        Optional<bool> ShowedPopupBeginner = default,
        Optional<bool> ShowedPopupWeekend1 = default,
        Optional<bool> ShowedPopupWeekend2 = default,
        Optional<bool> PurchasedBeginner = default,
        Optional<bool> PurchasedWeekend1 = default,
        Optional<bool> PurchasedWeekend2 = default,
        Optional<bool> PurchasedHard = default,
        Optional<bool> PurchasedCheerup1 = default,
        Optional<bool> PurchasedCheerup2 = default,
        Optional<string> LastPopupDateBeginner = default,
        Optional<string> LastPopupDateWeekend1 = default,
        Optional<string> LastPopupDateWeekend2 = default,
        Optional<string> LastPopupDateHard = default,
        Optional<string> LastPopupDateCheerup1 = default,
        Optional<string> LastPopupDateCheerup2 = default,
        Optional<string> PurchasedDateBeginner = default,
        Optional<string> PurchasedDateWeekend1 = default,
        Optional<string> PurchasedDateWeekend2 = default,
        Optional<string> PurchasedDateHard = default,
        Optional<string> PurchasedDateCheerup1 = default,
        Optional<string> PurchasedDateCheerup2 = default,
        Optional<string> WeekendStartDate = default,
        Optional<string> WeekendEndDate = default,
        Optional<string> NextPopupDateHard = default,
        Optional<string> NextPopupDateCheerup = default
    )
    {
        if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				showedPopupBeginner: ShowedPopupBeginner,
                showedPopupWeekend1: ShowedPopupWeekend1,
				showedPopupWeekend2: ShowedPopupWeekend2,
                purchasedBeginner: PurchasedBeginner,
                purchasedWeekend1: PurchasedWeekend1,
                purchasedWeekend2: PurchasedWeekend2,
                purchasedHard: PurchasedHard,
                purchasedCheerup1: PurchasedCheerup1,
                purchasedCheerup2: PurchasedCheerup2,
                lastPopupDateBeginner: LastPopupDateBeginner,
                lastPopupDateWeekend1: LastPopupDateWeekend1,
                lastPopupDateWeekend2: LastPopupDateWeekend2,
                lastPopupDateHard: LastPopupDateHard,
                lastPopupDateCheerup1: LastPopupDateCheerup1,
                lastPopupDateCheerup2: LastPopupDateCheerup2,
                purchasedDateBeginner: PurchasedDateBeginner,
                purchasedDateWeekend1: PurchasedDateWeekend1,
                purchasedDateWeekend2: PurchasedDateWeekend2,
                purchasedDateHard: PurchasedDateHard,
                purchasedDateCheerup1: PurchasedDateCheerup1,
                purchasedDateCheerup2: PurchasedDateCheerup2,
                weekendStartDate: WeekendStartDate,
                weekendEndDate: WeekendEndDate,
                nextPopupDateHard: NextPopupDateHard,
                nextPopupDateCheerup: NextPopupDateCheerup
			)
		);
    }

    public void UpdateEvent_SweetHolic(
        Optional<int> TotalExp = default,
        Optional<bool> ShowedPopup = default,
        Optional<string> StartDate = default,
        Optional<string> EndDate = default,
        Optional<DateTimeOffset> ExpiredSweetHolicBoosterAt = default
    )
    {
        if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				event_SweetHolic_TotalExp: TotalExp,
                event_SweetHolic_ShowedPopup: ShowedPopup,
                event_SweetHolic_StartDate: StartDate,
                event_SweetHolic_EndDate: EndDate,
                expiredSweetHolicBoosterAt: ExpiredSweetHolicBoosterAt
			)
		);
    }

    public void GetItems
	(
	 	Optional<long> addCoin = default, 
		Optional<Dictionary<SkillItemType, int>> addSkillItems = default,
        Optional<long> addBooster = default
	)
	{
		if (m_user?.Value == null)
		{
			return;
		}

        long oldCoin = m_user.Value.Coin;
        long oldBooster = m_user.Value.ExpiredLifeBoosterTime;
        Dictionary<SkillItemType, int> oldItems = m_user.Value.OwnSkillItems;

        m_user.Update(
			user => user.Update(
				coin: user.Coin + addCoin.GetValueOrDefault(0),
				ownSkillItems: 
                    addSkillItems.HasValue ?
                    user.OwnSkillItems.Select(
                        pair => {
                            int value = pair.Value + (addSkillItems.Value.ContainsKey(pair.Key) ? addSkillItems.Value[pair.Key] : 0);
                            return KeyValuePair.Create(pair.Key, value);
                        }
                    )
                    .ToDictionary(keySelector: pair => pair.Key, elementSelector: pair => pair.Value) : user.OwnSkillItems,
                expiredLifeBoosterAt: Current.IsBoosterTime() ? user.ExpiredLifeBoosterAt + TimeSpan.FromMinutes(addBooster.GetValueOrDefault(0))
                                                              : DateTimeOffset.Now + TimeSpan.FromMinutes(addBooster.GetValueOrDefault(0))
			)
		);

        if(addCoin.GetValueOrDefault(0) != 0)
        {
            LogGetItem("Coin", oldCoin, addCoin.Value, m_user.Value.Coin);
        }
        if(addSkillItems.HasValue)
        {
            LogGetItems(oldItems, addSkillItems.Value, m_user.Value.OwnSkillItems);
        }
        if(addBooster.GetValueOrDefault(0) != 0)
        {
            LogGetItem("Booster", oldBooster, addBooster.Value, m_user.Value.ExpiredLifeBoosterTime);
        }
	}

    public void ResetLife()
    {
        if (m_user?.Value == null)
		{
			return;
		}

        m_user.Update(
			user => user.Update(
				life: Constant.User.MAX_LIFE_COUNT,
                endChargeLifeAt: DateTimeOffset.Now
			)
		);

        Debug.Log(CodeManager.GetMethodName() + string.Format("[Life] {0} / Full : {1}", Current.Life, Current.EndChargeLifeAt.LocalDateTime));
    }

    public void GetItem_Life(int addCount)
	{
		if (m_user?.Value == null)
		{
			return;
		}

        int oldLife = Current.Life;

        m_user.Update(
			user => user.Update(
				life: user.Life + addCount,
				endChargeLifeAt: CalcualteChargeLifeAt(user.EndChargeLifeTime, user.Life, addCount)
			)
		);

        Debug.Log(CodeManager.GetMethodName() + string.Format("[Life] {0} + {1} = {2} / Full : {3}", oldLife, addCount, Current.Life, Current.EndChargeLifeAt.LocalDateTime));

        DateTimeOffset CalcualteChargeLifeAt(long _oldEndChargeLifeTime, int _oldLife, int _addCount)
        {
            DateTimeOffset at = DateTimeOffset.FromUnixTimeMilliseconds(_oldEndChargeLifeTime);
            DateTimeOffset chargeAt = at > DateTimeOffset.Now ? at : DateTimeOffset.Now;

            return (_oldLife + _addCount) >= Constant.User.MAX_LIFE_COUNT ? DateTimeOffset.Now : (
                _addCount == 0 ? chargeAt : chargeAt.Subtract(TimeSpan.FromMilliseconds(Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS * _addCount)));
        }
	}

    public void UpdatePuzzleOpenIndex(
        Optional<int> puzzleOpenPopupIndex = default
    )
    {
        if (m_user?.Value == null)
		{
			return;
		}

		m_user.Update(
			user => user.Update(
				puzzleOpenPopupIndex: puzzleOpenPopupIndex
			)
		);
    }

	public void UpdateSettings(SettingsType type, bool value)
	{
		if (m_user?.Value == null)
		{
			return;
		}

        if(!m_user.Value.Settings.ContainsKey(type))
        {
            m_user.Value.Settings.TryAdd(type, value);
        }

        m_user.Update(
			user => user.Update(
				settings: user.Settings
					.Select(
						pair => {
                            if (pair.Key == type)
							{
								return KeyValuePair.Create(type, value);
							}
							return pair;
						}
					)
					.ToDictionary(keySelector: pair => pair.Key, elementSelector: pair => pair.Value)
			)
		);
	}

	private void OnUpdateEverySecond(TimeSpan second)
	{
		if (!Current.IsFullLife() || Current.IsBoosterTime())
		{
			m_user.Update(user => user.Update());
		}
	}

    public void ResetUser()
    {
        m_user.Value = User.NewUser;
        Save();
    }

    public void ResetPuzzle()
    {
        m_user.Value = m_user.Value with { 
            CurrentPlayingPuzzleIndex = 1001,
            ClearedPuzzleCollection = new(),
            PlayingPuzzleCollection = new Dictionary<int, uint>{{1001, 0}},
            UnlockedPuzzlePieceDic = new Dictionary<int, uint>() };
        Save();
    }


#region SDK Log

    private void LogGetItem(string itemName, long oldCount, long addCount, long newValue)
    {
        if (itemName.Contains("Booster"))
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} + {2} = {3}m</color>", itemName, oldCount, addCount, newValue));
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster Time] {0}</color>", Current.ExpiredLifeBoosterAt));
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} + {2} = {3}</color>", itemName, oldCount, addCount, newValue));
        }

        SDKManager.SendAnalytics_C_Item_Get(itemName, (int)addCount);
    }

    private void LogGetItems(Dictionary<ProductType, long> rewards)
    {
        foreach(var item in rewards)
        {
            if (item.Value > 0)
            {
                if (item.Key == ProductType.HeartBooster)
                {   
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1}m</color>", item.Key, item.Value));
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster Time] {0}</color>", Current.ExpiredLifeBoosterAt));
                    
                    SDKManager.SendAnalytics_C_Item_Get(item.Key.ToString(), (int)item.Value);
                }
                else if (item.Key == ProductType.DoubleTime)
                {   
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1}m</color>", item.Key, item.Value));
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster Time] {0}</color>", Current.ExpiredSweetHolicBoosterAt));
                    
                    SDKManager.SendAnalytics_C_Item_Get(item.Key.ToString(), (int)item.Value);
                }
                else
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1}</color>", item.Key, item.Value));
                    
                    string itemName = "Unknown";
                    switch(item.Key)
                    {
                        case ProductType.Coin:        itemName = "Coin"; break;
                        case ProductType.StashItem:   itemName = "Return"; break;
                        case ProductType.UndoItem:    itemName = "Undo"; break;
                        case ProductType.ShuffleItem: itemName = "Shuffle"; break;
                        case ProductType.PuzzlePiece: itemName = "PuzzlePiece"; break;
                        case ProductType.Landmark:    itemName = "Landmark"; break;
                    }
                    SDKManager.SendAnalytics_C_Item_Get(itemName, (int)item.Value);
                }
            }
        }
    }

    private void LogGetItems(Dictionary<SkillItemType, int> oldItems, Dictionary<SkillItemType, int> rewards, Dictionary<SkillItemType, int> newValue)
    {
        foreach(var item in rewards)
        {
            if (item.Value != 0)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} + {2} = {3}</color>", item.Key, oldItems[item.Key], item.Value, newValue[item.Key]));
                
                string itemName = "Unknown";
                switch(item.Key)
                {
                    case SkillItemType.Stash:   itemName = "Return"; break;
                    case SkillItemType.Undo:    itemName = "Undo"; break;
                    case SkillItemType.Shuffle: itemName = "Shuffle"; break;
                }
                SDKManager.SendAnalytics_C_Item_Get(itemName, item.Value);
            }
        }
    }

#endregion SDK Log

}
