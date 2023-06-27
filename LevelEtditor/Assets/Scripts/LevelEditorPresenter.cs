// #define ENABLE_DEBUG_LOG
using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public record BrushInfo
(
	float Size,
	float Snapping,
	Vector2 Position
)
{
	public Bounds GetBounds() => new Bounds(Position, Vector2.one * Size);

	public bool Overlap(Bounds other)
	{
		Bounds bounds = GetBounds();
		return bounds.min.x >= other.min.x &&
			bounds.min.y >= other.min.y &&
			bounds.max.x <= other.max.x &&
			bounds.max.y <= other.max.y;
	}
}

public record BrushWidgetInfo
(
	bool Interactable,
	bool Drawable,
	Vector2 LocalPosition
);

public class LevelEditorPresenter : IDisposable
{
	private readonly LevelDataManager m_dataManager;
	private readonly LevelEditor m_view;
	private readonly Bounds m_boardBounds;
	private readonly List<Bounds> m_placedTileBounds;
	private readonly AsyncReactiveProperty<BrushInfo> m_brushInfo;
	private readonly AsyncMessageBroker<BrushWidgetInfo> m_messageBroker = new();
	
	public IUniTaskAsyncEnumerable<BrushWidgetInfo> BrushBroker => m_messageBroker.Subscribe();

	public LevelEditorPresenter(LevelEditor view, float cellSize, float cellCount)
	{
		m_dataManager = new LevelDataManager();
		m_view = view;
		m_boardBounds = new Bounds(Vector2.zero, Vector2.one * cellSize * cellCount);
		m_brushInfo = new(new BrushInfo(cellSize, cellSize, Position: Vector2.zero));
		m_placedTileBounds = new();

		m_brushInfo.Subscribe(
			info =>
			{
				m_messageBroker.Publish(
					new BrushWidgetInfo(
						Interactable: info.Overlap(m_boardBounds),
						Drawable: m_placedTileBounds.All(
							placed => placed.SqrDistance(info.Position) >= Mathf.Pow(info.Size * 0.5f, 2f)
						),
						LocalPosition: info.Position
					)
				);
			}
		);

		#if ENABLE_DEBUG_LOG
		UniTask.Void( 
			async () =>
			{
				await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate))
				{
					Bounds brushBounds = new(m_brushInfo.Value.Position, Vector2.one * m_brushInfo.Value.Size);
					OnDebugDraw(m_boardBounds, Color.yellow);
					OnDebugDraw(brushBounds, Color.green);
					m_placedTileBounds.ForEach(bounds => OnDebugDraw(bounds, Color.red));
				}
			}
		);

		void OnDebugDraw(Bounds bounds, Color color)
		{
			Vector2 vMin = bounds.min;
			Vector2 vMax = bounds.max;

			var p1 = new Vector2(vMin.x, vMin.y);
			var p2 = new Vector2(vMax.x, vMin.y);
			var p3 = new Vector2(vMax.x, vMax.y);
			var p4 = new Vector2(vMin.x, vMax.y);

			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p2, p3, color);
			Debug.DrawLine(p3, p4, color);
			Debug.DrawLine(p4, p1, color);
		}
		#endif
	}

	public void SetBrushPosition(Vector2 inputPosition)
	{
		var (x, y) = inputPosition;
		BrushInfo info = m_brushInfo.Value;
		float snapping = info.Snapping;

		Vector2 positionWithSnapping = new Vector2(
			Mathf.RoundToInt(x / snapping) * snapping, 
			Mathf.RoundToInt(y / snapping) * snapping
		);

		m_brushInfo.Value = info with { Position = positionWithSnapping };
	}

	public void ChangeSnapping(float snapping)
	{
		BrushInfo info = m_brushInfo.Value;
		m_brushInfo.Value = info with { Snapping = snapping };
	}

	public void AddTileInLayer()
	{
		(float size, _, Vector2 position) = m_brushInfo.Value;

		m_placedTileBounds.Add(new Bounds(position, Vector2.one * size));
		m_dataManager.AddTileData(position);
		m_view.OnDrawCell(position);
	}

	public void AllClearTiles()
	{
		m_placedTileBounds.Clear();
		m_dataManager.ClearTileDatasInLayer();
		m_view.ClearTilesInLayer();
	}

	public void Dispose()
	{
		m_brushInfo.Dispose();
		m_messageBroker.Dispose();
		m_placedTileBounds.Clear();
	}
}
