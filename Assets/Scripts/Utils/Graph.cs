using System;
using System.Collections.Generic;

public class Graph
{
    private int Vertices;
    private List<Tuple<int, int>>[] AdjacencyList;

    public Graph(int vertices)
    {
        Vertices = vertices;
        AdjacencyList = new List<Tuple<int, int>>[Vertices];

        for (int i = 0; i < Vertices; i++)
            AdjacencyList[i] = new List<Tuple<int, int>>();
    }

    public void AddEdge(int u, int v, int weight)
    {
        AdjacencyList[u].Add(new Tuple<int, int>(v, weight));
    }

    public List<Tuple<int, int>>[] GetAdjacencyList()
    {
        return AdjacencyList;
    }
}

public static class DijkstraAlgorithm
{
    public static int[] Dijkstra(Graph graph, int source)
    {
        int vertices = graph.GetAdjacencyList().Length;
        int[] distances = new int[vertices];
        bool[] shortestPathTreeSet = new bool[vertices];

        for (int i = 0; i < vertices; i++)
        {
            distances[i] = int.MaxValue;
            shortestPathTreeSet[i] = false;
        }

        distances[source] = 0;

        for (int count = 0; count < vertices - 1; count++)
        {
            int u = MinimumDistance(distances, shortestPathTreeSet);
            shortestPathTreeSet[u] = true;

            foreach (var neighbor in graph.GetAdjacencyList()[u])
            {
                int v = neighbor.Item1;
                int weight = neighbor.Item2;

                if (!shortestPathTreeSet[v] && distances[u] != int.MaxValue && distances[u] + weight < distances[v])
                    distances[v] = distances[u] + weight;
            }
        }

        return distances;
    }

    private static int MinimumDistance(int[] distances, bool[] shortestPathTreeSet)
    {
        int min = int.MaxValue, minIndex = -1;

        for (int v = 0; v < distances.Length; v++)
        {
            if (!shortestPathTreeSet[v] && distances[v] <= min)
            {
                min = distances[v];
                minIndex = v;
            }
        }

        return minIndex;
    }


}
