using UnityEngine;
using Janus;
using System.Collections.Generic;
using System.Reflection;
using System;

[AddComponentMenu("Janus/Timeline Client Starter")]
public class TimelineClientStarter : MonoBehaviour
{
	static TimelineClientStarter _instance;

	public bool UseIniFile = false;                // get client settings from an ini file or from this script
	public string ServerAddress = "127.0.0.1";     // ip address for the timeline server
	public int ServerPort = 14242;                 // port number to use for communication with the server   
	public int StepRate = 60;                      // update frequency in frames per second

	void Awake ()
	{
		if (_instance == null)
		{
			UnityTimelineUtils.SetDefautTimelineFunctions();  // initialize behaviour for Unity Timeline classes
			DontDestroyOnLoad(this);
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	void Start ()
	{		
		// Start the Timeline client either based on the values in an ini file or based on the settings in this file
		if(UseIniFile)
		{
			TimelineClient.Start(true, false);
		}
		else
		{
			TimelineClient.Start(false, false);
			TimelineClient.Connect(ServerAddress, ServerPort);
		}
	}
	
	// Increment the Timeline time
	void Update ()
	{
		TimelineClient.Step(Time.deltaTime);
	}
	
	// Cleanup when application closes
	void OnDestroy ()
	{
		TimelineClient.Stop();

		if (_instance == this)
			_instance = null;
	}
}
		
