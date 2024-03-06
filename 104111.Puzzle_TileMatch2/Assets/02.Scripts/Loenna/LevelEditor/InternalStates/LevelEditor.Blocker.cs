using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LevelEditor
{
    [Header("★ [Settings] Blocker")]
    public List<BlockerTypeEditor> blockerList = new List<BlockerTypeEditor>()
    {
        BlockerTypeEditor.None,
        BlockerTypeEditor.Glue,
        BlockerTypeEditor.Bush,
    };

    [Header("★ [Live] Blocker")]
    public BlockerTypeEditor blockerType;
    public int blockerCount;
    public int addCount;

    public void UpdateBlockerIndex(int index)
    {
        blockerType = blockerList[index];
        m_presenter.SetUpdateBlocker(blockerCount);
    }

    public void IncrementBlockerCount(int increment)
	{
        blockerCount = Mathf.Max(0, blockerCount + increment);
        m_presenter.SetUpdateBlocker(blockerCount);
	}

    public void IncrementBlockerAddCount(int increment)
	{
        addCount = Mathf.Max(0, addCount + increment);
        m_presenter.SetUpdateBlocker(blockerCount);
	}
}
