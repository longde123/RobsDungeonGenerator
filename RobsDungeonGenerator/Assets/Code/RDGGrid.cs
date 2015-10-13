using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// @author Rob Giusti
/// RDGGrid
/// </summary>
public class RDGGrid {

	RDGRoom[,] grid;

	int offsetX, offsetY;

	public int minX, minY, maxX, maxY;

	const int EDGE_BUFFR = 5;

	public Stack<RDGGridNode> lastPath;

	public RDGRoom this[int x, int y]
	{
		get
		{
			return grid[x-minX, y-minY];
		}
		set
		{
			grid[x-minX, y-minY] = value;
		}
	}

	public RDGRoom this[RDGGridNode node]
	{
		get
		{
			return this[node.x, node.y];
		}
	}

	public RDGGrid(int minX, int minY, int maxX, int maxY)
	{
		this.minX = minX-EDGE_BUFFR;
		this.minY = minY-EDGE_BUFFR;
		this.maxX = maxX+EDGE_BUFFR;
		this.maxY = maxY+EDGE_BUFFR;

		int diffX = maxX - minX;
		int diffY = maxY - minY;
		int sizeX = diffX + EDGE_BUFFR * 2;
		int sizeY = diffY + EDGE_BUFFR * 2;

		grid = new RDGRoom[sizeX, sizeY];
	}

	public void AddRoom(RDGRoom room)
	{
		for (int x = 0; x < room.width; x++) 
		{
			for (int y = 0; y < room.height; y++) 
			{
				this[room.x + x, room.y + y] = room;
			}
		}
	}

	public IEnumerator Pathfind(RDGRoom roomFrom, RDGRoom roomTo)
	{
		List<RDGGridNode> open = new List<RDGGridNode>();
		List<RDGGridNode> closed = new List<RDGGridNode>();

		RDGGridNode currNode = new RDGGridNode(x: Mathf.FloorToInt(roomFrom.center.x), y: Mathf.FloorToInt(roomFrom.center.y));

		open.Add(currNode);

		RDGGridNode otherNode;
		while(open.Count > 0)
		{
			currNode = open.OrderBy(x=>x.fullCost).ElementAt(0);
			open.Remove(currNode);

			//Debug.Log("Testing node: " + currNode.x + ", " + currNode.y);

			if (currNode.x == Mathf.FloorToInt(roomTo.center.x) && currNode.y == Mathf.FloorToInt(roomTo.center.y))
			{
				lastPath = BuildPath(currNode);
				yield break;
			}

			foreach (var item in GetNeighbors(currNode, roomFrom, roomTo))
			{
				otherNode = open.Find(n=>n.x==currNode.x && n.y==currNode.y);
				if (otherNode != null)
				{
					if (otherNode.cost > item.cost)
					{
						open.Remove(otherNode);
					}
					else
					{
						continue;
					}
				}
				otherNode = closed.Find(n=>n.x==currNode.x && n.y==currNode.y);
				if (otherNode != null)
				{
					if (otherNode.cost > item.cost)
					{
						closed.Remove(otherNode);
					}
					else
					{
						continue;
					}
				}
				open.Add(item);
				//yield return null;
			}

			closed.Add(currNode);
		}


	}

	Stack<RDGGridNode> BuildPath(RDGGridNode lastNode)
	{
		Stack<RDGGridNode> path = new Stack<RDGGridNode>();

		do
		{
			path.Push(lastNode);
			lastNode = lastNode.parent;
		}while(lastNode != null);

		return path;
	}

	List<RDGGridNode> GetNeighbors(RDGGridNode fromNode, RDGRoom roomFrom = null, RDGRoom roomTo = null)
	{
		List<RDGGridNode> neighbors = new List<RDGGridNode>();

		RDGGridNode currNode;
		if (fromNode.x > minX)
		{
			currNode = new RDGGridNode(x: fromNode.x-1, y: fromNode.y, dir: Direction.WEST, parent: fromNode);
			currNode.SetCost(fromNode, roomFrom, roomTo, this);
			neighbors.Add (currNode);
		}
		if (fromNode.x < maxX)
		{
			currNode = new RDGGridNode(x: fromNode.x+1, y: fromNode.y, dir: Direction.EAST, parent: fromNode);
			currNode.SetCost(fromNode, roomFrom, roomTo, this);
			neighbors.Add (currNode);
		}
		if (fromNode.y > minY)
		{
			currNode = new RDGGridNode(x: fromNode.x, y: fromNode.y-1, dir: Direction.SOUTH, parent: fromNode);
			currNode.SetCost(fromNode, roomFrom, roomTo, this);
			neighbors.Add (currNode);
		}
		if (fromNode.y < maxY)
		{
			currNode = new RDGGridNode(x: fromNode.x, y: fromNode.y+1, dir: Direction.NORTH, parent: fromNode);
			currNode.SetCost(fromNode, roomFrom, roomTo, this);
			neighbors.Add (currNode);
		}

		return neighbors;
	}
}

public class RDGGridNode
{
	public int x, y;
	public Direction dir;
	public int cost;
	public RDGGridNode parent = null;
	public int hueristic;

	public int fullCost{ get{ return cost + hueristic; } }

	public RDGGridNode(int x = 0, int y = 0, Direction dir = Direction.NONE, int cost = 0, RDGGridNode parent = null)
	{
		this.x = x;
		this.y = y;
		this.dir = dir;
		this.cost = cost;
		this.parent = parent;
	}

	public void SetCost(RDGGridNode node, RDGRoom roomFrom, RDGRoom roomTo, RDGGrid grid)
	{
		hueristic = Mathf.Abs(x - Mathf.CeilToInt(roomTo.center.x)) + Mathf.Abs(y-Mathf.CeilToInt(roomTo.center.y));

		if ((roomFrom!=null && grid[x,y]==roomFrom) || (roomTo!=null && grid[x,y]==roomTo))
		{
			cost = 0;
		}
		else if (grid[x, y] != null)
		{
			if (grid[x,y].type == RoomType.CORRIDOR)
			{
				cost = 0;
			}
			else
			{
				cost = int.MaxValue - hueristic; //Subtract the huersitic so that we don't overflow when we call fullCost
			}
		}
		else
		{
			cost = parent.cost + 1;
			if (dir != node.dir)
			{
				cost += 2;
			}
		}
	}
}
