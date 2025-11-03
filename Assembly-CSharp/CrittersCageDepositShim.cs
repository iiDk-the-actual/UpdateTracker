using System;
using UnityEngine;

public class CrittersCageDepositShim : MonoBehaviour
{
	[ContextMenu("Copy Deposit Data To Shim")]
	private CrittersCageDeposit CopySpawnerDataInPrefab()
	{
		CrittersCageDeposit component = base.gameObject.GetComponent<CrittersCageDeposit>();
		this.cageBoxCollider = (BoxCollider)component.gameObject.GetComponent<Collider>();
		this.type = component.actorType;
		this.disableGrabOnAttach = component.disableGrabOnAttach;
		this.allowMultiAttach = component.allowMultiAttach;
		this.snapOnAttach = component.snapOnAttach;
		this.startLocation = component.depositStartLocation;
		this.endLocation = component.depositEndLocation;
		this.submitDuration = component.submitDuration;
		this.returnDuration = component.returnDuration;
		this.depositAudio = component.depositAudio;
		this.depositStartSound = component.depositStartSound;
		this.depositEmptySound = component.depositEmptySound;
		this.depositCritterSound = component.depositCritterSound;
		this.attachPointTransform = component.GetComponentInChildren<CrittersActor>().transform;
		this.visiblePlatformTransform = this.attachPointTransform.transform.GetChild(0).transform;
		return component;
	}

	[ContextMenu("Replace Deposit With Shim")]
	private void ReplaceSpawnerWithShim()
	{
		CrittersCageDeposit crittersCageDeposit = this.CopySpawnerDataInPrefab();
		if (crittersCageDeposit.attachPoint.GetComponent<Rigidbody>() != null)
		{
			Object.DestroyImmediate(crittersCageDeposit.attachPoint.GetComponent<Rigidbody>());
		}
		Object.DestroyImmediate(crittersCageDeposit.attachPoint);
		Object.DestroyImmediate(crittersCageDeposit);
	}

	public BoxCollider cageBoxCollider;

	public CrittersActor.CrittersActorType type;

	public bool disableGrabOnAttach;

	public bool allowMultiAttach;

	public bool snapOnAttach;

	public Vector3 startLocation;

	public Vector3 endLocation;

	public float submitDuration;

	public float returnDuration;

	public AudioSource depositAudio;

	public AudioClip depositStartSound;

	public AudioClip depositEmptySound;

	public AudioClip depositCritterSound;

	public Transform attachPointTransform;

	public Transform visiblePlatformTransform;
}
