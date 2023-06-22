using System;
using System.Collections.Generic;

public class Edge {
    public string ID {get; set;}
    public string From{ get; set;}
    public string To{ get; set;}
    public string Label {get; set;}

    public List<string> Flags {get; set;}
    public string AsProperty{ get; set;}

    public Edge(Node from, Node to, string link) {
        this.ID = System.Guid.NewGuid().ToString().Substring(0, 6);
        this.From = from.ID;
        from.AddOutcomingLink(this);

        this.To = to.ID;
        to.AddIncomingLink(this);

        this.Label = link;
    }

    public Edge(Edge e) {
        this.ID = e.ID;
        this.From = e.From;
        this.To = e.To;
        this.Label = e.Label;
        this.Flags = e.Flags;
    }

    public Edge() {}

}

public class Node {
    public string ID {get; set;}
    public List<string> Incoming{ get; set;}
    public List<string> Outcoming{ get; set;}

    public string Label{ get; set;}
    public string AsProperty{ get; set;}

    public Dictionary<string, Object> Data{ get; set;}

    public Node(string id, string label) {
        this.ID = id;
        this.Label = label;
        this.Incoming = new List<string>();
        this.Outcoming = new List<string>();
        this.Data = new Dictionary<string, object>();
    }

    public Node(string label) {
        this.ID = System.Guid.NewGuid().ToString().Substring(0, 6);
        this.Label = label;
        this.Incoming = new List<string>();
        this.Outcoming = new List<string>();
        this.Data = new Dictionary<string, object>();
    }

    public Node() {
        this.ID = System.Guid.NewGuid().ToString().Substring(0, 6);
        this.Incoming = new List<string>();
        this.Outcoming = new List<string>();
        this.Data = new Dictionary<string, object>();
    }

    public Edge LinkTo(Node n, string type) {
        Edge link = new Edge(this, n, type);
        link.AsProperty = this.AsProperty;

        return link;
    }
    
    public Edge LinkTo(Node n, string type, List<string> flags) {
        Edge link = new Edge(this, n, type);
        link.Flags = flags;
        link.AsProperty = this.AsProperty;

        return link;
    }

    public void AddIncomingLink(Edge e) {
        if(this.Incoming == null) {
            this.Incoming = new List<string>();
        }
       this.Incoming.Add(e.ID);
    }

    public void AddOutcomingLink(Edge e) {
        if(this.Outcoming == null) {
            this.Outcoming = new List<string>();
        }
        this.Outcoming.Add(e.ID);
    }

    public List<Node>GetProperties(Graph graph) {
        var propListID = this.GetPropertiesIDs(new List<string>(), graph);

        return graph.GetNodes(propListID);
    }
    
    public List<string>GetPropertiesIDs(List<string> nodesPassed, Graph graph) {
        // Stop condition
        if( nodesPassed.IndexOf(this.ID) != -1) return new List<string>();
        nodesPassed.Add(this.ID);
        // End Stop Condition

        List<string> propListIDs = new List<string>();
        var outEdges = graph.GetEdges(this.Outcoming);

         // Have all edges from main and parent.
        List<string> parentNodeIDs = new List<string>();
        foreach( var edge in outEdges ){
            if( edge.Label == "IS_A") {
                parentNodeIDs.Add(edge.To);
            }else if(edge.Label == "HAS"){
                propListIDs.Add(edge.To);
            }
        }

        var parentNodes = graph.GetNodes(parentNodeIDs);
        foreach( var parentNode in parentNodes ) {
            // Add check to avoid adding repeated props
            propListIDs.AddRange(parentNode.GetPropertiesIDs(nodesPassed, graph));
        }

        return propListIDs;

    }

}


