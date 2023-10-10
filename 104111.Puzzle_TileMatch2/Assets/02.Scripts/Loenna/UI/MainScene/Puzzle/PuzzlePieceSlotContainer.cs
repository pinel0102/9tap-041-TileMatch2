using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using NineTap.Common;

public class PuzzlePieceSlotContainer : CachedBehaviour
{
	private const float CHECK_OFFSET = 15f;
	private record Slot
	(
		int Index,
		Transform Transform,
		bool HasPiece
	)
	{
		public static Slot Create(int index, Transform parent)
		{
			GameObject go = new GameObject($"Slot_{index}");
			Image image = go.AddComponent<Image>();
			image.color = Color.clear;
			Transform trans = go.transform;
			trans.SetParentReset(parent);

			return new Slot (
				Index: index,
				Transform: trans,
				HasPiece: false
			);
		}

		public static Slot AttachPiece(Slot slot, GameObject piece)
		{
			piece.transform.SetParentReset(slot.Transform);
			
			return slot with { HasPiece = true };
		}

		public static Slot DetachPiece(Slot slot)
		{
			JigsawPuzzlePiece piece = slot.Transform.GetComponentInChildren<JigsawPuzzlePiece>();
			if (piece != null)
			{
				Destroy(piece.CachedGameObject);
			}
			return slot with { HasPiece = false };
		}
	}

	[SerializeField]
	private GridLayoutGroup m_layoutGroup;
	private List<Slot> m_slots = new List<Slot>();

	public void OnSetup()
	{
		for (int i = 0; i < 25; i++)
		{
			m_slots.Add(Slot.Create(i, CachedTransform));
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(CachedRectTransform);
	}

	public void TearDown()
	{
		for (int i = 0; i < m_slots.Count; i++)
		{
			m_slots[i] = Slot.DetachPiece(m_slots[i]);
		}
	}

	public void UpdateSlot(int index, GameObject pieceObject)
	{
		if (index < 0 || index > 25)
		{
			return;
		}
		
		Slot slot = m_slots[index];

		if (slot.HasPiece)
		{
			return;
		}

		m_slots[index] = Slot.AttachPiece(slot, pieceObject);
	}

	public void Check(int index, JigsawPuzzlePiece piece, Action<int> onAttach)
	{
		Vector2 slotPosition = m_slots[index].Transform.position;
		Vector2 piecePosition = piece.CachedTransform.position;

		float distance = Vector2.Distance(slotPosition, piecePosition);

		if (distance < CHECK_OFFSET)
		{
			piece.Attached();
			UpdateSlot(index, piece.CachedGameObject);
			onAttach?.Invoke(index);
		}
	}
}
