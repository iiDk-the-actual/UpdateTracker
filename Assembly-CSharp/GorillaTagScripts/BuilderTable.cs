using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BoingKit;
using CjLib;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTagScripts.Builder;
using Ionic.Zlib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using Unity.Collections;
using Unity.Jobs;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class BuilderTable : MonoBehaviour, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		[HideInInspector]
		public float gridSize
		{
			get
			{
				return this.pieceScale / 2f;
			}
		}

		private void ExecuteAction(BuilderAction action)
		{
			if (!this.isTableMutable)
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(action.pieceId);
			BuilderPiece piece2 = this.GetPiece(action.parentPieceId);
			int playerActorNumber = action.playerActorNumber;
			bool flag = PhotonNetwork.LocalPlayer.ActorNumber == action.playerActorNumber;
			switch (action.type)
			{
			case BuilderActionType.AttachToPlayer:
			{
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				piece.transform.localScale = Vector3.one;
				RigContainer rigContainer;
				if (!VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(playerActorNumber), out rigContainer))
				{
					string.Format("Execute Builder Action {0} {1} {2} {3} {4}", new object[] { action.localCommandId, action.type, action.pieceId, action.playerActorNumber, action.isLeftHand });
					return;
				}
				BodyDockPositions myBodyDockPositions = rigContainer.Rig.myBodyDockPositions;
				Transform transform = (action.isLeftHand ? myBodyDockPositions.leftHandTransform : myBodyDockPositions.rightHandTransform);
				piece.SetParentHeld(transform, playerActorNumber, action.isLeftHand);
				piece.transform.SetLocalPositionAndRotation(action.localPosition, action.localRotation);
				BuilderPiece.State state = (flag ? BuilderPiece.State.GrabbedLocal : BuilderPiece.State.Grabbed);
				piece.SetState(state, false);
				if (!flag)
				{
					BuilderPieceInteractor.instance.RemovePieceFromHeld(piece);
				}
				if (flag)
				{
					BuilderPieceInteractor.instance.AddPieceToHeld(piece, action.isLeftHand, action.localPosition, action.localRotation);
					return;
				}
				break;
			}
			case BuilderActionType.DetachFromPlayer:
				if (flag)
				{
					BuilderPieceInteractor.instance.RemovePieceFromHeld(piece);
				}
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				piece.transform.localScale = Vector3.one;
				return;
			case BuilderActionType.AttachToPiece:
			{
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				piece.transform.localScale = Vector3.one;
				Quaternion identity = Quaternion.identity;
				Vector3 zero = Vector3.zero;
				Vector3 position = piece.transform.position;
				Quaternion rotation = piece.transform.rotation;
				if (piece2 != null)
				{
					piece.BumpTwistToPositionRotation(action.twist, action.bumpOffsetx, action.bumpOffsetz, action.attachIndex, piece2.gridPlanes[action.parentAttachIndex], out zero, out identity, out position, out rotation);
				}
				piece.transform.SetPositionAndRotation(position, rotation);
				BuilderPiece.State state2;
				if (piece2 == null)
				{
					state2 = BuilderPiece.State.AttachedAndPlaced;
				}
				else if (piece2.isArmShelf || piece2.state == BuilderPiece.State.AttachedToArm)
				{
					state2 = BuilderPiece.State.AttachedToArm;
				}
				else if (piece2.isBuiltIntoTable || piece2.state == BuilderPiece.State.AttachedAndPlaced)
				{
					state2 = BuilderPiece.State.AttachedAndPlaced;
				}
				else if (piece2.state == BuilderPiece.State.Grabbed)
				{
					state2 = BuilderPiece.State.Grabbed;
				}
				else if (piece2.state == BuilderPiece.State.GrabbedLocal)
				{
					state2 = BuilderPiece.State.GrabbedLocal;
				}
				else
				{
					state2 = BuilderPiece.State.AttachedToDropped;
				}
				BuilderPiece rootPiece = piece2.GetRootPiece();
				this.gridPlaneData.Clear();
				this.checkGridPlaneData.Clear();
				this.allPotentialPlacements.Clear();
				BuilderTable.tempPieceSet.Clear();
				QueryParameters queryParameters = new QueryParameters
				{
					layerMask = this.allPiecesMask
				};
				OverlapSphereCommand overlapSphereCommand = new OverlapSphereCommand(position, 1f, queryParameters);
				this.nearbyPiecesCommands[0] = overlapSphereCommand;
				OverlapSphereCommand.ScheduleBatch(this.nearbyPiecesCommands, this.nearbyPiecesResults, 1, 1024, default(JobHandle)).Complete();
				int num = 0;
				while (num < 1024 && this.nearbyPiecesResults[num].instanceID != 0)
				{
					BuilderPiece builderPiece = piece;
					BuilderPiece builderPieceFromCollider = BuilderPiece.GetBuilderPieceFromCollider(this.nearbyPiecesResults[num].collider);
					if (builderPieceFromCollider != null && !BuilderTable.tempPieceSet.Contains(builderPieceFromCollider))
					{
						BuilderTable.tempPieceSet.Add(builderPieceFromCollider);
						if (this.CanPiecesPotentiallyOverlap(builderPiece, rootPiece, state2, builderPieceFromCollider))
						{
							for (int i = 0; i < builderPieceFromCollider.gridPlanes.Count; i++)
							{
								BuilderGridPlaneData builderGridPlaneData = new BuilderGridPlaneData(builderPieceFromCollider.gridPlanes[i], -1);
								this.checkGridPlaneData.Add(in builderGridPlaneData);
							}
						}
					}
					num++;
				}
				BuilderTableJobs.BuildTestPieceListForJob(piece, this.gridPlaneData);
				BuilderPotentialPlacement builderPotentialPlacement = new BuilderPotentialPlacement
				{
					localPosition = zero,
					localRotation = identity,
					attachIndex = action.attachIndex,
					parentAttachIndex = action.parentAttachIndex,
					attachPiece = piece,
					parentPiece = piece2
				};
				this.CalcAllPotentialPlacements(this.gridPlaneData, this.checkGridPlaneData, builderPotentialPlacement, this.allPotentialPlacements);
				piece.SetParentPiece(action.attachIndex, piece2, action.parentAttachIndex);
				for (int j = 0; j < this.allPotentialPlacements.Count; j++)
				{
					BuilderPotentialPlacement builderPotentialPlacement2 = this.allPotentialPlacements[j];
					BuilderAttachGridPlane builderAttachGridPlane = builderPotentialPlacement2.attachPiece.gridPlanes[builderPotentialPlacement2.attachIndex];
					BuilderAttachGridPlane builderAttachGridPlane2 = builderPotentialPlacement2.parentPiece.gridPlanes[builderPotentialPlacement2.parentAttachIndex];
					BuilderAttachGridPlane movingParentGrid = builderAttachGridPlane.GetMovingParentGrid();
					bool flag2 = movingParentGrid != null;
					BuilderAttachGridPlane movingParentGrid2 = builderAttachGridPlane2.GetMovingParentGrid();
					bool flag3 = movingParentGrid2 != null;
					if (flag2 == flag3 && (!flag2 || !(movingParentGrid != movingParentGrid2)))
					{
						SnapOverlap snapOverlap = this.builderPool.CreateSnapOverlap(builderAttachGridPlane2, builderPotentialPlacement2.attachBounds);
						builderAttachGridPlane.AddSnapOverlap(snapOverlap);
						SnapOverlap snapOverlap2 = this.builderPool.CreateSnapOverlap(builderAttachGridPlane, builderPotentialPlacement2.parentAttachBounds);
						builderAttachGridPlane2.AddSnapOverlap(snapOverlap2);
					}
				}
				piece.transform.SetLocalPositionAndRotation(zero, identity);
				if (piece2 != null && piece2.state == BuilderPiece.State.GrabbedLocal)
				{
					BuilderPiece rootPiece2 = piece2.GetRootPiece();
					BuilderPieceInteractor.instance.OnCountChangedForRoot(rootPiece2);
				}
				if (piece2 == null)
				{
					piece.SetActivateTimeStamp(action.timeStamp);
					piece.SetState(BuilderPiece.State.AttachedAndPlaced, false);
					this.SetIsDirty(true);
					if (flag)
					{
						BuilderPieceInteractor.instance.DisableCollisionsWithHands();
						return;
					}
				}
				else
				{
					if (piece2.isArmShelf || piece2.state == BuilderPiece.State.AttachedToArm)
					{
						piece.SetState(BuilderPiece.State.AttachedToArm, false);
						return;
					}
					if (piece2.isBuiltIntoTable || piece2.state == BuilderPiece.State.AttachedAndPlaced)
					{
						piece.SetActivateTimeStamp(action.timeStamp);
						piece.SetState(BuilderPiece.State.AttachedAndPlaced, false);
						if (piece2 != null)
						{
							BuilderPiece attachedBuiltInPiece = piece2.GetAttachedBuiltInPiece();
							BuilderPiecePrivatePlot builderPiecePrivatePlot;
							if (attachedBuiltInPiece != null && attachedBuiltInPiece.TryGetPlotComponent(out builderPiecePrivatePlot))
							{
								builderPiecePrivatePlot.OnPieceAttachedToPlot(piece);
							}
						}
						this.SetIsDirty(true);
						if (flag)
						{
							BuilderPieceInteractor.instance.DisableCollisionsWithHands();
							return;
						}
					}
					else
					{
						if (piece2.state == BuilderPiece.State.Grabbed)
						{
							piece.SetState(BuilderPiece.State.Grabbed, false);
							return;
						}
						if (piece2.state == BuilderPiece.State.GrabbedLocal)
						{
							piece.SetState(BuilderPiece.State.GrabbedLocal, false);
							return;
						}
						piece.SetState(BuilderPiece.State.AttachedToDropped, false);
						return;
					}
				}
				break;
			}
			case BuilderActionType.DetachFromPiece:
			{
				BuilderPiece builderPiece2 = piece;
				bool flag4 = piece.state == BuilderPiece.State.GrabbedLocal;
				if (flag4)
				{
					builderPiece2 = piece.GetRootPiece();
				}
				if (piece.state == BuilderPiece.State.AttachedAndPlaced)
				{
					this.SetIsDirty(true);
					BuilderPiece attachedBuiltInPiece2 = piece.GetAttachedBuiltInPiece();
					BuilderPiecePrivatePlot builderPiecePrivatePlot2;
					if (attachedBuiltInPiece2 != null && attachedBuiltInPiece2.TryGetPlotComponent(out builderPiecePrivatePlot2))
					{
						builderPiecePrivatePlot2.OnPieceDetachedFromPlot(piece);
					}
				}
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				piece.transform.localScale = Vector3.one;
				if (flag4)
				{
					BuilderPieceInteractor.instance.OnCountChangedForRoot(builderPiece2);
					return;
				}
				break;
			}
			case BuilderActionType.MakePieceRoot:
				BuilderPiece.MakePieceRoot(piece);
				return;
			case BuilderActionType.DropPiece:
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				piece.transform.localScale = Vector3.one;
				piece.SetState(BuilderPiece.State.Dropped, false);
				piece.transform.SetLocalPositionAndRotation(action.localPosition, action.localRotation);
				if (piece.rigidBody != null)
				{
					piece.rigidBody.position = action.localPosition;
					piece.rigidBody.rotation = action.localRotation;
					piece.rigidBody.linearVelocity = action.velocity;
					piece.rigidBody.angularVelocity = action.angVelocity;
					return;
				}
				break;
			case BuilderActionType.AttachToShelf:
			{
				piece.ClearParentHeld();
				piece.ClearParentPiece(false);
				int attachIndex = action.attachIndex;
				bool isLeftHand = action.isLeftHand;
				int parentAttachIndex = action.parentAttachIndex;
				float x = action.velocity.x;
				piece.transform.localScale = Vector3.one;
				piece.SetState(isLeftHand ? BuilderPiece.State.OnConveyor : BuilderPiece.State.OnShelf, false);
				if (isLeftHand)
				{
					if (attachIndex >= 0 && attachIndex < this.conveyors.Count)
					{
						BuilderConveyor builderConveyor = this.conveyors[attachIndex];
						float num2 = x / builderConveyor.GetFrameMovement();
						if (PhotonNetwork.ServerTimestamp >= parentAttachIndex)
						{
							uint num3 = (uint)(PhotonNetwork.ServerTimestamp - parentAttachIndex);
							num2 += num3 / 1000f;
						}
						piece.shelfOwner = attachIndex;
						builderConveyor.OnShelfPieceCreated(piece, num2);
						return;
					}
				}
				else
				{
					if (attachIndex >= 0 && attachIndex < this.dispenserShelves.Count)
					{
						BuilderDispenserShelf builderDispenserShelf = this.dispenserShelves[attachIndex];
						piece.shelfOwner = attachIndex;
						builderDispenserShelf.OnShelfPieceCreated(piece, false);
						return;
					}
					piece.transform.SetLocalPositionAndRotation(action.localPosition, action.localRotation);
				}
				break;
			}
			default:
				return;
			}
		}

		public static bool AreStatesCompatibleForOverlap(BuilderPiece.State stateA, BuilderPiece.State stateB, BuilderPiece rootA, BuilderPiece rootB)
		{
			switch (stateA)
			{
			case BuilderPiece.State.None:
				return false;
			case BuilderPiece.State.AttachedAndPlaced:
				return stateB == BuilderPiece.State.AttachedAndPlaced;
			case BuilderPiece.State.AttachedToDropped:
			case BuilderPiece.State.Dropped:
			case BuilderPiece.State.OnShelf:
			case BuilderPiece.State.OnConveyor:
				return (stateB == BuilderPiece.State.AttachedToDropped || stateB == BuilderPiece.State.Dropped || stateB == BuilderPiece.State.OnShelf || stateB == BuilderPiece.State.OnConveyor) && rootA.Equals(rootB);
			case BuilderPiece.State.Grabbed:
				return stateB == BuilderPiece.State.Grabbed && rootA.Equals(rootB);
			case BuilderPiece.State.Displayed:
				return false;
			case BuilderPiece.State.GrabbedLocal:
				return stateB == BuilderPiece.State.GrabbedLocal && rootA.heldInLeftHand == rootB.heldInLeftHand;
			case BuilderPiece.State.AttachedToArm:
			{
				if (stateB != BuilderPiece.State.AttachedToArm)
				{
					return false;
				}
				object obj = ((rootA.parentPiece != null) ? rootA.parentPiece : rootA);
				BuilderPiece builderPiece = ((rootB.parentPiece != null) ? rootB.parentPiece : rootB);
				return obj.Equals(builderPiece);
			}
			default:
				return false;
			}
		}

		public int CurrentSaveSlot
		{
			get
			{
				return this.currentSaveSlot;
			}
			set
			{
				if (this.saveInProgress)
				{
					return;
				}
				if (!BuilderScanKiosk.IsSaveSlotValid(value))
				{
					this.currentSaveSlot = -1;
				}
				if (this.currentSaveSlot != value)
				{
					this.SetIsDirty(true);
				}
				this.currentSaveSlot = value;
			}
		}

		private void Awake()
		{
			if (BuilderTable.zoneToInstance == null)
			{
				BuilderTable.zoneToInstance = new Dictionary<GTZone, BuilderTable>(2);
			}
			if (!BuilderTable.zoneToInstance.TryAdd(this.tableZone, this))
			{
				Object.Destroy(this);
			}
			this.acceptableSqrDistFromCenter = Mathf.Pow(217f * this.pieceScale, 2f);
			if (this.buttonSnapRotation != null)
			{
				this.buttonSnapRotation.Setup(new Action<BuilderOptionButton, bool>(this.OnButtonFreeRotation));
				this.buttonSnapRotation.SetPressed(this.useSnapRotation);
			}
			if (this.buttonSnapPosition != null)
			{
				this.buttonSnapPosition.Setup(new Action<BuilderOptionButton, bool>(this.OnButtonFreePosition));
				this.buttonSnapPosition.SetPressed(this.usePlacementStyle > BuilderPlacementStyle.Float);
			}
			if (this.buttonSaveLayout != null)
			{
				this.buttonSaveLayout.Setup(new Action<BuilderOptionButton, bool>(this.OnButtonSaveLayout));
			}
			if (this.buttonClearLayout != null)
			{
				this.buttonClearLayout.Setup(new Action<BuilderOptionButton, bool>(this.OnButtonClearLayout));
			}
			this.isSetup = false;
			this.nextPieceId = 10000;
			BuilderTable.placedLayer = LayerMask.NameToLayer("Gorilla Object");
			BuilderTable.heldLayerLocal = LayerMask.NameToLayer("Prop");
			BuilderTable.heldLayer = LayerMask.NameToLayer("BuilderProp");
			BuilderTable.droppedLayer = LayerMask.NameToLayer("BuilderProp");
			this.currSnapParams = this.pushAndEaseParams;
			this.tableState = BuilderTable.TableState.WaitingForZoneAndRoom;
			this.inRoom = false;
			this.inBuilderZone = false;
			this.builderNetworking.SetTable(this);
			this.plotOwners = new Dictionary<int, int>(10);
			this.doesLocalPlayerOwnPlot = false;
			this.queuedBuildCommands = new List<BuilderTable.BuilderCommand>(1028);
			if (this.isTableMutable)
			{
				this.playerToArmShelfLeft = new Dictionary<int, int>(10);
				this.playerToArmShelfRight = new Dictionary<int, int>(10);
				this.rollBackBufferedCommands = new List<BuilderTable.BuilderCommand>(1028);
				this.rollBackActions = new List<BuilderAction>(1028);
				this.rollForwardCommands = new List<BuilderTable.BuilderCommand>(1028);
				this.droppedPieces = new List<BuilderPiece>(BuilderTable.DROPPED_PIECE_LIMIT + 50);
				this.droppedPieceData = new List<BuilderTable.DroppedPieceData>(BuilderTable.DROPPED_PIECE_LIMIT + 50);
				this.SetupMonkeBlocksRoom();
				this.gridPlaneData = new NativeList<BuilderGridPlaneData>(1024, Allocator.Persistent);
				this.checkGridPlaneData = new NativeList<BuilderGridPlaneData>(1024, Allocator.Persistent);
				this.nearbyPiecesResults = new NativeArray<ColliderHit>(1024, Allocator.Persistent, NativeArrayOptions.ClearMemory);
				this.nearbyPiecesCommands = new NativeArray<OverlapSphereCommand>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
				this.allPotentialPlacements = new List<BuilderPotentialPlacement>(1024);
			}
			else
			{
				this.rollBackBufferedCommands = new List<BuilderTable.BuilderCommand>(128);
				this.rollBackActions = new List<BuilderAction>(128);
				this.rollForwardCommands = new List<BuilderTable.BuilderCommand>(128);
			}
			this.SetupResources();
			if (!this.isTableMutable && this.linkedTerminal != null)
			{
				this.linkedTerminal.Init(this);
			}
		}

		private void OnEnable()
		{
			TickSystem<object>.AddTickCallback(this);
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveTickCallback(this);
		}

		public static bool TryGetBuilderTableForZone(GTZone zone, out BuilderTable table)
		{
			if (BuilderTable.zoneToInstance == null)
			{
				table = null;
				return false;
			}
			return BuilderTable.zoneToInstance.TryGetValue(zone, out table);
		}

		private void SetupMonkeBlocksRoom()
		{
			if (this.shelves == null)
			{
				this.shelves = new List<BuilderShelf>(64);
			}
			if (this.shelvesRoot != null)
			{
				this.shelvesRoot.GetComponentsInChildren<BuilderShelf>(this.shelves);
			}
			this.conveyors = new List<BuilderConveyor>(32);
			this.dispenserShelves = new List<BuilderDispenserShelf>(32);
			if (this.allShelvesRoot != null)
			{
				for (int i = 0; i < this.allShelvesRoot.Count; i++)
				{
					this.allShelvesRoot[i].GetComponentsInChildren<BuilderConveyor>(BuilderTable.tempConveyors);
					this.conveyors.AddRange(BuilderTable.tempConveyors);
					BuilderTable.tempConveyors.Clear();
					this.allShelvesRoot[i].GetComponentsInChildren<BuilderDispenserShelf>(BuilderTable.tempDispensers);
					this.dispenserShelves.AddRange(BuilderTable.tempDispensers);
					BuilderTable.tempDispensers.Clear();
				}
			}
			this.recyclers = new List<BuilderRecycler>(5);
			if (this.recyclerRoot != null)
			{
				for (int j = 0; j < this.recyclerRoot.Count; j++)
				{
					this.recyclerRoot[j].GetComponentsInChildren<BuilderRecycler>(BuilderTable.tempRecyclers);
					this.recyclers.AddRange(BuilderTable.tempRecyclers);
					BuilderTable.tempRecyclers.Clear();
				}
			}
			for (int k = 0; k < this.recyclers.Count; k++)
			{
				this.recyclers[k].recyclerID = k;
				this.recyclers[k].table = this;
			}
			this.dropZones = new List<BuilderDropZone>(6);
			this.dropZoneRoot.GetComponentsInChildren<BuilderDropZone>(this.dropZones);
			for (int l = 0; l < this.dropZones.Count; l++)
			{
				this.dropZones[l].dropZoneID = l;
				this.dropZones[l].table = this;
			}
			foreach (BuilderResourceMeter builderResourceMeter in this.resourceMeters)
			{
				builderResourceMeter.table = this;
			}
		}

		private void SetupResources()
		{
			this.maxResources = new int[3];
			if (this.totalResources != null && this.totalResources.quantities != null)
			{
				for (int i = 0; i < this.totalResources.quantities.Count; i++)
				{
					if (this.totalResources.quantities[i].type >= BuilderResourceType.Basic && this.totalResources.quantities[i].type < BuilderResourceType.Count)
					{
						this.maxResources[(int)this.totalResources.quantities[i].type] += this.totalResources.quantities[i].count;
					}
				}
			}
			this.usedResources = new int[3];
			this.reservedResources = new int[3];
			if (this.totalReservedResources != null && this.totalReservedResources.quantities != null)
			{
				for (int j = 0; j < this.totalReservedResources.quantities.Count; j++)
				{
					if (this.totalReservedResources.quantities[j].type >= BuilderResourceType.Basic && this.totalReservedResources.quantities[j].type < BuilderResourceType.Count)
					{
						this.reservedResources[(int)this.totalReservedResources.quantities[j].type] += this.totalReservedResources.quantities[j].count;
					}
				}
			}
			this.plotMaxResources = new int[3];
			if (this.resourcesPerPrivatePlot != null && this.resourcesPerPrivatePlot.quantities != null)
			{
				for (int k = 0; k < this.resourcesPerPrivatePlot.quantities.Count; k++)
				{
					if (this.resourcesPerPrivatePlot.quantities[k].type >= BuilderResourceType.Basic && this.resourcesPerPrivatePlot.quantities[k].type < BuilderResourceType.Count)
					{
						this.plotMaxResources[(int)this.resourcesPerPrivatePlot.quantities[k].type] += this.resourcesPerPrivatePlot.quantities[k].count;
					}
				}
			}
			this.OnAvailableResourcesChange();
		}

		private void Start()
		{
			if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom != this.inRoom)
			{
				this.SetInRoom(NetworkSystem.Instance.InRoom);
			}
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.HandleOnZoneChanged));
			this.HandleOnZoneChanged();
			this.RequestTableConfiguration();
			this.FetchSharedBlocksStartingMapConfig();
			PlayFabTitleDataCache.Instance.OnTitleDataUpdate.AddListener(new UnityAction<string>(this.OnTitleDataUpdate));
		}

		private void OnApplicationQuit()
		{
			this.ClearTable();
			this.tableState = BuilderTable.TableState.WaitingForZoneAndRoom;
		}

		private void OnDestroy()
		{
			PlayFabTitleDataCache.Instance.OnTitleDataUpdate.RemoveListener(new UnityAction<string>(this.OnTitleDataUpdate));
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.HandleOnZoneChanged));
			if (this.isTableMutable)
			{
				if (this.gridPlaneData.IsCreated)
				{
					this.gridPlaneData.Dispose();
				}
				if (this.checkGridPlaneData.IsCreated)
				{
					this.checkGridPlaneData.Dispose();
				}
				if (this.nearbyPiecesResults.IsCreated)
				{
					this.nearbyPiecesResults.Dispose();
				}
				if (this.nearbyPiecesCommands.IsCreated)
				{
					this.nearbyPiecesCommands.Dispose();
				}
			}
			this.DestroyData();
		}

		private void HandleOnZoneChanged()
		{
			bool flag = ZoneManagement.instance.IsZoneActive(this.tableZone);
			this.SetInBuilderZone(flag);
		}

		public void InitIfNeeded()
		{
			if (!this.isSetup)
			{
				if (BuilderSetManager.instance == null)
				{
					return;
				}
				BuilderSetManager.instance.InitPieceDictionary();
				this.builderRenderer.BuildRenderer(BuilderSetManager.pieceList);
				this.baseGridPlanes.Clear();
				this.basePieces = new List<BuilderPiece>(1024);
				for (int i = 0; i < this.builtInPieceRoots.Count; i++)
				{
					this.builtInPieceRoots[i].SetActive(true);
					this.builtInPieceRoots[i].GetComponentsInChildren<BuilderPiece>(false, BuilderTable.tempPieces);
					this.basePieces.AddRange(BuilderTable.tempPieces);
				}
				this.allPrivatePlots = new List<BuilderPiecePrivatePlot>(20);
				this.CreateData();
				for (int j = 0; j < this.basePieces.Count; j++)
				{
					BuilderPiece builderPiece = this.basePieces[j];
					builderPiece.SetTable(this);
					builderPiece.pieceId = 5 + j;
					builderPiece.SetScale(this.pieceScale);
					builderPiece.SetupPiece(this.gridSize);
					builderPiece.OnCreate();
					builderPiece.SetState(BuilderPiece.State.OnShelf, true);
					this.baseGridPlanes.AddRange(builderPiece.gridPlanes);
					BuilderPiecePrivatePlot builderPiecePrivatePlot;
					if (builderPiece.IsPrivatePlot() && builderPiece.TryGetPlotComponent(out builderPiecePrivatePlot))
					{
						this.allPrivatePlots.Add(builderPiecePrivatePlot);
					}
					this.AddPieceData(builderPiece);
				}
				this.builderPool = BuilderPool.instance;
				this.builderPool.Setup();
				base.StartCoroutine(this.builderPool.BuildFromPieceSets());
				if (this.isTableMutable)
				{
					for (int k = 0; k < this.conveyors.Count; k++)
					{
						this.conveyors[k].table = this;
						this.conveyors[k].shelfID = k;
						this.conveyors[k].Setup();
					}
					for (int l = 0; l < this.dispenserShelves.Count; l++)
					{
						this.dispenserShelves[l].table = this;
						this.dispenserShelves[l].shelfID = l;
						this.dispenserShelves[l].Setup();
					}
					this.conveyorManager.Setup(this);
					this.repelledPieceRoots = new HashSet<int>[this.repelHistoryLength];
					for (int m = 0; m < this.repelHistoryLength; m++)
					{
						this.repelledPieceRoots[m] = new HashSet<int>(10);
					}
					this.sharedBuildAreas = this.sharedBuildArea.GetComponents<BoxCollider>();
					BoxCollider[] array = this.sharedBuildAreas;
					for (int n = 0; n < array.Length; n++)
					{
						array[n].enabled = false;
					}
					this.sharedBuildArea.SetActive(false);
				}
				BoxCollider[] components = this.noBlocksArea.GetComponents<BoxCollider>();
				this.noBlocksAreas = new List<BuilderTable.BoxCheckParams>(components.Length);
				foreach (BoxCollider boxCollider in components)
				{
					boxCollider.enabled = true;
					BuilderTable.BoxCheckParams boxCheckParams = new BuilderTable.BoxCheckParams
					{
						center = boxCollider.transform.TransformPoint(boxCollider.center),
						halfExtents = Vector3.Scale(boxCollider.transform.lossyScale, boxCollider.size) / 2f,
						rotation = boxCollider.transform.rotation
					};
					this.noBlocksAreas.Add(boxCheckParams);
					boxCollider.enabled = false;
				}
				this.noBlocksArea.SetActive(false);
				this.isSetup = true;
			}
		}

		private void SetIsDirty(bool dirty)
		{
			if (this.isDirty != dirty)
			{
				UnityEvent<bool> onSaveDirtyChanged = this.OnSaveDirtyChanged;
				if (onSaveDirtyChanged != null)
				{
					onSaveDirtyChanged.Invoke(dirty);
				}
			}
			this.isDirty = dirty;
		}

		private void FixedUpdate()
		{
			if (this.tableState != BuilderTable.TableState.Ready && this.tableState != BuilderTable.TableState.WaitForMasterResync)
			{
				return;
			}
			foreach (IBuilderPieceFunctional builderPieceFunctional in this.funcComponentsToRegisterFixed)
			{
				if (builderPieceFunctional != null)
				{
					this.fixedUpdateFunctionalComponents.Add(builderPieceFunctional);
				}
			}
			foreach (IBuilderPieceFunctional builderPieceFunctional2 in this.funcComponentsToUnregisterFixed)
			{
				this.fixedUpdateFunctionalComponents.Remove(builderPieceFunctional2);
			}
			this.funcComponentsToRegisterFixed.Clear();
			this.funcComponentsToUnregisterFixed.Clear();
			foreach (IBuilderPieceFunctional builderPieceFunctional3 in this.fixedUpdateFunctionalComponents)
			{
				builderPieceFunctional3.FunctionalPieceFixedUpdate();
			}
		}

		public void Tick()
		{
			this.RunUpdate();
		}

		private void RunUpdate()
		{
			this.InitIfNeeded();
			this.UpdateTableState();
			if (this.isTableMutable)
			{
				this.UpdateDroppedPieces(Time.deltaTime);
				this.repelHistoryIndex = (this.repelHistoryIndex + 1) % this.repelHistoryLength;
				int num = (this.repelHistoryIndex + 1) % this.repelHistoryLength;
				this.repelledPieceRoots[num].Clear();
			}
		}

		public void AddQueuedCommand(BuilderTable.BuilderCommand cmd)
		{
			this.queuedBuildCommands.Add(cmd);
		}

		public void ClearQueuedCommands()
		{
			if (this.queuedBuildCommands != null)
			{
				this.queuedBuildCommands.Clear();
			}
			this.RemoveRollBackActions();
			if (this.rollBackBufferedCommands != null)
			{
				this.rollBackBufferedCommands.Clear();
			}
			this.RemoveRollForwardCommands();
		}

		public int GetNumQueuedCommands()
		{
			if (this.queuedBuildCommands != null)
			{
				return this.queuedBuildCommands.Count;
			}
			return 0;
		}

		public void AddRollbackAction(BuilderAction action)
		{
			this.rollBackActions.Add(action);
		}

		public void RemoveRollBackActions()
		{
			this.rollBackActions.Clear();
		}

		public void RemoveRollBackActions(int localCommandId)
		{
			for (int i = this.rollBackActions.Count - 1; i >= 0; i--)
			{
				if (localCommandId == -1 || this.rollBackActions[i].localCommandId == localCommandId)
				{
					this.rollBackActions.RemoveAt(i);
				}
			}
		}

		public bool HasRollBackActionsForCommand(int localCommandId)
		{
			for (int i = 0; i < this.rollBackActions.Count; i++)
			{
				if (this.rollBackActions[i].localCommandId == localCommandId)
				{
					return true;
				}
			}
			return false;
		}

		public void AddRollForwardCommand(BuilderTable.BuilderCommand command)
		{
			this.rollForwardCommands.Add(command);
		}

		public void RemoveRollForwardCommands()
		{
			this.rollForwardCommands.Clear();
		}

		public void RemoveRollForwardCommands(int localCommandId)
		{
			for (int i = this.rollForwardCommands.Count - 1; i >= 0; i--)
			{
				if (localCommandId == -1 || this.rollForwardCommands[i].localCommandId == localCommandId)
				{
					this.rollForwardCommands.RemoveAt(i);
				}
			}
		}

		public bool HasRollForwardCommand(int localCommandId)
		{
			for (int i = 0; i < this.rollForwardCommands.Count; i++)
			{
				if (this.rollForwardCommands[i].localCommandId == localCommandId)
				{
					return true;
				}
			}
			return false;
		}

		public bool ShouldRollbackBufferCommand(BuilderTable.BuilderCommand cmd)
		{
			return cmd.type != BuilderTable.BuilderCommandType.Create && cmd.type != BuilderTable.BuilderCommandType.CreateArmShelf && this.rollBackActions.Count > 0 && (cmd.player == null || !cmd.player.IsLocal || !this.HasRollForwardCommand(cmd.localCommandId));
		}

		public void AddRollbackBufferedCommand(BuilderTable.BuilderCommand bufferedCmd)
		{
			this.rollBackBufferedCommands.Add(bufferedCmd);
		}

		private void ExecuteRollBackActions()
		{
			for (int i = this.rollBackActions.Count - 1; i >= 0; i--)
			{
				this.ExecuteAction(this.rollBackActions[i]);
			}
			this.rollBackActions.Clear();
		}

		private void ExecuteRollbackBufferedCommands()
		{
			for (int i = 0; i < this.rollBackBufferedCommands.Count; i++)
			{
				BuilderTable.BuilderCommand builderCommand = this.rollBackBufferedCommands[i];
				builderCommand.isQueued = false;
				builderCommand.canRollback = false;
				this.ExecuteBuildCommand(builderCommand);
			}
			this.rollBackBufferedCommands.Clear();
		}

		private void ExecuteRollForwardCommands()
		{
			BuilderTable.tempRollForwardCommands.Clear();
			for (int i = 0; i < this.rollForwardCommands.Count; i++)
			{
				BuilderTable.tempRollForwardCommands.Add(this.rollForwardCommands[i]);
			}
			this.rollForwardCommands.Clear();
			for (int j = 0; j < BuilderTable.tempRollForwardCommands.Count; j++)
			{
				BuilderTable.BuilderCommand builderCommand = BuilderTable.tempRollForwardCommands[j];
				builderCommand.isQueued = true;
				builderCommand.canRollback = true;
				this.ExecuteBuildCommand(builderCommand);
			}
			BuilderTable.tempRollForwardCommands.Clear();
		}

		private void UpdateRollForwardCommandData()
		{
			for (int i = 0; i < this.rollForwardCommands.Count; i++)
			{
				BuilderTable.BuilderCommand builderCommand = this.rollForwardCommands[i];
				if (builderCommand.type == BuilderTable.BuilderCommandType.Drop)
				{
					BuilderPiece piece = this.GetPiece(builderCommand.pieceId);
					if (piece != null && piece.rigidBody != null)
					{
						builderCommand.localPosition = piece.rigidBody.position;
						builderCommand.localRotation = piece.rigidBody.rotation;
						builderCommand.velocity = piece.rigidBody.linearVelocity;
						builderCommand.angVelocity = piece.rigidBody.angularVelocity;
						this.rollForwardCommands[i] = builderCommand;
					}
				}
			}
		}

		public bool TryRollbackAndReExecute(int localCommandId)
		{
			if (this.HasRollBackActionsForCommand(localCommandId))
			{
				if (this.rollBackBufferedCommands.Count > 0)
				{
					this.UpdateRollForwardCommandData();
					this.ExecuteRollBackActions();
					this.ExecuteRollbackBufferedCommands();
					this.ExecuteRollForwardCommands();
					this.RemoveRollBackActions(localCommandId);
					this.RemoveRollForwardCommands(localCommandId);
				}
				else
				{
					this.RemoveRollBackActions(localCommandId);
					this.RemoveRollForwardCommands(localCommandId);
				}
				return true;
			}
			return false;
		}

		public void RollbackFailedCommand(int localCommandId)
		{
			if (this.HasRollBackActionsForCommand(localCommandId))
			{
				this.UpdateRollForwardCommandData();
				this.ExecuteRollBackActions();
				this.ExecuteRollbackBufferedCommands();
				this.RemoveRollForwardCommands(-1);
				this.ExecuteRollForwardCommands();
			}
		}

		public BuilderTable.TableState GetTableState()
		{
			return this.tableState;
		}

		public void SetTableState(BuilderTable.TableState newState)
		{
			this.InitIfNeeded();
			if (newState == this.tableState)
			{
				return;
			}
			BuilderTable.TableState tableState = this.tableState;
			this.tableState = newState;
			switch (this.tableState)
			{
			case BuilderTable.TableState.WaitingForInitalBuild:
				if (!this.isTableMutable && !NetworkSystem.Instance.IsMasterClient)
				{
					this.sharedBlocksMap = null;
					UnityEvent onMapCleared = this.OnMapCleared;
					if (onMapCleared == null)
					{
						return;
					}
					onMapCleared.Invoke();
					return;
				}
				break;
			case BuilderTable.TableState.ReceivingInitialBuild:
			case BuilderTable.TableState.ReceivingMasterResync:
			case BuilderTable.TableState.InitialBuild:
			case BuilderTable.TableState.ExecuteQueuedCommands:
				break;
			case BuilderTable.TableState.WaitForInitialBuildMaster:
				this.nextPieceId = 10000;
				if (this.isTableMutable)
				{
					this.BuildInitialTableForPlayer();
					return;
				}
				this.BuildSelectedSharedMap();
				return;
			case BuilderTable.TableState.WaitForMasterResync:
				this.ClearQueuedCommands();
				this.ResetConveyors();
				return;
			case BuilderTable.TableState.Ready:
				this.OnAvailableResourcesChange();
				if (!this.isTableMutable)
				{
					string text = ((this.sharedBlocksMap == null) ? "" : this.sharedBlocksMap.MapID);
					UnityEvent<string> onMapLoaded = this.OnMapLoaded;
					if (onMapLoaded != null)
					{
						onMapLoaded.Invoke(text);
					}
					this.SetPendingMap(null);
					return;
				}
				break;
			case BuilderTable.TableState.BadData:
				this.ClearTable();
				this.ClearQueuedCommands();
				break;
			case BuilderTable.TableState.WaitingForSharedMapLoad:
				this.ClearTable();
				this.ClearQueuedCommands();
				this.builderNetworking.ResetSerializedTableForAllPlayers();
				return;
			default:
				return;
			}
		}

		public void SetPendingMap(string mapID)
		{
			this.pendingMapID = mapID;
		}

		public string GetPendingMap()
		{
			return this.pendingMapID;
		}

		public string GetCurrentMapID()
		{
			SharedBlocksManager.SharedBlocksMap sharedBlocksMap = this.sharedBlocksMap;
			if (sharedBlocksMap == null)
			{
				return null;
			}
			return sharedBlocksMap.MapID;
		}

		public void LoadSharedMap(SharedBlocksManager.SharedBlocksMap map)
		{
			if (NetworkSystem.Instance.InRoom)
			{
				if (map.MapID.IsNullOrEmpty())
				{
					GTDev.LogWarning<string>("Invalid map to load", null);
					UnityEvent<string> onMapLoadFailed = this.OnMapLoadFailed;
					if (onMapLoadFailed == null)
					{
						return;
					}
					onMapLoadFailed.Invoke("Invalid Map ID");
					return;
				}
				else
				{
					if (this.tableState == BuilderTable.TableState.Ready || this.tableState == BuilderTable.TableState.BadData)
					{
						this.builderNetworking.RequestLoadSharedBlocksMap(map.MapID);
						return;
					}
					UnityEvent<string> onMapLoadFailed2 = this.OnMapLoadFailed;
					if (onMapLoadFailed2 == null)
					{
						return;
					}
					onMapLoadFailed2.Invoke("WAIT FOR LOAD IN PROGRESS");
					return;
				}
			}
			else
			{
				UnityEvent<string> onMapLoadFailed3 = this.OnMapLoadFailed;
				if (onMapLoadFailed3 == null)
				{
					return;
				}
				onMapLoadFailed3.Invoke("Not In Room");
				return;
			}
		}

		public void SetInRoom(bool inRoom)
		{
			this.inRoom = inRoom;
			bool flag = inRoom && this.inBuilderZone;
			if (!inRoom)
			{
				this.pendingMapID = null;
				this.sharedBlocksMap = null;
				UnityEvent onMapCleared = this.OnMapCleared;
				if (onMapCleared != null)
				{
					onMapCleared.Invoke();
				}
			}
			if (flag && this.tableState == BuilderTable.TableState.WaitingForZoneAndRoom)
			{
				this.SetTableState(BuilderTable.TableState.WaitingForInitalBuild);
				this.builderNetworking.PlayerEnterBuilder();
				return;
			}
			if (!flag && this.tableState != BuilderTable.TableState.WaitingForZoneAndRoom && !this.builderNetworking.IsPrivateMasterClient())
			{
				this.SetTableState(BuilderTable.TableState.WaitingForZoneAndRoom);
				this.builderNetworking.PlayerExitBuilder();
				return;
			}
			if (flag && PhotonNetwork.IsMasterClient && this.isTableMutable)
			{
				this.builderNetworking.RequestCreateArmShelfForPlayer(PhotonNetwork.LocalPlayer);
				return;
			}
			if (!flag && this.builderNetworking.IsPrivateMasterClient() && this.isTableMutable)
			{
				this.RemoveArmShelfForPlayer(PhotonNetwork.LocalPlayer);
			}
		}

		public static bool IsLocalPlayerInBuilderZone()
		{
			GorillaTagger instance = GorillaTagger.Instance;
			ZoneEntity zoneEntity;
			if (instance == null)
			{
				zoneEntity = null;
			}
			else
			{
				VRRig offlineVRRig = instance.offlineVRRig;
				zoneEntity = ((offlineVRRig != null) ? offlineVRRig.zoneEntity : null);
			}
			ZoneEntity zoneEntity2 = zoneEntity;
			BuilderTable builderTable;
			return !(zoneEntity2 == null) && BuilderTable.TryGetBuilderTableForZone(zoneEntity2.currentZone, out builderTable) && builderTable.IsInBuilderZone();
		}

		public bool IsInBuilderZone()
		{
			return this.inBuilderZone;
		}

		public void SetInBuilderZone(bool inBuilderZone)
		{
			this.inBuilderZone = inBuilderZone;
			this.ShowPieces(inBuilderZone);
			bool flag = this.inRoom && inBuilderZone;
			if (flag && this.tableState == BuilderTable.TableState.WaitingForZoneAndRoom)
			{
				this.SetTableState(BuilderTable.TableState.WaitingForInitalBuild);
				this.builderNetworking.PlayerEnterBuilder();
				return;
			}
			if (!flag && this.tableState != BuilderTable.TableState.WaitingForZoneAndRoom && !this.builderNetworking.IsPrivateMasterClient())
			{
				this.SetTableState(BuilderTable.TableState.WaitingForZoneAndRoom);
				this.builderNetworking.PlayerExitBuilder();
				return;
			}
			if (flag && PhotonNetwork.IsMasterClient)
			{
				this.builderNetworking.RequestCreateArmShelfForPlayer(PhotonNetwork.LocalPlayer);
				return;
			}
			if (!flag && this.builderNetworking.IsPrivateMasterClient())
			{
				this.RemoveArmShelfForPlayer(PhotonNetwork.LocalPlayer);
			}
		}

		private void ShowPieces(bool show)
		{
			if (this.builderRenderer != null)
			{
				this.builderRenderer.Show(show);
			}
			if (this.pieces == null || this.basePieces == null)
			{
				return;
			}
			for (int i = 0; i < this.pieces.Count; i++)
			{
				this.pieces[i].SetDirectRenderersVisible(show);
			}
			for (int j = 0; j < this.basePieces.Count; j++)
			{
				this.basePieces[j].SetDirectRenderersVisible(show);
			}
		}

		private void UpdateTableState()
		{
			switch (this.tableState)
			{
			case BuilderTable.TableState.InitialBuild:
			{
				BuilderTableNetworking.PlayerTableInitState localTableInit = this.builderNetworking.GetLocalTableInit();
				try
				{
					this.ClearTable();
					this.ClearQueuedCommands();
					byte[] array = GZipStream.UncompressBuffer(localTableInit.serializedTableState);
					localTableInit.totalSerializedBytes = array.Length;
					Array.Copy(array, 0, localTableInit.serializedTableState, 0, localTableInit.totalSerializedBytes);
					this.DeserializeTableState(localTableInit.serializedTableState, localTableInit.numSerializedBytes);
					if (this.tableState == BuilderTable.TableState.BadData)
					{
						return;
					}
					this.SetTableState(BuilderTable.TableState.ExecuteQueuedCommands);
					this.SetIsDirty(true);
					return;
				}
				catch (Exception)
				{
					this.SetTableState(BuilderTable.TableState.BadData);
					return;
				}
				break;
			}
			case BuilderTable.TableState.ExecuteQueuedCommands:
				break;
			case BuilderTable.TableState.Ready:
			{
				JobHandle jobHandle = default(JobHandle);
				if (this.isTableMutable)
				{
					this.conveyorManager.UpdateManager();
					jobHandle = this.conveyorManager.ConstructJobHandle();
					JobHandle.ScheduleBatchedJobs();
					foreach (BuilderDispenserShelf builderDispenserShelf in this.dispenserShelves)
					{
						builderDispenserShelf.UpdateShelf();
					}
					foreach (BuilderPiecePrivatePlot builderPiecePrivatePlot in this.allPrivatePlots)
					{
						builderPiecePrivatePlot.UpdatePlot();
					}
					foreach (BuilderRecycler builderRecycler in this.recyclers)
					{
						builderRecycler.UpdateRecycler();
					}
					for (int i = this.shelfSliceUpdateIndex; i < this.dispenserShelves.Count; i += BuilderTable.SHELF_SLICE_BUCKETS)
					{
						this.dispenserShelves[i].UpdateShelfSliced();
					}
					this.shelfSliceUpdateIndex = (this.shelfSliceUpdateIndex + 1) % BuilderTable.SHELF_SLICE_BUCKETS;
				}
				foreach (IBuilderPieceFunctional builderPieceFunctional in this.funcComponentsToRegister)
				{
					if (builderPieceFunctional != null)
					{
						this.activeFunctionalComponents.Add(builderPieceFunctional);
					}
				}
				foreach (IBuilderPieceFunctional builderPieceFunctional2 in this.funcComponentsToUnregister)
				{
					this.activeFunctionalComponents.Remove(builderPieceFunctional2);
				}
				this.funcComponentsToRegister.Clear();
				this.funcComponentsToUnregister.Clear();
				foreach (IBuilderPieceFunctional builderPieceFunctional3 in this.activeFunctionalComponents)
				{
					if (builderPieceFunctional3 != null)
					{
						builderPieceFunctional3.FunctionalPieceUpdate();
					}
				}
				if (this.isTableMutable)
				{
					foreach (BuilderResourceMeter builderResourceMeter in this.resourceMeters)
					{
						builderResourceMeter.UpdateMeterFill();
					}
					this.CleanUpDroppedPiece();
					jobHandle.Complete();
					return;
				}
				return;
			}
			default:
				return;
			}
			for (int j = 0; j < this.queuedBuildCommands.Count; j++)
			{
				BuilderTable.BuilderCommand builderCommand = this.queuedBuildCommands[j];
				builderCommand.isQueued = true;
				this.ExecuteBuildCommand(builderCommand);
			}
			this.queuedBuildCommands.Clear();
			this.SetTableState(BuilderTable.TableState.Ready);
		}

		private void RouteNewCommand(BuilderTable.BuilderCommand cmd, bool force)
		{
			bool flag = this.ShouldExecuteCommand();
			if (force)
			{
				this.ExecuteBuildCommand(cmd);
				return;
			}
			if (flag && this.ShouldRollbackBufferCommand(cmd))
			{
				this.AddRollbackBufferedCommand(cmd);
				return;
			}
			if (flag)
			{
				this.ExecuteBuildCommand(cmd);
				return;
			}
			if (this.ShouldQueueCommand())
			{
				this.AddQueuedCommand(cmd);
				return;
			}
			this.ShouldDiscardCommand();
		}

		private void ExecuteBuildCommand(BuilderTable.BuilderCommand cmd)
		{
			if (!this.isTableMutable && cmd.type != BuilderTable.BuilderCommandType.FunctionalStateChange)
			{
				return;
			}
			switch (cmd.type)
			{
			case BuilderTable.BuilderCommandType.Create:
				this.ExecutePieceCreated(cmd);
				return;
			case BuilderTable.BuilderCommandType.Place:
				this.ExecutePiecePlacedWithActions(cmd);
				return;
			case BuilderTable.BuilderCommandType.Grab:
				this.ExecutePieceGrabbedWithActions(cmd);
				return;
			case BuilderTable.BuilderCommandType.Drop:
				this.ExecutePieceDroppedWithActions(cmd);
				return;
			case BuilderTable.BuilderCommandType.Remove:
				break;
			case BuilderTable.BuilderCommandType.Paint:
				this.ExecutePiecePainted(cmd);
				return;
			case BuilderTable.BuilderCommandType.Recycle:
				this.ExecutePieceRecycled(cmd);
				return;
			case BuilderTable.BuilderCommandType.ClaimPlot:
				this.ExecuteClaimPlot(cmd);
				return;
			case BuilderTable.BuilderCommandType.FreePlot:
				this.ExecuteFreePlot(cmd);
				return;
			case BuilderTable.BuilderCommandType.CreateArmShelf:
				this.ExecuteArmShelfCreated(cmd);
				return;
			case BuilderTable.BuilderCommandType.PlayerLeftRoom:
				this.ExecutePlayerLeftRoom(cmd);
				return;
			case BuilderTable.BuilderCommandType.FunctionalStateChange:
				this.ExecuteSetFunctionalPieceState(cmd);
				return;
			case BuilderTable.BuilderCommandType.SetSelection:
				this.ExecuteSetSelection(cmd);
				return;
			case BuilderTable.BuilderCommandType.Repel:
				this.ExecutePieceRepelled(cmd);
				break;
			default:
				return;
			}
		}

		public void ClearTable()
		{
			this.ClearTableInternal();
		}

		private void ClearTableInternal()
		{
			BuilderTable.tempDeletePieces.Clear();
			for (int i = 0; i < this.pieces.Count; i++)
			{
				BuilderTable.tempDeletePieces.Add(this.pieces[i]);
			}
			if (this.isTableMutable)
			{
				this.droppedPieces.Clear();
				this.droppedPieceData.Clear();
			}
			for (int j = 0; j < BuilderTable.tempDeletePieces.Count; j++)
			{
				BuilderTable.tempDeletePieces[j].ClearParentPiece(false);
				BuilderTable.tempDeletePieces[j].ClearParentHeld();
				BuilderTable.tempDeletePieces[j].SetState(BuilderPiece.State.None, false);
				this.RemovePiece(BuilderTable.tempDeletePieces[j]);
			}
			for (int k = 0; k < BuilderTable.tempDeletePieces.Count; k++)
			{
				this.builderPool.DestroyPiece(BuilderTable.tempDeletePieces[k]);
			}
			BuilderTable.tempDeletePieces.Clear();
			this.pieces.Clear();
			this.pieceIDToIndexCache.Clear();
			this.nextPieceId = 10000;
			if (this.isTableMutable)
			{
				this.conveyorManager.OnClearTable();
				foreach (BuilderDispenserShelf builderDispenserShelf in this.dispenserShelves)
				{
					builderDispenserShelf.OnClearTable();
				}
				for (int l = 0; l < this.repelHistoryLength; l++)
				{
					this.repelledPieceRoots[l].Clear();
				}
			}
			this.funcComponentsToRegister.Clear();
			this.funcComponentsToUnregister.Clear();
			this.activeFunctionalComponents.Clear();
			foreach (BuilderPiece builderPiece in this.basePieces)
			{
				foreach (BuilderAttachGridPlane builderAttachGridPlane in builderPiece.gridPlanes)
				{
					builderAttachGridPlane.OnReturnToPool(this.builderPool);
				}
			}
			if (this.isTableMutable)
			{
				this.ClearBuiltInPlots();
				this.playerToArmShelfLeft.Clear();
				this.playerToArmShelfRight.Clear();
				if (BuilderPieceInteractor.instance != null)
				{
					BuilderPieceInteractor.instance.RemovePiecesFromHands();
				}
			}
		}

		private void ClearBuiltInPlots()
		{
			foreach (BuilderPiecePrivatePlot builderPiecePrivatePlot in this.allPrivatePlots)
			{
				builderPiecePrivatePlot.ClearPlot();
			}
			this.plotOwners.Clear();
			this.SetLocalPlayerOwnsPlot(false);
		}

		private void OnDeserializeUpdatePlots()
		{
			foreach (BuilderPiecePrivatePlot builderPiecePrivatePlot in this.allPrivatePlots)
			{
				builderPiecePrivatePlot.RecountPlotCost();
			}
		}

		public void BuildPiecesOnShelves()
		{
			if (!this.isTableMutable)
			{
				return;
			}
			if (this.shelves == null)
			{
				return;
			}
			for (int i = 0; i < this.shelves.Count; i++)
			{
				if (this.shelves[i] != null)
				{
					this.shelves[i].Init();
				}
			}
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int j = 0; j < this.shelves.Count; j++)
				{
					if (this.shelves[j].HasOpenSlot())
					{
						this.shelves[j].BuildNextPiece(this);
						if (this.shelves[j].HasOpenSlot())
						{
							flag = true;
						}
					}
				}
			}
		}

		private void OnFinishedInitialTableBuild()
		{
			this.BuildPiecesOnShelves();
			this.SetTableState(BuilderTable.TableState.Ready);
			this.CreateArmShelvesForPlayersInBuilder();
		}

		public int CreatePieceId()
		{
			int num = this.nextPieceId;
			if (this.nextPieceId == 2147483647)
			{
				this.nextPieceId = 20000;
			}
			this.nextPieceId++;
			return num;
		}

		public void ResetConveyors()
		{
			if (this.isTableMutable)
			{
				foreach (BuilderConveyor builderConveyor in this.conveyors)
				{
					builderConveyor.ResetConveyorState();
				}
			}
		}

		public void RequestCreateConveyorPiece(int newPieceType, int materialType, int shelfID)
		{
			if (shelfID < 0 || shelfID >= this.conveyors.Count)
			{
				return;
			}
			BuilderConveyor builderConveyor = this.conveyors[shelfID];
			if (builderConveyor == null)
			{
				return;
			}
			Transform spawnTransform = builderConveyor.GetSpawnTransform();
			this.builderNetworking.CreateShelfPiece(newPieceType, spawnTransform.position, spawnTransform.rotation, materialType, BuilderPiece.State.OnConveyor, shelfID);
		}

		public void RequestCreateDispenserShelfPiece(int pieceType, Vector3 position, Quaternion rotation, int materialType, int shelfID)
		{
			if (shelfID < 0 || shelfID >= this.dispenserShelves.Count)
			{
				return;
			}
			if (this.dispenserShelves[shelfID] == null)
			{
				return;
			}
			this.builderNetworking.CreateShelfPiece(pieceType, position, rotation, materialType, BuilderPiece.State.OnShelf, shelfID);
		}

		public void CreateConveyorPiece(int pieceType, int pieceId, Vector3 position, Quaternion rotation, int materialType, int shelfID, int sendTimestamp)
		{
			if (shelfID < 0 || shelfID >= this.conveyors.Count)
			{
				return;
			}
			if (this.conveyors[shelfID] == null)
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Create,
				pieceType = pieceType,
				pieceId = pieceId,
				localPosition = position,
				localRotation = rotation,
				materialType = materialType,
				state = BuilderPiece.State.OnConveyor,
				parentPieceId = shelfID,
				parentAttachIndex = sendTimestamp,
				player = NetworkSystem.Instance.MasterClient
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void CreateDispenserShelfPiece(int pieceType, int pieceId, Vector3 position, Quaternion rotation, int materialType, int shelfID)
		{
			if (shelfID < 0 || shelfID >= this.dispenserShelves.Count)
			{
				return;
			}
			if (this.dispenserShelves[shelfID] == null)
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Create,
				pieceType = pieceType,
				pieceId = pieceId,
				localPosition = position,
				localRotation = rotation,
				materialType = materialType,
				state = BuilderPiece.State.OnShelf,
				parentPieceId = shelfID,
				isLeft = true,
				player = NetworkSystem.Instance.MasterClient
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void RequestShelfSelection(int shelfId, int groupID, bool isConveyor)
		{
			if (this.tableState != BuilderTable.TableState.Ready)
			{
				return;
			}
			this.builderNetworking.RequestShelfSelection(shelfId, groupID, isConveyor);
		}

		public void VerifySetSelections()
		{
			if (!this.isTableMutable)
			{
				return;
			}
			foreach (BuilderConveyor builderConveyor in this.conveyors)
			{
				builderConveyor.VerifySetSelection();
			}
			foreach (BuilderDispenserShelf builderDispenserShelf in this.dispenserShelves)
			{
				builderDispenserShelf.VerifySetSelection();
			}
		}

		public bool ValidateShelfSelectionParams(int shelfId, int displayGroupID, bool isConveyor, Player player)
		{
			bool flag = shelfId >= 0 && ((isConveyor && shelfId < this.conveyors.Count) || (!isConveyor && shelfId < this.dispenserShelves.Count)) && BuilderSetManager.instance.DoesPlayerOwnDisplayGroup(player, displayGroupID);
			if (PhotonNetwork.IsMasterClient)
			{
				if (isConveyor)
				{
					BuilderConveyor builderConveyor = this.conveyors[shelfId];
					bool flag2 = this.IsPlayerHandNearAction(NetPlayer.Get(player), builderConveyor.transform.position, false, true, 4f);
					flag = flag && flag2;
				}
				else
				{
					BuilderDispenserShelf builderDispenserShelf = this.dispenserShelves[shelfId];
					bool flag3 = this.IsPlayerHandNearAction(NetPlayer.Get(player), builderDispenserShelf.transform.position, false, true, 4f);
					flag = flag && flag3;
				}
			}
			return flag;
		}

		private void SetConveyorSelection(int conveyorId, int setId)
		{
			BuilderConveyor builderConveyor = this.conveyors[conveyorId];
			if (builderConveyor == null)
			{
				return;
			}
			builderConveyor.SetSelection(setId);
		}

		private void SetDispenserSelection(int conveyorId, int setId)
		{
			BuilderDispenserShelf builderDispenserShelf = this.dispenserShelves[conveyorId];
			if (builderDispenserShelf == null)
			{
				return;
			}
			builderDispenserShelf.SetSelection(setId);
		}

		public void ChangeSetSelection(int shelfID, int setID, bool isConveyor)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.SetSelection,
				parentPieceId = shelfID,
				pieceType = setID,
				isLeft = isConveyor,
				player = NetworkSystem.Instance.MasterClient
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void ExecuteSetSelection(BuilderTable.BuilderCommand cmd)
		{
			bool isLeft = cmd.isLeft;
			int parentPieceId = cmd.parentPieceId;
			int pieceType = cmd.pieceType;
			if (isLeft)
			{
				this.SetConveyorSelection(parentPieceId, pieceType);
				return;
			}
			this.SetDispenserSelection(parentPieceId, pieceType);
		}

		public bool ValidateFunctionalPieceState(int pieceID, byte state, NetPlayer player)
		{
			BuilderPiece piece = this.GetPiece(pieceID);
			return !(piece == null) && piece.functionalPieceComponent != null && (!NetworkSystem.Instance.IsMasterClient || player.IsMasterClient || this.IsPlayerHandNearAction(player, piece.transform.position, true, false, piece.functionalPieceComponent.GetInteractionDistace())) && piece.functionalPieceComponent.IsStateValid(state);
		}

		public void OnFunctionalStateRequest(int pieceID, byte state, NetPlayer player, int timeStamp)
		{
			BuilderPiece piece = this.GetPiece(pieceID);
			if (piece == null)
			{
				return;
			}
			if (piece.functionalPieceComponent == null)
			{
				return;
			}
			if (player == null)
			{
				return;
			}
			piece.functionalPieceComponent.OnStateRequest(state, player, timeStamp);
		}

		public void SetFunctionalPieceState(int pieceID, byte state, NetPlayer player, int timeStamp)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.FunctionalStateChange,
				pieceId = pieceID,
				twist = state,
				player = player,
				serverTimeStamp = timeStamp
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void ExecuteSetFunctionalPieceState(BuilderTable.BuilderCommand cmd)
		{
			BuilderPiece piece = this.GetPiece(cmd.pieceId);
			if (piece == null)
			{
				return;
			}
			piece.SetFunctionalPieceState(cmd.twist, cmd.player, cmd.serverTimeStamp);
		}

		public void RegisterFunctionalPiece(IBuilderPieceFunctional component)
		{
			if (component != null)
			{
				this.funcComponentsToRegister.Add(component);
			}
		}

		public void UnregisterFunctionalPiece(IBuilderPieceFunctional component)
		{
			if (component != null)
			{
				this.funcComponentsToUnregister.Add(component);
			}
		}

		public void RegisterFunctionalPieceFixedUpdate(IBuilderPieceFunctional component)
		{
			if (component != null)
			{
				this.funcComponentsToRegisterFixed.Add(component);
			}
		}

		public void UnregisterFunctionalPieceFixedUpdate(IBuilderPieceFunctional component)
		{
			if (component != null)
			{
				this.funcComponentsToRegisterFixed.Remove(component);
			}
		}

		public void RequestCreatePiece(int newPieceType, Vector3 position, Quaternion rotation, int materialType)
		{
		}

		public void CreatePiece(int pieceType, int pieceId, Vector3 position, Quaternion rotation, int materialType, BuilderPiece.State state, Player player)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Create,
				pieceType = pieceType,
				pieceId = pieceId,
				localPosition = position,
				localRotation = rotation,
				materialType = materialType,
				state = state,
				player = NetPlayer.Get(player)
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void RequestRecyclePiece(BuilderPiece piece, bool playFX, int recyclerID)
		{
			this.builderNetworking.RequestRecyclePiece(piece.pieceId, piece.transform.position, piece.transform.rotation, playFX, recyclerID);
		}

		public void RecyclePiece(int pieceId, Vector3 position, Quaternion rotation, bool playFX, int recyclerID, Player player)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Recycle,
				pieceId = pieceId,
				localPosition = position,
				localRotation = rotation,
				player = NetPlayer.Get(player),
				isLeft = playFX,
				parentPieceId = recyclerID
			};
			this.RouteNewCommand(builderCommand, false);
		}

		private bool ShouldExecuteCommand()
		{
			return this.tableState == BuilderTable.TableState.Ready || this.tableState == BuilderTable.TableState.WaitForInitialBuildMaster;
		}

		private bool ShouldQueueCommand()
		{
			return this.tableState == BuilderTable.TableState.ReceivingInitialBuild || this.tableState == BuilderTable.TableState.ReceivingMasterResync || this.tableState == BuilderTable.TableState.InitialBuild || this.tableState == BuilderTable.TableState.ExecuteQueuedCommands;
		}

		private bool ShouldDiscardCommand()
		{
			return this.tableState == BuilderTable.TableState.WaitingForInitalBuild || this.tableState == BuilderTable.TableState.WaitForInitialBuildMaster || this.tableState == BuilderTable.TableState.WaitingForZoneAndRoom;
		}

		public bool DoesChainContainPiece(BuilderPiece targetPiece, BuilderPiece firstInChain, BuilderPiece nextInChain)
		{
			return !(targetPiece == null) && !(firstInChain == null) && (targetPiece.Equals(firstInChain) || (!(nextInChain == null) && (targetPiece.Equals(nextInChain) || (!(firstInChain == nextInChain) && this.DoesChainContainPiece(targetPiece, firstInChain, nextInChain.parentPiece)))));
		}

		public bool DoesChainContainChain(BuilderPiece chainARoot, BuilderPiece chainBAttachPiece)
		{
			if (chainARoot == null || chainBAttachPiece == null)
			{
				return false;
			}
			if (this.DoesChainContainPiece(chainARoot, chainBAttachPiece, chainBAttachPiece.parentPiece))
			{
				return true;
			}
			BuilderPiece builderPiece = chainARoot.firstChildPiece;
			while (builderPiece != null)
			{
				if (this.DoesChainContainChain(builderPiece, chainBAttachPiece))
				{
					return true;
				}
				builderPiece = builderPiece.nextSiblingPiece;
			}
			return false;
		}

		private bool IsPlayerHandNearAction(NetPlayer player, Vector3 worldPosition, bool isLeftHand, bool checkBothHands, float acceptableRadius = 2.5f)
		{
			bool flag = true;
			RigContainer rigContainer;
			if (player != null && VRRigCache.Instance != null && VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
			{
				if (isLeftHand || checkBothHands)
				{
					flag = (worldPosition - rigContainer.Rig.leftHandTransform.position).sqrMagnitude < acceptableRadius * acceptableRadius;
				}
				if (!isLeftHand || checkBothHands)
				{
					float sqrMagnitude = (worldPosition - rigContainer.Rig.rightHandTransform.position).sqrMagnitude;
					flag = flag && sqrMagnitude < acceptableRadius * acceptableRadius;
				}
			}
			return flag;
		}

		public bool ValidatePlacePieceParams(int pieceId, int attachPieceId, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, int parentPieceId, int attachIndex, int parentAttachIndex, NetPlayer placedByPlayer)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return false;
			}
			BuilderPiece piece2 = this.GetPiece(attachPieceId);
			if (piece2 == null)
			{
				return false;
			}
			if (piece.heldByPlayerActorNumber != placedByPlayer.ActorNumber)
			{
				return false;
			}
			if (piece.isBuiltIntoTable || piece2.isBuiltIntoTable)
			{
				return false;
			}
			if (twist > 3)
			{
				return false;
			}
			BuilderPiece piece3 = this.GetPiece(parentPieceId);
			if (!(piece3 != null))
			{
				return false;
			}
			if (!BuilderPiece.CanPlayerAttachPieceToPiece(placedByPlayer.ActorNumber, piece2, piece3))
			{
				return false;
			}
			if (this.DoesChainContainChain(piece2, piece3))
			{
				return false;
			}
			if (attachIndex < 0 || attachIndex >= piece2.gridPlanes.Count)
			{
				return false;
			}
			if (piece3 != null && (parentAttachIndex < 0 || parentAttachIndex >= piece3.gridPlanes.Count))
			{
				return false;
			}
			if (piece3 != null)
			{
				bool flag = (long)(twist % 2) == 1L;
				BuilderAttachGridPlane builderAttachGridPlane = piece2.gridPlanes[attachIndex];
				int num = (flag ? builderAttachGridPlane.length : builderAttachGridPlane.width);
				int num2 = (flag ? builderAttachGridPlane.width : builderAttachGridPlane.length);
				BuilderAttachGridPlane builderAttachGridPlane2 = piece3.gridPlanes[parentAttachIndex];
				int num3 = Mathf.FloorToInt((float)builderAttachGridPlane2.width / 2f);
				int num4 = num3 - (builderAttachGridPlane2.width - 1);
				if ((int)bumpOffsetX < num4 - num || (int)bumpOffsetX > num3 + num)
				{
					return false;
				}
				int num5 = Mathf.FloorToInt((float)builderAttachGridPlane2.length / 2f);
				int num6 = num5 - (builderAttachGridPlane2.length - 1);
				if ((int)bumpOffsetZ < num6 - num2 || (int)bumpOffsetZ > num5 + num2)
				{
					return false;
				}
			}
			if (placedByPlayer == null)
			{
				return false;
			}
			if (PhotonNetwork.IsMasterClient && piece3 != null)
			{
				Vector3 vector;
				Quaternion quaternion;
				Vector3 vector2;
				Quaternion quaternion2;
				piece2.BumpTwistToPositionRotation(twist, bumpOffsetX, bumpOffsetZ, attachIndex, piece3.gridPlanes[parentAttachIndex], out vector, out quaternion, out vector2, out quaternion2);
				Vector3 vector3 = piece2.transform.InverseTransformPoint(piece.transform.position);
				Vector3 vector4 = vector2 + quaternion2 * vector3;
				if (!this.IsPlayerHandNearAction(placedByPlayer, vector4, piece.heldInLeftHand, false, 2.5f))
				{
					return false;
				}
				if (!this.ValidatePieceWorldTransform(vector2, quaternion2))
				{
					return false;
				}
			}
			return true;
		}

		public bool ValidatePlacePieceState(int pieceId, int attachPieceId, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, int parentPieceId, int attachIndex, int parentAttachIndex, Player placedByPlayer)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return false;
			}
			BuilderPiece piece2 = this.GetPiece(attachPieceId);
			return !(piece2 == null) && !(this.GetPiece(parentPieceId) == null) && placedByPlayer != null && !piece2.GetRootPiece() != piece;
		}

		public void ExecutePieceCreated(BuilderTable.BuilderCommand cmd)
		{
			if ((cmd.player == null || !cmd.player.IsLocal) && !this.ValidateCreatePieceParams(cmd.pieceType, cmd.pieceId, cmd.state, cmd.materialType))
			{
				return;
			}
			BuilderPiece builderPiece = this.CreatePieceInternal(cmd.pieceType, cmd.pieceId, cmd.localPosition, cmd.localRotation, cmd.state, cmd.materialType, 0, this);
			if (!(builderPiece != null) || cmd.state != BuilderPiece.State.OnConveyor)
			{
				if (builderPiece != null && cmd.isLeft && cmd.state == BuilderPiece.State.OnShelf)
				{
					if (cmd.parentPieceId < 0 || cmd.parentPieceId >= this.dispenserShelves.Count)
					{
						return;
					}
					builderPiece.shelfOwner = cmd.parentPieceId;
					this.dispenserShelves[builderPiece.shelfOwner].OnShelfPieceCreated(builderPiece, true);
				}
				return;
			}
			if (cmd.parentPieceId < 0 || cmd.parentPieceId >= this.conveyors.Count)
			{
				return;
			}
			builderPiece.shelfOwner = cmd.parentPieceId;
			BuilderConveyor builderConveyor = this.conveyors[builderPiece.shelfOwner];
			int parentAttachIndex = cmd.parentAttachIndex;
			float num = 0f;
			if (PhotonNetwork.ServerTimestamp > parentAttachIndex)
			{
				num = (PhotonNetwork.ServerTimestamp - parentAttachIndex) / 1000f;
			}
			builderConveyor.OnShelfPieceCreated(builderPiece, num);
		}

		public void ExecutePieceRecycled(BuilderTable.BuilderCommand cmd)
		{
			this.RecyclePieceInternal(cmd.pieceId, false, cmd.isLeft, cmd.parentPieceId);
		}

		private bool ValidateCreatePieceParams(int newPieceType, int newPieceId, BuilderPiece.State state, int materialType)
		{
			return !(this.GetPiecePrefab(newPieceType) == null) && !(this.GetPiece(newPieceId) != null);
		}

		private bool ValidateDeserializedRootPieceState(int pieceId, BuilderPiece.State state, int shelfOwner, int heldByActor, Vector3 localPosition, Quaternion localRotation)
		{
			switch (state)
			{
			case BuilderPiece.State.Grabbed:
			case BuilderPiece.State.GrabbedLocal:
				if (heldByActor == -1)
				{
					return false;
				}
				if (!this.isTableMutable)
				{
					GTDev.LogError<string>(string.Format("Deserialized bad CreatePiece parameters. held piece in immutable table {0}", pieceId), null);
				}
				else if (localPosition.sqrMagnitude > 6.25f)
				{
					return false;
				}
				break;
			case BuilderPiece.State.Dropped:
				if (!this.ValidatePieceWorldTransform(localPosition, localRotation))
				{
					return false;
				}
				if (!this.isTableMutable)
				{
					GTDev.LogError<string>(string.Format("Deserialized bad CreatePiece parameters. dropped piece in immutable table {0}", pieceId), null);
					return false;
				}
				break;
			case BuilderPiece.State.OnShelf:
			case BuilderPiece.State.Displayed:
				if (shelfOwner == -1 && !this.ValidatePieceWorldTransform(localPosition, localRotation))
				{
					return false;
				}
				break;
			case BuilderPiece.State.OnConveyor:
				if (shelfOwner == -1)
				{
					return false;
				}
				if (!this.isTableMutable)
				{
					GTDev.LogError<string>(string.Format("Deserialized bad CreatePiece parameters. OnConveyor piece in immutable table {0}", pieceId), null);
					return false;
				}
				break;
			case BuilderPiece.State.AttachedToArm:
				if (heldByActor == -1)
				{
					return false;
				}
				if (!this.isTableMutable)
				{
					GTDev.LogError<string>(string.Format("Deserialized bad CreatePiece parameters. AttachedToArm piece in immutable table {0}", pieceId), null);
					return false;
				}
				if (localPosition.sqrMagnitude > 6.25f)
				{
					return false;
				}
				break;
			default:
				return false;
			}
			return true;
		}

		private bool ValidateDeserializedChildPieceState(int pieceId, BuilderPiece.State state)
		{
			switch (state)
			{
			case BuilderPiece.State.AttachedAndPlaced:
			case BuilderPiece.State.OnShelf:
			case BuilderPiece.State.Displayed:
				return true;
			case BuilderPiece.State.AttachedToDropped:
			case BuilderPiece.State.Grabbed:
			case BuilderPiece.State.GrabbedLocal:
			case BuilderPiece.State.AttachedToArm:
				if (!this.isTableMutable)
				{
					GTDev.LogError<string>(string.Format("Deserialized bad CreatePiece parameters. Invalid state {0} of child piece {1} in Immutable table", state, pieceId), null);
					return false;
				}
				return true;
			}
			return false;
		}

		public bool ValidatePieceWorldTransform(Vector3 position, Quaternion rotation)
		{
			float num = 10000f;
			return (in position).IsValid(in num) && (in rotation).IsValid() && (this.roomCenter.position - position).sqrMagnitude <= this.acceptableSqrDistFromCenter;
		}

		private BuilderPiece CreatePieceInternal(int newPieceType, int newPieceId, Vector3 position, Quaternion rotation, BuilderPiece.State state, int materialType, int activateTimeStamp, BuilderTable table)
		{
			if (this.GetPiecePrefab(newPieceType) == null)
			{
				return null;
			}
			if (!PhotonNetwork.IsMasterClient)
			{
				this.nextPieceId = newPieceId + 1;
			}
			BuilderPiece builderPiece = this.builderPool.CreatePiece(newPieceType, false);
			builderPiece.SetScale(table.pieceScale);
			builderPiece.transform.SetPositionAndRotation(position, rotation);
			builderPiece.pieceType = newPieceType;
			builderPiece.pieceId = newPieceId;
			builderPiece.SetTable(table);
			builderPiece.gameObject.SetActive(true);
			builderPiece.SetupPiece(this.gridSize);
			builderPiece.OnCreate();
			builderPiece.activatedTimeStamp = ((state == BuilderPiece.State.AttachedAndPlaced) ? activateTimeStamp : 0);
			builderPiece.SetMaterial(materialType, true);
			builderPiece.SetState(state, true);
			this.AddPiece(builderPiece);
			return builderPiece;
		}

		private void RecyclePieceInternal(int pieceId, bool ignoreHaptics, bool playFX, int recyclerId)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return;
			}
			if (playFX)
			{
				try
				{
					piece.PlayRecycleFx();
				}
				catch (Exception)
				{
				}
			}
			if (!ignoreHaptics)
			{
				BuilderPiece rootPiece = piece.GetRootPiece();
				if (rootPiece != null && rootPiece.IsHeldLocal())
				{
					GorillaTagger.Instance.StartVibration(piece.IsHeldInLeftHand(), GorillaTagger.Instance.tapHapticStrength, this.pushAndEaseParams.snapDelayTime * 2f);
				}
			}
			BuilderPiece builderPiece = piece.firstChildPiece;
			while (builderPiece != null)
			{
				int pieceId2 = builderPiece.pieceId;
				builderPiece = builderPiece.nextSiblingPiece;
				this.RecyclePieceInternal(pieceId2, true, playFX, recyclerId);
			}
			if (this.isTableMutable && recyclerId >= 0 && recyclerId < this.recyclers.Count)
			{
				this.recyclers[recyclerId].OnRecycleRequestedAtRecycler(piece);
			}
			if (piece.state == BuilderPiece.State.OnConveyor && piece.shelfOwner >= 0 && piece.shelfOwner < this.conveyors.Count)
			{
				this.conveyors[piece.shelfOwner].OnShelfPieceRecycled(piece);
			}
			else if ((piece.state == BuilderPiece.State.OnShelf || piece.state == BuilderPiece.State.Displayed) && piece.shelfOwner >= 0 && piece.shelfOwner < this.dispenserShelves.Count)
			{
				this.dispenserShelves[piece.shelfOwner].OnShelfPieceRecycled(piece);
			}
			if (piece.isArmShelf && this.isTableMutable)
			{
				if (piece.armShelf != null)
				{
					piece.armShelf.piece = null;
					piece.armShelf = null;
				}
				int num;
				if (piece.heldInLeftHand && this.playerToArmShelfLeft.TryGetValue(piece.heldByPlayerActorNumber, out num) && num == piece.pieceId)
				{
					this.playerToArmShelfLeft.Remove(piece.heldByPlayerActorNumber);
				}
				int num2;
				if (!piece.heldInLeftHand && this.playerToArmShelfRight.TryGetValue(piece.heldByPlayerActorNumber, out num2) && num2 == piece.pieceId)
				{
					this.playerToArmShelfRight.Remove(piece.heldByPlayerActorNumber);
				}
			}
			else if (PhotonNetwork.LocalPlayer.ActorNumber == piece.heldByPlayerActorNumber)
			{
				BuilderPieceInteractor.instance.RemovePieceFromHeld(piece);
			}
			int pieceId3 = piece.pieceId;
			piece.ClearParentPiece(false);
			piece.ClearParentHeld();
			piece.SetState(BuilderPiece.State.None, false);
			this.RemovePiece(piece);
			this.builderPool.DestroyPiece(piece);
		}

		public BuilderPiece GetPiecePrefab(int pieceType)
		{
			return BuilderSetManager.instance.GetPiecePrefab(pieceType);
		}

		private bool ValidateAttachPieceParams(int pieceId, int attachIndex, int parentId, int parentAttachIndex, int piecePlacement)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return false;
			}
			BuilderPiece piece2 = this.GetPiece(parentId);
			if (piece2 == null)
			{
				return false;
			}
			if ((piecePlacement & 262143) != piecePlacement)
			{
				return false;
			}
			if (piece.isBuiltIntoTable)
			{
				return false;
			}
			if (this.DoesChainContainChain(piece, piece2))
			{
				return false;
			}
			if (attachIndex < 0 || attachIndex >= piece.gridPlanes.Count)
			{
				return false;
			}
			if (parentAttachIndex < 0 || parentAttachIndex >= piece2.gridPlanes.Count)
			{
				return false;
			}
			byte b;
			sbyte b2;
			sbyte b3;
			BuilderTable.UnpackPiecePlacement(piecePlacement, out b, out b2, out b3);
			bool flag = (long)(b % 2) == 1L;
			BuilderAttachGridPlane builderAttachGridPlane = piece.gridPlanes[attachIndex];
			int num = (flag ? builderAttachGridPlane.length : builderAttachGridPlane.width);
			int num2 = (flag ? builderAttachGridPlane.width : builderAttachGridPlane.length);
			BuilderAttachGridPlane builderAttachGridPlane2 = piece2.gridPlanes[parentAttachIndex];
			int num3 = Mathf.FloorToInt((float)builderAttachGridPlane2.width / 2f);
			int num4 = num3 - (builderAttachGridPlane2.width - 1);
			if ((int)b2 < num4 - num || (int)b2 > num3 + num)
			{
				return false;
			}
			int num5 = Mathf.FloorToInt((float)builderAttachGridPlane2.length / 2f);
			int num6 = num5 - (builderAttachGridPlane2.length - 1);
			return (int)b3 >= num6 - num2 && (int)b3 <= num5 + num2;
		}

		private void AttachPieceInternal(int pieceId, int attachIndex, int parentId, int parentAttachIndex, int placement)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			BuilderPiece piece2 = this.GetPiece(parentId);
			if (piece == null)
			{
				return;
			}
			byte b;
			sbyte b2;
			sbyte b3;
			BuilderTable.UnpackPiecePlacement(placement, out b, out b2, out b3);
			Vector3 zero = Vector3.zero;
			Quaternion quaternion;
			if (piece2 != null && parentAttachIndex >= 0 && parentAttachIndex < piece2.gridPlanes.Count)
			{
				Vector3 vector;
				Quaternion quaternion2;
				piece.BumpTwistToPositionRotation(b, b2, b3, attachIndex, piece2.gridPlanes[parentAttachIndex], out zero, out quaternion, out vector, out quaternion2);
			}
			else
			{
				quaternion = Quaternion.Euler(0f, (float)b * 90f, 0f);
			}
			piece.SetParentPiece(attachIndex, piece2, parentAttachIndex);
			piece.transform.SetLocalPositionAndRotation(zero, quaternion);
		}

		private void AttachPieceToActorInternal(int pieceId, int actorNumber, bool isLeftHand)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(actorNumber);
			RigContainer rigContainer;
			if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
			{
				return;
			}
			VRRig rig = rigContainer.Rig;
			BodyDockPositions myBodyDockPositions = rig.myBodyDockPositions;
			Transform transform = (isLeftHand ? myBodyDockPositions.leftHandTransform : myBodyDockPositions.rightHandTransform);
			if (piece.isArmShelf)
			{
				if (!this.isTableMutable)
				{
					return;
				}
				transform = (isLeftHand ? rig.builderArmShelfLeft.pieceAnchor : rig.builderArmShelfRight.pieceAnchor);
				if (isLeftHand)
				{
					rig.builderArmShelfLeft.piece = piece;
					piece.armShelf = rig.builderArmShelfLeft;
					int num;
					if (this.playerToArmShelfLeft.TryGetValue(actorNumber, out num) && num != pieceId)
					{
						BuilderPiece piece2 = this.GetPiece(num);
						if (piece2 != null && piece2.isArmShelf)
						{
							piece2.ClearParentHeld();
							this.playerToArmShelfLeft.Remove(actorNumber);
						}
					}
					this.playerToArmShelfLeft.TryAdd(actorNumber, pieceId);
				}
				else
				{
					rig.builderArmShelfRight.piece = piece;
					piece.armShelf = rig.builderArmShelfRight;
					int num2;
					if (this.playerToArmShelfRight.TryGetValue(actorNumber, out num2) && num2 != pieceId)
					{
						BuilderPiece piece3 = this.GetPiece(num2);
						if (piece3 != null && piece3.isArmShelf)
						{
							piece3.ClearParentHeld();
							this.playerToArmShelfRight.Remove(actorNumber);
						}
					}
					this.playerToArmShelfRight.TryAdd(actorNumber, pieceId);
				}
			}
			Vector3 localPosition = piece.transform.localPosition;
			Quaternion localRotation = piece.transform.localRotation;
			piece.ClearParentHeld();
			piece.ClearParentPiece(false);
			piece.SetParentHeld(transform, actorNumber, isLeftHand);
			piece.transform.SetLocalPositionAndRotation(localPosition, localRotation);
			BuilderPiece.State state = (player.IsLocal ? BuilderPiece.State.GrabbedLocal : BuilderPiece.State.Grabbed);
			if (piece.isArmShelf)
			{
				state = BuilderPiece.State.AttachedToArm;
				piece.transform.localScale = Vector3.one;
			}
			piece.SetState(state, false);
			if (!player.IsLocal)
			{
				BuilderPieceInteractor.instance.RemovePieceFromHeld(piece);
			}
			if (player.IsLocal && !piece.isArmShelf)
			{
				BuilderPieceInteractor.instance.AddPieceToHeld(piece, isLeftHand, localPosition, localRotation);
			}
		}

		public void RequestPlacePiece(BuilderPiece piece, BuilderPiece attachPiece, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, BuilderPiece parentPiece, int attachIndex, int parentAttachIndex)
		{
			if (this.tableState != BuilderTable.TableState.Ready)
			{
				return;
			}
			this.builderNetworking.RequestPlacePiece(piece, attachPiece, bumpOffsetX, bumpOffsetZ, twist, parentPiece, attachIndex, parentAttachIndex);
		}

		public void PlacePiece(int localCommandId, int pieceId, int attachPieceId, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, int parentPieceId, int attachIndex, int parentAttachIndex, NetPlayer placedByPlayer, int timeStamp, bool force)
		{
			this.PiecePlacedInternal(localCommandId, pieceId, attachPieceId, bumpOffsetX, bumpOffsetZ, twist, parentPieceId, attachIndex, parentAttachIndex, placedByPlayer, timeStamp, force);
		}

		public void PiecePlacedInternal(int localCommandId, int pieceId, int attachPieceId, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, int parentPieceId, int attachIndex, int parentAttachIndex, NetPlayer placedByPlayer, int timeStamp, bool force)
		{
			if (!force && placedByPlayer == NetworkSystem.Instance.LocalPlayer && this.HasRollForwardCommand(localCommandId) && this.TryRollbackAndReExecute(localCommandId))
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Place,
				pieceId = pieceId,
				bumpOffsetX = bumpOffsetX,
				bumpOffsetZ = bumpOffsetZ,
				twist = twist,
				attachPieceId = attachPieceId,
				parentPieceId = parentPieceId,
				attachIndex = attachIndex,
				parentAttachIndex = parentAttachIndex,
				player = placedByPlayer,
				canRollback = force,
				localCommandId = localCommandId,
				serverTimeStamp = timeStamp
			};
			this.RouteNewCommand(builderCommand, force);
		}

		public void ExecutePiecePlacedWithActions(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			int attachPieceId = cmd.attachPieceId;
			int parentPieceId = cmd.parentPieceId;
			int parentAttachIndex = cmd.parentAttachIndex;
			int attachIndex = cmd.attachIndex;
			NetPlayer player = cmd.player;
			int localCommandId = cmd.localCommandId;
			int actorNumber = cmd.player.ActorNumber;
			byte twist = cmd.twist;
			sbyte bumpOffsetX = cmd.bumpOffsetX;
			sbyte bumpOffsetZ = cmd.bumpOffsetZ;
			if ((player == null || !player.IsLocal) && !this.ValidatePlacePieceParams(pieceId, attachPieceId, bumpOffsetX, bumpOffsetZ, twist, parentPieceId, attachIndex, parentAttachIndex, player))
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return;
			}
			BuilderPiece piece2 = this.GetPiece(attachPieceId);
			if (piece2 == null)
			{
				return;
			}
			BuilderAction builderAction = BuilderActions.CreateDetachFromPlayer(localCommandId, pieceId, actorNumber);
			BuilderAction builderAction2 = BuilderActions.CreateMakeRoot(localCommandId, attachPieceId);
			BuilderAction builderAction3 = BuilderActions.CreateAttachToPiece(localCommandId, attachPieceId, cmd.parentPieceId, cmd.attachIndex, cmd.parentAttachIndex, bumpOffsetX, bumpOffsetZ, twist, actorNumber, cmd.serverTimeStamp);
			if (cmd.canRollback)
			{
				BuilderAction builderAction4 = BuilderActions.CreateDetachFromPiece(localCommandId, attachPieceId, actorNumber);
				BuilderAction builderAction5 = BuilderActions.CreateMakeRoot(localCommandId, pieceId);
				BuilderAction builderAction6 = BuilderActions.CreateAttachToPlayerRollback(localCommandId, piece);
				this.AddRollbackAction(builderAction6);
				this.AddRollbackAction(builderAction5);
				this.AddRollbackAction(builderAction4);
				this.AddRollForwardCommand(cmd);
			}
			this.ExecuteAction(builderAction);
			this.ExecuteAction(builderAction2);
			this.ExecuteAction(builderAction3);
			if (!cmd.isQueued)
			{
				piece2.PlayPlacementFx();
			}
		}

		public bool ValidateGrabPieceParams(int pieceId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, NetPlayer grabbedByPlayer)
		{
			float num = 10000f;
			if (!(in localPosition).IsValid(in num) || !(in localRotation).IsValid())
			{
				return false;
			}
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return false;
			}
			if (piece.isBuiltIntoTable)
			{
				return false;
			}
			if (grabbedByPlayer == null)
			{
				return false;
			}
			if (!piece.CanPlayerGrabPiece(grabbedByPlayer.ActorNumber, piece.transform.position))
			{
				return false;
			}
			if (localPosition.sqrMagnitude > 6400f)
			{
				return false;
			}
			if (PhotonNetwork.IsMasterClient)
			{
				Vector3 position = piece.transform.position;
				if (!this.IsPlayerHandNearAction(grabbedByPlayer, position, isLeftHand, false, 2.5f))
				{
					return false;
				}
			}
			return true;
		}

		public bool ValidateGrabPieceState(int pieceId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, Player grabbedByPlayer)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			return !(piece == null) && piece.state != BuilderPiece.State.Displayed && piece.state != BuilderPiece.State.None;
		}

		public bool IsLocationWithinSharedBuildArea(Vector3 worldPosition)
		{
			Vector3 vector = this.sharedBuildArea.transform.InverseTransformPoint(worldPosition);
			foreach (BoxCollider boxCollider in this.sharedBuildAreas)
			{
				Vector3 vector2 = boxCollider.center + boxCollider.size / 2f;
				Vector3 vector3 = boxCollider.center - boxCollider.size / 2f;
				if (vector.x >= vector3.x && vector.x <= vector2.x && vector.y >= vector3.y && vector.y <= vector2.y && vector.z >= vector3.z && vector.z <= vector2.z)
				{
					return true;
				}
			}
			return false;
		}

		private bool NoBlocksCheck()
		{
			foreach (BuilderTable.BoxCheckParams boxCheckParams in this.noBlocksAreas)
			{
				DebugUtil.DrawBox(boxCheckParams.center, boxCheckParams.rotation, boxCheckParams.halfExtents * 2f, Color.magenta, true, DebugUtil.Style.Wireframe);
				int num = 0;
				num |= 1 << BuilderTable.placedLayer;
				int num2 = Physics.OverlapBoxNonAlloc(boxCheckParams.center, boxCheckParams.halfExtents, this.noBlocksCheckResults, boxCheckParams.rotation, num);
				for (int i = 0; i < num2; i++)
				{
					BuilderPiece builderPieceFromCollider = BuilderPiece.GetBuilderPieceFromCollider(this.noBlocksCheckResults[i]);
					if (builderPieceFromCollider != null && builderPieceFromCollider.GetTable() == this && builderPieceFromCollider.state == BuilderPiece.State.AttachedAndPlaced && !builderPieceFromCollider.isBuiltIntoTable)
					{
						GTDev.LogError<string>(string.Format("Builder Table found piece {0} {1} in NO BLOCK AREA {2}", builderPieceFromCollider.pieceId, builderPieceFromCollider.displayName, builderPieceFromCollider.transform.position), null);
						return false;
					}
				}
			}
			return true;
		}

		public void RequestGrabPiece(BuilderPiece piece, bool isLefHand, Vector3 localPosition, Quaternion localRotation)
		{
			if (this.tableState != BuilderTable.TableState.Ready)
			{
				return;
			}
			this.builderNetworking.RequestGrabPiece(piece, isLefHand, localPosition, localRotation);
		}

		public void GrabPiece(int localCommandId, int pieceId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, NetPlayer grabbedByPlayer, bool force)
		{
			this.PieceGrabbedInternal(localCommandId, pieceId, isLeftHand, localPosition, localRotation, grabbedByPlayer, force);
		}

		public void PieceGrabbedInternal(int localCommandId, int pieceId, bool isLeftHand, Vector3 localPosition, Quaternion localRotation, NetPlayer grabbedByPlayer, bool force)
		{
			if (!force && grabbedByPlayer == NetworkSystem.Instance.LocalPlayer && this.HasRollForwardCommand(localCommandId) && this.TryRollbackAndReExecute(localCommandId))
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Grab,
				pieceId = pieceId,
				attachPieceId = -1,
				isLeft = isLeftHand,
				localPosition = localPosition,
				localRotation = localRotation,
				player = grabbedByPlayer,
				canRollback = force,
				localCommandId = localCommandId
			};
			this.RouteNewCommand(builderCommand, force);
		}

		public void ExecutePieceGrabbedWithActions(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			bool isLeft = cmd.isLeft;
			NetPlayer player = cmd.player;
			Vector3 localPosition = cmd.localPosition;
			Quaternion localRotation = cmd.localRotation;
			int localCommandId = cmd.localCommandId;
			int actorNumber = cmd.player.ActorNumber;
			if ((player == null || !player.Equals(NetworkSystem.Instance.LocalPlayer)) && !this.ValidateGrabPieceParams(pieceId, isLeft, localPosition, localRotation, player))
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return;
			}
			bool flag = PhotonNetwork.CurrentRoom.GetPlayer(piece.heldByPlayerActorNumber, false) != null;
			bool flag2 = BuilderPiece.IsDroppedState(piece.state);
			bool flag3 = piece.state == BuilderPiece.State.OnConveyor || piece.state == BuilderPiece.State.OnShelf || piece.state == BuilderPiece.State.Displayed;
			BuilderAction builderAction = BuilderActions.CreateAttachToPlayer(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, actorNumber, cmd.isLeft);
			BuilderAction builderAction2 = BuilderActions.CreateDetachFromPlayer(localCommandId, pieceId, actorNumber);
			if (flag)
			{
				BuilderAction builderAction3 = BuilderActions.CreateDetachFromPlayer(localCommandId, pieceId, piece.heldByPlayerActorNumber);
				if (cmd.canRollback)
				{
					BuilderAction builderAction4 = BuilderActions.CreateAttachToPlayerRollback(localCommandId, piece);
					this.AddRollbackAction(builderAction4);
					this.AddRollbackAction(builderAction2);
					this.AddRollForwardCommand(cmd);
				}
				this.ExecuteAction(builderAction3);
				this.ExecuteAction(builderAction);
				return;
			}
			if (flag3)
			{
				BuilderAction builderAction5;
				if (piece.state == BuilderPiece.State.OnConveyor)
				{
					int serverTimestamp = PhotonNetwork.ServerTimestamp;
					float splineProgressForPiece = this.conveyorManager.GetSplineProgressForPiece(piece);
					builderAction5 = BuilderActions.CreateAttachToShelfRollback(localCommandId, piece, piece.shelfOwner, true, serverTimestamp, splineProgressForPiece);
				}
				else
				{
					if (piece.state == BuilderPiece.State.Displayed)
					{
						int actorNumber2 = NetworkSystem.Instance.LocalPlayer.ActorNumber;
					}
					builderAction5 = BuilderActions.CreateAttachToShelfRollback(localCommandId, piece, piece.shelfOwner, false, 0, 0f);
				}
				BuilderAction builderAction6 = BuilderActions.CreateMakeRoot(localCommandId, pieceId);
				BuilderPiece rootPiece = piece.GetRootPiece();
				BuilderAction builderAction7 = BuilderActions.CreateMakeRoot(localCommandId, rootPiece.pieceId);
				if (cmd.canRollback)
				{
					this.AddRollbackAction(builderAction5);
					this.AddRollbackAction(builderAction7);
					this.AddRollbackAction(builderAction2);
					this.AddRollForwardCommand(cmd);
				}
				this.ExecuteAction(builderAction6);
				this.ExecuteAction(builderAction);
				return;
			}
			if (flag2)
			{
				BuilderAction builderAction8 = BuilderActions.CreateMakeRoot(localCommandId, pieceId);
				BuilderPiece rootPiece2 = piece.GetRootPiece();
				BuilderAction builderAction9 = BuilderActions.CreateDropPieceRollback(localCommandId, rootPiece2, actorNumber);
				BuilderAction builderAction10 = BuilderActions.CreateMakeRoot(localCommandId, rootPiece2.pieceId);
				if (cmd.canRollback)
				{
					this.AddRollbackAction(builderAction9);
					this.AddRollbackAction(builderAction10);
					this.AddRollbackAction(builderAction2);
					this.AddRollForwardCommand(cmd);
				}
				this.ExecuteAction(builderAction8);
				this.ExecuteAction(builderAction);
				return;
			}
			if (piece.parentPiece != null)
			{
				BuilderAction builderAction11 = BuilderActions.CreateDetachFromPiece(localCommandId, pieceId, actorNumber);
				BuilderAction builderAction12 = BuilderActions.CreateAttachToPieceRollback(localCommandId, piece, actorNumber);
				if (cmd.canRollback)
				{
					this.AddRollbackAction(builderAction12);
					this.AddRollbackAction(builderAction2);
					this.AddRollForwardCommand(cmd);
				}
				this.ExecuteAction(builderAction11);
				this.ExecuteAction(builderAction);
			}
		}

		public bool ValidateDropPieceParams(int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, NetPlayer droppedByPlayer)
		{
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid())
			{
				float num2 = 10000f;
				if ((in velocity).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in angVelocity).IsValid(in num3))
					{
						BuilderPiece piece = this.GetPiece(pieceId);
						if (piece == null)
						{
							return false;
						}
						if (piece.isBuiltIntoTable)
						{
							return false;
						}
						if (droppedByPlayer == null)
						{
							return false;
						}
						if (velocity.sqrMagnitude > BuilderTable.MAX_DROP_VELOCITY * BuilderTable.MAX_DROP_VELOCITY)
						{
							return false;
						}
						if (angVelocity.sqrMagnitude > BuilderTable.MAX_DROP_ANG_VELOCITY * BuilderTable.MAX_DROP_ANG_VELOCITY)
						{
							return false;
						}
						if ((this.roomCenter.position - position).sqrMagnitude > this.acceptableSqrDistFromCenter)
						{
							return false;
						}
						if (piece.state == BuilderPiece.State.AttachedToArm)
						{
							if (piece.parentPiece == null)
							{
								return false;
							}
							if (piece.parentPiece.heldByPlayerActorNumber != droppedByPlayer.ActorNumber)
							{
								return false;
							}
						}
						else if (piece.heldByPlayerActorNumber != droppedByPlayer.ActorNumber)
						{
							return false;
						}
						return !PhotonNetwork.IsMasterClient || this.IsPlayerHandNearAction(droppedByPlayer, position, piece.heldInLeftHand, false, 2.5f);
					}
				}
			}
			return false;
		}

		public bool ValidateDropPieceState(int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, Player droppedByPlayer)
		{
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return false;
			}
			bool flag = piece.state == BuilderPiece.State.AttachedToArm;
			return (flag || piece.heldByPlayerActorNumber == droppedByPlayer.ActorNumber) && (!flag || piece.parentPiece.heldByPlayerActorNumber == droppedByPlayer.ActorNumber);
		}

		public void RequestDropPiece(BuilderPiece piece, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity)
		{
			if (this.tableState != BuilderTable.TableState.Ready)
			{
				return;
			}
			this.builderNetworking.RequestDropPiece(piece, position, rotation, velocity, angVelocity);
		}

		public void DropPiece(int localCommandId, int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, NetPlayer droppedByPlayer, bool force)
		{
			this.PieceDroppedInternal(localCommandId, pieceId, position, rotation, velocity, angVelocity, droppedByPlayer, force);
		}

		public void PieceDroppedInternal(int localCommandId, int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, NetPlayer droppedByPlayer, bool force)
		{
			if (!force && droppedByPlayer == NetworkSystem.Instance.LocalPlayer && this.HasRollForwardCommand(localCommandId) && this.TryRollbackAndReExecute(localCommandId))
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Drop,
				pieceId = pieceId,
				parentPieceId = pieceId,
				localPosition = position,
				localRotation = rotation,
				velocity = velocity,
				angVelocity = angVelocity,
				player = droppedByPlayer,
				canRollback = force,
				localCommandId = localCommandId
			};
			this.RouteNewCommand(builderCommand, force);
		}

		public void ExecutePieceDroppedWithActions(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			int localCommandId = cmd.localCommandId;
			int actorNumber = cmd.player.ActorNumber;
			if ((cmd.player == null || !cmd.player.IsLocal) && !this.ValidateDropPieceParams(pieceId, cmd.localPosition, cmd.localRotation, cmd.velocity, cmd.angVelocity, cmd.player))
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null)
			{
				return;
			}
			if (piece.state == BuilderPiece.State.AttachedToArm)
			{
				BuilderPiece parentPiece = piece.parentPiece;
				BuilderAction builderAction = BuilderActions.CreateDetachFromPiece(localCommandId, pieceId, actorNumber);
				BuilderAction builderAction2 = BuilderActions.CreateDropPiece(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, cmd.velocity, cmd.angVelocity, actorNumber);
				if (cmd.canRollback)
				{
					BuilderAction builderAction3 = BuilderActions.CreateAttachToPieceRollback(localCommandId, piece, actorNumber);
					this.AddRollbackAction(builderAction3);
					this.AddRollForwardCommand(cmd);
				}
				this.ExecuteAction(builderAction);
				this.ExecuteAction(builderAction2);
				return;
			}
			BuilderAction builderAction4 = BuilderActions.CreateDetachFromPlayer(localCommandId, pieceId, actorNumber);
			BuilderAction builderAction5 = BuilderActions.CreateDropPiece(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, cmd.velocity, cmd.angVelocity, actorNumber);
			if (cmd.canRollback)
			{
				BuilderAction builderAction6 = BuilderActions.CreateAttachToPlayerRollback(localCommandId, piece);
				this.AddRollbackAction(builderAction6);
				this.AddRollForwardCommand(cmd);
			}
			this.ExecuteAction(builderAction4);
			this.ExecuteAction(builderAction5);
		}

		public void ExecutePieceRepelled(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			int localCommandId = cmd.localCommandId;
			int actorNumber = cmd.player.ActorNumber;
			int attachPieceId = cmd.attachPieceId;
			BuilderPiece piece = this.GetPiece(pieceId);
			Vector3 vector = cmd.velocity;
			if (piece == null)
			{
				return;
			}
			if (piece.isBuiltIntoTable || piece.isArmShelf)
			{
				return;
			}
			if (piece.state != BuilderPiece.State.Grabbed && piece.state != BuilderPiece.State.GrabbedLocal && piece.state != BuilderPiece.State.Dropped && piece.state != BuilderPiece.State.AttachedToDropped && piece.state != BuilderPiece.State.AttachedToArm)
			{
				return;
			}
			if (attachPieceId >= 0 && attachPieceId < this.dropZones.Count)
			{
				BuilderDropZone builderDropZone = this.dropZones[attachPieceId];
				builderDropZone.PlayEffect();
				if (builderDropZone.overrideDirection)
				{
					vector = builderDropZone.GetRepelDirectionWorld() * BuilderTable.DROP_ZONE_REPEL;
				}
			}
			if (piece.heldByPlayerActorNumber >= 0)
			{
				BuilderAction builderAction = BuilderActions.CreateDetachFromPlayer(localCommandId, pieceId, piece.heldByPlayerActorNumber);
				BuilderAction builderAction2 = BuilderActions.CreateDropPiece(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, vector, cmd.angVelocity, actorNumber);
				this.ExecuteAction(builderAction);
				this.ExecuteAction(builderAction2);
				return;
			}
			if (piece.state == BuilderPiece.State.AttachedToArm && piece.parentPiece != null)
			{
				BuilderAction builderAction3 = BuilderActions.CreateDetachFromPiece(localCommandId, pieceId, piece.heldByPlayerActorNumber);
				BuilderAction builderAction4 = BuilderActions.CreateDropPiece(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, vector, cmd.angVelocity, actorNumber);
				this.ExecuteAction(builderAction3);
				this.ExecuteAction(builderAction4);
				return;
			}
			BuilderAction builderAction5 = BuilderActions.CreateDropPiece(localCommandId, pieceId, cmd.localPosition, cmd.localRotation, vector, cmd.angVelocity, actorNumber);
			this.ExecuteAction(builderAction5);
		}

		private void CleanUpDroppedPiece()
		{
			if (!PhotonNetwork.IsMasterClient || this.droppedPieces.Count <= BuilderTable.DROPPED_PIECE_LIMIT)
			{
				return;
			}
			BuilderPiece builderPiece = this.FindFirstSleepingPiece();
			if (builderPiece != null && builderPiece.state == BuilderPiece.State.Dropped)
			{
				this.RequestRecyclePiece(builderPiece, false, -1);
				return;
			}
			Debug.LogErrorFormat("Piece {0} in Dropped List is {1}", new object[] { builderPiece.pieceId, builderPiece.state });
		}

		public void FreezeDroppedPiece(BuilderPiece piece)
		{
			int num = this.droppedPieces.IndexOf(piece);
			if (num >= 0)
			{
				BuilderTable.DroppedPieceData droppedPieceData = this.droppedPieceData[num];
				droppedPieceData.droppedState = BuilderTable.DroppedPieceState.Frozen;
				droppedPieceData.speedThreshCrossedTime = 0f;
				this.droppedPieceData[num] = droppedPieceData;
				if (piece.rigidBody != null)
				{
					piece.SetKinematic(true, false);
				}
				piece.forcedFrozen = true;
			}
		}

		public void AddPieceToDropList(BuilderPiece piece)
		{
			this.droppedPieces.Add(piece);
			this.droppedPieceData.Add(new BuilderTable.DroppedPieceData
			{
				speedThreshCrossedTime = 0f,
				droppedState = BuilderTable.DroppedPieceState.Light,
				filteredSpeed = 0f
			});
		}

		private BuilderPiece FindFirstSleepingPiece()
		{
			if (this.droppedPieces.Count < 1)
			{
				return null;
			}
			BuilderPiece builderPiece = this.droppedPieces[0];
			for (int i = 0; i < this.droppedPieces.Count; i++)
			{
				if (this.droppedPieces[i].rigidBody != null && this.droppedPieces[i].rigidBody.IsSleeping())
				{
					BuilderPiece builderPiece2 = this.droppedPieces[i];
					this.droppedPieces.RemoveAt(i);
					this.droppedPieceData.RemoveAt(i);
					return builderPiece2;
				}
			}
			BuilderPiece builderPiece3 = this.droppedPieces[0];
			this.droppedPieces.RemoveAt(0);
			this.droppedPieceData.RemoveAt(0);
			return builderPiece3;
		}

		public void RemovePieceFromDropList(BuilderPiece piece)
		{
			if (piece.state == BuilderPiece.State.Dropped)
			{
				this.droppedPieces.Remove(piece);
			}
		}

		private void UpdateDroppedPieces(float dt)
		{
			for (int i = 0; i < this.droppedPieces.Count; i++)
			{
				if (this.droppedPieceData[i].droppedState == BuilderTable.DroppedPieceState.Frozen && this.droppedPieces[i].state == BuilderPiece.State.Dropped)
				{
					BuilderTable.DroppedPieceData droppedPieceData = this.droppedPieceData[i];
					droppedPieceData.speedThreshCrossedTime += dt;
					if (droppedPieceData.speedThreshCrossedTime > 60f)
					{
						this.droppedPieces[i].forcedFrozen = false;
						this.droppedPieces[i].ClearCollisionHistory();
						this.droppedPieces[i].SetKinematic(false, true);
						droppedPieceData.droppedState = BuilderTable.DroppedPieceState.Light;
						droppedPieceData.speedThreshCrossedTime = 0f;
					}
					this.droppedPieceData[i] = droppedPieceData;
				}
				else
				{
					Rigidbody rigidBody = this.droppedPieces[i].rigidBody;
					if (rigidBody != null)
					{
						BuilderTable.DroppedPieceData droppedPieceData2 = this.droppedPieceData[i];
						float magnitude = rigidBody.linearVelocity.magnitude;
						droppedPieceData2.filteredSpeed = droppedPieceData2.filteredSpeed * 0.95f + magnitude * 0.05f;
						switch (droppedPieceData2.droppedState)
						{
						case BuilderTable.DroppedPieceState.Light:
							droppedPieceData2.speedThreshCrossedTime = ((droppedPieceData2.filteredSpeed < 0.05f) ? (droppedPieceData2.speedThreshCrossedTime + dt) : 0f);
							if (droppedPieceData2.speedThreshCrossedTime > 0f)
							{
								rigidBody.mass = 10000f;
								droppedPieceData2.droppedState = BuilderTable.DroppedPieceState.Heavy;
								droppedPieceData2.speedThreshCrossedTime = 0f;
							}
							break;
						case BuilderTable.DroppedPieceState.Heavy:
							droppedPieceData2.speedThreshCrossedTime += dt;
							droppedPieceData2.speedThreshCrossedTime = ((droppedPieceData2.filteredSpeed > 0.075f) ? (droppedPieceData2.speedThreshCrossedTime + dt) : 0f);
							if (droppedPieceData2.speedThreshCrossedTime > 0.5f)
							{
								rigidBody.mass = 1f;
								droppedPieceData2.droppedState = BuilderTable.DroppedPieceState.Light;
								droppedPieceData2.speedThreshCrossedTime = 0f;
							}
							break;
						}
						this.droppedPieceData[i] = droppedPieceData2;
					}
				}
			}
		}

		private void SetLocalPlayerOwnsPlot(bool ownsPlot)
		{
			this.doesLocalPlayerOwnPlot = ownsPlot;
			UnityEvent<bool> onLocalPlayerClaimedPlot = this.OnLocalPlayerClaimedPlot;
			if (onLocalPlayerClaimedPlot == null)
			{
				return;
			}
			onLocalPlayerClaimedPlot.Invoke(this.doesLocalPlayerOwnPlot);
		}

		public void PlotClaimed(int plotPieceId, Player claimingPlayer)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.ClaimPlot,
				pieceId = plotPieceId,
				player = NetPlayer.Get(claimingPlayer)
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void ExecuteClaimPlot(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			NetPlayer player = cmd.player;
			if (pieceId == -1)
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece == null || !piece.IsPrivatePlot())
			{
				return;
			}
			if (player == null)
			{
				return;
			}
			BuilderPiecePrivatePlot builderPiecePrivatePlot;
			if (this.plotOwners.TryAdd(player.ActorNumber, pieceId) && piece.TryGetPlotComponent(out builderPiecePrivatePlot))
			{
				builderPiecePrivatePlot.ClaimPlotForPlayerNumber(player.ActorNumber);
				if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					this.SetLocalPlayerOwnsPlot(true);
				}
			}
		}

		public void PlayerLeftRoom(int playerActorNumber)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.PlayerLeftRoom,
				pieceId = playerActorNumber,
				player = null
			};
			bool flag = this.tableState == BuilderTable.TableState.WaitForMasterResync;
			this.RouteNewCommand(builderCommand, flag);
		}

		public void ExecutePlayerLeftRoom(BuilderTable.BuilderCommand cmd)
		{
			NetPlayer player = cmd.player;
			int num = ((player != null) ? player.ActorNumber : cmd.pieceId);
			this.FreePlotInternal(-1, num);
			int num2;
			if (this.playerToArmShelfLeft.TryGetValue(num, out num2))
			{
				this.RecyclePieceInternal(num2, true, false, -1);
			}
			this.playerToArmShelfLeft.Remove(num);
			int num3;
			if (this.playerToArmShelfRight.TryGetValue(num, out num3))
			{
				this.RecyclePieceInternal(num3, true, false, -1);
			}
			this.playerToArmShelfRight.Remove(num);
		}

		public void PlotFreed(int plotPieceId, Player claimingPlayer)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.FreePlot,
				pieceId = plotPieceId,
				player = NetPlayer.Get(claimingPlayer)
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public void ExecuteFreePlot(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			NetPlayer player = cmd.player;
			if (player == null)
			{
				return;
			}
			this.FreePlotInternal(pieceId, player.ActorNumber);
		}

		private void FreePlotInternal(int plotPieceId, int requestingPlayer)
		{
			if (plotPieceId == -1 && !this.plotOwners.TryGetValue(requestingPlayer, out plotPieceId))
			{
				return;
			}
			BuilderPiece piece = this.GetPiece(plotPieceId);
			if (piece == null || !piece.IsPrivatePlot())
			{
				return;
			}
			BuilderPiecePrivatePlot builderPiecePrivatePlot;
			if (piece.TryGetPlotComponent(out builderPiecePrivatePlot))
			{
				int ownerActorNumber = builderPiecePrivatePlot.GetOwnerActorNumber();
				this.plotOwners.Remove(ownerActorNumber);
				builderPiecePrivatePlot.FreePlot();
				if (ownerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
				{
					this.SetLocalPlayerOwnsPlot(false);
				}
			}
		}

		public bool DoesPlayerOwnPlot(int actorNum)
		{
			return this.plotOwners.ContainsKey(actorNum);
		}

		public void RequestPaintPiece(int pieceId, int materialType)
		{
			this.builderNetworking.RequestPaintPiece(pieceId, materialType);
		}

		public void PaintPiece(int pieceId, int materialType, Player paintingPlayer, bool force)
		{
			this.PaintPieceInternal(pieceId, materialType, paintingPlayer, force);
		}

		private void PaintPieceInternal(int pieceId, int materialType, Player paintingPlayer, bool force)
		{
			if (!force && paintingPlayer == PhotonNetwork.LocalPlayer)
			{
				return;
			}
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Paint,
				pieceId = pieceId,
				materialType = materialType,
				player = NetPlayer.Get(paintingPlayer)
			};
			this.RouteNewCommand(builderCommand, force);
		}

		public void ExecutePiecePainted(BuilderTable.BuilderCommand cmd)
		{
			int pieceId = cmd.pieceId;
			int materialType = cmd.materialType;
			BuilderPiece piece = this.GetPiece(pieceId);
			if (piece != null && !piece.isBuiltIntoTable)
			{
				piece.SetMaterial(materialType, false);
			}
		}

		public void CreateArmShelvesForPlayersInBuilder()
		{
			if (!this.isTableMutable)
			{
				return;
			}
			if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
			{
				foreach (Player player in this.builderNetworking.armShelfRequests)
				{
					if (player != null)
					{
						this.builderNetworking.RequestCreateArmShelfForPlayer(player);
					}
				}
				this.builderNetworking.armShelfRequests.Clear();
			}
		}

		public void RemoveArmShelfForPlayer(Player player)
		{
			if (!this.isTableMutable)
			{
				return;
			}
			if (player == null)
			{
				return;
			}
			if (this.tableState != BuilderTable.TableState.Ready)
			{
				this.builderNetworking.armShelfRequests.Remove(player);
				return;
			}
			int num;
			if (this.playerToArmShelfLeft.TryGetValue(player.ActorNumber, out num))
			{
				BuilderPiece piece = this.GetPiece(num);
				this.playerToArmShelfLeft.Remove(player.ActorNumber);
				if (piece.armShelf != null)
				{
					piece.armShelf.piece = null;
					piece.armShelf = null;
				}
				if (PhotonNetwork.IsMasterClient)
				{
					this.builderNetworking.RequestRecyclePiece(num, piece.transform.position, piece.transform.rotation, false, -1);
				}
				else
				{
					this.DropPieceForPlayerLeavingInternal(piece, player.ActorNumber);
				}
			}
			int num2;
			if (this.playerToArmShelfRight.TryGetValue(player.ActorNumber, out num2))
			{
				BuilderPiece piece2 = this.GetPiece(num2);
				this.playerToArmShelfRight.Remove(player.ActorNumber);
				if (piece2.armShelf != null)
				{
					piece2.armShelf.piece = null;
					piece2.armShelf = null;
				}
				if (PhotonNetwork.IsMasterClient)
				{
					this.builderNetworking.RequestRecyclePiece(num2, piece2.transform.position, piece2.transform.rotation, false, -1);
					return;
				}
				this.DropPieceForPlayerLeavingInternal(piece2, player.ActorNumber);
			}
		}

		public void DropAllPiecesForPlayerLeaving(int playerActorNumber)
		{
			List<BuilderPiece> list = this.pieces;
			if (list == null)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				BuilderPiece builderPiece = list[i];
				if (builderPiece != null && builderPiece.heldByPlayerActorNumber == playerActorNumber && (builderPiece.state == BuilderPiece.State.Grabbed || builderPiece.state == BuilderPiece.State.GrabbedLocal))
				{
					this.DropPieceForPlayerLeavingInternal(builderPiece, playerActorNumber);
				}
			}
		}

		public void RecycleAllPiecesForPlayerLeaving(int playerActorNumber)
		{
			List<BuilderPiece> list = this.pieces;
			if (list == null)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				BuilderPiece builderPiece = list[i];
				if (builderPiece != null && builderPiece.heldByPlayerActorNumber == playerActorNumber && (builderPiece.state == BuilderPiece.State.Grabbed || builderPiece.state == BuilderPiece.State.GrabbedLocal))
				{
					this.RecyclePieceForPlayerLeavingInternal(builderPiece, playerActorNumber);
				}
			}
		}

		private void DropPieceForPlayerLeavingInternal(BuilderPiece piece, int playerActorNumber)
		{
			BuilderAction builderAction = BuilderActions.CreateDetachFromPlayer(-1, piece.pieceId, playerActorNumber);
			BuilderAction builderAction2 = BuilderActions.CreateDropPiece(-1, piece.pieceId, piece.transform.position, piece.transform.rotation, Vector3.zero, Vector3.zero, playerActorNumber);
			this.ExecuteAction(builderAction);
			this.ExecuteAction(builderAction2);
		}

		private void RecyclePieceForPlayerLeavingInternal(BuilderPiece piece, int playerActorNumber)
		{
			this.builderNetworking.RequestRecyclePiece(piece.pieceId, piece.transform.position, piece.transform.rotation, false, -1);
		}

		private void DetachPieceForPlayerLeavingInternal(BuilderPiece piece, int playerActorNumber)
		{
			BuilderAction builderAction = BuilderActions.CreateDetachFromPiece(-1, piece.pieceId, playerActorNumber);
			BuilderAction builderAction2 = BuilderActions.CreateDropPiece(-1, piece.pieceId, piece.transform.position, piece.transform.rotation, Vector3.zero, Vector3.zero, playerActorNumber);
			this.ExecuteAction(builderAction);
			this.ExecuteAction(builderAction2);
		}

		public void CreateArmShelf(int pieceIdLeft, int pieceIdRight, int pieceType, Player player)
		{
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.CreateArmShelf,
				pieceId = pieceIdLeft,
				pieceType = pieceType,
				player = NetPlayer.Get(player),
				isLeft = true
			};
			this.RouteNewCommand(builderCommand, false);
			BuilderTable.BuilderCommand builderCommand2 = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.CreateArmShelf,
				pieceId = pieceIdRight,
				pieceType = pieceType,
				player = NetPlayer.Get(player),
				isLeft = false
			};
			this.RouteNewCommand(builderCommand2, false);
		}

		public void ExecuteArmShelfCreated(BuilderTable.BuilderCommand cmd)
		{
			NetPlayer player = cmd.player;
			if (player == null)
			{
				return;
			}
			bool isLeft = cmd.isLeft;
			if (this.GetPiece(cmd.pieceId) != null)
			{
				return;
			}
			RigContainer rigContainer;
			if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
			{
				BuilderArmShelf builderArmShelf = (isLeft ? rigContainer.Rig.builderArmShelfLeft : rigContainer.Rig.builderArmShelfRight);
				if (builderArmShelf != null)
				{
					if (builderArmShelf.piece != null)
					{
						if (builderArmShelf.piece.isArmShelf && builderArmShelf.piece.isActiveAndEnabled)
						{
							builderArmShelf.piece.armShelf = null;
							this.RecyclePiece(builderArmShelf.piece.pieceId, builderArmShelf.piece.transform.position, builderArmShelf.piece.transform.rotation, false, -1, PhotonNetwork.LocalPlayer);
						}
						else
						{
							builderArmShelf.piece = null;
						}
						BuilderPiece builderPiece = this.CreatePieceInternal(cmd.pieceType, cmd.pieceId, builderArmShelf.pieceAnchor.position, builderArmShelf.pieceAnchor.rotation, BuilderPiece.State.AttachedToArm, -1, 0, this);
						builderArmShelf.piece = builderPiece;
						builderPiece.armShelf = builderArmShelf;
						builderPiece.SetParentHeld(builderArmShelf.pieceAnchor, cmd.player.ActorNumber, isLeft);
						builderPiece.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
						builderPiece.transform.localScale = Vector3.one;
						if (isLeft)
						{
							this.playerToArmShelfLeft.AddOrUpdate(player.ActorNumber, cmd.pieceId);
							return;
						}
						this.playerToArmShelfRight.AddOrUpdate(player.ActorNumber, cmd.pieceId);
						return;
					}
					else
					{
						BuilderPiece builderPiece2 = this.CreatePieceInternal(cmd.pieceType, cmd.pieceId, builderArmShelf.pieceAnchor.position, builderArmShelf.pieceAnchor.rotation, BuilderPiece.State.AttachedToArm, -1, 0, this);
						builderArmShelf.piece = builderPiece2;
						builderPiece2.armShelf = builderArmShelf;
						builderPiece2.SetParentHeld(builderArmShelf.pieceAnchor, cmd.player.ActorNumber, isLeft);
						builderPiece2.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
						builderPiece2.transform.localScale = Vector3.one;
						if (isLeft)
						{
							this.playerToArmShelfLeft.TryAdd(player.ActorNumber, cmd.pieceId);
							return;
						}
						this.playerToArmShelfRight.TryAdd(player.ActorNumber, cmd.pieceId);
					}
				}
			}
		}

		public void ClearLocalArmShelf()
		{
			VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
			if (offlineVRRig != null)
			{
				BuilderArmShelf builderArmShelf = offlineVRRig.builderArmShelfLeft;
				if (builderArmShelf != null)
				{
					BuilderPiece piece = builderArmShelf.piece;
					builderArmShelf.piece = null;
					if (piece != null)
					{
						piece.transform.SetParent(null);
					}
				}
				builderArmShelf = offlineVRRig.builderArmShelfRight;
				if (builderArmShelf != null)
				{
					BuilderPiece piece2 = builderArmShelf.piece;
					builderArmShelf.piece = null;
					if (piece2 != null)
					{
						piece2.transform.SetParent(null);
					}
				}
			}
		}

		public void PieceEnteredDropZone(int pieceId, Vector3 worldPos, Quaternion worldRot, int dropZoneId)
		{
			Vector3 vector = (this.roomCenter.position - worldPos).normalized * BuilderTable.DROP_ZONE_REPEL;
			BuilderTable.BuilderCommand builderCommand = new BuilderTable.BuilderCommand
			{
				type = BuilderTable.BuilderCommandType.Repel,
				pieceId = pieceId,
				parentPieceId = pieceId,
				attachPieceId = dropZoneId,
				localPosition = worldPos,
				localRotation = worldRot,
				velocity = vector,
				angVelocity = Vector3.zero,
				player = NetworkSystem.Instance.MasterClient,
				canRollback = false
			};
			this.RouteNewCommand(builderCommand, false);
		}

		public bool ValidateRepelPiece(BuilderPiece piece)
		{
			if (!this.isSetup)
			{
				return false;
			}
			if (piece.isBuiltIntoTable || piece.isArmShelf)
			{
				return false;
			}
			if (piece.state == BuilderPiece.State.Grabbed || piece.state == BuilderPiece.State.GrabbedLocal || piece.state == BuilderPiece.State.Dropped || piece.state == BuilderPiece.State.AttachedToDropped || piece.state == BuilderPiece.State.AttachedToArm)
			{
				bool flag = false;
				for (int i = 0; i < this.repelHistoryLength; i++)
				{
					flag = flag || this.repelledPieceRoots[i].Contains(piece.pieceId);
					if (flag)
					{
						return false;
					}
				}
				this.repelledPieceRoots[this.repelHistoryIndex].Add(piece.pieceId);
				return true;
			}
			return false;
		}

		public void RepelPieceTowardTable(int pieceID)
		{
			BuilderPiece piece = this.GetPiece(pieceID);
			if (piece == null)
			{
				return;
			}
			Vector3 position = piece.transform.position;
			Quaternion rotation = piece.transform.rotation;
			if (position.y < this.tableCenter.position.y)
			{
				position.y = this.tableCenter.position.y;
			}
			Vector3 vector = (this.tableCenter.position - position).normalized * BuilderTable.DROP_ZONE_REPEL;
			if (piece.IsHeldLocal())
			{
				BuilderPieceInteractor.instance.RemovePieceFromHeld(piece);
			}
			piece.ClearParentHeld();
			piece.ClearParentPiece(false);
			piece.transform.localScale = Vector3.one;
			piece.SetState(BuilderPiece.State.Dropped, false);
			piece.transform.SetLocalPositionAndRotation(position, rotation);
			if (piece.rigidBody != null)
			{
				piece.rigidBody.position = position;
				piece.rigidBody.rotation = rotation;
				piece.rigidBody.linearVelocity = vector;
				piece.rigidBody.AddForce(Vector3.up * (BuilderTable.DROP_ZONE_REPEL / 2f) * piece.rigidBody.mass, ForceMode.Impulse);
				piece.rigidBody.angularVelocity = Vector3.zero;
			}
		}

		public BuilderPiece GetPiece(int pieceId)
		{
			int num;
			if (this.pieceIDToIndexCache.TryGetValue(pieceId, out num))
			{
				if (num >= 0 && num < this.pieces.Count)
				{
					return this.pieces[num];
				}
				this.pieceIDToIndexCache.Remove(pieceId);
			}
			for (int i = 0; i < this.pieces.Count; i++)
			{
				if (this.pieces[i].pieceId == pieceId)
				{
					this.pieceIDToIndexCache.Add(pieceId, i);
					return this.pieces[i];
				}
			}
			for (int j = 0; j < this.basePieces.Count; j++)
			{
				if (this.basePieces[j].pieceId == pieceId)
				{
					return this.basePieces[j];
				}
			}
			return null;
		}

		public void AddPiece(BuilderPiece piece)
		{
			this.pieces.Add(piece);
			this.UseResources(piece);
			this.AddPieceData(piece);
		}

		public void RemovePiece(BuilderPiece piece)
		{
			this.pieces.Remove(piece);
			this.AddResources(piece);
			this.RemovePieceData(piece);
			this.pieceIDToIndexCache.Clear();
		}

		private void CreateData()
		{
		}

		private void DestroyData()
		{
		}

		private int AddPieceData(BuilderPiece piece)
		{
			return -1;
		}

		public void UpdatePieceData(BuilderPiece piece)
		{
		}

		private void RemovePieceData(BuilderPiece piece)
		{
		}

		private int AddGridPlaneData(BuilderAttachGridPlane gridPlane)
		{
			return -1;
		}

		private void RemoveGridPlaneData(BuilderAttachGridPlane gridPlane)
		{
		}

		private int AddPrivatePlotData(BuilderPiecePrivatePlot plot)
		{
			return -1;
		}

		private void RemovePrivatePlotData(BuilderPiecePrivatePlot plot)
		{
		}

		public void OnButtonFreeRotation(BuilderOptionButton button, bool isLeftHand)
		{
			this.useSnapRotation = !this.useSnapRotation;
			button.SetPressed(this.useSnapRotation);
		}

		public void OnButtonFreePosition(BuilderOptionButton button, bool isLeftHand)
		{
			if (this.usePlacementStyle == BuilderPlacementStyle.Float)
			{
				this.usePlacementStyle = BuilderPlacementStyle.SnapDown;
			}
			else if (this.usePlacementStyle == BuilderPlacementStyle.SnapDown)
			{
				this.usePlacementStyle = BuilderPlacementStyle.Float;
			}
			button.SetPressed(this.usePlacementStyle > BuilderPlacementStyle.Float);
		}

		public void OnButtonSaveLayout(BuilderOptionButton button, bool isLeftHand)
		{
		}

		public void OnButtonClearLayout(BuilderOptionButton button, bool isLeftHand)
		{
		}

		public bool TryPlaceGridPlane(BuilderPiece piece, BuilderAttachGridPlane gridPlane, List<BuilderAttachGridPlane> checkGridPlanes, out BuilderPotentialPlacement potentialPlacement)
		{
			potentialPlacement = default(BuilderPotentialPlacement);
			potentialPlacement.Reset();
			Vector3 position = gridPlane.transform.position;
			Quaternion rotation = gridPlane.transform.rotation;
			if (this.gridSize <= 0f)
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < checkGridPlanes.Count; i++)
			{
				BuilderAttachGridPlane builderAttachGridPlane = checkGridPlanes[i];
				this.TryPlaceGridPlaneOnGridPlane(piece, gridPlane, position, rotation, builderAttachGridPlane, ref potentialPlacement, ref flag);
			}
			return flag;
		}

		public bool TryPlaceGridPlaneOnGridPlane(BuilderPiece piece, BuilderAttachGridPlane gridPlane, Vector3 gridPlanePos, Quaternion gridPlaneRot, BuilderAttachGridPlane checkGridPlane, ref BuilderPotentialPlacement potentialPlacement, ref bool success)
		{
			if (checkGridPlane.male == gridPlane.male)
			{
				return false;
			}
			if (checkGridPlane.piece == gridPlane.piece)
			{
				return false;
			}
			Transform center = checkGridPlane.center;
			Vector3 position = center.position;
			float sqrMagnitude = (position - gridPlanePos).sqrMagnitude;
			float num = checkGridPlane.boundingRadius + gridPlane.boundingRadius;
			if (sqrMagnitude > num * num)
			{
				return false;
			}
			Quaternion rotation = center.rotation;
			Quaternion quaternion = Quaternion.Inverse(rotation);
			Quaternion quaternion2 = quaternion * gridPlaneRot;
			if (Vector3.Dot(Vector3.up, quaternion2 * Vector3.up) < this.currSnapParams.maxUpDotProduct)
			{
				return false;
			}
			Vector3 vector = quaternion * (gridPlanePos - position);
			float y = vector.y;
			float num2 = -Mathf.Abs(y);
			if (success && num2 < potentialPlacement.score)
			{
				return false;
			}
			if (Mathf.Abs(y) > 1f)
			{
				return false;
			}
			if ((gridPlane.male && y > this.currSnapParams.minOffsetY) || (!gridPlane.male && y < -this.currSnapParams.minOffsetY))
			{
				return false;
			}
			if (Mathf.Abs(y) > this.currSnapParams.maxOffsetY)
			{
				return false;
			}
			Quaternion quaternion3;
			Quaternion quaternion4;
			global::BoingKit.QuaternionUtil.DecomposeSwingTwist(quaternion2, Vector3.up, out quaternion3, out quaternion4);
			float maxTwistDotProduct = this.currSnapParams.maxTwistDotProduct;
			Vector3 vector2 = quaternion4 * Vector3.forward;
			float num3 = Vector3.Dot(vector2, Vector3.forward);
			float num4 = Vector3.Dot(vector2, Vector3.right);
			bool flag = Mathf.Abs(num3) > maxTwistDotProduct;
			bool flag2 = Mathf.Abs(num4) > maxTwistDotProduct;
			if (!flag && !flag2)
			{
				return false;
			}
			float num5;
			uint num6;
			if (flag)
			{
				num5 = ((num3 > 0f) ? 0f : 180f);
				num6 = ((num3 > 0f) ? 0U : 2U);
			}
			else
			{
				num5 = ((num4 > 0f) ? 90f : 270f);
				num6 = ((num4 > 0f) ? 1U : 3U);
			}
			int num7 = (flag2 ? gridPlane.width : gridPlane.length);
			int num8 = (flag2 ? gridPlane.length : gridPlane.width);
			float num9 = ((num8 % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num10 = ((num7 % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num11 = ((checkGridPlane.width % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num12 = ((checkGridPlane.length % 2 == 0) ? (this.gridSize / 2f) : 0f);
			float num13 = num9 - num11;
			float num14 = num10 - num12;
			int num15 = Mathf.RoundToInt((vector.x - num13) / this.gridSize);
			int num16 = Mathf.RoundToInt((vector.z - num14) / this.gridSize);
			int num17 = num15 + Mathf.FloorToInt((float)num8 / 2f);
			int num18 = num16 + Mathf.FloorToInt((float)num7 / 2f);
			int num19 = num17 - (num8 - 1);
			int num20 = num18 - (num7 - 1);
			int num21 = Mathf.FloorToInt((float)checkGridPlane.width / 2f);
			int num22 = Mathf.FloorToInt((float)checkGridPlane.length / 2f);
			int num23 = num21 - (checkGridPlane.width - 1);
			int num24 = num22 - (checkGridPlane.length - 1);
			if (num19 > num21 || num17 < num23 || num20 > num22 || num18 < num24)
			{
				return false;
			}
			BuilderPiece rootPiece = checkGridPlane.piece.GetRootPiece();
			if (BuilderTable.ShareSameRoot(gridPlane.piece, rootPiece))
			{
				return false;
			}
			if (!BuilderPiece.CanPlayerAttachPieceToPiece(PhotonNetwork.LocalPlayer.ActorNumber, gridPlane.piece, rootPiece))
			{
				return false;
			}
			BuilderPiece piece2 = checkGridPlane.piece;
			if (piece2 != null)
			{
				if (piece2.preventSnapUntilMoved > 0)
				{
					return false;
				}
				if (piece2.requestedParentPiece != null && BuilderTable.ShareSameRoot(piece, piece2.requestedParentPiece))
				{
					return false;
				}
			}
			Quaternion quaternion5 = Quaternion.Euler(0f, num5, 0f);
			Quaternion quaternion6 = rotation * quaternion5;
			float num25 = (float)num15 * this.gridSize + num13;
			float num26 = (float)num16 * this.gridSize + num14;
			Vector3 vector3 = new Vector3(num25, 0f, num26);
			Vector3 vector4 = position + rotation * vector3;
			Transform center2 = gridPlane.center;
			Quaternion quaternion7 = quaternion6 * Quaternion.Inverse(center2.localRotation);
			Vector3 vector5 = piece.transform.InverseTransformPoint(center2.position);
			Vector3 vector6 = vector4 - quaternion7 * vector5;
			potentialPlacement.localPosition = vector6;
			potentialPlacement.localRotation = quaternion7;
			potentialPlacement.score = num2;
			success = true;
			potentialPlacement.parentPiece = piece2;
			potentialPlacement.parentAttachIndex = checkGridPlane.attachIndex;
			potentialPlacement.attachDistance = Mathf.Abs(y);
			potentialPlacement.attachPlaneNormal = Vector3.up;
			if (!checkGridPlane.male)
			{
				potentialPlacement.attachPlaneNormal *= -1f;
			}
			if (potentialPlacement.parentPiece != null)
			{
				BuilderAttachGridPlane builderAttachGridPlane = potentialPlacement.parentPiece.gridPlanes[potentialPlacement.parentAttachIndex];
				potentialPlacement.localPosition = builderAttachGridPlane.transform.InverseTransformPoint(potentialPlacement.localPosition);
				potentialPlacement.localRotation = Quaternion.Inverse(builderAttachGridPlane.transform.rotation) * potentialPlacement.localRotation;
			}
			potentialPlacement.parentAttachBounds.min.x = Mathf.Max(num23, num19);
			potentialPlacement.parentAttachBounds.min.y = Mathf.Max(num24, num20);
			potentialPlacement.parentAttachBounds.max.x = Mathf.Min(num21, num17);
			potentialPlacement.parentAttachBounds.max.y = Mathf.Min(num22, num18);
			Vector2Int vector2Int = Vector2Int.zero;
			Vector2Int vector2Int2 = Vector2Int.zero;
			vector2Int.x = potentialPlacement.parentAttachBounds.min.x - num15;
			vector2Int2.x = potentialPlacement.parentAttachBounds.max.x - num15;
			vector2Int.y = potentialPlacement.parentAttachBounds.min.y - num16;
			vector2Int2.y = potentialPlacement.parentAttachBounds.max.y - num16;
			potentialPlacement.twist = (byte)num6;
			potentialPlacement.bumpOffsetX = (sbyte)num15;
			potentialPlacement.bumpOffsetZ = (sbyte)num16;
			int num27 = ((num8 % 2 == 0) ? 1 : 0);
			int num28 = ((num7 % 2 == 0) ? 1 : 0);
			if (flag && num3 < 0f)
			{
				vector2Int = this.Rotate180(vector2Int, num27, num28);
				vector2Int2 = this.Rotate180(vector2Int2, num27, num28);
			}
			else if (flag2 && num4 < 0f)
			{
				vector2Int = this.Rotate270(vector2Int, num27, num28);
				vector2Int2 = this.Rotate270(vector2Int2, num27, num28);
			}
			else if (flag2 && num4 > 0f)
			{
				vector2Int = this.Rotate90(vector2Int, num27, num28);
				vector2Int2 = this.Rotate90(vector2Int2, num27, num28);
			}
			potentialPlacement.attachBounds.min.x = Mathf.Min(vector2Int.x, vector2Int2.x);
			potentialPlacement.attachBounds.min.y = Mathf.Min(vector2Int.y, vector2Int2.y);
			potentialPlacement.attachBounds.max.x = Mathf.Max(vector2Int.x, vector2Int2.x);
			potentialPlacement.attachBounds.max.y = Mathf.Max(vector2Int.y, vector2Int2.y);
			return true;
		}

		private Vector2Int Rotate90(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.y * -1 + offsetY, v.x);
		}

		private Vector2Int Rotate270(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.y, v.x * -1 + offsetX);
		}

		private Vector2Int Rotate180(Vector2Int v, int offsetX, int offsetY)
		{
			return new Vector2Int(v.x * -1 + offsetX, v.y * -1 + offsetY);
		}

		public bool ShareSameRoot(BuilderAttachGridPlane plane, BuilderAttachGridPlane otherPlane)
		{
			return !(plane == null) && !(otherPlane == null) && !(otherPlane.piece == null) && BuilderTable.ShareSameRoot(plane.piece, otherPlane.piece);
		}

		public static bool ShareSameRoot(BuilderPiece piece, BuilderPiece otherPiece)
		{
			if (otherPiece == null || piece == null)
			{
				return false;
			}
			if (piece == otherPiece)
			{
				return true;
			}
			BuilderPiece builderPiece = piece;
			int num = 2048;
			while (builderPiece.parentPiece != null && !builderPiece.parentPiece.isBuiltIntoTable)
			{
				builderPiece = builderPiece.parentPiece;
				num--;
				if (num <= 0)
				{
					return true;
				}
			}
			num = 2048;
			BuilderPiece builderPiece2 = otherPiece;
			while (builderPiece2.parentPiece != null && !builderPiece2.parentPiece.isBuiltIntoTable)
			{
				builderPiece2 = builderPiece2.parentPiece;
				num--;
				if (num <= 0)
				{
					return true;
				}
			}
			return builderPiece == builderPiece2;
		}

		public bool TryPlacePieceOnTableNoDrop(bool leftHand, BuilderPiece testPiece, List<BuilderAttachGridPlane> checkGridPlanesMale, List<BuilderAttachGridPlane> checkGridPlanesFemale, out BuilderPotentialPlacement potentialPlacement)
		{
			potentialPlacement = default(BuilderPotentialPlacement);
			potentialPlacement.Reset();
			if (this == null)
			{
				return false;
			}
			if (testPiece == null)
			{
				return false;
			}
			this.currSnapParams = this.pushAndEaseParams;
			return this.TryPlacePieceGridPlanesOnTableInternal(testPiece, this.maxPlacementChildDepth, checkGridPlanesMale, checkGridPlanesFemale, out potentialPlacement);
		}

		public bool TryPlacePieceOnTableNoDropJobs(NativeList<BuilderGridPlaneData> gridPlaneData, NativeList<BuilderPieceData> pieceData, NativeList<BuilderGridPlaneData> checkGridPlaneData, NativeList<BuilderPieceData> checkPieceData, out BuilderPotentialPlacement potentialPlacement, List<BuilderPotentialPlacement> allPlacements)
		{
			potentialPlacement = default(BuilderPotentialPlacement);
			potentialPlacement.Reset();
			if (this == null)
			{
				return false;
			}
			this.currSnapParams = this.pushAndEaseParams;
			NativeQueue<BuilderPotentialPlacementData> nativeQueue = new NativeQueue<BuilderPotentialPlacementData>(Allocator.TempJob);
			new BuilderFindPotentialSnaps
			{
				gridSize = this.gridSize,
				currSnapParams = this.currSnapParams,
				gridPlanes = gridPlaneData,
				checkGridPlanes = checkGridPlaneData,
				worldToLocalPos = Vector3.zero,
				worldToLocalRot = Quaternion.identity,
				localToWorldPos = Vector3.zero,
				localToWorldRot = Quaternion.identity,
				potentialPlacements = nativeQueue.AsParallelWriter()
			}.Schedule(gridPlaneData.Length, 32, default(JobHandle)).Complete();
			BuilderPotentialPlacementData builderPotentialPlacementData = default(BuilderPotentialPlacementData);
			bool flag = false;
			while (!nativeQueue.IsEmpty())
			{
				BuilderPotentialPlacementData builderPotentialPlacementData2 = nativeQueue.Dequeue();
				if (!flag || builderPotentialPlacementData2.score > builderPotentialPlacementData.score)
				{
					builderPotentialPlacementData = builderPotentialPlacementData2;
					flag = true;
				}
			}
			if (flag)
			{
				potentialPlacement = builderPotentialPlacementData.ToPotentialPlacement(this);
			}
			if (flag)
			{
				nativeQueue.Clear();
				this.currSnapParams = this.overlapParams;
				Vector3 vector = -potentialPlacement.attachPiece.transform.position;
				Quaternion quaternion = Quaternion.Inverse(potentialPlacement.attachPiece.transform.rotation);
				BuilderAttachGridPlane builderAttachGridPlane = potentialPlacement.parentPiece.gridPlanes[potentialPlacement.parentAttachIndex];
				Quaternion quaternion2 = builderAttachGridPlane.transform.rotation * potentialPlacement.localRotation;
				Vector3 vector2 = builderAttachGridPlane.transform.TransformPoint(potentialPlacement.localPosition);
				new BuilderFindPotentialSnaps
				{
					gridSize = this.gridSize,
					currSnapParams = this.currSnapParams,
					gridPlanes = gridPlaneData,
					checkGridPlanes = checkGridPlaneData,
					worldToLocalPos = vector,
					worldToLocalRot = quaternion,
					localToWorldPos = vector2,
					localToWorldRot = quaternion2,
					potentialPlacements = nativeQueue.AsParallelWriter()
				}.Schedule(gridPlaneData.Length, 32, default(JobHandle)).Complete();
				while (!nativeQueue.IsEmpty())
				{
					BuilderPotentialPlacementData builderPotentialPlacementData3 = nativeQueue.Dequeue();
					if (builderPotentialPlacementData3.attachDistance < this.currSnapParams.maxBlockSnapDist)
					{
						allPlacements.Add(builderPotentialPlacementData3.ToPotentialPlacement(this));
					}
				}
			}
			nativeQueue.Dispose();
			return flag;
		}

		public bool CalcAllPotentialPlacements(NativeList<BuilderGridPlaneData> gridPlaneData, NativeList<BuilderGridPlaneData> checkGridPlaneData, BuilderPotentialPlacement potentialPlacement, List<BuilderPotentialPlacement> allPlacements)
		{
			if (this == null)
			{
				return false;
			}
			bool flag = false;
			this.currSnapParams = this.overlapParams;
			NativeQueue<BuilderPotentialPlacementData> nativeQueue = new NativeQueue<BuilderPotentialPlacementData>(Allocator.TempJob);
			nativeQueue.Clear();
			Vector3 vector = -potentialPlacement.attachPiece.transform.position;
			Quaternion quaternion = Quaternion.Inverse(potentialPlacement.attachPiece.transform.rotation);
			BuilderAttachGridPlane builderAttachGridPlane = potentialPlacement.parentPiece.gridPlanes[potentialPlacement.parentAttachIndex];
			Quaternion quaternion2 = builderAttachGridPlane.transform.rotation * potentialPlacement.localRotation;
			Vector3 vector2 = builderAttachGridPlane.transform.TransformPoint(potentialPlacement.localPosition);
			new BuilderFindPotentialSnaps
			{
				gridSize = this.gridSize,
				currSnapParams = this.currSnapParams,
				gridPlanes = gridPlaneData,
				checkGridPlanes = checkGridPlaneData,
				worldToLocalPos = vector,
				worldToLocalRot = quaternion,
				localToWorldPos = vector2,
				localToWorldRot = quaternion2,
				potentialPlacements = nativeQueue.AsParallelWriter()
			}.Schedule(gridPlaneData.Length, 32, default(JobHandle)).Complete();
			while (!nativeQueue.IsEmpty())
			{
				BuilderPotentialPlacementData builderPotentialPlacementData = nativeQueue.Dequeue();
				if (builderPotentialPlacementData.attachDistance < this.currSnapParams.maxBlockSnapDist)
				{
					allPlacements.Add(builderPotentialPlacementData.ToPotentialPlacement(this));
				}
			}
			nativeQueue.Dispose();
			return flag;
		}

		public bool CanPiecesPotentiallySnap(BuilderPiece pieceInHand, BuilderPiece piece)
		{
			BuilderPiece rootPiece = piece.GetRootPiece();
			return !(rootPiece == pieceInHand) && BuilderPiece.CanPlayerAttachPieceToPiece(PhotonNetwork.LocalPlayer.ActorNumber, pieceInHand, rootPiece) && (!(piece.requestedParentPiece != null) || !BuilderTable.ShareSameRoot(pieceInHand, piece.requestedParentPiece)) && piece.preventSnapUntilMoved <= 0;
		}

		public bool CanPiecesPotentiallyOverlap(BuilderPiece pieceInHand, BuilderPiece rootWhenPlaced, BuilderPiece.State stateWhenPlaced, BuilderPiece otherPiece)
		{
			BuilderPiece rootPiece = otherPiece.GetRootPiece();
			if (rootPiece == pieceInHand)
			{
				return false;
			}
			if (!BuilderPiece.CanPlayerAttachPieceToPiece(PhotonNetwork.LocalPlayer.ActorNumber, pieceInHand, rootPiece))
			{
				return false;
			}
			if (otherPiece.requestedParentPiece != null && BuilderTable.ShareSameRoot(pieceInHand, otherPiece.requestedParentPiece))
			{
				return false;
			}
			if (otherPiece.preventSnapUntilMoved > 0)
			{
				return false;
			}
			BuilderPiece.State state = otherPiece.state;
			if (otherPiece.isBuiltIntoTable && !otherPiece.isArmShelf)
			{
				state = BuilderPiece.State.AttachedAndPlaced;
			}
			return BuilderTable.AreStatesCompatibleForOverlap(stateWhenPlaced, state, rootWhenPlaced, rootPiece);
		}

		public void TryDropPiece(bool leftHand, BuilderPiece testPiece, Vector3 velocity, Vector3 angVelocity)
		{
			if (this == null)
			{
				return;
			}
			if (testPiece == null)
			{
				return;
			}
			this.RequestDropPiece(testPiece, testPiece.transform.position, testPiece.transform.rotation, velocity, angVelocity);
		}

		public bool TryPlacePieceGridPlanesOnTableInternal(BuilderPiece testPiece, int recurse, List<BuilderAttachGridPlane> checkGridPlanesMale, List<BuilderAttachGridPlane> checkGridPlanesFemale, out BuilderPotentialPlacement potentialPlacement)
		{
			potentialPlacement = default(BuilderPotentialPlacement);
			potentialPlacement.Reset();
			bool flag = false;
			bool flag2 = false;
			if (testPiece != null && testPiece.gridPlanes != null && testPiece.gridPlanes.Count > 0 && testPiece.gridPlanes != null)
			{
				for (int i = 0; i < testPiece.gridPlanes.Count; i++)
				{
					List<BuilderAttachGridPlane> list = (testPiece.gridPlanes[i].male ? checkGridPlanesFemale : checkGridPlanesMale);
					BuilderPotentialPlacement builderPotentialPlacement;
					if (this.TryPlaceGridPlane(testPiece, testPiece.gridPlanes[i], list, out builderPotentialPlacement))
					{
						if (builderPotentialPlacement.attachDistance < this.currSnapParams.snapAttachDistance * 1.1f)
						{
							flag2 = true;
						}
						if (builderPotentialPlacement.score > potentialPlacement.score && testPiece.preventSnapUntilMoved <= 0)
						{
							potentialPlacement = builderPotentialPlacement;
							potentialPlacement.attachIndex = i;
							potentialPlacement.attachPiece = testPiece;
							flag = true;
						}
					}
				}
			}
			if (recurse > 0)
			{
				BuilderPiece builderPiece = testPiece.firstChildPiece;
				while (builderPiece != null)
				{
					BuilderPotentialPlacement builderPotentialPlacement2;
					if (this.TryPlacePieceGridPlanesOnTableInternal(builderPiece, recurse - 1, checkGridPlanesMale, checkGridPlanesFemale, out builderPotentialPlacement2))
					{
						if (builderPotentialPlacement2.attachDistance < this.currSnapParams.snapAttachDistance * 1.1f)
						{
							flag2 = true;
						}
						if (builderPotentialPlacement2.score > potentialPlacement.score && testPiece.preventSnapUntilMoved <= 0)
						{
							potentialPlacement = builderPotentialPlacement2;
							flag = true;
						}
					}
					builderPiece = builderPiece.nextSiblingPiece;
				}
			}
			if (testPiece.preventSnapUntilMoved > 0 && !flag2)
			{
				testPiece.preventSnapUntilMoved--;
				this.UpdatePieceData(testPiece);
			}
			return flag;
		}

		public void TryPlaceRandomlyOnTable(BuilderPiece piece)
		{
			BuilderAttachGridPlane builderAttachGridPlane = piece.gridPlanes[Random.Range(0, piece.gridPlanes.Count)];
			List<BuilderAttachGridPlane> list = this.baseGridPlanes;
			int num = Random.Range(0, list.Count);
			int i = 0;
			while (i < list.Count)
			{
				int num2 = (i + num) % list.Count;
				BuilderAttachGridPlane builderAttachGridPlane2 = list[num2];
				if (builderAttachGridPlane2.male != builderAttachGridPlane.male && !(builderAttachGridPlane2.piece == builderAttachGridPlane.piece) && !this.ShareSameRoot(builderAttachGridPlane, builderAttachGridPlane2))
				{
					Vector3 zero = Vector3.zero;
					Quaternion identity = Quaternion.identity;
					BuilderPiece piece2 = builderAttachGridPlane2.piece;
					int attachIndex = builderAttachGridPlane2.attachIndex;
					Transform center = builderAttachGridPlane.center;
					Quaternion quaternion = builderAttachGridPlane2.transform.rotation * Quaternion.Inverse(center.localRotation);
					Vector3 vector = piece.transform.InverseTransformPoint(center.position);
					Vector3 vector2 = builderAttachGridPlane2.transform.position - quaternion * vector;
					if (piece2 != null)
					{
						BuilderAttachGridPlane builderAttachGridPlane3 = piece2.gridPlanes[attachIndex];
						Vector3 lossyScale = builderAttachGridPlane3.transform.lossyScale;
						Vector3 vector3 = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
						Quaternion.Inverse(builderAttachGridPlane3.transform.rotation) * Vector3.Scale(vector2 - builderAttachGridPlane3.transform.position, vector3);
						Quaternion.Inverse(builderAttachGridPlane3.transform.rotation) * quaternion;
						return;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}

		public void UseResources(BuilderPiece piece)
		{
			BuilderResources cost = piece.cost;
			if (cost == null)
			{
				return;
			}
			for (int i = 0; i < cost.quantities.Count; i++)
			{
				this.UseResource(cost.quantities[i]);
			}
		}

		private void UseResource(BuilderResourceQuantity quantity)
		{
			if (quantity.type < BuilderResourceType.Basic || quantity.type >= BuilderResourceType.Count)
			{
				return;
			}
			this.usedResources[(int)quantity.type] += quantity.count;
			if (this.tableState == BuilderTable.TableState.Ready)
			{
				this.OnAvailableResourcesChange();
			}
		}

		public void AddResources(BuilderPiece piece)
		{
			BuilderResources cost = piece.cost;
			if (cost == null)
			{
				return;
			}
			for (int i = 0; i < cost.quantities.Count; i++)
			{
				this.AddResource(cost.quantities[i]);
			}
		}

		private void AddResource(BuilderResourceQuantity quantity)
		{
			if (quantity.type < BuilderResourceType.Basic || quantity.type >= BuilderResourceType.Count)
			{
				return;
			}
			this.usedResources[(int)quantity.type] -= quantity.count;
			if (this.tableState == BuilderTable.TableState.Ready)
			{
				this.OnAvailableResourcesChange();
			}
		}

		public bool HasEnoughUnreservedResources(BuilderResources resources)
		{
			if (resources == null)
			{
				return false;
			}
			for (int i = 0; i < resources.quantities.Count; i++)
			{
				if (!this.HasEnoughUnreservedResource(resources.quantities[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool HasEnoughUnreservedResource(BuilderResourceQuantity quantity)
		{
			return quantity.type >= BuilderResourceType.Basic && quantity.type < BuilderResourceType.Count && this.usedResources[(int)quantity.type] + this.reservedResources[(int)quantity.type] + quantity.count <= this.maxResources[(int)quantity.type];
		}

		public bool HasEnoughResources(BuilderPiece piece)
		{
			BuilderResources cost = piece.cost;
			if (cost == null)
			{
				return false;
			}
			for (int i = 0; i < cost.quantities.Count; i++)
			{
				if (!this.HasEnoughResource(cost.quantities[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool HasEnoughResource(BuilderResourceQuantity quantity)
		{
			return quantity.type >= BuilderResourceType.Basic && quantity.type < BuilderResourceType.Count && this.usedResources[(int)quantity.type] + quantity.count <= this.maxResources[(int)quantity.type];
		}

		public int GetAvailableResources(BuilderResourceType type)
		{
			if (type < BuilderResourceType.Basic || type >= BuilderResourceType.Count)
			{
				return 0;
			}
			return this.maxResources[(int)type] - this.usedResources[(int)type];
		}

		private void OnAvailableResourcesChange()
		{
			if (this.isSetup && this.isTableMutable)
			{
				for (int i = 0; i < this.conveyors.Count; i++)
				{
					this.conveyors[i].OnAvailableResourcesChange();
				}
				foreach (BuilderResourceMeter builderResourceMeter in this.resourceMeters)
				{
					builderResourceMeter.OnAvailableResourcesChange();
				}
			}
		}

		public int GetPrivateResourceLimitForType(int type)
		{
			if (this.plotMaxResources == null)
			{
				return 0;
			}
			return this.plotMaxResources[type];
		}

		private void WriteVector3(BinaryWriter writer, Vector3 data)
		{
			writer.Write(data.x);
			writer.Write(data.y);
			writer.Write(data.z);
		}

		private void WriteQuaternion(BinaryWriter writer, Quaternion data)
		{
			writer.Write(data.x);
			writer.Write(data.y);
			writer.Write(data.z);
			writer.Write(data.w);
		}

		private Vector3 ReadVector3(BinaryReader reader)
		{
			Vector3 vector;
			vector.x = reader.ReadSingle();
			vector.y = reader.ReadSingle();
			vector.z = reader.ReadSingle();
			return vector;
		}

		private Quaternion ReadQuaternion(BinaryReader reader)
		{
			Quaternion quaternion;
			quaternion.x = reader.ReadSingle();
			quaternion.y = reader.ReadSingle();
			quaternion.z = reader.ReadSingle();
			quaternion.w = reader.ReadSingle();
			return quaternion;
		}

		public static int PackPiecePlacement(byte twist, sbyte xOffset, sbyte zOffset)
		{
			int num = (int)(twist & 3);
			int num2 = (int)xOffset + 128;
			int num3 = (int)zOffset + 128;
			return num2 + (num3 << 8) + (num << 16);
		}

		public static void UnpackPiecePlacement(int packed, out byte twist, out sbyte xOffset, out sbyte zOffset)
		{
			int num = packed & 255;
			int num2 = (packed >> 8) & 255;
			int num3 = (packed >> 16) & 3;
			twist = (byte)num3;
			xOffset = (sbyte)(num - 128);
			zOffset = (sbyte)(num2 - 128);
		}

		private long PackSnapInfo(int attachGridIndex, int otherAttachGridIndex, Vector2Int min, Vector2Int max)
		{
			long num = (long)Mathf.Clamp(attachGridIndex, 0, 31);
			long num2 = (long)Mathf.Clamp(otherAttachGridIndex, 0, 31);
			long num3 = (long)Mathf.Clamp(min.x + 1024, 0, 2047);
			long num4 = (long)Mathf.Clamp(min.y + 1024, 0, 2047);
			long num5 = (long)Mathf.Clamp(max.x + 1024, 0, 2047);
			long num6 = (long)Mathf.Clamp(max.y + 1024, 0, 2047);
			return num + (num2 << 5) + (num3 << 10) + (num4 << 21) + (num5 << 32) + (num6 << 43);
		}

		private void UnpackSnapInfo(long packed, out int attachGridIndex, out int otherAttachGridIndex, out Vector2Int min, out Vector2Int max)
		{
			long num = packed & 31L;
			attachGridIndex = (int)num;
			num = (packed >> 5) & 31L;
			otherAttachGridIndex = (int)num;
			int num2 = (int)((packed >> 10) & 2047L) - 1024;
			int num3 = (int)((packed >> 21) & 2047L) - 1024;
			min = new Vector2Int(num2, num3);
			int num4 = (int)((packed >> 32) & 2047L) - 1024;
			int num5 = (int)((packed >> 43) & 2047L) - 1024;
			max = new Vector2Int(num4, num5);
		}

		private void OnTitleDataUpdate(string key)
		{
			if (key.Equals(this.SharedMapConfigTitleDataKey))
			{
				this.FetchSharedBlocksStartingMapConfig();
			}
		}

		private void FetchSharedBlocksStartingMapConfig()
		{
			if (!this.isTableMutable)
			{
				PlayFabTitleDataCache.Instance.GetTitleData(this.SharedMapConfigTitleDataKey, new Action<string>(this.OnGetStartingMapConfigSuccess), new Action<PlayFabError>(this.OnGetStartingMapConfigFail), false);
			}
		}

		private void OnGetStartingMapConfigSuccess(string result)
		{
			this.ResetStartingMapConfig();
			if (result.IsNullOrEmpty())
			{
				return;
			}
			try
			{
				SharedBlocksManager.StartingMapConfig startingMapConfig = JsonUtility.FromJson<SharedBlocksManager.StartingMapConfig>(result);
				if (startingMapConfig.useMapID)
				{
					if (SharedBlocksManager.IsMapIDValid(startingMapConfig.mapID))
					{
						this.startingMapConfig.useMapID = true;
						this.startingMapConfig.mapID = startingMapConfig.mapID;
					}
					else
					{
						GTDev.LogError<string>(string.Format("BuilderTable {0} OnGetStartingMapConfigSuccess Title Data Default Map Config has Invalid Map ID", this.tableZone), null);
					}
				}
				else
				{
					this.startingMapConfig.pageNumber = Mathf.Max(startingMapConfig.pageNumber, 0);
					this.startingMapConfig.pageSize = Mathf.Max(startingMapConfig.pageSize, 1);
					if (!startingMapConfig.sortMethod.IsNullOrEmpty() && (startingMapConfig.sortMethod.Equals(SharedBlocksManager.MapSortMethod.Top.ToString()) || startingMapConfig.sortMethod.Equals(SharedBlocksManager.MapSortMethod.NewlyCreated.ToString()) || startingMapConfig.sortMethod.Equals(SharedBlocksManager.MapSortMethod.RecentlyUpdated.ToString())))
					{
						this.startingMapConfig.sortMethod = startingMapConfig.sortMethod;
					}
					else
					{
						GTDev.LogError<string>("BuilderTable " + this.tableZone.ToString() + " OnGetStartingMapConfigSuccess Unknown sort method " + startingMapConfig.sortMethod, null);
					}
				}
			}
			catch (Exception ex)
			{
				GTDev.LogError<string>("BuilderTable " + this.tableZone.ToString() + " OnGetStartingMapConfigSuccess Exception Deserializing " + ex.Message, null);
			}
		}

		private void OnGetStartingMapConfigFail(PlayFabError error)
		{
			GTDev.LogWarning<string>("BuilderTable " + this.tableZone.ToString() + " OnGetStartingMapConfigFail " + error.Error.ToString(), null);
			this.ResetStartingMapConfig();
		}

		private void ResetStartingMapConfig()
		{
			this.startingMapConfig = new SharedBlocksManager.StartingMapConfig
			{
				pageNumber = 0,
				pageSize = 10,
				sortMethod = SharedBlocksManager.MapSortMethod.Top.ToString(),
				useMapID = false,
				mapID = null
			};
		}

		private void RequestTableConfiguration()
		{
			SharedBlocksManager.instance.OnGetTableConfiguration += this.OnGetTableConfiguration;
			SharedBlocksManager.instance.RequestTableConfiguration();
		}

		private void OnGetTableConfiguration(string configString)
		{
			SharedBlocksManager.instance.OnGetTableConfiguration -= this.OnGetTableConfiguration;
			if (!configString.IsNullOrEmpty())
			{
				this.ParseTableConfiguration(configString);
			}
		}

		private void ParseTableConfiguration(string dataRecord)
		{
			if (string.IsNullOrEmpty(dataRecord))
			{
				return;
			}
			BuilderTableConfiguration builderTableConfiguration = JsonUtility.FromJson<BuilderTableConfiguration>(dataRecord);
			if (builderTableConfiguration != null)
			{
				if (builderTableConfiguration.TableResourceLimits != null)
				{
					for (int i = 0; i < builderTableConfiguration.TableResourceLimits.Length; i++)
					{
						int num = builderTableConfiguration.TableResourceLimits[i];
						if (num >= 0)
						{
							this.maxResources[i] = num;
						}
					}
				}
				if (builderTableConfiguration.PlotResourceLimits != null)
				{
					for (int j = 0; j < builderTableConfiguration.PlotResourceLimits.Length; j++)
					{
						int num2 = builderTableConfiguration.PlotResourceLimits[j];
						if (num2 >= 0)
						{
							this.plotMaxResources[j] = num2;
						}
					}
				}
				int droppedPieceLimit = builderTableConfiguration.DroppedPieceLimit;
				if (droppedPieceLimit >= 0)
				{
					BuilderTable.DROPPED_PIECE_LIMIT = droppedPieceLimit;
				}
				if (builderTableConfiguration.updateCountdownDate != null && !string.IsNullOrEmpty(builderTableConfiguration.updateCountdownDate))
				{
					try
					{
						DateTime.Parse(builderTableConfiguration.updateCountdownDate, CultureInfo.InvariantCulture);
						BuilderTable.nextUpdateOverride = builderTableConfiguration.updateCountdownDate;
						goto IL_00DC;
					}
					catch
					{
						BuilderTable.nextUpdateOverride = string.Empty;
						goto IL_00DC;
					}
				}
				BuilderTable.nextUpdateOverride = string.Empty;
				IL_00DC:
				this.OnAvailableResourcesChange();
				UnityEvent onTableConfigurationUpdated = this.OnTableConfigurationUpdated;
				if (onTableConfigurationUpdated == null)
				{
					return;
				}
				onTableConfigurationUpdated.Invoke();
			}
		}

		private void DumpTableConfig()
		{
			BuilderTableConfiguration builderTableConfiguration = new BuilderTableConfiguration();
			Array.Clear(builderTableConfiguration.TableResourceLimits, 0, builderTableConfiguration.TableResourceLimits.Length);
			Array.Clear(builderTableConfiguration.PlotResourceLimits, 0, builderTableConfiguration.PlotResourceLimits.Length);
			foreach (BuilderResourceQuantity builderResourceQuantity in this.totalResources.quantities)
			{
				if (builderResourceQuantity.type >= BuilderResourceType.Basic && builderResourceQuantity.type < (BuilderResourceType)builderTableConfiguration.TableResourceLimits.Length)
				{
					builderTableConfiguration.TableResourceLimits[(int)builderResourceQuantity.type] = builderResourceQuantity.count;
				}
			}
			foreach (BuilderResourceQuantity builderResourceQuantity2 in this.resourcesPerPrivatePlot.quantities)
			{
				if (builderResourceQuantity2.type >= BuilderResourceType.Basic && builderResourceQuantity2.type < (BuilderResourceType)builderTableConfiguration.PlotResourceLimits.Length)
				{
					builderTableConfiguration.PlotResourceLimits[(int)builderResourceQuantity2.type] = builderResourceQuantity2.count;
				}
			}
			builderTableConfiguration.DroppedPieceLimit = BuilderTable.DROPPED_PIECE_LIMIT;
			builderTableConfiguration.updateCountdownDate = "1/10/2025 16:00:00";
			string text = JsonUtility.ToJson(builderTableConfiguration);
			Debug.Log("Configuration Dump \n" + text);
		}

		private string GetSaveDataTimeKey(int slot)
		{
			return BuilderTable.personalBuildKey + slot.ToString("D2") + "Time";
		}

		private string GetSaveDataKey(int slot)
		{
			return BuilderTable.personalBuildKey + slot.ToString("D2");
		}

		public void FindAndLoadSharedBlocksMap(string mapID)
		{
			SharedBlocksManager.instance.RequestMapDataFromID(mapID, new SharedBlocksManager.BlocksMapRequestCallback(this.FoundSharedBlocksMap));
		}

		public string GetSharedBlocksMapID()
		{
			if (this.sharedBlocksMap != null)
			{
				return this.sharedBlocksMap.MapID;
			}
			return string.Empty;
		}

		private void FoundSharedBlocksMap(SharedBlocksManager.SharedBlocksMap map)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (map == null || map.MapData.IsNullOrEmpty())
			{
				this.builderNetworking.LoadSharedBlocksFailedMaster((map == null) ? string.Empty : map.MapID);
				this.sharedBlocksMap = null;
				this.tableData = new BuilderTableData();
				this.ClearTable();
				this.ClearQueuedCommands();
				this.SetTableState(BuilderTable.TableState.Ready);
				return;
			}
			this.sharedBlocksMap = map;
			this.SetTableState(BuilderTable.TableState.WaitForInitialBuildMaster);
		}

		private void BuildInitialTableForPlayer()
		{
			if (NetworkSystem.Instance.IsNull() || !NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.SessionIsPrivate || NetworkSystem.Instance.GetLocalPlayer() == null || !NetworkSystem.Instance.IsMasterClient)
			{
				this.TryBuildingFromTitleData();
				return;
			}
			if (!BuilderScanKiosk.IsSaveSlotValid(this.currentSaveSlot))
			{
				this.TryBuildingFromTitleData();
				return;
			}
			SharedBlocksManager.instance.OnFetchPrivateScanComplete += this.OnFetchPrivateScanComplete;
			SharedBlocksManager.instance.RequestFetchPrivateScan(this.currentSaveSlot);
		}

		private void OnFetchPrivateScanComplete(int slot, bool success)
		{
			SharedBlocksManager.instance.OnFetchPrivateScanComplete -= this.OnFetchPrivateScanComplete;
			if (this.tableState != BuilderTable.TableState.WaitForInitialBuildMaster)
			{
				return;
			}
			string text;
			if (!success || !SharedBlocksManager.instance.TryGetPrivateScanResponse(slot, out text))
			{
				this.TryBuildingFromTitleData();
				return;
			}
			if (!this.BuildTableFromJson(text, false))
			{
				this.TryBuildingFromTitleData();
				return;
			}
			this.SetIsDirty(false);
			this.OnFinishedInitialTableBuild();
		}

		private void BuildSelectedSharedMap()
		{
			if (!NetworkSystem.Instance.IsNull() && NetworkSystem.Instance.InRoom && NetworkSystem.Instance.IsMasterClient)
			{
				if (this.sharedBlocksMap != null && !this.sharedBlocksMap.MapData.IsNullOrEmpty())
				{
					this.TryBuildingSharedBlocksMap(this.sharedBlocksMap.MapData);
					return;
				}
				if (SharedBlocksManager.IsMapIDValid(this.pendingMapID))
				{
					SharedBlocksManager.SharedBlocksMap sharedBlocksMap = new SharedBlocksManager.SharedBlocksMap
					{
						MapID = this.pendingMapID
					};
					this.LoadSharedMap(sharedBlocksMap);
					return;
				}
				this.FindStartingMap();
			}
		}

		private void FindStartingMap()
		{
			if (this.hasStartingMap && Time.timeAsDouble < this.startingMapCacheTime + 60.0)
			{
				this.FoundDefaultSharedBlocksMap(true, this.startingMap);
				return;
			}
			if (this.getStartingMapInProgress)
			{
				return;
			}
			this.hasStartingMap = false;
			this.getStartingMapInProgress = true;
			if (this.startingMapConfig.useMapID && SharedBlocksManager.IsMapIDValid(this.startingMapConfig.mapID))
			{
				this.startingMap = new SharedBlocksManager.SharedBlocksMap
				{
					MapID = this.startingMapConfig.mapID
				};
				SharedBlocksManager.instance.RequestMapDataFromID(this.startingMapConfig.mapID, new SharedBlocksManager.BlocksMapRequestCallback(this.FoundTopMapData));
				return;
			}
			if (this.hasCachedTopMaps && Time.timeAsDouble <= this.lastGetTopMapsTime + 60.0)
			{
				this.ChooseMapFromList();
				return;
			}
			SharedBlocksManager.instance.OnGetPopularMapsComplete += this.FoundStartingMapList;
			if (!SharedBlocksManager.instance.RequestGetTopMaps(this.startingMapConfig.pageNumber, this.startingMapConfig.pageSize, this.startingMapConfig.sortMethod.ToString()))
			{
				this.FoundStartingMapList(false);
			}
		}

		private void FoundStartingMapList(bool success)
		{
			SharedBlocksManager.instance.OnGetPopularMapsComplete -= this.FoundStartingMapList;
			if (success && SharedBlocksManager.instance.LatestPopularMaps.Count > 0)
			{
				this.startingMapList.Clear();
				this.startingMapList.AddRange(SharedBlocksManager.instance.LatestPopularMaps);
				this.hasCachedTopMaps = this.startingMapList.Count > 0;
				this.lastGetTopMapsTime = (double)Time.time;
				this.ChooseMapFromList();
				return;
			}
			this.FoundDefaultSharedBlocksMap(false, null);
		}

		private void ChooseMapFromList()
		{
			int num = Random.Range(0, this.startingMapList.Count);
			this.startingMap = this.startingMapList[num];
			if (this.startingMap == null || !SharedBlocksManager.IsMapIDValid(this.startingMap.MapID))
			{
				this.FoundDefaultSharedBlocksMap(false, null);
				return;
			}
			SharedBlocksManager.instance.RequestMapDataFromID(this.startingMap.MapID, new SharedBlocksManager.BlocksMapRequestCallback(this.FoundTopMapData));
		}

		private void FoundTopMapData(SharedBlocksManager.SharedBlocksMap map)
		{
			if (map == null || !SharedBlocksManager.IsMapIDValid(map.MapID) || map.MapID != this.startingMap.MapID)
			{
				this.FoundDefaultSharedBlocksMap(false, null);
				return;
			}
			this.hasStartingMap = true;
			this.startingMapCacheTime = Time.timeAsDouble;
			this.startingMap.MapData = map.MapData;
			this.FoundDefaultSharedBlocksMap(true, this.startingMap);
		}

		private void FoundDefaultSharedBlocksMap(bool success, SharedBlocksManager.SharedBlocksMap map)
		{
			this.getStartingMapInProgress = false;
			if (success && !map.MapData.IsNullOrEmpty())
			{
				this.startingMapCacheTime = Time.timeAsDouble;
				this.startingMap = map;
				this.hasStartingMap = true;
				this.sharedBlocksMap = map;
				this.TryBuildingSharedBlocksMap(this.sharedBlocksMap.MapData);
				return;
			}
			this.TryBuildingFromTitleData();
		}

		private void TryBuildingSharedBlocksMap(string mapData)
		{
			if (this.tableState != BuilderTable.TableState.WaitForInitialBuildMaster)
			{
				return;
			}
			if (!this.BuildTableFromJson(mapData, true))
			{
				GTDev.LogWarning<string>("Unable to build shared blocks map", null);
				this.builderNetworking.LoadSharedBlocksFailedMaster(this.sharedBlocksMap.MapID);
				this.sharedBlocksMap = null;
				this.tableData = new BuilderTableData();
				this.ClearTable();
				this.ClearQueuedCommands();
				this.SetTableState(BuilderTable.TableState.Ready);
				return;
			}
			base.StartCoroutine(this.CheckForNoBlocks());
		}

		private IEnumerator CheckForNoBlocks()
		{
			yield return null;
			if (!this.NoBlocksCheck())
			{
				GTDev.LogError<string>("Failed No Blocks Check", null);
				this.builderNetworking.SharedBlocksOutOfBoundsMaster(this.sharedBlocksMap.MapID);
				this.sharedBlocksMap = null;
				this.tableData = new BuilderTableData();
				this.ClearTable();
				this.ClearQueuedCommands();
				this.SetTableState(BuilderTable.TableState.Ready);
				yield break;
			}
			this.OnFinishedInitialTableBuild();
			yield break;
		}

		private void TryBuildingFromTitleData()
		{
			SharedBlocksManager.instance.OnGetTitleDataBuildComplete += this.OnGetTitleDataBuildComplete;
			SharedBlocksManager.instance.FetchTitleDataBuild();
		}

		private void OnGetTitleDataBuildComplete(string titleDataBuild)
		{
			SharedBlocksManager.instance.OnGetTitleDataBuildComplete -= this.OnGetTitleDataBuildComplete;
			if (this.tableState != BuilderTable.TableState.WaitForInitialBuildMaster)
			{
				return;
			}
			if (!titleDataBuild.IsNullOrEmpty())
			{
				if (!this.BuildTableFromJson(titleDataBuild, true))
				{
					this.tableData = new BuilderTableData();
				}
			}
			else
			{
				this.tableData = new BuilderTableData();
			}
			this.OnFinishedInitialTableBuild();
		}

		public void SaveTableForPlayer(string busyStr, string blocksErrStr)
		{
			if (SharedBlocksManager.instance.IsWaitingOnRequest())
			{
				this.SetIsDirty(true);
				UnityEvent<string> onSaveFailure = this.OnSaveFailure;
				if (onSaveFailure == null)
				{
					return;
				}
				onSaveFailure.Invoke(busyStr);
				return;
			}
			else
			{
				this.saveInProgress = true;
				if (!BuilderScanKiosk.IsSaveSlotValid(this.currentSaveSlot))
				{
					this.saveInProgress = false;
					return;
				}
				if (!this.isDirty)
				{
					this.saveInProgress = false;
					UnityEvent onSaveTimeUpdated = this.OnSaveTimeUpdated;
					if (onSaveTimeUpdated == null)
					{
						return;
					}
					onSaveTimeUpdated.Invoke();
					return;
				}
				else
				{
					if (this.NoBlocksCheck())
					{
						if (this.tableData == null)
						{
							this.tableData = new BuilderTableData();
						}
						this.SetIsDirty(false);
						this.tableData.numEdits++;
						string text = this.WriteTableToJson();
						text = Convert.ToBase64String(GZipStream.CompressString(text));
						SharedBlocksManager.instance.OnSavePrivateScanSuccess += this.OnSaveScanSuccess;
						SharedBlocksManager.instance.OnSavePrivateScanFailed += this.OnSaveScanFailure;
						SharedBlocksManager.instance.RequestSavePrivateScan(this.currentSaveSlot, text);
						return;
					}
					this.saveInProgress = false;
					this.SetIsDirty(true);
					UnityEvent<string> onSaveFailure2 = this.OnSaveFailure;
					if (onSaveFailure2 == null)
					{
						return;
					}
					onSaveFailure2.Invoke(blocksErrStr);
					return;
				}
			}
		}

		private void OnSaveScanSuccess(int scan)
		{
			SharedBlocksManager.instance.OnSavePrivateScanSuccess -= this.OnSaveScanSuccess;
			SharedBlocksManager.instance.OnSavePrivateScanFailed -= this.OnSaveScanFailure;
			this.saveInProgress = false;
			UnityEvent onSaveSuccess = this.OnSaveSuccess;
			if (onSaveSuccess == null)
			{
				return;
			}
			onSaveSuccess.Invoke();
		}

		private void OnSaveScanFailure(int scan, string message)
		{
			SharedBlocksManager.instance.OnSavePrivateScanSuccess -= this.OnSaveScanSuccess;
			SharedBlocksManager.instance.OnSavePrivateScanFailed -= this.OnSaveScanFailure;
			this.saveInProgress = false;
			this.SetIsDirty(true);
			UnityEvent<string> onSaveFailure = this.OnSaveFailure;
			if (onSaveFailure == null)
			{
				return;
			}
			onSaveFailure.Invoke(message);
		}

		private string WriteTableToJson()
		{
			this.tableData.Clear();
			BuilderTable.tempDuplicateOverlaps.Clear();
			for (int i = 0; i < this.pieces.Count; i++)
			{
				if (this.pieces[i].state == BuilderPiece.State.AttachedAndPlaced)
				{
					this.tableData.pieceType.Add(this.pieces[i].overrideSavedPiece ? this.pieces[i].savedPieceType : this.pieces[i].pieceType);
					this.tableData.pieceId.Add(this.pieces[i].pieceId);
					this.tableData.parentId.Add((this.pieces[i].parentPiece == null) ? (-1) : this.pieces[i].parentPiece.pieceId);
					this.tableData.attachIndex.Add(this.pieces[i].attachIndex);
					this.tableData.parentAttachIndex.Add((this.pieces[i].parentPiece == null) ? (-1) : this.pieces[i].parentAttachIndex);
					this.tableData.placement.Add(this.pieces[i].GetPiecePlacement());
					this.tableData.materialType.Add(this.pieces[i].overrideSavedPiece ? this.pieces[i].savedMaterialType : this.pieces[i].materialType);
					BuilderMovingSnapPiece component = this.pieces[i].GetComponent<BuilderMovingSnapPiece>();
					int num = ((component == null) ? 0 : component.GetTimeOffset());
					this.tableData.timeOffset.Add(num);
					for (int j = 0; j < this.pieces[i].gridPlanes.Count; j++)
					{
						if (!(this.pieces[i].gridPlanes[j] == null))
						{
							for (SnapOverlap snapOverlap = this.pieces[i].gridPlanes[j].firstOverlap; snapOverlap != null; snapOverlap = snapOverlap.nextOverlap)
							{
								if (snapOverlap.otherPlane.piece.state == BuilderPiece.State.AttachedAndPlaced || snapOverlap.otherPlane.piece.isBuiltIntoTable)
								{
									BuilderTable.SnapOverlapKey snapOverlapKey = BuilderTable.BuildOverlapKey(this.pieces[i].pieceId, snapOverlap.otherPlane.piece.pieceId, j, snapOverlap.otherPlane.attachIndex);
									if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey))
									{
										BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey);
										long num2 = this.PackSnapInfo(j, snapOverlap.otherPlane.attachIndex, snapOverlap.bounds.min, snapOverlap.bounds.max);
										this.tableData.overlapingPieces.Add(this.pieces[i].pieceId);
										this.tableData.overlappedPieces.Add(snapOverlap.otherPlane.piece.pieceId);
										this.tableData.overlapInfo.Add(num2);
									}
								}
							}
						}
					}
				}
			}
			foreach (BuilderPiece builderPiece in this.basePieces)
			{
				if (!(builderPiece == null))
				{
					for (int k = 0; k < builderPiece.gridPlanes.Count; k++)
					{
						if (!(builderPiece.gridPlanes[k] == null))
						{
							for (SnapOverlap snapOverlap2 = builderPiece.gridPlanes[k].firstOverlap; snapOverlap2 != null; snapOverlap2 = snapOverlap2.nextOverlap)
							{
								if (snapOverlap2.otherPlane.piece.state == BuilderPiece.State.AttachedAndPlaced || snapOverlap2.otherPlane.piece.isBuiltIntoTable)
								{
									BuilderTable.SnapOverlapKey snapOverlapKey2 = BuilderTable.BuildOverlapKey(builderPiece.pieceId, snapOverlap2.otherPlane.piece.pieceId, k, snapOverlap2.otherPlane.attachIndex);
									if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey2))
									{
										BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey2);
										long num3 = this.PackSnapInfo(k, snapOverlap2.otherPlane.attachIndex, snapOverlap2.bounds.min, snapOverlap2.bounds.max);
										this.tableData.overlapingPieces.Add(builderPiece.pieceId);
										this.tableData.overlappedPieces.Add(snapOverlap2.otherPlane.piece.pieceId);
										this.tableData.overlapInfo.Add(num3);
									}
								}
							}
						}
					}
				}
			}
			BuilderTable.tempDuplicateOverlaps.Clear();
			this.tableData.numPieces = this.tableData.pieceType.Count;
			return JsonUtility.ToJson(this.tableData);
		}

		private static BuilderTable.SnapOverlapKey BuildOverlapKey(int pieceId, int otherPieceId, int attachGridIndex, int otherAttachGridIndex)
		{
			BuilderTable.SnapOverlapKey snapOverlapKey = default(BuilderTable.SnapOverlapKey);
			snapOverlapKey.piece = (long)pieceId;
			snapOverlapKey.piece <<= 32;
			snapOverlapKey.piece |= (long)attachGridIndex;
			snapOverlapKey.otherPiece = (long)otherPieceId;
			snapOverlapKey.otherPiece <<= 32;
			snapOverlapKey.otherPiece |= (long)otherAttachGridIndex;
			return snapOverlapKey;
		}

		private bool BuildTableFromJson(string tableJson, bool fromTitleData)
		{
			if (string.IsNullOrEmpty(tableJson))
			{
				return false;
			}
			this.tableData = null;
			try
			{
				this.tableData = JsonUtility.FromJson<BuilderTableData>(tableJson);
			}
			catch
			{
			}
			try
			{
				if (this.tableData == null)
				{
					tableJson = GZipStream.UncompressString(Convert.FromBase64String(tableJson));
					this.tableData = JsonUtility.FromJson<BuilderTableData>(tableJson);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
				return false;
			}
			if (this.tableData == null)
			{
				return false;
			}
			if (this.tableData.version < 4)
			{
				return false;
			}
			int num = ((this.tableData.pieceType == null) ? 0 : this.tableData.pieceType.Count);
			if (num == 0)
			{
				this.OnDeserializeUpdatePlots();
				return true;
			}
			if (this.tableData.pieceId == null || this.tableData.pieceId.Count != num || this.tableData.placement == null || this.tableData.placement.Count != num)
			{
				GTDev.LogError<string>("BuildTableFromJson Piece Count Mismatch", null);
				return false;
			}
			if (num >= this.maxResources[0])
			{
				GTDev.LogError<string>(string.Format("BuildTableFromJson Failed sanity piece count check {0}", num), null);
				return false;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>(num);
			bool flag = this.tableData.timeOffset != null && this.tableData.timeOffset.Count > 0;
			if (flag && this.tableData.timeOffset.Count != num)
			{
				GTDev.LogError<string>("BuildTableFromJson Piece Count Mismatch (Time Offsets)", null);
				return false;
			}
			int i = 0;
			while (i < this.tableData.pieceType.Count)
			{
				int num2 = this.CreatePieceId();
				if (!dictionary.TryAdd(this.tableData.pieceId[i], num2))
				{
					GTDev.LogError<string>("BuildTableFromJson Piece id duplicate in save", null);
					this.ClearTable();
					return false;
				}
				int num3 = ((this.tableData.materialType != null && this.tableData.materialType.Count > i) ? this.tableData.materialType[i] : (-1));
				int num4 = this.tableData.pieceType[i];
				int num5 = num3;
				bool flag2 = true;
				BuilderPiece piecePrefab = this.GetPiecePrefab(this.tableData.pieceType[i]);
				if (piecePrefab == null)
				{
					this.ClearTable();
					return false;
				}
				if (fromTitleData)
				{
					goto IL_02B2;
				}
				if (num5 == -1 && piecePrefab.materialOptions != null)
				{
					int num6;
					Material material;
					int num7;
					piecePrefab.materialOptions.GetDefaultMaterial(out num6, out material, out num7);
					num5 = num6;
				}
				flag2 = BuilderSetManager.instance.IsPieceOwnedLocally(this.tableData.pieceType[i], num5);
				if (!fromTitleData && !flag2)
				{
					if (!piecePrefab.fallbackInfo.materialSwapThisPrefab)
					{
						if (piecePrefab.fallbackInfo.prefab == null)
						{
							goto IL_03E0;
						}
						num4 = piecePrefab.fallbackInfo.prefab.name.GetStaticHash();
					}
					num5 = -1;
				}
				goto IL_02B2;
				IL_03E0:
				i++;
				continue;
				IL_02B2:
				if (piecePrefab.cost != null && piecePrefab.cost.quantities != null)
				{
					for (int j = 0; j < piecePrefab.cost.quantities.Count; j++)
					{
						BuilderResourceQuantity builderResourceQuantity = piecePrefab.cost.quantities[j];
						if (!this.HasEnoughResource(builderResourceQuantity))
						{
							if (builderResourceQuantity.type == BuilderResourceType.Basic)
							{
								this.ClearTable();
								GTDev.LogError<string>("BuildTableFromJson saved table uses too many basic resource", null);
								return false;
							}
							GTDev.LogWarning<string>("BuildTableFromJson saved table uses too many functional or decorative resource", null);
						}
					}
				}
				int num8 = (flag ? this.tableData.timeOffset[i] : 0);
				BuilderPiece builderPiece = this.CreatePieceInternal(num4, num2, Vector3.zero, Quaternion.identity, BuilderPiece.State.AttachedAndPlaced, num5, NetworkSystem.Instance.ServerTimestamp - num8, this);
				if (builderPiece == null)
				{
					this.ClearTable();
					GTDev.LogError<string>(string.Format("Piece Type {0} is not defined", this.tableData.pieceType[i]), null);
					return false;
				}
				if (!fromTitleData && !flag2)
				{
					builderPiece.overrideSavedPiece = true;
					builderPiece.savedPieceType = this.tableData.pieceType[i];
					builderPiece.savedMaterialType = num3;
				}
				goto IL_03E0;
			}
			for (int k = 0; k < this.tableData.pieceType.Count; k++)
			{
				int num9 = ((this.tableData.parentAttachIndex == null || this.tableData.parentAttachIndex.Count <= k) ? (-1) : this.tableData.parentAttachIndex[k]);
				int num10 = ((this.tableData.attachIndex == null || this.tableData.attachIndex.Count <= k) ? (-1) : this.tableData.attachIndex[k]);
				int valueOrDefault = dictionary.GetValueOrDefault(this.tableData.pieceId[k], -1);
				int num11 = -1;
				int num12;
				if (dictionary.TryGetValue(this.tableData.parentId[k], out num12))
				{
					num11 = num12;
				}
				else if (this.tableData.parentId[k] < 10000 && this.tableData.parentId[k] >= 5)
				{
					num11 = this.tableData.parentId[k];
				}
				this.AttachPieceInternal(valueOrDefault, num10, num11, num9, this.tableData.placement[k]);
			}
			foreach (BuilderPiece builderPiece2 in this.pieces)
			{
				if (builderPiece2.state == BuilderPiece.State.AttachedAndPlaced)
				{
					builderPiece2.OnPlacementDeserialized();
				}
			}
			this.OnDeserializeUpdatePlots();
			BuilderTable.tempDuplicateOverlaps.Clear();
			if (this.tableData.overlapingPieces != null)
			{
				int num13 = 0;
				while (num13 < this.tableData.overlapingPieces.Count && num13 < this.tableData.overlappedPieces.Count && num13 < this.tableData.overlapInfo.Count)
				{
					int num14 = -1;
					int num15;
					if (dictionary.TryGetValue(this.tableData.overlapingPieces[num13], out num15))
					{
						num14 = num15;
					}
					else if (this.tableData.overlapingPieces[num13] < 10000 && this.tableData.overlapingPieces[num13] >= 5)
					{
						num14 = this.tableData.overlapingPieces[num13];
					}
					int num16 = -1;
					int num17;
					if (dictionary.TryGetValue(this.tableData.overlappedPieces[num13], out num17))
					{
						num16 = num17;
					}
					else if (this.tableData.overlappedPieces[num13] < 10000 && this.tableData.overlappedPieces[num13] >= 5)
					{
						num16 = this.tableData.overlappedPieces[num13];
					}
					if (num14 != -1 && num16 != -1)
					{
						long num18 = this.tableData.overlapInfo[num13];
						BuilderPiece piece = this.GetPiece(num14);
						if (!(piece == null))
						{
							BuilderPiece piece2 = this.GetPiece(num16);
							if (!(piece2 == null))
							{
								int num19;
								int num20;
								Vector2Int vector2Int;
								Vector2Int vector2Int2;
								this.UnpackSnapInfo(num18, out num19, out num20, out vector2Int, out vector2Int2);
								if (num19 >= 0 && num19 < piece.gridPlanes.Count && num20 >= 0 && num20 < piece2.gridPlanes.Count)
								{
									BuilderTable.SnapOverlapKey snapOverlapKey = BuilderTable.BuildOverlapKey(num14, num16, num19, num20);
									if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey))
									{
										BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey);
										piece.gridPlanes[num19].AddSnapOverlap(this.builderPool.CreateSnapOverlap(piece2.gridPlanes[num20], new SnapBounds(vector2Int, vector2Int2)));
									}
								}
							}
						}
					}
					num13++;
				}
			}
			BuilderTable.tempDuplicateOverlaps.Clear();
			return true;
		}

		public int SerializeTableState(byte[] bytes, int maxBytes)
		{
			MemoryStream memoryStream = new MemoryStream(bytes);
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			if (this.conveyors == null)
			{
				binaryWriter.Write(0);
			}
			else
			{
				binaryWriter.Write(this.conveyors.Count);
				foreach (BuilderConveyor builderConveyor in this.conveyors)
				{
					int selectedDisplayGroupID = builderConveyor.GetSelectedDisplayGroupID();
					binaryWriter.Write(selectedDisplayGroupID);
				}
			}
			if (this.dispenserShelves == null)
			{
				binaryWriter.Write(0);
			}
			else
			{
				binaryWriter.Write(this.dispenserShelves.Count);
				foreach (BuilderDispenserShelf builderDispenserShelf in this.dispenserShelves)
				{
					int selectedDisplayGroupID2 = builderDispenserShelf.GetSelectedDisplayGroupID();
					binaryWriter.Write(selectedDisplayGroupID2);
				}
			}
			BuilderTable.childPieces.Clear();
			BuilderTable.rootPieces.Clear();
			BuilderTable.childPieces.EnsureCapacity(this.pieces.Count);
			BuilderTable.rootPieces.EnsureCapacity(this.pieces.Count);
			foreach (BuilderPiece builderPiece in this.pieces)
			{
				if (builderPiece.parentPiece == null)
				{
					BuilderTable.rootPieces.Add(builderPiece);
				}
				else
				{
					BuilderTable.childPieces.Add(builderPiece);
				}
			}
			binaryWriter.Write(BuilderTable.rootPieces.Count);
			for (int i = 0; i < BuilderTable.rootPieces.Count; i++)
			{
				BuilderPiece builderPiece2 = BuilderTable.rootPieces[i];
				binaryWriter.Write(builderPiece2.pieceType);
				binaryWriter.Write(builderPiece2.pieceId);
				binaryWriter.Write((byte)builderPiece2.state);
				if (builderPiece2.state == BuilderPiece.State.OnConveyor || builderPiece2.state == BuilderPiece.State.OnShelf || builderPiece2.state == BuilderPiece.State.Displayed)
				{
					binaryWriter.Write(builderPiece2.shelfOwner);
				}
				else
				{
					binaryWriter.Write(builderPiece2.heldByPlayerActorNumber);
				}
				binaryWriter.Write(builderPiece2.heldInLeftHand ? 1 : 0);
				binaryWriter.Write(builderPiece2.materialType);
				long num = BitPackUtils.PackWorldPosForNetwork(builderPiece2.transform.localPosition);
				int num2 = BitPackUtils.PackQuaternionForNetwork(builderPiece2.transform.localRotation);
				binaryWriter.Write(num);
				binaryWriter.Write(num2);
				if (builderPiece2.state == BuilderPiece.State.AttachedAndPlaced)
				{
					binaryWriter.Write(builderPiece2.functionalPieceState);
					binaryWriter.Write(builderPiece2.activatedTimeStamp);
				}
				if (builderPiece2.state == BuilderPiece.State.OnConveyor)
				{
					binaryWriter.Write((this.conveyorManager == null) ? 0 : this.conveyorManager.GetPieceCreateTimestamp(builderPiece2));
				}
			}
			binaryWriter.Write(BuilderTable.childPieces.Count);
			for (int j = 0; j < BuilderTable.childPieces.Count; j++)
			{
				BuilderPiece builderPiece3 = BuilderTable.childPieces[j];
				binaryWriter.Write(builderPiece3.pieceType);
				binaryWriter.Write(builderPiece3.pieceId);
				int num3 = ((builderPiece3.parentPiece == null) ? (-1) : builderPiece3.parentPiece.pieceId);
				binaryWriter.Write(num3);
				binaryWriter.Write(builderPiece3.attachIndex);
				binaryWriter.Write(builderPiece3.parentAttachIndex);
				binaryWriter.Write((byte)builderPiece3.state);
				if (builderPiece3.state == BuilderPiece.State.OnConveyor || builderPiece3.state == BuilderPiece.State.OnShelf || builderPiece3.state == BuilderPiece.State.Displayed)
				{
					binaryWriter.Write(builderPiece3.shelfOwner);
				}
				else
				{
					binaryWriter.Write(builderPiece3.heldByPlayerActorNumber);
				}
				binaryWriter.Write(builderPiece3.heldInLeftHand ? 1 : 0);
				binaryWriter.Write(builderPiece3.materialType);
				int piecePlacement = builderPiece3.GetPiecePlacement();
				binaryWriter.Write(piecePlacement);
				if (builderPiece3.state == BuilderPiece.State.AttachedAndPlaced)
				{
					binaryWriter.Write(builderPiece3.functionalPieceState);
					binaryWriter.Write(builderPiece3.activatedTimeStamp);
				}
				if (builderPiece3.state == BuilderPiece.State.OnConveyor)
				{
					binaryWriter.Write((this.conveyorManager == null) ? 0 : this.conveyorManager.GetPieceCreateTimestamp(builderPiece3));
				}
			}
			if (this.isTableMutable)
			{
				binaryWriter.Write(this.plotOwners.Count);
				using (Dictionary<int, int>.Enumerator enumerator4 = this.plotOwners.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						KeyValuePair<int, int> keyValuePair = enumerator4.Current;
						binaryWriter.Write(keyValuePair.Key);
						binaryWriter.Write(keyValuePair.Value);
					}
					goto IL_04F9;
				}
			}
			if (this.sharedBlocksMap == null || this.sharedBlocksMap.MapID == null || !SharedBlocksManager.IsMapIDValid(this.sharedBlocksMap.MapID))
			{
				for (int k = 0; k < BuilderTable.mapIDBuffer.Length; k++)
				{
					BuilderTable.mapIDBuffer[k] = 'a';
				}
			}
			else
			{
				for (int l = 0; l < BuilderTable.mapIDBuffer.Length; l++)
				{
					BuilderTable.mapIDBuffer[l] = this.sharedBlocksMap.MapID[l];
				}
			}
			binaryWriter.Write(BuilderTable.mapIDBuffer);
			IL_04F9:
			long position = memoryStream.Position;
			BuilderTable.overlapPieces.Clear();
			BuilderTable.overlapOtherPieces.Clear();
			BuilderTable.overlapPacked.Clear();
			BuilderTable.tempDuplicateOverlaps.Clear();
			foreach (BuilderPiece builderPiece4 in this.pieces)
			{
				if (!(builderPiece4 == null))
				{
					for (int m = 0; m < builderPiece4.gridPlanes.Count; m++)
					{
						if (!(builderPiece4.gridPlanes[m] == null))
						{
							for (SnapOverlap snapOverlap = builderPiece4.gridPlanes[m].firstOverlap; snapOverlap != null; snapOverlap = snapOverlap.nextOverlap)
							{
								BuilderTable.SnapOverlapKey snapOverlapKey = BuilderTable.BuildOverlapKey(builderPiece4.pieceId, snapOverlap.otherPlane.piece.pieceId, m, snapOverlap.otherPlane.attachIndex);
								if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey))
								{
									BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey);
									long num4 = this.PackSnapInfo(m, snapOverlap.otherPlane.attachIndex, snapOverlap.bounds.min, snapOverlap.bounds.max);
									BuilderTable.overlapPieces.Add(builderPiece4.pieceId);
									BuilderTable.overlapOtherPieces.Add(snapOverlap.otherPlane.piece.pieceId);
									BuilderTable.overlapPacked.Add(num4);
								}
							}
						}
					}
				}
			}
			foreach (BuilderPiece builderPiece5 in this.basePieces)
			{
				if (!(builderPiece5 == null))
				{
					for (int n = 0; n < builderPiece5.gridPlanes.Count; n++)
					{
						if (!(builderPiece5.gridPlanes[n] == null))
						{
							for (SnapOverlap snapOverlap2 = builderPiece5.gridPlanes[n].firstOverlap; snapOverlap2 != null; snapOverlap2 = snapOverlap2.nextOverlap)
							{
								BuilderTable.SnapOverlapKey snapOverlapKey2 = BuilderTable.BuildOverlapKey(builderPiece5.pieceId, snapOverlap2.otherPlane.piece.pieceId, n, snapOverlap2.otherPlane.attachIndex);
								if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey2))
								{
									BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey2);
									long num5 = this.PackSnapInfo(n, snapOverlap2.otherPlane.attachIndex, snapOverlap2.bounds.min, snapOverlap2.bounds.max);
									BuilderTable.overlapPieces.Add(builderPiece5.pieceId);
									BuilderTable.overlapOtherPieces.Add(snapOverlap2.otherPlane.piece.pieceId);
									BuilderTable.overlapPacked.Add(num5);
								}
							}
						}
					}
				}
			}
			BuilderTable.tempDuplicateOverlaps.Clear();
			binaryWriter.Write(BuilderTable.overlapPieces.Count);
			for (int num6 = 0; num6 < BuilderTable.overlapPieces.Count; num6++)
			{
				binaryWriter.Write(BuilderTable.overlapPieces[num6]);
				binaryWriter.Write(BuilderTable.overlapOtherPieces[num6]);
				binaryWriter.Write(BuilderTable.overlapPacked[num6]);
			}
			return (int)memoryStream.Position;
		}

		public void DeserializeTableState(byte[] bytes, int numBytes)
		{
			if (numBytes <= 0)
			{
				return;
			}
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
			BuilderTable.tempPeiceIds.Clear();
			BuilderTable.tempParentPeiceIds.Clear();
			BuilderTable.tempAttachIndexes.Clear();
			BuilderTable.tempParentAttachIndexes.Clear();
			BuilderTable.tempParentActorNumbers.Clear();
			BuilderTable.tempInLeftHand.Clear();
			BuilderTable.tempPiecePlacement.Clear();
			int num = binaryReader.ReadInt32();
			bool flag = this.conveyors != null;
			for (int i = 0; i < num; i++)
			{
				int num2 = binaryReader.ReadInt32();
				if (flag && i < this.conveyors.Count)
				{
					this.conveyors[i].SetSelection(num2);
				}
			}
			int num3 = binaryReader.ReadInt32();
			bool flag2 = this.dispenserShelves != null;
			for (int j = 0; j < num3; j++)
			{
				int num4 = binaryReader.ReadInt32();
				if (flag2 && j < this.dispenserShelves.Count)
				{
					this.dispenserShelves[j].SetSelection(num4);
				}
			}
			int num5 = binaryReader.ReadInt32();
			for (int k = 0; k < num5; k++)
			{
				int num6 = binaryReader.ReadInt32();
				int num7 = binaryReader.ReadInt32();
				BuilderPiece.State state = (BuilderPiece.State)binaryReader.ReadByte();
				int num8 = binaryReader.ReadInt32();
				bool flag3 = binaryReader.ReadByte() > 0;
				int num9 = binaryReader.ReadInt32();
				long num10 = binaryReader.ReadInt64();
				int num11 = binaryReader.ReadInt32();
				Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(num10);
				Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(num11);
				byte b = ((state == BuilderPiece.State.AttachedAndPlaced) ? binaryReader.ReadByte() : 0);
				int num12 = ((state == BuilderPiece.State.AttachedAndPlaced) ? binaryReader.ReadInt32() : 0);
				int num13 = ((state == BuilderPiece.State.OnConveyor) ? binaryReader.ReadInt32() : 0);
				float num14 = 10000f;
				if (!(in vector).IsValid(in num14) || !(in quaternion).IsValid() || !this.ValidateCreatePieceParams(num6, num7, state, num9))
				{
					this.SetTableState(BuilderTable.TableState.BadData);
					return;
				}
				int num15 = -1;
				if (state == BuilderPiece.State.OnConveyor || state == BuilderPiece.State.OnShelf || state == BuilderPiece.State.Displayed)
				{
					num15 = num8;
					num8 = -1;
				}
				if (this.ValidateDeserializedRootPieceState(num7, state, num15, num8, vector, quaternion))
				{
					BuilderPiece builderPiece = this.CreatePieceInternal(num6, num7, vector, quaternion, state, num9, num12, this);
					BuilderTable.tempPeiceIds.Add(num7);
					BuilderTable.tempParentActorNumbers.Add(num8);
					BuilderTable.tempInLeftHand.Add(flag3);
					builderPiece.SetFunctionalPieceState(b, NetPlayer.Get(PhotonNetwork.MasterClient), PhotonNetwork.ServerTimestamp);
					if (num15 >= 0 && this.isTableMutable)
					{
						builderPiece.shelfOwner = num15;
						if (state == BuilderPiece.State.OnConveyor)
						{
							BuilderConveyor builderConveyor = this.conveyors[num15];
							float num16 = 0f;
							if (PhotonNetwork.ServerTimestamp > num13)
							{
								num16 = (PhotonNetwork.ServerTimestamp - num13) / 1000f;
							}
							builderConveyor.OnShelfPieceCreated(builderPiece, num16);
						}
						else if (state == BuilderPiece.State.OnShelf || state == BuilderPiece.State.Displayed)
						{
							this.dispenserShelves[num15].OnShelfPieceCreated(builderPiece, false);
						}
					}
				}
			}
			for (int l = 0; l < BuilderTable.tempPeiceIds.Count; l++)
			{
				if (BuilderTable.tempParentActorNumbers[l] >= 0)
				{
					this.AttachPieceToActorInternal(BuilderTable.tempPeiceIds[l], BuilderTable.tempParentActorNumbers[l], BuilderTable.tempInLeftHand[l]);
				}
			}
			BuilderTable.tempPeiceIds.Clear();
			BuilderTable.tempParentActorNumbers.Clear();
			BuilderTable.tempInLeftHand.Clear();
			int num17 = binaryReader.ReadInt32();
			for (int m = 0; m < num17; m++)
			{
				int num18 = binaryReader.ReadInt32();
				int num19 = binaryReader.ReadInt32();
				int num20 = binaryReader.ReadInt32();
				int num21 = binaryReader.ReadInt32();
				int num22 = binaryReader.ReadInt32();
				BuilderPiece.State state2 = (BuilderPiece.State)binaryReader.ReadByte();
				int num23 = binaryReader.ReadInt32();
				bool flag4 = binaryReader.ReadByte() > 0;
				int num24 = binaryReader.ReadInt32();
				int num25 = binaryReader.ReadInt32();
				byte b2 = ((state2 == BuilderPiece.State.AttachedAndPlaced) ? binaryReader.ReadByte() : 0);
				int num26 = ((state2 == BuilderPiece.State.AttachedAndPlaced) ? binaryReader.ReadInt32() : 0);
				int num27 = ((state2 == BuilderPiece.State.OnConveyor) ? binaryReader.ReadInt32() : 0);
				if (!this.ValidateCreatePieceParams(num18, num19, state2, num24))
				{
					this.SetTableState(BuilderTable.TableState.BadData);
					return;
				}
				int num28 = -1;
				if (state2 == BuilderPiece.State.OnConveyor || state2 == BuilderPiece.State.OnShelf || state2 == BuilderPiece.State.Displayed)
				{
					num28 = num23;
					num23 = -1;
				}
				if (this.ValidateDeserializedChildPieceState(num19, state2))
				{
					BuilderPiece builderPiece2 = this.CreatePieceInternal(num18, num19, this.roomCenter.position, Quaternion.identity, state2, num24, num26, this);
					builderPiece2.SetFunctionalPieceState(b2, NetPlayer.Get(PhotonNetwork.MasterClient), PhotonNetwork.ServerTimestamp);
					BuilderTable.tempPeiceIds.Add(num19);
					BuilderTable.tempParentPeiceIds.Add(num20);
					BuilderTable.tempAttachIndexes.Add(num21);
					BuilderTable.tempParentAttachIndexes.Add(num22);
					BuilderTable.tempParentActorNumbers.Add(num23);
					BuilderTable.tempInLeftHand.Add(flag4);
					BuilderTable.tempPiecePlacement.Add(num25);
					if (num28 >= 0 && this.isTableMutable)
					{
						builderPiece2.shelfOwner = num28;
						if (state2 == BuilderPiece.State.OnConveyor)
						{
							BuilderConveyor builderConveyor2 = this.conveyors[num28];
							float num29 = 0f;
							if (PhotonNetwork.ServerTimestamp > num27)
							{
								num29 = (PhotonNetwork.ServerTimestamp - num27) / 1000f;
							}
							builderConveyor2.OnShelfPieceCreated(builderPiece2, num29);
						}
						else if (state2 == BuilderPiece.State.OnShelf || state2 == BuilderPiece.State.Displayed)
						{
							this.dispenserShelves[num28].OnShelfPieceCreated(builderPiece2, false);
						}
					}
				}
			}
			for (int n = 0; n < BuilderTable.tempPeiceIds.Count; n++)
			{
				if (!this.ValidateAttachPieceParams(BuilderTable.tempPeiceIds[n], BuilderTable.tempAttachIndexes[n], BuilderTable.tempParentPeiceIds[n], BuilderTable.tempParentAttachIndexes[n], BuilderTable.tempPiecePlacement[n]))
				{
					this.RecyclePieceInternal(BuilderTable.tempPeiceIds[n], true, false, -1);
				}
				else
				{
					this.AttachPieceInternal(BuilderTable.tempPeiceIds[n], BuilderTable.tempAttachIndexes[n], BuilderTable.tempParentPeiceIds[n], BuilderTable.tempParentAttachIndexes[n], BuilderTable.tempPiecePlacement[n]);
				}
			}
			for (int num30 = 0; num30 < BuilderTable.tempPeiceIds.Count; num30++)
			{
				if (BuilderTable.tempParentActorNumbers[num30] >= 0)
				{
					this.AttachPieceToActorInternal(BuilderTable.tempPeiceIds[num30], BuilderTable.tempParentActorNumbers[num30], BuilderTable.tempInLeftHand[num30]);
				}
			}
			foreach (BuilderPiece builderPiece3 in this.pieces)
			{
				if (builderPiece3.state == BuilderPiece.State.AttachedAndPlaced)
				{
					builderPiece3.OnPlacementDeserialized();
				}
			}
			if (this.isTableMutable)
			{
				this.plotOwners.Clear();
				this.doesLocalPlayerOwnPlot = false;
				int num31 = binaryReader.ReadInt32();
				for (int num32 = 0; num32 < num31; num32++)
				{
					int num33 = binaryReader.ReadInt32();
					int num34 = binaryReader.ReadInt32();
					BuilderPiecePrivatePlot builderPiecePrivatePlot;
					if (this.plotOwners.TryAdd(num33, num34) && this.GetPiece(num34).TryGetPlotComponent(out builderPiecePrivatePlot))
					{
						builderPiecePrivatePlot.ClaimPlotForPlayerNumber(num33);
						if (num33 == PhotonNetwork.LocalPlayer.ActorNumber)
						{
							this.doesLocalPlayerOwnPlot = true;
						}
					}
				}
				UnityEvent<bool> onLocalPlayerClaimedPlot = this.OnLocalPlayerClaimedPlot;
				if (onLocalPlayerClaimedPlot != null)
				{
					onLocalPlayerClaimedPlot.Invoke(this.doesLocalPlayerOwnPlot);
				}
				this.OnDeserializeUpdatePlots();
			}
			else
			{
				BuilderTable.mapIDBuffer = binaryReader.ReadChars(BuilderTable.mapIDBuffer.Length);
				string text = new string(BuilderTable.mapIDBuffer);
				if (SharedBlocksManager.IsMapIDValid(text))
				{
					this.sharedBlocksMap = new SharedBlocksManager.SharedBlocksMap
					{
						MapID = text
					};
				}
			}
			BuilderTable.tempDuplicateOverlaps.Clear();
			int num35 = binaryReader.ReadInt32();
			for (int num36 = 0; num36 < num35; num36++)
			{
				int num37 = binaryReader.ReadInt32();
				int num38 = binaryReader.ReadInt32();
				long num39 = binaryReader.ReadInt64();
				BuilderPiece piece = this.GetPiece(num37);
				if (!(piece == null))
				{
					BuilderPiece piece2 = this.GetPiece(num38);
					if (!(piece2 == null))
					{
						int num40;
						int num41;
						Vector2Int vector2Int;
						Vector2Int vector2Int2;
						this.UnpackSnapInfo(num39, out num40, out num41, out vector2Int, out vector2Int2);
						if (num40 >= 0 && num40 < piece.gridPlanes.Count && num41 >= 0 && num41 < piece2.gridPlanes.Count)
						{
							BuilderTable.SnapOverlapKey snapOverlapKey = BuilderTable.BuildOverlapKey(num37, num38, num40, num41);
							if (!BuilderTable.tempDuplicateOverlaps.Contains(snapOverlapKey))
							{
								BuilderTable.tempDuplicateOverlaps.Add(snapOverlapKey);
								piece.gridPlanes[num40].AddSnapOverlap(this.builderPool.CreateSnapOverlap(piece2.gridPlanes[num41], new SnapBounds(vector2Int, vector2Int2)));
							}
						}
					}
				}
			}
			BuilderTable.tempDuplicateOverlaps.Clear();
		}

		public const GTZone BUILDER_ZONE = GTZone.monkeBlocks;

		private const int INITIAL_BUILTIN_PIECE_ID = 5;

		private const int INITIAL_CREATED_PIECE_ID = 10000;

		public static float MAX_DROP_VELOCITY = 20f;

		public static float MAX_DROP_ANG_VELOCITY = 50f;

		private const float MAX_DISTANCE_FROM_CENTER = 217f;

		private const float MAX_LOCAL_MAGNITUDE = 80f;

		public const float MAX_DISTANCE_FROM_HAND = 2.5f;

		public static float DROP_ZONE_REPEL = 2.25f;

		public static int placedLayer;

		public static int heldLayer;

		public static int heldLayerLocal;

		public static int droppedLayer;

		private float acceptableSqrDistFromCenter = 47089f;

		public float pieceScale = 0.04f;

		public GTZone tableZone = GTZone.monkeBlocks;

		[SerializeField]
		private string SharedMapConfigTitleDataKey = "SharedBlocksStartingMapConfig";

		public BuilderTableNetworking builderNetworking;

		public BuilderRenderer builderRenderer;

		[HideInInspector]
		public BuilderPool builderPool;

		public Transform tableCenter;

		public Transform roomCenter;

		public Transform worldCenter;

		public GameObject noBlocksArea;

		public List<GameObject> builtInPieceRoots;

		[Tooltip("Optional terminal to control loaded blocks")]
		public SharedBlocksTerminal linkedTerminal;

		[Tooltip("Can Blocks Be Placed and Grabbed")]
		public bool isTableMutable;

		public GameObject shelvesRoot;

		public GameObject dropZoneRoot;

		public List<GameObject> recyclerRoot;

		public List<GameObject> allShelvesRoot;

		[NonSerialized]
		public List<BuilderConveyor> conveyors = new List<BuilderConveyor>();

		[NonSerialized]
		public List<BuilderDispenserShelf> dispenserShelves = new List<BuilderDispenserShelf>();

		public BuilderConveyorManager conveyorManager;

		public List<BuilderResourceMeter> resourceMeters;

		public GameObject sharedBuildArea;

		private BoxCollider[] sharedBuildAreas;

		public BuilderPiece armShelfPieceType;

		[NonSerialized]
		public List<BuilderRecycler> recyclers;

		[NonSerialized]
		public List<BuilderDropZone> dropZones;

		private int shelfSliceUpdateIndex;

		public static int SHELF_SLICE_BUCKETS = 6;

		public float defaultTint = 1f;

		public float droppedTint = 0.75f;

		public float grabbedTint = 0.75f;

		public float shelfTint = 1f;

		public float potentialGrabTint = 0.75f;

		public float paintingTint = 0.6f;

		private List<BuilderTable.BoxCheckParams> noBlocksAreas;

		private Collider[] noBlocksCheckResults = new Collider[64];

		public LayerMask allPiecesMask;

		public bool useSnapRotation;

		public BuilderPlacementStyle usePlacementStyle;

		public BuilderOptionButton buttonSnapRotation;

		public BuilderOptionButton buttonSnapPosition;

		public BuilderOptionButton buttonSaveLayout;

		public BuilderOptionButton buttonClearLayout;

		[HideInInspector]
		public List<BuilderAttachGridPlane> baseGridPlanes;

		private List<BuilderPiece> basePieces;

		[HideInInspector]
		public List<BuilderPiecePrivatePlot> allPrivatePlots;

		private int nextPieceId;

		[HideInInspector]
		public List<BuilderTable.BuildPieceSpawn> buildPieceSpawns;

		[HideInInspector]
		public List<BuilderShelf> shelves;

		[NonSerialized]
		public List<BuilderPiece> pieces = new List<BuilderPiece>(1024);

		private Dictionary<int, int> pieceIDToIndexCache = new Dictionary<int, int>(1024);

		[HideInInspector]
		public Dictionary<int, int> plotOwners;

		private bool doesLocalPlayerOwnPlot;

		public Dictionary<int, int> playerToArmShelfLeft;

		public Dictionary<int, int> playerToArmShelfRight;

		private HashSet<int> builderPiecesVisited = new HashSet<int>(128);

		public BuilderResources totalResources;

		[Tooltip("Resources reserved for conveyors and dispensers")]
		public BuilderResources totalReservedResources;

		public BuilderResources resourcesPerPrivatePlot;

		[NonSerialized]
		public int[] maxResources;

		private int[] plotMaxResources;

		[NonSerialized]
		public int[] usedResources;

		[NonSerialized]
		public int[] reservedResources;

		private List<int> playersInBuilder;

		private List<IBuilderPieceFunctional> activeFunctionalComponents = new List<IBuilderPieceFunctional>(128);

		private List<IBuilderPieceFunctional> funcComponentsToRegister = new List<IBuilderPieceFunctional>(10);

		private List<IBuilderPieceFunctional> funcComponentsToUnregister = new List<IBuilderPieceFunctional>(10);

		private List<IBuilderPieceFunctional> fixedUpdateFunctionalComponents = new List<IBuilderPieceFunctional>(128);

		private List<IBuilderPieceFunctional> funcComponentsToRegisterFixed = new List<IBuilderPieceFunctional>(10);

		private List<IBuilderPieceFunctional> funcComponentsToUnregisterFixed = new List<IBuilderPieceFunctional>(10);

		private const int MAX_SPHERE_CHECK_RESULTS = 1024;

		private NativeList<BuilderGridPlaneData> gridPlaneData;

		private NativeList<BuilderGridPlaneData> checkGridPlaneData;

		private NativeArray<ColliderHit> nearbyPiecesResults;

		private NativeArray<OverlapSphereCommand> nearbyPiecesCommands;

		private List<BuilderPotentialPlacement> allPotentialPlacements;

		private static HashSet<BuilderPiece> tempPieceSet = new HashSet<BuilderPiece>(512);

		private BuilderTable.TableState tableState;

		private bool inRoom;

		private bool inBuilderZone;

		private static int DROPPED_PIECE_LIMIT = 100;

		public static string nextUpdateOverride = string.Empty;

		private List<BuilderPiece> droppedPieces;

		private List<BuilderTable.DroppedPieceData> droppedPieceData;

		private HashSet<int>[] repelledPieceRoots;

		private int repelHistoryLength = 3;

		private int repelHistoryIndex;

		private bool hasRequestedConfig;

		private bool isDirty;

		private bool saveInProgress;

		private int currentSaveSlot = -1;

		[HideInInspector]
		public UnityEvent OnSaveTimeUpdated;

		[HideInInspector]
		public UnityEvent<bool> OnSaveDirtyChanged;

		[HideInInspector]
		public UnityEvent OnSaveSuccess;

		[HideInInspector]
		public UnityEvent<string> OnSaveFailure;

		[HideInInspector]
		public UnityEvent OnTableConfigurationUpdated;

		[HideInInspector]
		public UnityEvent<bool> OnLocalPlayerClaimedPlot;

		[HideInInspector]
		public UnityEvent OnMapCleared;

		[HideInInspector]
		public UnityEvent<string> OnMapLoaded;

		[HideInInspector]
		public UnityEvent<string> OnMapLoadFailed;

		private List<BuilderTable.BuilderCommand> queuedBuildCommands;

		private List<BuilderAction> rollBackActions;

		private List<BuilderTable.BuilderCommand> rollBackBufferedCommands;

		private List<BuilderTable.BuilderCommand> rollForwardCommands;

		[OnEnterPlay_Clear]
		private static Dictionary<GTZone, BuilderTable> zoneToInstance;

		private bool isSetup;

		public BuilderTable.SnapParams pushAndEaseParams;

		public BuilderTable.SnapParams overlapParams;

		private BuilderTable.SnapParams currSnapParams;

		public int maxPlacementChildDepth = 5;

		private static List<BuilderPiece> tempPieces = new List<BuilderPiece>(256);

		private static List<BuilderConveyor> tempConveyors = new List<BuilderConveyor>(256);

		private static List<BuilderDispenserShelf> tempDispensers = new List<BuilderDispenserShelf>(256);

		private static List<BuilderRecycler> tempRecyclers = new List<BuilderRecycler>(5);

		private static List<BuilderTable.BuilderCommand> tempRollForwardCommands = new List<BuilderTable.BuilderCommand>(128);

		private static List<BuilderPiece> tempDeletePieces = new List<BuilderPiece>(1024);

		public const int MAX_PIECE_DATA = 2560;

		public const int MAX_GRID_PLANE_DATA = 10240;

		public const int MAX_PRIVATE_PLOT_DATA = 64;

		public const int MAX_PLAYER_DATA = 64;

		private BuilderTableData tableData;

		private int fetchConfigurationAttempts;

		private int maxRetries = 3;

		private SharedBlocksManager.SharedBlocksMap sharedBlocksMap;

		private string pendingMapID;

		private SharedBlocksManager.StartingMapConfig startingMapConfig = new SharedBlocksManager.StartingMapConfig
		{
			pageNumber = 0,
			pageSize = 10,
			sortMethod = SharedBlocksManager.MapSortMethod.Top.ToString(),
			useMapID = false,
			mapID = null
		};

		private List<SharedBlocksManager.SharedBlocksMap> startingMapList = new List<SharedBlocksManager.SharedBlocksMap>();

		private SharedBlocksManager.SharedBlocksMap startingMap;

		private bool hasStartingMap;

		private double startingMapCacheTime = double.MinValue;

		private bool getStartingMapInProgress;

		private bool hasCachedTopMaps;

		private double lastGetTopMapsTime = double.MinValue;

		private static string personalBuildKey = "MyBuild";

		private static HashSet<BuilderTable.SnapOverlapKey> tempDuplicateOverlaps = new HashSet<BuilderTable.SnapOverlapKey>(16384);

		private static List<BuilderPiece> childPieces = new List<BuilderPiece>(4096);

		private static List<BuilderPiece> rootPieces = new List<BuilderPiece>(4096);

		private static List<int> overlapPieces = new List<int>(4096);

		private static List<int> overlapOtherPieces = new List<int>(4096);

		private static List<long> overlapPacked = new List<long>(4096);

		private static char[] mapIDBuffer = new char[8];

		private static Dictionary<long, int> snapOverlapSanity = new Dictionary<long, int>(16384);

		private static List<int> tempPeiceIds = new List<int>(4096);

		private static List<int> tempParentPeiceIds = new List<int>(4096);

		private static List<int> tempAttachIndexes = new List<int>(4096);

		private static List<int> tempParentAttachIndexes = new List<int>(4096);

		private static List<int> tempParentActorNumbers = new List<int>(4096);

		private static List<bool> tempInLeftHand = new List<bool>(4096);

		private static List<int> tempPiecePlacement = new List<int>(4096);

		private struct BoxCheckParams
		{
			public Vector3 center;

			public Vector3 halfExtents;

			public Quaternion rotation;
		}

		[Serializable]
		public class BuildPieceSpawn
		{
			public GameObject buildPiecePrefab;

			public int count = 1;
		}

		public enum BuilderCommandType
		{
			Create,
			Place,
			Grab,
			Drop,
			Remove,
			Paint,
			Recycle,
			ClaimPlot,
			FreePlot,
			CreateArmShelf,
			PlayerLeftRoom,
			FunctionalStateChange,
			SetSelection,
			Repel
		}

		public enum TableState
		{
			WaitingForZoneAndRoom,
			WaitingForInitalBuild,
			ReceivingInitialBuild,
			WaitForInitialBuildMaster,
			WaitForMasterResync,
			ReceivingMasterResync,
			InitialBuild,
			ExecuteQueuedCommands,
			Ready,
			BadData,
			WaitingForSharedMapLoad
		}

		public enum DroppedPieceState
		{
			None = -1,
			Light,
			Heavy,
			Frozen
		}

		private struct DroppedPieceData
		{
			public BuilderTable.DroppedPieceState droppedState;

			public float speedThreshCrossedTime;

			public float filteredSpeed;
		}

		public struct BuilderCommand
		{
			public BuilderTable.BuilderCommandType type;

			public int pieceType;

			public int pieceId;

			public int attachPieceId;

			public int parentPieceId;

			public int parentAttachIndex;

			public int attachIndex;

			public Vector3 localPosition;

			public Quaternion localRotation;

			public byte twist;

			public sbyte bumpOffsetX;

			public sbyte bumpOffsetZ;

			public Vector3 velocity;

			public Vector3 angVelocity;

			public bool isLeft;

			public int materialType;

			public NetPlayer player;

			public BuilderPiece.State state;

			public bool isQueued;

			public bool canRollback;

			public int localCommandId;

			public int serverTimeStamp;
		}

		[Serializable]
		public struct SnapParams
		{
			public float minOffsetY;

			public float maxOffsetY;

			public float maxUpDotProduct;

			public float maxTwistDotProduct;

			public float snapAttachDistance;

			public float snapDelayTime;

			public float snapDelayOffsetDist;

			public float unSnapDelayTime;

			public float unSnapDelayDist;

			public float maxBlockSnapDist;
		}

		private struct SnapOverlapKey
		{
			public override int GetHashCode()
			{
				return HashCode.Combine<int, int>(this.piece.GetHashCode(), this.otherPiece.GetHashCode());
			}

			public bool Equals(BuilderTable.SnapOverlapKey other)
			{
				return this.piece == other.piece && this.otherPiece == other.otherPiece;
			}

			public override bool Equals(object o)
			{
				return o is BuilderTable.SnapOverlapKey && this.Equals((BuilderTable.SnapOverlapKey)o);
			}

			public long piece;

			public long otherPiece;
		}
	}
}
