using UnityEngine;
using System;
using System.Collections.Generic;

public enum SpacingType { Pow, LevelOrder, InOrder }

public class BinaryTreeVisualizer : MonoBehaviour
{
    public GameObject nodePrefab;

    public SpacingType spacingType = SpacingType.Pow;
    public float verticalSpacing = 2.0f;
    public float horizontalSpacing = 2.0f;

    private Dictionary<object, Vector3> nodePositions = new Dictionary<object, Vector3>();

    private NodeVisualizer CreateNode<TKey, TValue>(TreeNode<TKey, TValue> node, Vector3 position)
    {
        GameObject nodeObj = Instantiate(nodePrefab, position, Quaternion.identity, transform);
        NodeVisualizer visualizer = nodeObj.GetComponent<NodeVisualizer>();

        visualizer.SetNode(node);

        return visualizer;
    }

    private void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void VisualizeTree<TKey, TValue>(VisualizableBST<TKey, TValue> tree)
        where TKey : IComparable<TKey>
    {
        Clear();

        var root = tree.GetRoot();
        if (root == null)
            return;

        nodePositions.Clear();

        switch (spacingType)
        {
            case SpacingType.Pow:
                VisualizeNode(root, Vector3.zero, 0);//root.Height - 1);
                break;
            case SpacingType.LevelOrder:
                nodePositions.Clear();
                AssignPositionsLevelOrder(root);
                CreateNode(root);
                break;
            case SpacingType.InOrder:
                nodePositions.Clear();
                int currentXIndex = 0;
                AssignPositionsInOrder(root, 0, ref currentXIndex);
                CreateNode(root);
                break;
        }
    }

    private NodeVisualizer VisualizeNode<TKey, TValue>(TreeNode<TKey, TValue> node, Vector3 position, int height) where TKey : IComparable<TKey>
    {
        if (node == null)
        {
            return null;
        }

        NodeVisualizer nodeVisualizer = CreateNode(node, position);

        Vector3 childBasePosition = position + new Vector3(0f, -verticalSpacing, 0);
        
        Vector3 leftPosition = childBasePosition;
        Vector3 rightPosition = childBasePosition;
        
        float horizontalOffset = horizontalSpacing * 0.5f * Mathf.Pow(2, height - 1);
        leftPosition.x -= horizontalOffset;
        rightPosition.x += horizontalOffset;

        NodeVisualizer leftChildVisualizer = VisualizeNode(node.Left, leftPosition, height - 1);
        if (leftChildVisualizer != null)
        {
            nodeVisualizer.SetLeftEdge(leftChildVisualizer.transform.position);
        }

        NodeVisualizer rightChildVisualizer = VisualizeNode(node.Right, rightPosition, height - 1);
        if (rightChildVisualizer != null)
        {
            nodeVisualizer.SetRightEdge(rightChildVisualizer.transform.position);
        }

        return nodeVisualizer;
    }

    private void AssignPositionsInOrder<TKey, TValue>(TreeNode<TKey, TValue> node, int depth, ref int currentXIndexRef)
    {        
        if (node == null) 
            return;

        AssignPositionsInOrder(node.Left, depth + 1, ref currentXIndexRef);

        float x = currentXIndexRef * horizontalSpacing;
        float y = -depth * verticalSpacing;
        nodePositions[node] = new Vector3(x, y, 0f);

        currentXIndexRef++;

        AssignPositionsInOrder(node.Right, depth + 1, ref currentXIndexRef);
    }


    private void AssignPositionsLevelOrder<TKey, TValue>(TreeNode<TKey, TValue> root)
    {
        if (root == null) 
            return;

        var queue = new Queue<(TreeNode<TKey, TValue> node, int level)>();
        queue.Enqueue((root, 0));

        List<List<TreeNode<TKey, TValue>>> levels = new List<List<TreeNode<TKey, TValue>>>();

        while (queue.Count > 0)
        {
            var (currentNode, level) = queue.Dequeue();

            if (levels.Count <= level)
            {
                levels.Add(new List<TreeNode<TKey, TValue>>());
            }
            levels[level].Add(currentNode);

            if (currentNode.Left != null)
                queue.Enqueue((currentNode.Left, level + 1));
            if (currentNode.Right != null)
                queue.Enqueue((currentNode.Right, level + 1));
        }

        for (int levelIndex = 0; levelIndex < levels.Count; levelIndex++)
        {
            var levelNodes = levels[levelIndex];
            int count = levelNodes.Count;
            float y = -levelIndex * verticalSpacing;

            for (int i = 0; i < count; i++)
            {
                float x = i * horizontalSpacing;
                TreeNode<TKey, TValue> node = levelNodes[i];
                nodePositions[node] = new Vector3(x, y, 0f);
            }
        }
    }


    private NodeVisualizer CreateNode<TKey, TValue>(TreeNode<TKey, TValue> node)
    {
        if (node == null)
            return null;

        Vector3 pos = nodePositions[node];

        NodeVisualizer vNode = CreateNode(node, pos);

        NodeVisualizer vNodeLeft = CreateNode(node.Left);
        if (vNodeLeft != null)
        {
            vNode.SetLeftEdge(vNodeLeft.transform.position);
        }

        NodeVisualizer vNodeRight = CreateNode(node.Right);
        if (vNodeRight != null)
        {
            vNode.SetRightEdge(vNodeRight.transform.position);
        }

        return vNode;
    }
}
