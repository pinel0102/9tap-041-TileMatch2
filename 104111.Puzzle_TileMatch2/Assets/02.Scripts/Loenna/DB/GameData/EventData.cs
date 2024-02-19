using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

public record EventData
(
	long Index,
	GameEventType EventType,
	DifficultType DifficultType,
	int Level,
	int EXP,
	int Coin,
	int StashItem, 
	int UndoItem, 
	int ShuffleItem,
	long HeartBooster,
    long DoubleTime
) : TableRowData<long>(Index)
{
	private readonly List<IReward> m_rewards = new();
	public List<IReward> Rewards => m_rewards;

    public void ResetRewards()
    {
        m_rewards.Clear();
    }

	public void CreateRewards()
	{
		List<IReward> result = new();
		PropertyInfo[] infos = typeof(EventData).GetProperties();

		foreach (var info in infos)
		{
			if (!System.Enum.TryParse(typeof(ProductType), info.Name, true, out var type))
			{
				continue;
			}

			if (type is ProductType.Unknown)
			{
				continue;
			}

			ProductType goodsType = (ProductType)type;

			long? count = goodsType switch {
				ProductType.HeartBooster => (long)info.GetValue(this),
                ProductType.DoubleTime => (long)info.GetValue(this),
				_=> (int)info.GetValue(this)
			};

			if (!count.HasValue || count <= 0)
			{
				continue;
			}

            //UnityEngine.Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", goodsType, count));

			result.Add(
				goodsType switch {
					ProductType.HeartBooster => new BoosterReward(goodsType, count.Value),
                    ProductType.DoubleTime => new BoosterReward(goodsType, count.Value),
					_=> new ItemReward(goodsType, (int)count.Value)
				}
			);
		}

		m_rewards.AddRange(result);
	}

	public IReward this[ProductType type] => m_rewards.FirstOrDefault(reward => reward.Type == type);
}
