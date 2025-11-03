using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

public class ThrownGadget : MonoBehaviour
{
	public event Action OnActivated;

	public event Action OnThrown;

	public event Action OnHitSurface;

	private void OnEnable()
	{
		this.isHeldLocal = false;
		this.lastThrowerLocal = false;
	}

	public bool IsHeld()
	{
		return this.gameEntity.heldByActorNumber != -1;
	}

	public bool IsHeldLocal()
	{
		return this.gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
	}

	public bool IsHeldByAnother()
	{
		return this.IsHeld() && !this.IsHeldLocal();
	}

	private bool IsButtonHeld()
	{
		if (!this.IsHeldLocal())
		{
			return false;
		}
		GamePlayer gamePlayer;
		if (!GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			return false;
		}
		if (gamePlayer == null)
		{
			return false;
		}
		int num = gamePlayer.FindHandIndex(this.gameEntity.id);
		return num != -1 && ControllerInputPoller.TriggerFloat(GamePlayer.IsLeftHand(num) ? XRNode.LeftHand : XRNode.RightHand) > 0.25f;
	}

	public void Update()
	{
		bool flag = this.IsHeldLocal();
		if (flag)
		{
			this.lastThrowerLocal = true;
			this.UpdateActivation();
		}
		else if (this.isHeldLocal)
		{
			Action onThrown = this.OnThrown;
			if (onThrown != null)
			{
				onThrown();
			}
		}
		else if (this.IsHeldByAnother())
		{
			this.lastThrowerLocal = false;
		}
		this.isHeldLocal = flag;
	}

	private void UpdateActivation()
	{
		bool flag = this.IsButtonHeld();
		if (!this.activationButtonLastInput && flag)
		{
			Action onActivated = this.OnActivated;
			if (onActivated != null)
			{
				onActivated();
			}
		}
		this.activationButtonLastInput = flag;
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (this.lastThrowerLocal)
		{
			Action onHitSurface = this.OnHitSurface;
			if (onHitSurface == null)
			{
				return;
			}
			onHitSurface();
		}
	}

	public GameEntity gameEntity;

	private bool isHeldLocal;

	private bool lastThrowerLocal;

	private bool activationButtonLastInput;
}
