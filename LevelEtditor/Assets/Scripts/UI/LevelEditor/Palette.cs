using System;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class Palette : IDisposable
{
	public enum ToolType
	{
		NONE,
		PENCIL,
		ERASER
	}

	private readonly int m_tileSize;

	private readonly AsyncReactiveProperty<ToolType> m_tool;
	private readonly AsyncReactiveProperty<SnapType> m_snapping;
	private readonly AsyncMessageBroker<float> m_messageBroker;

	public IAsyncReactiveProperty<ToolType> Tool => m_tool;
	public IAsyncReactiveProperty<SnapType> Snapping => m_snapping;

	public IUniTaskAsyncEnumerable<float> Subscriber => m_messageBroker.Subscribe();

	public Palette(int tileSize)
	{
		m_tileSize = tileSize;
		m_tool = new(ToolType.PENCIL);
		m_snapping = new(SnapType.FULL);
		m_messageBroker = new();

		m_snapping.Subscribe(snapping => m_messageBroker.Publish(snapping.GetSnappingAmount(tileSize)));
		m_messageBroker.Publish(tileSize);
	}

    public void Dispose()
    {
		m_tool.Dispose();
		m_snapping.Dispose();
    }
}
