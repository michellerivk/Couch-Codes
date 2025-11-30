using UnityEngine;

[CreateAssetMenu(fileName = "CardWord", menuName = "Codenames/Card Word")]
public class CardWord : ScriptableObject
{
    public string id;       // The id - for example: dog
    public string english;  // English translation: Dog
    public string hebrew;   // Hebrew translation: kelev
}
