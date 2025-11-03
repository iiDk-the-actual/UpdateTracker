using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaLocomotion.Climbing;
using GorillaLocomotion.Gameplay;
using GorillaLocomotion.Swimming;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsGorillaRopeSwing : GorillaRopeSwing
{
	protected override void Awake()
	{
		base.CalculateId(true);
		base.StartCoroutine(this.WaitForRopeLength());
	}

	protected override void Start()
	{
	}

	protected override void OnEnable()
	{
		if (!this.isRopeLengthSet)
		{
			return;
		}
		base.OnEnable();
	}

	public void SetRopeLength(int length)
	{
		this.ropeLength = length;
		this.isRopeLengthSet = true;
	}

	public void SetRopeProperties(GTObjectPlaceholder placeholder)
	{
		this.ropePlaceholder = placeholder;
		this.ropeLength = this.ropePlaceholder.ropeLength;
		this.ropeBitGenOffset = this.ropePlaceholder.ropeSegmentGenerationOffset;
		this.preExistingSegments = this.ropePlaceholder.ropeSwingSegments;
		this.ropeScale = this.ropePlaceholder.transform.localScale;
		base.transform.localScale = Vector3.one;
		this.isRopeLengthSet = true;
	}

	private IEnumerator WaitForRopeLength()
	{
		while (!this.isRopeLengthSet)
		{
			yield return null;
		}
		this.RopeGeneration();
		base.Awake();
		base.OnEnable();
		base.Start();
		yield break;
	}

	private void RopeGeneration()
	{
		List<Transform> list = new List<Transform>();
		if (this.preExistingSegments != null && this.preExistingSegments.Count > 0)
		{
			for (int i = 0; i < this.preExistingSegments.Count; i++)
			{
				this.preExistingSegments[i].transform.SetParent(base.transform);
				GorillaClimbable gorillaClimbable = this.preExistingSegments[i].AddComponent<GorillaClimbable>();
				gorillaClimbable.snapX = this.snapX;
				gorillaClimbable.snapY = this.snapY;
				gorillaClimbable.snapZ = this.snapZ;
				gorillaClimbable.maxDistanceSnap = this.maxDistanceSnap;
				gorillaClimbable.clip = this.onGrabSFX;
				gorillaClimbable.clipOnFullRelease = this.OnReleaseSFX;
				GorillaRopeSegment gorillaRopeSegment = this.preExistingSegments[i].AddComponent<GorillaRopeSegment>();
				gorillaRopeSegment.swing = this;
				gorillaRopeSegment.boneIndex = this.preExistingSegments[i].boneIndex;
				list.Add(this.preExistingSegments[i].transform);
			}
			base.transform.localScale = this.ropeScale;
			this.ropePlaceholder.transform.localScale = Vector3.one;
		}
		else
		{
			Vector3 vector = Vector3.zero;
			float y = this.prefabRopeBit.GetComponentInChildren<Renderer>().bounds.size.y;
			WaterVolume[] array = Object.FindObjectsByType<WaterVolume>(FindObjectsSortMode.None);
			List<Collider> list2 = new List<Collider>(array.Length);
			WaterVolume[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				foreach (Collider collider in array2[j].volumeColliders)
				{
					if (!(collider == null))
					{
						list2.Add(collider);
					}
				}
			}
			for (int k = 0; k < this.ropeLength + 1; k++)
			{
				bool flag = false;
				if (list2.Count > 0)
				{
					Collider collider2 = list2[0];
					if (collider2 != null)
					{
						Vector3 vector2 = base.transform.position + vector;
						Vector3 vector3 = vector2 + new Vector3(0f, -y, 0f);
						flag = collider2.bounds.Contains(vector2) || collider2.bounds.Contains(vector3);
					}
				}
				GameObject gameObject = Object.Instantiate<GameObject>(flag ? this.partiallyUnderwaterPrefab : this.prefabRopeBit, base.transform);
				gameObject.name = string.Format("RopeBone_{0:00}", k);
				gameObject.transform.localPosition = vector;
				gameObject.transform.localRotation = Quaternion.identity;
				vector += new Vector3(0f, -this.ropeBitGenOffset, 0f);
				GorillaRopeSegment component = gameObject.GetComponent<GorillaRopeSegment>();
				component.swing = this;
				component.boneIndex = k;
				list.Add(gameObject.transform);
			}
			list[0].GetComponent<BoxCollider>().center = new Vector3(0f, -0.65f, 0f);
			list[0].GetComponent<BoxCollider>().size = new Vector3(0.3f, 0.65f, 0.3f);
		}
		if (list.Count > 0)
		{
			list.Last<Transform>().gameObject.SetActive(false);
		}
		this.nodes = list.ToArray();
	}

	[SerializeField]
	private GameObject partiallyUnderwaterPrefab;

	private bool isRopeLengthSet;

	private List<RopeSwingSegment> preExistingSegments;

	private GTObjectPlaceholder ropePlaceholder;

	private Vector3 ropeScale = Vector3.one;

	public bool snapX;

	public bool snapY;

	public bool snapZ;

	public float maxDistanceSnap = 0.05f;

	public AudioClip onGrabSFX;

	public AudioClip OnReleaseSFX;
}
