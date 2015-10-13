using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// @author Rob Giusti
/// MWGround
/// </summary>
public class RDGGround : MonoBehaviour {

	public void Init(Vector2 size)
	{
        transform.localScale = size;
	}
}
