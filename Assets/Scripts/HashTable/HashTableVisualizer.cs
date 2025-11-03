using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashTableVisualizer : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField keyInput;
    public TMP_InputField valueInput;
    public Button addButton;
    public Button setButton;
    public Button getButton;
    public Button removeButton;
    public Button clearButton;
    public Button autoFillButton;

    public RectTransform gridParent;         // GridLayoutGroup 있는 오브젝트
    public GameObject bucketCellPrefab;      // BucketCell 프리팹
    public TextMeshProUGUI logText;          // 결과/로그 출력용

    private ChainingHashTable<string, int> table;
    private BucketCell[] cells;

    void Start()
    {
        table = new ChainingHashTable<string, int>(); // 기본 16 버킷
        BuildGrid();

        addButton.onClick.AddListener(OnAdd);
        setButton.onClick.AddListener(OnSet);
        getButton.onClick.AddListener(OnGet);
        removeButton.onClick.AddListener(OnRemove);
        clearButton.onClick.AddListener(OnClear);
        autoFillButton.onClick.AddListener(OnAutoFill);

        RefreshGrid();
        Log("Ready.");
    }

    private void BuildGrid()
    {
        // 기존 셀 제거
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        int n = table.DebugSize;
        cells = new BucketCell[n];

        for (int i = 0; i < n; i++)
        {
            var go = Instantiate(bucketCellPrefab, gridParent);
            go.name = $"Bucket_{i:00}";
            var cell = go.GetComponent<BucketCell>();
            cells[i] = cell;
        }
    }

    private void RefreshGrid()
    {
        var buckets = table.DebugSnapshot().ToList();
        int n = buckets.Count;

        // 사이즈가 리사이즈되어 변경되었으면 다시 빌드
        if (cells == null || cells.Length != n)
        {
            BuildGrid();
        }

        for (int i = 0; i < n; i++)
        {
            var list = buckets[i];
            // "(k:v) (k:v) ..." 형태로 표시
            string content = list.Count == 0
                ? $"[{i}] empty"
                : $"[{i}] " + string.Join(" ", list.Select(kv => $"({kv.Key}:{kv.Value})"));

            cells[i].SetContent(content, list.Count);
        }
    }

    // ===== 버튼 핸들러 =====

    private void OnAdd()
    {
        if (!TryParseKV(out var k, out var v)) return;

        try
        {
            table.Add(k, v);
            Log($"Add: {k} => {v}");
        }
        catch (Exception e)
        {
            Log($"Add failed: {e.Message}");
        }
        RefreshGrid();
    }

    private void OnSet()
    {
        if (!TryParseKV(out var k, out var v)) return;
        try
        {
            table[k] = v;
            Log($"Set: {k} => {v}");
        }
        catch (Exception e)
        {
            Log($"Set failed: {e.Message}");
        }
        RefreshGrid();
    }

    private void OnGet()
    {
        var k = keyInput.text ?? "";
        if (string.IsNullOrWhiteSpace(k))
        {
            Log("Get: key is empty");
            return;
        }
        if (table.TryGetValue(k, out var val))
            Log($"Get: {k} => {val}");
        else
            Log($"Get: {k} not found");

        RefreshGrid();
    }

    private void OnRemove()
    {
        var k = keyInput.text ?? "";
        if (string.IsNullOrWhiteSpace(k))
        {
            Log("Remove: key is empty");
            return;
        }
        bool ok = table.Remove(k);
        Log(ok ? $"Remove: {k} ✓" : $"Remove: {k} (not found)");
        RefreshGrid();
    }

    private void OnClear()
    {
        table.Clear();
        Log("Cleared.");
        RefreshGrid();
    }

    private void OnAutoFill()
    {
        // 샘플 데이터 채우기 (충돌 시각화 목적)
        string[] keys = { "Apple", "Banana", "Cherry", "Dates", "Elder", "Fig", "Grape",
                          "Honey", "Ice", "Jack", "Kiwi", "Lemon", "Mango", "Nectar", "Orange",
                          "Peach", "Quince", "Raisin", "Straw", "Tomato", "Ugli", "Vanilla",
                          "Water", "Xigua", "Yam", "Zucchini" };

        var rand = new System.Random(1234);
        foreach (var k in keys)
        {
            int v = rand.Next(1, 100);
            try { table.Add(k, v); } catch { /* 중복 시 스킵 */ }
        }

        Log("Auto-filled sample keys.");
        RefreshGrid();
    }

    private bool TryParseKV(out string key, out int value)
    {
        key = keyInput.text ?? "";
        if (string.IsNullOrWhiteSpace(key))
        {
            Log("Key is empty");
            value = default;
            return false;
        }
        if (!int.TryParse(valueInput.text, out value))
        {
            Log("Value must be an integer");
            return false;
        }
        return true;
    }

    private void Log(string msg)
    {
        if (!logText) return;
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
        sb.Append(logText.text);
        logText.text = sb.ToString();
    }
}
