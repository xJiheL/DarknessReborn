using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputDatabase))]
public class InputDatabaseEditor : Editor
{
	SerializedProperty entryProperty;

	void OnEnable ()
	{
		entryProperty = serializedObject.FindProperty("entry");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		/* -------- Add -------- */

		GUILayout.BeginHorizontal();

		if( GUILayout.Button("Add Commmand", GUILayout.Height(30f))  )
		{
			entryProperty.InsertArrayElementAtIndex( entryProperty.arraySize );
		}

		GUILayout.EndHorizontal();

		GUILayout.Space(20f);

		/* -------- List -------- */


		for (int i = 0; i < entryProperty.arraySize; i++)
		{
			SerializedProperty elementProperty = entryProperty.GetArrayElementAtIndex(i);

			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("command"));
			if( GUILayout.Button("Remove", GUILayout.Width(70f), GUILayout.Height(15f)) )
			{
				entryProperty.DeleteArrayElementAtIndex(i);
				break;	//sinon il continue de faire la suite du code (la liste est décalée, ou le dernier élément existe plus)
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("type"));
			GUILayout.EndHorizontal();

			switch (elementProperty.FindPropertyRelative("type").enumNames[elementProperty.FindPropertyRelative("type").enumValueIndex]) {
			case "Button":

				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("keyCode"));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("pressState"));
				GUILayout.EndHorizontal();

				break;

			case "Axis1D":
				
				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("axisName"));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("useLikeButton"));
				GUILayout.EndHorizontal();

				if(elementProperty.FindPropertyRelative("useLikeButton").boolValue)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("pressState"));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("axisButtonSide"));
					GUILayout.EndHorizontal();
				}

				break;

			case "Axis2D":

				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("axisName"));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("useLikeButton"));
				GUILayout.EndHorizontal();

				if(elementProperty.FindPropertyRelative("useLikeButton").boolValue)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative("pressState"));
					GUILayout.EndHorizontal();
				}

				break;
			}

			GUILayout.Space(20f);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
