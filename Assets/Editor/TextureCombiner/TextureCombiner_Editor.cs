using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCombiner_Editor : EditorWindow 
{
	Texture2D rChannel;
	Texture2D gChannel;
	Texture2D bChannel;
	Texture2D aChannel;
	bool[] inverseChannels;
	string nameTex = "";
	bool[] stateRead;

	[MenuItem ("Tools/Texture Combiner")]
	static void Init () 
	{
		TextureCombiner_Editor window = (TextureCombiner_Editor)EditorWindow.GetWindow (typeof (TextureCombiner_Editor));
		window.Show();
	}

	void OnEnable ()
	{
		stateRead = new bool[4] { false, false, false, false };
		inverseChannels = new bool[4] { false, false, false, false };
	}

	void OnGUI()
	{
		EditorGUIUtility.labelWidth = 125f;

		inverseChannels [0] = EditorGUILayout.Toggle ("Inverse R channel", inverseChannels [0]);
		rChannel = EditorGUILayout.ObjectField ("R (Original Tex)", rChannel, typeof(Texture2D), false) as Texture2D;
		inverseChannels [1] = EditorGUILayout.Toggle ("Inverse G channel", inverseChannels [1]);
		gChannel = EditorGUILayout.ObjectField ("G", gChannel, typeof(Texture2D), false) as Texture2D;
		inverseChannels [2] = EditorGUILayout.Toggle ("Inverse B channel", inverseChannels [2]);
		bChannel = EditorGUILayout.ObjectField ("B", bChannel, typeof(Texture2D), false) as Texture2D;
		inverseChannels [3] = EditorGUILayout.Toggle ("Inverse A channel", inverseChannels [3]);
		aChannel = EditorGUILayout.ObjectField ("A", aChannel, typeof(Texture2D), false) as Texture2D;

		nameTex = EditorGUILayout.TextField ("Texture name", nameTex);

		if (rChannel != null && nameTex == "")
			nameTex = rChannel.name;

		if (GUILayout.Button ("Combine Texture"))
			OnCombinePressed ();

		if (GUI.changed)
			Repaint ();
	}

	void OnCombinePressed ()
	{
		if (rChannel == null)
			EditorUtility.DisplayDialog ("Warning!", "R channel couldn't be empty (Original texture).", "Ok");
		else if (gChannel == null && bChannel == null && aChannel == null)
			EditorUtility.DisplayDialog ("Warning!", "You must have at least one G B or A channel filled.", "Ok");
		else
			CombineTexture ();
	}

	void CheckReadableTex (Texture2D tex, bool state, int indice)
	{
		string texPath = AssetDatabase.GetAssetPath (tex);
		TextureImporter ti = AssetImporter.GetAtPath (texPath) as TextureImporter;

		if (ti.isReadable != state) 
		{
			stateRead [indice] = !state;
			ti.isReadable = state;
			ti.SaveAndReimport ();
		}
	}

	void CombineTexture ()
	{		
		TextureFormat newTexFormat = TextureFormat.RGB24;

		if (aChannel != null || rChannel.format == TextureFormat.ARGB32)
			newTexFormat = TextureFormat.ARGB32;
		
		Texture2D newTex = new Texture2D (rChannel.width, rChannel.height, newTexFormat, true, true);

		CheckReadableTex (rChannel, true, 0);

		//Get Base RGB Channel
		Color32[] rChannelColors = new Color32[newTex.width * newTex.height];
		rChannelColors = rChannel.GetPixels32 ();

		//Add G Channel
		if (gChannel != null)
		{
			CheckReadableTex (gChannel, true, 1);

			Color32[] gChannelColors = new Color32 [newTex.width * newTex.height];
			gChannelColors = gChannel.GetPixels32 ();

			for (int i = 0; i < rChannelColors.Length; i++) 
					rChannelColors[i].g = gChannelColors[i].r;

			CheckReadableTex (gChannel, stateRead [1], 1);
		}

		if (bChannel != null)
		{
			CheckReadableTex (bChannel, true, 2);

			Color32[] bChannelColors = new Color32 [newTex.width * newTex.height];
			bChannelColors = bChannel.GetPixels32 ();

			for (int i = 0; i < rChannelColors.Length; i++) 
				rChannelColors[i].b = bChannelColors[i].r;

			CheckReadableTex (bChannel, stateRead [2], 2);
		}

		if (aChannel != null)
		{
			CheckReadableTex (aChannel, true, 3);

			Color32[] aChannelColors = new Color32 [newTex.width * newTex.height];
			aChannelColors = aChannel.GetPixels32 ();

			for (int i = 0; i < rChannelColors.Length; i++) 
				rChannelColors [i].a = aChannelColors[i].r;
			
			CheckReadableTex (aChannel, stateRead [3], 3);
		}

		for (int i = 0; i < rChannelColors.Length; i++) 
		{
			Color32 myColor = rChannelColors [i];
			if (inverseChannels [0]) 
				myColor.r = (byte)(1 - myColor.r);
			if (inverseChannels [1])
				myColor.g = (byte)(1 - myColor.g);
			if (inverseChannels [2])
				myColor.b = (byte)(1 - myColor.b);
			if (inverseChannels [3])
				myColor.a = (byte)(1 - myColor.a);
			
			rChannelColors [i] = myColor;
		}

		newTex.SetPixels32 (rChannelColors);
		newTex.Apply ();

		byte[] bytesNewTex = newTex.EncodeToTGA ();

		string path = AssetDatabase.GetAssetPath (rChannel);
		string[] name = path.Split ("/" [0]);

		path = "";
		for (int i = 0; i < name.Length - 1; i++) 
			path += name [i] + "/";
		

		if (nameTex == rChannel.name)
			nameTex += "_combined";
				
		File.WriteAllBytes (path + nameTex + ".tga", bytesNewTex);

		CheckReadableTex (rChannel, stateRead[0], 0);
		AssetDatabase.Refresh ();
	}
}
