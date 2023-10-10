#nullable enable

namespace NineTap.Common
{
	public readonly struct ErrorResult<TError>
	{
		public readonly TError Error;

		public ErrorResult(TError error)
		{
			Error = error;
		}
	}
}
