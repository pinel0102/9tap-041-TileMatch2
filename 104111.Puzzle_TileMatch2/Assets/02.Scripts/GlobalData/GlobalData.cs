using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class GlobalData : SingletonMono<GlobalData>
{
    public HUD HUD => Game.Inst.Get<HUD>();
    public UserManager userManager => Game.Inst.Get<UserManager>();

    public long oldCoin = 0;
    public long oldPuzzlePiece = 0;
    public long oldGoldPiece = 0;

    public int missionCollected = 0;

    public void Initialize()
    {
        missionCollected = 0;
        oldCoin = 0;
        oldPuzzlePiece = 0;
        oldGoldPiece = 0;
    }

    public void SetOldItems(long _coin, long _puzzlePiece, long _goldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", _coin, _puzzlePiece, _goldPiece));

        oldCoin = _coin;
        oldPuzzlePiece = _puzzlePiece;
        oldGoldPiece = _goldPiece;
    }

    public void HUD_LateUpdate(int _getCoin, int _getPuzzlePiece, int _getGoldPiece)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} / {1} / {2}", _getCoin, _getPuzzlePiece, _getGoldPiece));

        long _oldCoin = oldCoin;
        long _oldPuzzle = oldPuzzlePiece;
        long _oldGoldPiece = oldGoldPiece;

        if(_getPuzzlePiece > 0)
            HUD?.behaviour.Fields[0].SetText(_oldPuzzle);
        if(_getCoin > 0)
            HUD?.behaviour.Fields[2].SetText(_oldCoin);
        //if(_goldPiece > 0)

        UniTask.Void(
			async token => {
                if(_getPuzzlePiece > 0)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("[PuzzlePiece] {0} + {1} = {2}", _oldPuzzle, _getPuzzlePiece, _oldPuzzle + _getPuzzlePiece));
                    
                    int count = _getPuzzlePiece;
                    float delay = GetDelay(0.5f, count);

                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

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
                    
                    int count = _getCoin;
                    float delay = GetDelay(0.5f, count);

                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

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
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
                }

            },
			this.GetCancellationTokenOnDestroy()
        );

        float GetDelay(float time, int amount)
        {
            return time/(float)amount;
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
