using System;
using Unity.XR.CoreUtils;
using UnityEngine;

public class GRBarrierSpectral : MonoBehaviour, IGameEntityComponent, IGameHittable
{
	public void Awake()
	{
		this.hitFx.SetActive(false);
		this.destroyedFx.SetActive(false);
	}

	public void OnEntityInit()
	{
		this.entity.SetState((long)this.health);
		Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(this.entity.createData);
		base.transform.localScale = vector;
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
		int num = (int)newState;
		this.ChangeHealth(num);
	}

	public void OnImpact(GameHitType hitType)
	{
		if (hitType == GameHitType.Flash)
		{
			int num = Mathf.Max(this.health - 1, 0);
			this.ChangeHealth(num);
			if (this.entity.IsAuthority())
			{
				this.entity.RequestState(this.entity.id, (long)this.health);
			}
		}
	}

	private void ChangeHealth(int nextHealth)
	{
		if (this.health != nextHealth)
		{
			this.health = nextHealth;
			if (this.health == 0)
			{
				this.collider.enabled = false;
				this.visualMesh.enabled = false;
				this.audioSource.PlayOneShot(this.onDestroyedClip, this.onDestroyedVolume);
				this.destroyedFx.SetActive(false);
				this.destroyedFx.SetActive(true);
			}
			else
			{
				this.audioSource.PlayOneShot(this.onDamageClip, this.onDamageVolume);
				this.hitFx.SetActive(false);
				this.hitFx.SetActive(true);
			}
			this.RefreshVisuals();
		}
	}

	public bool IsHitValid(GameHitData hit)
	{
		return true;
	}

	public void OnHit(GameHitData hit)
	{
		GameHitType hitTypeId = (GameHitType)hit.hitTypeId;
		if (this.entity.manager.GetGameComponent<GRTool>(hit.hitByEntityId) != null)
		{
			this.OnImpact(hitTypeId);
		}
	}

	public void RefreshVisuals()
	{
		if (this.lastVisualUpdateHealth != this.health)
		{
			this.lastVisualUpdateHealth = this.health;
			Color color = this.visualMesh.material.GetColor("_BaseColor");
			color.a = (float)this.health / (float)this.maxHealth;
			this.visualMesh.material.SetColor("_BaseColor", color);
		}
	}

	public GameEntity entity;

	public MeshRenderer visualMesh;

	public Collider collider;

	public AudioSource audioSource;

	public AudioClip onDamageClip;

	public float onDamageVolume;

	public AudioClip onDestroyedClip;

	public float onDestroyedVolume;

	[SerializeField]
	private GameObject hitFx;

	[SerializeField]
	private GameObject destroyedFx;

	public int maxHealth = 3;

	[ReadOnly]
	public int health = 3;

	private int lastVisualUpdateHealth = -1;
}
