using UnityEngine;
using System;

public class InputAxis2D : InputBase
{
	InputEntry entry;
	public Action onButton;
	public Action<Vector2> onAxis2D;

	float axisSensitivity = 0.1f;
	bool pressedAxis;

	public InputAxis2D (Action onButton, InputEntry entry)
	{
		this.onButton = onButton;
		this.entry = entry;
	}

	public InputAxis2D (Action<Vector2> onAxis2D, InputEntry entry)
	{
		this.onAxis2D = onAxis2D;
		this.entry = entry;
	}

	public override void Check ()
	{
		if(!entry.UseLikeButton)
		{
			if(onAxis2D != null)
			{
				onAxis2D(
					new Vector2(
						Input.GetAxis(entry.AxisName+"X"),
						Input.GetAxis(entry.AxisName+"Y")
					)
				);
			}
		}
		else
		{
			if(onButton != null)
			{
				Vector2 valueJoystick = new Vector2(
					Input.GetAxis(entry.AxisName+"X"),
					Input.GetAxis(entry.AxisName+"Y")
				);
					
				if( !pressedAxis && valueJoystick.magnitude >= axisSensitivity )
				{
					pressedAxis = true;
					if( entry.PressState == InputPressState.Down )
						onButton();
				}
				if( pressedAxis && valueJoystick.magnitude >= axisSensitivity )
				{
					if( entry.PressState == InputPressState.Stay )
						onButton();
				}
				if( pressedAxis && valueJoystick.magnitude < axisSensitivity )
				{
					pressedAxis = false;
					if( entry.PressState == InputPressState.Up )
						onButton();
				}
			}
		}
	}
}
