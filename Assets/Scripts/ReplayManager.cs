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

    public void UnwrapTurns()
    {
        turns = wrappedTurns.list;
    }

    public void AddTurn(int _frameNum, int _playerAction, bool _playerFlip, int _playerNum)
    {
        Turn newTurn = new Turn(_frameNum, _playerAction, _playerFlip, _playerNum);
        turns.Add(newTurn);
    }
}

[System.Serializable]
public class ReplayPlayer
{
    public string playerName;
    public int skinID;
    public bool isMe;

    public ReplayPlayer (string _name, int _skinID, bool _isMe)
    {
        playerName = _name;
        skinID = _skinID;
        isMe = _isMe;
    }
}

[System.Serializable]
public class Turn
{
    public int frameNum;
    public int playerAction;
    public bool playerFlip;
    public int playerNum; // 1 or 2

    public Turn (int _frameNum, int _playerAction, bool _playerFlip, int _playerNum)
    {
        frameNum = _frameNum;
        playerAction = _playerAction;
        playerFlip = _playerFlip;
        playerNum = _playerNum;
    }
}

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance = null;

    public Replay replay;

    public int replayTurn = -1;
    public bool replayPaused = false;

    public void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    //Replay file
    public FileInfo[] LoadReplayFiles()
    {
        string replayFolderPath = Application.persistentDataPath + "/Replays";

        if (Directory.Exists(replayFolderPath))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(replayFolderPath);
            return directoryInfo.GetFiles("*.replay");
        }
        else
        {
            Directory.CreateDirectory(replayFolderPath);
            return new FileInfo[0];
        }
    }

    public bool SetCurrentReplay(string replayFilePath)
    {
        if (File.Exists(replayFilePath))
        {
            string replayFileContents = File.ReadAllText(replayFilePath);
            replay = JsonUtility.FromJson<Replay>(replayFileContents);
            replay.UnwrapTurns();

            return true;
        }

        return false;
    }

    public void AddReplay(string newReplayName, string newReplayText)
    {
        string newReplayFilePath = Application.persistentDataPath + "/Replays/" + newReplayName + ".replay";

        int dupeCount = 1;

        while (File.Exists(newReplayFilePath))
        {
            string tempFileName = string.Format("{0} ({1})", newReplayName, dupeCount++);
            newReplayFilePath = Application.persistentDataPath + "/Replays/" + tempFileName + ".replay";
        }

        File.WriteAllText(newReplayFilePath, newReplayText);
    }

    public void RemoveReplay(string removingReplayname)
    {
        string removingReplayPath = Application.persistentDataPath + "/Replays/" + removingReplayname + ".replay";

        if (!File.Exists(removingReplayPath))
        {
            Debug.Log("Replay file not found");
            return;
        }

        File.Delete(removingReplayPath);
    }

    //Replay writing (only host can write)
    public void StartRecording(string hostname, int hostID, string clientname, int clientID)
    {
        replay = new(new ReplayPlayer(hostname, hostID, true) , new ReplayPlayer(clientname, clientID, false));
    }

    //Converting to and from JSON
    public string ReplayToJson()
    {
        string replayAsJSON = JsonUtility.ToJson(replay);

        return replayAsJSON;
    }
    public void JsonToReplay(string replayJson)
    {
        replay = JsonUtility.FromJson<Replay>(replayJson);
    }
}
