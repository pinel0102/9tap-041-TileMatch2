using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NineTap.Common;
using TMPro;
using Coffee.UIExtensions;

public record PuzzleCompletePopupParameter(
    int Index, 
    string PuzzleName, 
    Texture2D Background, 
    CurrentPlayingPuzzleContent Content,
    Action OnContinue
): DefaultParameterWithoutHUD;

[ResourcePath("UI/Popup/PuzzleCompletePopup")]
public class PuzzleCompletePopup : UIPopup
{
    [SerializeField]
	private RawImage m_backgroundImage = default!;

    [SerializeField]
	private TMP_Text m_puzzleNameText = default!;

    [SerializeField]
	private Button m_backgroundButton = default!;

    [SerializeField]
	private GameObject m_touchLock = default!;

    [SerializeField]
	private GameObject m_closeText = default!;

    [SerializeField]
    private RectTransform slotParent;

    [SerializeField]
    private UIParticle FXLayer;

    private List<Transform> m_slots = new();
    private JigsawPuzzlePiece m_piecePrefab;
    private CurrentPlayingPuzzleContent m_content;

    public int PuzzleIndex;
    public string PuzzleName = string.Empty;
    private bool existReward;
    private RewardData rewardData;

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not PuzzleCompletePopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        RewardDataTable rewardDataTable = GlobalData.Instance.tableManager.RewardDataTable;
        existReward = rewardDataTable.TryPuzzleCompleteReward(out rewardData);

        PuzzleIndex = parameter.Index;
        PuzzleName = parameter.PuzzleName;
        m_content = parameter.Content;
        m_piecePrefab = ResourcePathAttribute.GetResource<JigsawPuzzlePiece>();
        m_backgroundImage.texture = parameter.Background;
        m_puzzleNameText.SetText(PuzzleName);
        m_backgroundButton.onClick.AddListener(() => { OnClick_Close(parameter.OnContinue); });
        m_touchLock.SetActive(true);
        m_closeText.SetActive(false);

        CreateSlot();
        CreatePuzzle();
        ClearFXLayer();
    }

    public override void OnShow()
    {
        base.OnShow();

        m_touchLock.SetActive(true);
        
        GlobalData.Instance.mainScene.LoadFXLocal(GlobalDefine.FX_Prefab_Confetti, Vector3.zero, FXLayer);
        GlobalData.Instance.soundManager?.PlayFx(Constant.Sound.SFX_PUZZLE_COMPLETE);

        GetReward();
    }

    private void OnClick_Close(Action onComplete)
    {
        OnClickClose();        
        UIManager.HUD?.Show(HUDType.ALL);

        onComplete?.Invoke();
    }

    private void CreateSlot()
    {
        m_slots.Clear();
        for (int i = 0; i < PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; i++)
		{
            GameObject go = new GameObject($"Slot_{i}");
            Image image = go.AddComponent<Image>();
			image.color = Color.clear;
            Transform trans = go.transform;
            trans.SetParentReset(slotParent);
			m_slots.Add(trans);
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(slotParent);
    }

    private void CreatePuzzle()
    {
        float ratio = m_backgroundImage.rectTransform.rect.width / JigsawPuzzleSetting.Instance.ImageSize;
		int pieceCost = m_content.PieceCost;
        var pieceSources = m_content.PieceSources;
		
        for (int index = 0; index < PuzzlePieceMaker.MAX_PUZZLE_PIECE_COUNT; index++)
		{
			PuzzlePieceSource pieceSource = pieceSources[index];
			int realIndex = pieceSource.Index;
            bool placed = true;

			PuzzlePieceItemData pieceData = new PuzzlePieceItemData { 
				Index = realIndex,
                Cost = pieceCost,
				Sprite = pieceSources[index].Sprite,
                Filter = pieceSources[index].Filter,
				Size = JigsawPuzzleSetting.Instance.PieceSizeWithPadding * ratio,
                PuzzleCurveTypes = pieceSources[index].PuzzleCurveTypes,
				OnTryUnlock = null
			};

            JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);
            piece.name = $"piece[{realIndex:00}]";
            piece.OnSetup(pieceData, placed);

            piece.CachedTransform.SetParentReset(m_slots[realIndex]);
		}
    }

    private void GetReward()
    {
        UniTask.Void(
			async token => {
                if (existReward)
                {
                    Dictionary<ProductType, long> collectRewardAll = new Dictionary<ProductType, long>();
                    GlobalDefine.AddRewards(collectRewardAll, rewardData.Rewards);
                    GlobalDefine.UpdateRewards(collectRewardAll);

                    await UniTask.Delay(TimeSpan.FromSeconds(2f));
                    
				    GlobalData.Instance.ShowPresentPopup(rewardData);
                }

                m_touchLock.SetActive(false);
                m_closeText.SetActive(true);
			},
			this.GetCancellationTokenOnDestroy()
		);
    }

    private void ClearFXLayer()
    {
        GlobalDefine.ClearChild(FXLayer.transform);
        FXLayer.RefreshParticles();
    }
}
