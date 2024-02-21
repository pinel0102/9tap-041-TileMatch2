using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GlobalData
{
    [Header("★ [Event] Sweet Holic")]
    public ExpTable eventSweetHolic_ExpTable;
    public bool eventSweetHolic_Activate;
    public string eventSweetHolic_ItemName;
    public int eventSweetHolic_ItemIndex;
    public int eventSweetHolic_GetCount = 0;
    public bool eventSweetHolic_IsBoosterTime => userManager?.Current?.IsEventBoosterTime(GameEventType.SweetHolic) ?? false;

    [Header("★ [Settings] Sweet Holic")]
    public int eventSweetHolic_TilePercent = 80;
    public bool eventSweetHolic_TestMode = false;
    public int eventSweetHolic_TestExp;
}