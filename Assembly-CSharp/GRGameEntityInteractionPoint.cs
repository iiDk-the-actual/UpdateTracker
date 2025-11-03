using System;
using UnityEngine;

public class GRGameEntityInteractionPoint : MonoBehaviour
{
	public void Start()
	{
		base.transform.parent = this.targetParent;
	}

	public void OnEnable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Combine(gameEntity2.OnReleased, new Action(this.OnReleased));
	}

	public void OnDisable()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Remove(gameEntity.OnGrabbed, new Action(this.OnGrabbed));
		GameEntity gameEntity2 = this.gameEntity;
		gameEntity2.OnReleased = (Action)Delegate.Remove(gameEntity2.OnReleased, new Action(this.OnReleased));
	}

	public void OnGrabbed()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnTick = (Action)Delegate.Combine(gameEntity.OnTick, new Action(this.TickWhileHeld));
		Action onGrabStart = this.OnGrabStart;
		if (onGrabStart == null)
		{
			return;
		}
		onGrabStart();
	}

	public void OnReleased()
	{
		GameEntity gameEntity = this.gameEntity;
		gameEntity.OnTick = (Action)Delegate.Remove(gameEntity.OnTick, new Action(this.TickWhileHeld));
		this.gameEntity.transform.parent = this.targetParent;
		this.gameEntity.transform.localRotation = Quaternion.identity;
		this.gameEntity.transform.localPosition = Vector3.zero;
		this.OnGrabEnd();
	}

	public void TickWhileHeld()
	{
		if (this.targetParent != null)
		{
			Vector3 position = this.targetParent.transform.position;
			Vector3 position2 = base.transform.position;
			if (Vector3.Magnitude(position - position2) > this.autoReleaseDistance)
			{
				GamePlayer gamePlayer = GamePlayer.GetGamePlayer(this.gameEntity.heldByActorNumber);
				if (gamePlayer != null)
				{
					gamePlayer.ClearGrabbedIfHeld(this.gameEntity.id);
				}
				if (gamePlayer != null && GamePlayerLocal.instance.gamePlayer == gamePlayer)
				{
					GamePlayerLocal.instance.ClearGrabbedIfHeld(this.gameEntity.id);
				}
				this.OnReleased();
				return;
			}
		}
		Action onGrabContinue = this.OnGrabContinue;
		if (onGrabContinue == null)
		{
			return;
		}
		onGrabContinue();
	}

	public GameEntity gameEntity;

	public float autoReleaseDistance = 0.1f;

	public Action OnGrabStart;

	public Action OnGrabContinue;

	public Action OnGrabEnd;

	public Transform targetParent;
}
