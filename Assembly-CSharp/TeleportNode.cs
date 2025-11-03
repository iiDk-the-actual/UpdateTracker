using System;
using System.Collections;
using GorillaLocomotion;
using UnityEngine;

public class TeleportNode : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		if (Time.time - this.teleportTime < 0.1f)
		{
			return;
		}
		base.OnBoxTriggered();
		Transform transform;
		if (!this.teleportFromRef.TryResolve<Transform>(out transform))
		{
			Debug.LogError("[TeleportNode] Failed to resolve teleportFromRef.");
			return;
		}
		Transform transform2;
		if (!this.teleportToRef.TryResolve<Transform>(out transform2))
		{
			Debug.LogError("[TeleportNode] Failed to resolve teleportToRef.");
			return;
		}
		GTPlayer instance = GTPlayer.Instance;
		if (instance == null)
		{
			Debug.LogError("[TeleportNode] GTPlayer.Instance is null.");
			return;
		}
		Physics.SyncTransforms();
		Vector3 vector = transform2.TransformPoint(transform.InverseTransformPoint(instance.transform.position));
		Quaternion quaternion = Quaternion.Inverse(transform.rotation) * instance.transform.rotation;
		Quaternion quaternion2 = transform2.rotation * quaternion;
		base.StartCoroutine(this.DelayedTeleport(instance, vector, quaternion2));
		this.teleportTime = Time.time;
	}

	private IEnumerator DelayedTeleport(GTPlayer p, Vector3 position, Quaternion rotation)
	{
		yield return null;
		p.TeleportTo(position, rotation, true, false);
		yield break;
	}

	[SerializeField]
	private XSceneRef teleportFromRef;

	[SerializeField]
	private XSceneRef teleportToRef;

	private float teleportTime;
}
