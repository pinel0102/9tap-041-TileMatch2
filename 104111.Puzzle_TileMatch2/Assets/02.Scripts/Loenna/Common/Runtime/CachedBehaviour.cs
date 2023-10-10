#nullable enable

using UnityEngine;

namespace NineTap.Common
{
	public class CachedBehaviour : MonoBehaviour
	{
		private GameObject m_cachedGameObject = null!;
		public GameObject? CachedGameObject
		{
			get
			{
				if (ReferenceEquals(m_cachedGameObject, null))
				{
					m_cachedGameObject = gameObject;
				}

				return ObjectUtility.GetRawObject(m_cachedGameObject);
			}
		}

		private Transform m_cachedTransform = null!;
		public Transform? CachedTransform
		{
			get
			{
				if (ReferenceEquals(m_cachedTransform, null))
				{
					TryGetComponent(out m_cachedTransform);
				}

				return ObjectUtility.GetRawObject(m_cachedTransform);
			}
		}

		private RectTransform m_cachedRectTransform = null!;

		public RectTransform? CachedRectTransform
		{
			get
			{
				if (ReferenceEquals(m_cachedRectTransform, null))
				{
					TryGetComponent(out m_cachedRectTransform);
				}

				return ObjectUtility.GetRawObject(m_cachedRectTransform);
			}
		}
	}
}
