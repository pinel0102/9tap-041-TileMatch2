using System.Text;

public static class StringExtensions
{
	public static char GetSafeChar(this string value, int index)
	{
		if (string.IsNullOrEmpty(value))
		{
			return char.MinValue;
		}

		int count = value.GetSafeCount();

		if (index < 0 || index >= count)
		{
			return char.MinValue;
		}

		return value[index];
	}

	public static int GetSafeCount(this string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}

		return value.Length;
	}

	public static string ConvertPascalCaseToLowerSnakeCase(this string value)
	{
		return ConvertPascalCaseToSnakeCase(value).ToLowerInvariant();
	}

	public static string ConvertPascalCaseToUpperSnakeCase(this string value)
	{
		return ConvertPascalCaseToSnakeCase(value).ToUpperInvariant();
	}

	private static string ConvertPascalCaseToSnakeCase(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		int startPosition = 0;
		int separatorIndex = 0;
		StringBuilder builder = new StringBuilder(value.Length);

		while (startPosition < value.Length)
		{
			separatorIndex = value.IndexOf('_', startPosition);

			int endPosition = separatorIndex == -1 ? value.Length : separatorIndex;
			int i = startPosition;

			while (i < endPosition)
			{
				if (char.IsDigit(value[i]))
				{
					while (i < endPosition && char.IsDigit(value[i]))
					{
						builder.Append(value[i]);
						i += 1;
					}
				}
				else if (char.IsUpper(value[i]))
				{
					int upperCharacterCount = 0;

					while (i < endPosition && char.IsUpper(value[i]))
					{
						builder.Append(value[i]);
						i += 1;
						upperCharacterCount += 1;
					}

					int lowerCharacterCount = 0;

					while (i < endPosition && char.IsLower(value[i]))
					{
						builder.Append(value[i]);
						i += 1;
						lowerCharacterCount += 1;
					}

					if (lowerCharacterCount > 0 && upperCharacterCount > 2)
					{
						builder.Insert(builder.Length - lowerCharacterCount - 1, '_');
					}
				}
				else if (char.IsLower(value[i]))
				{
					while (i < endPosition && char.IsLower(value[i]))
					{
						builder.Append(value[i]);
						i += 1;
					}
				}
				else if (!char.IsWhiteSpace(value[i]))
				{
					builder.Append(value[i]);
					i += 1;

					continue;
				}

				while (i < endPosition && char.IsWhiteSpace(value[i]))
				{
					i += 1;
				}

				if (i < endPosition && char.IsLetterOrDigit(value[i]))
				{
					builder.Append('_');
				}
			}

			if (separatorIndex == -1)
			{
				break;
			}

			builder.Append('_');
			startPosition = separatorIndex + 1;
		}

		return builder.ToString();
	}
}