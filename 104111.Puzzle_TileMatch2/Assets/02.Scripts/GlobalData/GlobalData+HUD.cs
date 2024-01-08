using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;
using NineTap.Common;

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
        //if(_goldPiece > 0)

        float _startDelay = 0;
        float _duration = 0.5f;

        UniTask.Void(
			async token => {
                if(_getPuzzlePiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
                    
                    if (_startDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                    var widget = Instantiate(ResourcePathAttribute.GetResource<IconReward>());
					widget.OnSetup("UI_Icon_Star");
                    widget.CachedRectTransform.SetParentReset(fragmentHome.CachedRectTransform);
                    widget.CachedRectTransform.SetSize(80);

                    var widget2 = Instantiate(widget);
                    widget2.OnSetup("UI_Icon_Star");
                    widget2.CachedRectTransform.SetParentReset(fragmentHome.CachedRectTransform);
                    widget2.CachedRectTransform.SetSize(80);

                    float duration = 1f;

                    await UniTask.WhenAll(
                        AsyncMove(widget.CachedRectTransform, fragmentHome.rewardPosition_puzzlePiece.position, duration),
                        AsyncMove(widget2.CachedRectTransform, HUD.behaviour.Fields[0].AttractorTarget.position, duration)
                    );

                    Destroy(widget.gameObject);
                    Destroy(widget2.gameObject);

                    int count = _getPuzzlePiece;
                    float delay = GetDelay(_duration, count);

                    for(int i=0; i < count; i++)
                    {
                        HUD?.behaviour.Fields[0].SetText(_oldPuzzle + i);
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }

                    HUD?.behaviour.Fields[0].SetText(_oldPuzzle + count);
                }

                if(_getCoin > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[Coin] {0} + {1} = {2}", _oldCoin, _getCoin, _oldCoin + _getCoin));
                    
                    if (_startDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                    var widget = Instantiate(ResourcePathAttribute.GetResource<IconReward>());
					widget.OnSetup("UI_Icon_Coin");
                    widget.CachedRectTransform.SetParentReset(fragmentHome.CachedRectTransform);
                    widget.CachedRectTransform.SetSize(80);

                    float duration = 1f;

                    await AsyncMove(widget.CachedRectTransform, HUD.behaviour.Fields[2].AttractorTarget.position, duration);
                    Destroy(widget.gameObject);
                    
                    int count = _getCoin;
                    float delay = GetDelay(_duration, count);

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
                    
                    if (_startDelay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(_startDelay));

                    var widget = Instantiate(ResourcePathAttribute.GetResource<IconReward>());
					widget.OnSetup("UI_Icon_GoldPuzzle_Big");
                    widget.CachedRectTransform.SetParentReset(fragmentHome.CachedRectTransform);
                    widget.CachedRectTransform.SetSize(80);

                    float duration = 1f;

                    await AsyncMove(widget.CachedRectTransform, fragmentHome.rewardPosition_goldPiece.position, duration);
                    Destroy(widget.gameObject);

                    int count = _getGoldPiece;
                    float delay = GetDelay(_duration, count);

                    for(int i=0; i < count; i++)
                    {
                        fragmentHome.goldPieceText.SetText(string.Format("{0}/{1}", _oldGoldPiece + i, 100));
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }

                    fragmentHome.goldPieceText.SetText(string.Format("{0}/{1}", _oldGoldPiece + count, 100));
                }

            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
        }

        UniTask AsyncMove(RectTransform rt, Vector3 targetPosition, float duration)
        {
            return rt.DOMove(targetPosition, duration)
                    .SetEase(Ease.OutQuad)
                    .ToUniTask();
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
