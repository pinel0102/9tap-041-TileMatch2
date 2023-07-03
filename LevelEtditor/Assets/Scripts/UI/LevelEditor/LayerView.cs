using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using static LevelEditor;

public class LayerView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private List<GameObject> m_placedTiles = new();
	private Queue<GameObject> m_pools = new();

	public void Draw(LayerInfo layer)
	{
		if (layer?.Tiles == null)
		{
			return;
		}

		foreach (var (position, size) in layer.Tiles)
		{
			Draw(position, size, layer.Color);
		}
	}

	public void Draw(Vector2 position, float size, Color color)
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

		if (!go.TryGetComponent<Outline>(out Outline outline))
		{
			outline = go.AddComponent<Outline>();
		}

		outline.effectColor = color * 0.5f;
		outline.effectDistance = new Vector2(2f, -2f);
		image.color = color;

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
		m_placedTiles.ForEach(tile => tile.SetActive(false));
		m_pools = new Queue<GameObject>(m_placedTiles.ToArray());
		m_placedTiles.Clear();
	}

	public void OnVisible(bool visible)
	{
		m_canvasGroup.alpha = visible? 1f : 0f;
	}
}
