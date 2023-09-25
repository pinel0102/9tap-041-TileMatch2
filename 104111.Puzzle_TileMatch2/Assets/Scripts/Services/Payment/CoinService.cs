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

	public void Request(ProductData product, Action<IPaymentResult.Success> onSuccess, Action<IPaymentResult.Error> onError)
	{
		long price = (long)product.Price;

		if (m_userManager.TryUpdate(requireCoin: price))
		{
			onSuccess?.Invoke(new CoinResult.Success(product.Index));
			return;
		}

		onError?.Invoke(new CoinResult.Error());
	}

}
