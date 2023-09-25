namespace NineTap.Payment
{
	public enum PaymentErrorCode
	{
		UnsupportedPlatform, // InitializationFailureReason.AppNotKnown
		/// <summary>
		/// 상품이 존재하지 않음
		/// </summary>
		NoProductsAvailable,

		/// <summary>
		/// 결제 요청을 위해 필요한 내용이 충분하지 않아 결제를 요청할 수 없음.
		/// </summary>
		RequestNotAvailable, // PurchasingUnavailable

		/// <summary>
		/// 결제 요청이 진행 중인 상황에서 다시 결제 요청이 발생. 한 번에 한 개의 결제만 요청할 수 있다.
		/// </summary>
		RequestInProgress,

		/// <summary>
		/// 결제 실패. 세부사항은 메시지를 참고한다.
		/// </summary>
		RequestFailed,

		/// <summary>
		/// 영수증 검증 실패
		/// </summary>
		ReceiptVerificationFailed,

		UserCancelled, //UserCancelled,

		/// <summary>
		/// 결제가 거부됨
		/// </summary>
		PaymentDeclined,

		DuplicateTransaction,

		Unknown
	}
}
