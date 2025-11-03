using System;
using System.Collections.Generic;
using GorillaTagScripts;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BuilderPiecePrivatePlot : MonoBehaviour
{
	private void Awake()
	{
		this.Init();
	}

	private void Init()
	{
		if (this.initDone)
		{
			return;
		}
		this.materialProps = new MaterialPropertyBlock();
		this.usedResources = new int[3];
		for (int i = 0; i < this.usedResources.Length; i++)
		{
			this.usedResources[i] = 0;
		}
		this.tempResourceCount = new int[3];
		this.piece = base.GetComponent<BuilderPiece>();
		this.SetPlotState(BuilderPiecePrivatePlot.PlotState.Vacant);
		this.piecesToCount = new Queue<BuilderPiece>(1024);
		this.initDone = true;
		this.privatePlotIndex = -1;
	}

	private void Start()
	{
		if (this.piece != null && this.piece.GetTable() != null)
		{
			BuilderTable table = this.piece.GetTable();
			this.doesLocalPlayerOwnAPlot = table.DoesPlayerOwnPlot(PhotonNetwork.LocalPlayer.ActorNumber);
			table.OnLocalPlayerClaimedPlot.AddListener(new UnityAction<bool>(this.OnLocalPlayerClaimedPlot));
			this.UpdateVisuals();
			foreach (BuilderResourceMeter builderResourceMeter in this.resourceMeters)
			{
				builderResourceMeter.table = this.piece.GetTable();
			}
		}
		this.buildArea.gameObject.SetActive(true);
		this.buildArea.enabled = true;
		this.buildAreaBounds = this.buildArea.bounds;
		this.buildArea.gameObject.SetActive(false);
		this.buildArea.enabled = false;
		this.zoneRenderers.Clear();
		this.zoneRenderers.Add(this.tmpLabel.GetComponent<Renderer>());
		foreach (BuilderResourceMeter builderResourceMeter2 in this.resourceMeters)
		{
			this.zoneRenderers.AddRange(builderResourceMeter2.GetComponentsInChildren<Renderer>());
		}
		this.zoneRenderers.AddRange(this.borderMeshes);
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
		this.inBuilderZone = true;
		this.OnZoneChanged();
	}

	private void OnDestroy()
	{
		if (this.piece != null && this.piece.GetTable() != null)
		{
			this.piece.GetTable().OnLocalPlayerClaimedPlot.RemoveListener(new UnityAction<bool>(this.OnLocalPlayerClaimedPlot));
		}
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.instance.IsZoneActive(this.piece.GetTable().tableZone);
		this.inBuilderZone = flag;
	}

	private void OnLocalPlayerClaimedPlot(bool claim)
	{
		this.doesLocalPlayerOwnAPlot = claim;
		this.UpdateVisuals();
	}

	public void UpdatePlot()
	{
		if (BuilderPieceInteractor.instance == null || BuilderPieceInteractor.instance.heldChainLength == null || BuilderPieceInteractor.instance.heldChainLength.Length < 2)
		{
			return;
		}
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		if (!this.initDone)
		{
			this.Init();
		}
		if ((this.plotState == BuilderPiecePrivatePlot.PlotState.Occupied && this.owningPlayerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) || (this.plotState == BuilderPiecePrivatePlot.PlotState.Vacant && !this.doesLocalPlayerOwnAPlot))
		{
			BuilderPiece parentPiece = BuilderPieceInteractor.instance.prevPotentialPlacement[0].parentPiece;
			BuilderPiece parentPiece2 = BuilderPieceInteractor.instance.prevPotentialPlacement[1].parentPiece;
			bool flag = false;
			if (parentPiece == null && this.leftPotentialParent != null)
			{
				this.isLeftOverPlot = false;
				this.leftPotentialParent = null;
				flag = true;
			}
			else if ((this.leftPotentialParent == null && parentPiece != null) || (parentPiece != null && !parentPiece.Equals(this.leftPotentialParent)))
			{
				BuilderPiece attachedBuiltInPiece = parentPiece.GetAttachedBuiltInPiece();
				this.isLeftOverPlot = attachedBuiltInPiece != null && attachedBuiltInPiece.Equals(this.piece);
				this.leftPotentialParent = parentPiece;
				flag = true;
			}
			if (parentPiece2 == null && this.rightPotentialParent != null)
			{
				this.isRightOverPlot = false;
				this.rightPotentialParent = null;
				flag = true;
			}
			else if ((this.rightPotentialParent == null && parentPiece2 != null) || (parentPiece2 != null && !parentPiece2.Equals(this.rightPotentialParent)))
			{
				BuilderPiece attachedBuiltInPiece2 = parentPiece2.GetAttachedBuiltInPiece();
				this.isRightOverPlot = attachedBuiltInPiece2 != null && attachedBuiltInPiece2.Equals(this.piece);
				this.rightPotentialParent = parentPiece2;
				flag = true;
			}
			if (flag)
			{
				this.UpdateVisuals();
			}
		}
		else if (this.isRightOverPlot || this.isLeftOverPlot)
		{
			this.isRightOverPlot = false;
			this.isLeftOverPlot = false;
			this.UpdateVisuals();
		}
		foreach (BuilderResourceMeter builderResourceMeter in this.resourceMeters)
		{
			builderResourceMeter.UpdateMeterFill();
		}
	}

	public void RecountPlotCost()
	{
		this.Init();
		this.piece.GetChainCost(this.usedResources);
		this.UpdateVisuals();
	}

	public void OnPieceAttachedToPlot(BuilderPiece attachPiece)
	{
		this.AddChainResourcesToCount(attachPiece, true);
		this.UpdateVisuals();
	}

	public void OnPieceDetachedFromPlot(BuilderPiece detachPiece)
	{
		this.AddChainResourcesToCount(detachPiece, false);
		this.UpdateVisuals();
	}

	public void ChangeAttachedPieceCount(int delta)
	{
		this.attachedPieceCount += delta;
		this.UpdateVisuals();
	}

	public void AddChainResourcesToCount(BuilderPiece chain, bool attach)
	{
		if (chain == null)
		{
			return;
		}
		this.piecesToCount.Clear();
		for (int i = 0; i < this.tempResourceCount.Length; i++)
		{
			this.tempResourceCount[i] = 0;
		}
		this.piecesToCount.Enqueue(chain);
		this.AddPieceCostToArray(chain, this.tempResourceCount);
		bool flag = false;
		while (this.piecesToCount.Count > 0 && !flag)
		{
			BuilderPiece builderPiece = this.piecesToCount.Dequeue().firstChildPiece;
			while (builderPiece != null)
			{
				this.piecesToCount.Enqueue(builderPiece);
				if (!this.AddPieceCostToArray(builderPiece, this.tempResourceCount))
				{
					Debug.LogWarning("Builder plot placing pieces over limits");
					flag = true;
					break;
				}
				builderPiece = builderPiece.nextSiblingPiece;
			}
		}
		for (int j = 0; j < this.usedResources.Length; j++)
		{
			if (attach)
			{
				this.usedResources[j] += this.tempResourceCount[j];
			}
			else
			{
				this.usedResources[j] -= this.tempResourceCount[j];
			}
		}
	}

	public void ClaimPlotForPlayerNumber(int player)
	{
		this.owningPlayerActorNumber = player;
		this.SetPlotState(BuilderPiecePrivatePlot.PlotState.Occupied);
	}

	public int GetOwnerActorNumber()
	{
		return this.owningPlayerActorNumber;
	}

	public void ClearPlot()
	{
		this.Init();
		this.attachedPieceCount = 0;
		for (int i = 0; i < this.usedResources.Length; i++)
		{
			this.usedResources[i] = 0;
		}
		this.SetPlotState(BuilderPiecePrivatePlot.PlotState.Vacant);
	}

	public void FreePlot()
	{
		this.SetPlotState(BuilderPiecePrivatePlot.PlotState.Vacant);
	}

	public bool IsPlotClaimed()
	{
		return this.plotState > BuilderPiecePrivatePlot.PlotState.Vacant;
	}

	public bool IsChainUnderCapacity(BuilderPiece chain)
	{
		if (chain == null)
		{
			return true;
		}
		this.piecesToCount.Clear();
		for (int i = 0; i < this.tempResourceCount.Length; i++)
		{
			this.tempResourceCount[i] = this.usedResources[i];
		}
		this.piecesToCount.Enqueue(chain);
		if (!this.AddPieceCostToArray(chain, this.tempResourceCount))
		{
			return false;
		}
		while (this.piecesToCount.Count > 0)
		{
			BuilderPiece builderPiece = this.piecesToCount.Dequeue().firstChildPiece;
			while (builderPiece != null)
			{
				this.piecesToCount.Enqueue(builderPiece);
				if (!this.AddPieceCostToArray(builderPiece, this.tempResourceCount))
				{
					return false;
				}
				builderPiece = builderPiece.nextSiblingPiece;
			}
		}
		return true;
	}

	public bool AddPieceCostToArray(BuilderPiece addedPiece, int[] array)
	{
		if (addedPiece == null)
		{
			return true;
		}
		if (addedPiece.cost != null)
		{
			foreach (BuilderResourceQuantity builderResourceQuantity in addedPiece.cost.quantities)
			{
				if (builderResourceQuantity.type >= BuilderResourceType.Basic && builderResourceQuantity.type < BuilderResourceType.Count)
				{
					array[(int)builderResourceQuantity.type] += builderResourceQuantity.count;
					if (array[(int)builderResourceQuantity.type] > this.piece.GetTable().GetPrivateResourceLimitForType((int)builderResourceQuantity.type))
					{
						return false;
					}
				}
			}
			return true;
		}
		return true;
	}

	public bool CanPlayerAttachToPlot(int actorNumber)
	{
		return (this.plotState == BuilderPiecePrivatePlot.PlotState.Occupied && this.owningPlayerActorNumber == actorNumber) || (this.plotState == BuilderPiecePrivatePlot.PlotState.Vacant && !this.piece.GetTable().DoesPlayerOwnPlot(actorNumber));
	}

	public bool CanPlayerGrabFromPlot(int actorNumber, Vector3 worldPosition)
	{
		if (this.owningPlayerActorNumber == actorNumber || this.plotState == BuilderPiecePrivatePlot.PlotState.Vacant)
		{
			return true;
		}
		int num;
		if (this.piece.GetTable().plotOwners.TryGetValue(actorNumber, out num))
		{
			BuilderPiece builderPiece = this.piece.GetTable().GetPiece(num);
			BuilderPiecePrivatePlot builderPiecePrivatePlot;
			if (builderPiece != null && builderPiece.TryGetPlotComponent(out builderPiecePrivatePlot))
			{
				return builderPiecePrivatePlot.IsLocationWithinPlotExtents(worldPosition);
			}
		}
		return false;
	}

	private void SetPlotState(BuilderPiecePrivatePlot.PlotState newState)
	{
		this.plotState = newState;
		BuilderPiecePrivatePlot.PlotState plotState = this.plotState;
		if (plotState != BuilderPiecePrivatePlot.PlotState.Vacant)
		{
			if (plotState == BuilderPiecePrivatePlot.PlotState.Occupied)
			{
				if (this.tmpLabel != null && NetworkSystem.Instance != null)
				{
					string text = string.Empty;
					NetPlayer player = NetworkSystem.Instance.GetPlayer(this.owningPlayerActorNumber);
					RigContainer rigContainer;
					if (player != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
					{
						text = rigContainer.Rig.playerNameVisible;
					}
					if (string.IsNullOrEmpty(text) && !this.tmpLabel.text.Equals("OCCUPIED"))
					{
						this.tmpLabel.text = "OCCUPIED";
					}
					else if (!this.tmpLabel.text.Equals(text))
					{
						this.tmpLabel.text = text;
					}
				}
				else if (this.tmpLabel != null && !this.tmpLabel.text.Equals("OCCUPIED"))
				{
					this.tmpLabel.text = "OCCUPIED";
				}
			}
		}
		else
		{
			this.owningPlayerActorNumber = -1;
			if (this.tmpLabel != null && !this.tmpLabel.text.Equals(string.Empty))
			{
				this.tmpLabel.text = string.Empty;
			}
		}
		this.UpdateVisuals();
	}

	public bool IsLocationWithinPlotExtents(Vector3 worldPosition)
	{
		if (!this.buildAreaBounds.Contains(worldPosition))
		{
			return false;
		}
		Vector3 vector = this.buildArea.transform.InverseTransformPoint(worldPosition);
		Vector3 vector2 = this.buildArea.center + this.buildArea.size / 2f;
		Vector3 vector3 = this.buildArea.center - this.buildArea.size / 2f;
		return vector.x >= vector3.x && vector.x <= vector2.x && vector.y >= vector3.y && vector.y <= vector2.y && vector.z >= vector3.z && vector.z <= vector2.z;
	}

	public void OnAvailableResourceChange()
	{
		this.UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (this.usedResources == null || this.piece.GetTable() == null)
		{
			return;
		}
		BuilderPiecePrivatePlot.PlotState plotState = this.plotState;
		if (plotState != BuilderPiecePrivatePlot.PlotState.Vacant)
		{
			if (plotState != BuilderPiecePrivatePlot.PlotState.Occupied)
			{
				return;
			}
			if (this.owningPlayerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
			{
				this.UpdateVisualsForOwner();
				return;
			}
			this.SetBorderColor(this.placementDisallowedColor);
			int num = 0;
			while (num < this.resourceMeters.Count && num < 3)
			{
				int privateResourceLimitForType = this.piece.GetTable().GetPrivateResourceLimitForType(num);
				if (privateResourceLimitForType != 0)
				{
					this.resourceMeters[num].SetNormalizedFillTarget((float)(privateResourceLimitForType - this.usedResources[num]) / (float)privateResourceLimitForType);
				}
				num++;
			}
		}
		else
		{
			if (!this.doesLocalPlayerOwnAPlot)
			{
				this.UpdateVisualsForOwner();
				return;
			}
			this.SetBorderColor(this.placementDisallowedColor);
			for (int i = 0; i < this.resourceMeters.Count; i++)
			{
				if (i >= 3)
				{
					return;
				}
				int privateResourceLimitForType2 = this.piece.GetTable().GetPrivateResourceLimitForType(i);
				if (privateResourceLimitForType2 != 0)
				{
					this.resourceMeters[i].SetNormalizedFillTarget((float)(privateResourceLimitForType2 - this.usedResources[i]) / (float)privateResourceLimitForType2);
				}
			}
			return;
		}
	}

	private void UpdateVisualsForOwner()
	{
		bool flag = true;
		if (this.usedResources == null)
		{
			return;
		}
		if (BuilderPieceInteractor.instance == null || BuilderPieceInteractor.instance.heldChainCost == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = this.usedResources[i];
			if (this.isLeftOverPlot)
			{
				num += BuilderPieceInteractor.instance.heldChainCost[0][i];
			}
			if (this.isRightOverPlot)
			{
				num += BuilderPieceInteractor.instance.heldChainCost[1][i];
			}
			int privateResourceLimitForType = this.piece.GetTable().GetPrivateResourceLimitForType(i);
			if (num < privateResourceLimitForType)
			{
				flag = false;
			}
			if (privateResourceLimitForType != 0 && this.resourceMeters.Count > i)
			{
				this.resourceMeters[i].SetNormalizedFillTarget((float)(privateResourceLimitForType - num) / (float)privateResourceLimitForType);
			}
		}
		if (flag)
		{
			this.SetBorderColor(this.placementDisallowedColor);
			return;
		}
		this.SetBorderColor(this.placementAllowedColor);
	}

	private void SetBorderColor(Color color)
	{
		this.borderMeshes[0].GetPropertyBlock(this.materialProps);
		this.materialProps.SetColor(ShaderProps._BaseColor, color);
		foreach (MeshRenderer meshRenderer in this.borderMeshes)
		{
			meshRenderer.SetPropertyBlock(this.materialProps);
		}
	}

	[SerializeField]
	private Color placementAllowedColor;

	[SerializeField]
	private Color placementDisallowedColor;

	[SerializeField]
	private Color overCapacityColor;

	public List<MeshRenderer> borderMeshes;

	public BoxCollider buildArea;

	[SerializeField]
	private TMP_Text tmpLabel;

	[SerializeField]
	private List<BuilderResourceMeter> resourceMeters;

	[NonSerialized]
	public int[] usedResources;

	[NonSerialized]
	public int[] tempResourceCount;

	[SerializeField]
	private GameObject plotClaimedFX;

	private BuilderPiece leftPotentialParent;

	private BuilderPiece rightPotentialParent;

	private bool isLeftOverPlot;

	private bool isRightOverPlot;

	private Bounds buildAreaBounds;

	[HideInInspector]
	public BuilderPiece piece;

	private int owningPlayerActorNumber;

	private int attachedPieceCount;

	[HideInInspector]
	public int privatePlotIndex;

	[HideInInspector]
	public BuilderPiecePrivatePlot.PlotState plotState;

	private bool doesLocalPlayerOwnAPlot;

	private Queue<BuilderPiece> piecesToCount;

	private bool initDone;

	private MaterialPropertyBlock materialProps;

	private List<Renderer> zoneRenderers = new List<Renderer>(12);

	private bool inBuilderZone;

	public enum PlotState
	{
		Vacant,
		Occupied
	}
}
