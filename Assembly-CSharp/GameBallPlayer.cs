using System;
using UnityEngine;

public class GameBallPlayer : MonoBehaviour
{
	private void Awake()
	{
		this.hands = new GameBallPlayer.HandData[2];
		for (int i = 0; i < 2; i++)
		{
			this.ClearGrabbed(i);
		}
		this.teamId = -1;
	}

	public void CleanupPlayer()
	{
		MonkeBallPlayer component = base.GetComponent<MonkeBallPlayer>();
		if (component != null)
		{
			component.currGoalZone = null;
			for (int i = 0; i < MonkeBallGame.Instance.goalZones.Count; i++)
			{
				MonkeBallGame.Instance.goalZones[i].CleanupPlayer(component);
			}
		}
	}

	public void SetGrabbed(GameBallId gameBallId, int handIndex)
	{
		if (gameBallId.IsValid())
		{
			this.ClearGrabbedIfHeld(gameBallId);
		}
		GameBallPlayer.HandData handData = this.hands[handIndex];
		handData.grabbedGameBallId = gameBallId;
		this.hands[handIndex] = handData;
	}

	public void ClearGrabbedIfHeld(GameBallId gameBallId)
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.hands[i].grabbedGameBallId == gameBallId)
			{
				this.ClearGrabbed(i);
			}
		}
	}

	public void ClearGrabbed(int handIndex)
	{
		this.SetGrabbed(GameBallId.Invalid, handIndex);
	}

	public void ClearAllGrabbed()
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			this.ClearGrabbed(i);
		}
	}

	public void SetInGoalZone(bool inZone)
	{
		if (inZone)
		{
			this.inGoalZone++;
			return;
		}
		this.inGoalZone--;
	}

	public bool IsHoldingBall()
	{
		return this.GetGameBallId().IsValid();
	}

	public GameBallId GetGameBallId(int handIndex)
	{
		return this.hands[handIndex].grabbedGameBallId;
	}

	public int FindHandIndex(GameBallId gameBallId)
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			if (this.hands[i].grabbedGameBallId == gameBallId)
			{
				return i;
			}
		}
		return -1;
	}

	public GameBallId GetGameBallId()
	{
		for (int i = 0; i < this.hands.Length; i++)
		{
			if (this.hands[i].grabbedGameBallId.IsValid())
			{
				return this.hands[i].grabbedGameBallId;
			}
		}
		return GameBallId.Invalid;
	}

	public bool IsLocalPlayer()
	{
		return VRRigCache.Instance.localRig.Creator.ActorNumber == this.rig.OwningNetPlayer.ActorNumber;
	}

	public static bool IsLeftHand(int handIndex)
	{
		return handIndex == 0;
	}

	public static int GetHandIndex(bool leftHand)
	{
		if (!leftHand)
		{
			return 1;
		}
		return 0;
	}

	public static VRRig GetRig(int actorNumber)
	{
		NetPlayer player = NetworkSystem.Instance.GetPlayer(actorNumber);
		RigContainer rigContainer;
		if (player == null || player.IsNull || !VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
		{
			return null;
		}
		return rigContainer.Rig;
	}

	public static GameBallPlayer GetGamePlayer(int actorNumber)
	{
		if (actorNumber < 0)
		{
			return null;
		}
		VRRig vrrig = GameBallPlayer.GetRig(actorNumber);
		if (vrrig == null)
		{
			return null;
		}
		return vrrig.GetComponent<GameBallPlayer>();
	}

	public static GameBallPlayer GetGamePlayer(Collider collider, bool bodyOnly = false)
	{
		Transform transform = collider.transform;
		while (transform != null)
		{
			GameBallPlayer component = transform.GetComponent<GameBallPlayer>();
			if (component != null)
			{
				return component;
			}
			if (bodyOnly)
			{
				break;
			}
			transform = transform.parent;
		}
		return null;
	}

	public VRRig rig;

	public int teamId;

	private GameBallPlayer.HandData[] hands;

	public const int MAX_HANDS = 2;

	public const int LEFT_HAND = 0;

	public const int RIGHT_HAND = 1;

	private int inGoalZone;

	private struct HandData
	{
		public GameBallId grabbedGameBallId;
	}
}
