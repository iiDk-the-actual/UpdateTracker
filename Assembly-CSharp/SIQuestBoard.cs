using System;
using System.Collections.Generic;
using GorillaLocomotion;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class SIQuestBoard : MonoBehaviour, IGorillaSliceableSimple
{
	public void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < this.questDisplays.Count; i++)
		{
			stream.SendNext(this.questDisplays[i].activePlayerActorNumber);
		}
	}

	public void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < this.questDisplays.Count; i++)
		{
			this.questDisplays[i].activePlayerActorNumber = (int)stream.ReceiveNext();
		}
	}

	public void GrantBonusPointProgress()
	{
		if (!this.bounds.Contains(GTPlayer.Instance.HeadCenterPosition))
		{
			return;
		}
		SIPlayer.LocalPlayer.GetBonusProgress(this.superInfection.siManager);
	}

	void IGorillaSliceableSimple.SliceUpdate()
	{
		if (this.superInfection.siManager.gameEntityManager.IsAuthority())
		{
			this.AuthorityUpdateScreenAssignments();
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime dateTime = utcNow.Date + SIProgression.Instance.CROSSOVER_TIME_OF_DAY;
		if (dateTime < utcNow)
		{
			dateTime = dateTime.AddDays(1.0);
		}
		TimeSpan timeSpan = dateTime - utcNow;
		if (this.lastHours != timeSpan.Hours || this.lastMinutes != timeSpan.Minutes || this.lastSeconds != timeSpan.Seconds)
		{
			this.timeToNewQuests.text = "NEW QUESTS IN: " + timeSpan.ToString("hh\\:mm\\:ss");
		}
		this.lastHours = timeSpan.Hours;
		this.lastMinutes = timeSpan.Minutes;
		this.lastSeconds = timeSpan.Seconds;
	}

	private void AuthorityUpdateScreenAssignments()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		NetPlayer[] allNetPlayers = NetworkSystem.Instance.AllNetPlayers;
		for (int i = 0; i < allNetPlayers.Length; i++)
		{
			list.Add(allNetPlayers[i].ActorNumber);
		}
		for (int j = 0; j < this.questDisplays.Count; j++)
		{
			int activePlayerActorNumber = this.questDisplays[j].activePlayerActorNumber;
			if (activePlayerActorNumber != -1)
			{
				if (!list.Contains(activePlayerActorNumber))
				{
					this.questDisplays[j].activePlayerActorNumber = -1;
				}
				else if (!list2.Contains(activePlayerActorNumber))
				{
					list2.Add(activePlayerActorNumber);
				}
			}
		}
		for (int k = 0; k < allNetPlayers.Length; k++)
		{
			int actorNumber = allNetPlayers[k].ActorNumber;
			if (!list2.Contains(actorNumber))
			{
				for (int l = 0; l < this.questDisplays.Count; l++)
				{
					if (this.questDisplays[l].activePlayerActorNumber == -1)
					{
						this.questDisplays[l].activePlayerActorNumber = actorNumber;
						break;
					}
				}
			}
		}
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (this.bonusPointArea.gameObject.activeSelf)
		{
			this.bounds = this.bonusPointArea.bounds;
			this.bonusPointArea.gameObject.SetActive(false);
		}
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void ForceCompleteQuest(int index)
	{
	}

	public void CheatAddPoints(int points)
	{
	}

	public void CheatAddBonusPoints(int points)
	{
	}

	public SuperInfection superInfection;

	public List<SIUIPlayerQuestDisplay> questDisplays;

	public BoxCollider bonusPointArea;

	public Bounds bounds;

	public ParticleSystem celebrateParticle;

	public TextMeshProUGUI timeToNewQuests;

	private int lastHours;

	private int lastMinutes;

	private int lastSeconds;
}
