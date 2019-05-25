using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor (typeof (MeshDecals_Build))]
public class MeshDecals_Editor : Editor
{
	string[] namesTexture;
	[SerializeField]int actualIndex;

	public override void OnInspectorGUI()
	{
		MeshDecals_Build decalScript = (MeshDecals_Build) target;
		DrawDefaultInspector();

		if (!decalScript.AutoUpdate)
			if (GUILayout.Button("Update Decal"))
				decalScript.BuildDecal();

		if (!decalScript.TarDecal)
		{
			EditorGUI.BeginChangeCheck();
			actualIndex = EditorGUILayout.Popup(actualIndex, namesTexture);
			if (EditorGUI.EndChangeCheck())
			{
				decalScript.SelectedDecal = decalScript.Decals[actualIndex];
			}
		}

	}
}
