using UnityEngine;
using TMPro;

public class NodeVisualizer : MonoBehaviour
{
    public SpriteRenderer nodeRenderer;
    public TextMeshPro nodeText;
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;

    public void SetNode<TKey, TValue>(TreeNode<TKey, TValue> node)
    {
        nodeText.text = $"K: {node.Key}\nV: {node.Value}\n H: {node.Height}";
    }

    public void SetLeftEdge(Vector3 target)
    {
        leftLineRenderer.positionCount = 2;
        leftLineRenderer.SetPosition(0, transform.position);
        leftLineRenderer.SetPosition(1, target);
    }

    public void SetRightEdge(Vector3 target)
    {
        rightLineRenderer.positionCount = 2;
        rightLineRenderer.SetPosition(0, transform.position);
        rightLineRenderer.SetPosition(1, target);
    }
}