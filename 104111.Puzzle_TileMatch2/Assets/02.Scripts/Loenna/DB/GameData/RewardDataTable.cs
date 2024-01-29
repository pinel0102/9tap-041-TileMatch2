using System;
using System.Collections.Generic;
using System.Linq;

public class RewardDataTable : Table<long, RewardData>
{
	public RewardDataTable(string path) : base(path)
	{
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		foreach (var data in m_rowDataDic.Values)
		{
			data.CreateRewards();
		}
	}

	public RewardData GetDefaultReward(bool hardMode)
	{
		return m_rowDataDic
			.Values?
			.FirstOrDefault(
				data => 
					data.RewardType is RewardType.Default && 
					data.DifficultType == (hardMode? DifficultType.HARD : DifficultType.NORMAL
				)
			);
	}

    public List<RewardData> GetDailyRewards()
	{
		return m_rowDataDic
			.Values?
			.Where(data => data.RewardType is RewardType.Daily)
            .OrderBy(data => data.Level).ToList();
	}

	public bool TryPreparedChestReward(int level, out RewardData reward)
	{
		reward = default;
		var chestQuests = m_rowDataDic.Values?.Where(data => data.RewardType is RewardType.Chest)?.ToArray() ?? Array.Empty<RewardData>();

		if (chestQuests.Count() <= 0)
		{
			return false;
		}

		if (!chestQuests.Any(data => data.Level >= level))
		{
			return false;
		}

		reward = chestQuests.First(data => data.Level >= level);
		return true;
	}

	public bool TryReceiveChestReward(int level, out RewardData reward)
	{
		reward = default;
		var chestQuests = m_rowDataDic.Values?.Where(data => data.RewardType is RewardType.Chest)?.ToArray() ?? Array.Empty<RewardData>();

		if (chestQuests.Count() <= 0)
		{
			return false;
		}

		if (!chestQuests.Any(data => data.Level <= level))
		{
			return false;
		}

		reward = chestQuests.Last(data => data.Level <= level);
		return true;
	}

    public bool TryPreviousChestReward(int level, out RewardData reward)
	{
		reward = default;
		var chestQuests = m_rowDataDic.Values?.Where(data => data.RewardType is RewardType.Chest)?.ToArray() ?? Array.Empty<RewardData>();

		if (chestQuests.Count() <= 0)
		{
			return false;
		}

		if (!chestQuests.Any(data => data.Level < level))
		{
			return false;
		}

		reward = chestQuests.Last(data => data.Level < level);
		return true;
	}

    public bool TryPuzzleCompleteReward(out RewardData reward)
	{
		reward = default;
		var chestQuests = m_rowDataDic.Values?.Where(data => data.RewardType is RewardType.PuzzleComplete)?.ToArray() ?? Array.Empty<RewardData>();

		if (chestQuests.Count() <= 0)
		{
			return false;
		}

		reward = chestQuests.First();
		return true;
	}
}
