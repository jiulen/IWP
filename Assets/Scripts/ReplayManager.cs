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
    public ReplayPlayer p1;
    public ReplayPlayer p2;
    public int winner;
    public int lastFrame; //check when replay ends

    [System.NonSerialized]
    public List<Turn> turns;

    public JSListWrapper<Turn> wrappedTurns;

    public Replay(ReplayPlayer _p1, ReplayPlayer _p2)
    {
        p1 = _p1;
        p2 = _p2;

        winner = -1; //undecided
        lastFrame = -1;

        turns = new List<Turn>();
        wrappedTurns = null;
    }

    public void WrapTurns()
    {
        wrappedTurns = new JSListWrapper<Turn>(turns);
    }

    public void AddTurn(int _frameNum, int _p1Action, bool _p1Flip, int _p2Action, bool _p2Flip)
    {
        Turn newTurn = new Turn(_frameNum, _p1Action, _p1Flip, _p2Action, _p2Flip);
        turns.Add(newTurn);
    }
}

[System.Serializable]
public class ReplayPlayer
{
    public string name;
    public int skinID;
    public bool isMe;

    public ReplayPlayer (string _name, int _skinID, bool _isMe)
    {
        name = _name;
        skinID = _skinID;
        isMe = _isMe;
    }
}

[System.Serializable]
public class Turn
{
    public int frameNum;
    public int p1Action;
    public bool p1Flip;
    public int p2Action;
    public bool p2Flip;

    public Turn (int _frameNum, int _p1Action, bool _p1Flip, int _p2Action, bool _p2Flip)
    {
        frameNum = _frameNum;
        p1Action = _p1Action;
        p1Flip = _p1Flip;
        p2Action = _p2Action;
        p2Flip = _p2Flip;
    }
}

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance = null;

    public Replay replay;

    public void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
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
            return new FileInfo[0];
        }
    }

    public void AddReplay(string newReplayName, string newReplayText)
    {
        string newReplayFilePath = Application.persistentDataPath + "/Replays/" + newReplayName + ".txt";

        int dupeCount = 1;

        while (File.Exists(newReplayFilePath))
        {
            string tempFileName = string.Format("{0} ({1})", newReplayName, dupeCount++);
            newReplayFilePath = Application.persistentDataPath + "/Replays/" + tempFileName + ".txt";
        }

        File.WriteAllText(newReplayFilePath, newReplayText);
    }

    public void RemoveReplay(string removingReplayname)
    {
        string removingReplayPath = Application.persistentDataPath + "/Replays/" + removingReplayname + ".txt";

        if (!File.Exists(removingReplayPath))
        {
            Debug.Log("Replay file not found");
            return;
        }

        File.Delete(removingReplayPath);
    }
}
