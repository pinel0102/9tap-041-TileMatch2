using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LevelEditor
{
    [Header("★ [Live] Blocker")]
    public BlockerTypeEditor blockerType;
    public int blockerCount;

    [Header("★ [Settings] Blocker")]
    public List<BlockerTypeEditor> blockerList = new List<BlockerTypeEditor>()
    {
        BlockerTypeEditor.None,
        BlockerTypeEditor.Glue,
        BlockerTypeEditor.Bush,
    };

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
}
