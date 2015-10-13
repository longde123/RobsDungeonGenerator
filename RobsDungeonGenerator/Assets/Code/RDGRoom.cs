using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// @author Rob Giusti
/// RDGRoom
/// </summary>
[System.Serializable]
public class RDGRoom {

	public int x,y;

	public int height, width;

	public int id;

	static int nextId = 0;

	public int buffer = 3;

	public bool pushed = false;

	public RoomType type = RoomType.ROOM;

	public Vector2 center
	{
		get
		{
			return new Vector2(x + width/2f, y + height/2f);
		}
	}

	public Vector3 centerV3
	{
		get
		{
			return new Vector3(x + width/2f, y + height/2f, 1);
		}
	}

	public static void ResetIDs(){ nextId = 0; }

	public RDGRoom (int x, int y, int width, int height)
	{
		id = nextId++;

		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	/// <summary>
	/// Checks if two lines overlap in one dimension
	/// Uses "left" and "right", but can be applied the same to any dimension
	/// </summary>
	/// <returns><c>true</c>, if dim was overlaped, <c>false</c> otherwise.</returns>
	/// <param name="left1">Left1.</param>
	/// <param name="right1">Right1.</param>
	/// <param name="left2">Left2.</param>
	/// <param name="right2">Right2.</param>
	public bool OverlapDim(int left1, int right1, int left2, int right2)
	{
		return left1 < right2 && left2 < right1;
	}

	/// <summary>
	/// Checks if this room overlaps another room in both the X and Y dimensions, 
	/// which constitutes a full collision
	/// </summary>
	/// <param name="other">Other.</param>
	public bool Overlaps(RDGRoom other)
	{
		return OverlapDim(x-buffer, x+width, other.x-buffer, other.x+other.width) && OverlapDim(y-buffer, y+height, other.y-buffer, other.y+other.height);
	}

	public void Push(RDGRoom other)
	{

		if (Mathf.Abs (center.x - other.center.x) >= Mathf.Abs(center.y - other.center.y))
		{
			if (center.x > other.center.x)
			{
				other.x--;
			} 
			else if (center.x < other.center.x)
			{
				other.x++;
			}
		}

		
		if (Mathf.Abs (center.x - other.center.x) <= Mathf.Abs(center.y - other.center.y))
		{
			if (center.y > other.center.y)
			{
				other.y--;
			}
			else if (center.y < other.center.y)
			{
				other.y++;
			}
		}

		pushed = false;
		other.pushed = true;
	}

	public void DrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(new Vector3(x, y, -id), new Vector3(x+width, y, -id));
		Gizmos.DrawLine(new Vector3(x, y, -id), new Vector3(x, y+height, -id));
		Gizmos.DrawLine(new Vector3(x, y+height, -id), new Vector3(x+width, y+height, -id));
		Gizmos.DrawLine(new Vector3(x+width, y, -id), new Vector3(x+width, y+height, -id));

		Gizmos.color = Color.yellow;
		for (int i = 1; i < width; i++) 
		{
			Gizmos.DrawLine(new Vector3(x+i, y, -id), new Vector3(x+i, y+height, -id));
		}
		for (int i = 1; i < height; i++) 
		{
			Gizmos.DrawLine(new Vector3(x, y+i, -id), new Vector3(x+width, y+i, -id));
		}

		Gizmos.color = Color.white;

		Handles.Label(centerV3, this.id.ToString());
	}
}

public enum RoomType { ROOM, CORRIDOR };