using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BucketNodeItem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image bg;                 // 노드 배경
    [SerializeField] private TextMeshProUGUI keyText;  // "K: ?"
    [SerializeField] private TextMeshProUGUI valueText;// "V: ?"
    void Awake()
    {
        // 안전장치: 높이 보장
        var le = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        if (le.preferredHeight <= 0) le.preferredHeight = 28f;
        le.flexibleWidth = 1f;
    }
    public void SetKV(string key, string value)
    {
        if (keyText) keyText.text = $"K: {key}";
        if (valueText) valueText.text = $"V: {value}";
        gameObject.SetActive(true);
    }

    // 프리팹에서 참조 안 달았을 때 자동으로 찾아줌(이름 기준)
    private void OnValidate()
    {
        if (!bg) bg = GetComponent<Image>();

        if (!keyText || !valueText)
        {
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                var n = t.name.ToLower();
                if (!keyText && (n.Contains("key") || n.Contains("k:"))) keyText = t;
                if (!valueText && (n.Contains("value") || n.Contains("v:"))) valueText = t;
            }
            // 아무 이름도 안 맞으면 첫/두 번째 TMP로 자동 지정
            if (!keyText && tmps.Length > 0) keyText = tmps[0];
            if (!valueText && tmps.Length > 1) valueText = tmps[1];
        }
    }
}
