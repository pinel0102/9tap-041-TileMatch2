using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NineTap.Common;
using TMPro;

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
	private GameObject m_effect = default!;

    [SerializeField]
	private GameObject m_touchLock = default!;

    [SerializeField]
    private RectTransform slotParent;

    private List<Transform> m_slots = new();
    private JigsawPuzzlePiece m_piecePrefab;
    private CurrentPlayingPuzzleContent m_content;

    public int PuzzleIndex;
    public string PuzzleName = string.Empty;

    public override void OnSetup(UIParameter uiParameter)
    {
        base.OnSetup(uiParameter);

		if (uiParameter is not PuzzleCompletePopupParameter parameter)
		{
			OnClickClose();
			return;
		}

        //TableManager tableManager = Game.Inst.Get<TableManager>();
		//RewardDataTable rewardDataTable = tableManager.RewardDataTable;
		//LevelDataTable levelDataTable = tableManager.LevelDataTable;

        PuzzleIndex = parameter.Index;
        PuzzleName = parameter.PuzzleName;
        m_content = parameter.Content;
        m_piecePrefab = ResourcePathAttribute.GetResource<JigsawPuzzlePiece>();
        m_backgroundImage.texture = parameter.Background;
        m_puzzleNameText.SetText(PuzzleName);
        m_backgroundButton.onClick.AddListener(() => { OnClick_Close(parameter.OnContinue); });
        m_effect.SetActive(false);
        m_touchLock.SetActive(true);

        CreateSlot();
        CreatePuzzle();
    }

    public override void OnShow()
    {
        base.OnShow();

        WaitTime();
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
                SpriteAttached = pieceSources[index].SpriteAttached,
				Size = JigsawPuzzleSetting.Instance.PieceSizeWithPadding * ratio,
				OnTryUnlock = null
			};

            JigsawPuzzlePiece piece = Instantiate(m_piecePrefab);
            piece.name = $"piece[{realIndex:00}]";
            piece.OnSetup(pieceData, placed);

            piece.CachedTransform.SetParentReset(m_slots[realIndex]);
		}
    }

    private void WaitTime()
    {
        m_touchLock.SetActive(true);
        m_effect.SetActive(true);

        UniTask.Void(
			async token => {                
                await UniTask.Delay(TimeSpan.FromSeconds(2f));
				m_touchLock.SetActive(false);
			},
			this.GetCancellationTokenOnDestroy()
		);
    }
}
