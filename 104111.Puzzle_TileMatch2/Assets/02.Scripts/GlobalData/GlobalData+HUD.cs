using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public partial class GlobalData
{
    public void HUD_LateUpdate_Coin(int _getCount, float _startDelay = 0, bool autoTurnOff_IncreaseMode = true)
    {
        if (_getCount <= 0) return;

        HUD_LateUpdate(2, oldCoin, _getCount, _startDelay, autoTurnOff_IncreaseMode);
        oldCoin += _getCount;
    }

    public void HUD_LateUpdate_Puzzle(int _getCount, float _startDelay = 0, bool autoTurnOff_IncreaseMode = true)
    {
        if (_getCount <= 0) return;

        HUD_LateUpdate(0, oldPuzzlePiece, _getCount, _startDelay, autoTurnOff_IncreaseMode);
        oldPuzzlePiece += _getCount;
    }

    public void HUD_LateUpdate_EventItem(int _getCount)
    {
        if (_getCount <= 0) return;
    }

    private void HUD_LateUpdate(int _index, long _oldCount, int _getCount, float _startDelay = 0, bool autoTurnOff_IncreaseMode = true)
    {
        if (_getCount > 0)
            HUD?.behaviour.Fields[_index].SetIncreaseText(_oldCount);

        UniTask.Void(
			async token => {
                Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] {1} + {2} = {3}", _index, _oldCount, _getCount, _oldCount + _getCount));

                if (_startDelay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                HUD?.behaviour.Fields[_index].IncreaseText(_oldCount, _getCount, autoTurnOff_IncreaseMode:autoTurnOff_IncreaseMode);
            },
			this.GetCancellationTokenOnDestroy()
        );
    }

    public async UniTask<bool> HUD_LateUpdate_MainSceneReward(int _clearedLevel, int _openPuzzleIndex, int _getCoin, int _getPuzzlePiece, int _getSweetHolicExp)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} : {1} / {2} / {3} / {4}", _clearedLevel, _openPuzzleIndex, _getCoin, _getPuzzlePiece, _getSweetHolicExp));

        long _oldCoin = oldCoin;
        int _oldPuzzle = oldPuzzlePiece;
        int _oldSweetHolicExp = oldSweetHolicExp;

#if UNITY_EDITOR
        if(eventSweetHolic_TestMode)
            _getSweetHolicExp = eventSweetHolic_TestExp;
#endif
        if(tableManager.EventDataTable.IsMaxLevel(GameEventType.SweetHolic, fragmentHome.eventBanner_SweetHolic.currentLevel))
            _getSweetHolicExp = 0;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetIncreaseText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetIncreaseText(_oldCoin);
        if(_getSweetHolicExp > 0)
            fragmentHome.eventBanner_SweetHolic.eventSlider.SetIncreaseText(_oldSweetHolicExp);
        
        float _startDelay = 0.5f;
        float _fxDuration = 1f;

        if(_getPuzzlePiece > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
            
            if (_startDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

            CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.rewardPosition_puzzlePiece, _fxDuration);
            CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, HUD.behaviour.Fields[0].AttractorTarget, _fxDuration, () => { 
                HUD?.behaviour.Fields[0].IncreaseText(_oldPuzzle, _getPuzzlePiece, onUpdate:fragmentHome.RefreshPuzzleBadge);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }

        if(_getCoin > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[Coin] {0} + {1} = {2}", _oldCoin, _getCoin, _oldCoin + _getCoin));
            
            CreateEffect("UI_Icon_Coin", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, HUD.behaviour.Fields[2].AttractorTarget, _fxDuration, () => {
                HUD?.behaviour.Fields[2].IncreaseText(_oldCoin, _getCoin);
            });
            
            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }

        if(_getSweetHolicExp > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[SweetHolic] {0} + {1} = {2}", _oldSweetHolicExp, _getSweetHolicExp, _oldSweetHolicExp + _getSweetHolicExp));

            bool increaseExpFinished = false;
            
            CreateEffect(GlobalDefine.GetSweetHolic_ItemImagePath(), Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.eventBanner_SweetHolic.targetItemPosition, _fxDuration, async () => {
                increaseExpFinished = await fragmentHome.eventBanner_SweetHolic.eventSlider.IncreaseText(_oldSweetHolicExp, _getSweetHolicExp);
            });

            await UniTask.WaitUntil(() => increaseExpFinished);
            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }

        return true;
    }

    public void HUD_Show(params HUDType[] types)
    {
        HUD?.Show(types);
    }

    public void HUD_Hide()
    {
        HUD?.Hide();
    }
}
