using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DayCycleTextures", menuName = "Gorilla Tag/Day Cycle Textures", order = 0)]
public class DayCycleTexturesSO : ScriptableObject
{
	public DayCycleTextureMoment[] moments = new DayCycleTextureMoment[10];
}
