using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashChainingUI : MonoBehaviour
{
    [Header("Right Panel UI")]
    [SerializeField] private TMP_InputField keyInput;     // Enter Key...
    [SerializeField] private TMP_InputField valueInput;   // Enter Value...
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private TextMeshProUGUI logText;     // 로그 출력

    [Header("Left Panel UI")]
    [SerializeField] private RectTransform bucketListRoot;   // VerticalLayoutGroup 있는 오브젝트
    [SerializeField] private GameObject bucketRowPrefab;     // BucketRow 프리팹(루트에 BucketRowView)
    [SerializeField] private BucketNodeItem nodeItemPrefab;  // 초록 노드 프리팹(BucketNodeItem 붙어있어야 함)

    private ChainingHashTable<int, int> table;
    private BucketRowView[] rows;

    private void Start()
    {
        table = new ChainingHashTable<int, int>(); // 기본 16 버킷
        BuildRows();
        WireButtons();
        Refresh();
        Log("READY");
    }

    private void WireButtons()
    {
        if (addButton) addButton.onClick.AddListener(OnAdd);
        if (removeButton) removeButton.onClick.AddListener(OnRemove);
        if (clearButton) clearButton.onClick.AddListener(OnClear);
    }

    // 버킷 수만큼 행(버킷 로우) 생성
    private void BuildRows()
    {
        // 기존 정리
        for (int i = bucketListRoot.childCount - 1; i >= 0; --i)
            Destroy(bucketListRoot.GetChild(i).gameObject);

        int n = table.DebugSize;
        rows = new BucketRowView[n];

        for (int i = 0; i < n; i++)
        {
            var go = Instantiate(bucketRowPrefab, bucketListRoot);
            go.name = $"BucketRow_{i:00}";
            var row = go.GetComponent<BucketRowView>();
            row.SetIndex(i);
            rows[i] = row;
        }
    }

    // 테이블 스냅샷을 화면에 렌더
    private void Refresh()
    {
        if (rows == null || rows.Length != table.DebugSize)
            BuildRows();

        var snapshot = table.DebugSnapshot().ToList(); // 각 버킷의 연결리스트

        for (int i = 0; i < snapshot.Count; i++)
        {
            // 한 버킷 줄에 체인 통째로 그리기 (세로 배치)
            rows[i].RenderChain(snapshot[i], nodeItemPrefab);
        }
    }

    // ========= 버튼 핸들러 =========
    private void OnAdd()
    {
        if (!TryParseKV(out int k, out int v)) return;

        try
        {
            table.Add(k, v);
            Log($"ADD: {k} -> I:{table.DebugBucketOf(k)}");
        }
        catch (Exception e)
        {
            Log($"ADD FAIL({k}): {e.Message}");
        }
        Refresh();
    }

    private void OnRemove()
    {
        if (!int.TryParse(keyInput.text, out int k))
        {
            Log("REMOVE: key empty/invalid");
            return;
        }

        bool ok = table.Remove(k);
        Log(ok ? $"REMOVE: {k} ✓" : $"REMOVE: {k} (not found)");
        Refresh();
    }

    private void OnClear()
    {
        table.Clear();
        Log("CLEARED");
        Refresh();
    }

    // ========= 유틸 =========
    private bool TryParseKV(out int key, out int val)
    {
        if (!int.TryParse(keyInput.text, out key))
        {
            Log("Key must be int");
            val = default;
            return false;
        }
        if (!int.TryParse(valueInput.text, out val))
        {
            Log("Value must be int");
            return false;
        }
        return true;
    }

    private void Log(string msg)
    {
        if (!logText) return;
        var sb = new StringBuilder();
        sb.AppendLine(msg);
        sb.Append(logText.text);
        logText.text = sb.ToString();
    }

#if UNITY_EDITOR
    // 프리팹 끼워넣기 잊었을 때 에디터에서 조금 도와줌
    private void OnValidate()
    {
        if (!bucketListRoot)
        {
            var t = transform.Find("Left/BucketListRoot") as RectTransform;
            if (t) bucketListRoot = t;
        }
    }
#endif
}
