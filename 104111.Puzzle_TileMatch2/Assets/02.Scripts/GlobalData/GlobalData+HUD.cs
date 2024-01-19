using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

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

    public void HUD_LateUpdate_GoldPiece(int _getCount)
    {
        if (_getCount <= 0) return;

        oldGoldPiece += _getCount;
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

    public void HUD_LateUpdate_MainSceneReward(int _clearedLevel, int _openPuzzleIndex, int _getCoin, int _getPuzzlePiece, int _getGoldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} : {1} / {2} / {3} / {4}", _clearedLevel, _openPuzzleIndex, _getCoin, _getPuzzlePiece, _getGoldPiece));

        // [TODO] 수집 이벤트 구현시 사용.
        bool isGoldPieceActivated = false;

        long _oldCoin = oldCoin;
        int _oldPuzzle = oldPuzzlePiece;
        int _oldGoldPiece = oldGoldPiece;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetIncreaseText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetIncreaseText(_oldCoin);
        if(isGoldPieceActivated)
        {
            if(_getGoldPiece > 0)
                fragmentHome.RefreshGoldPiece(_oldGoldPiece, GetGoldPiece_NextLevel());
        }
        
        float _startDelay = 0.5f;
        float _fxDuration = 1f;

        UniTask.Void(
			async token => {
                SetTouchLock_MainScene(true);

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

                if(isGoldPieceActivated)
                {
                    if(_getGoldPiece > 0)
                    {
                        Debug.Log(CodeManager.GetMethodName() + string.Format("[GoldPiece] {0} + {1} = {2}", _oldGoldPiece, _getGoldPiece, _oldGoldPiece + _getGoldPiece));
                        
                        CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.rewardPosition_goldPiece, _fxDuration, () => {
                            fragmentHome.IncreaseGoldPiece(_oldGoldPiece, _getGoldPiece, GetGoldPiece_NextLevel());
                        });

                        await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
                    }
                }

                CheckPuzzleOpen(_openPuzzleIndex);
            },
			this.GetCancellationTokenOnDestroy()
        );
    }

    public void CreateEffect(string spriteName, string soundClip, Transform from, Transform to, float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        MissionCollectedFx fx = m_particlePool.Get();
        fx.SetImage(spriteName);
        fx.CachedRectTransform.SetParentReset(from, true);
        
        Vector2 worldPosition = from.TransformPoint(Vector2.zero);
        Vector2 position = from.InverseTransformPoint(worldPosition) / UIManager.SceneCanvas.scaleFactor;
        Vector2 direction = from.InverseTransformPoint(to.position);
        
        fx.Play(position, direction, duration, () => {
                soundManager?.PlayFx(soundClip);
                m_particlePool.Release(fx);
                onComplete?.Invoke();
            }, sizeFrom, sizeTo
        );
    }

    public void CheckPuzzleOpen(int openPuzzleIndex)
    {   
        if (openPuzzleIndex < 0)
        {
            SetTouchLock_MainScene(false);
            return;
        }

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>Open Puzzle {0}</color>", openPuzzleIndex));

        if (GlobalDefine.IsTutorialPuzzle(openPuzzleIndex))
        {
            ShowTutorial_Puzzle();
        }

        SetTouchLock_MainScene(false);
    }

    public int GetOpenedPuzzleIndex(int clearedLevel)
    {
        var puzzle = tableManager.PuzzleDataTable.Dic.FirstOrDefault(item => item.Value.Level == clearedLevel+1).Value;
        return puzzle?.Index ?? -1;
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
