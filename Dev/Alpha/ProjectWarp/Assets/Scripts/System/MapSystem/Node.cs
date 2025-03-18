using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public enum NodeType
    {
        Elite,
        Normal,
        OutPost,
        Event,
        Boss
    }

    public Vector2Int point;
    public List<Vector2Int> incoming = new List<Vector2Int>();
    public List<Vector2Int> outgoing = new List<Vector2Int>();
    public NodeType type;

    public Vector2 position;
    public Node(NodeType nodeType, Vector2Int point)
    {
        type = nodeType;    
        this.point = point;
    }

    public void AddIncoming(Vector2Int NodePoint)
    {
        if(incoming.Any(element => element.Equals(NodePoint)))
        {
            return;
        }
        incoming.Add(NodePoint);
    }

    public void AddOutgoing(Vector2Int NodePoint)
    {
        if (outgoing.Any(element => element.Equals(NodePoint)))
        {
            return;
        }
        outgoing.Add(NodePoint);
    }

    public void RemoveIncoming(Vector2Int NodePoint)
    {
        incoming.RemoveAll(element => element.Equals(NodePoint));
    }
    public void RemoveOutgoing(Vector2Int NodePoint)
    {
        outgoing.RemoveAll(element => element.Equals(NodePoint));
    }

    public bool HasNoConnection()
    {
        return incoming.Count == 0 && outgoing.Count == 0;
    }
}
