using System;
using System.Reflection;

namespace NineTap.Common
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class CustomSortingOrderAttribute : Attribute
	{
		public int SortingOrder { get; set; }

		public CustomSortingOrderAttribute(int sortingOrder)
		{
			SortingOrder = sortingOrder;
		}

		public static bool TryGetSortingOrder<T>(out int sortingOrder)
		{
			CustomSortingOrderAttribute attribute
				= typeof(T).GetCustomAttribute<CustomSortingOrderAttribute>(inherit: false);

			sortingOrder = attribute?.SortingOrder ?? -1;

			return attribute != null && attribute.SortingOrder > 0;
		}
	}
}