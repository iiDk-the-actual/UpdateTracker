using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderMaterialResources", menuName = "Gorilla Tag/Builder/Resources", order = 0)]
public class BuilderResources : ScriptableObject
{
	public List<BuilderResourceQuantity> quantities;
}
