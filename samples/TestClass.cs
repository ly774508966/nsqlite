using UnityEngine;
using System.Collections;
using NPlugins.Sqlite;
using System;

/// <summary>
/// 	Model classes MUST inherit from <see cref="Storable"/>. 
/// </summary>

public class TestClass : Storable
{
	public string Name { get; set; }

	public Vector3 Vector3 { get; set; } 

	public Color Color { get; set; } 

	public bool Boolean { get; set; } 

	public DateTime DateTime { get; set; }
}
