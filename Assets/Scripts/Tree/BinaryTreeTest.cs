using UnityEngine;

public class BinaryTreeTest : MonoBehaviour
{
    public BinaryTreeVisualizer treeVisualizer;

    [SerializeField] private int nodeCount = 10;
    [SerializeField] private int minKey = 1;
    [SerializeField] private int maxKey = 1000;

    private void Start()    
    {
        GenerateRandomTree();
    }

    public void GenerateRandomTree()
    {
        var tree = new VisualizableBST<int, string>();

        int addedNodes = 0;
        while (addedNodes < nodeCount)
        {
            int key = Random.Range(minKey, maxKey + 1);

            if (!tree.ContainsKey(key))
            {
                string value = $"V-{key}";
                tree.Add(key, value);
                addedNodes++;
            }
        }

        treeVisualizer.VisualizeTree(tree);
    }

    [ContextMenu("Generate New Random Tree")]
    public void RegenerateTree()
    {
        GenerateRandomTree();
    }
}