using System;
using System.Collections.Generic;
using UnityEngine;

public class ButterflySwarmManager : MonoBehaviour
{
	public float PerchedFlapSpeed { get; private set; }

	public float PerchedFlapPhase { get; private set; }

	public float BeeSpeed { get; private set; }

	public float BeeMaxTravelTime { get; private set; }

	public float BeeAcceleration { get; private set; }

	public float BeeJitterStrength { get; private set; }

	public float BeeJitterDamping { get; private set; }

	public float BeeMaxJitterRadius { get; private set; }

	public float BeeNearDestinationRadius { get; private set; }

	public float DestRotationAlignmentSpeed { get; private set; }

	public Vector3 TravellingLocalRotationEuler { get; private set; }

	public Quaternion TravellingLocalRotation { get; private set; }

	public float AvoidPointRadius { get; private set; }

	public float BeeMinFlowerDuration { get; private set; }

	public float BeeMaxFlowerDuration { get; private set; }

	public Color[] BeeColors { get; private set; }

	private void Awake()
	{
		this.TravellingLocalRotation = Quaternion.Euler(this.TravellingLocalRotationEuler);
		this.butterflies = new List<AnimatedButterfly>(this.numBees);
		for (int i = 0; i < this.numBees; i++)
		{
			AnimatedButterfly animatedButterfly = default(AnimatedButterfly);
			animatedButterfly.InitVisual(this.beePrefab, this);
			if (this.BeeColors.Length != 0)
			{
				animatedButterfly.SetColor(this.BeeColors[i % this.BeeColors.Length]);
			}
			this.butterflies.Add(animatedButterfly);
		}
	}

	private void Start()
	{
		foreach (XSceneRef xsceneRef in this.perchSections)
		{
			GameObject gameObject;
			if (xsceneRef.TryResolve(out gameObject))
			{
				List<GameObject> list = new List<GameObject>();
				this.allPerchZones.Add(list);
				foreach (object obj in gameObject.transform)
				{
					Transform transform = (Transform)obj;
					list.Add(transform.gameObject);
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
		for (int i = 0; i < this.butterflies.Count; i++)
		{
			AnimatedButterfly animatedButterfly = this.butterflies[i];
			animatedButterfly.UpdateVisual(RandomTimedSeedManager.instance.currentSyncTime, this);
			this.butterflies[i] = animatedButterfly;
		}
	}

	private void OnSeedChange()
	{
		SRand srand = new SRand(RandomTimedSeedManager.instance.seed);
		List<List<GameObject>> list = new List<List<GameObject>>(this.allPerchZones.Count);
		for (int i = 0; i < this.allPerchZones.Count; i++)
		{
			List<GameObject> list2 = new List<GameObject>();
			list2.AddRange(this.allPerchZones[i]);
			list.Add(list2);
		}
		List<GameObject> list3 = new List<GameObject>(this.loopSizePerBee);
		List<float> list4 = new List<float>(this.loopSizePerBee);
		for (int j = 0; j < this.butterflies.Count; j++)
		{
			AnimatedButterfly animatedButterfly = this.butterflies[j];
			animatedButterfly.SetFlapSpeed(srand.NextFloat(this.minFlapSpeed, this.maxFlapSpeed));
			list3.Clear();
			list4.Clear();
			this.PickPoints(this.loopSizePerBee, list, ref srand, list3);
			for (int k = 0; k < list3.Count; k++)
			{
				list4.Add(srand.NextFloat(this.BeeMinFlowerDuration, this.BeeMaxFlowerDuration));
			}
			if (list3.Count == 0)
			{
				this.butterflies.Clear();
				return;
			}
			animatedButterfly.InitRoute(list3, list4, this);
			this.butterflies[j] = animatedButterfly;
		}
	}

	private void PickPoints(int n, List<List<GameObject>> pickBuffer, ref SRand rand, List<GameObject> resultBuffer)
	{
		int num = rand.NextInt(0, pickBuffer.Count);
		int num2 = -1;
		int num3 = n - 2;
		while (resultBuffer.Count < n)
		{
			int num4;
			if (resultBuffer.Count < num3)
			{
				num4 = rand.NextIntWithExclusion(0, pickBuffer.Count, num2);
			}
			else
			{
				num4 = rand.NextIntWithExclusion2(0, pickBuffer.Count, num2, num);
			}
			int num5 = 10;
			while (num4 == num2 || pickBuffer[num4].Count == 0)
			{
				num4 = (num4 + 1) % pickBuffer.Count;
				num5--;
				if (num5 <= 0)
				{
					return;
				}
			}
			num2 = num4;
			List<GameObject> list = pickBuffer[num2];
			while (list.Count == 0)
			{
				num2 = (num2 + 1) % pickBuffer.Count;
				list = pickBuffer[num2];
			}
			resultBuffer.Add(list[list.Count - 1]);
			list.RemoveAt(list.Count - 1);
		}
	}

	[SerializeField]
	private XSceneRef[] perchSections;

	[SerializeField]
	private int loopSizePerBee;

	[SerializeField]
	private int numBees;

	[SerializeField]
	private MeshRenderer beePrefab;

	[SerializeField]
	private float maxFlapSpeed;

	[SerializeField]
	private float minFlapSpeed;

	private List<AnimatedButterfly> butterflies;

	private List<List<GameObject>> allPerchZones = new List<List<GameObject>>();
}
