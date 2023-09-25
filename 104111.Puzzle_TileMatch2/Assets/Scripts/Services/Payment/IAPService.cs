#define IAP_DEBUG_LOG

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.Purchasing.Extension;

using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;
using static NineTap.Common.ResultUtility;
using static NineTap.Common.ResultTag;

namespace NineTap.Payment
{
	public class IAPService : IDetailedStoreListener, IPaymentService
	{
		#region Fields
		private readonly IDisposableAsyncReactiveProperty<IPaymentResult> m_purchaseResult;

		private IStoreController m_storeController;
		private IExtensionProvider m_storeExtensionProvider;

		private StandardPurchasingModule m_purchasingModule;
		private CrossPlatformValidator m_validator;

		private Action<IPaymentResult> m_onConfirmPendingPurchase;
		#endregion

		#region IPaymentService Interface
		private List<ProductData> m_products;
		public IReadOnlyList<ProductData> Products => m_products;

		public PaymentType PaymentType => PaymentType.IAP;
		#endregion

		#region Constructors
		public IAPService()
		{
			m_purchaseResult = new AsyncReactiveProperty<IPaymentResult>(new IAPResult.Nothing())
				.WithDispatcher()
				.WithAfterSetValue(value => m_onConfirmPendingPurchase?.Invoke(value));
		}

		~IAPService()
		{
			m_purchaseResult.Dispose();
		}
		#endregion

		#region IPaymentService Interface
		public UniTask<bool> LoadProducts(ProductDataTable productDataTable)
		{
			//#if UNITY_EDITOR && IAP_DEBUG_LOG
			//m_purchasingModule = StandardPurchasingModule.Instance(AppStore.fake);
			//m_purchasingModule.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
			//#else
			//m_purchasingModule = StandardPurchasingModule.Instance();
			//#endif

			//ConfigurationBuilder builder = ConfigurationBuilder.Instance(m_purchasingModule);
			//#if UNITY_IOS
			////상품 추가
			////builder.AddProduct
			//#else
			////상품 추가
			////builder.AddProduct
			//#endif
			//UnityPurchasing.Initialize(this, builder);

			return UniTask.FromResult(true);
		}

		public void Request(ProductData productData, Action<IPaymentResult.Success> onSuccess, Action<IPaymentResult.Error> onError)
		{
			if (m_purchaseResult?.Value is IAPResult.Purchasing)
			{
				onError?.Invoke(Error(PurchaseFailureReason.ExistingPurchasePending));
				return;
			}

			Product product = m_storeController?.products?.all?.FirstOrDefault(product => product.definition.id == productData.ProductId) ?? null;
			if (product == null)
			{
				onError?.Invoke(new IAPResult.Error(""));
				return;
			}

			m_purchaseResult.Update(value => value = new IAPResult.Nothing());

			m_onConfirmPendingPurchase = (value) => {
				if (value is IAPResult.Error or IAPResult.Success)
				{
					m_storeController.ConfirmPendingPurchase(product);
					switch (value)
					{
						case IAPResult.Error errorResult:
							onError?.Invoke(errorResult);
							break;
						case IAPResult.Success successResult:
							onSuccess?.Invoke(successResult);
							break;
					}
				}
			};

			m_storeController.InitiatePurchase(product);
		}
		#endregion
		
		#region IDetailedStoreListener Interface
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			m_storeController = controller;
			m_storeExtensionProvider = extensions;

			#if IAP_DEBUG_LOG
			Debug.Log("IAPService.OnInitialized");
			#endif

			if (IsCurrentStoreSupportedByValidator())
			{
				// Tangle Data 추가
				//#if UNITY_EDITOR
				//m_validator = new CrossPlatformValidator(
				//	GooglePlayTangle.Data(),
				//	AppleStoreKitTestTangle.Data(),
				//	Application.identifier
				//);
				//#else
				//m_validator = new CrossPlatformValidator(
				//	GooglePlayTangle.Data(),
				//	AppleTangle.Data(),
				//	Application.identifier
				//);
				//#endif
			}
			else
			{
				#if IAP_DEBUG_LOG
				Debug.LogWarning(WarnInvalidStoreMessage(m_purchasingModule.appStore));

				#region Local Functions
				string WarnInvalidStoreMessage(AppStore appStore)
				{
					return $"The cross-platform validator is not implemented for the currently selected store: {appStore}.";
				}
				#endregion
				#endif
			}
			
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			#if IAP_DEBUG_LOG
			Debug.LogError($"[IAPService.OnInitializeFailed] Reason: {error}");
			#endif
			m_purchaseResult.Update(value => value = Error(error));
		}

		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			#if IAP_DEBUG_LOG
			Debug.LogError($"[IAPService.OnInitializeFailed] Reason: {error} message: {message}");
			#endif
			m_purchaseResult.Update(value => value = Error(error));
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
		{
			#if IAP_DEBUG_LOG
			Debug.Log($"[Success Purchase] productID {purchaseEvent.purchasedProduct.definition.id}");
			#endif
			Product product = purchaseEvent.purchasedProduct;

			#if UNITY_EDITOR
			m_purchaseResult.Update(value => value = new IAPResult.Success(0, purchaseEvent.purchasedProduct.definition.id));
			return PurchaseProcessingResult.Complete;
			#else
			var result = ProcessPurchaseInternal();

			if (result is { Tag: ERROR })
			{
				throw new IAPSecurityException();
			}

			// 펜딩상태에 돌입
			// 서버에 영수증을 body로 실어 검증 요청한다
			RequestReceiptValidation(product, result.Value).Forget();
			return PurchaseProcessingResult.Pending;

			#region Local Functions
			Result<IPurchaseReceipt, Unit> ProcessPurchaseInternal()
			{
				if (IsPurchaseValid(product, out IPurchaseReceipt receipt))
				{
					if (receipt == null)
					{
						#if IAP_DEBUG_LOG
						Debug.LogError("IsPurchaseValid Success But receipt is null...");
						#endif
						return ResultUtility.Error();
					}

					return Ok(receipt);
				}
				#if IAP_DEBUG_LOG
				Debug.LogError("IsPurchaseValid False");
				#endif
				return ResultUtility.Error();
			}
			#endregion

			#endif
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			#if IAP_DEBUG_LOG
			Debug.LogError($"[IAPService.OnPurchaseFailed] ProductID: {failureDescription.productId} - Reason: {failureDescription.reason}");
			#endif
			m_purchaseResult.Update(value => value = Error(failureDescription.reason));
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			#if IAP_DEBUG_LOG
			Debug.LogError($"[IAPService.OnPurchaseFailed] ProductID: {product?.definition?.id} - Reason: {failureReason}");
			#endif
			m_purchaseResult.Update(value => value = Error(failureReason));
		}
		#endregion

		private bool IsPurchaseValid(Product product, out IPurchaseReceipt receipt)
		{
			receipt = null;

			if (IsCurrentStoreSupportedByValidator())
			{
				try
				{
					IPurchaseReceipt[] result = m_validator.Validate(product.receipt);
					//The validator returns parsed receipts.
					LogReceipts(result);

					receipt = Application.platform switch
					{
						RuntimePlatform.Android => result.FirstOrDefault(receipt => receipt is GooglePlayReceipt),
						RuntimePlatform.IPhonePlayer => result.FirstOrDefault(receipt =>receipt is AppleReceipt),
						_ => result.FirstOrDefault()
					};
				}
				//If the purchase is deemed invalid, the validator throws an IAPSecurityException.
				catch (IAPSecurityException reason)
				{
#if IAP_DEBUG_LOG
					Debug.Log($"Invalid receipt: {reason}");
#endif
					return false;
				}
			}

			return true;
		}

		private void LogReceipts(IEnumerable<IPurchaseReceipt> receipts)
		{
#if IAP_DEBUG_LOG
			Debug.Log("Receipt is valid. Contents:");
			foreach (var receipt in receipts)
			{
				LogReceipt(receipt);
			}
#endif
		}

		private void LogReceipt(IPurchaseReceipt receipt)
		{
			Debug.Log($"Product ID: {receipt.productID}\n" +
					$"Purchase Date: {receipt.purchaseDate}\n" +
					$"Transaction ID: {receipt.transactionID}");

			if (receipt is GooglePlayReceipt googleReceipt)
			{
				Debug.Log($"Purchase State: {googleReceipt.purchaseState}\n" +
						$"Purchase Token: {googleReceipt.purchaseToken}");
			}

			if (receipt is AppleInAppPurchaseReceipt appleReceipt)
			{
				Debug.Log($"Original Transaction ID: {appleReceipt.originalTransactionIdentifier}\n" +
						$"Subscription Expiration Date: {appleReceipt.subscriptionExpirationDate}\n" +
						$"Cancellation Date: {appleReceipt.cancellationDate}\n" +
						$"Quantity: {appleReceipt.quantity}");
			}
		}

		private async UniTaskVoid RequestReceiptValidation(Product product, IPurchaseReceipt receipt)
		{
#if !UNITY_EDITOR

			// 서버에서 validate

			m_storeController.ConfirmPendingPurchase(product);

			//m_purchaseResult.Update(value =>
				// 구매에 성공하면 result 콜백
			//);
#else
			Debug.Log($"Confirming purchase of {product.definition.id}");
			m_storeController.ConfirmPendingPurchase(product);

			await UniTask.CompletedTask;
#endif
		}

		private bool IsCurrentStoreSupportedByValidator()
		{
			return Application.platform switch
			{
				RuntimePlatform.Android => m_purchasingModule.appStore == AppStore.GooglePlay,
				RuntimePlatform.IPhonePlayer => m_purchasingModule.appStore == AppStore.AppleAppStore,
				_=> false
			};
		}

		private IPaymentResult.Error Error(InitializationFailureReason reason)
		{
			string type = nameof(InitializationFailureReason).ConvertPascalCaseToUpperSnakeCase();
			string failureReason = reason.ToString().ConvertPascalCaseToUpperSnakeCase();

			return new IAPResult.Error($"PAYMENT_ERROR_{type}_{failureReason}");
		}

		private IPaymentResult.Error Error(PurchaseFailureReason reason)
		{
			string type = nameof(PurchaseFailureReason).ConvertPascalCaseToUpperSnakeCase();
			string failureReason = reason.ToString().ConvertPascalCaseToUpperSnakeCase();

			return new IAPResult.Error($"PAYMENT_ERROR_{type}_{failureReason}");
		}
	}
}