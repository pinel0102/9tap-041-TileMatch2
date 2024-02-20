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

        eventSweetHolic += _getCount;
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

    public async UniTask HUD_LateUpdate_MainSceneReward(int _clearedLevel, int _openPuzzleIndex, int _getCoin, int _getPuzzlePiece, int _getSweetHolic)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("Level {0} : {1} / {2} / {3} / {4}", _clearedLevel, _openPuzzleIndex, _getCoin, _getPuzzlePiece, _getSweetHolic));

        long _oldCoin = oldCoin;
        int _oldPuzzle = oldPuzzlePiece;
        int _oldSweetHolic = eventSweetHolic;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetIncreaseText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetIncreaseText(_oldCoin);
        if(_getSweetHolic > 0)
            fragmentHome.eventBanner_SweetHolic.SetIncreaseText(_oldSweetHolic);
        
        float _startDelay = 0.5f;
        float _fxDuration = 1f;

        if(_getPuzzlePiece > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
            
            if (_startDelay > 0)
                await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

            CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.objectPool, fragmentHome.rewardPosition_puzzlePiece, _fxDuration);
            CreateEffect("UI_Icon_GoldPuzzle_Big", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.objectPool, HUD.behaviour.Fields[0].AttractorTarget, _fxDuration, () => { 
                HUD?.behaviour.Fields[0].IncreaseText(_oldPuzzle, _getPuzzlePiece, onUpdate:fragmentHome.RefreshPuzzleBadge);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }

        if(_getCoin > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[Coin] {0} + {1} = {2}", _oldCoin, _getCoin, _oldCoin + _getCoin));
            
            CreateEffect("UI_Icon_Coin", Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.objectPool, HUD.behaviour.Fields[2].AttractorTarget, _fxDuration, () => {
                HUD?.behaviour.Fields[2].IncreaseText(_oldCoin, _getCoin);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }

        if(_getSweetHolic > 0)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("[SweetHolic] {0} + {1} = {2}", _oldSweetHolic, _getSweetHolic, _oldSweetHolic + _getSweetHolic));
            
            CreateEffect(GlobalDefine.GetSweetHolic_ItemPath(), Constant.Sound.SFX_GOLD_PIECE, fragmentHome.objectPool, fragmentHome.objectPool, fragmentHome.eventBanner_SweetHolic.targetItemPosition, _fxDuration, () => {
                fragmentHome.eventBanner_SweetHolic.IncreaseText(_oldSweetHolic, _getSweetHolic, onUpdate:fragmentHome.RefreshPuzzleBadge);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(_fxDuration));
        }
    }

    /// <summary>
    /// For MainScene
    /// </summary>
    /// <param name="spriteName"></param>
    /// <param name="soundClip"></param>
    /// <param name="parent"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="onComplete"></param>
    /// <param name="sizeFrom"></param>
    /// <param name="sizeTo"></param>
    public void CreateEffect(string spriteName, string soundClip, Transform parent, Transform from, Transform to, float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        MissionCollectedFx fx = m_particlePool.Get();
        fx.SetImage(spriteName);
        fx.CachedRectTransform.SetParentReset(parent, true);
        
        //Vector2 worldPosition = from.TransformPoint(Vector2.zero);
        //Vector2 position = from.InverseTransformPoint(worldPosition) / UIManager.SceneCanvas.scaleFactor;
        //Vector2 direction = from.InverseTransformPoint(to.position);
        
        fx.Play(from.position, to.position, duration, () => {
                soundManager?.PlayFx(soundClip);
                m_particlePool.Release(fx);
                onComplete?.Invoke();
            }, sizeFrom, sizeTo
        );
    }

    /// <summary>
    /// For GameScene
    /// </summary>
    /// <param name="fx"></param>
    /// <param name="spriteName"></param>
    /// <param name="soundClip"></param>
    /// <param name="parent"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="onComplete"></param>
    /// <param name="sizeFrom"></param>
    /// <param name="sizeTo"></param>
    public void CreateEffect(UnityEngine.Pool.IObjectPool<MissionCollectedFx> particlePool, string spriteName, string soundClip, Transform parent, Transform from, Transform to, float duration = 1f, Action onComplete = null, float sizeFrom = 70f, float sizeTo = 82f)
    {
        MissionCollectedFx fx = particlePool.Get();
        fx.SetImage(spriteName);
        fx.CachedRectTransform.SetParentReset(parent, true);
        
        //Vector2 worldPosition = from.TransformPoint(Vector2.zero);
        //Vector2 position = from.InverseTransformPoint(worldPosition) / UIManager.SceneCanvas.scaleFactor;
        //Vector2 direction = from.InverseTransformPoint(to.position);
        
        fx.Play(from.position, to.position, duration, () => {
                soundManager?.PlayFx(soundClip);
                particlePool.Release(fx);
                onComplete?.Invoke();
            }, sizeFrom, sizeTo
        );
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
