using UnityEngine;

public class TreeNode<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public int Height { get; set; }

    public TreeNode<TKey, TValue> Left { get; set; }
    public TreeNode<TKey, TValue> Right { get; set; }

    public TreeNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Height = 1;
    }
}
