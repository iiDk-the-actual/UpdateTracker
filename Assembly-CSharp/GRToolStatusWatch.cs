using System;
using System.Text;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GRToolStatusWatch : MonoBehaviour, IGameEntityComponent
{
	public void OnEntityInit()
	{
		if (this.gameEntity == null)
		{
			this.gameEntity = base.GetComponent<GameEntity>();
		}
		this.UpdateVisuals();
		this.progression = this.gameEntity.manager.GetComponent<GhostReactorManager>().reactor.toolProgression;
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnSnapped = (Action)Delegate.Combine(gameEntity.OnSnapped, new Action(this.UpdateSnappedPlayer));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnUnsnapped = (Action)Delegate.Combine(gameEntity2.OnUnsnapped, new Action(this.RemoveSnappedPlayer));
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
	}

	public void UpdateSnappedPlayer()
	{
		this.currentPlayer = GRPlayer.Get(this.gameEntity.snappedByActorNumber);
		this.lastKills = -1;
		this.lastCredits = -1;
		this.lastJuice = -1;
		this.lastGrade = -1;
		if (this.currentPlayer == GRPlayer.GetLocal())
		{
			this.state = GRToolStatusWatch.WatchState.SnappedLocal;
		}
		else
		{
			this.state = GRToolStatusWatch.WatchState.SnappedRemote;
		}
		this.disabledText.text = "LEAVE ME ALONE!\n\nTHIS IS ONLY FOR MY OWNER!!!";
		this.UpdateVisuals();
	}

	public void RemoveSnappedPlayer()
	{
		this.currentPlayer = null;
		this.state = GRToolStatusWatch.WatchState.Dropped;
		this.disabledText.text = "LOW POWER\n\nPUT ME ON";
		this.UpdateVisuals();
	}

	private void Update()
	{
		if (this.currentPlayer == null)
		{
			return;
		}
		this.UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		bool flag = this.state == GRToolStatusWatch.WatchState.SnappedLocal || this.state == GRToolStatusWatch.WatchState.SnappedRemote;
		if (this.disabledVisuals.activeSelf == flag)
		{
			this.disabledVisuals.SetActive(!flag);
		}
		if (this.enabledVisuals.activeSelf != flag)
		{
			this.enabledVisuals.SetActive(flag);
		}
		if (this.state != GRToolStatusWatch.WatchState.SnappedLocal)
		{
			return;
		}
		if (this.visibleHP != this.currentPlayer.Hp / 100)
		{
			this.visibleHP = this.currentPlayer.Hp / 100;
			for (int i = 0; i < this.healthHearts.Length; i++)
			{
				if (this.healthHearts[i].activeSelf != i < this.visibleHP)
				{
					this.healthHearts[i].SetActive(i < this.visibleHP);
				}
			}
		}
		if (this.visibleShield != this.currentPlayer.ShieldHp / 100)
		{
			this.visibleShield = this.currentPlayer.ShieldHp / 100;
			if (this.shieldSymbol.activeSelf != this.visibleShield > 0)
			{
				this.shieldSymbol.SetActive(this.visibleShield > 0);
			}
		}
		this.gimbaledCompass.LookAt(this.homeBase, Vector3.up);
		int num = (int)this.currentPlayer.synchronizedSessionStats[5];
		int shiftCredits = this.currentPlayer.ShiftCredits;
		int numberOfResearchPoints = this.progression.GetNumberOfResearchPoints();
		ValueTuple<int, int, int, int> gradePointDetails = GhostReactorProgression.GetGradePointDetails(this.currentPlayer.CurrentProgression.redeemedPoints);
		int item = gradePointDetails.Item1;
		int item2 = gradePointDetails.Item2;
		if (num == this.lastKills && shiftCredits == this.lastCredits && numberOfResearchPoints == this.lastJuice && item2 == this.lastGrade)
		{
			return;
		}
		this.sb.Clear();
		this.sb.Append(num);
		this.sb.Append("\n\n");
		this.sb.Append(numberOfResearchPoints);
		this.sb.Append("\n\n");
		this.sb.Append(shiftCredits);
		this.sb.Append("\n\n\n");
		this.sb.Append(GhostReactorProgression.GetTitleNameFromLevel(item)[0]);
		this.sb.Append(item2);
		this.statsText.text = this.sb.ToString();
		this.lastKills = num;
		this.lastCredits = shiftCredits;
		this.lastJuice = numberOfResearchPoints;
		this.lastGrade = item2;
	}

	public GameEntity gameEntity;

	private GRPlayer currentPlayer;

	private int visibleHP;

	private int visibleShield;

	public GameObject disabledVisuals;

	public GameObject enabledVisuals;

	public GameObject[] healthHearts;

	public GameObject shieldSymbol;

	public Vector3 homeBase;

	public Transform gimbaledCompass;

	public TextMeshPro statsText;

	public TextMeshPro disabledText;

	private int lastKills;

	private int lastCredits;

	private int lastJuice;

	private int lastGrade;

	private StringBuilder sb = new StringBuilder();

	private GRToolStatusWatch.WatchState state;

	private GRToolProgressionManager progression;

	private enum WatchState
	{
		Dropped,
		SnappedLocal,
		SnappedRemote
	}
}
