using TMPro;
using UnityEngine;

public class HashTableDebugger : MonoBehaviour
{
    public TextMeshProUGUI outputText;

    private ChainingHashTable<string, int> table;

    void Start()
    {
        table = new ChainingHashTable<string, int>();
        table.Add("Apple", 1);
        table.Add("Banana", 2);
        table.Add("Cherry", 3);
        table["Banana"] = 99;

        UpdateUI();
    }

    void UpdateUI()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Count: {table.Count}");
        sb.AppendLine("========== Hash Table ==========");

        int i = 0;
        foreach (var list in table.DebugEnumerateBuckets())
        {
            sb.Append($"[{i}] ");
            if (list == null)
                sb.AppendLine("empty");
            else
            {
                foreach (var kv in list)
                    sb.Append($"({kv.Key}:{kv.Value}) ");
                sb.AppendLine();
            }
            i++;
        }

        outputText.text = sb.ToString();
    }
}
