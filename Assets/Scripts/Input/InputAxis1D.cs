using UnityEngine;
using System;

public class InputAxis1D : InputBase
{
	InputEntry entry;
	public Action onButton;
	public Action<float> onAxis1D;

	float axisSensitivity = 0.1f;
	bool pressedAxis;

	public InputAxis1D (Action onButton, InputEntry entry)
	{
		this.onButton = onButton;
		this.entry = entry;
	}

	public InputAxis1D (Action<float> onAxis1D, InputEntry entry)
	{
		this.onAxis1D = onAxis1D;
		this.entry = entry;
	}
		
	public override void Check ()
	{
		if(!entry.UseLikeButton)
		{
			if(onAxis1D != null)
			{
				onAxis1D(Input.GetAxis(entry.AxisName));
			}
		}
		else
		{
			if(onButton != null)
			{
				float valueJoystick = Input.GetAxis(entry.AxisName);

				if(entry.AxisButtonSide == InputAxis1DButtonSide.Positive)
				{
					if( !pressedAxis && valueJoystick >= axisSensitivity )
					{
						pressedAxis = true;
						if( entry.PressState == InputPressState.Down )
							onButton();
					}
					if( pressedAxis && valueJoystick >= axisSensitivity )
					{
						if( entry.PressState == InputPressState.Stay )
							onButton();
					}
					if( pressedAxis && valueJoystick < axisSensitivity )
					{
						pressedAxis = false;
						if( entry.PressState == InputPressState.Up )
							onButton();
					}
				}
				else
				{
					if( !pressedAxis && valueJoystick <= -axisSensitivity )
					{
						pressedAxis = true;
						if( entry.PressState == InputPressState.Down )
							onButton();
					}
					if( pressedAxis && valueJoystick <= -axisSensitivity )
					{
						if( entry.PressState == InputPressState.Stay )
							onButton();
					}
					if( pressedAxis && valueJoystick > -axisSensitivity )
					{
						pressedAxis = false;
						if( entry.PressState == InputPressState.Up )
							onButton();
					}
				}
			}
		}
	}
}
