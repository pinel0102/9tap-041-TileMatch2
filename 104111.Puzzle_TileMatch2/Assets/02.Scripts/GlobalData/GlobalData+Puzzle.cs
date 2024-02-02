using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class GlobalData
{
    public PuzzleData GetLatestPuzzle()
    {
        if (tableManager != null)
        {
            var puzzle = tableManager.PuzzleDataTable.Dic.LastOrDefault(item => 
                item.Value.Level <= tableManager.LastLevel && 
                item.Value.Level <= userManager.Current.Level).Value;
            
            return puzzle ?? tableManager.PuzzleDataTable.Dic.First().Value;
        }

        return tableManager.PuzzleDataTable.Dic.First().Value;
    }

    public void MoveToLatestPuzzle()
    {
        MoveToPuzzle(GetLatestPuzzle());
    }

    public void MoveToPuzzle(PuzzleData puzzleData)
    {
        User user = userManager.Current;
        int newIndex = puzzleData.Index;
        uint placedPieces = user.PlayingPuzzleCollection.TryGetValue(newIndex, out uint result)? result : 0;
        uint unlockedPieces = user.UnlockedPuzzlePieceDic == null? 0 : 
            user.UnlockedPuzzlePieceDic.TryGetValue(puzzleData.Key, out uint result2)? 
            result2 : 0;
        
        mainScene.lobbyManager.OnSelectPuzzle(puzzleData, placedPieces, unlockedPieces);
    }

    public int GetOpenedPuzzleIndex(int clearedLevel)
    {
        var puzzle = tableManager.PuzzleDataTable.Dic.FirstOrDefault(item => 
            item.Value.Level <= tableManager.LastLevel && 
            item.Value.Level == clearedLevel + 1).Value;
        
        return puzzle?.Index ?? -1;
    }
}
