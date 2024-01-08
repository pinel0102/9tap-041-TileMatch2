using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;

public partial class GlobalData : SingletonMono<GlobalData>
{
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
}
