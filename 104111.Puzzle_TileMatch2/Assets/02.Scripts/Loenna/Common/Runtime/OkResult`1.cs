#nullable enable

namespace NineTap.Common
{
	public readonly struct OkResult<T>
	{
		public readonly T Value;

		public OkResult(T value)
		{
			Value = value;
		}
	}
}
