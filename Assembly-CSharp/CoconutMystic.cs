using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CoconutMystic : MonoBehaviour
{
	private void Awake()
	{
		this.rig = base.GetComponentInParent<VRRig>();
	}

	private void OnEnable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += this.OnPhotonEvent;
	}

	private void OnDisable()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= this.OnPhotonEvent;
	}

	private void OnPhotonEvent(EventData evData)
	{
		if (evData.Code != 176)
		{
			return;
		}
		object[] array = (object[])evData.CustomData;
		object obj = array[0];
		if (!(obj is int))
		{
			return;
		}
		int num = (int)obj;
		if (num != CoconutMystic.kUpdateLabelEvent)
		{
			return;
		}
		NetPlayer player = NetworkSystem.Instance.GetPlayer(evData.Sender);
		NetPlayer owningNetPlayer = this.rig.OwningNetPlayer;
		if (player != owningNetPlayer)
		{
			return;
		}
		int num2 = (int)array[1];
		this.label.text = this.answers.GetItem(num2).GetLocalizedString();
		this.soundPlayer.Play();
		this.breakEffect.Play();
	}

	public void UpdateLabel()
	{
		bool flag = this.geodeItem.currentState == TransferrableObject.PositionState.InLeftHand;
		this.label.rectTransform.localRotation = Quaternion.Euler(0f, flag ? 270f : 90f, 0f);
	}

	public void ShowAnswer()
	{
		this.answers.distinct = this.distinct;
		this.label.text = this.answers.NextItem().GetLocalizedString();
		this.soundPlayer.Play();
		this.breakEffect.Play();
		object obj = new object[]
		{
			CoconutMystic.kUpdateLabelEvent,
			this.answers.lastItemIndex
		};
		PhotonNetwork.RaiseEvent(176, obj, RaiseEventOptions.Default, SendOptions.SendReliable);
	}

	public VRRig rig;

	public GeodeItem geodeItem;

	public SoundBankPlayer soundPlayer;

	public ParticleSystem breakEffect;

	public RandomLocalizedStrings answers;

	public TMP_Text label;

	public bool distinct;

	private static readonly int kUpdateLabelEvent = "CoconutMystic.UpdateLabel".GetStaticHash();
}
