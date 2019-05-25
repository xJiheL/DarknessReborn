using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MeshDecals_Build : MonoBehaviour
{
	[SerializeField, HideInInspector]List<Texture2D> decals;
	public List<Texture2D> Decals 
	{
		get 
		{
			return decals;
		}
	}

	[SerializeField, HideInInspector]Texture2D selectedDecal;
	public Texture2D SelectedDecal 
	{
		get 
		{
			return selectedDecal;
		}
		set 
		{
			selectedDecal = value;
		}
	}

	[SerializeField]LayerMask targetLayers;
	[SerializeField, HideInInspector]Material myMaterial;
	[SerializeField]bool autoUpdate = true;
	public bool AutoUpdate 
	{
		get 
		{
			return autoUpdate;
		}
		set 
		{
			autoUpdate = value;
		}
	}

	[SerializeField]bool _tarDecal = true;
	public bool TarDecal 
	{
		get 
		{
			return _tarDecal;
		}
		set 
		{
			_tarDecal = value;
		}
	}
	MeshRenderer meshRend;
	MeshFilter meshFilt;

#if UNITY_EDITOR	
	Plane right = new Plane (Vector3.right, 0.5f);
	Plane left = new Plane (Vector3.left, 0.5f);
	Plane top = new Plane (Vector3.up, 0.5f);
	Plane bottom = new Plane (Vector3.down, 0.5f);
	Plane front = new Plane (Vector3.forward, 0.5f);
	Plane back = new Plane (Vector3.back, 0.5f);

	List<Vector3> vertices = new List<Vector3> ();
	List<Vector3> normals = new List<Vector3> ();
	List<Vector2> texCoords = new List<Vector2> ();
	List<int> indices = new List<int> ();

	void OnEnable () 
	{
		Texture2D[] tags = Resources.LoadAll ("Decals/Graff", typeof (Texture2D)).Cast <Texture2D> ().ToArray ();

		if (decals == null)
			decals = new List<Texture2D> (tags);
		else if (decals.Count != tags.Length)
			for (int i = decals.Count - 1; i < tags.Length; i++)
				decals.Add (tags[i]);

//		if (selectedDecal == null && decals != null)
//			selectedDecal = decals[0];


		meshRend = GetComponent <MeshRenderer> ();
		meshFilt = GetComponent <MeshFilter> ();

		if (myMaterial == null && meshRend.sharedMaterial == null)
			myMaterial = new Material (Shader.Find ("Standard"));
		else if (myMaterial == null || meshRend.sharedMaterial != null)
			myMaterial = meshRend.sharedMaterial;

		meshRend.material = myMaterial;
	}
	
	void OnDrawGizmosSelected ()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube (Vector3.zero, Vector3.one);

		if (autoUpdate)
			BuildDecal ();
	}

	public void BuildDecal ()
	{
		MeshFilter[] touchMeshes = DetectMesh ();

		for (int i = 0; i < touchMeshes.Length; i++) 
			Build (touchMeshes[i]);

		if (meshFilt.sharedMesh == null) 
		{
			meshFilt.sharedMesh = new Mesh ();
			meshFilt.sharedMesh.name = "Decal";
		}

		Push (0.001f);
		ToMesh (meshFilt.sharedMesh);
	}

	// A checker
	MeshFilter[] DetectMesh ()
	{
		Bounds bounds = GetBounds ();

		MeshFilter[] targetsMesh = GameObject.FindObjectsOfType <MeshRenderer> ()
			.Where (obj => obj.gameObject.isStatic)
			.Where (obj => CheckLayer (obj.gameObject))
			.Where (obj => obj.GetComponent <MeshDecals_Build> () == null)
			.Where (obj => bounds.Intersects (obj.bounds))

			.Select( obj => obj.GetComponent<MeshFilter>() )
			.Where( obj => obj != null && obj.sharedMesh != null )
			.ToArray();

		return targetsMesh;
	}

	Bounds GetBounds ()
	{		
		Vector3 scale = transform.lossyScale;
		Vector3 min = -scale / 2f;
		Vector3 max = scale / 2f;

		Vector3[] verts = new Vector3[] {
			new Vector3 (min.x, min.y, min.z),
			new Vector3 (max.x, min.y, min.z),
			new Vector3 (min.x, max.y, min.z),
			new Vector3 (max.x, max.y, min.z),

			new Vector3 (min.x, min.y, max.z),
			new Vector3 (max.x, min.y, max.z),
			new Vector3 (min.x, max.y, max.z),
			new Vector3 (max.x, max.y, max.z),
		};

		verts = verts.Select (transform.TransformDirection ).ToArray();
		min = verts.Aggregate (Vector3.Min);
		max = verts.Aggregate (Vector3.Max);

		return new Bounds (transform.position, max - min);
	}

	// A checker
	bool CheckLayer (GameObject target)
	{
		return (targetLayers.value & 1 << target.layer) != 0;
	}

	void Build (MeshFilter meshFilter)
	{
		Matrix4x4 objToDecalMatrix = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;

		Mesh mesh = meshFilter.sharedMesh;
		
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;

		for (int i = 0; i < triangles.Length; i += 3) 
		{
			int i1 = triangles[i];
			int i2 = triangles[i + 1];
			int i3 = triangles[i + 2];

			Vector3 v1 = objToDecalMatrix.MultiplyPoint (vertices[i1]);
			Vector3 v2 = objToDecalMatrix.MultiplyPoint (vertices[i2]);
			Vector3 v3 = objToDecalMatrix.MultiplyPoint (vertices[i3]);

			AddTriangle (v1, v2, v3);
		}
	}

	void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) 
	{
		Rect rect = new Rect (Vector2.zero, new Vector2 (1024, 1024));
		Rect uvRect = To01 (rect, new Texture2D(1024,1024));
		Vector3 normal = Vector3.Cross (v2 - v1, v3 - v1).normalized;

		if (Vector3.Angle (Vector3.forward, -normal) <= 90f) 
		{
			Vector3[] poly = Clip (v1, v2, v3);
			if (poly.Length > 0) 
				AddPolygon (poly, normal, uvRect);
		}
	}

	Rect To01 (Rect rect, Texture2D texture) 
	{
		rect.x /= texture.width;
		rect.y /= texture.height;
		rect.width /= texture.width;
		rect.height /= texture.height;
		return rect;
	}

	Vector3[] Clip (params Vector3[] poly) 
	{
		poly = Clip (poly, right).ToArray ();
		poly = Clip (poly, left).ToArray ();
		poly = Clip (poly, top).ToArray ();
		poly = Clip (poly, bottom).ToArray ();
		poly = Clip (poly, front).ToArray ();
		poly = Clip (poly, back).ToArray ();
		return poly;
	}

	IEnumerable<Vector3> Clip (Vector3[] poly, Plane plane) 
	{
		for (int i = 0; i < poly.Length; i++) 
		{
			int next = (i + 1) % poly.Length;
			Vector3 v1 = poly[i];
			Vector3 v2 = poly[next];

			if (plane.GetSide (v1)) 
				yield return v1;

			if (plane.GetSide (v1) != plane.GetSide (v2)) 
				yield return PlaneLineCast (plane, v1, v2);
		}
	}

	private static Vector3 PlaneLineCast (Plane plane, Vector3 a, Vector3 b) 
	{
		float dis;
		Ray ray = new Ray (a, b - a);
		plane.Raycast (ray, out dis);
		return ray.GetPoint (dis);
	}

	void AddPolygon (Vector3[] poly, Vector3 normal, Rect uvRect) 
	{
		int ind1 = AddVertex (poly[0], normal, uvRect);

		for (int i = 1; i < poly.Length - 1; i++) 
		{
			int ind2 = AddVertex (poly[i], normal, uvRect);
			int ind3 = AddVertex (poly[i + 1], normal, uvRect);

			indices.Add (ind1);
			indices.Add (ind2);
			indices.Add (ind3);
		}
	}

	private int AddVertex (Vector3 vertex, Vector3 normal, Rect uvRect) 
	{
		int index = FindVertex (vertex);
		if (index == -1) 
		{
			vertices.Add (vertex);
			normals.Add (normal);
			AddTexCoord (vertex, uvRect);
			return vertices.Count - 1;
		}
		else 
		{
			normals[index] = (normals[index] + normal).normalized;
			return index;
		}
	}

	private int FindVertex(Vector3 vertex) 
	{
		for (int i = 0; i < vertices.Count; i++) 
		{
			if (Vector3.Distance (vertices[i], vertex) < 0.01f) 
				return i;
		}
		return -1;
	}

	private void AddTexCoord (Vector3 ver, Rect uvRect) 
	{
		float u = Mathf.Lerp (uvRect.xMin, uvRect.xMax, ver.x + 0.5f);
		float v = Mathf.Lerp (uvRect.yMin, uvRect.yMax, ver.y + 0.5f);
		texCoords.Add (new Vector2 (u, v));
	}

	void ToMesh (Mesh mesh) 
	{
		mesh.Clear (true);
		if (indices.Count == 0) 
			return;

		mesh.vertices = vertices.ToArray ();
		mesh.normals = normals.ToArray ();
		mesh.uv = texCoords.ToArray ();
		mesh.uv2 = texCoords.ToArray ();
		mesh.triangles = indices.ToArray ();


		vertices.Clear ();
		normals.Clear ();
		texCoords.Clear ();
		indices.Clear ();
	}

	void Push (float distance) 
	{
		for (int i = 0; i < vertices.Count; i++)
			vertices[i] += normals[i] * distance;		
	}
#endif

//	void OnDestroy ()
//	{
//		DestroyImmediate (myMaterial);
//	}
}
