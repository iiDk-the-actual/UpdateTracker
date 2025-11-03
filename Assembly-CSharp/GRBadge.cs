using System;
using System.Collections;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class GRBadge : MonoBehaviour, IGameEntityComponent
{
	public void OnEntityInit()
	{
		this.gameEntity.manager.ghostReactorManager.reactor.employeeBadges.LinkBadgeToDispenser(this, (long)((int)this.gameEntity.createData));
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long nextState)
	{
	}

	private void OnDestroy()
	{
		GhostReactor ghostReactor = GhostReactor.Get(this.gameEntity);
		if (ghostReactor != null && ghostReactor.employeeBadges != null)
		{
			ghostReactor.employeeBadges.RemoveBadge(this);
		}
	}

	public void Setup(NetPlayer player, int index)
	{
		this.gameEntity.onlyGrabActorNumber = player.ActorNumber;
		this.dispenserIndex = index;
		this.actorNr = player.ActorNumber;
		GRPlayer grplayer = GRPlayer.Get(player.ActorNumber);
		bool flag = (int)this.gameEntity.GetState() == 1;
		if (player.IsLocal)
		{
			flag |= Time.timeAsDouble < grplayer.lastLeftWithBadgeAttachedTime + 60.0;
		}
		if (grplayer != null && flag)
		{
			base.transform.position = grplayer.badgeBodyAnchor.position;
			grplayer.AttachBadge(this);
		}
		this.RefreshText(player);
	}

	public void RefreshText(NetPlayer player)
	{
		this.playerName.text = player.SanitizedNickName;
		GRPlayer grplayer = GRPlayer.Get(player.ActorNumber);
		if (grplayer != null && this.lastRedeemedPoints != grplayer.CurrentProgression.redeemedPoints)
		{
			this.lastRedeemedPoints = grplayer.CurrentProgression.redeemedPoints;
			this.playerTitle.text = GhostReactorProgression.GetTitleName(grplayer.CurrentProgression.redeemedPoints);
			this.playerLevel.text = GhostReactorProgression.GetGrade(grplayer.CurrentProgression.redeemedPoints).ToString();
		}
	}

	public void Hide()
	{
		this.badgeMesh.enabled = false;
		this.playerName.gameObject.SetActive(false);
		this.playerTitle.gameObject.SetActive(false);
		this.playerLevel.gameObject.SetActive(false);
	}

	public void UnHide()
	{
		this.badgeMesh.enabled = true;
		this.playerName.gameObject.SetActive(true);
		this.playerTitle.gameObject.SetActive(true);
		this.playerLevel.gameObject.SetActive(true);
	}

	public bool IsAttachedToPlayer()
	{
		return (int)this.gameEntity.GetState() == 1;
	}

	public void StartRetracting()
	{
		this.gameEntity.RequestState(this.gameEntity.id, 1L);
		this.PlayAttachFx();
		if (this.retractCoroutine != null)
		{
			base.StopCoroutine(this.retractCoroutine);
		}
		this.retractCoroutine = base.StartCoroutine(this.RetractCoroutine());
	}

	private IEnumerator RetractCoroutine()
	{
		base.transform.localRotation = Quaternion.identity;
		Vector3 vector = base.transform.localPosition;
		for (float num = vector.sqrMagnitude; num > 1E-05f; num = vector.sqrMagnitude)
		{
			vector = Vector3.MoveTowards(vector, Vector3.zero, this.retractSpeed * Time.deltaTime);
			base.transform.localPosition = vector;
			yield return null;
			vector = base.transform.localPosition;
		}
		base.transform.localPosition = Vector3.zero;
		yield break;
	}

	private void PlayAttachFx()
	{
		if (this.audioSource != null)
		{
			this.audioSource.volume = this.badgeAttachSoundVolume;
			this.audioSource.clip = this.badgeAttachSound;
			this.audioSource.Play();
		}
	}

	private const float RESTORE_BADGE_TO_DOCK_WINDOW = 60f;

	[SerializeField]
	private GameEntity gameEntity;

	[SerializeField]
	public TMP_Text playerName;

	[SerializeField]
	public TMP_Text playerTitle;

	[SerializeField]
	public TMP_Text playerLevel;

	[SerializeField]
	private MeshRenderer badgeMesh;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private float retractSpeed = 4f;

	[SerializeField]
	private AudioClip badgeAttachSound;

	[SerializeField]
	private float badgeAttachSoundVolume;

	[SerializeField]
	public int dispenserIndex;

	public int actorNr;

	private Coroutine retractCoroutine;

	private int lastRedeemedPoints = -1;

	public enum BadgeState
	{
		AtDispenser,
		WithPlayer
	}
}
