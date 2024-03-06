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

    /// <summary>
    /// <para>ICD를 사용하는 Blocker : _blockerICD.</para>
    /// <para>ICD를 사용하지 않는 Blocker : 0.</para>
    /// </summary>
    /// <param name="_blockerType"></param>
    /// <param name="_blockerICD"></param>
    /// <returns></returns>
    public int GetBlockerICD(BlockerTypeEditor _blockerType, int _blockerICD)
    {
        if (HasBlockerICD(_blockerType))
            return _blockerICD;
        else 
            return 0;
    }

    public bool HasBlockerICD(BlockerTypeEditor _blockerType)
    {
        switch(_blockerType)
        {
            case BlockerTypeEditor.Suitcase:
                return true;
            default:
                return false;
        }
    }
}
