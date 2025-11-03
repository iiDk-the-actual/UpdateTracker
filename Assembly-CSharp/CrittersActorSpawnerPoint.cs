using System;
using System.Collections.Generic;
using Photon.Pun;

public class CrittersActorSpawnerPoint : CrittersActor
{
	public event Action<CrittersActor> OnSpawnChanged;

	public override void Initialize()
	{
		base.Initialize();
		base.UpdateImpulses(false, false);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		this.spawnedActorID = -1;
		this.spawnedActor = null;
	}

	public void SetSpawnedActor(CrittersActor actor)
	{
		if (this.spawnedActor == actor)
		{
			return;
		}
		this.spawnedActor = actor;
		if (this.spawnedActor != null)
		{
			this.spawnedActorID = this.spawnedActor.actorId;
		}
		else
		{
			this.spawnedActorID = -1;
		}
		Action<CrittersActor> onSpawnChanged = this.OnSpawnChanged;
		if (onSpawnChanged != null)
		{
			onSpawnChanged(this.spawnedActor);
		}
		this.updatedSinceLastFrame = true;
	}

	private void UpdateSpawnedActor(int newSpawnedActorID)
	{
		if (this.spawnedActorID == newSpawnedActorID)
		{
			return;
		}
		if (newSpawnedActorID == -1)
		{
			this.spawnedActorID = newSpawnedActorID;
			this.spawnedActor = null;
		}
		else
		{
			CrittersActor crittersActor;
			if (!CrittersManager.instance.actorById.TryGetValue(newSpawnedActorID, out crittersActor))
			{
				return;
			}
			this.spawnedActorID = newSpawnedActorID;
			this.spawnedActor = crittersActor;
		}
		Action<CrittersActor> onSpawnChanged = this.OnSpawnChanged;
		if (onSpawnChanged == null)
		{
			return;
		}
		onSpawnChanged(this.spawnedActor);
	}

	public override void SendDataByCrittersActorType(PhotonStream stream)
	{
		base.SendDataByCrittersActorType(stream);
		stream.SendNext(this.spawnedActorID);
	}

	public override bool UpdateSpecificActor(PhotonStream stream)
	{
		if (!base.UpdateSpecificActor(stream))
		{
			return false;
		}
		int num;
		if (!CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num))
		{
			return false;
		}
		if (num < -1 || num >= CrittersManager.instance.universalActorId)
		{
			return false;
		}
		this.UpdateSpawnedActor(num);
		return true;
	}

	public override int AddActorDataToList(ref List<object> objList)
	{
		base.AddActorDataToList(ref objList);
		objList.Add(this.spawnedActorID);
		return this.TotalActorDataLength();
	}

	public override int TotalActorDataLength()
	{
		return base.BaseActorDataLength() + 1;
	}

	public override int UpdateFromRPC(object[] data, int startingIndex)
	{
		startingIndex += base.UpdateFromRPC(data, startingIndex);
		int num;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex], out num))
		{
			return this.TotalActorDataLength();
		}
		if (num >= -1 && num < CrittersManager.instance.universalActorId)
		{
			return this.TotalActorDataLength();
		}
		this.UpdateSpawnedActor(num);
		return this.TotalActorDataLength();
	}

	private CrittersActor spawnedActor;

	private int spawnedActorID = -1;
}
