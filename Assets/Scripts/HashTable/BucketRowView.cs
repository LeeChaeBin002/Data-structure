using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BucketRowView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI indexLabel;   // "I: 5" 같은 라벨
    [SerializeField] private RectTransform contentRoot;    // 노드들이 붙을 부모(가로 레이아웃)
    
    
    [Header("Layout Options")]
    [SerializeField, Range(0, 40)] private float spacing = 8f;
    [SerializeField] private int paddingLeft = 0;
    [SerializeField] private int paddingRight = 0;
    [SerializeField] private int paddingTop = 0;
    [SerializeField] private int paddingBottom = 0;
    /// contentRoot를 외부에서 필요하면 읽기 전용으로 노출
    public RectTransform ContentRoot => contentRoot;

    // ====== 기본 API ======

    private void Awake()
    {
        ResolveRefs();
        EnsureVerticalLayout(contentRoot);  
    }

    private void ResolveRefs()
    {
        if (!indexLabel)
        {
            var t = transform.Find("Index");
            if (t) indexLabel = t.GetComponent<TextMeshProUGUI>();
        }
        if (!contentRoot)
        {
            var t = transform.Find("Content");
            if (t) contentRoot = t as RectTransform;
        }
    }
    /// 버킷 인덱스 표시
    public void SetIndex(int i)
    {
        if (indexLabel != null)
            indexLabel.text = $"I: {i}";
    }

    /// Content 밑의 모든 노드 제거
    public void ClearNodes()
    {
        if (contentRoot == null) return;
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }

    /// 노드 하나 추가(프리팹 인스턴스 생성) 후 KV 세팅
    public BucketNodeItem AddNode(BucketNodeItem nodePrefab, string key, string value)
    {
        if (contentRoot == null || nodePrefab == null) return null;

        var go = Instantiate(nodePrefab, contentRoot);
        var item = go.GetComponent<BucketNodeItem>();
        if (item != null)
            item.SetKV(key, value);
        return item;
    }

    /// 체이닝 리스트를 한 번에 렌더(편의 함수)
    public void RenderChain<TK, TV>(IReadOnlyList<KeyValuePair<TK, TV>> chain, BucketNodeItem nodePrefab)
    {
        ClearNodes();
        if (chain == null) return;

        EnsureVerticalLayout(contentRoot); // 안전장치(없으면 자동 추가)
        
        foreach (var kv in chain)
            AddNode(nodePrefab, kv.Key?.ToString(), kv.Value?.ToString());
    }

    // ====== 에디터/안전 보조 ======

    private void OnValidate()
    {
        ResolveRefs();


        if (contentRoot)
        {
            var any = contentRoot.GetComponent<LayoutGroup>();
            if (any && !(any is VerticalLayoutGroup))
            {
                Debug.LogWarning($"'{contentRoot.name}'에 {any.GetType().Name}이 있어 VerticalLayoutGroup 대신 사용 중입니다.", contentRoot);
            }
        }


    }

    private  void EnsureVerticalLayout(RectTransform rt)
    {
        if (!rt) return;

        // 이미 다른 LayoutGroup이 있으면 경고
        var any = rt.GetComponent<UnityEngine.UI.LayoutGroup>();
        if (any && !(any is UnityEngine.UI.VerticalLayoutGroup))
        {
            Debug.LogWarning($"'{rt.name}'에 이미 {any.GetType().Name}이 있어 VerticalLayoutGroup 추가 안 함.", rt);
            return;
        }

        // 없으면 VerticalLayoutGroup 추가
        var v = rt.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (!v) v = rt.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();

        // 정렬 및 옵션 세팅
        v.childAlignment = TextAnchor.UpperLeft;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = false;
        v.childForceExpandHeight = false;
        v.spacing = 8f;   // 항목 간 간격
        v.spacing = spacing;
        v.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
        // ContentSizeFitter 정리 (레이아웃 충돌 방지)
        var fitter = rt.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fitter)
        {
            if (Application.isPlaying) Destroy(fitter);
            else DestroyImmediate(fitter);
        }
    }

}
