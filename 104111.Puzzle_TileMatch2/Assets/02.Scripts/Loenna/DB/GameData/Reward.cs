using System;
using System.Text;

public interface IReward
{
	public ProductType Type { get;}
	public string GetAmountString();
	public long GetAmount();
}

public abstract class Reward<T> : IReward
{
	private readonly ProductType m_type;
	public ProductType Type => m_type;

	public Reward(ProductType type)
	{
		m_type = type;
	}

	public abstract long GetAmount();
	public abstract string GetAmountString();
	public abstract T GetValue();

}

public class ItemReward : Reward<int>
{
	public readonly int Count;

	public ItemReward(ProductType type, int count) : base (type)
	{
		Count = count;
	}

	public override long GetAmount()
	{
		return Count;
	}

	public override string GetAmountString()
	{
		return Count.ToString();
	}

	public override int GetValue() 
	{
		return Count;
	}
}

public class BoosterReward : Reward<TimeSpan>
{
	public readonly long Minutes;

	public BoosterReward(ProductType type, long minutes) : base(type)
	{
		Minutes = minutes;
	}

	public override long GetAmount()
	{
		return Minutes;
	}

	public override string GetAmountString()
	{
        TimeSpan timeSpan = GetValue();
        StringBuilder stringBuilder = new StringBuilder();
        if (timeSpan.Hours > 0)
        {
            stringBuilder.Append($"{timeSpan.Hours}h");
        }

        if (timeSpan.Minutes > 0)
        {
            if (timeSpan.Hours > 0)
                stringBuilder.Append($" ");

            stringBuilder.Append($"{timeSpan.Minutes}m");
        }
        return stringBuilder.ToString();
	}

	public override TimeSpan GetValue() 
	{
		return TimeSpan.FromMinutes(Minutes);
	}
}