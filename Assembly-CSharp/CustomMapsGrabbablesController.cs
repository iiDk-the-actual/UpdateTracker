using System;
using GorillaExtensions;
using GT_CustomMapSupportRuntime;
using UnityEngine;

public class CustomMapsGrabbablesController : MonoBehaviour, IGameEntityComponent
{
	private void Awake()
	{
		this.isGrabbed = false;
		GameEntity gameEntity = this.entity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.entity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.OnReleased));
	}

	private void OnDestroy()
	{
		GameEntity gameEntity = this.entity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.entity;
		gameEntity2.OnReleased = (Action)Delegate.Remove(gameEntity2.OnReleased, new Action(this.OnReleased));
	}

	public void OnEntityInit()
	{
		GTDev.Log<string>("CustomMapsGrabbablesController::OnEntityInit", null);
		if (MapSpawnManager.instance == null)
		{
			return;
		}
		base.transform.parent = MapSpawnManager.instance.transform;
		byte b;
		GrabbableEntity.UnpackCreateData(this.entity.createData, out b, out this.luaAgentID);
		MapEntity mapEntity;
		if (!MapSpawnManager.instance.SpawnEntity((int)b, out mapEntity))
		{
			GTDev.LogError<string>("CustomMapsGrabbablesController::OnEntityInit could not spawn grabbable", null);
			Object.Destroy(base.gameObject);
			return;
		}
		GrabbableEntity grabbableEntity = (GrabbableEntity)mapEntity;
		if (grabbableEntity == null)
		{
			return;
		}
		grabbableEntity.gameObject.SetActive(true);
		grabbableEntity.transform.parent = this.entity.transform;
		grabbableEntity.transform.localPosition = Vector3.zero;
		grabbableEntity.transform.localRotation = Quaternion.identity;
		this.returnParent = this.entity.transform.parent;
		this.entity.audioSource = grabbableEntity.audioSource;
		this.entity.catchSound = grabbableEntity.catchSound;
		this.entity.catchSoundVolume = grabbableEntity.catchSoundVolume;
		this.entity.throwSound = grabbableEntity.throwSound;
		this.entity.throwSoundVolume = grabbableEntity.throwSoundVolume;
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Prop");
		}
	}

	public int GetGrabbingActor()
	{
		if (!this.isGrabbed)
		{
			return -1;
		}
		return this.entity.heldByActorNumber;
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
	}

	private void OnGrabbed()
	{
		this.isGrabbed = true;
	}

	private void OnReleased()
	{
		this.isGrabbed = false;
		if (this.returnParent.IsNotNull())
		{
			this.entity.transform.parent = this.returnParent;
		}
	}

	public GameEntity entity;

	public short luaAgentID;

	private bool isGrabbed;

	private Transform returnParent;
}
