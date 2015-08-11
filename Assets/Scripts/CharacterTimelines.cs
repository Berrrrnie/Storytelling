using UnityEngine;
using System.Collections;
using Janus;
using Jeli;
using System.Collections.Generic;

public class CharacterTimelines : MonoBehaviour {

	GameObject playerCharacter;
	public Timeline<string> characterCommand;

	// Use this for initialization
	void Start () {
		Debug.Log("Starting Timelines Script");

		Application.runInBackground = true;

		// create a timeline 
		characterCommand = TimelineManager.Default.Get<string>("command");
		// listen for commands being inserted on both  local and remote clients
		characterCommand.EntryInserted += HandleCharacterCommandEntryInserted;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void HandleCharacterCommandEntryInserted(Timeline<string> timeline, TimelineEntry<string> entry){

		Debug.Log("Character command recieved");

		// the various parts of the command are separated by commas
		string[]tokens = entry.Value.Split(',');
		string action = tokens[0];
		
	}
}
