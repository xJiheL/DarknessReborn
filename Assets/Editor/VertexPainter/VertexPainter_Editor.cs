using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VertexPainter_Editor : EditorWindow 
{
	float brushSize;
	int brushIntensity;
	int brushFalloff;
	bool isUsing = false;
	bool lockPaint = false;
	bool isPainting = false;
	bool showVertColor = false;
	bool showVertAlpha = false;
	GameObject parentMeshes;
	Vector3 oldPos;
	Vector3 oldPosHit;
	Vector3 oldOrHit;
	Tool oldTool; // Keep unity shortcut actives when using tool.
	Material vertMat;

	bool[] writeChannel;
	List<Mesh> meshesToPaint;
	List<Transform> transformToPaint;
	List<List<Vector3>> verticesToPaint;
	List<string> nameMeshes;
	List<List<Material>> mat;

	// Brush
	Transform transformBrush;
	Projector projectorBrush;
	Material matProjector;

	[MenuItem ("Tools/Vertex Painter")]
	static void Init () 
	{
		VertexPainter_Editor window = (VertexPainter_Editor)EditorWindow.GetWindow (typeof (VertexPainter_Editor));
		window.Show();
	}

	void Awake()
	{
		SceneView.onSceneGUIDelegate += SceneGUI;

		// Init brush values
		brushIntensity = 50;
		brushSize = 1.0f;
		brushFalloff = 50;

		writeChannel = new bool[4] {true, true, true, true};

		Undo.undoRedoPerformed += UndoRedoCallback;
		InitList ();
	}

	void InitList ()
	{		
		if (transformToPaint == null)
			transformToPaint = new List<Transform> ();
		if (meshesToPaint == null)
			meshesToPaint = new List<Mesh> ();
		if (verticesToPaint == null)
			verticesToPaint = new List<List<Vector3>> ();
		if (nameMeshes == null)
			nameMeshes = new List<string> ();
	}

	void OnGUI()
	{
		EditorGUILayout.Space ();

		EditorGUIUtility.labelWidth = 155f;
		GameObject newParent = EditorGUILayout.ObjectField ("Meshes to paint (Parent) : ", parentMeshes, typeof(GameObject), true) as GameObject;
		if (newParent != null)
		{
			if (parentMeshes != newParent)
				if (EditorUtility.IsPersistent (newParent))
					EditorUtility.DisplayDialog ("Warning!", "Only scene objects allowed!", "Ok");
				else
					parentMeshes = newParent;	
		}
		else if (parentMeshes != null && newParent == null)
			parentMeshes = null;

		EditorGUILayout.Space ();

		EditorGUIUtility.labelWidth = 150f;
		EditorGUILayout.LabelField ("Channel to paint : ");

		EditorGUIUtility.labelWidth = 35;
		GUILayout.BeginVertical ();
		GUILayout.BeginHorizontal ();
		string channel = "R (1)";
		writeChannel[0] = EditorGUILayout.Toggle (channel, writeChannel[0]);
		channel = "G (2)";
		writeChannel[1] = EditorGUILayout.Toggle (channel, writeChannel[1]);
		channel = "B (3)";
		writeChannel[2] = EditorGUILayout.Toggle (channel, writeChannel[2]);
		channel = "A (4)";
		writeChannel[3] = EditorGUILayout.Toggle (channel, writeChannel[3]);
		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUI.enabled = isUsing;
		if (GUILayout.Button ("Fill Red"))
			FillColor (new Color (1.0f, 0.0f, 0.0f, 0.0f), false);
		if (GUILayout.Button ("Fill Green"))
			FillColor (new Color (0.0f, 1.0f, 0.0f, 0.0f), false);
		if (GUILayout.Button ("Fill Blue"))
			FillColor (new Color (0.0f, 0.0f, 1.0f, 0.0f), false);
		if (GUILayout.Button ("Fill Alpha"))
			FillColor (new Color (0.0f, 0.0f, 0.0f, 1.0f), false);

		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Clear Red"))
			ClearChannel (0);
		if (GUILayout.Button ("Clear Green"))
			ClearChannel (1);
		if (GUILayout.Button ("Clear Blue"))
			ClearChannel (2);
		if (GUILayout.Button ("Clear Alpha"))
			ClearChannel (3);

		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();

		if (GUILayout.Button ("Clear All Color (All meshes selected)"))
			FillColor (new Color (0.0f, 0.0f, 0.0f, 0.0f), true);
		
		GUI.enabled = true;
		GUILayout.EndVertical ();
		GUILayout.BeginHorizontal ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		EditorGUIUtility.labelWidth = 120f;
		GUI.enabled = isUsing;
		bool showTemp = EditorGUILayout.Toggle ("Show vertex color : ", showVertColor);
		if (showVertColor != showTemp)
		{
			showVertColor = showTemp;
			SwitchMaterial (showVertColor);
		}

		GUI.enabled = showVertColor;
		bool showAlpha = EditorGUILayout.Toggle ("Alpha : ", showVertAlpha);
		if (showVertAlpha != showAlpha)
		{
			showVertAlpha = showAlpha;
			ActivateAlpha (showVertAlpha);
		}
		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUI.enabled = true;
		GUILayout.BeginVertical ();
		EditorGUIUtility.labelWidth = 100f;
		int newBrushIntensity = EditorGUILayout.IntField ("Brush intensity : ", brushIntensity);
		newBrushIntensity = Mathf.Clamp (newBrushIntensity, -100, 100);
		float newBrushSize = EditorGUILayout.FloatField ("Brush size : ", brushSize);
		if (newBrushSize < 0.0f)
			newBrushSize = 0.0f;
		int newBrushFallof = EditorGUILayout.IntField ("Brush falloff : ", brushFalloff);
		newBrushFallof = Mathf.Clamp (newBrushFallof, 0, 100);

		if (brushIntensity != newBrushIntensity)
			brushIntensity = newBrushIntensity;
		
		if (brushSize != newBrushSize || brushFalloff != newBrushFallof)
		{			
			brushSize = newBrushSize;
			brushFalloff = newBrushFallof;
			if (isUsing)
				UpdateBrush ();
		}
		GUILayout.EndVertical ();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUILayout.BeginHorizontal ();
		GUI.enabled = !isUsing;
		if (GUILayout.Button ("Start painting"))
			MeshConfiguration ();
		
		GUI.enabled = isUsing;
		if (GUILayout.Button ("Stop painting"))
			StopPainting ();
		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		GUILayout.BeginVertical ();
		GUI.enabled = false;
		EditorGUILayout.LabelField ("Objects target : " + nameMeshes.Count);
		EditorGUILayout.Space ();
		for (int i = 0; i < nameMeshes.Count; i++) 
		{
			EditorGUILayout.LabelField (nameMeshes[i]);
		}
		GUILayout.EndVertical ();
	}

	void SwitchMaterial (bool state)
	{
		if (state)
		{
			if (vertMat == null) 
			{
				vertMat = new Material (Shader.Find ("VertexPainter/Shader_VertexPaint_Emission"));
				vertMat.hideFlags = HideFlags.DontSave;
			}

			if (mat == null)
				mat = new List <List<Material>> ();

			for (int i = 0; i < transformToPaint.Count; i++) 
			{
				mat.Add (new List<Material> ());
				if (mat [i] == null)
					mat [i] = new List<Material> ();

				Material[] actualMat = transformToPaint [i].GetComponent <MeshRenderer> ().sharedMaterials;
			    actualMat = transformToPaint [i].GetComponent <MeshRenderer> ().sharedMaterials;
				for (int j = 0; j < actualMat.Length; j++) 
				{
					mat[i].Add (actualMat [j]);			
					actualMat [j] = vertMat;
				}
				transformToPaint [i].GetComponent <MeshRenderer> ().materials = actualMat;
			}
		}
		else 
		{
			for (int i = 0; i < transformToPaint.Count; i++) 
			{
				Material[] actualMat = transformToPaint [i].GetComponent <MeshRenderer> ().sharedMaterials;
				for (int j = 0; j < actualMat.Length; j++) 
					actualMat[j] = mat[i][j];		

				transformToPaint [i].GetComponent <MeshRenderer> ().sharedMaterials = actualMat;

			}
	
			if (mat != null)
				mat.Clear ();

		}
		SceneView.RepaintAll ();
	}

	void ActivateAlpha (bool state)
	{
		float stateF = 0f;
		if (state)
			stateF = 1f;
			
		Shader.SetGlobalFloat ("_ShowAlpha", stateF);
		SceneView.RepaintAll ();
	}

	void FillColor (Color colorToFill, bool eraseOldColor)
	{
		for (int i = 0; i < meshesToPaint.Count; i++) 
		{
			Undo.RecordObject (meshesToPaint[i] as Object, "Fill Color");
			Color[] newColors = new Color[meshesToPaint [i].vertices.Length];

			for (int j = 0; j < newColors.Length; j++)
				newColors [j] = colorToFill;

			if (!eraseOldColor) 
			{
				Color[] oldColors = meshesToPaint [i].colors;

				if (oldColors.Length > 0) 
				{
					for (int k = 0; k < newColors.Length; k++) 
					{
						newColors [k] += oldColors [k];
						newColors [k] = ClampColor (newColors [k]);
					}
				}
			}
			meshesToPaint [i].colors = newColors;
		}
	}

	Color ClampColor (Color clampedColor)
	{
		clampedColor = new Color (Mathf.Clamp01 (clampedColor.r), Mathf.Clamp01 (clampedColor.g), Mathf.Clamp01 (clampedColor.b), Mathf.Clamp01 (clampedColor.a));
		return clampedColor;
	}

	void ClearChannel (int channel)
	{
		for (int i = 0; i < meshesToPaint.Count; i++) 
		{
			Undo.RecordObject (meshesToPaint[i] as Object, "Clear Color");
			Color[] oldColors = meshesToPaint [i].colors;

			for (int k = 0; k < oldColors.Length; k++) 
			{
				switch (channel)
				{
				case 0:
					oldColors [k] = new Color (0.0f, oldColors [k].g, oldColors [k].b, oldColors [k].a);
					break;

				case 1:
					oldColors [k] = new Color (oldColors [k].r, 0.0f, oldColors [k].b, oldColors [k].a);
					break;

				case 2:
					oldColors [k] = new Color (oldColors [k].r, oldColors [k].g, 0.0f, oldColors [k].a);
					break;

				case 3:
					oldColors [k] = new Color (oldColors [k].r, oldColors [k].g, oldColors [k].b, 0.0f);
					break;
				}
				oldColors [k] =  ClampColor (oldColors [k]);
			}

			meshesToPaint [i].colors = oldColors;
		}
	}

	void MeshConfiguration () //Replace Mesh filter with instance for vertex painting 
	{
		if (parentMeshes == null)
		{
			EditorUtility.DisplayDialog ("Warning!", "Please refer meshes filters' parent.", "Ok");
			return;
		}

		Transform[] transforms = parentMeshes.GetComponentsInChildren <Transform> ();
		// Collect infos
		for (int i = 0; i < transforms.Length; i++) 
		{
			MeshFilter meshFilter = transforms [i].GetComponent <MeshFilter> ();
			if (meshFilter != null)
			{
				if (transformToPaint == null)
					InitList ();			
				transformToPaint.Add (transforms [i]);

				Mesh newMesh = new Mesh ();
				if (meshFilter.sharedMesh.name.Substring (0, 3) != "VP_") 
				{
					newMesh = Mesh.Instantiate (meshFilter.sharedMesh) as Mesh;
					newMesh.name = "VP_" + meshFilter.name;
					meshFilter.mesh = newMesh;
				}
				else 
					newMesh = meshFilter.sharedMesh;

				if (meshesToPaint == null)
					InitList ();
				meshesToPaint.Add (newMesh);

				List<Vector3> newListVert = new List<Vector3> (newMesh.vertices);
				if (verticesToPaint == null)
					InitList ();	
				verticesToPaint.Add (newListVert);

				if (nameMeshes == null)
					InitList ();
				nameMeshes.Add (meshFilter.gameObject.name);
			}
		}

		if (transformToPaint.Count == 0) 
		{
			EditorUtility.DisplayDialog ("Warning!", "No <MeshFilter> found in children.", "Ok");
			return;
		}

		isUsing = true;
		InitBrushProjector ();

		oldTool = Tools.current;
		Tools.current = Tool.View;
	}
		
	void InitBrushProjector ()
	{
		if (matProjector == null)
			matProjector = new Material (Shader.Find ("VertexPainter/Shader_Projector_Brush"));

		string[] guidBrush = AssetDatabase.FindAssets ("Prefab_VertexPainter_Brush");
		string path = AssetDatabase.GUIDToAssetPath(guidBrush[0]);
		Transform projector = AssetDatabase.LoadAssetAtPath <Transform> (path);

		guidBrush = AssetDatabase.FindAssets ("Tex_Brush");
		path = AssetDatabase.GUIDToAssetPath(guidBrush[0]);
		Texture texBrush = AssetDatabase.LoadAssetAtPath <Texture> (path);

		matProjector.SetTexture ("_MainTex", texBrush);

		transformBrush = PrefabUtility.InstantiatePrefab (projector) as Transform;
		projectorBrush = transformBrush.GetComponentInChildren <Projector> ();
		projectorBrush.material = matProjector;

		transformBrush.gameObject.hideFlags = HideFlags.HideInHierarchy;
		UpdateBrush ();
	}

	void UpdateBrush ()
	{
		matProjector.SetFloat ("_Falloff", brushFalloff);
		projectorBrush.orthographicSize = brushSize / 2f;
	}

	void SceneGUI(SceneView sceneView)
	{		
		Event cur = Event.current;

		if (cur.type == EventType.KeyDown && 
				(cur.keyCode == KeyCode.LeftAlt ||
					cur.keyCode == KeyCode.RightAlt ||
						cur.keyCode == KeyCode.LeftControl ||
							cur.keyCode == KeyCode.RightControl))
		{
			lockPaint = true;
			isPainting = false;
		}

		if (cur.type == EventType.KeyUp && 
				(cur.keyCode == KeyCode.LeftAlt ||
					cur.keyCode == KeyCode.RightAlt ||
						cur.keyCode == KeyCode.LeftControl ||
							cur.keyCode == KeyCode.RightControl))
		{
			lockPaint = false;
			isPainting = false;
		}

		if (cur.type == EventType.MouseDown && cur.button == 0 && isUsing && !lockPaint)
		{
			oldPos = cur.mousePosition;

			BakeColor (RaycastFromMouse (cur.mousePosition)[0]);

			isPainting = true;
			cur.Use ();
		}

		if (isUsing && cur.type == EventType.MouseDrag && !lockPaint && isPainting && cur.button == 0)
		{
			float dist = Vector3.Distance (cur.mousePosition, oldPos);
			if (dist > .1f)
			{
				oldPos = cur.mousePosition;
				BakeColor (RaycastFromMouse (cur.mousePosition)[0]);
			}
		}

		if (cur.type == EventType.MouseUp && cur.button == 0 && isUsing && !lockPaint && !isPainting)
		{
			oldPos = cur.mousePosition;
			isPainting = false;
			cur.Use ();
		}

		if (isUsing && !lockPaint)
		{
			Vector3[] values = RaycastFromMouse (cur.mousePosition);
			transformBrush.position = values[0];
			transformBrush.eulerAngles = values[1];
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha1 && !lockPaint) 
		{
			writeChannel [0] = !writeChannel [0];
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha1 && lockPaint) 
		{
			FillColor (new Color (1.0f, 0.0f, 0.0f, 0.0f), false);
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha2 && !lockPaint)
		{
			writeChannel [1] = !writeChannel [1];
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha2 && lockPaint) 
		{
			FillColor (new Color (0.0f, 1.0f, 0.0f, 0.0f), false);
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha3 && !lockPaint)
		{
			writeChannel [2] = !writeChannel [2];
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha3 && lockPaint) 
		{
			FillColor (new Color (0.0f, 0.0f, 1.0f, 0.0f), false);
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha4 && !lockPaint)
		{
			writeChannel [3] = !writeChannel [3];
			cur.Use ();
		}

		if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.Alpha4 && lockPaint) 
		{
			FillColor (new Color (0.0f, 0.0f, 0.0f, 1.0f), false);
			cur.Use ();
		}

		Repaint();
	}

	Vector3[] RaycastFromMouse (Vector3 mousePos)
	{
		Vector3[] values = new Vector3[2];
		Ray ray = HandleUtility.GUIPointToWorldRay (mousePos);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) 
		{
			values [0] = hit.point;
			oldPosHit = values [0];
			values [1] = SetOrientation (hit);
			oldOrHit = values [1];
		}
		else 
		{
			values[0] = oldPosHit;
			values[1] = oldOrHit;
		}
		return values;
	}

	Vector3 SetOrientation (RaycastHit hit)
	{
		Quaternion orientationTag = Quaternion.LookRotation (hit.normal);
		transformBrush.rotation = orientationTag;
		transformBrush.eulerAngles = new Vector3 (transformBrush.eulerAngles.x + 90f, transformBrush.eulerAngles.y, transformBrush.eulerAngles.z);
		return transformBrush.eulerAngles;
	}

	void UndoRedoCallback ()
	{
		for (int i = 0; i < meshesToPaint.Count; i++)
			meshesToPaint [i].colors = meshesToPaint [i].colors;
	}

	void BakeColor (Vector3 hitPos)
	{
		Color[] newColors = new Color[0];
		for (int i = 0; i < verticesToPaint.Count; i++) 
		{
			for (int j = 0; j < verticesToPaint[i].Count; j++) 
			{
				Color[] oldColors = new Color[0];
				float distVertex = Vector3.Distance (hitPos, transformToPaint[i].TransformPoint (verticesToPaint [i][j]));

				if (distVertex <= brushSize)
				{
					Undo.RecordObject (meshesToPaint[i] as Object, "Paint Color");
					oldColors = meshesToPaint [i].colors;
					newColors = new Color[verticesToPaint[i].Count];
					newColors[j] = CalculNewColor (distVertex);

					if (oldColors.Length > 0) 
					{
						for (int k = 0; k < newColors.Length; k++) 
						{
							newColors [k] += oldColors [k];
							newColors [k] = ClampColor (newColors [k]);
						}
					}
					meshesToPaint [i].colors = newColors;
					SmoothVertexColors (meshesToPaint [i].triangles, meshesToPaint [i].colors);
//					SmoothVertexColors (meshesToPaint [i].triangles, meshesToPaint [i].colors);
				}				
			}
		}
	}

	Color CalculNewColor (float dist)
	{
		float intensity = brushIntensity / 100f;
		Color newColor = new Color (0f, 0f, 0f, 0f);
		float distFactor = (brushSize * brushFalloff) / 100; //Percentage brush size
		if (dist <= distFactor)
			distFactor = 1f;
		else
			distFactor = 1 - ((dist - distFactor) / (brushSize - distFactor));

		if (writeChannel[0])
			newColor.r = 1f * intensity * distFactor; 
		if (writeChannel[1])
			newColor.g = 1f * intensity * distFactor; 
		if (writeChannel[2])
			newColor.b = 1f * intensity * distFactor; 
		if (writeChannel[3])
			newColor.a = 1f * intensity * distFactor; 
		
		return newColor;
	}

	Color[] SmoothVertexColors (int[] triangles, Color[] colors) 
	{
		for (int i = 0; i < triangles.Length ; i += 3) 
		{
			Color color0 = colors [triangles[i]];
			Color color1 = colors [triangles[i+1]];
			Color color2 = colors [triangles[i+2]];
			Color avgColor = color0 + color1 + color2;

			avgColor = ClampColor (avgColor);

			avgColor.r *= 0.333f;
			avgColor.g *= 0.333f;
			avgColor.b *= 0.333f;
			avgColor.a *= 0.333f;

			colors [triangles[i]] = avgColor;
			colors [triangles[i+1]] = avgColor;
			colors [triangles[i+2]] = avgColor;
		}
		return colors;
	}

	void StopPainting ()
	{
		CleanUp ();
	}

	void CleanUp ()
	{
		isUsing = false;

		if (showVertColor) 
		{
			if (showVertAlpha) 
			{
				showVertAlpha = false;
				ActivateAlpha (showVertAlpha);
			}
			showVertColor = false;
			SwitchMaterial (showVertColor);
		}

		if (meshesToPaint != null)
				meshesToPaint.Clear ();

		if (nameMeshes != null)
				nameMeshes.Clear ();

		if (verticesToPaint != null)
				verticesToPaint.Clear ();

		if (transformToPaint != null)
				transformToPaint.Clear ();		

		Tools.current = oldTool;

		if (matProjector != null)
			DestroyImmediate (matProjector);

		if (projectorBrush != null)
			DestroyImmediate (transformBrush.gameObject);
	}

//	void OnDisable()
//	{
//		SceneView.onSceneGUIDelegate += SceneGUI;
//	}

	void OnDestroy ()
	{
		Tools.current = Tool.Move;
		CleanUp ();
		SceneView.onSceneGUIDelegate -= SceneGUI;
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}
}
