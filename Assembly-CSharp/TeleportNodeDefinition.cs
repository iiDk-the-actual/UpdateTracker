using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New TeleportNode Definition", menuName = "Teleportation/TeleportNode Definition", order = 1)]
public class TeleportNodeDefinition : ScriptableObject
{
	public TeleportNode Forward
	{
		get
		{
			return this.forward;
		}
	}

	public TeleportNode Backward
	{
		get
		{
			return this.backward;
		}
	}

	public void SetForward(TeleportNode node)
	{
		Debug.Log("registered fwd node " + node.name);
		this.forward = node;
	}

	public void SetBackward(TeleportNode node)
	{
		Debug.Log("registered bkwd node " + node.name);
		this.backward = node;
	}

	[SerializeField]
	private TeleportNode forward;

	[SerializeField]
	private TeleportNode backward;
}
