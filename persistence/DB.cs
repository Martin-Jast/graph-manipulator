using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

interface DataWorker {
    public static void SaveDB(Graph g){}
    public static Graph LoadDB(){return null;}
}

public class Graph {
    public Dictionary<string, Edge> EdgesCollection{get; set;}
    public Dictionary<string, Node> NodesCollection{get; set;}
    public Dictionary<string, Node> InstancesCollection{get; set;}

    public Graph() {
        this.EdgesCollection = new Dictionary<string, Edge>();
        this.NodesCollection = new Dictionary<string, Node>();
        this.InstancesCollection = new Dictionary<string, Node>();
    }

    public int GetNumNodes() {
        return this.NodesCollection.Count;
    }
    public int GetNumEdges() {
        return this.EdgesCollection.Count;
    }
    public int GetNumInstances() {
        return this.InstancesCollection.Count;
    }

    public List<Edge>GetEdges(List<string> e) {
        List<Edge> list = new List<Edge>();

        foreach(var edgeID in e){
            list.Add(this.EdgesCollection.GetValueOrDefault(edgeID));
        }
        return list;
    }

    public List<Node>GetNodes(List<string> n) {
        List<Node> list = new List<Node>();

        foreach(var nodeID in n){
            list.Add(this.NodesCollection.GetValueOrDefault(nodeID));
        }
        return list;
    }

    public List<Node>GetInstances(List<string> n) {
        List<Node> list = new List<Node>();

        foreach(var nodeID in n){
            list.Add(this.InstancesCollection.GetValueOrDefault(nodeID));
        }
        return list;
    }
    
}

class JsonWorker : DataWorker {
    public static void SaveJson(Object data, string fileName) {
        var options = new JsonSerializerOptions(){
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(data, options);
        System.IO.File.WriteAllText(fileName, json);
    }

     public static Object LoadJson<T>(string fileName) {
        using (StreamReader r = new StreamReader(fileName))
        {
            string json = r.ReadToEnd();
            var data = JsonSerializer.Deserialize<List<T>>(json);
            return data;
        }
    }

    public static void SaveDB(Graph g) {
        JsonWorker.SaveJson(new List<Node>(g.NodesCollection.Values), "nodes.json");
        JsonWorker.SaveJson(new List<Node>(g.InstancesCollection.Values), "instances.json");
        JsonWorker.SaveJson(new List<Edge>(g.EdgesCollection.Values), "edges.json");
    }

    public static Graph LoadDB() {
        List<Node> nodes = (List<Node>)JsonWorker.LoadJson<Node>("nodes.json");
        List<Node> instances = (List<Node>)JsonWorker.LoadJson<Node>("instances.json");   
        List<Edge> edges = (List<Edge>)JsonWorker.LoadJson<Edge>("edges.json");
        Graph graph = new Graph();

        foreach( var item in nodes) {
            graph.NodesCollection.Add(item.ID, item);
        }
        foreach( var item in edges) {
            graph.EdgesCollection.Add(item.ID, item);
        }
        foreach( var item in instances) {
            graph.InstancesCollection.Add(item.ID, item);
        }
        return graph;
    }
}