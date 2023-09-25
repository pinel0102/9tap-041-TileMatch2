using UnityEngine;

using System;

using NineTap.Common;

[ResourcePath("UI/Widgets/StashLayer")]
public class StashLayer : CachedBehaviour
{
	private Transform[] m_tileObjects = new Transform[Constant.Game.STASH_TILE_AMOUNT];

	public Transform this[int index] { get => m_tileObjects[index]; set => m_tileObjects[index] = value; }

	public int GetEmptySlot() => Array.FindIndex(m_tileObjects, transform => transform == null);

}
