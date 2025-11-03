using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomLocalColliders : MonoBehaviour
{
	private void Start()
	{
		this.colliders = new List<Collider>();
		this.seekFreq = RandomLocalColliders.rand.NextFloat(this.minseekFreq, this.maxseekFreq);
	}

	private void Update()
	{
		this.timeSinceSeek += Time.deltaTime;
		if (this.timeSinceSeek > this.seekFreq)
		{
			this.seek();
			this.timeSinceSeek = 0f;
			this.seekFreq = RandomLocalColliders.rand.NextFloat(this.minseekFreq, this.maxseekFreq);
		}
	}

	private void seek()
	{
		float num = Mathf.Max(new float[]
		{
			base.transform.lossyScale.x,
			base.transform.lossyScale.y,
			base.transform.lossyScale.z
		});
		this.colliders.Clear();
		this.colliders.AddRange(Physics.OverlapSphere(base.transform.position, this.maxRadias * num));
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.minRadias * num);
		for (int i = 0; i < array.Length; i++)
		{
			this.colliders.Remove(array[i]);
		}
		if (this.colliders.Count > 0 && this.colliderFound != null)
		{
			this.colliderFound.Invoke(base.transform.position, this.colliders[RandomLocalColliders.rand.NextInt(this.colliders.Count)].transform.position);
		}
	}

	private static SRand rand = new SRand("RandomLocalColliders");

	[SerializeField]
	private float minseekFreq = 3f;

	[SerializeField]
	private float maxseekFreq = 6f;

	[SerializeField]
	private float minRadias = 1f;

	[SerializeField]
	private float maxRadias = 10f;

	[SerializeField]
	private LightningDispatcherEvent colliderFound;

	private List<Collider> colliders;

	private float timeSinceSeek;

	private float seekFreq;
}
