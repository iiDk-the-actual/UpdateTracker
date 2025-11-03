using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class GamePressableButton : MonoBehaviour, IClickable
{
	public void Click(bool leftHand = false)
	{
		this.PressButton(leftHand);
	}

	protected void OnTriggerEnter(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.touchTime + this.debounceTime >= Time.time)
		{
			return;
		}
		GorillaTriggerColliderHandIndicator component = collider.gameObject.GetComponent<GorillaTriggerColliderHandIndicator>();
		if (!component)
		{
			return;
		}
		if (!this.CheckValidEquippedState(component.isLeftHand))
		{
			return;
		}
		this.PressButton(component.isLeftHand);
	}

	private bool CheckValidEquippedState(bool pressedHandLeft)
	{
		if (!this.requireEquipped)
		{
			return true;
		}
		int num = -1;
		GamePlayer gamePlayer;
		if (this.gameEntity.IsHeldByLocalPlayer() && this.activeWhileGrabbed && GamePlayer.TryGetGamePlayer(this.gameEntity.heldByActorNumber, out gamePlayer))
		{
			num = gamePlayer.FindHandIndex(this.gameEntity.id);
		}
		GamePlayer gamePlayer2;
		if (num == -1 && this.gameEntity.IsSnappedByLocalPlayer() && this.activeWhileSnapped && GamePlayer.TryGetGamePlayer(this.gameEntity.snappedByActorNumber, out gamePlayer2))
		{
			num = gamePlayer2.FindSnapIndex(this.gameEntity.id);
		}
		if (num == -1)
		{
			return false;
		}
		bool flag = GamePlayer.IsLeftHand(num);
		return pressedHandLeft != flag;
	}

	private void PressButton(bool isLeftHand)
	{
		this.touchTime = Time.time;
		UnityEvent unityEvent = this.onPressButton;
		if (unityEvent != null)
		{
			unityEvent.Invoke();
		}
		GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(this.pressButtonSoundIndex, isLeftHand, 0.05f);
		GorillaTagger.Instance.StartVibration(isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, isLeftHand, 0.05f });
		}
	}

	[SerializeField]
	private GameEntity gameEntity;

	[SerializeField]
	private bool requireEquipped;

	[SerializeField]
	private bool activeWhileGrabbed;

	[SerializeField]
	private bool activeWhileSnapped;

	public UnityEvent onPressButton;

	[Header("Button Press")]
	public float debounceTime = 0.25f;

	public int pressButtonSoundIndex = 67;

	private float touchTime;
}
