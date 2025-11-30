using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordDatabase", menuName = "Codenames/Word Database")] // Add this property to the unity menu
public class WordDataBase : ScriptableObject
{
    public List<CardWord> words;
}
