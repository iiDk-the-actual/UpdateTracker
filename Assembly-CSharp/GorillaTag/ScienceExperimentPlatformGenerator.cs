using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaTag.GuidedRefs;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag
{
	public class ScienceExperimentPlatformGenerator : MonoBehaviourPun, ITickSystemPost, IGuidedRefReceiverMono, IGuidedRefMonoBehaviour, IGuidedRefObject
	{
		private void Awake()
		{
			((IGuidedRefObject)this).GuidedRefInitialize();
			this.scienceExperimentManager = base.GetComponent<ScienceExperimentManager>();
		}

		private void OnEnable()
		{
			if (((IGuidedRefReceiverMono)this).GuidedRefsWaitingToResolveCount > 0)
			{
				return;
			}
			TickSystem<object>.AddPostTickCallback(this);
		}

		protected void OnDisable()
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		bool ITickSystemPost.PostTickRunning { get; set; }

		void ITickSystemPost.PostTick()
		{
			double num = (PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.unscaledTimeAsDouble);
			this.UpdateTrails(num);
			this.RemoveExpiredBubbles(num);
			this.SpawnNewBubbles(num);
			this.UpdateActiveBubbles(num);
		}

		private void RemoveExpiredBubbles(double currentTime)
		{
			for (int i = this.activeBubbles.Count - 1; i >= 0; i--)
			{
				if (Mathf.Clamp01((float)(currentTime - this.activeBubbles[i].spawnTime) / this.activeBubbles[i].lifetime) >= 1f)
				{
					this.activeBubbles[i].bubble.Pop();
					this.activeBubbles.RemoveAt(i);
				}
			}
		}

		private void SpawnNewBubbles(double currentTime)
		{
			if (base.photonView.IsMine && this.scienceExperimentManager.GameState == ScienceExperimentManager.RisingLiquidState.Rising)
			{
				int num = Mathf.Min((int)(this.rockCountVsLavaProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.bubbleCountMultiplier), this.maxBubbleCount) - this.activeBubbles.Count;
				if (this.activeBubbles.Count < this.maxBubbleCount)
				{
					for (int i = 0; i < num; i++)
					{
						this.SpawnRockAuthority(currentTime, this.scienceExperimentManager.RiseProgressLinear);
					}
				}
			}
		}

		private void UpdateActiveBubbles(double currentTime)
		{
			if (this.liquidSurfacePlane == null)
			{
				return;
			}
			float y = this.liquidSurfacePlane.transform.position.y;
			float num = this.bubblePopWobbleAmplitude * Mathf.Sin(this.bubblePopWobbleFrequency * 0.5f * 3.1415927f * Time.time);
			for (int i = 0; i < this.activeBubbles.Count; i++)
			{
				ScienceExperimentPlatformGenerator.BubbleData bubbleData = this.activeBubbles[i];
				float num2 = Mathf.Clamp01((float)(currentTime - bubbleData.spawnTime) / bubbleData.lifetime);
				float num3 = bubbleData.spawnSize * this.rockSizeVsLifetime.Evaluate(num2) * this.scaleFactor;
				bubbleData.position.y = y;
				bubbleData.bubble.body.gameObject.transform.localScale = Vector3.one * num3;
				bubbleData.bubble.body.MovePosition(bubbleData.position);
				float num4 = (float)((double)bubbleData.lifetime + bubbleData.spawnTime - currentTime);
				if (num4 < this.bubblePopAnticipationTime)
				{
					float num5 = Mathf.Clamp01(1f - num4 / this.bubblePopAnticipationTime);
					bubbleData.bubble.bubbleMesh.transform.localScale = Vector3.one * (1f + num5 * num);
				}
				this.activeBubbles[i] = bubbleData;
			}
		}

		private void UpdateTrails(double currentTime)
		{
			if (base.photonView.IsMine)
			{
				int num = (int)(this.trailCountVsProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.trailCountMultiplier) - this.trailHeads.Count;
				if (num > 0 && this.scienceExperimentManager.GameState == ScienceExperimentManager.RisingLiquidState.Rising)
				{
					for (int i = 0; i < num; i++)
					{
						this.SpawnTrailAuthority(currentTime, this.scienceExperimentManager.RiseProgressLinear);
					}
				}
				else if (num < 0)
				{
					for (int j = 0; j > num; j--)
					{
						this.trailHeads.RemoveAt(0);
					}
				}
				float num2 = this.trailSpawnRateVsProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.trailSpawnRateMultiplier;
				float num3 = this.trailBubbleBoundaryRadiusVsProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.surfaceRadiusSpawnRange.y;
				for (int k = this.trailHeads.Count - 1; k >= 0; k--)
				{
					if ((float)(currentTime - this.trailHeads[k].spawnTime) > num2)
					{
						float num4 = -this.trailMaxTurnAngle;
						float num5 = this.trailMaxTurnAngle;
						float num6 = Vector3.SignedAngle(this.trailHeads[k].direction, this.trailHeads[k].position - this.liquidSurfacePlane.transform.position, Vector3.up);
						float num7 = num3 - Vector3.Distance(this.trailHeads[k].position, this.liquidSurfacePlane.transform.position);
						if (num7 < this.trailEdgeAvoidanceSpawnsMinMax.x * this.trailDistanceBetweenSpawns * this.scaleFactor)
						{
							float num8 = Mathf.InverseLerp(this.trailEdgeAvoidanceSpawnsMinMax.x * this.trailDistanceBetweenSpawns * this.scaleFactor, this.trailEdgeAvoidanceSpawnsMinMax.y * this.trailDistanceBetweenSpawns * this.scaleFactor, num7);
							if (num6 > 0f)
							{
								float num9 = num6 - 90f * num8;
								num5 = Mathf.Min(num5, num9);
								num4 = Mathf.Min(num4, num5 - this.trailMaxTurnAngle);
							}
							else
							{
								float num10 = num6 + 90f * num8;
								num4 = Mathf.Max(num4, num10);
								num5 = Mathf.Max(num5, num4 + this.trailMaxTurnAngle);
							}
						}
						Vector3 vector = Quaternion.AngleAxis(Random.Range(num4, num5), Vector3.up) * this.trailHeads[k].direction;
						Vector3 vector2 = this.trailHeads[k].position + vector * this.trailDistanceBetweenSpawns * this.scaleFactor - this.liquidSurfacePlane.transform.position;
						if (vector2.sqrMagnitude > this.surfaceRadiusSpawnRange.y * this.surfaceRadiusSpawnRange.y)
						{
							vector2 = vector2.normalized * this.surfaceRadiusSpawnRange.y;
						}
						Vector2 vector3 = new Vector2(vector2.x, vector2.z);
						float num11 = this.trailBubbleSize;
						float num12 = this.trailBubbleLifetimeVsProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.trailBubbleLifetimeMultiplier;
						this.trailHeads.RemoveAt(k);
						base.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector3, num11, num12, currentTime });
						this.SpawnSodaBubbleLocal(vector3, num11, num12, currentTime, true, vector);
					}
				}
			}
		}

		private void SpawnRockAuthority(double currentTime, float lavaProgress)
		{
			if (base.photonView.IsMine)
			{
				float num = this.rockLifetimeMultiplierVsLavaProgress.Evaluate(lavaProgress);
				float num2 = this.rockMaxSizeMultiplierVsLavaProgress.Evaluate(lavaProgress);
				float num3 = Random.Range(this.lifetimeRange.x, this.lifetimeRange.y) * num;
				float num4 = Random.Range(this.sizeRange.x, this.sizeRange.y * num2);
				float num5 = this.spawnRadiusMultiplierVsLavaProgress.Evaluate(lavaProgress);
				Vector2 vector = Random.insideUnitCircle.normalized * Random.Range(this.surfaceRadiusSpawnRange.x, this.surfaceRadiusSpawnRange.y) * num5;
				vector = this.GetSpawnPositionWithClearance(vector, num4 * this.scaleFactor, this.surfaceRadiusSpawnRange.y, this.liquidSurfacePlane.transform.position);
				base.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, num4, num3, currentTime });
				this.SpawnSodaBubbleLocal(vector, num4, num3, currentTime, false, default(Vector3));
			}
		}

		private void SpawnTrailAuthority(double currentTime, float lavaProgress)
		{
			if (base.photonView.IsMine)
			{
				float num = this.trailBubbleLifetimeVsProgress.Evaluate(this.scienceExperimentManager.RiseProgressLinear) * this.trailBubbleLifetimeMultiplier;
				float num2 = this.trailBubbleSize;
				Vector2 vector = Random.insideUnitCircle.normalized * Random.Range(this.surfaceRadiusSpawnRange.x, this.surfaceRadiusSpawnRange.y);
				vector = this.GetSpawnPositionWithClearance(vector, num2 * this.scaleFactor, this.surfaceRadiusSpawnRange.y, this.liquidSurfacePlane.transform.position);
				Vector3 vector2 = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
				base.photonView.RPC("SpawnSodaBubbleRPC", RpcTarget.Others, new object[] { vector, num2, num, currentTime });
				this.SpawnSodaBubbleLocal(vector, num2, num, currentTime, true, vector2);
			}
		}

		private void SpawnSodaBubbleLocal(Vector2 surfacePosLocal, float spawnSize, float lifetime, double spawnTime, bool addAsTrail = false, Vector3 direction = default(Vector3))
		{
			if (this.activeBubbles.Count < this.maxBubbleCount)
			{
				Vector3 vector = this.liquidSurfacePlane.transform.position + new Vector3(surfacePosLocal.x, 0f, surfacePosLocal.y);
				ScienceExperimentPlatformGenerator.BubbleData bubbleData = new ScienceExperimentPlatformGenerator.BubbleData
				{
					position = vector,
					spawnSize = spawnSize,
					lifetime = lifetime,
					spawnTime = spawnTime,
					isTrail = false
				};
				bubbleData.bubble = ObjectPools.instance.Instantiate(this.spawnedPrefab, bubbleData.position, Quaternion.identity, 0f, true).GetComponent<SodaBubble>();
				if (base.photonView.IsMine && addAsTrail)
				{
					bubbleData.direction = direction;
					bubbleData.isTrail = true;
					this.trailHeads.Add(bubbleData);
				}
				this.activeBubbles.Add(bubbleData);
			}
		}

		[PunRPC]
		public void SpawnSodaBubbleRPC(Vector2 surfacePosLocal, float spawnSize, float lifetime, double spawnTime, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SpawnSodaBubbleRPC");
			if (info.Sender == PhotonNetwork.MasterClient)
			{
				if (!float.IsFinite(spawnSize) || !float.IsFinite(lifetime) || !double.IsFinite(spawnTime))
				{
					return;
				}
				float num = Mathf.Clamp01(this.scienceExperimentManager.RiseProgressLinear);
				(ref surfacePosLocal).ClampThisMagnitudeSafe(this.surfaceRadiusSpawnRange.y);
				spawnSize = Mathf.Clamp(spawnSize, this.sizeRange.x, this.sizeRange.y * this.rockMaxSizeMultiplierVsLavaProgress.Evaluate(num));
				lifetime = Mathf.Clamp(lifetime, this.lifetimeRange.x, this.lifetimeRange.y * this.rockLifetimeMultiplierVsLavaProgress.Evaluate(num));
				double num2 = (PhotonNetwork.InRoom ? PhotonNetwork.Time : Time.unscaledTimeAsDouble);
				spawnTime = ((Mathf.Abs((float)(spawnTime - num2)) < 10f) ? spawnTime : num2);
				this.SpawnSodaBubbleLocal(surfacePosLocal, spawnSize, lifetime, spawnTime, false, default(Vector3));
			}
		}

		private Vector2 GetSpawnPositionWithClearance(Vector2 inputPosition, float inputSize, float maxDistance, Vector3 lavaSurfaceOrigin)
		{
			Vector2 vector = inputPosition;
			for (int i = 0; i < this.activeBubbles.Count; i++)
			{
				Vector3 vector2 = this.activeBubbles[i].position - lavaSurfaceOrigin;
				Vector2 vector3 = new Vector2(vector2.x, vector2.z);
				Vector2 vector4 = vector - vector3;
				float num = (inputSize + this.activeBubbles[i].spawnSize * this.scaleFactor) * 0.5f;
				if (vector4.sqrMagnitude < num * num)
				{
					float magnitude = vector4.magnitude;
					if (magnitude > 0.001f)
					{
						Vector2 vector5 = vector4 / magnitude;
						vector += vector5 * (num - magnitude);
						if (vector.sqrMagnitude > maxDistance * maxDistance)
						{
							vector = vector.normalized * maxDistance;
						}
					}
				}
			}
			if (vector.sqrMagnitude > this.surfaceRadiusSpawnRange.y * this.surfaceRadiusSpawnRange.y)
			{
				vector = vector.normalized * this.surfaceRadiusSpawnRange.y;
			}
			return vector;
		}

		void IGuidedRefObject.GuidedRefInitialize()
		{
			GuidedRefHub.RegisterReceiverField<ScienceExperimentPlatformGenerator>(this, "liquidSurfacePlane", ref this.liquidSurfacePlane_gRef);
			GuidedRefHub.ReceiverFullyRegistered<ScienceExperimentPlatformGenerator>(this);
		}

		int IGuidedRefReceiverMono.GuidedRefsWaitingToResolveCount { get; set; }

		bool IGuidedRefReceiverMono.GuidedRefTryResolveReference(GuidedRefTryResolveInfo target)
		{
			return GuidedRefHub.TryResolveField<ScienceExperimentPlatformGenerator, Transform>(this, ref this.liquidSurfacePlane, this.liquidSurfacePlane_gRef, target);
		}

		void IGuidedRefReceiverMono.OnAllGuidedRefsResolved()
		{
			if (!base.enabled)
			{
				return;
			}
			TickSystem<object>.AddPostTickCallback(this);
		}

		void IGuidedRefReceiverMono.OnGuidedRefTargetDestroyed(int fieldId)
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		Transform IGuidedRefMonoBehaviour.get_transform()
		{
			return base.transform;
		}

		int IGuidedRefObject.GetInstanceID()
		{
			return base.GetInstanceID();
		}

		[SerializeField]
		private GameObject spawnedPrefab;

		[SerializeField]
		private float scaleFactor = 0.03f;

		[Header("Random Bubbles")]
		[SerializeField]
		private Vector2 surfaceRadiusSpawnRange = new Vector2(0.1f, 0.7f);

		[SerializeField]
		private Vector2 lifetimeRange = new Vector2(5f, 10f);

		[SerializeField]
		private Vector2 sizeRange = new Vector2(0.5f, 2f);

		[SerializeField]
		private AnimationCurve rockCountVsLavaProgress = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		[FormerlySerializedAs("rockCountMultiplier")]
		private float bubbleCountMultiplier = 80f;

		[SerializeField]
		private int maxBubbleCount = 100;

		[SerializeField]
		private AnimationCurve rockLifetimeMultiplierVsLavaProgress = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		private AnimationCurve rockMaxSizeMultiplierVsLavaProgress = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		private AnimationCurve spawnRadiusMultiplierVsLavaProgress = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		private AnimationCurve rockSizeVsLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Header("Bubble Trails")]
		[SerializeField]
		private AnimationCurve trailSpawnRateVsProgress = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float trailSpawnRateMultiplier = 1f;

		[SerializeField]
		private AnimationCurve trailBubbleLifetimeVsProgress = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private AnimationCurve trailBubbleBoundaryRadiusVsProgress = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float trailBubbleLifetimeMultiplier = 6f;

		[SerializeField]
		private float trailDistanceBetweenSpawns = 3f;

		[SerializeField]
		private float trailMaxTurnAngle = 55f;

		[SerializeField]
		private float trailBubbleSize = 1.5f;

		[SerializeField]
		private AnimationCurve trailCountVsProgress = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float trailCountMultiplier = 12f;

		[SerializeField]
		private Vector2 trailEdgeAvoidanceSpawnsMinMax = new Vector2(3f, 1f);

		[Header("Feedback Effects")]
		[SerializeField]
		private float bubblePopAnticipationTime = 2f;

		[SerializeField]
		private float bubblePopWobbleFrequency = 25f;

		[SerializeField]
		private float bubblePopWobbleAmplitude = 0.01f;

		[SerializeField]
		private Transform liquidSurfacePlane;

		[SerializeField]
		private GuidedRefReceiverFieldInfo liquidSurfacePlane_gRef = new GuidedRefReceiverFieldInfo(true);

		private List<ScienceExperimentPlatformGenerator.BubbleData> activeBubbles = new List<ScienceExperimentPlatformGenerator.BubbleData>();

		private List<ScienceExperimentPlatformGenerator.BubbleData> trailHeads = new List<ScienceExperimentPlatformGenerator.BubbleData>();

		private List<ScienceExperimentPlatformGenerator.BubbleSpawnDebug> bubbleSpawnDebug = new List<ScienceExperimentPlatformGenerator.BubbleSpawnDebug>();

		private ScienceExperimentManager scienceExperimentManager;

		private struct BubbleData
		{
			public Vector3 position;

			public Vector3 direction;

			public float spawnSize;

			public float lifetime;

			public double spawnTime;

			public bool isTrail;

			public SodaBubble bubble;
		}

		private struct BubbleSpawnDebug
		{
			public Vector3 initialPosition;

			public Vector3 initialDirection;

			public Vector3 spawnPosition;

			public float minAngle;

			public float maxAngle;

			public float edgeCorrectionAngle;

			public double spawnTime;
		}
	}
}
