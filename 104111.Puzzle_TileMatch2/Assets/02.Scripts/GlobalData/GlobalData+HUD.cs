using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public partial class GlobalData
{
    public HUD HUD => Game.Inst.Get<HUD>();
    
    public void HUD_LateUpdate_Coin(int _getCount)
    {
        if (_getCount <= 0) return;

        HUD_LateUpdate(2, oldCoin, _getCount, 0, 0.5f);
        oldCoin += _getCount;
    }

    public void HUD_LateUpdate_Puzzle(int _getCount)
    {
        if (_getCount <= 0) return;

        HUD_LateUpdate(0, oldPuzzlePiece, _getCount, 0, 0.5f);
        oldPuzzlePiece += _getCount;
    }

    public void HUD_LateUpdate_GoldPiece(int _getCount)
    {
        if (_getCount <= 0) return;

        oldGoldPiece += _getCount;
    }

    private void HUD_LateUpdate(int _index, long _oldCount, int _getCount, float _startDelay, float _duration)
    {
        if (_getCount <= 0) return;

        HUD?.behaviour.Fields[_index].SetText(_oldCount);

        UniTask.Void(
			async token => {
                Debug.Log(CodeManager.GetMethodName() + string.Format("[{0}] {1} + {2} = {3}", _index, _oldCount, _getCount, _oldCount + _getCount));
                
                float delay = GetDelay(_duration, _getCount);

                if (_startDelay > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                for(int i=0; i < _getCount; i++)
                {
                    HUD?.behaviour.Fields[_index].SetText(_oldCount + i);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }

                HUD?.behaviour.Fields[_index].SetText(_oldCount + _getCount);

            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }
    }

    public void HUD_LateUpdate_MainSceneReward(MainScene mainScene, int _getCoin, int _getPuzzlePiece, int _getGoldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", _getCoin, _getPuzzlePiece, _getGoldPiece));

        var fragmentHome = mainScene.scrollView.Contents[(int)MainMenuType.HOME] as MainSceneFragmentContent_Home;

        long _oldCoin = oldCoin;
        long _oldPuzzle = oldPuzzlePiece;
        long _oldGoldPiece = oldGoldPiece;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetText(_oldCoin);
        if(_getGoldPiece > 0)
            fragmentHome.RefreshGoldPiece(_oldGoldPiece, GetGoldPiece_NextLevel());

        float _startDelay = 0.5f;
        float _fxDuration = 1f;
        float _numDuration = 0.5f;

        UniTask.Void(
			async token => {
                mainScene.m_block.SetActive(true);

                if(_getPuzzlePiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
                    
                    if (_startDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                    CreateEffect("UI_Icon_Star", fragmentHome.rewardPosition_puzzlePiece, _fxDuration);
                    CreateEffect("UI_Icon_Star", HUD.behaviour.Fields[0].AttractorTarget, _fxDuration);
                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));

                    int count = _getPuzzlePiece;
                    float delay = GetDelay(_numDuration, count);

                    for(int i=0; i < count; i++)
                    {
                        HUD?.behaviour.Fields[0].SetText(_oldPuzzle + i);
                        fragmentHome.RefreshPuzzleBadge(_oldPuzzle + count);
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }

                    HUD?.behaviour.Fields[0].SetText(_oldPuzzle + count);
                    fragmentHome.RefreshPuzzleBadge(_oldPuzzle + count);
                }

                if(_getCoin > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[Coin] {0} + {1} = {2}", _oldCoin, _getCoin, _oldCoin + _getCoin));
                    
                    CreateEffect("UI_Icon_Coin", HUD.behaviour.Fields[2].AttractorTarget, _fxDuration);
                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));

                    int count = _getCoin;
                    float delay = GetDelay(_numDuration, count);

                    for(int i=0; i < count; i++)
                    {
                        HUD?.behaviour.Fields[2].SetText(_oldCoin + i);
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }

                    HUD?.behaviour.Fields[2].SetText(_oldCoin + count);
                }

                if(_getGoldPiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[GoldPiece] {0} + {1} = {2}", _oldGoldPiece, _getGoldPiece, _oldGoldPiece + _getGoldPiece));
                    
                    CreateEffect("UI_Icon_GoldPuzzle_Big", fragmentHome.rewardPosition_goldPiece, _fxDuration);
                    await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));

                    int count = _getGoldPiece;
                    float delay = GetDelay(_numDuration, count);

                    for(int i=0; i < count; i++)
                    {
                        fragmentHome.RefreshGoldPiece(_oldGoldPiece + i, GetGoldPiece_NextLevel());
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }

                    fragmentHome.RefreshGoldPiece(_oldGoldPiece + count, GetGoldPiece_NextLevel());
                }

                mainScene.m_block.SetActive(false);
            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }

        void CreateEffect(string spriteName, Transform target, float duration = 1f)
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
