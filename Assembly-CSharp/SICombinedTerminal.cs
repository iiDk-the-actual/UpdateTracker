using System;
using System.Collections.Generic;
using System.IO;
using GorillaTag;
using Photon.Pun;
using UnityEngine;

public class SICombinedTerminal : MonoBehaviour, IGorillaSliceableSimple
{
	public bool IsAuthority
	{
		get
		{
			return this.superInfection.siManager.gameEntityManager.IsAuthority();
		}
	}

	public SuperInfectionManager SIManager
	{
		get
		{
			return this.superInfection.siManager;
		}
	}

	public int ActivePage
	{
		get
		{
			return this._activePage;
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void SliceUpdate()
	{
		this.wasOccupied = this.isOccupied;
		this.isOccupied = false;
		this.isOccupiedByActivePlayer = false;
		VRRigCache.Instance.GetActiveRigs(this.rigs);
		for (int i = 0; i < this.rigs.Count; i++)
		{
			if (this.activeUserBounds.bounds.Contains(this.rigs[i].transform.position))
			{
				this.isOccupied = true;
				if (this.rigs[i].OwningNetPlayer.IsLocal)
				{
					this.isOccupiedByActivePlayer = true;
					break;
				}
			}
		}
		if (this.isOccupied)
		{
			float num = Time.time - SIProgression.Instance.timeTelemetryLastChecked;
			if (this.activePlayer != null && this.activePlayer.ActorNr == SIPlayer.LocalPlayer.ActorNr && this.isOccupiedByLocalPlayer)
			{
				SIProgression.Instance.activeTerminalTimeInterval += num;
				SIProgression.Instance.activeTerminalTimeTotal += num;
			}
			if (!this.wasOccupied && this.state == EKioskAnimState.Closing)
			{
				this.AnimQueueState(EKioskAnimState.Opening);
			}
			this.foldupTimeStart = Time.time;
			return;
		}
		if (this.state == EKioskAnimState.Opening && Time.time > this.foldupTimeStart + this.foldupDelay && !this.isOccupied)
		{
			this.AnimQueueState(EKioskAnimState.Closing);
		}
	}

	public void Reset()
	{
		this.SetActivePage(0);
		this.dispenser.Initialize();
		this.techTree.Initialize();
		this.resourceCollection.Initialize();
		this.dispenser.Reset();
		this.techTree.Reset();
		this.resourceCollection.Reset();
		this.AnimQueueState(EKioskAnimState.Closing);
	}

	public void Awake()
	{
		if (this.superInfection == null)
		{
			this.superInfection = base.GetComponentInParent<SuperInfection>();
		}
		this.dispenser.Initialize();
		this.techTree.Initialize();
		this.resourceCollection.Initialize();
		this.Reset();
	}

	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (this.activePlayer != null)
		{
			stream.SendNext(this.activePlayer.ActorNr);
		}
		else
		{
			stream.SendNext(-1);
		}
		stream.SendNext(this._activePage);
		this.dispenser.WriteDataPUN(stream, info);
		this.techTree.WriteDataPUN(stream, info);
		this.resourceCollection.WriteDataPUN(stream, info);
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		this.activePlayer = SIPlayer.Get((int)stream.ReceiveNext());
		this._activePage = (int)stream.ReceiveNext();
		this.dispenser.ReadDataPUN(stream, info);
		this.techTree.ReadDataPUN(stream, info);
		this.resourceCollection.ReadDataPUN(stream, info);
	}

	public void SerializeZoneData(BinaryWriter writer)
	{
		writer.Write(this._activePage);
		this.dispenser.ZoneDataSerializeWrite(writer);
		this.techTree.ZoneDataSerializeWrite(writer);
		this.resourceCollection.ZoneDataSerializeWrite(writer);
	}

	public void DeserializeZoneData(BinaryReader reader)
	{
		this._activePage = reader.ReadInt32();
		this.SetActivePage(this._activePage);
		this.dispenser.ZoneDataSerializeRead(reader);
		this.techTree.ZoneDataSerializeRead(reader);
		this.resourceCollection.ZoneDataSerializeRead(reader);
	}

	public void PlayerHandScanned(int actorNr)
	{
		if (!this.IsAuthority)
		{
			this.superInfection.siManager.CallRPC(SuperInfectionManager.ClientToAuthorityRPC.CombinedTerminalHandScan, new object[] { this.index });
			return;
		}
		SIPlayer siplayer = SIPlayer.Get(actorNr);
		if (this.activePlayer != null && this.activePlayer.isActiveAndEnabled && siplayer != this.activePlayer && this.activeUserBounds.bounds.Contains(this.activePlayer.transform.position))
		{
			return;
		}
		this.activePlayer = siplayer;
		this.dispenser.PlayerHandScanned(actorNr);
		this.techTree.PlayerHandScanned(actorNr);
		this.resourceCollection.PlayerHandScanned(actorNr);
	}

	public void TouchscreenButtonPressed(SITouchscreenButton.SITouchscreenButtonType buttonType, int data, int actorNr, SICombinedTerminal.TerminalSubFunction subFunction)
	{
		if (!this.IsAuthority)
		{
			this.SIManager.CallRPC(SuperInfectionManager.ClientToAuthorityRPC.CombinedTerminalButtonPress, new object[]
			{
				(int)buttonType,
				data,
				(int)subFunction,
				this.index
			});
			return;
		}
		switch (subFunction)
		{
		case SICombinedTerminal.TerminalSubFunction.TechTree:
			this.techTree.TouchscreenButtonPressed(buttonType, data, actorNr);
			return;
		case SICombinedTerminal.TerminalSubFunction.GadgetDispenser:
			this.dispenser.TouchscreenButtonPressed(buttonType, data, actorNr);
			return;
		case SICombinedTerminal.TerminalSubFunction.ResourceCollection:
			this.resourceCollection.TouchscreenButtonPressed(buttonType, data, actorNr);
			return;
		default:
			return;
		}
	}

	public void SetActivePage(int pageId)
	{
		this._activePage = pageId;
		if (this.techTree.IsValidPage(pageId))
		{
			this.techTree.SetActivePage();
		}
		if (this.dispenser.IsValidPage(pageId))
		{
			this.dispenser.SetActivePage();
		}
	}

	private void AnimQueueState(EKioskAnimState newState)
	{
		this.state = newState;
		for (int i = 0; i < this.m_gtAnimators.Length; i++)
		{
			if (!(this.m_gtAnimators[i] == null))
			{
				this.m_gtAnimators[i].QueueState((long)newState);
			}
		}
	}

	[DebugReadout]
	internal int index;

	[DebugReadout]
	internal SIPlayer activePlayer;

	[DebugReadout]
	internal bool isOccupiedByActivePlayer;

	[DebugReadout]
	internal bool isOccupiedByLocalPlayer;

	[DebugReadout]
	internal bool isOccupied;

	[DebugReadout]
	internal bool wasOccupied;

	[DebugReadout]
	internal SuperInfection superInfection;

	public SIGadgetDispenser dispenser;

	public SITechTreeStation techTree;

	public SIResourceCollection resourceCollection;

	[SerializeField]
	private GTAnimator[] m_gtAnimators;

	public Collider activeUserBounds;

	public float foldupDelay = 20f;

	private float foldupTimeStart;

	private EKioskAnimState state;

	[DebugReadout]
	private int _activePage;

	[Header("Flattener")]
	public Transform zeroZeroImage;

	public Transform onePointTwoText;

	private List<VRRig> rigs = new List<VRRig>();

	public enum TerminalSubFunction
	{
		TechTree,
		GadgetDispenser,
		ResourceCollection
	}
}
