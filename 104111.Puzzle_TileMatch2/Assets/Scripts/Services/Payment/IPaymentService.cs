using Unity.VisualScripting;

using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace NineTap.Payment
{
	public interface IPaymentService
	{
		PaymentType PaymentType { get; }
		IReadOnlyList<ProductData> Products { get; }
		UniTask<bool> LoadProducts(ProductDataTable productDataTable);
		void Request(ProductData product, Action<IPaymentResult.Success> onSuccess, Action<IPaymentResult.Error> onError);
	}

	public class PaymentService
	{
		private readonly Dictionary<PaymentType, IPaymentService> m_services;
		private ProductDataTable m_productDataTable;

		public PaymentService(params IPaymentService[] services)
		{
			m_services = new();
			m_services.AddRange(services.ToDictionary(keySelector: service => service.PaymentType));
		}

		public async UniTask<bool> LoadProducts(ProductDataTable productDataTable)
		{
			m_productDataTable = productDataTable;
			await foreach (var (_, service) in m_services.ToUniTaskAsyncEnumerable())
			{
				await service.LoadProducts(productDataTable);
			}

			return true;
		}

		public void Request(int productIndex, Action<IPaymentResult.Success> onSuccess, Action<IPaymentResult.Error> onError)
		{
			if (m_productDataTable.TryGetValue(productIndex, out var product))
			{
				if (m_services.TryGetValue(product.PaymentType, out var service))
				{
					service.Request(product, onSuccess, onError);
				}
			}
		}
	}
}
