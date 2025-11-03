using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion;
using GorillaExtensions;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Scripting;

namespace GorillaTagScripts
{
	public class FriendshipGroupDetection : NetworkSceneObject, ITickSystemTick
	{
		public static FriendshipGroupDetection Instance { get; private set; }

		public List<Color> myBeadColors { get; private set; } = new List<Color>();

		public Color myBraceletColor { get; private set; }

		public int MyBraceletSelfIndex { get; private set; }

		public List<string> PartyMemberIDs
		{
			get
			{
				return this.myPartyMemberIDs;
			}
		}

		public bool IsInParty
		{
			get
			{
				return this.myPartyMemberIDs != null;
			}
		}

		public GroupJoinZoneAB partyZone { get; private set; }

		public bool TickRunning { get; set; }

		private void Awake()
		{
			FriendshipGroupDetection.Instance = this;
			if (this.friendshipBubble)
			{
				this.particleSystem = this.friendshipBubble.GetComponent<ParticleSystem>();
				this.audioSource = this.friendshipBubble.GetComponent<AudioSource>();
			}
			NetworkSystem.Instance.OnPlayerJoined += this.OnPlayerJoinedRoom;
		}

		private new void OnEnable()
		{
			NetworkBehaviourUtils.InternalOnEnable(this);
			base.OnEnable();
			TickSystem<object>.AddTickCallback(this);
		}

		private new void OnDisable()
		{
			NetworkBehaviourUtils.InternalOnDisable(this);
			base.OnDisable();
			TickSystem<object>.RemoveTickCallback(this);
		}

		private void OnPlayerJoinedRoom(NetPlayer joiningPlayer)
		{
			if (!this.IsInParty)
			{
				return;
			}
			bool flag = (int)RoomSystem.GetRoomSize("") == NetworkSystem.Instance.RoomPlayerCount;
			Debug.Log(string.Concat(new string[]
			{
				"[FriendshipGroupDetection::OnPlayerJoinedRoom] JoiningPlayer: ",
				joiningPlayer.NickName,
				", ",
				joiningPlayer.UserId,
				" ",
				string.Format("| IsLocal: {0} | Room Full: {1}", joiningPlayer.IsLocal, flag)
			}));
			if (joiningPlayer.IsLocal)
			{
				this.lastJoinedRoomTime = (double)Time.time;
				if (!flag)
				{
					Debug.Log("[FriendshipGroupDetection::OnPlayerJoinedRoom] Delaying PartyRefresh...");
					this.wantsPartyRefreshPostJoin = true;
					return;
				}
			}
			if (flag)
			{
				this.RefreshPartyMembers();
			}
		}

		public void AddGroupZoneCallback(Action<GroupJoinZoneAB> callback)
		{
			this.groupZoneCallbacks.Add(callback);
		}

		public void RemoveGroupZoneCallback(Action<GroupJoinZoneAB> callback)
		{
			this.groupZoneCallbacks.Remove(callback);
		}

		public bool IsInMyGroup(string userID)
		{
			return this.myPartyMemberIDs != null && this.myPartyMemberIDs.Contains(userID);
		}

		public bool AnyPartyMembersOutsideFriendCollider()
		{
			if (!this.IsInParty)
			{
				return false;
			}
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig.IsLocalPartyMember && !GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(vrrig.creator.UserId))
				{
					return true;
				}
			}
			return false;
		}

		public bool DidJoinLeftHanded { get; private set; }

		public void Tick()
		{
			if (this.wantsPartyRefreshPostJoin && this.lastJoinedRoomTime + this.joinedRoomRefreshPartyDelay < (double)Time.time)
			{
				this.RefreshPartyMembers();
			}
			if (this.wantsPartyRefreshPostFollowFailed && this.lastFailedToFollowPartyTime + this.failedToFollowRefreshPartyDelay < (double)Time.time)
			{
				this.RefreshPartyMembers();
			}
			List<int> list = this.playersInProvisionalGroup;
			List<int> list2 = this.playersInProvisionalGroup;
			List<int> list3 = this.tempIntList;
			this.tempIntList = list2;
			this.playersInProvisionalGroup = list3;
			Vector3 vector;
			this.UpdateProvisionalGroup(out vector);
			if (this.playersInProvisionalGroup.Count > 0)
			{
				this.friendshipBubble.transform.position = vector;
			}
			bool flag = false;
			if (list.Count == this.playersInProvisionalGroup.Count)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != this.playersInProvisionalGroup[i])
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				this.groupCreateAfterTimestamp = Time.time + this.groupTime;
				this.amFirstProvisionalPlayer = this.playersInProvisionalGroup.Count > 0 && this.playersInProvisionalGroup[0] == NetworkSystem.Instance.LocalPlayer.ActorNumber;
				if (this.playersInProvisionalGroup.Count > 0 && !this.amFirstProvisionalPlayer)
				{
					List<int> list4 = this.tempIntList;
					list4.Clear();
					NetPlayer netPlayer = null;
					foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
					{
						if (vrrig.creator.ActorNumber == this.playersInProvisionalGroup[0])
						{
							netPlayer = vrrig.creator;
							if (vrrig.IsLocalPartyMember)
							{
								list4.Clear();
								break;
							}
						}
						else if (vrrig.IsLocalPartyMember)
						{
							list4.Add(vrrig.creator.ActorNumber);
						}
					}
					if (list4.Count > 0)
					{
						this.photonView.RPC("NotifyPartyMerging", netPlayer.GetPlayerRef(), new object[] { list4.ToArray() });
					}
					else
					{
						this.photonView.RPC("NotifyNoPartyToMerge", netPlayer.GetPlayerRef(), Array.Empty<object>());
					}
				}
				if (this.playersInProvisionalGroup.Count == 0)
				{
					if (Time.time > this.suppressPartyCreationUntilTimestamp && this.playEffectsAfterTimestamp == 0f)
					{
						this.audioSource.GTStop();
						this.audioSource.GTPlayOneShot(this.fistBumpInterruptedAudio, 1f);
					}
					this.particleSystem.Stop();
					this.playEffectsAfterTimestamp = 0f;
				}
				else
				{
					this.playEffectsAfterTimestamp = Time.time + this.playEffectsDelay;
				}
			}
			else if (this.playEffectsAfterTimestamp > 0f && Time.time > this.playEffectsAfterTimestamp)
			{
				this.audioSource.time = 0f;
				this.audioSource.GTPlay();
				this.particleSystem.Play();
				this.playEffectsAfterTimestamp = 0f;
			}
			else if (this.playersInProvisionalGroup.Count > 0 && Time.time > this.groupCreateAfterTimestamp && this.amFirstProvisionalPlayer)
			{
				List<int> list5 = this.tempIntList;
				list5.Clear();
				list5.AddRange(this.playersInProvisionalGroup);
				int num = 0;
				if (this.IsInParty)
				{
					foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
					{
						if (vrrig2.IsLocalPartyMember)
						{
							list5.Add(vrrig2.creator.ActorNumber);
							num++;
						}
					}
				}
				int num2 = 0;
				foreach (int num3 in this.playersInProvisionalGroup)
				{
					int[] array;
					if (this.partyMergeIDs.TryGetValue(num3, out array))
					{
						list5.AddRange(array);
						num2++;
					}
				}
				list5.Sort();
				int[] array2 = list5.Distinct<int>().ToArray<int>();
				this.myBraceletColor = GTColor.RandomHSV(this.braceletRandomColorHSVRanges);
				this.SendPartyFormedRPC(FriendshipGroupDetection.PackColor(this.myBraceletColor), array2, false);
				this.groupCreateAfterTimestamp = Time.time + this.cooldownAfterCreatingGroup;
			}
			if (this.myPartyMemberIDs != null)
			{
				this.UpdateWarningSigns();
			}
		}

		private void UpdateProvisionalGroup(out Vector3 midpoint)
		{
			this.playersInProvisionalGroup.Clear();
			bool flag;
			VRMap makingFist = VRRig.LocalRig.GetMakingFist(this.debug, out flag);
			if (makingFist == null || !NetworkSystem.Instance.InRoom || VRRig.LocalRig.leftHandLink.IsLinkActive() || VRRig.LocalRig.rightHandLink.IsLinkActive() || GorillaParent.instance.vrrigs.Count == 0 || Time.time < this.suppressPartyCreationUntilTimestamp || (global::GorillaGameModes.GameMode.ActiveGameMode != null && !global::GorillaGameModes.GameMode.ActiveGameMode.CanJoinFrienship(NetworkSystem.Instance.LocalPlayer)))
			{
				midpoint = Vector3.zero;
				return;
			}
			this.WillJoinLeftHanded = flag;
			this.playersToPropagateFrom.Clear();
			this.provisionalGroupUsingLeftHands.Clear();
			this.playersMakingFists.Clear();
			int actorNumber = NetworkSystem.Instance.LocalPlayer.ActorNumber;
			int num = -1;
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				bool flag2;
				VRMap makingFist2 = vrrig.GetMakingFist(this.debug, out flag2);
				if (makingFist2 != null && !vrrig.leftHandLink.IsLinkActive() && !vrrig.rightHandLink.IsLinkActive() && (!(global::GorillaGameModes.GameMode.ActiveGameMode != null) || global::GorillaGameModes.GameMode.ActiveGameMode.CanJoinFrienship(vrrig.OwningNetPlayer)))
				{
					FriendshipGroupDetection.PlayerFist playerFist = new FriendshipGroupDetection.PlayerFist
					{
						actorNumber = vrrig.creator.ActorNumber,
						position = makingFist2.rigTarget.position,
						isLeftHand = flag2
					};
					if (vrrig.isOfflineVRRig)
					{
						num = this.playersMakingFists.Count;
					}
					this.playersMakingFists.Add(playerFist);
				}
			}
			if (this.playersMakingFists.Count <= 1 || num == -1)
			{
				midpoint = Vector3.zero;
				return;
			}
			this.playersToPropagateFrom.Enqueue(this.playersMakingFists[num]);
			this.playersInProvisionalGroup.Add(actorNumber);
			midpoint = makingFist.rigTarget.position;
			int num2 = 1 << num;
			FriendshipGroupDetection.PlayerFist playerFist2;
			while (this.playersToPropagateFrom.TryDequeue(out playerFist2))
			{
				for (int i = 0; i < this.playersMakingFists.Count; i++)
				{
					if ((num2 & (1 << i)) == 0)
					{
						FriendshipGroupDetection.PlayerFist playerFist3 = this.playersMakingFists[i];
						if ((playerFist2.position - playerFist3.position).IsShorterThan(this.detectionRadius))
						{
							int num3 = ~this.playersInProvisionalGroup.BinarySearch(playerFist3.actorNumber);
							num2 |= 1 << i;
							this.playersInProvisionalGroup.Insert(num3, playerFist3.actorNumber);
							if (playerFist3.isLeftHand)
							{
								this.provisionalGroupUsingLeftHands.Add(playerFist3.actorNumber);
							}
							this.playersToPropagateFrom.Enqueue(playerFist3);
							midpoint += playerFist3.position;
						}
					}
				}
			}
			if (this.playersInProvisionalGroup.Count == 1)
			{
				this.playersInProvisionalGroup.Clear();
			}
			if (this.playersInProvisionalGroup.Count > 0)
			{
				midpoint /= (float)this.playersInProvisionalGroup.Count;
			}
		}

		private void UpdateWarningSigns()
		{
			ZoneEntity zoneEntity = GorillaTagger.Instance.offlineVRRig.zoneEntity;
			GTZone currentRoomZone = PhotonNetworkController.Instance.CurrentRoomZone;
			GroupJoinZoneAB groupJoinZoneAB = 0;
			if (this.myPartyMemberIDs != null)
			{
				foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
				{
					if (vrrig.IsLocalPartyMember && !vrrig.isOfflineVRRig)
					{
						groupJoinZoneAB |= vrrig.zoneEntity.GroupZone;
					}
				}
			}
			if (groupJoinZoneAB != this.partyZone)
			{
				this.debugStr.Clear();
				foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
				{
					if (vrrig2.IsLocalPartyMember && !vrrig2.isOfflineVRRig)
					{
						this.debugStr.Append(string.Format("{0} in {1};", vrrig2.playerNameVisible, vrrig2.zoneEntity.GroupZone));
					}
				}
				this.partyZone = groupJoinZoneAB;
				foreach (Action<GroupJoinZoneAB> action in this.groupZoneCallbacks)
				{
					action(this.partyZone);
				}
			}
		}

		[PunRPC]
		private void NotifyNoPartyToMerge(PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "NotifyNoPartyToMerge");
			if (info.Sender == null || this.partyMergeIDs == null)
			{
				return;
			}
			this.partyMergeIDs.Remove(info.Sender.ActorNumber);
		}

		[Rpc]
		private unsafe static void RPC_NotifyNoPartyToMerge(NetworkRunner runner, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					int num = 8;
					if (SimulationMessage.CanAllocateUserPayload(num))
					{
						if (runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyNoPartyToMerge(Fusion.NetworkRunner,Fusion.RpcInfo)"));
							int num2 = 8;
							ptr->Offset = num2 * 8;
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
						info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
						goto IL_0010;
					}
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyNoPartyToMerge(Fusion.NetworkRunner,Fusion.RpcInfo)", num);
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.partyMergeIDs.Remove(info.Source.PlayerId);
		}

		[PunRPC]
		private void NotifyPartyMerging(int[] memberIDs, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "NotifyPartyMerging");
			if (memberIDs == null)
			{
				return;
			}
			if (memberIDs.Length > 10)
			{
				return;
			}
			this.partyMergeIDs[info.Sender.ActorNumber] = memberIDs;
		}

		[Rpc]
		private unsafe static void RPC_NotifyPartyMerging(NetworkRunner runner, [RpcTarget] PlayerRef playerRef, int[] memberIDs, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(playerRef);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(playerRef, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyMerging(Fusion.NetworkRunner,Fusion.PlayerRef,System.Int32[],Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						num += (memberIDs.Length * 4 + 4 + 3) & -4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyMerging(Fusion.NetworkRunner,Fusion.PlayerRef,System.Int32[],Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyMerging(Fusion.NetworkRunner,Fusion.PlayerRef,System.Int32[],Fusion.RpcInfo)"));
							int num2 = 8;
							*(int*)(ptr2 + num2) = memberIDs.Length;
							num2 += 4;
							num2 = ((Native.CopyFromArray<int>((void*)(ptr2 + num2), memberIDs) + 3) & -4) + num2;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(playerRef);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			if (memberIDs.Length > 10)
			{
				return;
			}
			FriendshipGroupDetection.Instance.partyMergeIDs[info.Source.PlayerId] = memberIDs;
		}

		public void SendAboutToGroupJoin()
		{
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				Debug.Log(string.Concat(new string[]
				{
					"Sending group join to ",
					GorillaParent.instance.vrrigs.Count.ToString(),
					" players. Party member:",
					vrrig.OwningNetPlayer.NickName,
					"Is offline rig",
					vrrig.isOfflineVRRig.ToString()
				}));
				if (vrrig.IsLocalPartyMember && !vrrig.isOfflineVRRig)
				{
					this.photonView.RPC("PartyMemberIsAboutToGroupJoin", vrrig.Creator.GetPlayerRef(), Array.Empty<object>());
				}
			}
		}

		[PunRPC]
		private void PartyMemberIsAboutToGroupJoin(PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PartyMemberIsAboutToGroupJoin");
			this.PartMemberIsAboutToGroupJoinWrapped(new PhotonMessageInfoWrapped(info));
		}

		[Rpc]
		private unsafe static void RPC_PartyMemberIsAboutToGroupJoin(NetworkRunner runner, [RpcTarget] PlayerRef targetPlayer, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(targetPlayer);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(targetPlayer, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyMemberIsAboutToGroupJoin(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyMemberIsAboutToGroupJoin(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyMemberIsAboutToGroupJoin(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)"));
							int num2 = 8;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(targetPlayer);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.PartMemberIsAboutToGroupJoinWrapped(new PhotonMessageInfoWrapped(info));
		}

		private void PartMemberIsAboutToGroupJoinWrapped(PhotonMessageInfoWrapped wrappedInfo)
		{
			float time = Time.time;
			float num = this.aboutToGroupJoin_CooldownUntilTimestamp;
			if (wrappedInfo.senderID < NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				this.aboutToGroupJoin_CooldownUntilTimestamp = Time.time + 5f;
				if (this.myPartyMembersHash.Contains(wrappedInfo.Sender.UserId))
				{
					PhotonNetworkController.Instance.DeferJoining(2f);
				}
			}
		}

		private void SendPartyFormedRPC(short braceletColor, int[] memberIDs, bool forceDebug)
		{
			string text = Enum.Parse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true).ToString();
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (this.playersInProvisionalGroup.BinarySearch(vrrig.creator.ActorNumber) >= 0)
				{
					this.photonView.RPC("PartyFormedSuccessfully", vrrig.Creator.GetPlayerRef(), new object[] { text, braceletColor, memberIDs, forceDebug });
				}
			}
		}

		[Rpc]
		private unsafe static void RPC_PartyFormedSuccessfully(NetworkRunner runner, [RpcTarget] PlayerRef targetPlayer, string partyGameMode, short braceletColor, int[] memberIDs, bool forceDebug, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(targetPlayer);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(targetPlayer, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyFormedSuccessfully(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],System.Boolean,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						num += (ReadWriteUtilsForWeaver.GetByteCountUtf8NoHash(partyGameMode) + 3) & -4;
						num += 4;
						num += (memberIDs.Length * 4 + 4 + 3) & -4;
						num += 4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyFormedSuccessfully(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],System.Boolean,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyFormedSuccessfully(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],System.Boolean,Fusion.RpcInfo)"));
							int num2 = 8;
							num2 = ((ReadWriteUtilsForWeaver.WriteStringUtf8NoHash((void*)(ptr2 + num2), partyGameMode) + 3) & -4) + num2;
							*(short*)(ptr2 + num2) = braceletColor;
							num2 += (2 + 3) & -4;
							*(int*)(ptr2 + num2) = memberIDs.Length;
							num2 += 4;
							num2 = ((Native.CopyFromArray<int>((void*)(ptr2 + num2), memberIDs) + 3) & -4) + num2;
							ReadWriteUtilsForWeaver.WriteBoolean((int*)(ptr2 + num2), forceDebug);
							num2 += 4;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(targetPlayer);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			GorillaNot.IncrementRPCCall(info, "PartyFormedSuccessfully");
			FriendshipGroupDetection.Instance.PartyFormedSuccesfullyWrapped(partyGameMode, braceletColor, memberIDs, forceDebug, new PhotonMessageInfoWrapped(info));
		}

		[PunRPC]
		private void PartyFormedSuccessfully(string partyGameMode, short braceletColor, int[] memberIDs, bool forceDebug, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PartyFormedSuccessfully");
			this.PartyFormedSuccesfullyWrapped(partyGameMode, braceletColor, memberIDs, forceDebug, new PhotonMessageInfoWrapped(info));
		}

		private void PartyFormedSuccesfullyWrapped(string partyGameMode, short braceletColor, int[] memberIDs, bool forceDebug, PhotonMessageInfoWrapped info)
		{
			if (memberIDs == null || memberIDs.Length > 10 || !memberIDs.Contains(info.Sender.ActorNumber) || this.playersInProvisionalGroup.IndexOf(info.Sender.ActorNumber) != 0 || Mathf.Abs(this.groupCreateAfterTimestamp - Time.time) > this.m_maxGroupJoinTimeDifference || !global::GorillaGameModes.GameMode.IsValidGameMode(partyGameMode))
			{
				return;
			}
			if (this.IsInParty)
			{
				string text = Enum.Parse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true).ToString();
				foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
				{
					if (vrrig.IsLocalPartyMember && !vrrig.isOfflineVRRig)
					{
						this.photonView.RPC("AddPartyMembers", vrrig.Creator.GetPlayerRef(), new object[] { text, braceletColor, memberIDs });
					}
				}
			}
			this.suppressPartyCreationUntilTimestamp = Time.time + this.cooldownAfterCreatingGroup;
			this.DidJoinLeftHanded = this.WillJoinLeftHanded;
			this.SetNewParty(partyGameMode, braceletColor, memberIDs);
		}

		[PunRPC]
		private void AddPartyMembers(string partyGameMode, short braceletColor, int[] memberIDs, PhotonMessageInfo info)
		{
			this.AddPartyMembersWrapped(partyGameMode, braceletColor, memberIDs, new PhotonMessageInfoWrapped(info));
		}

		[Rpc]
		private unsafe static void RPC_AddPartyMembers(NetworkRunner runner, [RpcTarget] PlayerRef rpcTarget, string partyGameMode, short braceletColor, int[] memberIDs, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(rpcTarget);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(rpcTarget, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_AddPartyMembers(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						num += (ReadWriteUtilsForWeaver.GetByteCountUtf8NoHash(partyGameMode) + 3) & -4;
						num += 4;
						num += (memberIDs.Length * 4 + 4 + 3) & -4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_AddPartyMembers(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_AddPartyMembers(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],Fusion.RpcInfo)"));
							int num2 = 8;
							num2 = ((ReadWriteUtilsForWeaver.WriteStringUtf8NoHash((void*)(ptr2 + num2), partyGameMode) + 3) & -4) + num2;
							*(short*)(ptr2 + num2) = braceletColor;
							num2 += (2 + 3) & -4;
							*(int*)(ptr2 + num2) = memberIDs.Length;
							num2 += 4;
							num2 = ((Native.CopyFromArray<int>((void*)(ptr2 + num2), memberIDs) + 3) & -4) + num2;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(rpcTarget);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.AddPartyMembersWrapped(partyGameMode, braceletColor, memberIDs, new PhotonMessageInfoWrapped(info));
		}

		private void AddPartyMembersWrapped(string partyGameMode, short braceletColor, int[] memberIDs, PhotonMessageInfoWrapped infoWrapped)
		{
			GorillaNot.IncrementRPCCall(infoWrapped, "AddPartyMembersWrapped");
			if (memberIDs.Length > 10 || !this.IsInParty || !this.myPartyMembersHash.Contains(NetworkSystem.Instance.GetUserID(infoWrapped.senderID)) || !global::GorillaGameModes.GameMode.IsValidGameMode(partyGameMode))
			{
				return;
			}
			Debug.Log("Adding party members: [" + string.Join<int>(",", memberIDs) + "]");
			this.SetNewParty(partyGameMode, braceletColor, memberIDs);
		}

		private void SetNewParty(string partyGameMode, short braceletColor, int[] memberIDs)
		{
			GorillaComputer.instance.SetGameModeWithoutButton(partyGameMode);
			this.myPartyMemberIDs = new List<string>();
			FriendshipGroupDetection.userIdLookup.Clear();
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				FriendshipGroupDetection.userIdLookup.Add(vrrig.creator.ActorNumber, vrrig.creator.UserId);
			}
			foreach (int num in memberIDs)
			{
				string text;
				if (FriendshipGroupDetection.userIdLookup.TryGetValue(num, out text))
				{
					this.myPartyMemberIDs.Add(text);
				}
			}
			this.myBraceletColor = FriendshipGroupDetection.UnpackColor(braceletColor);
			GorillaTagger.Instance.StartVibration(this.DidJoinLeftHanded, this.hapticStrength, this.hapticDuration);
			this.OnPartyMembershipChanged();
			PlayerGameEvents.MiscEvent("FriendshipGroupJoined", 1);
		}

		public void LeaveParty()
		{
			if (this.myPartyMemberIDs == null)
			{
				return;
			}
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig.IsLocalPartyMember && !vrrig.isOfflineVRRig)
				{
					this.photonView.RPC("PlayerLeftParty", vrrig.Creator.GetPlayerRef(), Array.Empty<object>());
				}
			}
			this.myPartyMemberIDs = null;
			this.OnPartyMembershipChanged();
			PhotonNetworkController.Instance.ClearDeferredJoin();
			GorillaTagger.Instance.StartVibration(false, this.hapticStrength, this.hapticDuration);
		}

		public void OnFailedToFollowParty()
		{
			if (!this.IsInParty)
			{
				return;
			}
			this.lastFailedToFollowPartyTime = (double)Time.time;
			this.wantsPartyRefreshPostFollowFailed = true;
		}

		public void RefreshPartyMembers()
		{
			if (this.myPartyMemberIDs.IsNullOrEmpty<string>())
			{
				return;
			}
			Debug.Log("[FriendshipGroupDetection::RefreshPartyMembers] refreshing...");
			List<string> list = new List<string>(this.myPartyMemberIDs);
			Debug.Log("[FriendshipGroupDetection::RefreshPartyMembers] found " + string.Format("{0} current players in Room...", NetworkSystem.Instance.AllNetPlayers.Length));
			for (int i = 0; i < NetworkSystem.Instance.AllNetPlayers.Length; i++)
			{
				if (NetworkSystem.Instance.AllNetPlayers[i] != null)
				{
					list.Remove(NetworkSystem.Instance.AllNetPlayers[i].UserId);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				Debug.Log("[FriendshipGroupDetection::RefreshPartyMembers] removing missing player " + list[j] + " from party...");
				this.PlayerIDLeftParty(list[j]);
			}
			this.wantsPartyRefreshPostJoin = false;
			this.wantsPartyRefreshPostFollowFailed = false;
		}

		[Rpc]
		private unsafe static void RPC_PlayerLeftParty(NetworkRunner runner, [RpcTarget] PlayerRef player, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(player);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(player, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PlayerLeftParty(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PlayerLeftParty(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PlayerLeftParty(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)"));
							int num2 = 8;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(player);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			GorillaNot.IncrementRPCCall(info, "PlayerLeftParty");
			FriendshipGroupDetection.Instance.PlayerLeftPartyWrapped(new PhotonMessageInfoWrapped(info));
		}

		[PunRPC]
		private void PlayerLeftParty(PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "PlayerLeftParty");
			this.PlayerLeftPartyWrapped(new PhotonMessageInfoWrapped(info));
		}

		private void PlayerLeftPartyWrapped(PhotonMessageInfoWrapped infoWrapped)
		{
			if (this.myPartyMemberIDs == null)
			{
				return;
			}
			if (!this.myPartyMemberIDs.Remove(infoWrapped.Sender.UserId))
			{
				return;
			}
			if (this.myPartyMemberIDs.Count <= 1)
			{
				this.myPartyMemberIDs = null;
			}
			this.OnPartyMembershipChanged();
			GorillaTagger.Instance.StartVibration(this.DidJoinLeftHanded, this.hapticStrength, this.hapticDuration);
		}

		private void PlayerIDLeftParty(string userID)
		{
			if (this.myPartyMemberIDs == null)
			{
				return;
			}
			if (!this.myPartyMemberIDs.Remove(userID))
			{
				return;
			}
			if (this.myPartyMemberIDs.Count <= 1)
			{
				this.myPartyMemberIDs = null;
			}
			this.OnPartyMembershipChanged();
			GorillaTagger.Instance.StartVibration(this.DidJoinLeftHanded, this.hapticStrength, this.hapticDuration);
		}

		public void SendVerifyPartyMember(NetPlayer player)
		{
			this.photonView.RPC("VerifyPartyMember", player.GetPlayerRef(), Array.Empty<object>());
		}

		[PunRPC]
		private void VerifyPartyMember(PhotonMessageInfo info)
		{
			this.VerifyPartyMemberWrapped(new PhotonMessageInfoWrapped(info));
		}

		[Rpc]
		private unsafe static void RPC_VerifyPartyMember(NetworkRunner runner, [RpcTarget] PlayerRef rpcTarget, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(rpcTarget);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(rpcTarget, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_VerifyPartyMember(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_VerifyPartyMember(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_VerifyPartyMember(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)"));
							int num2 = 8;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(rpcTarget);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.VerifyPartyMemberWrapped(new PhotonMessageInfoWrapped(info));
		}

		private void VerifyPartyMemberWrapped(PhotonMessageInfoWrapped infoWrapped)
		{
			GorillaNot.IncrementRPCCall(infoWrapped, "VerifyPartyMemberWrapped");
			RigContainer rigContainer;
			if (!VRRigCache.Instance.TryGetVrrig(infoWrapped.Sender, out rigContainer) || !FXSystem.CheckCallSpam(rigContainer.Rig.fxSettings, 15, infoWrapped.SentServerTime))
			{
				return;
			}
			if (this.myPartyMemberIDs == null || !this.myPartyMemberIDs.Contains(NetworkSystem.Instance.GetUserID(infoWrapped.senderID)))
			{
				this.photonView.RPC("PlayerLeftParty", infoWrapped.Sender.GetPlayerRef(), Array.Empty<object>());
			}
		}

		public void SendRequestPartyGameMode(string gameMode)
		{
			int num = int.MaxValue;
			NetPlayer netPlayer = null;
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig.IsLocalPartyMember && vrrig.creator.ActorNumber < num)
				{
					netPlayer = vrrig.creator;
					num = vrrig.creator.ActorNumber;
				}
			}
			if (netPlayer != null)
			{
				this.photonView.RPC("RequestPartyGameMode", netPlayer.GetPlayerRef(), new object[] { gameMode });
			}
		}

		[Rpc]
		private unsafe static void RPC_RequestPartyGameMode(NetworkRunner runner, [RpcTarget] PlayerRef targetPlayer, string gameMode, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(targetPlayer);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(targetPlayer, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_RequestPartyGameMode(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						num += (ReadWriteUtilsForWeaver.GetByteCountUtf8NoHash(gameMode) + 3) & -4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_RequestPartyGameMode(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_RequestPartyGameMode(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)"));
							int num2 = 8;
							num2 = ((ReadWriteUtilsForWeaver.WriteStringUtf8NoHash((void*)(ptr2 + num2), gameMode) + 3) & -4) + num2;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(targetPlayer);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.RequestPartyGameModeWrapped(gameMode, new PhotonMessageInfoWrapped(info));
		}

		[PunRPC]
		private void RequestPartyGameMode(string gameMode, PhotonMessageInfo info)
		{
			this.RequestPartyGameModeWrapped(gameMode, new PhotonMessageInfoWrapped(info));
		}

		private void RequestPartyGameModeWrapped(string gameMode, PhotonMessageInfoWrapped info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestPartyGameModeWrapped");
			if (!this.IsInParty || !this.IsInMyGroup(info.Sender.UserId) || !global::GorillaGameModes.GameMode.IsValidGameMode(gameMode))
			{
				return;
			}
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig.IsLocalPartyMember)
				{
					this.photonView.RPC("NotifyPartyGameModeChanged", vrrig.creator.GetPlayerRef(), new object[] { gameMode });
				}
			}
		}

		[Rpc]
		private unsafe static void RPC_NotifyPartyGameModeChanged(NetworkRunner runner, [RpcTarget] PlayerRef targetPlayer, string gameMode, RpcInfo info = default(RpcInfo))
		{
			if (NetworkBehaviourUtils.InvokeRpc)
			{
				NetworkBehaviourUtils.InvokeRpc = false;
			}
			else
			{
				if (runner == null)
				{
					throw new ArgumentNullException("runner");
				}
				if (runner.Stage != SimulationStages.Resimulate)
				{
					RpcTargetStatus rpcTargetStatus = runner.GetRpcTargetStatus(targetPlayer);
					if (rpcTargetStatus == RpcTargetStatus.Unreachable)
					{
						NetworkBehaviourUtils.NotifyRpcTargetUnreachable(targetPlayer, "System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyGameModeChanged(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)");
					}
					else
					{
						if (rpcTargetStatus == RpcTargetStatus.Self)
						{
							info = RpcInfo.FromLocal(runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0010;
						}
						int num = 8;
						num += (ReadWriteUtilsForWeaver.GetByteCountUtf8NoHash(gameMode) + 3) & -4;
						if (!SimulationMessage.CanAllocateUserPayload(num))
						{
							NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyGameModeChanged(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)", num);
						}
						else
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(NetworkBehaviourUtils.GetRpcStaticIndexOrThrow("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyGameModeChanged(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)"));
							int num2 = 8;
							num2 = ((ReadWriteUtilsForWeaver.WriteStringUtf8NoHash((void*)(ptr2 + num2), gameMode) + 3) & -4) + num2;
							ptr->Offset = num2 * 8;
							ptr->SetTarget(targetPlayer);
							ptr->SetStatic();
							runner.SendRpc(ptr);
						}
					}
				}
				return;
			}
			IL_0010:
			FriendshipGroupDetection.Instance.NotifyPartyGameModeChangedWrapped(gameMode, new PhotonMessageInfoWrapped(info));
		}

		[PunRPC]
		private void NotifyPartyGameModeChanged(string gameMode, PhotonMessageInfo info)
		{
			this.NotifyPartyGameModeChangedWrapped(gameMode, new PhotonMessageInfoWrapped(info));
		}

		private void NotifyPartyGameModeChangedWrapped(string gameMode, PhotonMessageInfoWrapped info)
		{
			GorillaNot.IncrementRPCCall(info, "NotifyPartyGameModeChangedWrapped");
			if (!this.IsInParty || !this.IsInMyGroup(info.Sender.UserId) || !global::GorillaGameModes.GameMode.IsValidGameMode(gameMode))
			{
				return;
			}
			GorillaComputer.instance.SetGameModeWithoutButton(gameMode);
		}

		private void OnPartyMembershipChanged()
		{
			this.myPartyMembersHash.Clear();
			if (this.myPartyMemberIDs != null)
			{
				foreach (string text in this.myPartyMemberIDs)
				{
					this.myPartyMembersHash.Add(text);
				}
			}
			this.myBeadColors.Clear();
			FriendshipGroupDetection.tempColorLookup.Clear();
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				vrrig.ClearPartyMemberStatus();
				if (vrrig.IsLocalPartyMember)
				{
					FriendshipGroupDetection.tempColorLookup.Add(vrrig.Creator.UserId, vrrig.playerColor);
				}
			}
			this.MyBraceletSelfIndex = 0;
			if (this.myPartyMemberIDs != null)
			{
				using (List<string>.Enumerator enumerator = this.myPartyMemberIDs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text2 = enumerator.Current;
						Color color;
						if (FriendshipGroupDetection.tempColorLookup.TryGetValue(text2, out color))
						{
							if (text2 == PhotonNetwork.LocalPlayer.UserId)
							{
								this.MyBraceletSelfIndex = this.myBeadColors.Count;
							}
							this.myBeadColors.Add(color);
						}
					}
					goto IL_0168;
				}
			}
			GorillaComputer.instance.SetGameModeWithoutButton(GorillaComputer.instance.lastPressedGameMode);
			this.wantsPartyRefreshPostJoin = false;
			this.wantsPartyRefreshPostFollowFailed = false;
			IL_0168:
			this.myBeadColors.Add(this.myBraceletColor);
			GorillaTagger.Instance.offlineVRRig.UpdateFriendshipBracelet();
			this.UpdateWarningSigns();
		}

		public bool IsPartyWithinCollider(GorillaFriendCollider friendCollider)
		{
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig.IsLocalPartyMember && !vrrig.isOfflineVRRig && !friendCollider.playerIDsCurrentlyTouching.Contains(vrrig.Creator.UserId))
				{
					return false;
				}
			}
			return true;
		}

		public static short PackColor(Color col)
		{
			return (short)(Mathf.RoundToInt(col.r * 9f) + Mathf.RoundToInt(col.g * 9f) * 10 + Mathf.RoundToInt(col.b * 9f) * 100);
		}

		public static Color UnpackColor(short data)
		{
			return new Color
			{
				r = (float)(data % 10) / 9f,
				g = (float)(data / 10 % 10) / 9f,
				b = (float)(data / 100 % 10) / 9f
			};
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyNoPartyToMerge(Fusion.NetworkRunner,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_NotifyNoPartyToMerge@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_NotifyNoPartyToMerge(runner, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyMerging(Fusion.NetworkRunner,Fusion.PlayerRef,System.Int32[],Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_NotifyPartyMerging@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			int num = 8;
			PlayerRef target = message->Target;
			int[] array = new int[*(int*)(ptr + num)];
			num += 4;
			num = ((Native.CopyToArray<int>(array, (void*)(ptr + num)) + 3) & -4) + num;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_NotifyPartyMerging(runner, target, array, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyMemberIsAboutToGroupJoin(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_PartyMemberIsAboutToGroupJoin@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			PlayerRef target = message->Target;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_PartyMemberIsAboutToGroupJoin(runner, target, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PartyFormedSuccessfully(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],System.Boolean,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_PartyFormedSuccessfully@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			int num = 8;
			PlayerRef target = message->Target;
			string text;
			num = ((ReadWriteUtilsForWeaver.ReadStringUtf8NoHash((void*)(ptr + num), out text) + 3) & -4) + num;
			short num2 = *(short*)(ptr + num);
			num += (2 + 3) & -4;
			short num3 = num2;
			int[] array = new int[*(int*)(ptr + num)];
			num += 4;
			num = ((Native.CopyToArray<int>(array, (void*)(ptr + num)) + 3) & -4) + num;
			bool flag = ReadWriteUtilsForWeaver.ReadBoolean((int*)(ptr + num));
			num += 4;
			bool flag2 = flag;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_PartyFormedSuccessfully(runner, target, text, num3, array, flag2, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_AddPartyMembers(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,System.Int16,System.Int32[],Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_AddPartyMembers@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			int num = 8;
			PlayerRef target = message->Target;
			string text;
			num = ((ReadWriteUtilsForWeaver.ReadStringUtf8NoHash((void*)(ptr + num), out text) + 3) & -4) + num;
			short num2 = *(short*)(ptr + num);
			num += (2 + 3) & -4;
			short num3 = num2;
			int[] array = new int[*(int*)(ptr + num)];
			num += 4;
			num = ((Native.CopyToArray<int>(array, (void*)(ptr + num)) + 3) & -4) + num;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_AddPartyMembers(runner, target, text, num3, array, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_PlayerLeftParty(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_PlayerLeftParty@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			PlayerRef target = message->Target;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_PlayerLeftParty(runner, target, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_VerifyPartyMember(Fusion.NetworkRunner,Fusion.PlayerRef,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_VerifyPartyMember@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			PlayerRef target = message->Target;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_VerifyPartyMember(runner, target, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_RequestPartyGameMode(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_RequestPartyGameMode@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			int num = 8;
			PlayerRef target = message->Target;
			string text;
			num = ((ReadWriteUtilsForWeaver.ReadStringUtf8NoHash((void*)(ptr + num), out text) + 3) & -4) + num;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_RequestPartyGameMode(runner, target, text, rpcInfo);
		}

		[NetworkRpcStaticWeavedInvoker("System.Void GorillaTagScripts.FriendshipGroupDetection::RPC_NotifyPartyGameModeChanged(Fusion.NetworkRunner,Fusion.PlayerRef,System.String,Fusion.RpcInfo)")]
		[Preserve]
		[WeaverGenerated]
		protected unsafe static void RPC_NotifyPartyGameModeChanged@Invoker(NetworkRunner runner, SimulationMessage* message)
		{
			byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
			int num = 8;
			PlayerRef target = message->Target;
			string text;
			num = ((ReadWriteUtilsForWeaver.ReadStringUtf8NoHash((void*)(ptr + num), out text) + 3) & -4) + num;
			RpcInfo rpcInfo = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsServer);
			NetworkBehaviourUtils.InvokeRpc = true;
			FriendshipGroupDetection.RPC_NotifyPartyGameModeChanged(runner, target, text, rpcInfo);
		}

		[SerializeField]
		private float detectionRadius = 0.5f;

		[SerializeField]
		private float groupTime = 5f;

		[SerializeField]
		private float cooldownAfterCreatingGroup = 5f;

		[SerializeField]
		private float hapticStrength = 1.5f;

		[SerializeField]
		private float hapticDuration = 2f;

		[SerializeField]
		private double joinedRoomRefreshPartyDelay = 30.0;

		[SerializeField]
		private double failedToFollowRefreshPartyDelay = 30.0;

		public bool debug;

		public double offset = 0.5;

		[SerializeField]
		private float m_maxGroupJoinTimeDifference = 1f;

		private List<string> myPartyMemberIDs;

		private HashSet<string> myPartyMembersHash = new HashSet<string>();

		private List<Action<GroupJoinZoneAB>> groupZoneCallbacks = new List<Action<GroupJoinZoneAB>>();

		[SerializeField]
		private GTColor.HSVRanges braceletRandomColorHSVRanges;

		public GameObject friendshipBubble;

		public AudioClip fistBumpInterruptedAudio;

		private ParticleSystem particleSystem;

		private AudioSource audioSource;

		private double lastJoinedRoomTime;

		private bool wantsPartyRefreshPostJoin;

		private double lastFailedToFollowPartyTime;

		private bool wantsPartyRefreshPostFollowFailed;

		private Queue<FriendshipGroupDetection.PlayerFist> playersToPropagateFrom = new Queue<FriendshipGroupDetection.PlayerFist>();

		private List<int> playersInProvisionalGroup = new List<int>();

		private List<int> provisionalGroupUsingLeftHands = new List<int>();

		private List<int> tempIntList = new List<int>();

		private bool amFirstProvisionalPlayer;

		private Dictionary<int, int[]> partyMergeIDs = new Dictionary<int, int[]>();

		private float groupCreateAfterTimestamp;

		private float playEffectsAfterTimestamp;

		[SerializeField]
		private float playEffectsDelay;

		private float suppressPartyCreationUntilTimestamp;

		private bool WillJoinLeftHanded;

		private List<FriendshipGroupDetection.PlayerFist> playersMakingFists = new List<FriendshipGroupDetection.PlayerFist>();

		private StringBuilder debugStr = new StringBuilder();

		private float aboutToGroupJoin_CooldownUntilTimestamp;

		private static Dictionary<int, string> userIdLookup = new Dictionary<int, string>();

		private static Dictionary<string, Color> tempColorLookup = new Dictionary<string, Color>();

		private struct PlayerFist
		{
			public int actorNumber;

			public Vector3 position;

			public bool isLeftHand;
		}
	}
}
