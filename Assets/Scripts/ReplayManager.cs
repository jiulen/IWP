using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class JSListWrapper<T>
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}

[System.Serializable]
public class Replay
{
    ReplayPlayer p1;
    ReplayPlayer p2;
    int winner;

    JSListWrapper<Turn> turns;
}

[System.Serializable]
public class ReplayPlayer
{
    string name;
    int skinID;
    bool isMe;
}

[System.Serializable]
public class Turn
{
    int turnNum;
    int p1Action;
    bool p1Flip;
    int p2Action;
    bool p2Flip;
}

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance = null;

    Replay replay;

    public void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public FileInfo[] LoadReplayFiles()
    {
        string replayFolderPath = Application.persistentDataPath + "/Replays";

        if (Directory.Exists(replayFolderPath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(replayFolderPath);
            return directoryInfo.GetFiles("*.txt");
        }
        else
        {
            Directory.CreateDirectory(replayFolderPath);
            return null;
        }
    }
}
