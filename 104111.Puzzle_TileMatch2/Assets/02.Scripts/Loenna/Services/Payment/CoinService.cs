using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using NineTap.Payment;


//코인 결제시 이곳에서 처리
public class CoinService : IPaymentService
{
	private readonly UserManager m_userManager;
	private List<ProductData> m_products;

	public IReadOnlyList<ProductData> Products => m_products;
	public PaymentType PaymentType => PaymentType.Coin;

	public CoinService(UserManager userManager)
	{
		m_userManager = userManager;
		m_products = new();
	}

	public UniTask<bool> LoadProducts(ProductDataTable productDataTable)
	{
		m_products.AddRange(productDataTable.GetProducts(PaymentType));
		return UniTask.FromResult(true);
	}

	public void Request(ProductData productData, Action<UnityEngine.Purchasing.Product, IPaymentResult.Success> onSuccess, Action<UnityEngine.Purchasing.Product, IPaymentResult.Error> onError)
	{
		long price = (long)productData.Price;

		if (m_userManager.TryUpdate(requireCoin: price))
		{
            SDKManager.SendAnalytics_C_Item_Use("Coin", (int)price);
			onSuccess?.Invoke(null, new CoinResult.Success(productData.Index));
			return;
		}

		onError?.Invoke(null, new CoinResult.Error());
	}

}
