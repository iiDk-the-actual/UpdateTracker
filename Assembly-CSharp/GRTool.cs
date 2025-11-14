using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;

public class GRTool : MonoBehaviour, IGameEntitySerialize, IGameEntityComponent, IGameEntityDebugComponent
{
	public event GRTool.EnergyChangeEvent OnEnergyChange;

	public event GRTool.ToolUpgradedEvent onToolUpgraded;

	private void Awake()
	{
	}

	private void Start()
	{
		if (this.gameEntity == null)
		{
			this.gameEntity = base.GetComponent<GameEntity>();
		}
		this.RefreshMeters();
	}

	public void OnEntityInit()
	{
		this.energy = this.GetEnergyStart();
		GhostReactor.ToolEntityCreateData toolEntityCreateData = GhostReactor.ToolEntityCreateData.Unpack(this.gameEntity.createData);
		GhostReactorManager ghostReactorManager = GhostReactorManager.Get(this.gameEntity);
		if (ghostReactorManager != null)
		{
			GRToolUpgradePurchaseStationFull toolUpgradeStationFullForIndex = ghostReactorManager.GetToolUpgradeStationFullForIndex(toolEntityCreateData.stationIndex);
			if (toolUpgradeStationFullForIndex != null)
			{
				toolUpgradeStationFullForIndex.InitLinkedEntity(this.gameEntity);
			}
		}
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	public int GetEnergyMax()
	{
		return this.attributes.CalculateFinalValueForAttribute(GRAttributeType.EnergyMax);
	}

	public int GetEnergyUseCost()
	{
		return this.attributes.CalculateFinalValueForAttribute(GRAttributeType.EnergyUseCost);
	}

	public int GetEnergyStart()
	{
		if (!this.attributes.HasValueForAttribute(GRAttributeType.EnergyStart))
		{
			return 0;
		}
		return this.attributes.CalculateFinalValueForAttribute(GRAttributeType.EnergyStart);
	}

	private void OnEnable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.GrabbedByPlayer));
	}

	private void OnDisable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this.GrabbedByPlayer));
	}

	public void RefillEnergy(int count, GameEntityId chargingEntityId)
	{
		this.SetEnergyInternal(this.energy + count, chargingEntityId);
	}

	public void RefillEnergy()
	{
		this.SetEnergyInternal(this.GetEnergyMax(), GameEntityId.Invalid);
	}

	public void UseEnergy()
	{
		this.SetEnergyInternal(this.energy - this.GetEnergyUseCost(), GameEntityId.Invalid);
	}

	public bool HasEnoughEnergy()
	{
		return this.energy >= this.GetEnergyUseCost();
	}

	public void SetEnergy(int newEnergy)
	{
		this.SetEnergyInternal(newEnergy, GameEntityId.Invalid);
	}

	public bool IsEnergyFull()
	{
		return this.energy >= this.GetEnergyMax();
	}

	private void SetEnergyInternal(int value, GameEntityId chargingEntityId)
	{
		int num = this.energy;
		this.energy = Mathf.Clamp(value, 0, this.GetEnergyMax());
		int num2 = this.energy - num;
		GRTool.EnergyChangeEvent onEnergyChange = this.OnEnergyChange;
		if (onEnergyChange != null)
		{
			onEnergyChange(this, num2, chargingEntityId);
		}
		this.RefreshMeters();
	}

	public void RefreshMeters()
	{
		for (int i = 0; i < this.energyMeters.Count; i++)
		{
			this.energyMeters[i].Refresh();
		}
	}

	public bool HasUpgradeInstalled(GRToolProgressionManager.ToolParts upgradeID)
	{
		for (int i = 0; i < this.upgradeSlots.Count; i++)
		{
			if (this.upgradeSlots[i].installedItem != null && this.upgradeSlots[i].installedItem.UpgradeType == upgradeID)
			{
				return true;
			}
		}
		return false;
	}

	public GRTool.Upgrade FindMatchingUpgrade(GRToolProgressionManager.ToolParts upgradeID)
	{
		for (int i = 0; i < this.upgrades.Count; i++)
		{
			if (this.upgrades[i].UpgradeType == upgradeID)
			{
				return this.upgrades[i];
			}
		}
		return null;
	}

	public float GetPointDistanceToUpgrade(Vector3 point, GRTool.Upgrade upgrade)
	{
		if (upgrade.VisibleItem.Count < 1)
		{
			return -1f;
		}
		if (this.upgradeListsAreValidFor != upgrade)
		{
			this.reservedMeshFilterSearchList.Clear();
			upgrade.VisibleItem[0].GetComponentsInChildren<MeshFilter>(this.reservedMeshFilterSearchList);
			this.reservedMeshFilterSearchListSkinned.Clear();
			upgrade.VisibleItem[0].GetComponentsInChildren<SkinnedMeshRenderer>(false, this.reservedMeshFilterSearchListSkinned);
			this.upgradeListsAreValidFor = upgrade;
		}
		float num = float.MaxValue;
		foreach (MeshFilter meshFilter in this.reservedMeshFilterSearchList)
		{
			Vector3 vector = meshFilter.transform.InverseTransformPoint(point);
			Bounds bounds = meshFilter.sharedMesh.bounds;
			Vector3 vector2 = new Vector3(Mathf.Clamp(vector.x, bounds.min.x, bounds.max.x), Mathf.Clamp(vector.y, bounds.min.y, bounds.max.y), Mathf.Clamp(vector.z, bounds.min.z, bounds.max.z));
			Vector3 vector3 = vector - vector2;
			float sqrMagnitude = meshFilter.transform.TransformVector(vector3).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
			}
		}
		if (this.reservedMeshFilterSearchListSkinned != null)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.reservedMeshFilterSearchListSkinned)
			{
				Vector3 vector4 = skinnedMeshRenderer.transform.InverseTransformPoint(point);
				Bounds localBounds = skinnedMeshRenderer.localBounds;
				Vector3 vector5 = new Vector3(Mathf.Clamp(vector4.x, localBounds.min.x, localBounds.max.x), Mathf.Clamp(vector4.y, localBounds.min.y, localBounds.max.y), Mathf.Clamp(vector4.z, localBounds.min.z, localBounds.max.z));
				Vector3 vector6 = vector4 - vector5;
				float sqrMagnitude2 = skinnedMeshRenderer.transform.TransformVector(vector6).sqrMagnitude;
				if (sqrMagnitude2 < num)
				{
					num = sqrMagnitude2;
				}
			}
		}
		if (num == 3.4028235E+38f)
		{
			return Vector3.Distance(point, upgrade.VisibleItem[0].transform.position);
		}
		return Mathf.Sqrt(num);
	}

	public Transform GetUpgradeAttachTransform(GRTool.Upgrade upgrade)
	{
		if (upgrade.VisibleItem.Count < 1)
		{
			return null;
		}
		return upgrade.VisibleItem[0].transform;
	}

	public void UpgradeTool(GRToolProgressionManager.ToolParts upgradeID)
	{
		for (int i = 0; i < this.upgrades.Count; i++)
		{
			if (this.upgrades[i].UpgradeType == upgradeID)
			{
				this.ClearUpgradeSlot(this.upgrades[i].Slot);
				for (int j = 0; j < this.upgrades[i].VisibleItem.Count; j++)
				{
					this.upgrades[i].VisibleItem[j].SetActive(true);
				}
				for (int k = 0; k < this.upgradeSlots[this.upgrades[i].Slot].DefaultVisibleItems.Count; k++)
				{
					this.upgradeSlots[this.upgrades[i].Slot].DefaultVisibleItems[k].SetActive(false);
				}
				foreach (GRBonusEntry grbonusEntry in this.upgrades[i].bonusEffects)
				{
					this.attributes.AddBonus(grbonusEntry);
				}
				this.upgradeSlots[this.upgrades[i].Slot].installedItem = this.upgrades[i];
				if (this.UpgradeFXNode != null && this.upgrades[i].VisibleItem.Count > 0)
				{
					this.UpgradeFXNode.transform.position = this.upgrades[i].VisibleItem[0].transform.position;
					this.UpgradeFXNode.transform.rotation = this.upgrades[i].VisibleItem[0].transform.rotation;
					ParticleSystem componentInChildren = this.UpgradeFXNode.GetComponentInChildren<ParticleSystem>();
					AudioSource componentInChildren2 = this.UpgradeFXNode.GetComponentInChildren<AudioSource>();
					if (componentInChildren != null)
					{
						componentInChildren.Play();
					}
					if (componentInChildren2 != null)
					{
						componentInChildren2.Play();
					}
				}
			}
		}
		GRTool.ToolUpgradedEvent toolUpgradedEvent = this.onToolUpgraded;
		if (toolUpgradedEvent == null)
		{
			return;
		}
		toolUpgradedEvent(this);
	}

	public void ClearUpgradeSlot(int slot)
	{
		if (this.upgradeSlots[slot].installedItem != null)
		{
			for (int i = 0; i < this.upgradeSlots[slot].installedItem.VisibleItem.Count; i++)
			{
				this.upgradeSlots[slot].installedItem.VisibleItem[i].SetActive(false);
			}
			foreach (GRBonusEntry grbonusEntry in this.upgradeSlots[slot].installedItem.bonusEffects)
			{
				this.attributes.RemoveBonus(grbonusEntry);
			}
			for (int j = 0; j < this.upgradeSlots[slot].DefaultVisibleItems.Count; j++)
			{
				this.upgradeSlots[slot].DefaultVisibleItems[j].SetActive(true);
			}
		}
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		writer.Write(this.upgradeSlots.Count);
		for (int i = 0; i < this.upgradeSlots.Count; i++)
		{
			if (this.upgradeSlots[i] != null)
			{
				if (this.upgradeSlots[i].installedItem != null)
				{
					writer.Write(this.upgradeSlots[i].installedItem.UpgradeType.ToString());
				}
				else
				{
					writer.Write("");
				}
			}
			else
			{
				writer.Write("");
			}
		}
		writer.Write(this.energy);
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			GRToolProgressionManager.ToolParts toolParts = GRToolProgressionManager.ToolParts.None;
			if (Enum.TryParse<GRToolProgressionManager.ToolParts>(reader.ReadString(), out toolParts))
			{
				this.UpgradeTool(toolParts);
			}
		}
		int num2 = reader.ReadInt32();
		this.SetEnergy(num2);
	}

	public void GrabbedByPlayer()
	{
		if (this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			GRPlayer grplayer = GRPlayer.Get(this.gameEntity.heldByActorNumber);
			if (grplayer)
			{
				grplayer.GrabbedItem(this.gameEntity.id, base.gameObject.name);
			}
		}
	}

	public void GetDebugTextLines(out List<string> strings)
	{
		strings = new List<string>();
		strings.Add(string.Format("Tool Energy: <color=\"yellow\">{0}<color=\"white\"> ", this.energy));
	}

	public GRAttributes attributes;

	public List<GRTool.Upgrade> upgrades;

	public List<GRTool.UpgradeSlot> upgradeSlots = new List<GRTool.UpgradeSlot>();

	public List<GRMeterEnergy> energyMeters;

	public GameEntity gameEntity;

	public GRTool.GRToolType toolType;

	[ReadOnly]
	public int energy;

	public GameObject UpgradeFXNode;

	private List<MeshFilter> reservedMeshFilterSearchList = new List<MeshFilter>(32);

	private List<SkinnedMeshRenderer> reservedMeshFilterSearchListSkinned = new List<SkinnedMeshRenderer>(32);

	private GRTool.Upgrade upgradeListsAreValidFor;

	public enum GRToolType
	{
		None,
		Club,
		Collector,
		Flash,
		Lantern,
		Revive,
		ShieldGun,
		DirectionalShield,
		DockWrist,
		EnergyEfficiency,
		DropPod,
		HockeyStick,
		StatusWatch,
		RattyBackpack
	}

	[Serializable]
	public class Upgrade
	{
		public GRToolProgressionManager.ToolParts UpgradeType;

		public int Slot;

		public List<GameObject> VisibleItem;

		public List<GRBonusEntry> bonusEffects;
	}

	[Serializable]
	public class UpgradeSlot
	{
		public List<GameObject> DefaultVisibleItems;

		[NonSerialized]
		public GRTool.Upgrade installedItem;
	}

	public delegate void EnergyChangeEvent(GRTool tool, int energyChange, GameEntityId chargingEntityId);

	public delegate void ToolUpgradedEvent(GRTool tool);
}
