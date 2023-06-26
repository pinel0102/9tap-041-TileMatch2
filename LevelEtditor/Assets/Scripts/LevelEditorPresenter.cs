using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorPresenter : IDisposable
{
	private List<LevelData> m_levelDatas = new();
	private LevelData m_currentLevelData;

	public void Dispose()
	{

	}
}
