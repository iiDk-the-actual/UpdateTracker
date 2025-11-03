using System;
using System.Collections.Generic;
using Fusion;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;

public class VRRigReliableState : MonoBehaviour, IWrappedSerializable, INetworkStruct
{
	public bool HasBracelet
	{
		get
		{
			return this.braceletBeadColors.Count > 0;
		}
	}

	public bool isDirty { get; private set; } = true;

	private void Awake()
	{
		VRRig.newPlayerJoined = (Action)Delegate.Combine(VRRig.newPlayerJoined, new Action(this.SetIsDirty));
		RoomSystem.JoinedRoomEvent += new Action(this.SetIsDirty);
	}

	private void OnDestroy()
	{
		VRRig.newPlayerJoined = (Action)Delegate.Remove(VRRig.newPlayerJoined, new Action(this.SetIsDirty));
	}

	public void SetIsDirty()
	{
		this.isDirty = true;
	}

	public void SetIsNotDirty()
	{
		this.isDirty = false;
	}

	public void SharedStart(bool isOfflineVRRig_, BodyDockPositions bDock_)
	{
		this.isOfflineVRRig = isOfflineVRRig_;
		this.bDock = bDock_;
		this.activeTransferrableObjectIndex = new int[5];
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			this.activeTransferrableObjectIndex[i] = -1;
		}
		this.transferrablePosStates = new TransferrableObject.PositionState[5];
		this.transferrableItemStates = new TransferrableObject.ItemStates[5];
		this.transferableDockPositions = new BodyDockPositions.DropPositions[5];
	}

	void IWrappedSerializable.OnSerializeRead(object newData)
	{
		this.Data = (ReliableStateData)newData;
		long header = this.Data.Header;
		int num;
		this.SetHeader(header, out num);
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			if ((header & (1L << (i & 31))) != 0L)
			{
				long num2 = this.Data.TransferrableStates[i];
				this.activeTransferrableObjectIndex[i] = (int)num2;
				this.transferrablePosStates[i] = (TransferrableObject.PositionState)((num2 >> 32) & 255L);
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)((num2 >> 40) & 255L);
				this.transferableDockPositions[i] = (BodyDockPositions.DropPositions)((num2 >> 48) & 255L);
			}
			else
			{
				this.activeTransferrableObjectIndex[i] = -1;
				this.transferrablePosStates[i] = TransferrableObject.PositionState.None;
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)0;
				this.transferableDockPositions[i] = BodyDockPositions.DropPositions.None;
			}
		}
		this.wearablesPackedStates = this.Data.WearablesPackedState;
		this.lThrowableProjectileIndex = this.Data.LThrowableProjectileIndex;
		this.rThrowableProjectileIndex = this.Data.RThrowableProjectileIndex;
		this.sizeLayerMask = this.Data.SizeLayerMask;
		this.randomThrowableIndex = this.Data.RandomThrowableIndex;
		this.braceletBeadColors.Clear();
		if (num > 0)
		{
			if (num <= 3)
			{
				int num3 = (int)this.Data.PackedBeads;
				this.braceletSelfIndex = num3 >> 30;
				VRRigReliableState.UnpackBeadColors((long)num3, 0, num, this.braceletBeadColors);
			}
			else
			{
				long packedBeads = this.Data.PackedBeads;
				this.braceletSelfIndex = (int)(packedBeads >> 60);
				if (num <= 6)
				{
					VRRigReliableState.UnpackBeadColors(packedBeads, 0, num, this.braceletBeadColors);
				}
				else
				{
					VRRigReliableState.UnpackBeadColors(packedBeads, 0, 6, this.braceletBeadColors);
					VRRigReliableState.UnpackBeadColors(this.Data.PackedBeadsMoreThan6, 6, num, this.braceletBeadColors);
				}
			}
		}
		this.bDock.RefreshTransferrableItems();
		this.bDock.myRig.UpdateFriendshipBracelet();
	}

	object IWrappedSerializable.OnSerializeWrite()
	{
		this.isDirty = false;
		ReliableStateData reliableStateData = default(ReliableStateData);
		long header = this.GetHeader();
		reliableStateData.Header = header;
		long[] array = this.GetTransferrableStates(header).ToArray();
		reliableStateData.TransferrableStates.CopyFrom(array, 0, array.Length);
		reliableStateData.WearablesPackedState = this.wearablesPackedStates;
		reliableStateData.LThrowableProjectileIndex = this.lThrowableProjectileIndex;
		reliableStateData.RThrowableProjectileIndex = this.rThrowableProjectileIndex;
		reliableStateData.SizeLayerMask = this.sizeLayerMask;
		reliableStateData.RandomThrowableIndex = this.randomThrowableIndex;
		if (this.braceletBeadColors.Count > 0)
		{
			long num = VRRigReliableState.PackBeadColors(this.braceletBeadColors, 0);
			if (this.braceletBeadColors.Count <= 3)
			{
				num |= (long)this.braceletSelfIndex << 30;
				reliableStateData.PackedBeads = num;
			}
			else
			{
				num |= (long)this.braceletSelfIndex << 60;
				reliableStateData.PackedBeads = num;
				if (this.braceletBeadColors.Count > 6)
				{
					reliableStateData.PackedBeadsMoreThan6 = VRRigReliableState.PackBeadColors(this.braceletBeadColors, 6);
				}
			}
		}
		this.Data = reliableStateData;
		return reliableStateData;
	}

	void IWrappedSerializable.OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!this.isDirty)
		{
			return;
		}
		this.isDirty = false;
		long header = this.GetHeader();
		stream.SendNext(header);
		foreach (long num in this.GetTransferrableStates(header))
		{
			stream.SendNext(num);
		}
		stream.SendNext(this.wearablesPackedStates);
		stream.SendNext(this.lThrowableProjectileIndex);
		stream.SendNext(this.rThrowableProjectileIndex);
		stream.SendNext(this.sizeLayerMask);
		stream.SendNext(this.randomThrowableIndex);
		if (this.braceletBeadColors.Count > 0)
		{
			long num2 = VRRigReliableState.PackBeadColors(this.braceletBeadColors, 0);
			if (this.braceletBeadColors.Count <= 3)
			{
				num2 |= (long)this.braceletSelfIndex << 30;
				stream.SendNext((int)num2);
				return;
			}
			num2 |= (long)this.braceletSelfIndex << 60;
			stream.SendNext(num2);
			if (this.braceletBeadColors.Count > 6)
			{
				stream.SendNext(VRRigReliableState.PackBeadColors(this.braceletBeadColors, 6));
			}
		}
	}

	void IWrappedSerializable.OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
	{
		long num = (long)stream.ReceiveNext();
		this.isMicEnabled = (num & 32L) != 0L;
		this.isBraceletLeftHanded = (num & 64L) != 0L;
		this.isBuilderWatchEnabled = (num & 128L) != 0L;
		int num2 = (int)(num >> 12) & 15;
		this.lThrowableProjectileColor.r = (byte)(num >> 16);
		this.lThrowableProjectileColor.g = (byte)(num >> 24);
		this.lThrowableProjectileColor.b = (byte)(num >> 32);
		this.rThrowableProjectileColor.r = (byte)(num >> 40);
		this.rThrowableProjectileColor.g = (byte)(num >> 48);
		this.rThrowableProjectileColor.b = (byte)(num >> 56);
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			if ((num & (1L << (i & 31))) != 0L)
			{
				long num3 = (long)stream.ReceiveNext();
				this.activeTransferrableObjectIndex[i] = (int)num3;
				this.transferrablePosStates[i] = (TransferrableObject.PositionState)((num3 >> 32) & 255L);
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)((num3 >> 40) & 255L);
				this.transferableDockPositions[i] = (BodyDockPositions.DropPositions)((num3 >> 48) & 255L);
			}
			else
			{
				this.activeTransferrableObjectIndex[i] = -1;
				this.transferrablePosStates[i] = TransferrableObject.PositionState.None;
				this.transferrableItemStates[i] = (TransferrableObject.ItemStates)0;
				this.transferableDockPositions[i] = BodyDockPositions.DropPositions.None;
			}
		}
		this.wearablesPackedStates = (int)stream.ReceiveNext();
		this.lThrowableProjectileIndex = (int)stream.ReceiveNext();
		this.rThrowableProjectileIndex = (int)stream.ReceiveNext();
		this.sizeLayerMask = (int)stream.ReceiveNext();
		this.randomThrowableIndex = (int)stream.ReceiveNext();
		this.braceletBeadColors.Clear();
		if (num2 > 0)
		{
			if (num2 <= 3)
			{
				int num4 = (int)stream.ReceiveNext();
				this.braceletSelfIndex = num4 >> 30;
				VRRigReliableState.UnpackBeadColors((long)num4, 0, num2, this.braceletBeadColors);
			}
			else
			{
				long num5 = (long)stream.ReceiveNext();
				this.braceletSelfIndex = (int)(num5 >> 60);
				if (num2 <= 6)
				{
					VRRigReliableState.UnpackBeadColors(num5, 0, num2, this.braceletBeadColors);
				}
				else
				{
					VRRigReliableState.UnpackBeadColors(num5, 0, 6, this.braceletBeadColors);
					VRRigReliableState.UnpackBeadColors((long)stream.ReceiveNext(), 6, num2, this.braceletBeadColors);
				}
			}
		}
		if (CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			this.bDock.RefreshTransferrableItems();
		}
		this.bDock.myRig.UpdateFriendshipBracelet();
		this.bDock.myRig.EnableBuilderResizeWatch(this.isBuilderWatchEnabled);
	}

	private long GetHeader()
	{
		long num = 0L;
		if (CosmeticsController.instance.isHidingCosmeticsFromRemotePlayers)
		{
			for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
			{
				if (this.activeTransferrableObjectIndex[i] != -1 && (this.transferrablePosStates[i] == TransferrableObject.PositionState.InLeftHand || this.transferrablePosStates[i] == TransferrableObject.PositionState.InRightHand))
				{
					num |= (long)((ulong)((byte)(1 << i)));
				}
			}
		}
		else
		{
			for (int j = 0; j < this.activeTransferrableObjectIndex.Length; j++)
			{
				if (this.activeTransferrableObjectIndex[j] != -1)
				{
					num |= (long)((ulong)((byte)(1 << j)));
				}
			}
		}
		if (this.isBraceletLeftHanded)
		{
			num |= 64L;
		}
		if (this.isMicEnabled)
		{
			num |= 32L;
		}
		if (this.isBuilderWatchEnabled && !CosmeticsController.instance.isHidingCosmeticsFromRemotePlayers)
		{
			num |= 128L;
		}
		num |= ((long)this.braceletBeadColors.Count & 15L) << 12;
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.r) << 16);
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.g) << 24);
		num |= (long)((long)((ulong)this.lThrowableProjectileColor.b) << 32);
		num |= (long)((long)((ulong)this.rThrowableProjectileColor.r) << 40);
		num |= (long)((long)((ulong)this.rThrowableProjectileColor.g) << 48);
		return num | (long)((long)((ulong)this.rThrowableProjectileColor.b) << 56);
	}

	private void SetHeader(long header, out int numBeadsToRead)
	{
		this.isMicEnabled = (header & 32L) != 0L;
		this.isBraceletLeftHanded = (header & 64L) != 0L;
		numBeadsToRead = (int)(header >> 12) & 15;
		this.lThrowableProjectileColor.r = (byte)(header >> 16);
		this.lThrowableProjectileColor.g = (byte)(header >> 24);
		this.lThrowableProjectileColor.b = (byte)(header >> 32);
		this.rThrowableProjectileColor.r = (byte)(header >> 40);
		this.rThrowableProjectileColor.g = (byte)(header >> 48);
		this.rThrowableProjectileColor.b = (byte)(header >> 56);
	}

	private List<long> GetTransferrableStates(long header)
	{
		List<long> list = new List<long>();
		for (int i = 0; i < this.activeTransferrableObjectIndex.Length; i++)
		{
			if ((header & (1L << (i & 31))) != 0L && this.activeTransferrableObjectIndex[i] != -1)
			{
				long num = (long)((ulong)this.activeTransferrableObjectIndex[i]);
				num |= (long)this.transferrablePosStates[i] << 32;
				num |= (long)this.transferrableItemStates[i] << 40;
				num |= (long)this.transferableDockPositions[i] << 48;
				list.Add(num);
			}
		}
		return list;
	}

	private static long PackBeadColors(List<Color> beadColors, int fromIndex)
	{
		long num = 0L;
		int num2 = Mathf.Min(fromIndex + 6, beadColors.Count);
		int num3 = 0;
		for (int i = fromIndex; i < num2; i++)
		{
			long num4 = (long)FriendshipGroupDetection.PackColor(beadColors[i]);
			num |= num4 << num3;
			num3 += 10;
		}
		return num;
	}

	private static void UnpackBeadColors(long packed, int startIndex, int endIndex, List<Color> beadColorsResult)
	{
		int num = Mathf.Min(startIndex + 6, endIndex);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			short num3 = (short)((packed >> num2) & 1023L);
			beadColorsResult.Add(FriendshipGroupDetection.UnpackColor(num3));
			num2 += 10;
		}
	}

	[NonSerialized]
	public int[] activeTransferrableObjectIndex;

	[NonSerialized]
	public TransferrableObject.PositionState[] transferrablePosStates;

	[NonSerialized]
	public TransferrableObject.ItemStates[] transferrableItemStates;

	[NonSerialized]
	public BodyDockPositions.DropPositions[] transferableDockPositions;

	[NonSerialized]
	public int wearablesPackedStates;

	[NonSerialized]
	public int lThrowableProjectileIndex = -1;

	[NonSerialized]
	public int rThrowableProjectileIndex = -1;

	[NonSerialized]
	public Color32 lThrowableProjectileColor = Color.white;

	[NonSerialized]
	public Color32 rThrowableProjectileColor = Color.white;

	[NonSerialized]
	public int randomThrowableIndex;

	[NonSerialized]
	public bool isMicEnabled;

	private bool isOfflineVRRig;

	private BodyDockPositions bDock;

	[NonSerialized]
	public int sizeLayerMask = 1;

	private const long IS_MIC_ENABLED_BIT = 32L;

	private const long BRACELET_LEFTHAND_BIT = 64L;

	private const long BUILDER_WATCH_ENABLED_BIT = 128L;

	private const int BRACELET_NUM_BEADS_SHIFT = 12;

	private const int LPROJECTILECOLOR_R_SHIFT = 16;

	private const int LPROJECTILECOLOR_G_SHIFT = 24;

	private const int LPROJECTILECOLOR_B_SHIFT = 32;

	private const int RPROJECTILECOLOR_R_SHIFT = 40;

	private const int RPROJECTILECOLOR_G_SHIFT = 48;

	private const int RPROJECTILECOLOR_B_SHIFT = 56;

	private const int POS_STATES_SHIFT = 32;

	private const int ITEM_STATES_SHIFT = 40;

	private const int DOCK_POSITIONS_SHIFT = 48;

	private const int BRACELET_SELF_INDEX_SHIFT = 60;

	[NonSerialized]
	public bool isBraceletLeftHanded;

	[NonSerialized]
	public int braceletSelfIndex;

	[NonSerialized]
	public List<Color> braceletBeadColors = new List<Color>(10);

	[NonSerialized]
	public bool isBuilderWatchEnabled;

	private ReliableStateData Data;
}
