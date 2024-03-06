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
    public int blockerICD = 3;

    public void UpdateBlockerIndex(int index)
    {
        blockerType = blockerList[index];
        m_presenter.SetUpdateBlocker(blockerCount);
    }

    public void UpdateBlockerCount(int count)
    {
        blockerCount = count;
        m_presenter.SetUpdateBlocker(blockerCount);
    }

    public void UpdateBlockerICD(int count)
    {
        blockerICD = count;
        m_presenter.SetUpdateBlockerICD(blockerCount);
    }

    public void IncrementBlockerCount(int increment)
	{
        blockerCount = Mathf.Max(0, blockerCount + increment);
        m_presenter.SetUpdateBlocker(blockerCount);
	}

    public void IncrementBlockerICD(int increment)
	{
        blockerICD = Mathf.Max(1, blockerICD + increment);
        m_presenter.SetUpdateBlockerICD(blockerCount);
	}

    public bool BlockerHasICD(BlockerTypeEditor blocker)
    {
        switch(blocker)
        {
            case BlockerTypeEditor.Suitcase:
                return true;
            default:
                return false;
        }
    }
}
