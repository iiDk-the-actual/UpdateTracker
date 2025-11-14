using System;
using System.Collections.Generic;
using GorillaNetworking;
using GorillaTagScripts.GhostReactor.SoakTasks;
using Photon.Pun;
using UnityEngine;

public class GhostReactorSoak
{
	public void Setup(GRPlayer grPlayer)
	{
		this.grPlayer = grPlayer;
		GhostReactorSoak.instance = this;
		if (this.IsSoaking())
		{
			Debug.LogFormat("Soak Setup {0} InRoom {1} Auth {2}", new object[]
			{
				this.state,
				this.grManager != null && this.grManager.IsAuthority(),
				PhotonNetwork.InRoom
			});
		}
		this._soakTasks.Add(new SoakTaskGrabThrow(grPlayer));
		this._soakTasks.Add(new SoakTaskDepositCollectibles(grPlayer));
		this._soakTasks.Add(new SoakTaskBreakable(grPlayer));
		this._soakTasks.Add(new SoakTaskHitEnemy(grPlayer));
	}

	public bool IsSoaking()
	{
		return false;
	}

	public void OnUpdate()
	{
		if (!this.IsSoaking())
		{
			return;
		}
		GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this.grPlayer.gamePlayer.rig.zoneEntity.currentZone);
		if (managerForZone == null)
		{
			return;
		}
		this.grManager = managerForZone.ghostReactorManager;
		if (this.grManager == null)
		{
			return;
		}
		double timeAsDouble = Time.timeAsDouble;
		switch (this.state)
		{
		case GhostReactorSoak.State.Disconnected:
			if (!PhotonNetwork.InRoom && timeAsDouble > this.reconnectTime)
			{
				this.SetState(GhostReactorSoak.State.Connecting);
				return;
			}
			break;
		case GhostReactorSoak.State.Connecting:
			if (this.grManager.IsZoneActive())
			{
				this.SetState(GhostReactorSoak.State.Active);
				return;
			}
			if (timeAsDouble > this.stateStartTime + 15.0)
			{
				this.SetState(GhostReactorSoak.State.Disconnected);
				return;
			}
			break;
		case GhostReactorSoak.State.Active:
			this.UpdateActive();
			if (timeAsDouble > this.disconnectTime)
			{
				this.SetState(GhostReactorSoak.State.Disconnected);
				return;
			}
			if (!PhotonNetwork.InRoom)
			{
				this.SetState(GhostReactorSoak.State.Disconnected);
			}
			break;
		default:
			return;
		}
	}

	private int GetActorNumber()
	{
		if (this.grPlayer.gamePlayer.rig.OwningNetPlayer == null)
		{
			return -1;
		}
		return this.grPlayer.gamePlayer.rig.OwningNetPlayer.ActorNumber;
	}

	public void SetState(GhostReactorSoak.State newState)
	{
		this.state = newState;
		this.stateStartTime = Time.timeAsDouble;
		Debug.LogFormat("Soak Set State {0} Player {1} InRoom {2} Auth {3}", new object[]
		{
			this.state,
			this.GetActorNumber(),
			this.grManager != null && this.grManager.IsAuthority(),
			PhotonNetwork.InRoom
		});
		switch (this.state)
		{
		case GhostReactorSoak.State.Disconnected:
			this.LeaveRoom();
			this.reconnectTime = this.stateStartTime + (double)Random.Range(3f, 6f);
			return;
		case GhostReactorSoak.State.Connecting:
			this.JoinRoom();
			return;
		case GhostReactorSoak.State.Active:
			this.disconnectTime = this.stateStartTime + (double)Random.Range(5f, 60f);
			return;
		default:
			return;
		}
	}

	public void JoinRoom()
	{
		Debug.LogFormat("Soak Join Room {0}", new object[] { "AKJSOAK" });
		PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("AKJSOAK", JoinType.Solo);
	}

	public void LeaveRoom()
	{
		Debug.LogFormat("Soak Leave Room", Array.Empty<object>());
		NetworkSystem.Instance.ReturnToSinglePlayer();
	}

	private void UpdateActive()
	{
		if (this._activeTask != null)
		{
			bool flag = false;
			if (!this._activeTask.Update())
			{
				Debug.LogError(string.Format("Failed to execute soak task of type {0}", this._activeTask.GetType()));
				flag = true;
			}
			if (flag || this._activeTask.Complete)
			{
				this._activeTask.Reset();
				this._activeTask = null;
				return;
			}
		}
		else if (Random.value <= 0.005f)
		{
			int num = Random.Range(0, this._soakTasks.Count);
			this._activeTask = this._soakTasks[num];
		}
	}

	public static GhostReactorSoak instance;

	private const string SOAK_ROOM = "AKJSOAK";

	private const float MIN_CONNECTED_TIME = 5f;

	private const float MAX_CONNECTED_TIME = 60f;

	private const float MIN_DISCONNECTED_TIME = 3f;

	private const float MAX_DISCONNECTED_TIME = 6f;

	public GRPlayer grPlayer;

	public GhostReactorManager grManager;

	public GhostReactorSoak.State state;

	public double stateStartTime;

	public double reconnectTime;

	public double disconnectTime;

	public const float START_NEW_TASK_ODDS = 0.005f;

	private IGhostReactorSoakTask _activeTask;

	private readonly List<IGhostReactorSoakTask> _soakTasks = new List<IGhostReactorSoakTask>();

	public enum State
	{
		Disconnected,
		Connecting,
		Active
	}
}
