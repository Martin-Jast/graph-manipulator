using System;
using System.Collections.Generic;


class Instance : Node {
    public Node MainInstanceNode{get; set;}
    private Graph GraphRef{get; set;}

    public Instance(){
        throw new Exception("Cannot instantiate without parent node.");
    }

    public Instance(string label, Node parent, Graph graph) {
        this.MainInstanceNode = parent;
        this.GraphRef = graph;
        this.Label = label;
        this.ID = System.Guid.NewGuid().ToString().Substring(0, 6);
        // A possible improvement: We could at instantiation create IS_A edges to all the relevant nodes ( parent, parent of parent, etc. )
        var link = this.LinkTo(parent, "INSTANCE_OF");
        graph.EdgesCollection.Add(link.ID, link);
        graph.InstancesCollection.Add(this.ID, this);
    }

    public List<Node>GetProperties() {
        var propListID = this.GetPropertiesIDs(new List<string>());

        return this.GraphRef.GetNodes(propListID);
    }

    public List<string>GetPropertiesIDs(List<string> nodesPassed) {
        // Stop condition
        if( nodesPassed.IndexOf(this.ID) != -1) return new List<string>();
        nodesPassed.Add(this.ID);
        // End Stop Condition

        List<string> propListIDs = new List<string>();
        var outEdges = this.GraphRef.GetEdges(this.MainInstanceNode.Outcoming);
        outEdges.AddRange(this.GraphRef.GetEdges(this.Outcoming));

         // Have all edges from main and parent
        List<string> parentNodeIDs = new List<string>();
        foreach( var edge in outEdges ){
            if(edge.Label == "IS_A"){
                parentNodeIDs.Add(edge.To);
            }else if(edge.Label == "HAS"){
                propListIDs.Add(edge.To);
            }
        }

        var parentNodes = this.GraphRef.GetNodes(parentNodeIDs);
        foreach( var parentNode in parentNodes ) {
            propListIDs.AddRange(parentNode.GetPropertiesIDs(new List<string>(), this.GraphRef));
        }

        return propListIDs;
    }
}