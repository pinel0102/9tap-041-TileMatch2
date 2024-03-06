using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class LevelEditor
{
    [Header("★ [Settings] Blocker Enable List")]
    public List<BlockerTypeEditor> blockerList = new List<BlockerTypeEditor>()
    {
        BlockerTypeEditor.None,
        BlockerTypeEditor.Glue,
        BlockerTypeEditor.Bush,
    };

    /// <summary>
    /// 에디터에서 보드에 설치 또는 추가할 Blocker 타입.
    /// </summary>
    [Header("★ [Live] Blocker")]
    public BlockerTypeEditor blockerType;
    /// <summary>
    /// 에디터에서 보드에 설치 또는 추가할 Blocker 개수.
    /// </summary>
    public int blockerCount;
    /// <summary>
    /// 에디터에서 보드에 설치 또는 추가할 Blocker의 가변 ICD. (가변 ICD 사용 항목만 적용.)
    /// </summary>
    public int blockerVariableICD = 3;

#region Presenter Function

    private void UpdateBlockerIndex(int index)
    {
        blockerType = blockerList[index];
        m_presenter.SetUpdateBlocker(blockerCount);
    }

    private void UpdateBlockerCount(int count)
    {
        blockerCount = count;
        m_presenter.SetUpdateBlocker(blockerCount);
    }

    private void UpdateBlockerICD(int count)
    {
        blockerVariableICD = Mathf.Max(1, count);
        m_presenter.SetUpdateBlockerICD(blockerCount);
    }

    private void IncrementBlockerCount(int increment)
	{
        blockerCount = Mathf.Max(0, blockerCount + increment);
        m_presenter.SetUpdateBlocker(blockerCount);
	}

    private void IncrementBlockerICD(int increment)
	{
        blockerVariableICD = Mathf.Max(1, blockerVariableICD + increment);
        m_presenter.SetUpdateBlockerICD(blockerCount);
	}

#endregion Presenter Function
}
