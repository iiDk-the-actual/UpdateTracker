using System;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;

public class DebugHudStats : MonoBehaviour
{
	public static DebugHudStats Instance
	{
		get
		{
			return DebugHudStats._instance;
		}
	}

	private void Awake()
	{
		if (DebugHudStats._instance != null && DebugHudStats._instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			DebugHudStats._instance = this;
		}
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (DebugHudStats._instance == this)
		{
			DebugHudStats._instance = null;
			if (this.drawCallsRecorder.Valid)
			{
				this.drawCallsRecorder.Dispose();
			}
			if (this.trisRecorder.Valid)
			{
				this.trisRecorder.Dispose();
			}
		}
	}

	private void Update()
	{
		bool flag = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
		if (flag != this.buttonDown)
		{
			this.buttonDown = flag;
			if (!this.buttonDown)
			{
				switch (this.currentState)
				{
				case DebugHudStats.State.ShowStats:
					PlayerGameEvents.OnPlayerMoved += this.OnPlayerMoved;
					PlayerGameEvents.OnPlayerSwam += this.OnPlayerSwam;
					break;
				}
				switch (this.currentState)
				{
				case DebugHudStats.State.Inactive:
					this.currentState = DebugHudStats.State.Active;
					this.text.gameObject.SetActive(true);
					break;
				case DebugHudStats.State.Active:
					this.currentState = DebugHudStats.State.ShowLog;
					break;
				case DebugHudStats.State.ShowLog:
					this.currentState = DebugHudStats.State.ShowStats;
					this.distanceMoved = (this.distanceSwam = 0f);
					PlayerGameEvents.OnPlayerMoved += this.OnPlayerMoved;
					PlayerGameEvents.OnPlayerSwam += this.OnPlayerSwam;
					break;
				case DebugHudStats.State.ShowStats:
					this.currentState = DebugHudStats.State.Inactive;
					this.text.gameObject.SetActive(false);
					break;
				}
				if (RigidbodyHighlighter.Instance != null)
				{
					RigidbodyHighlighter.Instance.Active = this.currentState > DebugHudStats.State.Inactive;
				}
			}
		}
		if (this.firstAwake == 0f)
		{
			this.firstAwake = Time.time;
		}
		if (this.updateTimer < this.delayUpdateRate)
		{
			this.updateTimer += Time.deltaTime;
			return;
		}
		int num = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
		if (num < 89)
		{
			this.lowFps++;
		}
		else
		{
			this.lowFps = 0;
		}
		this.fpsWarning.gameObject.SetActive(this.lowFps > 5 && this.currentState == DebugHudStats.State.Inactive);
		if (this.currentState != DebugHudStats.State.Inactive)
		{
			this.builder.Clear();
			this.builder.Append("v: ");
			this.builder.Append(GorillaComputer.instance.version);
			this.builder.Append(":");
			this.builder.Append(GorillaComputer.instance.buildCode);
			num = Mathf.Min(num, 90);
			this.builder.Append((num < 89) ? " - <color=\"red\">" : " - <color=\"white\">");
			this.builder.Append(num);
			this.builder.AppendLine(" fps</color>");
			this.builder.AppendLine(string.Format("draw calls: {0} tris: {1}", this.drawCallsRecorder.LastValue, this.trisRecorder.LastValue));
			if (GorillaComputer.instance != null)
			{
				this.builder.AppendLine(GorillaComputer.instance.GetServerTime().ToString());
			}
			else
			{
				this.builder.AppendLine("Server Time Unavailable");
			}
			this.zones = GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone.ToString();
			if (NetworkSystem.Instance.IsMasterClient)
			{
				this.builder.Append("H");
			}
			if (NetworkSystem.Instance.InRoom)
			{
				if (NetworkSystem.Instance.SessionIsPrivate)
				{
					this.builder.Append("Pri ");
				}
				else
				{
					this.builder.Append("Pub ");
				}
			}
			else
			{
				this.builder.Append("DC ");
			}
			this.builder.Append("Z: <color=\"orange\">");
			this.builder.Append(this.zones);
			this.builder.AppendLine("</color>");
			if (NetworkSystem.Instance.InRoom)
			{
				GorillaGameManager instance = GorillaGameManager.instance;
				if (instance != null)
				{
					GorillaTagCompetitiveManager gorillaTagCompetitiveManager = instance as GorillaTagCompetitiveManager;
					if (gorillaTagCompetitiveManager != null)
					{
						this.builder.Append("Ranked Mode ELO: ");
						this.builder.Append(gorillaTagCompetitiveManager.GetScoring().Progression.GetEloScore().ToString());
						this.builder.Append("  Tier: ");
						this.builder.AppendLine(gorillaTagCompetitiveManager.GetScoring().Progression.GetRankedProgressionTierName());
						RankedMultiplayerScore.PlayerScoreInRound inGameScoreForSelf = gorillaTagCompetitiveManager.GetScoring().GetInGameScoreForSelf();
						this.builder.Append("Tags: ");
						this.builder.Append(inGameScoreForSelf.NumTags.ToString());
						this.builder.Append("  Defense: ");
						this.builder.Append(Mathf.RoundToInt(inGameScoreForSelf.PointsOnDefense).ToString());
						this.builder.Append("  Score: ");
						this.builder.AppendLine(Mathf.RoundToInt(gorillaTagCompetitiveManager.GetScoring().ComputeGameScore(inGameScoreForSelf.NumTags, inGameScoreForSelf.PointsOnDefense)).ToString());
						if (gorillaTagCompetitiveManager.ShowDebugPing)
						{
							this.builder.AppendLine("Server MatchID Ping!");
						}
					}
				}
			}
			if (this.currentState == DebugHudStats.State.ShowStats)
			{
				this.builder.AppendLine();
				Vector3 vector = GTPlayer.Instance.AveragedVelocity;
				Vector3 headCenterPosition = GTPlayer.Instance.HeadCenterPosition;
				float magnitude = vector.magnitude;
				this.groundVelocity = vector;
				this.groundVelocity.y = 0f;
				this.builder.AppendLine(string.Format("v: {0:F1} m/s", magnitude));
				this.builder.AppendLine(string.Format("ground: {0:F1} m/s", this.groundVelocity.magnitude));
				this.builder.AppendLine(string.Format("head: {0:F2}\n", headCenterPosition));
				this.builder.AppendLine(string.Format("odo: {0:F2}m", this.distanceMoved));
				this.builder.AppendLine(string.Format("swam: {0:F2}m", this.distanceSwam));
			}
			else if (this.currentState == DebugHudStats.State.ShowLog)
			{
				this.builder.AppendLine();
				for (int i = 0; i < this.logMessages.Count; i++)
				{
					this.builder.AppendLine(this.logMessages[i]);
				}
			}
			this.text.text = this.builder.ToString();
		}
		this.updateTimer = 0f;
	}

	private void OnPlayerSwam(float distance, float speed)
	{
		if (distance > 0.005f)
		{
			this.distanceSwam += distance;
		}
	}

	private void OnPlayerMoved(float distance, float speed)
	{
		if (distance > 0.005f)
		{
			this.distanceMoved += distance;
		}
	}

	private void OnEnable()
	{
		Application.logMessageReceived += this.LogMessageReceived;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= this.LogMessageReceived;
	}

	private void LogMessageReceived(string condition, string stackTrace, LogType type)
	{
		this.logMessages.Add(this.getColorStringFromLogType(type) + condition + "</color>");
		if (this.logMessages.Count > 6)
		{
			this.logMessages.RemoveAt(0);
		}
	}

	private string getColorStringFromLogType(LogType type)
	{
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			return "<color=\"red\">";
		case LogType.Warning:
			return "<color=\"yellow\">";
		}
		return "<color=\"white\">";
	}

	private void OnZoneChanged(ZoneData[] zoneData)
	{
		this.zones = string.Empty;
		for (int i = 0; i < zoneData.Length; i++)
		{
			if (zoneData[i].active)
			{
				this.zones = this.zones + zoneData[i].zone.ToString().ToUpper() + "; ";
			}
		}
	}

	private const int FPS_THRESHOLD = 89;

	private static DebugHudStats _instance;

	[SerializeField]
	public TMP_Text text;

	[SerializeField]
	private TMP_Text fpsWarning;

	[SerializeField]
	private float delayUpdateRate = 0.25f;

	private float updateTimer;

	public float sessionAnytrackingLost;

	public float last30SecondsTrackingLost;

	private float firstAwake;

	private bool leftHandTracked;

	private bool rightHandTracked;

	private StringBuilder builder;

	private Vector3 averagedVelocity;

	private Vector3 groundVelocity;

	private Vector3 centerHeadPos;

	private float distanceMoved;

	private float distanceSwam;

	private List<string> logMessages = new List<string>();

	private bool buttonDown;

	private bool showLog;

	private int lowFps;

	private string zones;

	private GroupJoinZoneAB lastGroupJoinZone;

	private DebugHudStats.State currentState = DebugHudStats.State.Active;

	private ProfilerRecorder drawCallsRecorder;

	private ProfilerRecorder trisRecorder;

	private enum State
	{
		Inactive,
		Active,
		ShowLog,
		ShowStats
	}
}
