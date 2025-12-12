using System.Collections.Generic;
using System;

[Serializable]
public class CardStateInfo
{
    public string id;    
    public string word;  
    public int index;    // board position (0-24)
    public string owner; // "red", "blue", "neutral", "bomb"
}

[Serializable]
public class BoardStateData
{
    public string room;
    public List<CardStateInfo> cards;
}