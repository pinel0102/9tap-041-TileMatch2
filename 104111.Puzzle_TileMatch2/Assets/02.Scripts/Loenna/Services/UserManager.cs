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
		m_timeManager = timeManager;
		m_user = new AsyncReactiveProperty<User>(User.NewUser).WithDispatcher();

		m_user.Subscribe(user => OnUpdated?.Invoke(user));
		m_timeManager.OnUpdateEverySecond += OnUpdateEverySecond;
	}

	public void Dispose()
	{
		OnUpdated = null;
	}

	public async UniTask<bool> LoadAsync(bool editorMode)
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

        LogUserData();

		return true;

		void CreateDummy(int level)
		{
			m_user.Value = new User(
				Coin: 9999999,
				Life: Constant.User.MAX_LIFE_COUNT,
				Puzzle: 0,
				ExpiredLifeBoosterTime: 0L,
				EndChargeLifeTime: 0L,
                IsRated: false,
				Level: level,
				CurrentPlayingPuzzleIndex: new(),
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
				}
			);
		}
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

		bool onBooster = DateTimeOffset.FromUnixTimeMilliseconds(Current.ExpiredLifeBoosterTime) - DateTimeOffset.Now >= TimeSpan.FromSeconds(0);
        int oldLife = Current.Life;
        int newLife = onBooster ? Current.Life : Current.Life - 1;

		m_user.Update(user => 
			user.Update(
				coin: user.Coin - requireCoin.GetValueOrDefault(0),
				puzzle: user.Puzzle - requirePuzzle.GetValueOrDefault(0),
				life: newLife,
				endChargeLifeAt: CalcualteChargeLifeAt(user.EndChargeLifeTime, newLife, requireLife && user.ExpiredLifeBoosterTime <= 0)
			)
		);

        if(requireLife)
            Debug.Log(CodeManager.GetMethodName() + string.Format("[Life] {0} - {1} = {2} / Full : {3}", oldLife, onBooster ? 0 : 1, Current.Life, Current.EndChargeLifeAt.LocalDateTime));

		DateTimeOffset CalcualteChargeLifeAt(long _oldEndChargeLifeTime, int _newLife, bool _requireLife)
		{
			DateTimeOffset at = DateTimeOffset.FromUnixTimeMilliseconds(_oldEndChargeLifeTime);
			DateTimeOffset chargeAt = at > DateTimeOffset.Now? at : DateTimeOffset.Now;

			return _newLife >= Constant.User.MAX_LIFE_COUNT ? DateTimeOffset.Now :
                _requireLife ? chargeAt.Add(TimeSpan.FromMilliseconds(Constant.User.REQUIRE_CHARGE_LIFE_MILLISECONDS)) : chargeAt;
		}
		return true;
	}

	public void Update(int level, Dictionary<ProductType, long> rewards)
	{
		if (m_user?.Value == null || rewards?.Count <= 0)
		{
			return;
		}

		TimeSpan booster = TimeSpan.Zero;

		if (rewards.TryGetValue(ProductType.HeartBooster, out var seconds))
		{
			booster = TimeSpan.FromSeconds(seconds);
		}

		m_user.Update(
			user => user.Update(
				level: level,
				coin: user.Coin + GetValue(ProductType.Coin),
				puzzle: user.Puzzle + GetValue(ProductType.PuzzlePiece),
				ownSkillItems: user.OwnSkillItems.Select(
					pair => {
						int value = pair.Value + GetValue(pair.Key.GetProductType());
						return KeyValuePair.Create(pair.Key, value);
					}
				)
				.ToDictionary(keySelector: pair => pair.Key, elementSelector: pair => pair.Value),
				expiredLifeBoosterAt: user.ExpiredLifeBoosterAt + booster
			)
		);

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
		Optional<DateTimeOffset> expiredLifeBoosterAt = default,
        Optional<int> level = default,
        Optional<bool> isRated = default,
		Optional<int> currentPlayingPuzzleIndex = default,
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
				expiredLifeBoosterAt: expiredLifeBoosterAt,
				level: level,
                isRated: isRated,
				currentPlayingPuzzleIndex: currentPlayingPuzzleIndex,
				unlockedPuzzlePiece: unlockedPuzzlePiece,
				clearedPuzzleCollection: clearedPuzzleCollection,
				playingPuzzle: playingPuzzle,
				ownSkillItems: ownSkillItems
			)
		);
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
		if (Current.IsFullLife())
		{
			return;
		}

        m_user.Update(user => user.Update());
	}
}
