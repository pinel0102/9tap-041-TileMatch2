using System;
using System.Text;

public enum ValueType
{
	None,
	Count,
	Time
}

public record ItemData
(
	int Index, 
	ValueType ValueType, 
	string ImagePath, 
	string Name,
    string Description,
	int ProductIndex
): TableRowData<int>(Index)
{
	public string ToString(long value)
	{
		if (ValueType is ValueType.Count)
		{
			return $"x{value}";
		}

		if (ValueType is ValueType.Time)
		{
			TimeSpan timeSpan = TimeSpan.FromMinutes(value);

			StringBuilder stringBuilder = new StringBuilder();
			if (timeSpan.Hours > 0)
			{
				stringBuilder.Append($"{timeSpan.Hours}h");
			}

			if (timeSpan.Minutes > 0)
			{
				stringBuilder.Append($"{timeSpan.Minutes}m");
			}

			return stringBuilder.ToString();
		}

		return value.ToString();
	}
}
