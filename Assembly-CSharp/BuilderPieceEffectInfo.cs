using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderPieceEffectInfo", menuName = "Gorilla Tag/Builder/EffectInfo", order = 0)]
public class BuilderPieceEffectInfo : ScriptableObject
{
	public GameObject placeVFX;

	public GameObject disconnectVFX;

	public GameObject grabbedVFX;

	public GameObject locationLockVFX;

	public GameObject recycleVFX;

	public GameObject tooHeavyVFX;
}
