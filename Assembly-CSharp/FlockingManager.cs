using System;
using System.Collections.Generic;
using Fusion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[NetworkBehaviourWeaved(337)]
public class FlockingManager : NetworkComponent
{
	protected override void Awake()
	{
		base.Awake();
		foreach (GameObject gameObject in this.fishAreaContainer)
		{
			Flocking[] componentsInChildren = gameObject.GetComponentsInChildren<Flocking>(false);
			FlockingManager.FishArea fishArea = new FlockingManager.FishArea();
			fishArea.id = gameObject.name;
			fishArea.colliders = gameObject.GetComponentsInChildren<BoxCollider>();
			fishArea.colliderCenter = fishArea.colliders[0].bounds.center;
			fishArea.fishList.AddRange(componentsInChildren);
			fishArea.zoneBasedObject = gameObject.GetComponent<ZoneBasedObject>();
			this.areaToWaypointDict[fishArea.id] = Vector3.zero;
			Flocking[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FishArea = fishArea;
			}
			this.fishAreaList.Add(fishArea);
			this.allFish.AddRange(fishArea.fishList);
			SlingshotProjectileHitNotifier component = gameObject.GetComponent<SlingshotProjectileHitNotifier>();
			if (component != null)
			{
				component.OnProjectileTriggerEnter += this.ProjectileHitReceiver;
				component.OnProjectileTriggerExit += this.ProjectileHitExit;
			}
			else
			{
				Debug.LogError("Needs SlingshotProjectileHitNotifier added to each fish area");
			}
		}
	}

	private new void Start()
	{
		NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
		this.fishAreaList.Clear();
		this.areaToWaypointDict.Clear();
		this.allFish.Clear();
		foreach (GameObject gameObject in this.fishAreaContainer)
		{
			SlingshotProjectileHitNotifier component = gameObject.GetComponent<SlingshotProjectileHitNotifier>();
			if (component != null)
			{
				component.OnProjectileTriggerExit -= this.ProjectileHitExit;
				component.OnProjectileTriggerEnter -= this.ProjectileHitReceiver;
			}
		}
	}

	private void Update()
	{
		if (Random.Range(0, 10000) < 50)
		{
			foreach (FlockingManager.FishArea fishArea in this.fishAreaList)
			{
				if (fishArea.zoneBasedObject != null)
				{
					fishArea.zoneBasedObject.gameObject.SetActive(fishArea.zoneBasedObject.IsLocalPlayerInZone());
				}
				fishArea.nextWaypoint = this.GetRandomPointInsideCollider(fishArea);
				this.areaToWaypointDict[fishArea.id] = fishArea.nextWaypoint;
				Debug.DrawLine(fishArea.nextWaypoint, Vector3.forward * 5f, Color.magenta);
			}
		}
	}

	public Vector3 GetRandomPointInsideCollider(FlockingManager.FishArea fishArea)
	{
		int num = Random.Range(0, fishArea.colliders.Length);
		BoxCollider boxCollider = fishArea.colliders[num];
		Vector3 vector = boxCollider.size / 2f;
		Vector3 vector2 = new Vector3(Random.Range(-vector.x, vector.x), Random.Range(-vector.y, vector.y), Random.Range(-vector.z, vector.z));
		return boxCollider.transform.TransformPoint(vector2);
	}

	public bool IsInside(Vector3 point, FlockingManager.FishArea fish)
	{
		foreach (BoxCollider boxCollider in fish.colliders)
		{
			Vector3 center = boxCollider.center;
			Vector3 vector = boxCollider.transform.InverseTransformPoint(point);
			vector -= center;
			Vector3 size = boxCollider.size;
			if (Mathf.Abs(vector.x) < size.x / 2f && Mathf.Abs(vector.y) < size.y / 2f && Mathf.Abs(vector.z) < size.z / 2f)
			{
				return true;
			}
		}
		return false;
	}

	public Vector3 RestrictPointToArea(Vector3 point, FlockingManager.FishArea fish)
	{
		Vector3 vector = default(Vector3);
		float num = float.MaxValue;
		foreach (BoxCollider boxCollider in fish.colliders)
		{
			Vector3 center = boxCollider.center;
			Vector3 vector2 = boxCollider.transform.InverseTransformPoint(point);
			Vector3 vector3 = vector2 - center;
			Vector3 size = boxCollider.size;
			float num2 = size.x / 2f;
			float num3 = size.y / 2f;
			float num4 = size.z / 2f;
			if (Mathf.Abs(vector3.x) < num2 && Mathf.Abs(vector3.y) < num3 && Mathf.Abs(vector3.z) < num4)
			{
				return point;
			}
			Vector3 vector4 = new Vector3(center.x - num2, center.y - num3, center.z - num4);
			Vector3 vector5 = new Vector3(center.x + num2, center.y + num3, center.z + num4);
			Vector3 vector6 = new Vector3(Mathf.Clamp(vector2.x, vector4.x, vector5.x), Mathf.Clamp(vector2.y, vector4.y, vector5.y), Mathf.Clamp(vector2.z, vector4.z, vector5.z));
			float num5 = Vector3.Distance(vector2, vector6);
			if (num5 < num)
			{
				num = num5;
				if (num5 > 1f)
				{
					Vector3 vector7 = Vector3.Normalize(vector2 - vector6);
					vector = boxCollider.transform.TransformPoint(vector6 + vector7 * 1f);
				}
				else
				{
					vector = point;
				}
			}
		}
		return vector;
	}

	private void ProjectileHitReceiver(SlingshotProjectile projectile, Collider collider1)
	{
		bool flag = projectile.CompareTag(this.foodProjectileTag);
		FlockingManager.FishFood fishFood = new FlockingManager.FishFood
		{
			collider = (collider1 as BoxCollider),
			isRealFood = flag,
			slingshotProjectile = projectile
		};
		UnityAction<FlockingManager.FishFood> unityAction = this.onFoodDetected;
		if (unityAction == null)
		{
			return;
		}
		unityAction(fishFood);
	}

	private void ProjectileHitExit(SlingshotProjectile projectile, Collider collider2)
	{
		UnityAction<BoxCollider> unityAction = this.onFoodDestroyed;
		if (unityAction == null)
		{
			return;
		}
		unityAction(collider2 as BoxCollider);
	}

	[Networked]
	[NetworkedWeaved(0, 337)]
	public unsafe FlockingData Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FlockingManager.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(FlockingData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FlockingManager.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(FlockingData*)(this.Ptr + 0) = value;
		}
	}

	public override void WriteDataFusion()
	{
		this.Data = new FlockingData(this.allFish);
	}

	public override void ReadDataFusion()
	{
		for (int i = 0; i < this.Data.count; i++)
		{
			Vector3 vector = this.Data.Positions[i];
			Quaternion quaternion = this.Data.Rotations[i];
			this.allFish[i].SetSyncPosRot(vector, quaternion);
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public static void RegisterAvoidPoint(GameObject obj)
	{
		FlockingManager.avoidPoints.Add(obj);
	}

	public static void UnregisterAvoidPoint(GameObject obj)
	{
		FlockingManager.avoidPoints.Remove(obj);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Data = this._Data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Data = this.Data;
	}

	public List<GameObject> fishAreaContainer;

	public string foodProjectileTag = "WaterBalloonProjectile";

	private Dictionary<string, Vector3> areaToWaypointDict = new Dictionary<string, Vector3>();

	private List<FlockingManager.FishArea> fishAreaList = new List<FlockingManager.FishArea>();

	private List<Flocking> allFish = new List<Flocking>();

	public UnityAction<FlockingManager.FishFood> onFoodDetected;

	public UnityAction<BoxCollider> onFoodDestroyed;

	private bool hasBeenSerialized;

	public static readonly List<GameObject> avoidPoints = new List<GameObject>();

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("Data", 0, 337)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private FlockingData _Data;

	public class FishArea
	{
		public string id;

		public List<Flocking> fishList = new List<Flocking>();

		public Vector3 colliderCenter;

		public BoxCollider[] colliders;

		public Vector3 nextWaypoint = Vector3.zero;

		public ZoneBasedObject zoneBasedObject;
	}

	public class FishFood
	{
		public BoxCollider collider;

		public bool isRealFood;

		public SlingshotProjectile slingshotProjectile;
	}
}
