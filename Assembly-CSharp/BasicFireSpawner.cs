using System;
using GorillaTag;
using GorillaTag.Reactions;
using UnityEngine;

public class BasicFireSpawner : MonoBehaviour
{
	private void Awake()
	{
		this.scale = this.fireScaleMinMax.y;
	}

	public void InterpolateScale(float f)
	{
		this.scale = Mathf.Lerp(this.fireScaleMinMax.x, this.fireScaleMinMax.y, f);
	}

	public void Spawn()
	{
		if (this.firePool == null)
		{
			this.firePool = ObjectPools.instance.GetPoolByHash(in this.firePrefab);
		}
		FireManager.SpawnFire(this.firePool, base.transform.position, Vector3.up, this.scale);
	}

	[SerializeField]
	private HashWrapper firePrefab;

	[SerializeField]
	private Vector2 fireScaleMinMax = Vector2.one;

	private SinglePool firePool;

	private float scale;
}
