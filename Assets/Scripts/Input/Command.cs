using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FranSTools.Essential;

public class Command : MonoBehaviourSingleton<Command>
{
	Dictionary<EnumCommand, InputBase> dictionary = new Dictionary<EnumCommand, InputBase>();
	#if UNITY_EDITOR 
	List<string> dictionaryContent = new List<string>();
	#endif

	public bool joystickConnected { get; private set; }

	string currentJoystickNames = "";
	InputDatabase currentDatabase;

	protected override void SingletonAwake()
	{
		if(Input.GetJoystickNames().Length > 0)
			currentJoystickNames = Input.GetJoystickNames()[0];
		LoadDatabase();
	}

	#region ---------- Database ----------

	private void LoadDatabase()
	{
		//change les input du dico actuel

		if( currentJoystickNames.Equals("") )
		{
			currentDatabase = Resources.Load("Input Database/Keyboard Mouse") as InputDatabase;
			joystickConnected = false;
		}
		else if( 
			currentJoystickNames.Equals("Controller (XBOX 360 For Windows)") || 
			currentJoystickNames.Equals("XBOX 360 For Windows (Controller)") 
		)
		{
			currentDatabase = Resources.Load("Input Database/XBOX 360") as InputDatabase;
			joystickConnected = true;
		}
		else
		{
			Check.Crash($"Unknown device!\n{currentJoystickNames}");
		}
	}

	#endregion

	public void Add( EnumCommand enumCommand, Action action )
	{
		if( dictionary.ContainsKey( enumCommand ) )
		{
			Debug.LogError("Add Command "+enumCommand+" : Command already exists !");
			return;
		}

		InputEntry newEntry = currentDatabase.GetEntry( enumCommand );

		switch (newEntry.Type)
		{
		case InputType.Button:
			StartCoroutine( CoroutineAdd( enumCommand, new InputButton( action, newEntry ) ) );
			break;

		case InputType.Axis1D:
			StartCoroutine( CoroutineAdd( enumCommand, new InputAxis1D( action, newEntry ) ) );
			break;

		case InputType.Axis2D:
			StartCoroutine( CoroutineAdd( enumCommand, new InputAxis2D( action, newEntry ) ) );
			break;
		}
	}

	public void Add( EnumCommand enumCommand, Action<float> action )
	{
		if( dictionary.ContainsKey( enumCommand ) )
		{
			Debug.LogError("Add Command "+enumCommand+" : Command already exists !");
			return;
		}

		InputEntry newEntry = currentDatabase.GetEntry( enumCommand );

		if( newEntry.Type == InputType.Axis1D )
			StartCoroutine( CoroutineAdd( enumCommand, new InputAxis1D( action, newEntry ) ) );
	}

	public void Add( EnumCommand enumCommand, Action<Vector2> action )
	{
		if( dictionary.ContainsKey( enumCommand ) )
		{
			Debug.LogError("Add Command "+enumCommand+" : Command already exists !");
			return;
		}

		InputEntry newEntry = currentDatabase.GetEntry( enumCommand );

		if( newEntry.Type == InputType.Axis2D )
			StartCoroutine( CoroutineAdd( enumCommand, new InputAxis2D( action, newEntry ) ) );
	}

	public void Remove( EnumCommand enumCommand )
	{
		if( !dictionary.ContainsKey( enumCommand ) )
		{
			Debug.LogError("Remove Command "+enumCommand+" : Command doesn't exists !");
			return;
		}

		//si je supprime durant l'execution de l'update il kiffe pas
		StartCoroutine( CoroutineRemove(enumCommand) );
	}

	private IEnumerator CoroutineAdd ( EnumCommand enumCommand, InputBase inputBase )
	{
		yield return new WaitForEndOfFrame();
		dictionary.Add( enumCommand, inputBase );
		#if UNITY_EDITOR
		dictionaryContent.Add(enumCommand.ToString());
		#endif
	}

	private IEnumerator CoroutineRemove ( EnumCommand enumCommand )
	{
		yield return new WaitForEndOfFrame();
		dictionary.Remove( enumCommand );
		#if UNITY_EDITOR
		dictionaryContent.Remove(enumCommand.ToString());
		#endif
	}

	void Update()
	{
		if( Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0] != currentJoystickNames )
		{
			currentJoystickNames = Input.GetJoystickNames()[0];
			LoadDatabase();
		}

		//attention à la caméra en premier !
		foreach( KeyValuePair<EnumCommand, InputBase> kvp in dictionary )
		{
			kvp.Value.Check();
		}
			
//		List<TypeAction> listKeys = new List<TypeAction>( dico.Keys );
//		for (int i = 0; i < listKeys.Count; i++) 
//		{//dico pas d'index
//			dico[ listKeys[i] ].Check();		
//		}
	}
}
