using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SIResource : MonoBehaviour
{
	private void Awake()
	{
		if (this.myGameEntity == null)
		{
			this.myGameEntity = base.GetComponent<GameEntity>();
		}
		if (this.myGameEntity == null)
		{
			Debug.LogError("missing gameentity reference! bad!", base.gameObject);
			return;
		}
		GameEntity gameEntity = this.myGameEntity;
		gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, new Action(this.SetLastGrabbed));
		this._rb = base.GetComponent<Rigidbody>();
		this.myGameEntity.onEntityDestroyed += this.HandleOnDestroyed;
	}

	public void Update()
	{
		if (this.isSleeping || !this.shouldSleep)
		{
			return;
		}
		if (Time.time < this.timeReleased + this.sleepTime)
		{
			return;
		}
		this._rb.isKinematic = true;
		this.isSleeping = true;
	}

	public void SetLastGrabbed()
	{
		this.lastPlayerHeld = SIPlayer.Get(this.myGameEntity.lastHeldByActorNumber);
		if (this.lastPlayerHeld == SIPlayer.LocalPlayer)
		{
			this.localEverGrabbed = true;
		}
	}

	protected virtual void OnEnable()
	{
		GameEntity gameEntity = this.myGameEntity;
		gameEntity.OnSnapped = (Action)Delegate.Combine(gameEntity.OnSnapped, new Action(this.GrabInitialization));
		GameEntity gameEntity2 = this.myGameEntity;
		gameEntity2.OnGrabbed = (Action)Delegate.Combine(gameEntity2.OnGrabbed, new Action(this.GrabInitialization));
		GameEntity gameEntity3 = this.myGameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Combine(gameEntity3.OnReleased, new Action(this.ReleaseInitialization));
		GameEntity gameEntity4 = this.myGameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Combine(gameEntity4.OnUnsnapped, new Action(this.ReleaseInitialization));
		this.timeReleased = Time.time;
		this._rb.isKinematic = true;
	}

	private void OnDisable()
	{
		GameEntity gameEntity = this.myGameEntity;
		gameEntity.OnSnapped = (Action)Delegate.Remove(gameEntity.OnSnapped, new Action(this.GrabInitialization));
		GameEntity gameEntity2 = this.myGameEntity;
		gameEntity2.OnGrabbed = (Action)Delegate.Remove(gameEntity2.OnGrabbed, new Action(this.GrabInitialization));
		GameEntity gameEntity3 = this.myGameEntity;
		gameEntity3.OnReleased = (Action)Delegate.Remove(gameEntity3.OnReleased, new Action(this.ReleaseInitialization));
		GameEntity gameEntity4 = this.myGameEntity;
		gameEntity4.OnUnsnapped = (Action)Delegate.Remove(gameEntity4.OnUnsnapped, new Action(this.ReleaseInitialization));
		SpawnRegion<GameEntity, SIResourceRegion>.RemoveItemFromRegion(this.myGameEntity);
	}

	public void GrabInitialization()
	{
		this.isSleeping = false;
		this.shouldSleep = false;
	}

	public void ReleaseInitialization()
	{
		this.shouldSleep = true;
		this.isSleeping = false;
		this.timeReleased = Time.time;
	}

	public virtual bool CanDeposit(SIPlayer depositingPlayer)
	{
		return this.lastPlayerHeld.gamePlayer.IsLocal() && !this.localDeposited && SIPlayer.LocalPlayer.CanLimitedResourceBeDeposited(this.limitedDepositType);
	}

	public virtual void HandleDepositLocal(SIPlayer depositingPlayer)
	{
		this.localDeposited = true;
	}

	public virtual void HandleDepositAuth(SIPlayer depositingPlayer)
	{
	}

	private void HandleOnDestroyed(GameEntity entity)
	{
		if (!this.localEverGrabbed || this.localDeposited || !entity.manager.IsZoneActive() || !PhotonNetwork.InRoom)
		{
			return;
		}
		if (this.type == SIResource.ResourceType.StrangeWood)
		{
			PlayerGameEvents.MiscEvent("SIHelpOtherCollectStrangeWood", 1);
			return;
		}
		if (this.type == SIResource.ResourceType.WeirdGear)
		{
			PlayerGameEvents.MiscEvent("SIHelpOtherCollectWeirdGears", 1);
			return;
		}
		if (this.type == SIResource.ResourceType.FloppyMetal)
		{
			PlayerGameEvents.MiscEvent("SIHelpOtherCollectFloppyMetal", 1);
			return;
		}
		if (this.type == SIResource.ResourceType.BouncySand)
		{
			PlayerGameEvents.MiscEvent("SIHelpOtherCollectBouncySand", 1);
			return;
		}
		if (this.type == SIResource.ResourceType.VibratingSpring)
		{
			PlayerGameEvents.MiscEvent("SIHelpOtherCollectVibratingSpring", 1);
		}
	}

	public static List<SIResource.ResourceCost> GetSum(params IList<SIResource.ResourceCost>[] costs)
	{
		List<SIResource.ResourceCost> list = new List<SIResource.ResourceCost>();
		if (costs == null)
		{
			return list;
		}
		for (int i = 0; i < costs.Length; i++)
		{
			foreach (SIResource.ResourceCost resourceCost in costs[i])
			{
				list.AddResourceCost(resourceCost);
			}
		}
		return list;
	}

	public static List<SIResource.ResourceCost> GetMax(params IList<SIResource.ResourceCost>[] costs)
	{
		List<SIResource.ResourceCost> list = new List<SIResource.ResourceCost>();
		if (costs == null)
		{
			return list;
		}
		for (int i = 0; i < costs.Length; i++)
		{
			foreach (SIResource.ResourceCost resourceCost in costs[i])
			{
				int num = Mathf.Max(list.GetAmount(resourceCost.type), resourceCost.amount);
				list.SetAmount(resourceCost.type, num);
			}
		}
		return list;
	}

	public static bool CategoryCostsMatch(IList<SIResource.ResourceCost> cost1, IList<SIResource.ResourceCost> cost2)
	{
		return cost1.GetCategoryCosts() == cost2.GetCategoryCosts();
	}

	public static bool CostsAreEqual(IList<SIResource.ResourceCost> cost1, IList<SIResource.ResourceCost> cost2, bool matchOrder = true)
	{
		if (cost1.Count != cost2.Count)
		{
			return false;
		}
		if (!matchOrder)
		{
			foreach (SIResource.ResourceCost resourceCost in cost1)
			{
				if (cost2.GetAmount(resourceCost.type) != resourceCost.amount)
				{
					return false;
				}
			}
			return true;
		}
		for (int i = 0; i < cost1.Count; i++)
		{
			if (!cost1[i].Equals(cost2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public SIPlayer lastPlayerHeld;

	public GameEntity myGameEntity;

	public SIResource.ResourceType type;

	public SIResource.LimitedDepositType limitedDepositType;

	public bool localDeposited;

	public bool localEverGrabbed;

	[Tooltip("The amount of pitch offset allowed during spawn, in degrees.  With this set to 0, item will always spawn aligned with surface.")]
	public float spawnPitchVariance;

	public float sleepTime = 10f;

	private bool shouldSleep = true;

	private bool isSleeping;

	private float timeReleased;

	private Rigidbody _rb;

	[Serializable]
	public struct ResourceCost : IComparable<SIResource.ResourceCost>, IEquatable<SIResource.ResourceCost>
	{
		public ResourceCost(SIResource.ResourceType type, int amount)
		{
			this.type = type;
			this.amount = amount;
		}

		public int CompareTo(SIResource.ResourceCost other)
		{
			int num = this.type.CompareTo(other.type);
			if (num != 0)
			{
				return num;
			}
			return this.amount.CompareTo(other.amount);
		}

		public bool Equals(SIResource.ResourceCost other)
		{
			return this.type == other.type && this.amount == other.amount;
		}

		public override bool Equals(object obj)
		{
			if (obj is SIResource.ResourceCost)
			{
				SIResource.ResourceCost resourceCost = (SIResource.ResourceCost)obj;
				return this.Equals(resourceCost);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine<int, int>((int)this.type, this.amount);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", this.type.ToString(), this.amount);
		}

		public SIResource.ResourceType type;

		public int amount;
	}

	public struct ResourceCategoryCost : IComparable<SIResource.ResourceCategoryCost>, IEquatable<SIResource.ResourceCategoryCost>
	{
		public ResourceCategoryCost(int techPoints, int misc)
		{
			this.techPoints = techPoints;
			this.misc = misc;
		}

		public int CompareTo(SIResource.ResourceCategoryCost other)
		{
			int num = this.techPoints.CompareTo(other.techPoints);
			if (num != 0)
			{
				return num;
			}
			return this.misc.CompareTo(other.misc);
		}

		public bool Equals(SIResource.ResourceCategoryCost other)
		{
			return this.techPoints == other.techPoints && this.misc == other.misc;
		}

		public static bool operator ==(SIResource.ResourceCategoryCost left, SIResource.ResourceCategoryCost right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SIResource.ResourceCategoryCost left, SIResource.ResourceCategoryCost right)
		{
			return !left.Equals(right);
		}

		public static SIResource.ResourceCategoryCost operator +(SIResource.ResourceCategoryCost left, SIResource.ResourceCategoryCost right)
		{
			return new SIResource.ResourceCategoryCost(left.techPoints + right.techPoints, left.misc + right.misc);
		}

		public static SIResource.ResourceCategoryCost operator -(SIResource.ResourceCategoryCost left, SIResource.ResourceCategoryCost right)
		{
			return new SIResource.ResourceCategoryCost(left.techPoints - right.techPoints, left.misc - right.misc);
		}

		public static SIResource.ResourceCategoryCost operator *(SIResource.ResourceCategoryCost cost, int multiple)
		{
			return new SIResource.ResourceCategoryCost(cost.techPoints * multiple, cost.misc * multiple);
		}

		public static SIResource.ResourceCategoryCost operator *(int multiple, SIResource.ResourceCategoryCost cost)
		{
			return new SIResource.ResourceCategoryCost(cost.techPoints * multiple, cost.misc * multiple);
		}

		public static SIResource.ResourceCategoryCost Max(SIResource.ResourceCategoryCost left, SIResource.ResourceCategoryCost right)
		{
			return new SIResource.ResourceCategoryCost(Mathf.Max(left.techPoints, right.techPoints), Mathf.Max(left.misc, right.misc));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine<int, int>(this.techPoints, this.misc);
		}

		public int techPoints;

		public int misc;
	}

	public enum ResourceType
	{
		TechPoint,
		StrangeWood,
		WeirdGear,
		VibratingSpring,
		BouncySand,
		FloppyMetal,
		Count
	}

	public enum LimitedDepositType
	{
		None,
		MonkeIdol,
		Count
	}
}
