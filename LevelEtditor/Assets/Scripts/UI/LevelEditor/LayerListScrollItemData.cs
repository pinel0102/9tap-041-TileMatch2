using System;
using Gpm.Ui;

public class LayerListScrollItemData : InfiniteScrollData
{
   public int Index = 0;
   public Action<int, bool> OnToggle;
}
