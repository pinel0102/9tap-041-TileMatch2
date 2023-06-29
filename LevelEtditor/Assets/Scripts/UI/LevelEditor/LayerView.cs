using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class LayerView : MonoBehaviour
{
	private readonly List<GameObject> m_placedTiles = new();
	private readonly Queue<GameObject> m_pools = new();

	public static LayerView CreateLayerView(Transform parent)
	{
		GameObject go = new GameObject("Layer");
		RectTransform rectTransform = go.AddComponent<RectTransform>();
		rectTransform.SetParent(parent, false);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.localScale = Vector3.one;

		LayerView result = go.AddComponent<LayerView>();
		return result;
	}

	public void Draw(IEnumerable<(Vector2 position, float size)> tiles)
	{
		if (tiles == null)
		{
			return;
		}

		foreach (var (position, size) in tiles)
		{
			Draw(position, size);
		}
	}

	public void Draw(Vector2 position, float size)
	{
		GameObject go = m_pools.Count > 0? m_pools.Dequeue() : new GameObject("Tile");
		go.SetActive(true);
		go.transform.SetParent(transform);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		
		if (!go.TryGetComponent<Image>(out Image image))
		{
			image = go.AddComponent<Image>();
		}
		image.color = Color.green;

		RectTransform rectTransform = go.transform as RectTransform;
		rectTransform.localPosition = position;
		rectTransform.SetSize(size);

		m_placedTiles.Add(go);
	}

	public void Erase(Vector2 position)
	{
		if (m_placedTiles.Count > 0)
		{
			GameObject minObject = m_placedTiles[0];
			float minDistance = Vector2.Distance(minObject.transform.localPosition, position);

			m_placedTiles.ForEach(tile => {
					float distance = Vector2.Distance(tile.transform.localPosition, position);
					if (minDistance > distance)
					{
						minObject = tile;
						minDistance = distance;
					}
				}
			);

			minObject.SetActive(false);
			m_pools.Enqueue(minObject);
			m_placedTiles.RemoveAll(tile => ReferenceEquals(minObject, tile));
		}
	}

	public void Clear()
	{
		m_placedTiles.ForEach(
			tile => {
				tile.SetActive(false);
				m_pools.Enqueue(tile);
			}
		);

		m_placedTiles.Clear();
	}

}
