using UnityEngine;
using System;

public class InputButton : InputBase
{
	InputEntry entry;
	public Action onButton;

	public InputButton (Action onButton, InputEntry entry)
	{
		this.onButton = onButton;
		this.entry = entry;
	}

	public override void Check ()
	{
		if(onButton != null)
		{
			switch (entry.PressState)
			{
			case InputPressState.Down:
				if(Input.GetKeyDown(entry.KeyCode))
					onButton();
				break;

			case InputPressState.Stay:
				if(Input.GetKey(entry.KeyCode))
					onButton();
				break;

			case InputPressState.Up:
				if(Input.GetKeyUp(entry.KeyCode))
					onButton();
				break;
			}
		}
	}
}
