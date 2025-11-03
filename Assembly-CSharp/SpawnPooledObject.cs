using System;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPooledObject : MonoBehaviour
{
	private void Awake()
	{
		if (this._pooledObject == null)
		{
			return;
		}
		this._pooledObjectHash = PoolUtils.GameObjHashCode(this._pooledObject);
	}

	public void SpawnObject()
	{
		if (!this.ShouldSpawn())
		{
			return;
		}
		if (this._pooledObject == null || this._spawnLocation == null)
		{
			return;
		}
		GameObject gameObject = ObjectPools.instance.Instantiate(this._pooledObjectHash, true);
		gameObject.transform.position = this.SpawnLocation();
		gameObject.transform.rotation = this.SpawnRotation();
		gameObject.transform.localScale = base.transform.lossyScale;
	}

	private Vector3 SpawnLocation()
	{
		return this._spawnLocation.transform.position + this.offset;
	}

	private Quaternion SpawnRotation()
	{
		Quaternion quaternion = this._spawnLocation.transform.rotation;
		if (this.facePlayer)
		{
			quaternion = Quaternion.LookRotation(GTPlayer.Instance.headCollider.transform.position - this._spawnLocation.transform.position);
		}
		if (this.upright)
		{
			quaternion.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
		}
		return quaternion;
	}

	private bool ShouldSpawn()
	{
		return Random.Range(0, 100) < this.chanceToSpawn;
	}

	[SerializeField]
	private Transform _spawnLocation;

	[SerializeField]
	private GameObject _pooledObject;

	[FormerlySerializedAs("_offset")]
	public Vector3 offset;

	[FormerlySerializedAs("_upright")]
	public bool upright;

	[FormerlySerializedAs("_facePlayer")]
	public bool facePlayer;

	[FormerlySerializedAs("_chanceToSpawn")]
	[Range(0f, 100f)]
	public int chanceToSpawn = 100;

	private int _pooledObjectHash;
}
