using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// @author Rob Giusti
/// RDGMath
/// Helper class with math methods for dungeon generation
/// </summary>
public static class RDGMath {

	/// <summary>
	/// Gets the manhattan distance between two rooms
	/// </summary>
	/// <returns>The between rooms.</returns>
	/// <param name="roomA">Room a.</param>
	/// <param name="roomB">Room b.</param>
	public static int DistBetweenRooms(RDGRoom roomA, RDGRoom roomB)
	{
		return (int) (Mathf.Abs (roomA.center.x - roomB.center.x) + Mathf.Abs(roomA.center.y - roomB.center.y));
	}

	public static Mesh MakeMesh(int w, int h)
	{
		return MakeMesh(
			new Vector3(-w/2f, -h/2f),
			new Vector3(w/2f, -h/2f),
			new Vector3(-w/2f, h/2f),
			new Vector3(w/2f, h/2f),
			w, h, 0
		);
	}

	/// <summary>
	/// Makes a mesh between the four given points with the specified number
	/// of segments in order to scale the material properly
	/// </summary>
	/// <param name="topRight">The top right point</param>
	/// <param name="botRight">The bottom right point</param>
	/// <param name="botLeft">THe bottom left point</param>
	/// <param name="topLeft">THe top left point</param>
	/// <param name="widthSegs">The number of segments to break it down horizontally</param>
	/// <param name="lengthSegs">The number of segments to break it down vertically</param>
	/// <returns>The fancy schmanchy mesh</returns>
	public static Mesh MakeMesh(Vector3 botLeft, Vector3 botRight,
	                            Vector3 topLeft, Vector3 topRight, int widthSegs, int lengthSegs, int rotation)
	{
		Mesh mesh = new Mesh();
		return MakeMesh(botLeft, botRight, topLeft, topRight, widthSegs, lengthSegs, rotation, mesh); ;
	}
	
	public static Mesh MakeMesh(Vector3 botLeft, Vector3 botRight,
	                            Vector3 topLeft, Vector3 topRight, int widthSegs, int lengthSegs, int rotation, Mesh mesh)
	{
		//The number of points is the number of segments
		//in that direction, plus one, because every segment
		//needs a buddy!
		int numHorizontalPoints = widthSegs + 1;
		int numVerticalPoints = lengthSegs + 1;
		
		//Total triangles = segments * six
		int numTriangles = widthSegs * lengthSegs * 6;
		
		//Total verts
		int numVertices = numHorizontalPoints * numVerticalPoints;
		
		//Make the arrays to fill in based on the counts we just determined
		Vector3[] vertices = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];
		int[] triangles = new int[numTriangles];
		
		float uvFactorX = 1.0f / widthSegs;
		float uvFactorY = 1.0f / lengthSegs;
		
		int index = 0;
		for (float y = 0; y < numVerticalPoints; y++)
		{
			//Disect sides to get lines
			Vector3 leftPoint = botLeft + (topLeft - botLeft) * (y / (numVerticalPoints-1));
			Vector3 rightPoint = botRight + (topRight - botRight) * (y / (numVerticalPoints-1));
			
			for (float x = 0; x < numHorizontalPoints; x++)
			{
				//Disect the line to get points
				vertices[index] = leftPoint + (rightPoint - leftPoint) * (x / (numHorizontalPoints-1));


				uvs[index] = new Vector2(((x)%2==0) ? 0 : 1, (y%2==0) ? 0 : 1);


				/*
				//Rotate UV's
				switch (rotation)
				{
				default:
				case (0):
					uvs[index] = new Vector2(x * uvFactorX, y * uvFactorY);
					break;
				case (1):
					uvs[index] = new Vector2(y * uvFactorY, (widthSegs - x) * uvFactorX);
					break;
				case (2):
					uvs[index] = new Vector2((widthSegs - x) * uvFactorX, (lengthSegs - y) * uvFactorY);
					break;
				case (3):
					uvs[index] = new Vector2((lengthSegs - y) * uvFactorY, x * uvFactorX);
					break;
				}
				*/

				index++;
			}
		}
		
		index = 0;
		for (int y = 0; y < lengthSegs; y++)
		{
			for (int x = 0; x < widthSegs; x++)
			{
				triangles[index++] = (y * numHorizontalPoints) + x;
				triangles[index++] = ((y + 1) * numHorizontalPoints) + x;
				triangles[index++] = (y * numHorizontalPoints) + x + 1;
				triangles[index++] = ((y + 1) * numHorizontalPoints) + x;
				triangles[index++] = ((y + 1) * numHorizontalPoints) + x + 1;
				triangles[index++] = (y * numHorizontalPoints) + x + 1;
			}
		}
		
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		
		mesh.RecalculateBounds();
		
		return mesh;
	}
	
	/// <summary>
	/// Resets the UVs for a mesh that's already done based on a new rotation
	/// </summary>
	/// <param name="mesh">The mesh to change</param>
	/// <param name="widthSegs"></param>
	/// <param name="lengthSegs"></param>
	/// <param name="rotation"></param>
	public static void SetMeshUVs(Mesh mesh, int widthSegs, int lengthSegs, int rotation)
	{
		Vector2[] uvs = new Vector2[(widthSegs + 1) * (lengthSegs + 1)];
		float uvFactorX = 1.0f / widthSegs;
		float uvFactorY = 1.0f / lengthSegs;
		int index = 0;
		for (int x = 0; x <= widthSegs; x++)
		{
			for (int y = 0; y <= lengthSegs; y++)
			{
				switch (rotation)
				{
				default:
				case (0):
					uvs[index] = new Vector2(x * uvFactorX, y * uvFactorY);
					break;
				case (1):
					uvs[index] = new Vector2(y * uvFactorY, (widthSegs - x) * uvFactorX);
					break;
				case (2):
					uvs[index] = new Vector2((widthSegs - x) * uvFactorX, (lengthSegs - y) * uvFactorY);
					break;
				case (3):
					uvs[index] = new Vector2((lengthSegs - y) * uvFactorY, x * uvFactorX);
					break;
				}
				index++;
			}
		}
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}

public enum Direction
{
    NORTH, SOUTH, EAST, WEST, NONE
}