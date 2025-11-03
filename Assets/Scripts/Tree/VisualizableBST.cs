using System;

public class VisualizableBST<TKey, TValue> : AVLTree<TKey, TValue> where TKey : IComparable<TKey>
{
    public TreeNode<TKey, TValue> GetRoot()
    {
        return root;
    }
}