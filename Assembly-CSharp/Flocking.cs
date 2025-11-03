using System;
using GorillaExtensions;
using GorillaTagScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Flocking : MonoBehaviour
{
	public FlockingManager.FishArea FishArea { get; set; }

	private void Awake()
	{
		this.manager = base.GetComponentInParent<FlockingManager>();
	}

	private void Start()
	{
		this.speed = Random.Range(this.minSpeed, this.maxSpeed);
		this.fishState = Flocking.FishState.patrol;
	}

	private void OnDisable()
	{
		FlockingManager flockingManager = this.manager;
		flockingManager.onFoodDetected = (UnityAction<FlockingManager.FishFood>)Delegate.Remove(flockingManager.onFoodDetected, new UnityAction<FlockingManager.FishFood>(this.HandleOnFoodDetected));
		FlockingManager flockingManager2 = this.manager;
		flockingManager2.onFoodDestroyed = (UnityAction<BoxCollider>)Delegate.Remove(flockingManager2.onFoodDestroyed, new UnityAction<BoxCollider>(this.HandleOnFoodDestroyed));
		FlockingUpdateManager.UnregisterFlocking(this);
	}

	public void InvokeUpdate()
	{
		if (this.manager == null)
		{
			this.manager = base.GetComponentInParent<FlockingManager>();
		}
		this.AvoidPlayerHands();
		this.MaybeTurn();
		switch (this.fishState)
		{
		case Flocking.FishState.flock:
			this.Flock(this.FishArea.nextWaypoint);
			this.SwitchState(Flocking.FishState.patrol);
			break;
		case Flocking.FishState.patrol:
			if (Random.Range(0, 10) < 2)
			{
				this.SwitchState(Flocking.FishState.flock);
			}
			break;
		case Flocking.FishState.followFood:
			if (this.isTurning)
			{
				return;
			}
			if (this.isRealFood)
			{
				if ((double)Vector3.Distance(base.transform.position, this.projectileGameObject.transform.position) > this.FollowFoodStopDistance)
				{
					this.FollowFood();
				}
				else
				{
					this.followingFood = false;
					this.Flock(this.projectileGameObject.transform.position);
					this.feedingTimeStarted += Time.deltaTime;
					if (this.feedingTimeStarted > this.eatFoodDuration)
					{
						this.SwitchState(Flocking.FishState.patrol);
					}
				}
			}
			else if (Vector3.Distance(base.transform.position, this.projectileGameObject.transform.position) > this.FollowFakeFoodStopDistance)
			{
				this.FollowFood();
			}
			else
			{
				this.followingFood = false;
				this.SwitchState(Flocking.FishState.patrol);
			}
			break;
		}
		if (!this.followingFood)
		{
			base.transform.Translate(0f, 0f, this.speed * Time.deltaTime);
		}
		this.pos = base.transform.position;
		this.rot = base.transform.rotation;
	}

	private void MaybeTurn()
	{
		if (!this.manager.IsInside(base.transform.position, this.FishArea))
		{
			this.Turn(this.FishArea.colliderCenter);
			if (Vector3.Angle(this.FishArea.colliderCenter - base.transform.position, Vector3.forward) > 5f)
			{
				this.isTurning = true;
				return;
			}
		}
		else
		{
			this.isTurning = false;
		}
	}

	private void Turn(Vector3 towardPoint)
	{
		this.isTurning = true;
		Quaternion quaternion = Quaternion.LookRotation(towardPoint - base.transform.position);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, this.rotationSpeed * Time.deltaTime);
	}

	private void SwitchState(Flocking.FishState state)
	{
		this.fishState = state;
	}

	private void Flock(Vector3 nextGoal)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		float num = 1f;
		int num2 = 0;
		foreach (Flocking flocking in this.FishArea.fishList)
		{
			if (flocking.gameObject != base.gameObject)
			{
				float num3 = Vector3.Distance(flocking.transform.position, base.transform.position);
				if (num3 <= this.maxNeighbourDistance)
				{
					vector += flocking.transform.position;
					num2++;
					if (num3 < this.flockingAvoidanceDistance)
					{
						vector2 += base.transform.position - flocking.transform.position;
					}
					num += flocking.speed;
				}
			}
		}
		if (num2 > 0)
		{
			this.fishState = Flocking.FishState.flock;
			vector = vector / (float)num2 + (nextGoal - base.transform.position);
			this.speed = num / (float)num2;
			this.speed = Mathf.Clamp(this.speed, this.minSpeed, this.maxSpeed);
			Vector3 vector3 = vector + vector2 - base.transform.position;
			if (vector3 != Vector3.zero)
			{
				Quaternion quaternion = Quaternion.LookRotation(vector3);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, this.rotationSpeed * Time.deltaTime);
			}
		}
	}

	private void HandleOnFoodDetected(FlockingManager.FishFood fishFood)
	{
		bool flag = false;
		foreach (BoxCollider boxCollider in this.FishArea.colliders)
		{
			if (fishFood.collider == boxCollider)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		this.SwitchState(Flocking.FishState.followFood);
		this.feedingTimeStarted = 0f;
		this.projectileGameObject = fishFood.slingshotProjectile.gameObject;
		this.isRealFood = fishFood.isRealFood;
	}

	private void HandleOnFoodDestroyed(BoxCollider collider)
	{
		bool flag = false;
		foreach (BoxCollider boxCollider in this.FishArea.colliders)
		{
			if (collider == boxCollider)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		this.SwitchState(Flocking.FishState.patrol);
		this.projectileGameObject = null;
		this.followingFood = false;
	}

	private void FollowFood()
	{
		this.followingFood = true;
		Quaternion quaternion = Quaternion.LookRotation(this.projectileGameObject.transform.position - base.transform.position);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, this.rotationSpeed * Time.deltaTime);
		base.transform.position = Vector3.MoveTowards(base.transform.position, this.projectileGameObject.transform.position, this.speed * this.followFoodSpeedMult * Time.deltaTime);
	}

	private void AvoidPlayerHands()
	{
		foreach (GameObject gameObject in FlockingManager.avoidPoints)
		{
			Vector3 position = gameObject.transform.position;
			if ((base.transform.position - position).IsShorterThan(this.avointPointRadius))
			{
				Vector3 randomPointInsideCollider = this.manager.GetRandomPointInsideCollider(this.FishArea);
				this.Turn(randomPointInsideCollider);
				this.speed = this.avoidHandSpeed;
			}
		}
	}

	internal void SetSyncPosRot(Vector3 syncPos, Quaternion syncRot)
	{
		if (this.manager == null)
		{
			this.manager = base.GetComponentInParent<FlockingManager>();
		}
		if (this.FishArea == null)
		{
			Debug.LogError("FISH AREA NULL");
		}
		if ((in syncRot).IsValid())
		{
			this.rot = syncRot;
		}
		float num = 10000f;
		if ((in syncPos).IsValid(in num))
		{
			this.pos = this.manager.RestrictPointToArea(syncPos, this.FishArea);
		}
	}

	private void OnEnable()
	{
		if (this.manager == null)
		{
			this.manager = base.GetComponentInParent<FlockingManager>();
		}
		FlockingManager flockingManager = this.manager;
		flockingManager.onFoodDetected = (UnityAction<FlockingManager.FishFood>)Delegate.Combine(flockingManager.onFoodDetected, new UnityAction<FlockingManager.FishFood>(this.HandleOnFoodDetected));
		FlockingManager flockingManager2 = this.manager;
		flockingManager2.onFoodDestroyed = (UnityAction<BoxCollider>)Delegate.Combine(flockingManager2.onFoodDestroyed, new UnityAction<BoxCollider>(this.HandleOnFoodDestroyed));
		FlockingUpdateManager.RegisterFlocking(this);
	}

	[Tooltip("Speed is randomly generated from min and max speed")]
	public float minSpeed = 2f;

	public float maxSpeed = 4f;

	public float rotationSpeed = 360f;

	[Tooltip("Maximum distance to the neighbours to form a flocking group")]
	public float maxNeighbourDistance = 4f;

	public float eatFoodDuration = 10f;

	[Tooltip("How fast should it follow the food? This value multiplies by the current speed")]
	public float followFoodSpeedMult = 3f;

	[Tooltip("How fast should it run away from players hand?")]
	public float avoidHandSpeed = 1.2f;

	[FormerlySerializedAs("avoidanceDistance")]
	[Tooltip("When flocking they will avoid each other if the distance between them is less than this value")]
	public float flockingAvoidanceDistance = 2f;

	[Tooltip("Follow the fish food until they are this far from it")]
	[FormerlySerializedAs("distanceToFollowFood")]
	public double FollowFoodStopDistance = 0.20000000298023224;

	[Tooltip("Follow any fake fish food until they are this far from it")]
	[FormerlySerializedAs("distanceToFollowFakeFood")]
	public float FollowFakeFoodStopDistance = 2f;

	private float speed;

	private Vector3 averageHeading;

	private Vector3 averagePosition;

	private float feedingTimeStarted;

	private GameObject projectileGameObject;

	private bool followingFood;

	private FlockingManager manager;

	private GameObjectManagerWithId _fishSceneGameObjectsManager;

	private UnityEvent<string, Transform> sendIdEvent;

	private Flocking.FishState fishState;

	[HideInInspector]
	public Vector3 pos;

	[HideInInspector]
	public Quaternion rot;

	private float velocity;

	private bool isTurning;

	private bool isRealFood;

	public float avointPointRadius = 0.5f;

	private float cacheSpeed;

	public enum FishState
	{
		flock,
		patrol,
		followFood
	}
}
