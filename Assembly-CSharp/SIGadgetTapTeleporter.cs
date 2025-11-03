using System;
using Photon.Pun;
using UnityEngine;

public class SIGadgetTapTeleporter : SIGadget
{
	public Color identifierColor { get; private set; }

	public bool useStealthTeleporters { get; private set; }

	public bool isVelocityPreserved { get; private set; }

	public bool hasInfiniteDuration { get; private set; }

	public override void OnEntityInit()
	{
		this.gameEntity.OnStateChanged += this.HandleStateChanged;
		this.gameEntity.onEntityDestroyed += this.HandleOnDestroyed;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.HandleHandAttached));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnSnapped = (Action)Delegate.Combine(gameEntity2.OnSnapped, new Action(this.HandleHandAttached));
		GameEntity gameEntity3 = this.gameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.HandleHandDetach));
		GameEntity gameEntity4 = this.gameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.HandleHandDetach));
		this.identifierColor = this.GenerateColor(this.gameEntity.GetNetId());
		this.ApplyIdentifierColor();
		this.UpdateNextSelectionDisplay();
	}

	private void HandleOnDestroyed(GameEntity entity)
	{
		if (this.gameEntity.IsAuthority())
		{
			if (this._selection1Teleport)
			{
				this.gameEntity.manager.RequestDestroyItem(this._selection1Teleport.gameEntity.id);
			}
			if (this._selection2Teleport)
			{
				this.gameEntity.manager.RequestDestroyItem(this._selection2Teleport.gameEntity.id);
			}
		}
	}

	private new void OnDisable()
	{
		this.HandleHandDetach();
	}

	private void HandleHandAttached()
	{
		if (this.IsEquippedLocal())
		{
			this.isHandTapSetup = true;
			GorillaTagger.Instance.OnHandTap += this.HandleOnHandTap;
		}
	}

	private void HandleHandDetach()
	{
		if (this.isHandTapSetup)
		{
			this.isHandTapSetup = false;
			GorillaTagger.Instance.OnHandTap -= this.HandleOnHandTap;
		}
		this.isActivated = false;
	}

	private void HandleOnHandTap(bool isLeft, Vector3 position, Vector3 normal)
	{
		bool flag;
		if (base.FindAttachedHand(out flag, true, true) && isLeft == flag && this.isActivated)
		{
			this.PlaceTapTeleporter(position, normal);
		}
	}

	private Color GenerateColor(int seed)
	{
		Random.InitState(seed);
		float num = Mathf.Lerp(this.maxBrightness, this.minBrightness, Random.value);
		float num2 = Mathf.Lerp(this.maxBrightness, this.minBrightness, Random.value);
		Color black = Color.black;
		switch (Random.Range(0, 3))
		{
		case 0:
			black.r = num;
			black.g = num2;
			break;
		case 1:
			black.g = num;
			black.b = num2;
			break;
		case 2:
			black.b = num;
			black.r = num2;
			break;
		}
		return black;
	}

	protected override void OnUpdateAuthority(float dt)
	{
		this.isActivated = this.buttonActivatable.CheckInput(true, true, 0.25f, true);
		if (this.nextPlacementDelay > 0f)
		{
			this.nextPlacementDelay -= dt;
		}
	}

	private void PlaceTapTeleporter(Vector3 position, Vector3 normal)
	{
		if (this.nextPlacementDelay > 0f)
		{
			return;
		}
		if (!this.CheckValidTeleporterPlacement(position, normal))
		{
			return;
		}
		if (base.IsBlocked())
		{
			this.blockedSFX.Play();
			return;
		}
		base.SendClientToAuthorityRPC(0, new object[]
		{
			position,
			Quaternion.LookRotation(normal, base.transform.forward),
			this.nextSelectionId,
			this.hasInfiniteDuration ? (-1f) : this.portalDefaultDuration
		});
		this.CycleSelection();
		this.nextPlacementDelay = this.placementDelay;
	}

	private bool CheckValidTeleporterPlacement(Vector3 position, Vector3 direction)
	{
		Vector3 vector = position + direction * this.nearOffset;
		Vector3 vector2 = position + direction * this.farOffset;
		return Physics.OverlapCapsuleNonAlloc(vector, vector2, this.overlapCheckRadius, this.overlapCheckResults, this.overlapCheckLayers) == 0;
	}

	public override void ApplyUpgradeNodes(SIUpgradeSet withUpgrades)
	{
		this.instanceUpgrades = withUpgrades;
		this.useStealthTeleporters = withUpgrades.Contains(SIUpgradeType.Tapteleport_Stealth);
		this.isVelocityPreserved = withUpgrades.Contains(SIUpgradeType.Tapteleport_Keep_Velocity);
		this.hasInfiniteDuration = withUpgrades.Contains(SIUpgradeType.Tapteleport_Infinite_Use);
	}

	public override void ProcessClientToAuthorityRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
		if (rpcID == 0)
		{
			if (data == null || data.Length != 4)
			{
				return;
			}
			Vector3 vector;
			if (!GameEntityManager.ValidateDataType<Vector3>(data[0], out vector))
			{
				return;
			}
			Quaternion quaternion;
			if (!GameEntityManager.ValidateDataType<Quaternion>(data[1], out quaternion))
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[2], out num))
			{
				return;
			}
			if (num < 0 || num > 100)
			{
				return;
			}
			float num2;
			if (!GameEntityManager.ValidateDataType<float>(data[3], out num2))
			{
				return;
			}
			if (!this.gameEntity.IsAttachedToPlayer(NetPlayer.Get(info.Sender)))
			{
				return;
			}
			if (Vector3.Distance(vector, base.transform.position) > this.placementCheckDistance)
			{
				return;
			}
			if (!this.CheckValidTeleporterPlacement(vector, quaternion * Vector3.forward))
			{
				return;
			}
			this.RemoveTeleporter(num);
			this.PlaceNewTapTeleporter(vector, quaternion, num, num2);
		}
	}

	public override void ProcessClientToClientRPC(PhotonMessageInfo info, int rpcID, object[] data)
	{
		if (rpcID == 0)
		{
			if (data == null || data.Length != 1)
			{
				return;
			}
			int num;
			if (!GameEntityManager.ValidateDataType<int>(data[0], out num))
			{
				return;
			}
			if (num < 0 || num > 1)
			{
				return;
			}
			if (!this.gameEntity.IsAttachedToPlayer(NetPlayer.Get(info.Sender)))
			{
				return;
			}
			this.nextSelectionId = num;
			this.UpdateNextSelectionDisplay();
		}
	}

	private void RemoveTeleporter(int selectId)
	{
		if (selectId == 0)
		{
			if (this._selection1Teleport != null && this._selection1Teleport.gameObject.activeSelf)
			{
				this.gameEntity.manager.RequestDestroyItem(this._selection1Teleport.gameEntity.id);
				this._selection1Teleport = null;
				return;
			}
		}
		else if (selectId == 1 && this._selection2Teleport != null && this._selection2Teleport.gameObject.activeSelf)
		{
			this.gameEntity.manager.RequestDestroyItem(this._selection2Teleport.gameEntity.id);
			this._selection2Teleport = null;
		}
	}

	private void PlaceNewTapTeleporter(Vector3 position, Quaternion rotation, int selectionId, float duration)
	{
		GameEntityId gameEntityId = this.gameEntity.manager.RequestCreateItem(this.teleportPointPrefab.gameObject.name.GetStaticHash(), position, rotation, BitPackUtils.PackIntsIntoLong(selectionId, (int)duration));
		if (gameEntityId != GameEntityId.Invalid)
		{
			SIGadgetTapTeleporterDeployable component = this.gameEntity.manager.GetGameEntity(gameEntityId).GetComponent<SIGadgetTapTeleporterDeployable>();
			if (selectionId == 0)
			{
				if (this._selection2Teleport != null)
				{
					this._selection2Teleport.SetLink(this, component);
				}
				component.SetLink(this, this._selection2Teleport);
				this._selection1Teleport = component;
			}
			else if (selectionId == 1)
			{
				if (this._selection1Teleport != null)
				{
					this._selection1Teleport.SetLink(this, component);
				}
				component.SetLink(this, this._selection1Teleport);
				this._selection2Teleport = component;
			}
			this.UpdateNewTeleporters();
		}
	}

	private void UpdateNewTeleporters()
	{
		int num;
		if (this._selection1Teleport)
		{
			num = this._selection1Teleport.gameEntity.GetNetId();
		}
		else
		{
			num = 0;
		}
		int num2;
		if (this._selection2Teleport)
		{
			num2 = this._selection2Teleport.gameEntity.GetNetId();
		}
		else
		{
			num2 = 0;
		}
		long num3 = BitPackUtils.PackIntsIntoLong(num, num2);
		this.gameEntity.RequestState(this.gameEntity.id, num3);
	}

	private void HandleStateChanged(long oldState, long newState)
	{
		if (this.gameEntity.IsAuthority())
		{
			return;
		}
		int num;
		int num2;
		BitPackUtils.UnpackIntsFromLong(newState, out num, out num2);
		GameEntity gameEntityFromNetId = this.gameEntity.manager.GetGameEntityFromNetId(num);
		if (gameEntityFromNetId != null)
		{
			this._selection1Teleport = gameEntityFromNetId.GetComponent<SIGadgetTapTeleporterDeployable>();
		}
		else
		{
			this._selection1Teleport = null;
		}
		GameEntity gameEntityFromNetId2 = this.gameEntity.manager.GetGameEntityFromNetId(num2);
		if (gameEntityFromNetId2 != null)
		{
			this._selection2Teleport = gameEntityFromNetId2.GetComponent<SIGadgetTapTeleporterDeployable>();
			return;
		}
		this._selection2Teleport = null;
	}

	private void ApplyIdentifierColor()
	{
		this.identifierColorDisplay.material.color = this.identifierColor;
	}

	private void UpdateNextSelectionDisplay()
	{
		if (this.nextSelectionId == 0)
		{
			this.selectionColorDisplay.material = this.selectionColor1;
			return;
		}
		if (this.nextSelectionId == 1)
		{
			this.selectionColorDisplay.material = this.selectionColor2;
		}
	}

	public void CycleSelection()
	{
		this.nextSelectionId = (this.nextSelectionId + 1) % 2;
		this.UpdateNextSelectionDisplay();
		base.SendClientToClientRPC(0, new object[] { this.nextSelectionId });
	}

	[SerializeField]
	private GameButtonActivatable buttonActivatable;

	[SerializeField]
	private GameObject teleportPointPrefab;

	[SerializeField]
	private SoundBankPlayer blockedSFX;

	[SerializeField]
	private float placementDelay = 0.5f;

	[SerializeField]
	private Renderer identifierColorDisplay;

	[SerializeField]
	private Renderer selectionColorDisplay;

	[SerializeField]
	private Material selectionColor1;

	[SerializeField]
	private Material selectionColor2;

	[SerializeField]
	private float portalDefaultDuration = 30f;

	private float placementCheckDistance = 0.3f;

	private SIGadgetTapTeleporterDeployable _selection1Teleport;

	private SIGadgetTapTeleporterDeployable _selection2Teleport;

	private bool isHandTapSetup;

	private bool isActivated;

	private float nextPlacementDelay;

	private int nextSelectionId;

	private SIUpgradeSet instanceUpgrades;

	private float minBrightness = 0.3f;

	private float maxBrightness = 1f;

	[SerializeField]
	private LayerMask overlapCheckLayers;

	[SerializeField]
	private float nearOffset = 0.11f;

	[SerializeField]
	private float farOffset = 0.664f;

	[SerializeField]
	private float overlapCheckRadius = 0.1f;

	private Collider[] overlapCheckResults = new Collider[1];
}
