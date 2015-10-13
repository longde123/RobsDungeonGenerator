using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DelaunayTriangulator;

/// <summary>
/// @author Rob Giusti
/// RDGGenerator
/// </summary>
public class RDGGenerator : MonoBehaviour {

	[SerializeField]
	int numRooms;

	[SerializeField]
	int maxDungeonDimension;

	[SerializeField]
	int minRoomSize;

	[SerializeField]
	int maxRoomSize;

	[SerializeField]
	float minPercentExtraLinks;

	[SerializeField]
	float maxPercentExtraLinks;

	[SerializeField]
	float squareSize;

	float percentExtras
	{
		get
		{
			return UnityEngine.Random.Range(minPercentExtraLinks, maxPercentExtraLinks);
		}
	}

	[SerializeField]
	Transform wallPrefab;

	[SerializeField]
	RDGGround floorPrefab;

	[SerializeField]
	Transform geometryParent;

	public List<RDGRoom> rooms = new List<RDGRoom>();

	public List<Triad> tris = new List<Triad>();

	RDGGraph graph = null;

	RDGGraph minGraph = null;

	RDGGrid theGrid = null;

	public bool acting = false;

	int randomDim
	{
		get
		{
			return UnityEngine.Random.Range(-maxDungeonDimension, maxDungeonDimension-maxRoomSize);
		}
	}

	int randomSize
	{
		get
		{
			return UnityEngine.Random.Range(minRoomSize, maxRoomSize);
		}
	}

	// Use this for initialization
	void Start ()
	{
		StartCoroutine(Generate());
	}

	IEnumerator Generate()
	{
		graph = null;
		minGraph = null;
		theGrid = null;

		acting = true;
		rooms.Clear ();
		RDGRoom.ResetIDs();

		for (int i = 0; i < numRooms; i++)
		{
			rooms.Add(MakeRoom());
			yield return null;
		}

		while(PushRoomsOut())
		{
			yield return null;
		}

		tris = Triangulate();

		graph = new RDGGraph(rooms, tris);

		minGraph = new RDGGraph();
		yield return StartCoroutine(graph.GenerateMinSpanTree(minGraph));

		yield return StartCoroutine(AddExtraConnections());

		BuildGrid();

		yield return StartCoroutine(BuildCorridors());

		foreach (var item in rooms) 
		{
			MakeRoomFloor(item);
			yield return null;
			yield return StartCoroutine(GenerateWalls(item));
		}

		acting = false;
	}

	RDGRoom MakeRoom()
	{
		return new RDGRoom(randomDim, randomDim, randomSize, randomSize);
	}

	bool PushRoomsOut()
	{
		bool pushed = false;

		for (int i = 0; i < rooms.Count; i++)
		{
			for (int j = i+1; j < rooms.Count; j++)
			{
				if (rooms[i].Overlaps(rooms[j]))
				{
					if (rooms[i].pushed && rooms[j].pushed)
					{
						rooms[i].pushed = false;
						rooms[j].pushed = false;
					}
					else if (rooms[j].pushed)
					{
						rooms[j].Push(rooms[i]);
					}
					else if (rooms[i].pushed)
					{
						rooms[i].Push(rooms[j]);
					}
					else if (rooms[i].center.sqrMagnitude < rooms[j].center.sqrMagnitude)
					{
						rooms[i].Push(rooms[j]);
					}
					else
					{
						rooms[j].Push(rooms[i]);
					}
					pushed = true;
				}
			}
		}

		return pushed;
	}

	List<Triad> Triangulate()
	{
		List<Vertex> verts = new List<Vertex>();

		foreach (var item in rooms)
		{
			verts.Add(new Vertex(item.center.x, item.center.y));
		}

		return (new Triangulator()).Triangulation(verts);
	}

	IEnumerator AddExtraConnections()
	{
		int num = Mathf.CeilToInt((graph.connections - minGraph.connections) * percentExtras);

		RDGRoom roomA = null, roomB = null;
		for (int i = 0; i < num; i++)
		{
			do
			{
				graph.GetRandomConnection(ref roomA, ref roomB);
			}while(minGraph.ContainsConnection(roomA, roomB));

			minGraph.AddConnection(roomA, roomB);

			yield return null;
		}

	}

	void BuildGrid()
	{
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int maxX = int.MinValue;
		int maxY = int.MinValue;

		foreach (var item in rooms)
		{
			minX = Mathf.Min(minX, item.x);
			maxX = Mathf.Max(maxX, item.x+item.width);
			minY = Mathf.Min(minY, item.y);
			maxY = Mathf.Max(maxY, item.y+item.height);
		}

		theGrid = new RDGGrid(minX, minY, maxX, maxY);

		foreach (var item in rooms)
		{
			theGrid.AddRoom(item);
		}
	}

	IEnumerator BuildCorridors ()
	{
		foreach (var roomFrom in minGraph.graph.Keys) {
			foreach (var roomTo in minGraph.graph [roomFrom]) {
				if (roomFrom.id < roomTo.id) continue;
				yield return StartCoroutine(theGrid.Pathfind (roomFrom, roomTo));
				Stack<RDGGridNode> path = theGrid.lastPath;
				RDGGridNode initialNode = null, finalNode = null, currNode;
				while (path != null && path.Count > 0) {
					currNode = path.Pop ();
					if (theGrid [currNode] != null) {
						if (initialNode != null) {
							AddRoom (initialNode, finalNode);
							initialNode = null;
							finalNode = null;
						}
					}
					else {
						if (initialNode == null) {
							initialNode = currNode;
						}
						else if (currNode.dir != initialNode.dir) {
							AddRoom (initialNode, finalNode);
							initialNode = currNode;
						}
						finalNode = currNode;
					}
				}
				yield return null;
			}
		}
	}

	void AddRoom (RDGGridNode initialNode, RDGGridNode finalNode)
	{
		RDGRoom room = new RDGRoom (Mathf.Min (initialNode.x, finalNode.x), Mathf.Min (initialNode.y, finalNode.y), Mathf.Abs (initialNode.x - finalNode.x) + 1, Mathf.Abs (initialNode.y - finalNode.y) + 1);
		room.type = RoomType.CORRIDOR;
		theGrid.AddRoom(room);
		rooms.Add(room);
	}

	IEnumerator GenerateWalls (RDGRoom room)
	{
		for (int i = room.x-1; i <= room.x+room.width; i++)
		{
			if (theGrid[i,room.y-1] == null)
			{
				MakeWallAt(i, room.y-1);
				theGrid[i,room.y-1] = room;
				yield return null;
			}
			if (theGrid[i,room.y+room.height] == null)
			{
				MakeWallAt(i, room.y+room.height);
				theGrid[i,room.y+room.height] = room;
				yield return null;
			}
		}
		for (int i = room.y-1; i <= room.y+room.height; i++)
		{
			if (theGrid[room.x-1,i] == null)
			{
				MakeWallAt(room.x-1, i);
				theGrid[room.x-1,i] = room;
				yield return null;
			}
			if (theGrid[room.x+room.width,i] == null)
			{
				MakeWallAt(room.x+room.width, i);
				theGrid[room.x+room.width,i] = room;
				yield return null;
			}
		}

	}

	void MakeWallAt(int x, int y)
	{
		Transform wall = (Instantiate(wallPrefab, new Vector3(x + .5f,y + .5f) * squareSize, Quaternion.identity) as Transform);
		//wall.parent = geometryParent;
	}

	void MakeRoomFloor(RDGRoom room)
	{
		RDGGround ground = Instantiate(floorPrefab, room.centerV3 * squareSize, Quaternion.identity) as RDGGround;
        ground.Init(new Vector2(room.width, room.height));
		ground.transform.parent = geometryParent;
	}

	void Update()
	{
		if (!acting && Input.GetKeyDown(KeyCode.Space))
		{
			StartCoroutine(Generate());
		}
	}

	void OnDrawGizmos()
	{
		if (rooms != null)
		{
			foreach (var item in rooms)
			{
				item.DrawGizmos();
			}
		}

		/*
		if (theGrid != null)
		{
			for (int x = theGrid.minX; x < theGrid.maxX; x++)
			{
				for (int y = theGrid.minY; y < theGrid.maxY; y++)
				{
					Gizmos.color = (theGrid[x,y] == null) ? Color.gray : Color.red;
					Gizmos.DrawCube(new Vector3(x+.5f, y+.5f,0), new Vector3(1,1,.1f));
				}
			}
		}
		*/

		/*
		Gizmos.color = Color.green;

		if (graph != null)
		{
			foreach (var item in graph.graph)
			{
				foreach (var child in item.Value)
				{
					Gizmos.DrawLine(item.Key.centerV3, child.centerV3);
				}
			}
		}
		*/

		Gizmos.color = Color.blue;

		if (minGraph != null)
		{
			foreach (var item in minGraph.graph)
			{
				foreach (var child in item.Value)
				{
					Gizmos.DrawLine(item.Key.centerV3, child.centerV3);
				}
			}
		}

		/*
		foreach (var item in tris)
		{
			Gizmos.DrawLine(rooms[item.a].centerV3, rooms[item.b].centerV3);
			Gizmos.DrawLine(rooms[item.b].centerV3, rooms[item.c].centerV3);
			Gizmos.DrawLine(rooms[item.c].centerV3, rooms[item.a].centerV3);
		}
		*/
	}
}
