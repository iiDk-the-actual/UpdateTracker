using System;
using UnityEngine;

public struct GameHitData
{
	public GameEntityId hitEntityId;

	public GameEntityId hitByEntityId;

	public int hitTypeId;

	public Vector3 hitEntityPosition;

	public Vector3 hitPosition;

	public Vector3 hitImpulse;

	public int hitAmount;
}
