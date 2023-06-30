using UnityEngine;

using System;

using Gpm.Ui;

public class LayerListScrollItemData : InfiniteScrollData
{
   public int Index = 0;
   public Color Color = Color.white;
   public Action<int, bool> OnToggle;
}
