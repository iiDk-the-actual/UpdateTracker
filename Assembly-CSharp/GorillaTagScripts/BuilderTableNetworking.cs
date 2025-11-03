using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaGameModes;
using GorillaTagScripts.Builder;
using Ionic.Zlib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class BuilderTableNetworking : MonoBehaviourPunCallbacks, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		private void Awake()
		{
			this.masterClientTableInit = new List<BuilderTableNetworking.PlayerTableInitState>(10);
			this.masterClientTableValidators = new List<BuilderTableNetworking.PlayerTableInitState>(10);
			this.localClientTableInit = new BuilderTableNetworking.PlayerTableInitState();
			this.localValidationTable = new BuilderTableNetworking.PlayerTableInitState();
			this.callLimiters = new CallLimiter[26];
			this.callLimiters[0] = new CallLimiter(20, 30f, 0.5f);
			this.callLimiters[1] = new CallLimiter(200, 1f, 0.5f);
			this.callLimiters[2] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[3] = new CallLimiter(2, 1f, 0.5f);
			this.callLimiters[4] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[5] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[6] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[7] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[8] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[9] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[10] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[11] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[12] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[13] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[14] = new CallLimiter(100, 1f, 0.5f);
			this.callLimiters[15] = new CallLimiter(100, 1f, 0.5f);
			this.callLimiters[16] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[17] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[18] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[19] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[20] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[21] = new CallLimiter(50, 1f, 0.5f);
			this.callLimiters[22] = new CallLimiter(20, 1f, 0.5f);
			this.callLimiters[23] = new CallLimiter(20, 1f, 0.5f);
			this.callLimiters[24] = new CallLimiter(3, 30f, 0.5f);
			this.callLimiters[25] = new CallLimiter(10, 1f, 0.5f);
			this.armShelfRequests = new List<Player>(10);
		}

		private new void OnEnable()
		{
			base.OnEnable();
			TickSystem<object>.AddTickCallback(this);
		}

		private new void OnDisable()
		{
			base.OnDisable();
			TickSystem<object>.RemoveTickCallback(this);
		}

		public void SetTable(BuilderTable table)
		{
			this.currTable = table;
		}

		private BuilderTable GetTable()
		{
			return this.currTable;
		}

		private int CreateLocalCommandId()
		{
			int num = this.nextLocalCommandId;
			this.nextLocalCommandId++;
			return num;
		}

		public BuilderTableNetworking.PlayerTableInitState GetLocalTableInit()
		{
			return this.localClientTableInit;
		}

		public override void OnMasterClientSwitched(Player newMasterClient)
		{
			if (!newMasterClient.IsLocal)
			{
				this.localClientTableInit.Reset();
				BuilderTable table = this.GetTable();
				if (table.GetTableState() != BuilderTable.TableState.WaitingForZoneAndRoom)
				{
					if (table.GetTableState() == BuilderTable.TableState.Ready)
					{
						table.SetTableState(BuilderTable.TableState.WaitForMasterResync);
					}
					else if (table.GetTableState() == BuilderTable.TableState.WaitForMasterResync || table.GetTableState() == BuilderTable.TableState.ReceivingMasterResync)
					{
						table.SetTableState(BuilderTable.TableState.WaitForMasterResync);
					}
					else
					{
						table.SetTableState(BuilderTable.TableState.WaitingForInitalBuild);
					}
					this.PlayerEnterBuilder();
				}
				return;
			}
			this.masterClientTableInit.Clear();
			this.localClientTableInit.Reset();
			BuilderTable table2 = this.GetTable();
			BuilderTable.TableState tableState = table2.GetTableState();
			bool flag = (tableState != BuilderTable.TableState.Ready && tableState != BuilderTable.TableState.WaitingForZoneAndRoom && tableState != BuilderTable.TableState.WaitForMasterResync && tableState != BuilderTable.TableState.ReceivingMasterResync) || table2.pieces.Count <= 0;
			if (!flag)
			{
				flag |= table2.pieces.Count <= 0;
			}
			if (flag)
			{
				table2.ClearTable();
				table2.ClearQueuedCommands();
				table2.SetTableState(BuilderTable.TableState.WaitForInitialBuildMaster);
				return;
			}
			for (int i = 0; i < table2.pieces.Count; i++)
			{
				BuilderPiece builderPiece = table2.pieces[i];
				Player player = PhotonNetwork.CurrentRoom.GetPlayer(builderPiece.heldByPlayerActorNumber, false);
				if (table2.pieces[i].state == BuilderPiece.State.Grabbed && player == null)
				{
					Vector3 position = builderPiece.transform.position;
					Quaternion rotation = builderPiece.transform.rotation;
					Debug.LogErrorFormat("We have a piece {0} {1} held by an invalid player {2} dropping", new object[] { builderPiece.name, builderPiece.pieceId, builderPiece.heldByPlayerActorNumber });
					this.CreateLocalCommandId();
					builderPiece.ClearParentHeld();
					builderPiece.ClearParentPiece(false);
					builderPiece.transform.localScale = Vector3.one;
					builderPiece.SetState(BuilderPiece.State.Dropped, false);
					builderPiece.transform.SetLocalPositionAndRotation(position, rotation);
					if (builderPiece.rigidBody != null)
					{
						builderPiece.rigidBody.position = position;
						builderPiece.rigidBody.rotation = rotation;
						builderPiece.rigidBody.linearVelocity = Vector3.zero;
						builderPiece.rigidBody.angularVelocity = Vector3.zero;
					}
				}
			}
			table2.ClearQueuedCommands();
			table2.SetTableState(BuilderTable.TableState.Ready);
		}

		public override void OnPlayerLeftRoom(Player player)
		{
			Debug.LogFormat("Player {0} left room", new object[] { player.ActorNumber });
			BuilderTable table = this.GetTable();
			if (table.GetTableState() != BuilderTable.TableState.WaitingForZoneAndRoom)
			{
				if (table.isTableMutable)
				{
					if (!PhotonNetwork.IsMasterClient)
					{
						table.DropAllPiecesForPlayerLeaving(player.ActorNumber);
					}
					else
					{
						table.RecycleAllPiecesForPlayerLeaving(player.ActorNumber);
					}
				}
				table.PlayerLeftRoom(player.ActorNumber);
			}
			if (!table.isTableMutable && table.linkedTerminal != null && table.linkedTerminal.IsPlayerDriver(player))
			{
				table.linkedTerminal.ResetTerminalControl();
				if (NetworkSystem.Instance.IsMasterClient)
				{
					base.photonView.RPC("SetBlocksTerminalDriverRPC", RpcTarget.All, new object[] { -2 });
				}
			}
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			table.RemoveArmShelfForPlayer(player);
			table.VerifySetSelections();
			if (player != PhotonNetwork.LocalPlayer)
			{
				this.DestroyPlayerTableInit(player);
			}
		}

		public override void OnJoinedRoom()
		{
			base.OnJoinedRoom();
			BuilderTable table = this.GetTable();
			table.SetPendingMap(null);
			table.SetInRoom(true);
		}

		public override void OnLeftRoom()
		{
			this.PlayerExitBuilder();
			BuilderTable table = this.GetTable();
			table.SetPendingMap(null);
			table.SetInRoom(false);
			this.armShelfRequests.Clear();
		}

		public void Tick()
		{
			if (PhotonNetwork.IsMasterClient)
			{
				this.UpdateNewPlayerInit();
			}
		}

		public void PlayerEnterBuilder()
		{
			this.tablePhotonView.RPC("PlayerEnterBuilderRPC", PhotonNetwork.MasterClient, new object[]
			{
				PhotonNetwork.LocalPlayer,
				true
			});
			GorillaGuardianManager gorillaGuardianManager = GameMode.ActiveGameMode as GorillaGuardianManager;
			if (gorillaGuardianManager != null && gorillaGuardianManager.isPlaying && gorillaGuardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
			{
				gorillaGuardianManager.RequestEjectGuardian(NetworkSystem.Instance.LocalPlayer);
			}
		}

		[PunRPC]
		public void PlayerEnterBuilderRPC(Player player, bool entered, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PlayerEnterBuilderRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.PlayerEnterMaster, info))
			{
				return;
			}
			if (player == null || !player.Equals(info.Sender))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (entered)
			{
				BuilderTable.TableState tableState = table.GetTableState();
				if (tableState == BuilderTable.TableState.WaitingForInitalBuild || (this.IsPrivateMasterClient() && tableState == BuilderTable.TableState.WaitingForZoneAndRoom))
				{
					table.SetTableState(BuilderTable.TableState.WaitForInitialBuildMaster);
				}
				if (player != PhotonNetwork.LocalPlayer)
				{
					this.CreateSerializedTableForNewPlayerInit(player);
				}
				if (table.isTableMutable)
				{
					this.RequestCreateArmShelfForPlayer(player);
					return;
				}
				if (table.linkedTerminal != null)
				{
					base.photonView.RPC("SetBlocksTerminalDriverRPC", player, new object[] { table.linkedTerminal.GetDriverID });
					return;
				}
			}
			else
			{
				if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
				{
					this.DestroyPlayerTableInit(player);
				}
				if (table.isTableMutable)
				{
					table.RemoveArmShelfForPlayer(player);
				}
			}
		}

		public void PlayerExitBuilder()
		{
			if (NetworkSystem.Instance.InRoom)
			{
				this.tablePhotonView.RPC("PlayerEnterBuilderRPC", PhotonNetwork.MasterClient, new object[]
				{
					PhotonNetwork.LocalPlayer,
					false
				});
			}
			BuilderTable table = this.GetTable();
			table.ClearTable();
			table.ClearQueuedCommands();
			this.localClientTableInit.Reset();
			this.armShelfRequests.Clear();
			this.masterClientTableInit.Clear();
		}

		public bool IsPrivateMasterClient()
		{
			return PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient && NetworkSystem.Instance.SessionIsPrivate;
		}

		private void UpdateNewPlayerInit()
		{
			if (this.GetTable().GetTableState() == BuilderTable.TableState.Ready)
			{
				for (int i = 0; i < this.masterClientTableInit.Count; i++)
				{
					if (this.masterClientTableInit[i].waitForInitTimeRemaining >= 0f)
					{
						this.masterClientTableInit[i].waitForInitTimeRemaining -= Time.deltaTime;
						if (this.masterClientTableInit[i].waitForInitTimeRemaining <= 0f)
						{
							this.StartCreatingSerializedTable(this.masterClientTableInit[i].player);
							this.masterClientTableInit[i].waitForInitTimeRemaining = -1f;
							this.masterClientTableInit[i].sendNextChunkTimeRemaining = 0f;
						}
					}
					else if (this.masterClientTableInit[i].sendNextChunkTimeRemaining >= 0f)
					{
						this.masterClientTableInit[i].sendNextChunkTimeRemaining -= Time.deltaTime;
						if (this.masterClientTableInit[i].sendNextChunkTimeRemaining <= 0f)
						{
							this.SendNextTableData(this.masterClientTableInit[i].player);
							if (this.masterClientTableInit[i].numSerializedBytes < this.masterClientTableInit[i].totalSerializedBytes)
							{
								this.masterClientTableInit[i].sendNextChunkTimeRemaining = 0f;
							}
							else
							{
								this.masterClientTableInit[i].sendNextChunkTimeRemaining = -1f;
							}
						}
					}
				}
			}
		}

		private void StartCreatingSerializedTable(Player newPlayer)
		{
			BuilderTable table = this.GetTable();
			BuilderTableNetworking.PlayerTableInitState playerTableInit = this.GetPlayerTableInit(newPlayer);
			playerTableInit.totalSerializedBytes = table.SerializeTableState(playerTableInit.serializedTableState, 1048576);
			byte[] array = GZipStream.CompressBuffer(playerTableInit.serializedTableState);
			playerTableInit.totalSerializedBytes = array.Length;
			Array.Copy(array, 0, playerTableInit.serializedTableState, 0, playerTableInit.totalSerializedBytes);
			playerTableInit.numSerializedBytes = 0;
			this.tablePhotonView.RPC("StartBuildTableRPC", newPlayer, new object[] { playerTableInit.totalSerializedBytes });
		}

		[PunRPC]
		public void StartBuildTableRPC(int totalBytes, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "StartBuildTableRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.TableDataStart, info))
			{
				return;
			}
			if (totalBytes <= 0 || totalBytes > 1048576)
			{
				Debug.LogError("Builder Table Bytes is too large: " + totalBytes.ToString());
				return;
			}
			BuilderTable table = this.GetTable();
			GTDev.Log<string>("StartBuildTableRPC with current state " + table.GetTableState().ToString(), null);
			if (table.GetTableState() != BuilderTable.TableState.WaitForMasterResync && table.GetTableState() != BuilderTable.TableState.WaitingForInitalBuild)
			{
				return;
			}
			if (table.GetTableState() == BuilderTable.TableState.WaitForMasterResync)
			{
				table.SetTableState(BuilderTable.TableState.ReceivingMasterResync);
			}
			else
			{
				table.SetTableState(BuilderTable.TableState.ReceivingInitialBuild);
			}
			this.localClientTableInit.Reset();
			BuilderTableNetworking.PlayerTableInitState playerTableInitState = this.localClientTableInit;
			playerTableInitState.player = PhotonNetwork.LocalPlayer;
			playerTableInitState.totalSerializedBytes = totalBytes;
			table.ClearQueuedCommands();
		}

		private void SendNextTableData(Player requestingPlayer)
		{
			BuilderTableNetworking.PlayerTableInitState playerTableInit = this.GetPlayerTableInit(requestingPlayer);
			if (playerTableInit == null)
			{
				Debug.LogErrorFormat("No Table init found for player {0}", new object[] { requestingPlayer.ActorNumber });
				return;
			}
			int num = Mathf.Min(1000, playerTableInit.totalSerializedBytes - playerTableInit.numSerializedBytes);
			if (num <= 0)
			{
				return;
			}
			Array.Copy(playerTableInit.serializedTableState, playerTableInit.numSerializedBytes, playerTableInit.chunk, 0, num);
			playerTableInit.numSerializedBytes += num;
			this.tablePhotonView.RPC("SendTableDataRPC", requestingPlayer, new object[] { num, playerTableInit.chunk });
		}

		[PunRPC]
		public void SendTableDataRPC(int numBytes, byte[] bytes, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SendTableDataRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (numBytes <= 0 || numBytes > 1000 || numBytes > bytes.Length)
			{
				Debug.LogErrorFormat("Builder Table Send Data numBytes is too large {0}", new object[] { numBytes });
				return;
			}
			if (bytes.Length > 1000)
			{
				return;
			}
			if (PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.TableData, info))
			{
				return;
			}
			BuilderTableNetworking.PlayerTableInitState playerTableInitState = this.localClientTableInit;
			if (playerTableInitState.numSerializedBytes + numBytes > 1048576)
			{
				Debug.LogErrorFormat("Builder Table serialized bytes is larger than buffer {0}", new object[] { playerTableInitState.numSerializedBytes + numBytes });
				return;
			}
			Array.Copy(bytes, 0, playerTableInitState.serializedTableState, playerTableInitState.numSerializedBytes, numBytes);
			playerTableInitState.numSerializedBytes += numBytes;
			if (playerTableInitState.numSerializedBytes >= playerTableInitState.totalSerializedBytes)
			{
				this.GetTable().SetTableState(BuilderTable.TableState.InitialBuild);
			}
		}

		private bool DoesTableInitExist(Player player)
		{
			for (int i = 0; i < this.masterClientTableInit.Count; i++)
			{
				if (this.masterClientTableInit[i].player.ActorNumber == player.ActorNumber)
				{
					return true;
				}
			}
			return false;
		}

		private BuilderTableNetworking.PlayerTableInitState CreatePlayerTableInit(Player player)
		{
			for (int i = 0; i < this.masterClientTableInit.Count; i++)
			{
				if (this.masterClientTableInit[i].player.ActorNumber == player.ActorNumber)
				{
					this.masterClientTableInit[i].Reset();
					return this.masterClientTableInit[i];
				}
			}
			BuilderTableNetworking.PlayerTableInitState playerTableInitState = new BuilderTableNetworking.PlayerTableInitState();
			playerTableInitState.player = player;
			this.masterClientTableInit.Add(playerTableInitState);
			return playerTableInitState;
		}

		public void ResetSerializedTableForAllPlayers()
		{
			for (int i = 0; i < this.masterClientTableInit.Count; i++)
			{
				this.masterClientTableInit[i].waitForInitTimeRemaining = 1f;
				this.masterClientTableInit[i].sendNextChunkTimeRemaining = -1f;
				this.masterClientTableInit[i].numSerializedBytes = 0;
				this.masterClientTableInit[i].totalSerializedBytes = 0;
			}
		}

		private void CreateSerializedTableForNewPlayerInit(Player newPlayer)
		{
			if (this.DoesTableInitExist(newPlayer))
			{
				return;
			}
			BuilderTableNetworking.PlayerTableInitState playerTableInitState = this.CreatePlayerTableInit(newPlayer);
			playerTableInitState.waitForInitTimeRemaining = 1f;
			playerTableInitState.sendNextChunkTimeRemaining = -1f;
		}

		private void DestroyPlayerTableInit(Player player)
		{
			for (int i = 0; i < this.masterClientTableInit.Count; i++)
			{
				if (this.masterClientTableInit[i].player.ActorNumber == player.ActorNumber)
				{
					this.masterClientTableInit.RemoveAt(i);
					i--;
				}
			}
		}

		private BuilderTableNetworking.PlayerTableInitState GetPlayerTableInit(Player player)
		{
			for (int i = 0; i < this.masterClientTableInit.Count; i++)
			{
				if (this.masterClientTableInit[i].player.ActorNumber == player.ActorNumber)
				{
					return this.masterClientTableInit[i];
				}
			}
			return null;
		}

		private bool ValidateMasterClientIsReady(Player player)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return false;
			}
			if (player != null && !player.IsMasterClient)
			{
				BuilderTableNetworking.PlayerTableInitState playerTableInit = this.GetPlayerTableInit(player);
				if (playerTableInit != null && playerTableInit.numSerializedBytes < playerTableInit.totalSerializedBytes)
				{
					return false;
				}
			}
			return this.GetTable().GetTableState() == BuilderTable.TableState.Ready;
		}

		private bool ValidateCallLimits(BuilderTableNetworking.RPC rpcCall, PhotonMessageInfo info)
		{
			return rpcCall >= BuilderTableNetworking.RPC.PlayerEnterMaster && rpcCall < BuilderTableNetworking.RPC.Count && this.callLimiters[(int)rpcCall].CheckCallTime(Time.time);
		}

		[PunRPC]
		public void RequestFailedRPC(int localCommandId, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestFailedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.RequestFailed, info))
			{
				return;
			}
			this.GetTable().RollbackFailedCommand(localCommandId);
		}

		public void RequestCreatePiece(int newPieceType, Vector3 position, Quaternion rotation, int materialType)
		{
		}

		public void RequestCreatePieceRPC(int newPieceType, long packedPosition, int packedRotation, int materialType, PhotonMessageInfo info)
		{
		}

		public void PieceCreatedRPC(int pieceType, int pieceId, long packedPosition, int packedRotation, int materialType, Player creatingPlayer, PhotonMessageInfo info)
		{
		}

		public void CreateShelfPiece(int pieceType, Vector3 position, Quaternion rotation, int materialType, BuilderPiece.State state, int shelfID)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (table.GetTableState() != BuilderTable.TableState.Ready)
			{
				return;
			}
			BuilderPiece piecePrefab = table.GetPiecePrefab(pieceType);
			if (!table.HasEnoughResources(piecePrefab))
			{
				Debug.Log("Not Enough Resources");
				return;
			}
			if (state != BuilderPiece.State.OnShelf)
			{
				if (state != BuilderPiece.State.OnConveyor)
				{
					return;
				}
				if (shelfID < 0 || shelfID >= table.conveyors.Count)
				{
					return;
				}
			}
			else if (shelfID < 0 || shelfID >= table.dispenserShelves.Count)
			{
				return;
			}
			int num = table.CreatePieceId();
			long num2 = BitPackUtils.PackWorldPosForNetwork(position);
			int num3 = BitPackUtils.PackQuaternionForNetwork(rotation);
			base.photonView.RPC("PieceCreatedByShelfRPC", RpcTarget.All, new object[]
			{
				pieceType,
				num,
				num2,
				num3,
				materialType,
				(byte)state,
				shelfID,
				PhotonNetwork.LocalPlayer
			});
		}

		[PunRPC]
		public void PieceCreatedByShelfRPC(int pieceType, int pieceId, long packedPosition, int packedRotation, int materialType, byte state, int shelfID, Player creatingPlayer, PhotonMessageInfo info)
		{
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.CreateShelfPieceMaster, info))
			{
				return;
			}
			if (!table.isTableMutable)
			{
				return;
			}
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(packedPosition);
			Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(packedRotation);
			if (!table.ValidatePieceWorldTransform(vector, quaternion))
			{
				return;
			}
			if (state == 4)
			{
				table.CreateDispenserShelfPiece(pieceType, pieceId, vector, quaternion, materialType, shelfID);
				return;
			}
			if (state != 7)
			{
				return;
			}
			table.CreateConveyorPiece(pieceType, pieceId, vector, quaternion, materialType, shelfID, info.SentServerTimestamp);
		}

		public void RequestRecyclePiece(int pieceId, Vector3 position, Quaternion rotation, bool playFX, int recyclerID)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.GetTableState() != BuilderTable.TableState.Ready)
			{
				return;
			}
			if (!table.isTableMutable)
			{
				return;
			}
			float num = 10000f;
			if (!(in position).IsValid(in num) || !(in rotation).IsValid())
			{
				return;
			}
			if (recyclerID > 32767 || recyclerID < -1)
			{
				return;
			}
			long num2 = BitPackUtils.PackWorldPosForNetwork(position);
			int num3 = BitPackUtils.PackQuaternionForNetwork(rotation);
			base.photonView.RPC("PieceDestroyedRPC", RpcTarget.All, new object[]
			{
				pieceId,
				num2,
				num3,
				playFX,
				(short)recyclerID
			});
		}

		[PunRPC]
		public void PieceDestroyedRPC(int pieceId, long packedPosition, int packedRotation, bool playFX, short recyclerID, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PieceDestroyedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.RecyclePieceMaster, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(packedPosition);
			Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(packedRotation);
			float num = 10000f;
			if (!(in vector).IsValid(in num) || !(in quaternion).IsValid())
			{
				return;
			}
			table.RecyclePiece(pieceId, vector, quaternion, playFX, (int)recyclerID, info.Sender);
		}

		public void RequestPlacePiece(BuilderPiece piece, BuilderPiece attachPiece, sbyte bumpOffsetX, sbyte bumpOffsetZ, byte twist, BuilderPiece parentPiece, int attachIndex, int parentAttachIndex)
		{
			if (piece == null)
			{
				return;
			}
			int pieceId = piece.pieceId;
			int num = ((parentPiece != null) ? parentPiece.pieceId : (-1));
			int num2 = ((attachPiece != null) ? attachPiece.pieceId : (-1));
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (!table.ValidatePlacePieceParams(pieceId, num2, bumpOffsetX, bumpOffsetZ, twist, num, attachIndex, parentAttachIndex, NetPlayer.Get(PhotonNetwork.LocalPlayer)))
			{
				return;
			}
			int num3 = this.CreateLocalCommandId();
			attachPiece.requestedParentPiece = parentPiece;
			table.UpdatePieceData(attachPiece);
			table.PlacePiece(num3, pieceId, num2, bumpOffsetX, bumpOffsetZ, twist, num, attachIndex, parentAttachIndex, NetPlayer.Get(PhotonNetwork.LocalPlayer), PhotonNetwork.ServerTimestamp, true);
			int num4 = BuilderTable.PackPiecePlacement(twist, bumpOffsetX, bumpOffsetZ);
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				base.photonView.RPC("RequestPlacePieceRPC", RpcTarget.MasterClient, new object[]
				{
					num3,
					pieceId,
					num2,
					num4,
					num,
					attachIndex,
					parentAttachIndex,
					PhotonNetwork.LocalPlayer
				});
			}
		}

		[PunRPC]
		public void RequestPlacePieceRPC(int localCommandId, int pieceId, int attachPieceId, int placement, int parentPieceId, int attachIndex, int parentAttachIndex, Player placedByPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestPlacePieceRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateMasterClientIsReady(info.Sender))
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.PlacePieceMaster, info) || placedByPlayer == null || !placedByPlayer.Equals(info.Sender))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			bool isMasterClient = info.Sender.IsMasterClient;
			byte b;
			sbyte b2;
			sbyte b3;
			BuilderTable.UnpackPiecePlacement(placement, out b, out b2, out b3);
			bool flag = isMasterClient || table.ValidatePlacePieceParams(pieceId, attachPieceId, b2, b3, b, parentPieceId, attachIndex, parentAttachIndex, NetPlayer.Get(placedByPlayer));
			if (flag)
			{
				flag &= isMasterClient || table.ValidatePlacePieceState(pieceId, attachPieceId, b2, b3, b, parentPieceId, attachIndex, parentAttachIndex, placedByPlayer);
			}
			if (flag)
			{
				BuilderPiece piece = table.GetPiece(parentPieceId);
				BuilderPiecePrivatePlot builderPiecePrivatePlot;
				if (piece != null && piece.TryGetPlotComponent(out builderPiecePrivatePlot) && !builderPiecePrivatePlot.IsPlotClaimed())
				{
					base.photonView.RPC("PlotClaimedRPC", RpcTarget.All, new object[] { parentPieceId, placedByPlayer, true });
				}
				base.photonView.RPC("PiecePlacedRPC", RpcTarget.All, new object[] { localCommandId, pieceId, attachPieceId, placement, parentPieceId, attachIndex, parentAttachIndex, placedByPlayer, info.SentServerTimestamp });
				return;
			}
			base.photonView.RPC("RequestFailedRPC", info.Sender, new object[] { localCommandId });
		}

		[PunRPC]
		public void PiecePlacedRPC(int localCommandId, int pieceId, int attachPieceId, int placement, int parentPieceId, int attachIndex, int parentAttachIndex, Player placedByPlayer, int timeStamp, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PiecePlacedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.PlacePiece, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (placedByPlayer == null)
			{
				return;
			}
			if ((ulong)(PhotonNetwork.ServerTimestamp - info.SentServerTimestamp) > (ulong)((long)PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout) || (ulong)(info.SentServerTimestamp - timeStamp) > (ulong)((long)PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout))
			{
				timeStamp = PhotonNetwork.ServerTimestamp;
			}
			byte b;
			sbyte b2;
			sbyte b3;
			BuilderTable.UnpackPiecePlacement(placement, out b, out b2, out b3);
			table.PlacePiece(localCommandId, pieceId, attachPieceId, b2, b3, b, parentPieceId, attachIndex, parentAttachIndex, NetPlayer.Get(placedByPlayer), timeStamp, false);
		}

		public void RequestGrabPiece(BuilderPiece piece, bool isLefHand, Vector3 localPosition, Quaternion localRotation)
		{
			if (piece == null)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (!table.ValidateGrabPieceParams(piece.pieceId, isLefHand, localPosition, localRotation, NetPlayer.Get(PhotonNetwork.LocalPlayer)))
			{
				return;
			}
			if (PhotonNetwork.IsMasterClient)
			{
				this.CheckForFreedPlot(piece.pieceId, PhotonNetwork.LocalPlayer);
			}
			int num = this.CreateLocalCommandId();
			table.GrabPiece(num, piece.pieceId, isLefHand, localPosition, localRotation, NetPlayer.Get(PhotonNetwork.LocalPlayer), true);
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				long num2 = BitPackUtils.PackHandPosRotForNetwork(localPosition, localRotation);
				base.photonView.RPC("RequestGrabPieceRPC", RpcTarget.MasterClient, new object[]
				{
					num,
					piece.pieceId,
					isLefHand,
					num2,
					PhotonNetwork.LocalPlayer
				});
			}
		}

		[PunRPC]
		public void RequestGrabPieceRPC(int localCommandId, int pieceId, bool isLeftHand, long packedPosRot, Player grabbedByPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestGrabPieceRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateMasterClientIsReady(info.Sender))
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.GrabPieceMaster, info) || !grabbedByPlayer.Equals(info.Sender))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			Vector3 vector;
			Quaternion quaternion;
			BitPackUtils.UnpackHandPosRotFromNetwork(packedPosRot, out vector, out quaternion);
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				bool isMasterClient = info.Sender.IsMasterClient;
				bool flag = isMasterClient || table.ValidateGrabPieceParams(pieceId, isLeftHand, vector, quaternion, NetPlayer.Get(grabbedByPlayer));
				if (flag)
				{
					flag &= isMasterClient || table.ValidateGrabPieceState(pieceId, isLeftHand, vector, quaternion, grabbedByPlayer);
				}
				if (flag)
				{
					if (!info.Sender.IsMasterClient)
					{
						this.CheckForFreedPlot(pieceId, grabbedByPlayer);
					}
					base.photonView.RPC("PieceGrabbedRPC", RpcTarget.All, new object[] { localCommandId, pieceId, isLeftHand, packedPosRot, grabbedByPlayer });
					return;
				}
				base.photonView.RPC("RequestFailedRPC", info.Sender, new object[] { localCommandId });
			}
		}

		private void CheckForFreedPlot(int pieceId, Player grabbedByPlayer)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderPiece piece = this.GetTable().GetPiece(pieceId);
			if (piece != null && piece.parentPiece != null && piece.parentPiece.IsPrivatePlot() && piece.parentPiece.firstChildPiece.Equals(piece) && piece.nextSiblingPiece == null)
			{
				base.photonView.RPC("PlotClaimedRPC", RpcTarget.All, new object[]
				{
					piece.parentPiece.pieceId,
					grabbedByPlayer,
					false
				});
			}
		}

		[PunRPC]
		public void PieceGrabbedRPC(int localCommandId, int pieceId, bool isLeftHand, long packedPosRot, Player grabbedByPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PieceGrabbedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.GrabPiece, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			Vector3 vector;
			Quaternion quaternion;
			BitPackUtils.UnpackHandPosRotFromNetwork(packedPosRot, out vector, out quaternion);
			table.GrabPiece(localCommandId, pieceId, isLeftHand, vector, quaternion, NetPlayer.Get(grabbedByPlayer), false);
		}

		public void RequestDropPiece(BuilderPiece piece, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity)
		{
			if (piece == null)
			{
				return;
			}
			int pieceId = piece.pieceId;
			float num = 10000f;
			if ((in velocity).IsValid(in num) && velocity.sqrMagnitude > BuilderTable.MAX_DROP_VELOCITY * BuilderTable.MAX_DROP_VELOCITY)
			{
				velocity = velocity.normalized * BuilderTable.MAX_DROP_VELOCITY;
			}
			num = 10000f;
			if ((in angVelocity).IsValid(in num) && angVelocity.sqrMagnitude > BuilderTable.MAX_DROP_ANG_VELOCITY * BuilderTable.MAX_DROP_ANG_VELOCITY)
			{
				angVelocity = angVelocity.normalized * BuilderTable.MAX_DROP_ANG_VELOCITY;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (!table.ValidateDropPieceParams(pieceId, position, rotation, velocity, angVelocity, NetPlayer.Get(PhotonNetwork.LocalPlayer)))
			{
				return;
			}
			int num2 = this.CreateLocalCommandId();
			table.DropPiece(num2, pieceId, position, rotation, velocity, angVelocity, NetPlayer.Get(PhotonNetwork.LocalPlayer), true);
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				base.photonView.RPC("RequestDropPieceRPC", RpcTarget.MasterClient, new object[]
				{
					num2,
					pieceId,
					position,
					rotation,
					velocity,
					angVelocity,
					PhotonNetwork.LocalPlayer
				});
			}
		}

		[PunRPC]
		public void RequestDropPieceRPC(int localCommandId, int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, Player droppedByPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestDropPieceRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateMasterClientIsReady(info.Sender))
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.DropPieceMaster, info) || !droppedByPlayer.Equals(info.Sender))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (table.GetTableState() != BuilderTable.TableState.Ready)
			{
				return;
			}
			bool isMasterClient = info.Sender.IsMasterClient;
			bool flag = isMasterClient || table.ValidateDropPieceParams(pieceId, position, rotation, velocity, angVelocity, NetPlayer.Get(droppedByPlayer));
			if (flag)
			{
				flag &= isMasterClient || table.ValidateDropPieceState(pieceId, position, rotation, velocity, angVelocity, droppedByPlayer);
			}
			if (flag)
			{
				base.photonView.RPC("PieceDroppedRPC", RpcTarget.All, new object[] { localCommandId, pieceId, position, rotation, velocity, angVelocity, droppedByPlayer });
				return;
			}
			base.photonView.RPC("RequestFailedRPC", info.Sender, new object[] { localCommandId });
		}

		[PunRPC]
		public void PieceDroppedRPC(int localCommandId, int pieceId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, Player droppedByPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PieceDroppedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.DropPiece, info))
			{
				return;
			}
			float num = 10000f;
			if ((in position).IsValid(in num) && (in rotation).IsValid())
			{
				float num2 = 10000f;
				if ((in velocity).IsValid(in num2))
				{
					float num3 = 10000f;
					if ((in angVelocity).IsValid(in num3))
					{
						BuilderTable table = this.GetTable();
						if (!table.isTableMutable)
						{
							return;
						}
						table.DropPiece(localCommandId, pieceId, position, rotation, velocity, angVelocity, NetPlayer.Get(droppedByPlayer), false);
						return;
					}
				}
			}
		}

		public void PieceEnteredDropZone(BuilderPiece piece, BuilderDropZone.DropType dropType, int dropZoneId)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			BuilderPiece rootPiece = piece.GetRootPiece();
			if (!table.ValidateRepelPiece(rootPiece))
			{
				return;
			}
			long num = BitPackUtils.PackWorldPosForNetwork(rootPiece.transform.position);
			int num2 = BitPackUtils.PackQuaternionForNetwork(rootPiece.transform.rotation);
			base.photonView.RPC("PieceEnteredDropZoneRPC", RpcTarget.All, new object[] { rootPiece.pieceId, num, num2, dropZoneId });
		}

		[PunRPC]
		public void PieceEnteredDropZoneRPC(int pieceId, long position, int rotation, int dropZoneId, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PieceEnteredDropZoneRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.PieceDropZone, info))
			{
				return;
			}
			Vector3 vector = BitPackUtils.UnpackWorldPosFromNetwork(position);
			float num = 10000f;
			if (!(in vector).IsValid(in num))
			{
				return;
			}
			Quaternion quaternion = BitPackUtils.UnpackQuaternionFromNetwork(rotation);
			if (!(in quaternion).IsValid())
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			table.PieceEnteredDropZone(pieceId, vector, quaternion, dropZoneId);
		}

		[PunRPC]
		public void PlotClaimedRPC(int pieceId, Player claimingPlayer, bool claimed, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PlotClaimedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.PlotClaimedMaster, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (claimed)
			{
				table.PlotClaimed(pieceId, claimingPlayer);
				return;
			}
			table.PlotFreed(pieceId, claimingPlayer);
		}

		public void RequestCreateArmShelfForPlayer(Player player)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (table.GetTableState() != BuilderTable.TableState.Ready)
			{
				if (!this.armShelfRequests.Contains(player))
				{
					this.armShelfRequests.Add(player);
				}
				return;
			}
			if (table.playerToArmShelfLeft.ContainsKey(player.ActorNumber))
			{
				return;
			}
			int num = table.CreatePieceId();
			int num2 = table.CreatePieceId();
			int staticHash = table.armShelfPieceType.name.GetStaticHash();
			base.photonView.RPC("ArmShelfCreatedRPC", RpcTarget.All, new object[] { num, num2, staticHash, player });
		}

		[PunRPC]
		public void ArmShelfCreatedRPC(int pieceIdLeft, int pieceIdRight, int pieceType, Player owningPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "ArmShelfCreatedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.ArmShelfCreated, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (pieceType != table.armShelfPieceType.name.GetStaticHash())
			{
				return;
			}
			table.CreateArmShelf(pieceIdLeft, pieceIdRight, pieceType, owningPlayer);
		}

		public void RequestShelfSelection(int shelfID, int groupID, bool isConveyor)
		{
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (isConveyor)
			{
				if (shelfID < 0 || shelfID >= table.conveyors.Count)
				{
					return;
				}
			}
			else if (shelfID < 0 || shelfID >= table.dispenserShelves.Count)
			{
				return;
			}
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				base.photonView.RPC("RequestShelfSelectionRPC", RpcTarget.MasterClient, new object[] { shelfID, groupID, isConveyor });
			}
		}

		[PunRPC]
		public void RequestShelfSelectionRPC(int shelfId, int setId, bool isConveyor, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestShelfSelectionRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.ShelfSelection, info))
			{
				return;
			}
			if (!this.ValidateMasterClientIsReady(info.Sender))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (!table.ValidateShelfSelectionParams(shelfId, setId, isConveyor, info.Sender))
			{
				return;
			}
			base.photonView.RPC("ShelfSelectionChangedRPC", RpcTarget.All, new object[] { shelfId, setId, isConveyor, info.Sender });
		}

		[PunRPC]
		public void ShelfSelectionChangedRPC(int shelfId, int setId, bool isConveyor, Player caller, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "ShelfSelectionChangedRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.ShelfSelectionMaster, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (!table.isTableMutable)
			{
				return;
			}
			if (shelfId < 0 || ((!isConveyor || shelfId >= table.conveyors.Count) && (isConveyor || shelfId >= table.dispenserShelves.Count)))
			{
				return;
			}
			table.ChangeSetSelection(shelfId, setId, isConveyor);
		}

		public void RequestFunctionalPieceStateChange(int pieceID, byte state)
		{
			BuilderTable table = this.GetTable();
			if (!table.ValidateFunctionalPieceState(pieceID, state, NetworkSystem.Instance.LocalPlayer))
			{
				return;
			}
			if (table.GetTableState() == BuilderTable.TableState.Ready)
			{
				base.photonView.RPC("RequestFunctionalPieceStateChangeRPC", RpcTarget.MasterClient, new object[] { pieceID, state });
			}
		}

		[PunRPC]
		public void RequestFunctionalPieceStateChangeRPC(int pieceID, byte state, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestFunctionalPieceStateChangeRPC");
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateMasterClientIsReady(info.Sender))
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.SetFunctionalState, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.GetTableState() != BuilderTable.TableState.Ready)
			{
				return;
			}
			if (table.ValidateFunctionalPieceState(pieceID, state, NetPlayer.Get(info.Sender)))
			{
				table.OnFunctionalStateRequest(pieceID, state, NetPlayer.Get(info.Sender), info.SentServerTimestamp);
			}
		}

		public void FunctionalPieceStateChangeMaster(int pieceID, byte state, Player instigator, int timeStamp)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.ValidateFunctionalPieceState(pieceID, state, NetPlayer.Get(instigator)) && state != table.GetPiece(pieceID).functionalPieceState)
			{
				base.photonView.RPC("FunctionalPieceStateChangeRPC", RpcTarget.All, new object[] { pieceID, state, instigator, timeStamp });
			}
		}

		[PunRPC]
		public void FunctionalPieceStateChangeRPC(int pieceID, byte state, Player caller, int timeStamp, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "FunctionalPieceStateChangeRPC");
			if (!info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.SetFunctionalStateMaster, info))
			{
				return;
			}
			if (caller == null)
			{
				return;
			}
			if ((ulong)(PhotonNetwork.ServerTimestamp - info.SentServerTimestamp) > (ulong)((long)PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout) || (ulong)(info.SentServerTimestamp - timeStamp) > (ulong)((long)PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout))
			{
				timeStamp = PhotonNetwork.ServerTimestamp;
			}
			BuilderTable table = this.GetTable();
			if (table.ValidateFunctionalPieceState(pieceID, state, NetPlayer.Get(info.Sender)))
			{
				table.SetFunctionalPieceState(pieceID, state, NetPlayer.Get(caller), timeStamp);
			}
		}

		public void RequestBlocksTerminalControl(bool locked)
		{
			BuilderTable table = this.GetTable();
			if (table.isTableMutable || table.linkedTerminal == null)
			{
				return;
			}
			if (table.linkedTerminal.IsTerminalLocked == locked)
			{
				return;
			}
			base.photonView.RPC("RequestBlocksTerminalControlRPC", RpcTarget.MasterClient, new object[] { locked });
		}

		[PunRPC]
		private void RequestBlocksTerminalControlRPC(bool lockedStatus, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestBlocksTerminalControlRPC");
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.RequestTerminalControl, info))
			{
				return;
			}
			if (info.Sender == null)
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.isTableMutable || table.linkedTerminal == null)
			{
				return;
			}
			RigContainer rigContainer;
			if (!(VRRigCache.Instance != null) || !VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
			{
				return;
			}
			if ((table.linkedTerminal.transform.position - rigContainer.Rig.bodyTransform.position).sqrMagnitude > 9f)
			{
				return;
			}
			if (table.linkedTerminal.ValidateTerminalControlRequest(lockedStatus, info.Sender.ActorNumber))
			{
				int num = (lockedStatus ? info.Sender.ActorNumber : (-2));
				base.photonView.RPC("SetBlocksTerminalDriverRPC", RpcTarget.All, new object[] { num });
			}
		}

		[PunRPC]
		private void SetBlocksTerminalDriverRPC(int driver, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SetBlocksTerminalDriverRPC");
			if (info.Sender == null || !info.Sender.IsMasterClient)
			{
				return;
			}
			if (driver != -2 && NetworkSystem.Instance.GetPlayer(driver) == null)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.SetTerminalDriver, info))
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.isTableMutable || table.linkedTerminal == null)
			{
				return;
			}
			table.linkedTerminal.SetTerminalDriver(driver);
		}

		public void RequestLoadSharedBlocksMap(string mapID)
		{
			base.photonView.RPC("LoadSharedBlocksMapRPC", RpcTarget.MasterClient, new object[] { mapID });
		}

		[PunRPC]
		private void LoadSharedBlocksMapRPC(string mapID, PhotonMessageInfo info)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "LoadSharedBlocksMapRPC");
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.LoadSharedBlocksMap, info))
			{
				return;
			}
			if (info.Sender == null || mapID.IsNullOrEmpty())
			{
				return;
			}
			BuilderTable table = this.GetTable();
			if (table.isTableMutable || table.linkedTerminal == null)
			{
				return;
			}
			if (!table.linkedTerminal.ValidateLoadMapRequest(mapID, info.Sender.ActorNumber))
			{
				GTDev.LogWarning<string>("SharedBlocks ValidateLoadMapRequest fail", null);
				return;
			}
			BuilderTable.TableState tableState = table.GetTableState();
			if (tableState == BuilderTable.TableState.Ready || tableState == BuilderTable.TableState.BadData)
			{
				table.SetPendingMap(mapID);
				base.photonView.RPC("SharedTableEventRPC", RpcTarget.Others, new object[] { 0, mapID });
				this.localClientTableInit.Reset();
				UnityEvent onMapCleared = table.OnMapCleared;
				if (onMapCleared != null)
				{
					onMapCleared.Invoke();
				}
				table.SetTableState(BuilderTable.TableState.WaitingForSharedMapLoad);
				table.FindAndLoadSharedBlocksMap(mapID);
				return;
			}
			GTDev.LogWarning<string>("SharedBlocks Invalid state " + tableState.ToString(), null);
			this.LoadSharedBlocksFailedMaster(mapID);
		}

		public void LoadSharedBlocksFailedMaster(string mapID)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (mapID.Length > 8)
			{
				return;
			}
			base.photonView.RPC("SharedTableEventRPC", RpcTarget.All, new object[] { 1, mapID });
		}

		public void SharedBlocksOutOfBoundsMaster(string mapID)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (mapID.Length > 8)
			{
				return;
			}
			base.photonView.RPC("SharedTableEventRPC", RpcTarget.All, new object[] { 2, mapID });
		}

		[PunRPC]
		private void SharedTableEventRPC(byte eventType, string mapID, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SharedTableEventRPC");
			if (eventType >= 3)
			{
				return;
			}
			if (!SharedBlocksManager.IsMapIDValid(mapID) && eventType != 1)
			{
				GTDev.LogWarning<string>("BuilderTableNetworking SharedTableEventRPC Invalid Map ID", null);
				return;
			}
			if (info.Sender == null || !info.Sender.IsMasterClient)
			{
				return;
			}
			if (!this.ValidateCallLimits(BuilderTableNetworking.RPC.SharedTableEvent, info))
			{
				GTDev.LogError<string>("SharedTableEventRPC Failed call limits", null);
				return;
			}
			if (this.GetTable().isTableMutable)
			{
				return;
			}
			switch (eventType)
			{
			case 0:
				this.OnSharedBlocksLoadStarted(mapID);
				return;
			case 1:
				this.OnLoadSharedBlocksFailed(mapID);
				return;
			case 2:
				this.OnSharedBlocksOutOfBounds(mapID);
				return;
			default:
				return;
			}
		}

		private void OnSharedBlocksLoadStarted(string mapID)
		{
			this.localClientTableInit.Reset();
			BuilderTable table = this.GetTable();
			if (table.GetTableState() != BuilderTable.TableState.WaitingForZoneAndRoom)
			{
				table.ClearTable();
				table.ClearQueuedCommands();
				table.SetPendingMap(mapID);
				table.SetTableState(BuilderTable.TableState.WaitingForInitalBuild);
				this.PlayerEnterBuilder();
			}
		}

		private void OnLoadSharedBlocksFailed(string mapID)
		{
			BuilderTable table = this.GetTable();
			string pendingMap = table.GetPendingMap();
			if (!pendingMap.IsNullOrEmpty() && !pendingMap.Equals(mapID))
			{
				GTDev.LogWarning<string>("BuilderTableNetworking OnLoadSharedBlocksFailed Unexpected map ID " + mapID, null);
			}
			BuilderTable.TableState tableState = table.GetTableState();
			if (!NetworkSystem.Instance.IsMasterClient && tableState != BuilderTable.TableState.WaitForMasterResync && tableState != BuilderTable.TableState.WaitingForInitalBuild && tableState != BuilderTable.TableState.Ready && tableState != BuilderTable.TableState.BadData)
			{
				GTDev.LogWarning<string>(string.Format("BuilderTableNetworking OnLoadSharedBlocksFailed Unexpected table state {0}", tableState), null);
				return;
			}
			if (NetworkSystem.Instance.IsMasterClient && tableState != BuilderTable.TableState.WaitingForSharedMapLoad && tableState != BuilderTable.TableState.WaitForInitialBuildMaster && tableState != BuilderTable.TableState.Ready && tableState != BuilderTable.TableState.BadData)
			{
				GTDev.LogWarning<string>(string.Format("BuilderTableNetworking OnLoadSharedBlocksFailed Unexpected table state {0}", tableState), null);
				return;
			}
			table.SetPendingMap(null);
			if (table != null && !table.isTableMutable && table.linkedTerminal != null)
			{
				if (!SharedBlocksManager.IsMapIDValid(mapID))
				{
					UnityEvent<string> onMapLoadFailed = table.OnMapLoadFailed;
					if (onMapLoadFailed == null)
					{
						return;
					}
					onMapLoadFailed.Invoke("BAD MAP ID");
					return;
				}
				else
				{
					UnityEvent<string> onMapLoadFailed2 = table.OnMapLoadFailed;
					if (onMapLoadFailed2 == null)
					{
						return;
					}
					onMapLoadFailed2.Invoke("LOAD FAILED");
				}
			}
		}

		private void OnSharedBlocksOutOfBounds(string mapID)
		{
			BuilderTable table = this.GetTable();
			string pendingMap = table.GetPendingMap();
			if (!pendingMap.IsNullOrEmpty() && !pendingMap.Equals(mapID))
			{
				GTDev.LogWarning<string>("BuilderTableNetworking OnSharedBlocksOutOfBounds Unexpected map ID " + mapID, null);
			}
			BuilderTable.TableState tableState = table.GetTableState();
			if (!NetworkSystem.Instance.IsMasterClient && tableState != BuilderTable.TableState.WaitForMasterResync && tableState != BuilderTable.TableState.WaitingForInitalBuild)
			{
				GTDev.LogWarning<string>(string.Format("BuilderTableNetworking OnSharedBlocksOutOfBounds Unexpected table state {0}", tableState), null);
				return;
			}
			if (NetworkSystem.Instance.IsMasterClient && tableState != BuilderTable.TableState.WaitForInitialBuildMaster && tableState != BuilderTable.TableState.BadData)
			{
				GTDev.LogWarning<string>(string.Format("BuilderTableNetworking OnSharedBlocksOutOfBounds Unexpected table state {0}", tableState), null);
				return;
			}
			table.SetPendingMap(null);
			if (table != null && !table.isTableMutable && table.linkedTerminal != null)
			{
				UnityEvent<string> onMapLoadFailed = table.OnMapLoadFailed;
				if (onMapLoadFailed == null)
				{
					return;
				}
				onMapLoadFailed.Invoke("BLOCKS ARE OUT OF BOUNDS FOR SHARED BLOCKS ROOM");
			}
		}

		public void RequestPaintPiece(int pieceID, int materialType)
		{
		}

		public PhotonView tablePhotonView;

		private const int MAX_TABLE_BYTES = 1048576;

		private const int MAX_TABLE_CHUNK_BYTES = 1000;

		private const float DELAY_CLIENT_TABLE_CREATION_TIME = 1f;

		private const float SEND_INIT_DATA_COOLDOWN = 0f;

		private const int PIECE_SYNC_BYTES = 128;

		private BuilderTable currTable;

		private int nextLocalCommandId;

		private List<BuilderTableNetworking.PlayerTableInitState> masterClientTableInit;

		private List<BuilderTableNetworking.PlayerTableInitState> masterClientTableValidators;

		private BuilderTableNetworking.PlayerTableInitState localClientTableInit;

		private BuilderTableNetworking.PlayerTableInitState localValidationTable;

		[HideInInspector]
		public List<Player> armShelfRequests;

		private CallLimiter[] callLimiters;

		public class PlayerTableInitState
		{
			public PlayerTableInitState()
			{
				this.serializedTableState = new byte[1048576];
				this.chunk = new byte[1000];
				this.Reset();
			}

			public void Reset()
			{
				this.player = null;
				this.numSerializedBytes = 0;
				this.totalSerializedBytes = 0;
			}

			public Player player;

			public int numSerializedBytes;

			public int totalSerializedBytes;

			public byte[] serializedTableState;

			public byte[] chunk;

			public float waitForInitTimeRemaining;

			public float sendNextChunkTimeRemaining;
		}

		private enum RPC
		{
			PlayerEnterMaster,
			TableDataMaster,
			TableData,
			TableDataStart,
			PlacePieceMaster,
			PlacePiece,
			GrabPieceMaster,
			GrabPiece,
			DropPieceMaster,
			DropPiece,
			RequestFailed,
			PieceDropZone,
			CreatePiece,
			CreatePieceMaster,
			CreateShelfPieceMaster,
			RecyclePieceMaster,
			PlotClaimedMaster,
			ArmShelfCreated,
			ShelfSelection,
			ShelfSelectionMaster,
			SetFunctionalState,
			SetFunctionalStateMaster,
			RequestTerminalControl,
			SetTerminalDriver,
			LoadSharedBlocksMap,
			SharedTableEvent,
			Count
		}

		private enum SharedTableEventTypes
		{
			LOAD_STARTED,
			LOAD_FAILED,
			OUT_OF_BOUNDS,
			COUNT
		}
	}
}
