using System;
using UnityEngine;

public class GRSummonerEgg : MonoBehaviour
{
	private void Awake()
	{
		this.summonedEntity = base.GetComponent<GRSummonedEntity>();
	}

	private void Start()
	{
		this.hatchTime = Random.Range(this.minHatchTime, this.maxHatchTime);
		Rigidbody component = base.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.position = base.transform.position;
			component.rotation = base.transform.rotation;
			component.linearVelocity = Vector3.up * 2f;
			component.angularVelocity = Vector3.zero;
		}
		base.Invoke("HatchEgg", this.hatchTime);
	}

	public void HatchEgg()
	{
		GRBreakable component = base.GetComponent<GRBreakable>();
		if (component)
		{
			component.BreakLocal();
		}
		if (this.entity.IsAuthority())
		{
			Vector3 vector = this.entity.transform.position + this.spawnOffset;
			Quaternion identity = Quaternion.identity;
			GameEntityManager gameEntityManager = GhostReactorManager.Get(this.entity).gameEntityManager;
			Debug.Log(string.Format("Attempting to spawn {0} from egg at {1}", this.entityPrefabToSpawn.name, vector.ToString()), this);
			gameEntityManager.RequestCreateItem(this.entityPrefabToSpawn.name.GetStaticHash(), vector, identity, (long)((this.summonedEntity != null) ? this.summonedEntity.GetSummonerNetID() : 0));
		}
		base.Invoke("DestroySelf", 2f);
		this.hatchSound.Play(this.hatchAudio);
	}

	private void Update()
	{
	}

	public void DestroySelf()
	{
		if (this.entity.IsAuthority())
		{
			this.entity.manager.RequestDestroyItem(this.entity.id);
		}
	}

	public GameEntity entity;

	public AudioSource hatchAudio;

	public AbilitySound hatchSound;

	public GameEntity entityPrefabToSpawn;

	public Vector3 spawnOffset = new Vector3(0f, 0f, 0.3f);

	public float minHatchTime = 3f;

	public float maxHatchTime = 6f;

	private float hatchTime = 2f;

	private GRSummonedEntity summonedEntity;
}
