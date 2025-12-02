[System.Serializable]
public class WordRecord
{
    public string id;
    public string en;
    public string he;
    public string ru;
    public string cs;
}

[System.Serializable]
public class WordRecordCollection
{
    public WordRecord[] words;
}
