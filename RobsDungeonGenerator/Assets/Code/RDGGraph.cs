using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DelaunayTriangulator;

/// <summary>
/// @author Rob Giusti
/// RDGGraph
/// </summary>
public class RDGGraph {

	/// <summary>
	/// The adjacency map
	/// </summary>
	public Dictionary<RDGRoom, List<RDGRoom>> graph = new Dictionary<RDGRoom, List<RDGRoom>>();

	public int connections
	{
		get
		{
			int num = 0;
			foreach (var item in graph) 
			{
				num += item.Value.Count;
			}
			return num/2; //Divide by two because each connection gets counted twice (A->B and B->A)
		}
	}

	/// <summary>
	/// Creates a new, empty graph.
	/// </summary>
	public RDGGraph(){}

	/// <summary>
	/// Creates a graph using a list of preexisting rooms and a list of triads
	/// which are used to build the connections.
	/// Used to create the graph that represents the Delaunay Triangulation of the
	/// rooms
	/// </summary>
	/// <param name="rooms">Rooms.</param>
	/// <param name="connections">Connections.</param>
	public RDGGraph(List<RDGRoom> rooms, List<Triad> connections)
	{
		foreach (var item in rooms) 
		{
			graph.Add(item, new List<RDGRoom>());
		}

		foreach (var item in connections) 
		{
			AddConnection(rooms[item.a], rooms[item.b]);
			AddConnection(rooms[item.b], rooms[item.c]);
			AddConnection(rooms[item.c], rooms[item.a]);
		}
	}

	/// <summary>
	/// Creates a minimum spanning tree using the connections that this graph already has.
	/// </summary>
	/// <returns>The minimum spanning tree.</returns>
	public IEnumerator GenerateMinSpanTree(RDGGraph mst)
	{

		List<RDGRoom> undiscovered = new List<RDGRoom>();
		foreach (var item in graph.Keys) 
		{
			undiscovered.Add(item);
		}

		RDGRoom currFrom = undiscovered[0];
		undiscovered.Remove(currFrom);
		mst.AddRoom(currFrom);

		RDGRoom currTo;
		int currMinDist;

		while (undiscovered.Count > 0)
		{
			//Reset vars
			currTo = currFrom = null;
			currMinDist = int.MaxValue;

			//Find the shortest connection to a new room

			//Check through every room we've discovered
			foreach (var discoveredRoom in mst.graph.Keys) 
			{
				//Get its adjency list from the creator object
				foreach (var room in graph[discoveredRoom]) 
				{
					//Check if undiscovered
					if (undiscovered.Contains(room))
					{
						int distance = RDGMath.DistBetweenRooms(discoveredRoom, room);
						if (currTo == null || distance < currMinDist)
						{
							currFrom = discoveredRoom;
							currTo = room;
							currMinDist = distance;
						}
					}
				}
			}

			//Remove that room from undiscovered and add it to the graph
			undiscovered.Remove(currTo);
			mst.AddRoom(currTo);

			//Connect the room to the graph
			mst.AddConnection(currFrom, currTo);

			yield return null;
		}

	}

	/// <summary>
	/// Adds a room to this graph
	/// </summary>
	/// <param name="room">Room.</param>
	void AddRoom(RDGRoom room)
	{
		graph.Add(room, new List<RDGRoom>());
	}

	/// <summary>
	/// Adds a direct connection between two rooms
	/// </summary>
	/// <param name="roomA">Room a.</param>
	/// <param name="roomB">Room b.</param>
	public void AddConnection(RDGRoom roomA, RDGRoom roomB)
	{
		if (!graph[roomA].Contains(roomB))
		{
			graph[roomA].Add(roomB);
		}
		if (!graph[roomB].Contains(roomA))
		{
			graph[roomB].Add(roomA);
		}
	}

	public bool ContainsConnection(RDGRoom roomA, RDGRoom roomB)
	{
		return graph[roomA].Contains(roomB);
	}

	public void GetRandomConnection(ref RDGRoom roomA, ref RDGRoom roomB)
	{
		roomA = graph.ElementAt(UnityEngine.Random.Range(0, graph.Count)).Key;
		roomB = graph[roomA].ElementAt(UnityEngine.Random.Range(0, graph[roomA].Count));
	}
}
