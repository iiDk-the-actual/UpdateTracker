using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CrittersActor : MonoBehaviour
{
	public event Action<CrittersActor> OnGrabbedChild;

	public virtual void UpdateAverageSpeed()
	{
		this.averageSpeed[this.averageSpeedIndex] = (base.transform.position - this.lastPosition).magnitude;
		this.averageSpeedIndex++;
		this.averageSpeedIndex %= 6;
		this.lastPosition = base.transform.position;
	}

	public float GetAverageSpeed
	{
		get
		{
			return (this.averageSpeed[0] + this.averageSpeed[1] + this.averageSpeed[2] + this.averageSpeed[3] + this.averageSpeed[4] + this.averageSpeed[5]) / 6f;
		}
	}

	protected virtual void Awake()
	{
		this._isOnPlayerDefault = this.isOnPlayer;
	}

	public virtual void Initialize()
	{
		if (this.defaultParentTransform == null)
		{
			this.SetDefaultParent(base.transform.parent);
		}
		if (this.rb == null)
		{
			this.rb = base.GetComponent<Rigidbody>();
		}
		if (this.rb == null)
		{
			Debug.LogError("I should have a rigidbody, but I don't!", base.gameObject);
		}
		this.wasEnabled = false;
		this.isEnabled = true;
		this.TogglePhysics(this.usesRB);
		if (!this.rb.isKinematic)
		{
			this.rb.linearVelocity = Vector3.zero;
			this.rb.angularVelocity = Vector3.zero;
		}
		if (this.resetPhysicsOnSpawn)
		{
			this.rb.linearVelocity = Vector3.zero;
			this.rb.angularVelocity = Vector3.zero;
			this.lastImpulseVelocity = Vector3.zero;
		}
		if (this.subObjectIndex >= 0 && this.subObjectIndex < this.subObjects.Length)
		{
			for (int i = 0; i < this.subObjects.Length; i++)
			{
				this.subObjects[i].SetActive(i == this.subObjectIndex);
			}
		}
		this.colliders = new Collider[50];
		if (this.preventDespawnUntilGrabbed)
		{
			this.isDespawnBlocked = true;
			this.despawnTime = 0.0;
		}
		else
		{
			this.isDespawnBlocked = false;
			this.despawnTime = (double)this.despawnDelay + (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		}
		this.rb.includeLayers = 0;
		this.rb.excludeLayers = CrittersManager.instance.containerLayer;
	}

	public virtual void OnEnable()
	{
		CrittersManager.RegisterActor(this);
		this.Initialize();
	}

	public virtual void OnDisable()
	{
		this.CleanupActor();
	}

	public virtual string GetActorSubtype()
	{
		if (this.subObjectIndex >= 0 && this.subObjectIndex < this.subObjects.Length)
		{
			return this.subObjects[this.subObjectIndex].name;
		}
		return base.name;
	}

	protected virtual void CleanupActor()
	{
		CrittersManager.DeregisterActor(this);
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
		for (int i = 0; i < this.subObjects.Length; i++)
		{
			if (this.subObjects[i].activeSelf)
			{
				this.subObjects[i].transform.localRotation = Quaternion.identity;
				this.subObjects[i].transform.localPosition = Vector3.zero;
				this.subObjects[i].SetActive(false);
			}
		}
		this.ReleasedEvent.Invoke(this);
		this.ReleasedEvent.RemoveAllListeners();
		this.isEnabled = false;
		this.wasEnabled = true;
		this.isOnPlayer = this._isOnPlayerDefault;
		this.rigPlayerId = -1;
		this.rigIndex = -1;
		this.despawnTime = 0.0;
		this.isDespawnBlocked = false;
		this.rb.isKinematic = false;
		if (this.parentActorId >= 0)
		{
			this.AttemptRemoveStoredObjectCollider(this.parentActorId, false);
		}
		this.parentActorId = -1;
		this.parentActor = null;
		this.lastParentActorId = -1;
		this.isGrabDisabled = false;
		this.lastGrabbedPlayer = -1;
		this.lastImpulsePosition = Vector3.zero;
		this.lastImpulseVelocity = Vector3.zero;
		this.lastImpulseQuaternion = Quaternion.identity;
		this.lastImpulseTime = -1.0;
		this.localLastImpulse = -1.0;
		this.updatedSinceLastFrame = false;
		this.localCanStore = false;
	}

	public virtual bool ProcessLocal()
	{
		this.updatedSinceLastFrame |= this.isEnabled != this.wasEnabled || this.parentActorId != this.lastParentActorId;
		this.lastParentActorId = this.parentActorId;
		this.wasEnabled = this.isEnabled;
		return this.updatedSinceLastFrame;
	}

	public virtual void ProcessRemote()
	{
		bool flag = this.forceUpdate;
		this.forceUpdate = false;
		if (base.gameObject.activeSelf != this.isEnabled)
		{
			base.gameObject.SetActive(this.isEnabled);
		}
		if (!this.isEnabled)
		{
			return;
		}
		bool flag2 = this.lastParentActorId == this.parentActorId || this.isOnPlayer || this.isSceneActor;
		bool flag3 = this.lastImpulseTime == this.localLastImpulse;
		if (flag2 && flag3 && !flag)
		{
			return;
		}
		if (!flag2)
		{
			if (this.lastParentActorId >= 0)
			{
				this.AttemptRemoveStoredObjectCollider(this.lastParentActorId, true);
			}
			this.lastParentActorId = this.parentActorId;
			if (this.parentActorId >= 0)
			{
				CrittersActor crittersActor;
				if (!CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
				{
					return;
				}
				this.parentActor = crittersActor.transform;
				base.transform.SetParent(this.parentActor, true);
				this.SetImpulse();
				if (crittersActor is CrittersBag)
				{
					((CrittersBag)crittersActor).AddStoredObjectCollider(this);
				}
				if (crittersActor.isOnPlayer)
				{
					this.lastGrabbedPlayer = crittersActor.rigPlayerId;
				}
				crittersActor.RemoteGrabbed(this);
				return;
			}
			else if (this.parentActorId == -1)
			{
				this.parentActor = null;
				this.SetTransformToDefaultParent(false);
				this.HandleRemoteReleased();
				this.SetImpulse();
				return;
			}
		}
		else
		{
			this.SetImpulse();
		}
	}

	public virtual void SetImpulse()
	{
		if (this.isOnPlayer || this.isSceneActor)
		{
			return;
		}
		this.localLastImpulse = this.lastImpulseTime;
		this.MoveActor(this.lastImpulsePosition, this.lastImpulseQuaternion, this.parentActorId >= 0, false, true);
		this.TogglePhysics(this.usesRB && this.parentActorId == -1);
		if (!this.rb.isKinematic)
		{
			this.rb.linearVelocity = this.lastImpulseVelocity;
			this.rb.angularVelocity = this.lastImpulseAngularVelocity;
		}
	}

	public virtual void TogglePhysics(bool enable)
	{
		if (enable)
		{
			this.rb.isKinematic = false;
			this.rb.interpolation = RigidbodyInterpolation.Interpolate;
			this.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			return;
		}
		this.rb.isKinematic = true;
		this.rb.interpolation = RigidbodyInterpolation.None;
		this.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	}

	public void AddPlayerCrittersActorDataToList(ref List<object> objList)
	{
		objList.Add(this.actorId);
		objList.Add(this.isOnPlayer);
		objList.Add(this.rigPlayerId);
		objList.Add(this.rigIndex);
	}

	public virtual int AddActorDataToList(ref List<object> objList)
	{
		objList.Add(this.actorId);
		objList.Add(this.lastImpulseTime);
		objList.Add(this.lastImpulsePosition);
		objList.Add(this.lastImpulseVelocity);
		objList.Add(this.lastImpulseAngularVelocity);
		objList.Add(this.lastImpulseQuaternion);
		objList.Add(this.parentActorId);
		objList.Add(this.isEnabled);
		objList.Add(this.subObjectIndex);
		return this.BaseActorDataLength();
	}

	public int BaseActorDataLength()
	{
		return 9;
	}

	public virtual int TotalActorDataLength()
	{
		return 9;
	}

	public virtual int UpdateFromRPC(object[] data, int startingIndex)
	{
		double num;
		if (!CrittersManager.ValidateDataType<double>(data[startingIndex + 1], out num))
		{
			return this.BaseActorDataLength();
		}
		Vector3 vector;
		if (!CrittersManager.ValidateDataType<Vector3>(data[startingIndex + 2], out vector))
		{
			return this.BaseActorDataLength();
		}
		Vector3 vector2;
		if (!CrittersManager.ValidateDataType<Vector3>(data[startingIndex + 3], out vector2))
		{
			return this.BaseActorDataLength();
		}
		Vector3 vector3;
		if (!CrittersManager.ValidateDataType<Vector3>(data[startingIndex + 4], out vector3))
		{
			return this.BaseActorDataLength();
		}
		Quaternion quaternion;
		if (!CrittersManager.ValidateDataType<Quaternion>(data[startingIndex + 5], out quaternion))
		{
			return this.BaseActorDataLength();
		}
		int num2;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 6], out num2))
		{
			return this.BaseActorDataLength();
		}
		bool flag;
		if (!CrittersManager.ValidateDataType<bool>(data[startingIndex + 7], out flag))
		{
			return this.BaseActorDataLength();
		}
		int num3;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 8], out num3))
		{
			return this.BaseActorDataLength();
		}
		this.lastImpulseTime = num.GetFinite();
		(ref this.lastImpulsePosition).SetValueSafe(in vector);
		(ref this.lastImpulseVelocity).SetValueSafe(in vector2);
		(ref this.lastImpulseAngularVelocity).SetValueSafe(in vector3);
		(ref this.lastImpulseQuaternion).SetValueSafe(in quaternion);
		this.parentActorId = num2;
		this.isEnabled = flag;
		this.subObjectIndex = num3;
		this.forceUpdate = true;
		if (this.isEnabled)
		{
			base.gameObject.SetActive(true);
		}
		for (int i = 0; i < this.subObjects.Length; i++)
		{
			this.subObjects[i].SetActive(i == this.subObjectIndex);
		}
		return this.BaseActorDataLength();
	}

	public int UpdatePlayerCrittersActorFromRPC(object[] data, int startingIndex)
	{
		bool flag;
		if (!CrittersManager.ValidateDataType<bool>(data[startingIndex + 1], out flag))
		{
			return 4;
		}
		int num;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 2], out num))
		{
			return 4;
		}
		int num2;
		if (!CrittersManager.ValidateDataType<int>(data[startingIndex + 3], out num2))
		{
			return 4;
		}
		this.isOnPlayer = flag;
		this.rigPlayerId = num;
		this.rigIndex = num2;
		if (this.rigPlayerId == -1 && CrittersManager.instance.guard.currentOwner != null)
		{
			this.rigPlayerId = CrittersManager.instance.guard.currentOwner.ActorNumber;
		}
		this.PlacePlayerCrittersActor();
		return 4;
	}

	public virtual bool UpdateSpecificActor(PhotonStream stream)
	{
		double num;
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		Quaternion quaternion;
		int num2;
		bool flag;
		int num3;
		if (!(CrittersManager.ValidateDataType<double>(stream.ReceiveNext(), out num) & CrittersManager.ValidateDataType<Vector3>(stream.ReceiveNext(), out vector) & CrittersManager.ValidateDataType<Vector3>(stream.ReceiveNext(), out vector2) & CrittersManager.ValidateDataType<Vector3>(stream.ReceiveNext(), out vector3) & CrittersManager.ValidateDataType<Quaternion>(stream.ReceiveNext(), out quaternion) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num2) & CrittersManager.ValidateDataType<bool>(stream.ReceiveNext(), out flag) & CrittersManager.ValidateDataType<int>(stream.ReceiveNext(), out num3)))
		{
			return false;
		}
		float num4 = 10000f;
		if ((in vector).IsValid(in num4))
		{
			(ref this.lastImpulsePosition).SetValueSafe(in vector);
		}
		num4 = 10000f;
		if ((in vector2).IsValid(in num4))
		{
			(ref this.lastImpulseVelocity).SetValueSafe(in vector2);
		}
		if ((in quaternion).IsValid())
		{
			(ref this.lastImpulseQuaternion).SetValueSafe(in quaternion);
		}
		num4 = 10000f;
		if ((in vector3).IsValid(in num4))
		{
			(ref this.lastImpulseAngularVelocity).SetValueSafe(in vector3);
		}
		if (num2 >= -1 && num2 < CrittersManager.instance.universalActorId)
		{
			this.parentActorId = num2;
		}
		if (num3 < this.subObjects.Length)
		{
			this.subObjectIndex = num3;
		}
		this.isEnabled = flag;
		this.lastImpulseTime = num;
		if (this.isEnabled != base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(this.isEnabled);
		}
		if (this.isEnabled && this.subObjectIndex >= 0)
		{
			this.subObjects[this.subObjectIndex].SetActive(true);
		}
		else if (!this.isEnabled && this.subObjectIndex >= 0)
		{
			this.subObjects[this.subObjectIndex].SetActive(false);
		}
		return true;
	}

	public virtual void SendDataByCrittersActorType(PhotonStream stream)
	{
		stream.SendNext(this.actorId);
		stream.SendNext(this.lastImpulseTime);
		stream.SendNext(this.lastImpulsePosition);
		stream.SendNext(this.lastImpulseVelocity);
		stream.SendNext(this.lastImpulseAngularVelocity);
		stream.SendNext(this.lastImpulseQuaternion);
		stream.SendNext(this.parentActorId);
		stream.SendNext(this.isEnabled);
		stream.SendNext(this.subObjectIndex);
		this.updatedSinceLastFrame = false;
	}

	public virtual void OnHover(bool isLeft)
	{
		GorillaTagger.Instance.StartVibration(isLeft, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
	}

	public virtual bool CanBeGrabbed(CrittersActor grabbedBy)
	{
		return !this.isGrabDisabled && this.grabbable;
	}

	public static CrittersActor GetRootActor(int actorId)
	{
		CrittersActor crittersActor;
		if (!CrittersManager.instance.actorById.TryGetValue(actorId, out crittersActor))
		{
			return null;
		}
		if (crittersActor.parentActorId > -1)
		{
			return CrittersActor.GetRootActor(crittersActor.parentActorId);
		}
		return crittersActor;
	}

	public static CrittersActor GetParentActor(int actorId)
	{
		CrittersActor crittersActor;
		if (CrittersManager.instance.actorById.TryGetValue(actorId, out crittersActor))
		{
			return crittersActor;
		}
		return null;
	}

	public bool AllowGrabbingActor(CrittersActor grabbedBy)
	{
		if (this.parentActorId == -1)
		{
			return true;
		}
		if (grabbedBy.crittersActorType != CrittersActor.CrittersActorType.Grabber)
		{
			return true;
		}
		CrittersActor rootActor = CrittersActor.GetRootActor(grabbedBy.actorId);
		CrittersActor crittersActor;
		if (CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor))
		{
			if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.Bag)
			{
				if (!CrittersManager.instance.allowGrabbingFromBags)
				{
					CrittersActor rootActor2 = CrittersActor.GetRootActor(this.actorId);
					Debug.Log(string.Format("Grieffing - FromBag {0} == {1} || {2} == -1 || {3} == -1  - ", new object[] { rootActor2.rigPlayerId, rootActor.rigPlayerId, crittersActor.parentActorId, rootActor.rigPlayerId }) + string.Format(" {0}", rootActor2.rigPlayerId == rootActor.rigPlayerId || rootActor2.rigPlayerId == -1 || rootActor.rigPlayerId == -1));
					return rootActor2.rigPlayerId == rootActor.rigPlayerId || rootActor2.rigPlayerId == -1 || rootActor.rigPlayerId == -1;
				}
			}
			else if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.BodyAttachPoint)
			{
				if (!CrittersManager.instance.allowGrabbingEntireBag)
				{
					Debug.Log(string.Format("Grieffing - EntireBag {0} == {1} || {2} == -1 || {3} == -1  -  {4}", new object[]
					{
						crittersActor.rigPlayerId,
						rootActor.rigPlayerId,
						crittersActor.parentActorId,
						rootActor.rigPlayerId,
						crittersActor.rigPlayerId == rootActor.rigPlayerId || crittersActor.rigPlayerId == -1 || rootActor.rigPlayerId == -1
					}));
					return crittersActor.rigPlayerId == rootActor.rigPlayerId || crittersActor.rigPlayerId == -1 || rootActor.rigPlayerId == -1;
				}
			}
			else if (crittersActor.crittersActorType == CrittersActor.CrittersActorType.Grabber && !CrittersManager.instance.allowGrabbingOutOfHands)
			{
				Debug.Log(string.Format("Grieffing - InHand {0} == {1} || {2} == -1 || {3} == -1  -  {4}", new object[]
				{
					crittersActor.rigPlayerId,
					rootActor.rigPlayerId,
					crittersActor.parentActorId,
					rootActor.rigPlayerId,
					crittersActor.rigPlayerId == rootActor.rigPlayerId || crittersActor.rigPlayerId == -1 || rootActor.rigPlayerId == -1
				}));
				return crittersActor.rigPlayerId == rootActor.rigPlayerId || crittersActor.rigPlayerId == -1 || rootActor.rigPlayerId == -1;
			}
		}
		return true;
	}

	public bool IsCurrentlyAttachedToBag()
	{
		CrittersActor crittersActor;
		return CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor) && crittersActor.crittersActorType == CrittersActor.CrittersActorType.Bag;
	}

	public void SetTransformToDefaultParent(bool resetOrigin = false)
	{
		if (this.IsNull())
		{
			return;
		}
		base.transform.SetParent(this.defaultParentTransform, true);
		if (resetOrigin)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
	}

	public void SetDefaultParent(Transform newDefaultParent)
	{
		this.defaultParentTransform = newDefaultParent;
	}

	protected virtual void RemoteGrabbed(CrittersActor actor)
	{
		Action<CrittersActor> onGrabbedChild = this.OnGrabbedChild;
		if (onGrabbedChild != null)
		{
			onGrabbedChild(actor);
		}
		actor.RemoteGrabbedBy(this);
	}

	protected virtual void RemoteGrabbedBy(CrittersActor grabbingActor)
	{
		this.GlobalGrabbedBy(grabbingActor);
	}

	public virtual void GrabbedBy(CrittersActor grabbingActor, bool positionOverride = false, Quaternion localRotation = default(Quaternion), Vector3 localOffset = default(Vector3), bool disableGrabbing = false)
	{
		this.GlobalGrabbedBy(grabbingActor);
		if (this.parentActorId >= 0)
		{
			this.AttemptRemoveStoredObjectCollider(this.parentActorId, true);
		}
		this.isGrabDisabled = disableGrabbing;
		this.parentActorId = grabbingActor.actorId;
		if (grabbingActor.isOnPlayer)
		{
			this.lastGrabbedPlayer = grabbingActor.rigPlayerId;
		}
		base.transform.SetParent(grabbingActor.transform, true);
		if (localRotation.w == 0f && localRotation.x == 0f && localRotation.y == 0f && localRotation.z == 0f)
		{
			localRotation = Quaternion.identity;
		}
		if (positionOverride)
		{
			this.MoveActor(localOffset, localRotation, true, false, true);
		}
		this.UpdateImpulses(true, true);
		this.rb.isKinematic = true;
		this.rb.interpolation = RigidbodyInterpolation.None;
		this.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		if (CrittersManager.instance.IsNotNull() && PhotonNetwork.InRoom && !CrittersManager.instance.LocalAuthority())
		{
			CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby", CrittersManager.instance.guard.currentOwner, new object[] { this.actorId, grabbingActor.actorId, this.lastImpulseQuaternion, this.lastImpulsePosition, this.isGrabDisabled });
		}
		Action<CrittersActor> onGrabbedChild = grabbingActor.OnGrabbedChild;
		if (onGrabbedChild != null)
		{
			onGrabbedChild(this);
		}
		this.AttemptAddStoredObjectCollider(grabbingActor);
	}

	protected virtual void GlobalGrabbedBy(CrittersActor grabbingActor)
	{
	}

	protected virtual void HandleRemoteReleased()
	{
		this.DisconnectJoint();
	}

	public virtual void Released(bool keepWorldPosition, Quaternion rotation = default(Quaternion), Vector3 position = default(Vector3), Vector3 impulseVelocity = default(Vector3), Vector3 impulseAngularVelocity = default(Vector3))
	{
		if (this.parentActorId >= 0)
		{
			this.AttemptRemoveStoredObjectCollider(this.parentActorId, true);
		}
		this.isGrabDisabled = false;
		this.parentActorId = -1;
		if (this.equipmentStorable)
		{
			this.localCanStore = false;
		}
		this.DisconnectJoint();
		this.SetTransformToDefaultParent(false);
		if (rotation.w == 0f && rotation.x == 0f && rotation.y == 0f && rotation.z == 0f)
		{
			rotation = Quaternion.identity;
		}
		if (!keepWorldPosition)
		{
			if (position.sqrMagnitude > 1f)
			{
				this.MoveActor(position, rotation, false, false, true);
			}
			else
			{
				GTDev.Log<string>(string.Format("Release called for: {0}, but sent in suspicious position data: {1}", base.name, position), null);
			}
		}
		if (this.despawnWhenIdle)
		{
			if (this.preventDespawnUntilGrabbed)
			{
				this.isDespawnBlocked = false;
			}
			this.despawnTime = (double)this.despawnDelay + (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		}
		this.UpdateImpulses(false, false);
		this.SetImpulseVelocity(impulseVelocity, impulseAngularVelocity);
		this.TogglePhysics(this.usesRB);
		this.SetImpulse();
		if (CrittersManager.instance.IsNotNull() && PhotonNetwork.InRoom && !CrittersManager.instance.LocalAuthority())
		{
			CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, new object[] { this.actorId, false, rotation, position, impulseVelocity, impulseAngularVelocity });
		}
		this.ReleasedEvent.Invoke(this);
		this.ReleasedEvent.RemoveAllListeners();
	}

	public void PlacePlayerCrittersActor()
	{
		if (this.rigIndex == -1)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
			return;
		}
		RigContainer rigContainer;
		CrittersRigActorSetup crittersRigActorSetup;
		if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(this.rigPlayerId), out rigContainer) || !CrittersManager.instance.rigSetupByRig.TryGetValue(rigContainer.Rig, out crittersRigActorSetup))
		{
			rigContainer != null;
			return;
		}
		if (this.rigPlayerId == NetworkSystem.Instance.LocalPlayer.ActorNumber && !CrittersManager.instance.rigSetupByRig.TryGetValue(GorillaTagger.Instance.offlineVRRig, out crittersRigActorSetup))
		{
			return;
		}
		if (this.rigIndex < 0 || this.rigIndex >= crittersRigActorSetup.rigActors.Length)
		{
			return;
		}
		base.gameObject.SetActive(true);
		base.transform.parent = crittersRigActorSetup.rigActors[this.rigIndex].location;
		this.MoveActor(Vector3.zero, Quaternion.identity, true, true, true);
		crittersRigActorSetup.rigActors[this.rigIndex] = new CrittersRigActorSetup.RigActor
		{
			actorSet = this,
			location = crittersRigActorSetup.rigActors[this.rigIndex].location,
			type = crittersRigActorSetup.rigActors[this.rigIndex].type,
			subIndex = crittersRigActorSetup.rigActors[this.rigIndex].subIndex
		};
	}

	public void MoveActor(Vector3 position, Quaternion rotation, bool local = false, bool updateImpulses = true, bool updateImpulseTime = true)
	{
		bool isKinematic = this.rb.isKinematic;
		this.TogglePhysics(false);
		if (local)
		{
			base.transform.localRotation = rotation;
			base.transform.localPosition = position;
			if (updateImpulses)
			{
				this.UpdateImpulses(true, updateImpulseTime);
			}
		}
		else
		{
			base.transform.rotation = rotation.normalized;
			base.transform.position = position;
			if (updateImpulses)
			{
				this.UpdateImpulses(false, updateImpulseTime);
			}
		}
		if (!isKinematic)
		{
			this.TogglePhysics(true);
		}
	}

	public void UpdateImpulses(bool local = false, bool updateTime = false)
	{
		if (local)
		{
			this.lastImpulsePosition = base.transform.localPosition;
			this.lastImpulseQuaternion = base.transform.localRotation;
		}
		else
		{
			this.lastImpulsePosition = base.transform.position;
			this.lastImpulseQuaternion = base.transform.rotation;
		}
		if (updateTime)
		{
			this.SetImpulseTime();
		}
	}

	public void UpdateImpulseVelocity()
	{
		if (this.rb)
		{
			this.lastImpulseVelocity = this.rb.linearVelocity;
			this.lastImpulseAngularVelocity = this.rb.angularVelocity;
		}
	}

	public virtual void CalculateFear(CrittersPawn critter, float multiplier)
	{
		critter.IncreaseFear(this.FearCurve.Evaluate(Vector3.Distance(critter.transform.position, base.transform.position) / this.maxRangeOfFearAttraction) * multiplier * this.FearAmount * Time.deltaTime, this);
	}

	public virtual void CalculateAttraction(CrittersPawn critter, float multiplier)
	{
		critter.IncreaseAttraction(this.AttractionCurve.Evaluate(Vector3.Distance(critter.transform.position, base.transform.position) / this.maxRangeOfFearAttraction) * multiplier * this.AttractionAmount * Time.deltaTime, this);
	}

	public void SetImpulseVelocity(Vector3 velocity, Vector3 angularVelocity)
	{
		this.lastImpulseVelocity = velocity;
		this.lastImpulseAngularVelocity = angularVelocity;
	}

	public void SetImpulseTime()
	{
		this.lastImpulseTime = (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
	}

	public virtual bool ShouldDespawn()
	{
		return this.despawnWhenIdle && this.parentActorId == -1 && !this.isDespawnBlocked && 0.0 < this.despawnTime && this.despawnTime <= (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
	}

	public void RemoveDespawnBlock()
	{
		if (this.despawnWhenIdle)
		{
			this.isDespawnBlocked = false;
			this.despawnTime = (double)this.despawnDelay + (PhotonNetwork.InRoom ? PhotonNetwork.Time : ((double)Time.time));
		}
	}

	public virtual bool CheckStorable()
	{
		if (!this.localCanStore)
		{
			return false;
		}
		Vector3 vector = this.storeCollider.transform.up * MathF.Max(0f, this.storeCollider.height / 2f - this.storeCollider.radius);
		int num = Physics.OverlapCapsuleNonAlloc(this.storeCollider.transform.position + vector, this.storeCollider.transform.position - vector, this.storeCollider.radius, this.colliders, CrittersManager.instance.containerLayer, QueryTriggerInteraction.Collide);
		bool flag = false;
		CrittersBag crittersBag = null;
		bool flag2 = true;
		CrittersActor crittersActor = null;
		if (this.lastGrabbedPlayer == PhotonNetwork.LocalPlayer.ActorNumber && CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor) && crittersActor.GetAverageSpeed > CrittersManager.instance.MaxAttachSpeed)
		{
			return false;
		}
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				CrittersActor component = this.colliders[i].attachedRigidbody.GetComponent<CrittersActor>();
				if (!(component == null) && !(component == this))
				{
					CrittersBag crittersBag2 = component as CrittersBag;
					if (!(crittersBag2 == null))
					{
						if (crittersBag2 == this.lastStoredObject)
						{
							flag = true;
							flag2 = false;
							break;
						}
						if (crittersBag2.IsActorValidStore(this))
						{
							if (crittersBag2.attachableCollider != this.colliders[i] && !this.colliders[i].isTrigger)
							{
								Vector3 vector2;
								float num2;
								Physics.ComputePenetration(this.colliders[i], this.colliders[i].transform.position, this.colliders[i].transform.rotation, this.storeCollider, this.storeCollider.transform.position, this.storeCollider.transform.rotation, out vector2, out num2);
								if (num2 >= CrittersManager.instance.overlapDistanceMax)
								{
									flag2 = false;
									break;
								}
							}
							else
							{
								crittersBag = crittersBag2;
							}
						}
					}
				}
			}
		}
		if (crittersBag.IsNotNull() && flag2)
		{
			if (crittersActor.IsNotNull())
			{
				CrittersGrabber crittersGrabber = crittersActor as CrittersGrabber;
				if (crittersGrabber.IsNotNull())
				{
					GorillaTagger.Instance.StartVibration(crittersGrabber.isLeft, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
				}
			}
			this.GrabbedBy(crittersBag, false, default(Quaternion), default(Vector3), false);
			this.localCanStore = false;
			this.lastStoredObject = crittersBag;
			this.DisconnectJoint();
			return true;
		}
		if (!flag)
		{
			this.lastStoredObject = null;
		}
		return false;
	}

	public void SetJointRigid(Rigidbody rbToConnect)
	{
		if (this.joint != null)
		{
			return;
		}
		string text = "Critters SetJointRigid ";
		GameObject gameObject = base.gameObject;
		Debug.Log(text + ((gameObject != null) ? gameObject.ToString() : null));
		this.CreateJoint(rbToConnect, false);
		this.joint.xMotion = ConfigurableJointMotion.Locked;
		this.joint.yMotion = ConfigurableJointMotion.Locked;
		this.joint.zMotion = ConfigurableJointMotion.Locked;
		this.joint.angularXMotion = ConfigurableJointMotion.Locked;
		this.joint.angularYMotion = ConfigurableJointMotion.Locked;
		this.joint.angularZMotion = ConfigurableJointMotion.Locked;
		this.rb.mass = CrittersManager.instance.heavyMass;
		this.TogglePhysics(true);
	}

	public void SetJointSoft(Rigidbody rbToConnect)
	{
		if (this.joint != null)
		{
			return;
		}
		string text = "Critters SetJointSoft ";
		GameObject gameObject = base.gameObject;
		Debug.Log(text + ((gameObject != null) ? gameObject.ToString() : null));
		this.CreateJoint(rbToConnect, true);
		this.joint.xMotion = ConfigurableJointMotion.Limited;
		this.joint.yMotion = ConfigurableJointMotion.Limited;
		this.joint.zMotion = ConfigurableJointMotion.Limited;
		this.joint.angularXMotion = ConfigurableJointMotion.Limited;
		this.joint.angularYMotion = ConfigurableJointMotion.Limited;
		this.joint.angularZMotion = ConfigurableJointMotion.Limited;
		this.rb.mass = CrittersManager.instance.lightMass;
		this.TogglePhysics(true);
	}

	private void CreateJoint(Rigidbody rbToConnect, bool setParentNull = true)
	{
		if (this.joint != null)
		{
			return;
		}
		this.joint = base.gameObject.AddComponent<ConfigurableJoint>();
		this.drive = new JointDrive
		{
			positionSpring = CrittersManager.instance.springForce,
			positionDamper = CrittersManager.instance.damperForce,
			maximumForce = 10000f
		};
		this.angularDrive = new JointDrive
		{
			positionSpring = CrittersManager.instance.springAngularForce,
			positionDamper = CrittersManager.instance.damperAngularForce,
			maximumForce = 10000f
		};
		this.linearLimitDrive = new SoftJointLimit
		{
			limit = CrittersManager.instance.springForce
		};
		this.linearLimitSpringDrive = new SoftJointLimitSpring
		{
			spring = CrittersManager.instance.springForce
		};
		this.joint.linearLimit = this.linearLimitDrive;
		this.joint.linearLimitSpring = this.linearLimitSpringDrive;
		this.joint.angularYLimit = this.joint.linearLimit;
		this.joint.angularZLimit = this.joint.linearLimit;
		this.joint.angularXDrive = this.angularDrive;
		this.joint.angularYZDrive = this.angularDrive;
		this.joint.xDrive = this.drive;
		this.joint.yDrive = this.drive;
		this.joint.zDrive = this.drive;
		this.joint.autoConfigureConnectedAnchor = true;
		this.joint.enableCollision = false;
		this.joint.connectedBody = rbToConnect;
		this.rb.excludeLayers = CrittersManager.instance.movementLayers;
		this.rb.useGravity = false;
		if (setParentNull)
		{
			base.transform.SetParent(null, true);
		}
	}

	public void DisconnectJoint()
	{
		this.rb.excludeLayers = CrittersManager.instance.containerLayer;
		this.rb.useGravity = true;
		if (this.joint != null)
		{
			Object.Destroy(this.joint);
		}
		this.joint = null;
		if (this.parentActorId != -1)
		{
			CrittersActor crittersActor;
			CrittersManager.instance.actorById.TryGetValue(this.parentActorId, out crittersActor);
			base.transform.SetParent(crittersActor.transform, true);
			this.MoveActor(this.lastImpulsePosition, this.lastImpulseQuaternion, true, false, true);
			this.TogglePhysics(false);
		}
	}

	public void AttemptRemoveStoredObjectCollider(int oldParentId, bool playSound = true)
	{
		CrittersActor crittersActor;
		if (CrittersManager.instance.actorById.TryGetValue(oldParentId, out crittersActor) && crittersActor is CrittersBag)
		{
			((CrittersBag)crittersActor).RemoveStoredObjectCollider(this, playSound);
		}
	}

	public void AttemptAddStoredObjectCollider(CrittersActor actor)
	{
		if (actor is CrittersBag)
		{
			((CrittersBag)actor).AddStoredObjectCollider(this);
		}
	}

	public bool AttemptSetEquipmentStorable()
	{
		if (!this.equipmentStorable)
		{
			return false;
		}
		this.localCanStore = true;
		return true;
	}

	public CrittersActor.CrittersActorType crittersActorType;

	public bool isSceneActor;

	public bool isOnPlayer;

	[NonSerialized]
	protected bool _isOnPlayerDefault;

	public int rigPlayerId;

	public int rigIndex;

	public bool grabbable;

	protected bool isGrabDisabled;

	public int lastGrabbedPlayer;

	public UnityEvent<CrittersActor> ReleasedEvent;

	public Rigidbody rb;

	[NonSerialized]
	public int actorId;

	[NonSerialized]
	protected Transform defaultParentTransform;

	[NonSerialized]
	public int parentActorId = -1;

	[NonSerialized]
	protected int lastParentActorId;

	[NonSerialized]
	public Vector3 lastImpulsePosition;

	[NonSerialized]
	public Vector3 lastImpulseVelocity;

	[NonSerialized]
	public Vector3 lastImpulseAngularVelocity;

	[NonSerialized]
	public Quaternion lastImpulseQuaternion;

	[NonSerialized]
	public double lastImpulseTime;

	[NonSerialized]
	public bool updatedSinceLastFrame;

	public bool isEnabled = true;

	public bool wasEnabled = true;

	[NonSerialized]
	protected double localLastImpulse;

	[NonSerialized]
	protected Transform parentActor;

	public GameObject[] subObjects;

	public int subObjectIndex = -1;

	public bool usesRB;

	public bool resetPhysicsOnSpawn;

	public bool despawnWhenIdle;

	public bool preventDespawnUntilGrabbed;

	public int despawnDelay;

	public double despawnTime;

	public bool isDespawnBlocked;

	public bool equipmentStorable;

	public bool localCanStore;

	public CrittersActor lastStoredObject;

	public CapsuleCollider storeCollider;

	[NonSerialized]
	public Collider[] colliders;

	[NonSerialized]
	public ConfigurableJoint joint;

	[NonSerialized]
	public float timeLastTouched;

	private JointDrive drive;

	private JointDrive angularDrive;

	private SoftJointLimit linearLimitDrive;

	private SoftJointLimitSpring linearLimitSpringDrive;

	public CapsuleCollider equipmentStoreTriggerCollider;

	public bool disconnectJointFlag;

	public bool forceUpdate;

	public float FearAmount = 1f;

	public AnimationCurve FearCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public float AttractionAmount = 1f;

	public AnimationCurve AttractionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[FormerlySerializedAs("maxDetectionDistance")]
	public float maxRangeOfFearAttraction = 3f;

	protected float[] averageSpeed = new float[6];

	protected int averageSpeedIndex;

	private Vector3 lastPosition = Vector3.zero;

	public enum CrittersActorType
	{
		Creature,
		Food,
		LoudNoise,
		BrightLight,
		Darkness,
		HidingArea,
		Disappear,
		Spawn,
		Player,
		Grabber,
		Cage,
		FoodSpawner,
		AttachPoint,
		StunBomb,
		Bag,
		BodyAttachPoint,
		NoiseMaker,
		StickyTrap,
		StickyGoo
	}
}
