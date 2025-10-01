using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapVisualizer : MonoBehaviour
{
    public NodeMap NodeMap { get; private set; }

    private void Awake()
    {
        List<INode> testNodes = new List<INode>();

        NodeMap = new NodeMap(testNodes);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
