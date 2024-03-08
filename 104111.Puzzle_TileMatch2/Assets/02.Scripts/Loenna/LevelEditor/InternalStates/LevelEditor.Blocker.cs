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
    public int blockerCount = 1;
    /// <summary>
    /// 에디터에서 보드에 설치 또는 추가할 Blocker의 가변 ICD. (가변 ICD 사용 항목만 적용.)
    /// </summary>
    public int blockerVariableICD = 3;
    /// <summary>
    /// 에디터에서 Blocker를 수정할 레이어 인덱스. (-1 : All)
    /// </summary>
    public int blockerTargetLayer = -1;

    public static Dictionary<BlockerType, int> CurrentBlockerDic = new Dictionary<BlockerType, int>();

    /// <summary>
    /// 여러 개의 타일 개수를 가지는 Blocker의 추가 타일 개수를 카운팅.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public int GetAdditionalTileCount(BoardInfo board)
    {
        int additionalCount = 0;

        var layers = board.Layers.ToList();
        layers.ForEach(layer => {
            additionalCount += layer.Tiles
                .Where(tile => tile.blockerType == BlockerType.Suitcase)
                .Sum(tile => {  return tile.blockerICD - 1; });
        });

        if (additionalCount > 0)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", additionalCount));

        return additionalCount;
    }

#region Presenter Function

    private void UpdateBlockerLayer(int index)
    {
        blockerTargetLayer = Mathf.Max(-1, index - 1);
        m_presenter.SetUpdateBoard();
    }

    private void UpdateBlockerIndex(int index)
    {
        blockerType = blockerList[index];
        m_presenter.SetUpdateBoard();
    }

    private void UpdateBlockerCount(int count)
    {
        blockerCount = count;
        m_presenter.SetUpdateBoard();
    }

    private void UpdateBlockerICD(int count)
    {
        blockerVariableICD = Mathf.Max(1, count);
        m_presenter.SetUpdateBoard();
    }

    private void IncrementBlockerCount(int increment)
	{
        blockerCount = Mathf.Max(0, blockerCount + increment);
        m_presenter.SetUpdateBoard();
	}

    private void IncrementBlockerICD(int increment)
	{
        blockerVariableICD = Mathf.Max(1, blockerVariableICD + increment);
        m_presenter.SetUpdateBoard();
	}

#endregion Presenter Function
}
