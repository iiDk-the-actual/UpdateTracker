using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(GameEntity))]
public abstract class SIGadget : MonoBehaviour, IGameEntityComponent, IPrefabRequirements, IGameActivatable
{
	public SITechTreePageId PageId
	{
		get
		{
			return this.pageId;
		}
		set
		{
			this.pageId = value;
		}
	}

	public IEnumerable<GameEntity> RequiredPrefabs
	{
		get
		{
			return this.additionalRequiredPrefabs;
		}
	}

	protected virtual void Update()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (this.IsEquippedLocal() || this.activatedLocally)
		{
			this.OnUpdateAuthority(deltaTime);
			return;
		}
		this.OnUpdateRemote(deltaTime);
	}

	protected virtual void OnUpdateAuthority(float dt)
	{
		this.SleepAfterDelay();
	}

	protected virtual void OnUpdateRemote(float dt)
	{
		this.SleepAfterDelay();
	}

	protected virtual bool IsEquippedLocal()
	{
		return this.gameEntity.IsHeldByLocalPlayer() || this.gameEntity.IsSnappedByLocalPlayer();
	}

	protected Vector2 GetJoystickInput()
	{
		if (!this.ShouldProcessInput())
		{
			return default(Vector2);
		}
		return ControllerInputPoller.Primary2DAxis((this.gameEntity.heldByHandIndex == 0 || this.gameEntity.snappedJoint == SnapJointType.ArmL) ? XRNode.LeftHand : XRNode.RightHand);
	}

	protected bool ShouldProcessInput()
	{
		if (this.gameEntity.IsHeldByLocalPlayer())
		{
			return true;
		}
		GamePlayer gamePlayer;
		if (this.gameEntity.IsSnappedByLocalPlayer() && GamePlayer.TryGetGamePlayer(this.gameEntity.snappedByActorNumber, out gamePlayer))
		{
			SnapJointType snappedJoint = this.gameEntity.snappedJoint;
			GameEntity gameEntity;
			if (snappedJoint != SnapJointType.ArmL)
			{
				if (snappedJoint != SnapJointType.ArmR)
				{
					gameEntity = null;
				}
				else
				{
					gameEntity = gamePlayer.GetGrabbedGameEntity(1);
				}
			}
			else
			{
				gameEntity = gamePlayer.GetGrabbedGameEntity(0);
			}
			GameEntity gameEntity2 = gameEntity;
			return !gameEntity2 || gameEntity2.GetComponent<IGameActivatable>() == null;
		}
		return false;
	}

	public void SleepAfterDelay()
	{
		if (this.isSleeping || !this.shouldSleep)
		{
			return;
		}
		if (Time.time < this.timeReleased + this.sleepTime)
		{
			return;
		}
		base.GetComponent<Rigidbody>().isKinematic = true;
		this.isSleeping = true;
	}

	public virtual SIUpgradeSet FilterUpgradeNodes(SIUpgradeSet upgrades)
	{
		return upgrades;
	}

	public virtual void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
	}

	public virtual void RefreshUpgradeVisuals(SIUpgradeSet withUpgrades)
	{
		foreach (SIGadget.UpgradeVisual upgradeVisual in this.UpgradeBasedVisuals)
		{
			upgradeVisual.Update(withUpgrades);
		}
		Action<SIUpgradeSet> onPostRefreshVisuals = this.OnPostRefreshVisuals;
		if (onPostRefreshVisuals == null)
		{
			return;
		}
		onPostRefreshVisuals(withUpgrades);
	}

	protected virtual void OnEnable()
	{
		if (!this.didApplyId)
		{
			GameObject gameObject = base.gameObject;
			gameObject.name = gameObject.name + "[" + SIGadget.uniqueId.ToString() + "]";
			this.didApplyId = true;
			SIGadget.uniqueId++;
		}
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnSnapped = (Action)Delegate.Combine(gameEntity.OnSnapped, new Action(this.GrabInitialization));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnGrabbed = (Action)Delegate.Combine(gameEntity2.OnGrabbed, new Action(this.GrabInitialization));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.ReleaseInitialization));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.ReleaseInitialization));
		this.timeReleased = Time.time;
	}

	protected virtual void OnDisable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnSnapped = (Action)Delegate.Remove(gameEntity.OnSnapped, new Action(this.GrabInitialization));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnGrabbed = (Action)Delegate.Remove(gameEntity2.OnGrabbed, new Action(this.GrabInitialization));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Remove(gameEntity3.OnReleased, new Action(this.ReleaseInitialization));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Remove(gameEntity4.OnUnsnapped, new Action(this.ReleaseInitialization));
		this.LeaveAllExclusionZones();
	}

	public void GrabInitialization()
	{
		this.isSleeping = false;
		this.shouldSleep = false;
		if (!this.gameEntity.IsHeldByLocalPlayer())
		{
			return;
		}
		SuperInfectionManager component = this.gameEntity.manager.GetComponent<SuperInfectionManager>();
		if (((component != null) ? component.zoneSuperInfection : null) == null)
		{
			return;
		}
		bool flag = SIPlayer.LocalPlayer.activePlayerGadgets.Contains(this.gameEntity.GetNetId());
		SIProgression.Instance.UpdateHeldGadgetsTelemetry(this.PageId, flag, 1);
	}

	public void ReleaseInitialization()
	{
		this.shouldSleep = true;
		this.isSleeping = false;
		this.timeReleased = Time.time;
		if (!this.gameEntity.WasLastHeldByLocalPlayer())
		{
			return;
		}
		SuperInfectionManager component = this.gameEntity.manager.GetComponent<SuperInfectionManager>();
		if (((component != null) ? component.zoneSuperInfection : null) == null)
		{
			return;
		}
		bool flag = SIPlayer.LocalPlayer.activePlayerGadgets.Contains(this.gameEntity.GetNetId());
		SIProgression.Instance.UpdateHeldGadgetsTelemetry(this.PageId, flag, -1);
	}

	public bool FindAttachedHand(out bool isLeft, bool checkHeld = true, bool checkSnapped = true)
	{
		isLeft = false;
		int num = -1;
		GamePlayer gamePlayer;
		if (checkHeld && GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			num = gamePlayer.FindHandIndex(this.gameEntity.id);
		}
		GamePlayer gamePlayer2;
		if (num == -1 && checkSnapped && GamePlayer.TryGetGamePlayer(this.gameEntity.snappedByActorNumber, out gamePlayer2))
		{
			num = gamePlayer2.FindSnapIndex(this.gameEntity.id);
		}
		if (num == -1)
		{
			return false;
		}
		isLeft = GamePlayer.IsLeftHand(num);
		return true;
	}

	public int GetAttachedPlayerActorNumber()
	{
		if (this.gameEntity.heldByActorNumber == -1)
		{
			return this.gameEntity.snappedByActorNumber;
		}
		return this.gameEntity.heldByActorNumber;
	}

	public virtual void OnEntityInit()
	{
	}

	public virtual void OnEntityDestroy()
	{
	}

	public virtual void OnEntityStateChange(long prevState, long newState)
	{
	}

	public virtual void ProcessClientToAuthorityRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
	}

	public virtual void ProcessAuthorityToClientRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
	}

	public virtual void ProcessClientToClientRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
	}

	public void SendClientToAuthorityRPC(int rpcID)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.ClientToAuthorityRPC.CallEntityRPC, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID
			});
		}
	}

	public void SendClientToAuthorityRPC(int rpcID, object[] data)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.ClientToAuthorityRPC.CallEntityRPCData, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID,
				data
			});
		}
	}

	public void SendAuthorityToClientRPC(int rpcID)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.AuthorityToClientRPC.CallEntityRPC, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID
			});
		}
	}

	public void SendAuthorityToClientRPC(int rpcID, object[] data)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.AuthorityToClientRPC.CallEntityRPCData, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID,
				data
			});
		}
	}

	public void SendClientToClientRPC(int rpcID)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.ClientToClientRPC.CallEntityRPC, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID
			});
		}
	}

	public void SendClientToClientRPC(int rpcID, object[] data)
	{
		SuperInfectionManager simanagerForZone = SuperInfectionManager.GetSIManagerForZone(this.gameEntity.manager.zone);
		if (simanagerForZone != null)
		{
			simanagerForZone.CallRPC(SuperInfectionManager.ClientToClientRPC.CallEntityRPCData, new object[]
			{
				this.gameEntity.GetNetId(),
				rpcID,
				data
			});
		}
	}

	public void ApplyExclusionZone(SIExclusionZone exclusionZone)
	{
		if (!this.appliedExclusionZones.Contains(exclusionZone))
		{
			if (this.appliedExclusionZones.Count == 0)
			{
				this.appliedExclusionZones.Add(exclusionZone);
				this.HandleBlockedActionChanged(true);
				return;
			}
			this.appliedExclusionZones.Add(exclusionZone);
		}
	}

	public void LeaveExclusionZone(SIExclusionZone exclusionZone)
	{
		if (this.appliedExclusionZones.Contains(exclusionZone))
		{
			this.appliedExclusionZones.Remove(exclusionZone);
			if (this.appliedExclusionZones.Count == 0)
			{
				this.HandleBlockedActionChanged(false);
			}
		}
	}

	private void LeaveAllExclusionZones()
	{
		foreach (SIExclusionZone siexclusionZone in this.appliedExclusionZones)
		{
			if (siexclusionZone != null)
			{
				siexclusionZone.ClearGadget(this);
			}
		}
		this.appliedExclusionZones.Clear();
	}

	protected bool IsBlocked()
	{
		return this.appliedExclusionZones.Count > 0;
	}

	protected virtual void HandleBlockedActionChanged(bool isBlocked)
	{
	}

	public GameEntity gameEntity;

	[Tooltip("Add additional required prefabs here.  These will be automatically added to the GameEntityManager factory.")]
	public GameEntity[] additionalRequiredPrefabs;

	public float sleepTime = 10f;

	private bool shouldSleep = true;

	private bool isSleeping;

	private float timeReleased;

	protected bool activatedLocally;

	[SerializeField]
	private SITechTreePageId pageId;

	public Action<SIUpgradeSet> OnPostRefreshVisuals;

	private static int uniqueId = 101;

	private bool didApplyId;

	[SerializeField]
	private SIGadget.UpgradeVisual[] UpgradeBasedVisuals;

	private readonly List<SIExclusionZone> appliedExclusionZones = new List<SIExclusionZone>();

	[Serializable]
	private struct UpgradeVisual
	{
		public void Update(SIUpgradeSet withUpgrades)
		{
			bool flag = true;
			if (this.appearRequirements.Length != 0)
			{
				flag = false;
				foreach (SIUpgradeType siupgradeType in this.appearRequirements)
				{
					if (withUpgrades.Contains(siupgradeType))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				foreach (SIUpgradeType siupgradeType2 in this.disappearRequirements)
				{
					if (withUpgrades.Contains(siupgradeType2))
					{
						flag = false;
						break;
					}
				}
			}
			GameObject[] array2 = this.objects;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetActive(flag);
			}
		}

		public GameObject[] objects;

		[Tooltip("For the objects to become activated, you must match AT LEAST ONE appearRequirement (if there are any), and not match any disappearRequirements.")]
		public SIUpgradeType[] appearRequirements;

		[Tooltip("For the objects to become deactivated, you must match AT LEAST ONE disappearRequirement (if there are any).")]
		public SIUpgradeType[] disappearRequirements;
	}
}
