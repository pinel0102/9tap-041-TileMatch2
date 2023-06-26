using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Palette : IDisposable
{
	public enum ToolType
	{
		NONE,
		PENCIL,
		ERASER
	}

	private AsyncReactiveProperty<ToolType> m_tool;
	private AsyncReactiveProperty<int> m_tileSize;
	private AsyncReactiveProperty<SnapType> m_snapping;

	public IReadOnlyAsyncReactiveProperty<int> TileSize => m_tileSize;
	public IAsyncReactiveProperty<ToolType> Tool => m_tool;
	public IAsyncReactiveProperty<SnapType> Snapping => m_snapping;

	public float SnappingAmount => m_snapping.Value.GetSnappingAmount(m_tileSize.Value);

	public Palette()
	{
		m_tileSize = new(80);
		m_tool = new(ToolType.PENCIL);
		m_snapping = new(SnapType.FULL);
	}

    public void Dispose()
    {
        m_tileSize.Dispose();
		m_tool.Dispose();
		m_snapping.Dispose();
    }
}
