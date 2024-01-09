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

    public void HUD_LateUpdate_MainSceneReward(int _getCoin, int _getPuzzlePiece, int _getGoldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", _getCoin, _getPuzzlePiece, _getGoldPiece));

        long _oldCoin = oldCoin;
        int _oldPuzzle = oldPuzzlePiece;
        int _oldGoldPiece = oldGoldPiece;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetIncreaseText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetIncreaseText(_oldCoin);
        if(_getGoldPiece > 0)
            fragmentHome.RefreshGoldPiece(_oldGoldPiece, GetGoldPiece_NextLevel());
        
        float _startDelay = 0.5f;
        float _fxDuration = 1f;

        UniTask.Void(
			async token => {
                mainScene.m_block.SetActive(true);

                if(_getPuzzlePiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
                    
                    if (_startDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                    CreateEffect("UI_Icon_Star", fragmentHome.rewardPosition_puzzlePiece, _fxDuration);
                    CreateEffect("UI_Icon_Star", HUD.behaviour.Fields[0].AttractorTarget, _fxDuration, () => { 
                        HUD?.behaviour.Fields[0].IncreaseText(_oldPuzzle, _getPuzzlePiece, onUpdate:fragmentHome.RefreshPuzzleBadge);
                    });

                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
                }

                if(_getCoin > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[Coin] {0} + {1} = {2}", _oldCoin, _getCoin, _oldCoin + _getCoin));
                    
                    CreateEffect("UI_Icon_Coin", HUD.behaviour.Fields[2].AttractorTarget, _fxDuration, () => {
                        HUD?.behaviour.Fields[2].IncreaseText(_oldCoin, _getCoin);
                    });

                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
                }

                if(_getGoldPiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[GoldPiece] {0} + {1} = {2}", _oldGoldPiece, _getGoldPiece, _oldGoldPiece + _getGoldPiece));
                    
                    CreateEffect("UI_Icon_GoldPuzzle_Big", fragmentHome.rewardPosition_goldPiece, _fxDuration, () => {
                        fragmentHome.IncreaseGoldPiece(_oldGoldPiece, _getGoldPiece, GetGoldPiece_NextLevel());
                    });

                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
                }

                mainScene.m_block.SetActive(false);
            },
			this.GetCancellationTokenOnDestroy()
        );

        void CreateEffect(string spriteName, Transform target, float duration = 1f, Action onComplete = null)
        {
            var parent = fragmentHome.objectPool;

            MissionCollectedFx fx = m_particlePool.Get();
            fx.SetImage(spriteName);
            fx.CachedRectTransform.SetParentReset(parent, true);
            
            Vector2 worldPosition = parent.TransformPoint(Vector2.zero);
            Vector2 position = parent.InverseTransformPoint(worldPosition) / UIManager.SceneCanvas.scaleFactor;
            Vector2 direction = parent.InverseTransformPoint(target.position);
            
            fx.Play(position, direction, duration, () => {
                    soundManager?.PlayFx(Constant.Sound.SFX_GOLD_PIECE);
                    m_particlePool.Release(fx);
                    onComplete?.Invoke();
                }
            );
        }
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
