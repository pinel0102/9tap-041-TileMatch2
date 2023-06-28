using UnityEngine;
using UnityEngine.InputSystem;

using System;

public class InputController
{
	public enum State
	{
		NONE,
		LEFT_BUTTON_PRESSED,
		RIGHT_BUTTON_PRESSED,
		LEFT_BUTTON_RELEASED,
		RIGHT_BUTTON_RELEASED
	}

	private static InputController s_instance;
	public static InputController Instance
	{
		get
		{
			if (ReferenceEquals(s_instance, null))
			{
				s_instance = new InputController();
			}

			return s_instance;
		}
	}

	private readonly Mouse m_mouse;

	public Vector2 Position 
	{
		get
		{
			return m_mouse?
				.position?
				.ReadValue() 
				?? throw new NullReferenceException(nameof(m_mouse));
		}
	}

	public State WasState
	{
		get
		{
			if (m_mouse == null)
			{
				return State.NONE;
			}

			if (m_mouse.leftButton.wasPressedThisFrame)
			{
				return State.LEFT_BUTTON_PRESSED;
			}

			if (m_mouse.rightButton.wasPressedThisFrame)
			{
				return State.RIGHT_BUTTON_PRESSED;
			}

			if (m_mouse.leftButton.wasReleasedThisFrame)
			{
				return State.LEFT_BUTTON_PRESSED;
			}

			if (m_mouse.rightButton.wasReleasedThisFrame)
			{
				return State.RIGHT_BUTTON_PRESSED;
			}

			return State.NONE;
		}
	}

	public InputController()
	{
		m_mouse = Mouse.current;
	}

	public Vector2 ToLocalPosition(Transform transform) => transform.InverseTransformPoint(Position);
}
