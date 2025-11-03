using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ReleaseCageWhenUpsideDown : MonoBehaviour
{
	private void Awake()
	{
		this.cage = base.GetComponentInChildren<CrittersCage>();
	}

	private void Update()
	{
		this.cage.inReleasingPosition = Vector3.Angle(base.transform.up, Vector3.down) < this.releaseCritterThreshold;
	}

	public CrittersCage cage;

	[FormerlySerializedAs("dumpThreshold")]
	[FormerlySerializedAs("angle")]
	public float releaseCritterThreshold = 30f;
}
