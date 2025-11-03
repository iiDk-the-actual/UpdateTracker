using System;
using System.Collections.Generic;
using UnityEngine;

public class BeeSwarmManager : MonoBehaviour
{
	public BeePerchPoint BeeHive { get; private set; }

	public float BeeSpeed { get; private set; }

	public float BeeMaxTravelTime { get; private set; }

	public float BeeAcceleration { get; private set; }

	public float BeeJitterStrength { get; private set; }

	public float BeeJitterDamping { get; private set; }

	public float BeeMaxJitterRadius { get; private set; }

	public float BeeNearDestinationRadius { get; private set; }

	public float AvoidPointRadius { get; private set; }

	public float BeeMinFlowerDuration { get; private set; }

	public float BeeMaxFlowerDuration { get; private set; }

	public float GeneralBuzzRange { get; private set; }

	private void Awake()
	{
		this.bees = new List<AnimatedBee>(this.numBees);
		for (int i = 0; i < this.numBees; i++)
		{
			AnimatedBee animatedBee = default(AnimatedBee);
			animatedBee.InitVisual(this.beePrefab, this);
			this.bees.Add(animatedBee);
		}
		this.playerCamera = Camera.main.transform;
	}

	private void Start()
	{
		foreach (XSceneRef xsceneRef in this.flowerSections)
		{
			GameObject gameObject;
			if (xsceneRef.TryResolve(out gameObject))
			{
				foreach (BeePerchPoint beePerchPoint in gameObject.GetComponentsInChildren<BeePerchPoint>())
				{
					this.allPerchPoints.Add(beePerchPoint);
				}
			}
		}
		this.OnSeedChange();
		RandomTimedSeedManager.instance.AddCallbackOnSeedChanged(new Action(this.OnSeedChange));
	}

	private void OnDestroy()
	{
		RandomTimedSeedManager.instance.RemoveCallbackOnSeedChanged(new Action(this.OnSeedChange));
	}

	private void Update()
	{
		Vector3 position = this.playerCamera.transform.position;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		float num = 1f / (float)this.bees.Count;
		float num2 = float.PositiveInfinity;
		float num3 = this.GeneralBuzzRange * this.GeneralBuzzRange;
		int num4 = 0;
		for (int i = 0; i < this.bees.Count; i++)
		{
			AnimatedBee animatedBee = this.bees[i];
			animatedBee.UpdateVisual(RandomTimedSeedManager.instance.currentSyncTime, this);
			Vector3 position2 = animatedBee.visual.transform.position;
			float sqrMagnitude = (position2 - position).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				vector = position2;
				num2 = sqrMagnitude;
			}
			if (sqrMagnitude < num3)
			{
				vector2 += position2;
				num4++;
			}
			this.bees[i] = animatedBee;
		}
		this.nearbyBeeBuzz.transform.position = vector;
		if (num4 > 0)
		{
			this.generalBeeBuzz.transform.position = vector2 / (float)num4;
			this.generalBeeBuzz.enabled = true;
			return;
		}
		this.generalBeeBuzz.enabled = false;
	}

	private void OnSeedChange()
	{
		SRand srand = new SRand(RandomTimedSeedManager.instance.seed);
		List<BeePerchPoint> list = new List<BeePerchPoint>(this.allPerchPoints.Count);
		List<BeePerchPoint> list2 = new List<BeePerchPoint>(this.loopSizePerBee);
		List<float> list3 = new List<float>(this.loopSizePerBee);
		for (int i = 0; i < this.bees.Count; i++)
		{
			AnimatedBee animatedBee = this.bees[i];
			list2 = new List<BeePerchPoint>(this.loopSizePerBee);
			list3 = new List<float>(this.loopSizePerBee);
			this.PickPoints(this.loopSizePerBee, list, this.allPerchPoints, ref srand, list2);
			for (int j = 0; j < list2.Count; j++)
			{
				list3.Add(srand.NextFloat(this.BeeMinFlowerDuration, this.BeeMaxFlowerDuration));
			}
			animatedBee.InitRoute(list2, list3, this);
			animatedBee.InitRouteTimestamps();
			this.bees[i] = animatedBee;
		}
	}

	private void PickPoints(int n, List<BeePerchPoint> pickBuffer, List<BeePerchPoint> allPerchPoints, ref SRand rand, List<BeePerchPoint> resultBuffer)
	{
		resultBuffer.Add(this.BeeHive);
		n--;
		int num = 100;
		while (pickBuffer.Count < n && num-- > 0)
		{
			n -= pickBuffer.Count;
			resultBuffer.AddRange(pickBuffer);
			pickBuffer.Clear();
			pickBuffer.AddRange(allPerchPoints);
			rand.Shuffle<BeePerchPoint>(pickBuffer);
		}
		resultBuffer.AddRange(pickBuffer.GetRange(pickBuffer.Count - n, n));
		pickBuffer.RemoveRange(pickBuffer.Count - n, n);
	}

	public static void RegisterAvoidPoint(GameObject obj)
	{
		BeeSwarmManager.avoidPoints.Add(obj);
	}

	public static void UnregisterAvoidPoint(GameObject obj)
	{
		BeeSwarmManager.avoidPoints.Remove(obj);
	}

	[SerializeField]
	private XSceneRef[] flowerSections;

	[SerializeField]
	private int loopSizePerBee;

	[SerializeField]
	private int numBees;

	[SerializeField]
	private MeshRenderer beePrefab;

	[SerializeField]
	private AudioSource nearbyBeeBuzz;

	[SerializeField]
	private AudioSource generalBeeBuzz;

	private GameObject[] flowerSectionsResolved;

	private List<AnimatedBee> bees;

	private Transform playerCamera;

	private List<BeePerchPoint> allPerchPoints = new List<BeePerchPoint>();

	public static readonly List<GameObject> avoidPoints = new List<GameObject>();
}
