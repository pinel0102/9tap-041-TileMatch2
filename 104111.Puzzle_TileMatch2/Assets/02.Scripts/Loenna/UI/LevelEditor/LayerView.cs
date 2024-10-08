using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using static LevelEditor;

public class LayerView : MonoBehaviour
{
    [SerializeField]
	private int m_layerIndex;
	[SerializeField]
	private CanvasGroup m_canvasGroup;
	private Queue<GameObject> m_pools = new();

	public void Draw(TileBrush tilePrefab, LayerInfo layer, int layerIndex)
	{
        m_layerIndex = layerIndex;

		if (layer?.Tiles == null)
		{
			return;
		}

		foreach (var (_, position, size, blockerType, blockerICD) in layer.Tiles)
		{
			Draw(tilePrefab, position, size, layer.Color, blockerType, blockerICD);
		}
	}

	public void Draw(TileBrush tilePrefab, Vector2 position, float size, Color color, BlockerType blockerType, int blockerICD)
	{
        GameObject go = m_pools.Count > 0 ? m_pools.Dequeue() : Instantiate(tilePrefab.gameObject, transform);
        
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
        go.SetActive(true);

		if (!go.TryGetComponent(out Outline outline))
		{
			outline = go.AddComponent<Outline>();
		}

		outline.effectColor = color * 0.5f;
		outline.effectDistance = new Vector2(2f, -2f);
		
		TileBrush tile = go.GetComponent<TileBrush>();
		tile.UpdateColor(color, blockerType, blockerICD, GlobalDefine.IsBlockerICD_Variable(blockerType));

		RectTransform rectTransform = go.transform as RectTransform;
		rectTransform.localPosition = position;
		rectTransform.SetSize(size);
	}

	public void Clear()
	{
        m_pools.Clear();

		for (int index = 0, count = transform.childCount; index < count; index++)
		{
			var child = transform.GetChild(index);
			if (child.TryGetComponent<TileBrush>(out var tile))
			{
				tile.gameObject.SetActive(false);
				m_pools.Enqueue(tile.gameObject);
			}
		}
	}

	public void OnVisible(bool visible)
	{
		m_canvasGroup.alpha = visible? 1f : 0f;
	}
}
