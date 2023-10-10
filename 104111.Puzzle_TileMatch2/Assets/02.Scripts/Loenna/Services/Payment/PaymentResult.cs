namespace NineTap.Payment
{

	public interface IPaymentResult
	{
		record Nothing() : IPaymentResult;
		record Success(int Index) : IPaymentResult;
		record Error(string ErrorMessage) : IPaymentResult;
	}

	public record IAPResult(bool IsProcessing = false) : IPaymentResult
	{
		public record Nothing() : IPaymentResult.Nothing();
		public sealed record Purchasing() : IAPResult(true), IPaymentResult;

		public sealed record Success(int Index, string ReceiptId) : IPaymentResult.Success(Index);

		public sealed record Error(string ErrorMessage) : IPaymentResult.Error(ErrorMessage);
	}

	public record CoinResult : IPaymentResult
	{
		public sealed record Success(int Index) : IPaymentResult.Success(Index);
		public sealed record Error() : IPaymentResult.Error("");
	}
}
