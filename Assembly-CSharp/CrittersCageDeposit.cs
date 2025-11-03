using System;
using System.Collections;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

public class CrittersCageDeposit : CrittersActorDeposit
{
	public event Action<Menagerie.CritterData, int> OnDepositCritter;

	private void Awake()
	{
		this.attachPoint.OnGrabbedChild += this.StartProcessCage;
	}

	protected override bool CanDeposit(CrittersActor depositActor)
	{
		return base.CanDeposit(depositActor) && !this.isHandlingDeposit;
	}

	private void StartProcessCage(CrittersActor depositedActor)
	{
		this.currentCage = depositedActor;
		base.StartCoroutine(this.ProcessCage());
	}

	private IEnumerator ProcessCage()
	{
		this.isHandlingDeposit = true;
		bool isLocalDeposit = this.currentCage.lastGrabbedPlayer == PhotonNetwork.LocalPlayer.ActorNumber;
		this.depositAudio.GTPlayOneShot(this.depositStartSound, isLocalDeposit ? 1f : 0.5f);
		float transition = 0f;
		CrittersPawn crittersPawn = this.currentCage.GetComponentInChildren<CrittersPawn>();
		int lastGrabbedPlayer = this.currentCage.lastGrabbedPlayer;
		Menagerie.CritterData critterData;
		if (crittersPawn.IsNotNull())
		{
			critterData = new Menagerie.CritterData(crittersPawn.visuals);
		}
		else
		{
			critterData = new Menagerie.CritterData();
		}
		while (transition < this.submitDuration)
		{
			transition += Time.deltaTime;
			this.attachPoint.transform.localPosition = Vector3.Lerp(this.depositStartLocation, this.depositEndLocation, Mathf.Min(transition / this.submitDuration, 1f));
			yield return null;
		}
		if (crittersPawn.IsNotNull())
		{
			Action<Menagerie.CritterData, int> onDepositCritter = this.OnDepositCritter;
			if (onDepositCritter != null)
			{
				onDepositCritter(critterData, lastGrabbedPlayer);
			}
			CrittersActor crittersActor = crittersPawn;
			bool flag = false;
			Vector3 zero = Vector3.zero;
			crittersActor.Released(flag, default(Quaternion), zero, default(Vector3), default(Vector3));
			crittersPawn.gameObject.SetActive(false);
			this.depositAudio.GTPlayOneShot(this.depositCritterSound, isLocalDeposit ? 1f : 0.5f);
		}
		else
		{
			this.depositAudio.GTPlayOneShot(this.depositEmptySound, isLocalDeposit ? 1f : 0.5f);
		}
		this.currentCage.transform.position = Vector3.zero;
		this.currentCage.gameObject.SetActive(false);
		this.currentCage = null;
		transition = 0f;
		while (transition < this.returnDuration)
		{
			transition += Time.deltaTime;
			this.attachPoint.transform.localPosition = Vector3.Lerp(this.depositEndLocation, this.depositStartLocation, Mathf.Min(transition / this.returnDuration, 1f));
			yield return null;
		}
		this.isHandlingDeposit = false;
		yield break;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.TransformPoint(this.depositStartLocation), 0.1f);
		Gizmos.DrawLine(base.transform.TransformPoint(this.depositStartLocation), base.transform.TransformPoint(this.depositEndLocation));
		Gizmos.DrawWireSphere(base.transform.TransformPoint(this.depositEndLocation), 0.1f);
	}

	private bool isHandlingDeposit;

	public Vector3 depositStartLocation;

	public Vector3 depositEndLocation;

	public float submitDuration = 0.5f;

	public float returnDuration = 1f;

	public AudioSource depositAudio;

	public AudioClip depositStartSound;

	public AudioClip depositEmptySound;

	public AudioClip depositCritterSound;

	private CrittersActor currentCage;
}
