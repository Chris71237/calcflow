﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogViewer : MonoBehaviour {
    public int numCurrentSpawns;

    public int lookupKey;

    public string lookupResult;
    public bool showFullLog_ThisCausesLag = false;
    public List<PlaybackLogEntry> log;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        numCurrentSpawns = Recorder.SpawnQueueSize();
        // if (PlaybackLogAction2.objectMap[lookupKey] != null){
        //     lookupResult = PlaybackLogAction2.objectMap[lookupKey].name;
        // } else {
        //     lookupResult = "______";
        // }
        //if (numCurrentSpawns > 0) print(numCurrentSpawns);
        if (showFullLog_ThisCausesLag)
            log = Recorder.recordLog.GetLogCopy();
    }
}
