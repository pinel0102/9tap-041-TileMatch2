using System;


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
	public readonly long Seconds;

	public BoosterReward(ProductType type, long seconds) : base(type)
	{
		Seconds = seconds;
	}

	public override long GetAmount()
	{
		return Seconds;
	}

	public override string GetAmountString()
	{
		return TimeSpan.FromMinutes(Seconds).ToString("mm");
	}

	public override TimeSpan GetValue() 
	{
		return TimeSpan.FromSeconds(Seconds);
	}
}