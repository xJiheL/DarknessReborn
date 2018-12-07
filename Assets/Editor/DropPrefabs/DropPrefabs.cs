using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

public class DropPrefabs : EditorWindow  
{
	ReorderableList objectsToDropList;
	List<GameObject> listPrefabs = new List<GameObject> ();
	GameObject selectedPrefab;
	ReorderableList parentsListReord;
	List<GameObject> parentsList = new List<GameObject> ();
	Material ghostMat;
	bool clickMode;
	bool paintMode;
	bool canPaint;
	bool randomMode;
	bool collapsed;
	bool isUsing;
	bool lockDrop;
	bool scaleHom;
	bool multiParents;
	int indexLayer;
	int selectedLayer;
	int size;
	int count = 0;
	float distanceDrop = .0f;
	float stepRotation = .5f;
	float stepScale = .1f;
	bool[] rotation = new bool[3];
	bool[] scale = new bool[3];
	bool standShortCut = false;
	bool firstClic;
	Vector2 scaleLimit = new Vector2 ();
	Vector2 rotLimit = new Vector2 ();
	Vector3 oldPos;
	Tool oldTool;// Keep unity shortcut actives when using tool.
	Quaternion oldHitNormal;

	[MenuItem ("Tools/Drop Prefabs %#d")]
	static void Init () 
	{
		DropPrefabs window = (DropPrefabs)EditorWindow.GetWindow (typeof (DropPrefabs));
		window.Show();
	}

	void OnEnable()
	{
		SceneView.onSceneGUIDelegate += SceneGUI;
		for (int i = 0; i < 3; i++) 
		{
			rotation [i] = true;
			scale [i] = true;
		}

		scaleLimit.x = 0;
		scaleLimit.y = 2;

		rotLimit.x = 0;
		rotLimit.y = 360;

		if (objectsToDropList == null)
			objectsToDropList = InitList ();

		objectsToDropList.drawElementCallback += DrawElementsListReorder;
		objectsToDropList.onSelectCallback += SelectElement;
		objectsToDropList.drawHeaderCallback += (Rect rect) => {
			EditorGUI.LabelField (rect, "Prefabs list");
		};

		if (parentsListReord == null) 
			parentsListReord = InitMultiParents ();

		parentsListReord.drawElementCallback += DrawElementsListParentReorder;
		parentsListReord.onSelectCallback += SelectElement;
		parentsListReord.drawHeaderCallback += (Rect rect) => {
			EditorGUI.LabelField (rect, "Parents list");
		};

		clickMode = true;
		paintMode = false;
	}

	void SelectElement (ReorderableList objectsToDropList)
	{
		Object prefab = objectsToDropList.list [objectsToDropList.index] as Object;

		if (prefab)
			EditorGUIUtility.PingObject (prefab);
	}

	void DrawElementsListReorder (Rect rect, int index, bool isActive, bool isFocused)
	{
		rect.y += 2;		
		listPrefabs [index] = EditorGUI.ObjectField (new Rect (rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),  listPrefabs[index], typeof(GameObject), false) as GameObject;
		if (listPrefabs [index] != null)
			if (listPrefabs [index].name == "New Game Object")
				DestroyImmediate (listPrefabs [index], true);
	}

	void DrawElementsListParentReorder (Rect rect, int index, bool isActive, bool isFocused)
	{
		rect.y += 2;		
		parentsList [index] = EditorGUI.ObjectField (new Rect (rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),  parentsList[index], typeof(GameObject), true) as GameObject;
		if (parentsList [index] != null) 
		{
			if (EditorUtility.IsPersistent (parentsList [index] as Object))
			{
				parentsList [index] = null;
			}
			else if (parentsList [index].name == "New Game Object")
				DestroyImmediate (parentsList [index], true);
		}
	}

	void OnGUI()
	{		
		EditorGUIUtility.labelWidth = 150f;

		EditorGUILayout.Space ();
		GUI.enabled = !isUsing;
		if (GUILayout.Button ("Start Drop 'C'"))
			StateTool (true);
		
		GUI.enabled = isUsing;
		if (GUILayout.Button ("Stop Drop 'C'"))
			StateTool (false);

		GUI.enabled = true;
		GUILayout.BeginHorizontal ();
		EditorGUIUtility.labelWidth = 100f;
		bool temp = EditorGUILayout.Toggle ("Click mode", clickMode);
		if (clickMode != temp)
		{
			clickMode = temp;
			paintMode = !clickMode;
		}

		temp = EditorGUILayout.Toggle ("Paint mode", paintMode);
		if (paintMode != temp)
		{
			paintMode = temp;
			clickMode = !paintMode;
		}
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		if (paintMode)
			distanceDrop = EditorGUILayout.FloatField ("Paint distance", distanceDrop);
		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();
		GUILayout.BeginVertical ();

		GUI.enabled = !isUsing;
		GUILayout.BeginHorizontal ();

		string [] layers = InternalEditorUtility.layers;
		indexLayer = EditorGUILayout.Popup ("Layer", indexLayer, layers);
		selectedLayer = LayerMask.NameToLayer (layers [indexLayer]);

		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();

		GUI.enabled = true;
		EditorGUILayout.LabelField ("Freeze axis rotation");

		GUILayout.BeginHorizontal ();
		EditorGUIUtility.labelWidth = 20f;

		for (int i = 0; i < 3; i++) 
		{
			string axis = "";
			if (i == 0)
				axis = "X";
			else if (i == 1)
				axis = "Y";
			else
				axis = "Z";
				
			rotation [i] = EditorGUILayout.Toggle (axis, rotation [i]);
		}	

		GUILayout.EndHorizontal ();
		EditorGUILayout.Space ();

		GUI.enabled = false;
		if (!rotation [0] || !rotation [1] || !rotation [2])
		{
			EditorGUILayout.LabelField ("Roll rotation : 'A'");
			EditorGUILayout.LabelField ("Add : 'Z'");
			EditorGUILayout.LabelField ("Substract : 'E'");
		}
		GUI.enabled = true;

		EditorGUIUtility.labelWidth = 100f;
		if (!rotation [0] || !rotation [1] || !rotation [2])
		{
			stepRotation = EditorGUILayout.FloatField ("Step rotation", stepRotation);
			rotLimit.x = EditorGUILayout.FloatField ("Rotation min", rotLimit.x);
			rotLimit.y = EditorGUILayout.FloatField ("Rotation max", rotLimit.y);

			rotLimit.x = Mathf.Clamp (rotLimit.x, -360f, 360f);
			rotLimit.y = Mathf.Clamp (rotLimit.y, -360f, 360f);
		}

		EditorGUILayout.Space ();

		EditorGUILayout.LabelField ("Freeze axis scale");
		EditorGUIUtility.labelWidth = 75f;

		GUILayout.BeginHorizontal ();
		EditorGUIUtility.labelWidth = 20f;
		for (int i = 0; i < 3; i++) 
		{
			string axis = "";
			if (i == 0)
				axis = "X";
			else if (i == 1)
				axis = "Y";
			else
				axis = "Z";

			scale [i] = EditorGUILayout.Toggle (axis, scale [i]);
		}

		GUILayout.EndHorizontal ();
		EditorGUILayout.Space ();
		EditorGUIUtility.labelWidth = 100f;

		GUI.enabled = false;
		if (!scale [0] || !scale [1] || !scale [2])
		{
			EditorGUILayout.LabelField ("Roll scale : 'Q'");
			EditorGUILayout.LabelField ("Add : 'S'");
			EditorGUILayout.LabelField ("Substract : 'D'");
		}
		GUI.enabled = true;

		int count = 0;
		for (int i = 0; i < scale.Length; i++)
		{
			if (!scale [i])
				count ++;
		}

		if (count > 1)
			scaleHom = EditorGUILayout.Toggle ("Homothety scale", scaleHom);			

		EditorGUILayout.Space ();

		if (!scale [0] || !scale [1] || !scale [2])
		{			
			stepScale = EditorGUILayout.FloatField ("Step scale", stepScale);

			scaleLimit.x = EditorGUILayout.FloatField ("Scale min", scaleLimit.x);
			scaleLimit.y = EditorGUILayout.FloatField ("Scale max", scaleLimit.y);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		GUI.enabled = !isUsing;

		EditorGUILayout.Space ();
		EditorGUIUtility.labelWidth = 175f;
		randomMode = EditorGUILayout.Toggle ("Random drop", randomMode);
		GUILayout.BeginVertical ();
		GUI.enabled = false;
		if (!randomMode) 
		{
			EditorGUILayout.LabelField ("'1' : previous object & '2' next Object");
			EditorGUILayout.LabelField ("Selected Object (infos)");
			EditorGUILayout.ObjectField (selectedPrefab, typeof(GameObject), false);
		}

		GUILayout.EndVertical ();

		EditorGUILayout.Space ();

		EditorGUIUtility.labelWidth = 215f;
		GUILayout.BeginVertical ();
		GUI.enabled = !isUsing;
		objectsToDropList.DoLayoutList ();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		multiParents = EditorGUILayout.Toggle ("Parent (optionnal) :", multiParents);

		if (multiParents) 
		{
			GUI.enabled = false;
			EditorGUILayout.LabelField ("Scene objects only");
			GUI.enabled = !isUsing;
			parentsListReord.DoLayoutList ();
		}

	}

	void StateTool (bool state)
	{
		if (state)
		{
			if (listPrefabs.Count == 0 || listPrefabs == null) 
			{
				EditorUtility.DisplayDialog ("Warning!", "List objects can't be empty.", "Ok");
				state = false;
			}
			else
			{
				for (int i = 0; i < listPrefabs.Count; i++) 
				{
					if (listPrefabs[i] == null) 
					{
						EditorUtility.DisplayDialog ("Warning!", "You have at least one empty slot, please remove it/them before.", "Ok");
						state = false;
						break;
					}
				}
			}

			if (selectedLayer == 0)
			{
				EditorUtility.DisplayDialog ("Warning!", "Please select another layer than 'Default'.", "Ok");
				state = false;
			}
		}
		isUsing = state;

		if (isUsing) 
		{
			EditorUtility.DisplayDialog ("Warning!", "Clic one time in scene view to activate shortcuts!", "Ok");

			if (randomMode)
				RandomSelection ();
			else
				ChangeSelectedPrefab (0);
			
			oldTool = Tools.current;
			Tools.current = Tool.View;
		} 
		else if (selectedPrefab != null) 
		{
			firstClic = false;
			DestroyImmediate (selectedPrefab);
			Tools.current = oldTool;
		}
	}

	ReorderableList InitList ()
	{
		ReorderableList newList = new ReorderableList (listPrefabs, listPrefabs.GetType () , true, true, true, true);
		return newList;
	}

	ReorderableList InitMultiParents ()
	{
		ReorderableList newList = new ReorderableList (parentsList, parentsList.GetType () , true, true, true, true);
		return newList;
	}

	void SceneGUI(SceneView sceneView)
	{
		Event cur = Event.current;

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.LeftAlt)
			lockDrop = true;
		if (cur.type == EventType.KeyUp && cur.keyCode == KeyCode.LeftAlt)
			lockDrop = false;

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.C)
		{
			standShortCut = !standShortCut;
			StateTool (standShortCut);
		}

		if (isUsing && !lockDrop) 
		{
			Ray ray = HandleUtility.GUIPointToWorldRay (cur.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) 
			{
				if (selectedLayer == hit.transform.gameObject.layer && hit.transform.gameObject != selectedPrefab) 
				{
					SetPosition (hit);
					SetOrientation (hit);
				}
			}			
		} 

		if (paintMode && cur.type == EventType.MouseDown && cur.button == 0 && isUsing && !lockDrop)
		{
			oldPos = cur.mousePosition;
			canPaint = true;
			cur.Use ();
		}
		else if (paintMode && cur.type == EventType.MouseUp && cur.button == 0 && isUsing && !lockDrop)
		{
			canPaint = false;
			cur.Use ();
		}

		if (cur.type == EventType.MouseDown && !lockDrop && cur.button == 0 && isUsing && clickMode)
		{
			if (!firstClic)
			{
				firstClic = true;
				return;
			}

			DropObject (selectedPrefab.name, selectedPrefab.transform.localPosition, selectedPrefab.transform.localRotation, selectedPrefab.transform.localScale);
			cur.Use ();
		}

		if (cur.type == EventType.MouseDrag && canPaint)
		{
			if (Vector3.Distance (oldPos, cur.mousePosition) > distanceDrop) 
			{
				oldPos = cur.mousePosition;
				DropObject (selectedPrefab.name, selectedPrefab.transform.localPosition, selectedPrefab.transform.localRotation, selectedPrefab.transform.localScale);
			}
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Q && isUsing) 
		{
			RandomScale (selectedPrefab.transform);		
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.S && isUsing) 
		{
			ModifyScale (1, selectedPrefab.transform);	
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.D && isUsing) 
		{
			ModifyScale (-1, selectedPrefab.transform);	
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.A && isUsing) 
		{
			RandomRot (selectedPrefab.transform);	
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Z && isUsing) 
		{
			ModifyRotation (1, selectedPrefab.transform);
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.E && isUsing) 
		{
			ModifyRotation (-1, selectedPrefab.transform);	
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha1 && isUsing && !randomMode) 
		{
			ChangeSelectedPrefab (-1);
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha2 && isUsing && !randomMode)
		{
			ChangeSelectedPrefab (1);
			cur.Use ();
		}

		if (isUsing)
			Tools.current = Tool.View;
	}

	void SetPosition (RaycastHit hit)
	{
		selectedPrefab.transform.position = hit.point;
	}

	void SetOrientation (RaycastHit hit)
	{
		Quaternion orientationTag = Quaternion.LookRotation (hit.normal);
		Vector3 oldAngles = selectedPrefab.transform.localEulerAngles;

		if (oldHitNormal != orientationTag)
		{
			oldHitNormal = orientationTag;
			oldAngles.x = orientationTag.eulerAngles.x + 90;
			oldAngles.y = orientationTag.eulerAngles.y;
			oldAngles.z = orientationTag.eulerAngles.z;
		}

		selectedPrefab.transform.localEulerAngles = oldAngles;
	}

	void RandomSelection ()
	{
		GhostCreation (listPrefabs[Random.Range (0, listPrefabs.Count)]);
	}

	void DropObject (string name, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		for (int i = 0; i < listPrefabs.Count; i ++)
		{
			if (listPrefabs[i].name == name)
			{
				GameObject clone = PrefabUtility.InstantiatePrefab (listPrefabs[i]) as GameObject;
				Undo.RegisterCreatedObjectUndo (clone, "new Object");
				clone.transform.localPosition = pos;
				clone.transform.localRotation = rot;
				clone.transform.localScale = scale;

				if (parentsList != null) 
				{
					if (i < parentsList.Count)
						clone.transform.parent = parentsList[i].transform;							
					else
						clone.transform.parent = parentsList[parentsList.Count-1].transform;	
				}

				EditorUtility.SetDirty (clone);

				if (randomMode)
					RandomSelection ();
				else
					ChangeSelectedPrefab (0);

				break;
			}
		}
	}

	void GhostCreation (GameObject objectSelected)
	{
		if (ghostMat == null)
		{
			Shader ghostShader = Shader.Find ("Hidden/SHADER_Ghost");
			ghostMat = new Material (ghostShader);
		}

		Vector3 pos = new Vector3 ();
		Quaternion rot = new Quaternion ();

		if (selectedPrefab != null)
		{
			pos = selectedPrefab.transform.localPosition;
			rot = selectedPrefab.transform.localRotation;
			DestroyImmediate (selectedPrefab);
		}

		selectedPrefab = PrefabUtility.InstantiatePrefab (objectSelected) as GameObject;
		if (pos != new Vector3 ())
			selectedPrefab.transform.localPosition = pos;
		if (rot != new Quaternion ())
			selectedPrefab.transform.localRotation = rot;

		RandomScale (selectedPrefab.transform);
		RandomRot (selectedPrefab.transform);

		Renderer[] rends = selectedPrefab.GetComponentsInChildren <Renderer> ();
		for (int i = 0; i < rends.Length; i ++)
		{
			Material[] mats = rends[i].sharedMaterials;
			for (int j = 0; j < mats.Length; j++)
				mats[j] = ghostMat;
			rends[i].sharedMaterials = mats;
		}
	}

	void RandomRot (Transform objectSelected)
	{
		Vector3 angles = objectSelected.localEulerAngles;
		if (!rotation[0])
			angles.x = Random.Range (rotLimit.x, rotLimit.y);
		if (!rotation[1])
			angles.y = Random.Range (rotLimit.x, rotLimit.y);
		if (!rotation[2])
			angles.z = Random.Range (rotLimit.x, rotLimit.y);

		objectSelected.localEulerAngles = angles;
	}

	void ModifyRotation (int factor, Transform objectSelected)
	{
		Vector3 angles = objectSelected.localEulerAngles;

		float value = stepRotation * factor;
		if (!rotation[0])
			angles.x += value;
		if (!rotation[1])
			angles.y += value;
		if (!rotation[2])
			angles.z += value;

		objectSelected.localEulerAngles = angles;
	}

	void RandomScale (Transform objectSelected)
	{
		Vector3 newScale = objectSelected.localScale;

		if (!scaleHom)
		{
			if (!scale[0])
				newScale.x = Random.Range (scaleLimit.x, scaleLimit.y);
			if (!scale[1])
				newScale.y = Random.Range (scaleLimit.x, scaleLimit.y);
			if (!scale[2])
				newScale.z = Random.Range (scaleLimit.x, scaleLimit.y);
		}
		else
		{
			float value = Random.Range (scaleLimit.x, scaleLimit.y);

			for (int i = 0; i < scale.Length; i++) 
				if (!scale[i])
					newScale[i] = value;
		}

		objectSelected.localScale = newScale;
	}

	void ModifyScale (int factor, Transform objectSelected)
	{
		Vector3 newScale = objectSelected.localScale;
		float value = stepScale * factor;

		if (!scale[0])
			newScale.x = Mathf.Clamp(newScale.x + value, scaleLimit.x, scaleLimit.y);
		if (!scale[1])
			newScale.y = Mathf.Clamp(newScale.y + value, scaleLimit.x, scaleLimit.y);
		if (!scale[2])
			newScale.z = Mathf.Clamp(newScale.z + value, scaleLimit.x, scaleLimit.y);

		objectSelected.localScale = newScale;
	}

	void ChangeSelectedPrefab (int indice)
	{
		count += indice;
		if (count >= listPrefabs.Count)
			count = 0;
		else if (count < 0)
			count = listPrefabs.Count - 1;
		
		GhostCreation (listPrefabs[count]);
	}

	void Update ()
	{
		Repaint ();
	}

	void OnDestroy ()
	{
		Tools.current = oldTool;
		SceneView.onSceneGUIDelegate -= SceneGUI;
	}
}