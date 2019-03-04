using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu( fileName = "Input Database", menuName = "Create new Input Database" )]
public class InputDatabase : ScriptableObject
{
	[SerializeField] List<InputEntry> entry = new List<InputEntry>();

	public InputEntry GetEntry( EnumCommand searchCommand )
	{
		return entry.Find( e => e.Command == searchCommand );
	}
}
