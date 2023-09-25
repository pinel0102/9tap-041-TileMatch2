using System.Collections.Generic;
using System.Linq;

public class ProductDataTable : Table<int, ProductData>
{
	public ProductDataTable(string path) : base(path)
	{
	}

	public List<ProductData> GetProducts(PaymentType paymentType)
	{
		return m_rowDataDic
		.Select(pair => pair.Value)
		.Where(value => value?.PaymentType == paymentType)
		.ToList();
	}

	public List<ProductData> GetProducts(ProductExposingType exposingType)
	{
		return m_rowDataDic
		.Select(pair => pair.Value)
		.Where(value => value?.Required == exposingType)
		.ToList();
	}
}
