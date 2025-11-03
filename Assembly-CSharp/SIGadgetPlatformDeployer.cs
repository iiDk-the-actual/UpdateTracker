using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(GameGrabbable))]
[RequireComponent(typeof(GameSnappable))]
[RequireComponent(typeof(GameButtonActivatable))]
public class SIGadgetPlatformDeployer : SIGadget, I_SIDisruptable
{
	private void Start()
	{
		this.previewPlatform.SetActive(false);
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnReleased = (Action)Delegate.Combine(gameEntity.OnReleased, new Action(this.HandleStopInteraction));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnUnsnapped = (Action)Delegate.Combine(gameEntity2.OnUnsnapped, new Action(this.HandleStopInteraction));
	}

	private void OnDestroy()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnReleased = (Action)Delegate.Remove(gameEntity.OnReleased, new Action(this.HandleStopInteraction));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnUnsnapped = (Action)Delegate.Remove(gameEntity2.OnUnsnapped, new Action(this.HandleStopInteraction));
	}

	private void HandleStopInteraction()
	{
		this.SetState(SIGadgetPlatformDeployer.State.Idle);
	}

	protected override void Update()
	{
		base.Update();
		if (this.remainingRechargeTime > 0f)
		{
			int num = Mathf.CeilToInt(this.remainingRechargeTime / this.chargeRecoveryTime);
			this.remainingRechargeTime = Mathf.Max(this.remainingRechargeTime - Time.deltaTime, 0f);
			int num2 = Mathf.CeilToInt(this.remainingRechargeTime / this.chargeRecoveryTime);
			this.chargeDisplay.UpdateDisplay(this.maxCharges - num2);
			if (num2 != num && this.IsEquippedLocal())
			{
				this.rechargeSFX.Play();
				bool flag;
				if (base.FindAttachedHand(out flag, true, true))
				{
					GorillaTagger.Instance.StartVibration(flag, GorillaTagger.Instance.tapHapticStrength * 0.5f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
				}
			}
		}
	}

	protected override bool IsEquippedLocal()
	{
		return (this.canActivateWhileHeld && this.gameEntity.IsHeldByLocalPlayer()) || this.gameEntity.IsSnappedByLocalPlayer();
	}

	protected override void OnUpdateAuthority(float dt)
	{
		SIGadgetPlatformDeployer.State state = this.state;
		if (state != SIGadgetPlatformDeployer.State.Idle)
		{
			if (state != SIGadgetPlatformDeployer.State.Deploying)
			{
				return;
			}
			if (this.CheckReleaseInputs())
			{
				if (this.IsChargeAvailable())
				{
					this.TryDeployPlatform();
				}
				this.SetStateAuthority(SIGadgetPlatformDeployer.State.Idle);
				return;
			}
			this.UpdatePreview();
			return;
		}
		else
		{
			if (this.CheckInitInputs())
			{
				if (this.IsChargeAvailable())
				{
					if (this.isInstancePlace)
					{
						if (!this.wasInputPressed)
						{
							this.TryDeployInstantPlatform();
						}
					}
					else
					{
						this.SetStateAuthority(SIGadgetPlatformDeployer.State.Deploying);
					}
				}
				this.wasInputPressed = true;
				return;
			}
			this.wasInputPressed = false;
			return;
		}
	}

	protected override void OnUpdateRemote(float dt)
	{
		SIGadgetPlatformDeployer.State state = (SIGadgetPlatformDeployer.State)this.gameEntity.GetState();
		if (state != this.state)
		{
			this.SetState(state);
		}
		SIGadgetPlatformDeployer.State state2 = this.state;
		if (state2 != SIGadgetPlatformDeployer.State.Idle && state2 == SIGadgetPlatformDeployer.State.Deploying)
		{
			this.UpdatePreview();
		}
	}

	private bool CheckInitInputs()
	{
		if (!this.buttonActivatable.CheckInput(this.canActivateWhileHeld, true, this.inputSensitivity, true))
		{
			return false;
		}
		if (this.isInstancePlace)
		{
			return true;
		}
		GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
		Vector3 position = gamePlayer.leftHand.position;
		Vector3 position2 = gamePlayer.rightHand.position;
		return Vector3.Distance(position, position2) <= this.activationHandDistance;
	}

	private bool CheckReleaseInputs()
	{
		return !this.buttonActivatable.CheckInput(this.canActivateWhileHeld, true, this.inputSensitivity, true);
	}

	private bool IsChargeAvailable()
	{
		return (float)this.maxCharges * this.chargeRecoveryTime - this.remainingRechargeTime > this.chargeRecoveryTime;
	}

	private void SpendCharge()
	{
		this.remainingRechargeTime += this.chargeRecoveryTime;
	}

	private void TryDeployInstantPlatform()
	{
		if (base.IsBlocked())
		{
			this.blockedSFX.Play();
			return;
		}
		GamePlayer gamePlayer;
		if (!this.TryGetGamePlayer(out gamePlayer))
		{
			return;
		}
		int num = gamePlayer.FindSnapIndex(this.gameEntity.id);
		if (num == -1 && this.canActivateWhileHeld)
		{
			num = gamePlayer.FindHandIndex(this.gameEntity.id);
		}
		if (num == -1)
		{
			return;
		}
		Vector3 vector;
		Quaternion quaternion;
		if (this.gameEntity.IsHeldByLocalPlayer())
		{
			vector = base.transform.position - base.transform.up * this.handDepthOffset;
			quaternion = base.transform.rotation;
			Debug.DrawRay(base.transform.position, -base.transform.up * 0.3f, Color.blue, 10f);
			Debug.DrawRay(base.transform.position, base.transform.forward * 0.3f, Color.blue, 10f);
			Debug.DrawRay(vector, quaternion * Vector3.forward * 0.3f, Color.green, 10f);
		}
		else
		{
			Transform transform = (GamePlayer.IsLeftHand(num) ? gamePlayer.leftHand : gamePlayer.rightHand);
			vector = transform.position;
			Vector3 up = transform.up;
			Vector3 right = transform.right;
			Debug.DrawRay(vector, right * 0.3f, Color.red, 10f);
			Debug.DrawRay(vector, up * 0.3f, Color.red, 10f);
			quaternion = Quaternion.LookRotation(up, right);
			vector += right * this.handDepthOffset;
			Debug.DrawRay(vector, quaternion * Vector3.forward * 0.3f, Color.green, 10f);
		}
		this.DeployPlatform(vector, quaternion);
	}

	private void TryDeployPlatform()
	{
		GamePlayer gamePlayer = GamePlayerLocal.instance.gamePlayer;
		Vector3 position = gamePlayer.leftHand.position;
		Vector3 position2 = gamePlayer.rightHand.position;
		if (Vector3.Distance(position, position2) > this.deployMinRequiredHandDistance)
		{
			if (base.IsBlocked())
			{
				this.blockedSFX.Play();
				return;
			}
			Vector3 vector;
			Quaternion quaternion;
			Vector3 vector2;
			if (this.TryGetPlatformPosRotScale(out vector, out quaternion, out vector2))
			{
				this.DeployPlatform(vector, quaternion);
				return;
			}
		}
	}

	private void DeployPlatform(Vector3 pos, Quaternion rot)
	{
		this.SpendCharge();
		this.CreateLocalPlatformInstance(pos, rot);
		int actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
		if (this.gameEntity.IsAuthority())
		{
			base.SendAuthorityToClientRPC(0, new object[] { actorNumber, pos, rot });
			return;
		}
		base.SendClientToAuthorityRPC(0, new object[] { actorNumber, pos, rot });
	}

	public override void ProcessClientToAuthorityRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
		if (rpcID == 0)
		{
			if (data == null || data.Length != 3)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			Vector3 vector;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[1], out vector))
			{
				return;
			}
			Quaternion quaternion;
			if (!GameEntityManager.ValidateDataType<Quaternion>(data[2], out quaternion))
			{
				return;
			}
			if (!this.gameEntity.IsAttachedToPlayer(NetPlayer.Get(info.Sender)))
			{
				return;
			}
			if (Vector3.Distance(base.transform.position, vector) > 2f)
			{
				return;
			}
			this.CreateLocalPlatformInstance(vector, quaternion);
			base.SendAuthorityToClientRPC(0, data);
		}
	}

	public override void ProcessAuthorityToClientRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
		if (rpcID == 0)
		{
			if (data == null || data.Length != 3)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			Vector3 vector;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[1], out vector))
			{
				return;
			}
			Quaternion quaternion;
			if (!GameEntityManager.ValidateDataType<Quaternion>(data[2], out quaternion))
			{
				return;
			}
			if (num != NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				this.CreateLocalPlatformInstance(vector, quaternion);
			}
		}
	}

	private void CreateLocalPlatformInstance(Vector3 pos, Quaternion rot)
	{
		if (this.deployedPlatformCount >= this.maxCharges)
		{
			return;
		}
		GameObject gameObject = ObjectPools.instance.Instantiate(this.platformPrefab, true);
		if (gameObject != null)
		{
			SIGadgetPlatformDeployerPlatform component = gameObject.GetComponent<SIGadgetPlatformDeployerPlatform>();
			if (component != null)
			{
				this.deployedPlatformCount++;
				SIGadgetPlatformDeployerPlatform sigadgetPlatformDeployerPlatform = component;
				sigadgetPlatformDeployerPlatform.OnDisabled = (Action)Delegate.Combine(sigadgetPlatformDeployerPlatform.OnDisabled, new Action(delegate
				{
					this.deployedPlatformCount--;
				}));
			}
			gameObject.transform.SetPositionAndRotation(pos, rot);
			ISIGameDeployable isigameDeployable;
			if (gameObject.TryGetComponent<ISIGameDeployable>(out isigameDeployable))
			{
				isigameDeployable.ApplyUpgrades(this.instanceUpgrades);
			}
		}
	}

	private void SetStateAuthority(SIGadgetPlatformDeployer.State newState)
	{
		this.SetState(newState);
		this.gameEntity.RequestState(this.gameEntity.id, (long)newState);
	}

	private void SetState(SIGadgetPlatformDeployer.State newState)
	{
		if (newState == this.state || !this.CanChangeState((long)newState))
		{
			return;
		}
		this.state = newState;
		SIGadgetPlatformDeployer.State state = this.state;
		if (state == SIGadgetPlatformDeployer.State.Idle)
		{
			this.SetPreviewVisibility(false);
			return;
		}
		if (state != SIGadgetPlatformDeployer.State.Deploying)
		{
			return;
		}
		this.SetPreviewVisibility(true);
	}

	public bool CanChangeState(long newStateIndex)
	{
		return newStateIndex >= 0L && newStateIndex < 2L;
	}

	private void SetPreviewVisibility(bool enabled)
	{
		this.previewPlatform.SetActive(enabled);
		if (enabled)
		{
			this.UpdatePreview();
		}
	}

	private void UpdatePreview()
	{
		Vector3 vector;
		Quaternion quaternion;
		Vector3 vector2;
		if (this.TryGetPlatformPosRotScale(out vector, out quaternion, out vector2))
		{
			this.previewPlatform.transform.SetPositionAndRotation(vector, quaternion);
			this.previewPlatform.transform.localScale = vector2;
			GamePlayer gamePlayer;
			if (this.TryGetGamePlayer(out gamePlayer))
			{
				Vector3 position = gamePlayer.leftHand.position;
				Vector3 position2 = gamePlayer.rightHand.position;
				if (Vector3.Distance(position, position2) > this.deployMinRequiredHandDistance)
				{
					this.previewMesh.material = this.validPreviewMaterial;
					return;
				}
				this.previewMesh.material = this.invalidPreviewMaterial;
			}
		}
	}

	private bool TryGetPlatformPosRotScale(out Vector3 pos, out Quaternion rot, out Vector3 scale)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		scale = Vector3.one;
		GamePlayer gamePlayer;
		if (this.TryGetGamePlayer(out gamePlayer))
		{
			Vector3 position = gamePlayer.leftHand.position;
			Vector3 position2 = gamePlayer.rightHand.position;
			Vector3 position3 = gamePlayer.rig.head.rigTarget.position;
			Vector3 vector = (position + position2) / 2f;
			Vector3 normalized = (position3 - vector).normalized;
			Vector3 vector2 = Vector3.ProjectOnPlane((position - position2).normalized, normalized);
			pos = vector + -normalized * this.handDepthOffset;
			rot = Quaternion.LookRotation(vector2, normalized);
			return true;
		}
		return false;
	}

	private bool TryGetGamePlayer(out GamePlayer player)
	{
		player = null;
		return GamePlayer.TryGetGamePlayer(this.gameEntity.snappedByActorNumber, out player) || (this.canActivateWhileHeld && GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out player));
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
		this.instanceUpgrades = withUpgrades;
		bool flag = withUpgrades.Contains(SIUpgradeType.Platform_Capacity);
		this.maxCharges = (flag ? this.maxChargesHighCapacity : this.maxChargesDefault);
		this.chargeDisplay = (flag ? this.chargeDisplayHighCapacity : this.chargeDisplayDefault);
		this.chargeRecoveryTime = (withUpgrades.Contains(SIUpgradeType.Platform_Cooldown) ? this.chargeRecoveryTimeFast : this.chargeRecoveryTimeDefault);
	}

	public void Disrupt(float disruptTime)
	{
		this.remainingRechargeTime = (float)this.maxCharges * this.chargeRecoveryTime + disruptTime;
	}

	protected override void HandleBlockedActionChanged(bool isBlocked)
	{
		this.blockedDisplayMesh.material = (isBlocked ? this.blockedMat : this.unblockedMat);
	}

	[SerializeField]
	private GameButtonActivatable buttonActivatable;

	[SerializeField]
	private SoundBankPlayer rechargeSFX;

	[SerializeField]
	private SoundBankPlayer blockedSFX;

	[SerializeField]
	private MeshRenderer blockedDisplayMesh;

	[SerializeField]
	private Material unblockedMat;

	[SerializeField]
	private Material blockedMat;

	[SerializeField]
	private GameObject platformPrefab;

	[Header("Activation")]
	[SerializeField]
	private bool canActivateWhileHeld = true;

	[SerializeField]
	private bool isInstancePlace;

	[SerializeField]
	private float activationHandDistance = 0.2f;

	[SerializeField]
	private float inputSensitivity = 0.25f;

	[Header("Deploy")]
	[SerializeField]
	private float deployMinRequiredHandDistance = 0.2f;

	[SerializeField]
	private GameObject previewPlatform;

	[SerializeField]
	private float handInset = 0.1f;

	[SerializeField]
	private float handDepthOffset = 0.3f;

	[SerializeField]
	private MeshRenderer previewMesh;

	[SerializeField]
	private Material validPreviewMaterial;

	[SerializeField]
	private Material invalidPreviewMaterial;

	[Header("Charges")]
	private int maxCharges = 3;

	private float chargeRecoveryTime = 10f;

	private SIChargeDisplay chargeDisplay;

	[SerializeField]
	private int maxChargesDefault = 3;

	[SerializeField]
	private int maxChargesHighCapacity = 5;

	[SerializeField]
	private SIChargeDisplay chargeDisplayDefault;

	[SerializeField]
	private SIChargeDisplay chargeDisplayHighCapacity;

	[SerializeField]
	private float chargeRecoveryTimeDefault = 10f;

	[SerializeField]
	private float chargeRecoveryTimeFast = 5f;

	private SIGadgetPlatformDeployer.State state;

	private bool wasInputPressed;

	private float remainingRechargeTime;

	private SIUpgradeSet instanceUpgrades;

	private const float MAX_DEPLOY_DIST = 2f;

	private int deployedPlatformCount;

	private enum State
	{
		Idle,
		Deploying,
		Count
	}
}
