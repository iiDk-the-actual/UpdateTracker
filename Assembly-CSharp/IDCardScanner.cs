using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class IDCardScanner : MonoBehaviour
{
	public event IDCardScanner.CardSwipeEvent OnPlayerCardSwipe;

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<ScannableIDCard>() != null)
		{
			UnityEvent unityEvent = this.onCardSwiped;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			GameEntity gameEntity = other.GetComponent<GameEntity>();
			if (gameEntity == null && other.attachedRigidbody != null)
			{
				gameEntity = other.attachedRigidbody.GetComponent<GameEntity>();
			}
			if (gameEntity != null && gameEntity.heldByActorNumber != -1)
			{
				bool flag = !this.requireSpecificPlayer || (this.restrictToPlayer != null && this.restrictToPlayer.ActorNumber == gameEntity.heldByActorNumber && gameEntity.heldByActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
				bool flag2 = !this.requireAuthority || gameEntity.manager.IsAuthority();
				if (flag && flag2)
				{
					UnityEvent<int> unityEvent2 = this.onCardSwipedByPlayer;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke(gameEntity.heldByActorNumber);
					}
					IDCardScanner.CardSwipeEvent onPlayerCardSwipe = this.OnPlayerCardSwipe;
					if (onPlayerCardSwipe == null)
					{
						return;
					}
					onPlayerCardSwipe(gameEntity.heldByActorNumber);
				}
			}
		}
	}

	public UnityEvent onCardSwiped;

	public UnityEvent<int> onCardSwipedByPlayer;

	[Tooltip("Has to be risen externally, by the receiver of the card swipe")]
	public UnityEvent onSucceeded;

	[Tooltip("Has to be risen externally, by the receiver of the card swipe")]
	public UnityEvent onFailed;

	public bool requireSpecificPlayer;

	public bool requireAuthority;

	[NonSerialized]
	public NetPlayer restrictToPlayer;

	public delegate void CardSwipeEvent(int actorNumber);
}
