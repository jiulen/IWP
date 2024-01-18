using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    int p2Action;
}

public class ReplayManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
