using UnityEngine;

public class HashTableTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var hashTable = new SimpleHashTable<string, int>();

        hashTable.Add("하나", 1);
        hashTable.Add("둘", 2);
        hashTable.Add("셋", 3);
        
        foreach(var kvp in hashTable)
        {
            Debug.Log($"[{kvp.Key},{kvp.Value}]");
        }
        foreach (var key in hashTable.keys)
        {
            Debug.Log(key);
        }
        Debug.Log($"탐색 하나:{hashTable.ContainsKey("하나")}");

        hashTable.Remove("하나");

        Debug.Log($"탐색 하나: {hashTable.ContainsKey("하나")}");
    }

    
}
