using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BucketCell : MonoBehaviour
{
    [SerializeField] private Image bg;
    [SerializeField] private TextMeshProUGUI label;

    // 충돌 개수에 따라 색상 변화
    public void SetContent(string text, int itemCount)
    {
        label.text = text;

        if (itemCount == 0) bg.color = new Color(0.25f, 0.25f, 0.25f);   // empty: 회색
        else if (itemCount == 1) bg.color = new Color(0.30f, 0.55f, 0.30f);   // 단일: 초록
        else if (itemCount <= 3) bg.color = new Color(0.70f, 0.65f, 0.30f);   // 충돌 낮음: 노랑
        else bg.color = new Color(0.70f, 0.30f, 0.30f);   // 충돌 높음: 빨강
    }
}
