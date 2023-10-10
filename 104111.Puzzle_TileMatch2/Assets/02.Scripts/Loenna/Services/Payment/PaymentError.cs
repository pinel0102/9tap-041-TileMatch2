namespace NineTap.Payment
{
	public readonly struct PaymentError
	{
		public readonly PaymentErrorCode Code;
		public readonly string Message;

		public PaymentError(PaymentErrorCode code, string message)
		{
			Code = code;
			Message = message;
		}
	}
}
