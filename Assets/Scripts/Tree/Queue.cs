using NUnit.Framework;
using UnityEngine;

public class Queue : MonoBehaviour
{
    PriorityQueue<string, int> queue;

    private void Start()
    {
        queue = new PriorityQueue<string, int>();
        
    }
}
