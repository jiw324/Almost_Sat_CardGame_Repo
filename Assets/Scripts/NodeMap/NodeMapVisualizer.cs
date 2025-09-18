using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class NodeMapVisualizer : MonoBehaviour
{
    public NodeMap NodeMap { get; private set; }

    private void Awake()
    {
        List<INode> testNodes = new List<INode>();
        testNodes.Add(new LootNode(0, 0));
        testNodes.Add(new CombatNode(0, 0));
        testNodes.Add(new ShopNode(0, 0));
        testNodes.Add(new RestNode(0, 0));
        testNodes.Add(new EventNode(0, 0));

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
