using UnityEngine;
using System;

[Serializable]
public class InputEntry
{
	[SerializeField] EnumCommand command = default;
	[SerializeField] InputType type = default;
	[SerializeField] KeyCode keyCode = default;
	[SerializeField] string axisName = "";
	[SerializeField] InputPressState pressState = default;
	[SerializeField] bool useLikeButton = false;
	[SerializeField] InputAxis1DButtonSide axisButtonSide = default;

	public EnumCommand Command => command;

	public InputType Type => type;

	public KeyCode KeyCode => keyCode;

	public string AxisName => axisName;

	public InputPressState PressState => pressState;

	public bool UseLikeButton => useLikeButton;

	public InputAxis1DButtonSide AxisButtonSide => axisButtonSide;
}
