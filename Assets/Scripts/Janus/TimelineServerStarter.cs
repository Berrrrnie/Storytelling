using UnityEngine;
using System.Collections;
using Janus;
using Jeli;

public class TimelineServerStarter : MonoBehaviour {
		
	public bool useIniFile = false;     // get client settings from an ini file or from this script
	public string iniFileName = "JanusCubes"; // name of ini file that determines if this client is also a server
	public bool isServer = true;        // if not using the an ini file, the TimelineServer is started if isServer is true

    // Start the TimelineServer if this client is acting as a server
	// One (and only one) client must be selected as a server
	void Awake () 
    {
        Config config=Config.Load(Application.dataPath+"/../" + iniFileName + ".ini");
		
		if (config == null)
		{
			config = new Config();
			config.AddChild("IsServer", isServer);
			config.Save(Application.dataPath+"/../" + iniFileName + ".ini");

		}

        if (useIniFile && config["IsServer"].BoolValue) 
        {
            DontDestroyOnLoad(this);
            TimelineServer.Start(true);
			Debug.Log("Server Started");
        }
			else
		{
			config["IsServer"].BoolValue = isServer;
			config.Save(Application.dataPath+"/../" + iniFileName + ".ini");
			if (isServer)
			{
				DontDestroyOnLoad(this);
                TimelineServer.Start(true);
			    Debug.Log("Server Started");
			}
				
		}
    }
}
