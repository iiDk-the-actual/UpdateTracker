using System;
using System.Collections.Generic;
using CjLib;
using GorillaLocomotion.Climbing;
using GorillaTag.GuidedRefs;
using GorillaTagScripts;
using GT_CustomMapSupportRuntime;
using UnityEngine;

namespace GorillaLocomotion.Swimming
{
	[RequireComponent(typeof(Collider))]
	public class WaterVolume : BaseGuidedRefTargetMono, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		public event WaterVolume.WaterVolumeEvent ColliderEnteredVolume;

		public event WaterVolume.WaterVolumeEvent ColliderExitedVolume;

		public event WaterVolume.WaterVolumeEvent ColliderEnteredWater;

		public event WaterVolume.WaterVolumeEvent ColliderExitedWater;

		public GTPlayer.LiquidType LiquidType
		{
			get
			{
				return this.liquidType;
			}
		}

		public WaterCurrent Current
		{
			get
			{
				return this.waterCurrent;
			}
		}

		public WaterParameters Parameters
		{
			get
			{
				return this.waterParams;
			}
		}

		private VRRig PlayerVRRig
		{
			get
			{
				if (this.playerVRRig == null)
				{
					GorillaTagger instance = GorillaTagger.Instance;
					if (instance != null)
					{
						this.playerVRRig = instance.offlineVRRig;
					}
				}
				return this.playerVRRig;
			}
		}

		public bool GetSurfaceQueryForPoint(Vector3 point, out WaterVolume.SurfaceQuery result, bool debugDraw = false)
		{
			result = default(WaterVolume.SurfaceQuery);
			if (!this.isStationary)
			{
				float num = float.MinValue;
				float num2 = float.MaxValue;
				for (int i = 0; i < this.volumeColliders.Count; i++)
				{
					float y = this.volumeColliders[i].bounds.max.y;
					float y2 = this.volumeColliders[i].bounds.min.y;
					if (y > num)
					{
						num = y;
					}
					if (y2 < num2)
					{
						num2 = y2;
					}
				}
				this.volumeMaxHeight = num;
				this.volumeMinHeight = num2;
			}
			Ray ray = new Ray(new Vector3(point.x, this.volumeMaxHeight, point.z), Vector3.down);
			Ray ray2 = new Ray(new Vector3(point.x, this.volumeMinHeight, point.z), Vector3.up);
			float num3 = this.volumeMaxHeight - this.volumeMinHeight;
			float num4 = float.MinValue;
			float num5 = float.MaxValue;
			bool flag = false;
			bool flag2 = false;
			float num6 = 0f;
			for (int j = 0; j < this.surfaceColliders.Count; j++)
			{
				bool enabled = this.surfaceColliders[j].enabled;
				this.surfaceColliders[j].enabled = true;
				RaycastHit raycastHit;
				if (this.surfaceColliders[j].Raycast(ray, out raycastHit, num3) && raycastHit.point.y > num4 && this.HitOutsideSurfaceOfMesh(ray.direction, this.surfaceColliders[j], raycastHit))
				{
					num4 = raycastHit.point.y;
					flag = true;
					result.surfacePoint = raycastHit.point;
					result.surfaceNormal = raycastHit.normal;
				}
				RaycastHit raycastHit2;
				if (this.surfaceColliders[j].Raycast(ray2, out raycastHit2, num3) && raycastHit2.point.y < num5 && this.HitOutsideSurfaceOfMesh(ray2.direction, this.surfaceColliders[j], raycastHit2))
				{
					num5 = raycastHit2.point.y;
					flag2 = true;
					num6 = raycastHit2.point.y;
				}
				this.surfaceColliders[j].enabled = enabled;
			}
			if (!flag && this.surfacePlane != null)
			{
				flag = true;
				result.surfacePoint = point - Vector3.Dot(point - this.surfacePlane.position, this.surfacePlane.up) * this.surfacePlane.up;
				result.surfaceNormal = this.surfacePlane.up;
			}
			if (flag && flag2)
			{
				result.maxDepth = result.surfacePoint.y - num6;
			}
			else if (flag)
			{
				result.maxDepth = result.surfacePoint.y - this.volumeMinHeight;
			}
			else
			{
				result.maxDepth = this.volumeMaxHeight - this.volumeMinHeight;
			}
			if (debugDraw)
			{
				if (flag)
				{
					DebugUtil.DrawLine(ray.origin, ray.origin + ray.direction * num3, Color.green, false);
					DebugUtil.DrawSphere(result.surfacePoint, 0.001f, 12, 12, Color.green, false, DebugUtil.Style.SolidColor);
				}
				else
				{
					DebugUtil.DrawLine(ray.origin, ray.origin + ray.direction * num3, Color.red, false);
				}
				if (flag2)
				{
					DebugUtil.DrawLine(ray2.origin, ray2.origin + ray2.direction * num3, Color.yellow, false);
					DebugUtil.DrawSphere(new Vector3(result.surfacePoint.x, num6, result.surfacePoint.z), 0.001f, 12, 12, Color.yellow, false, DebugUtil.Style.SolidColor);
				}
			}
			return flag;
		}

		private bool HitOutsideSurfaceOfMesh(Vector3 castDir, MeshCollider meshCollider, RaycastHit hit)
		{
			if (!WaterVolume.meshTrianglesDict.TryGetValue(meshCollider.sharedMesh, out this.sharedMeshTris))
			{
				this.sharedMeshTris = (int[])meshCollider.sharedMesh.triangles.Clone();
				WaterVolume.meshTrianglesDict.Add(meshCollider.sharedMesh, this.sharedMeshTris);
			}
			if (!WaterVolume.meshVertsDict.TryGetValue(meshCollider.sharedMesh, out this.sharedMeshVerts))
			{
				this.sharedMeshVerts = (Vector3[])meshCollider.sharedMesh.vertices.Clone();
				WaterVolume.meshVertsDict.Add(meshCollider.sharedMesh, this.sharedMeshVerts);
			}
			Vector3 vector = this.sharedMeshVerts[this.sharedMeshTris[hit.triangleIndex * 3]];
			Vector3 vector2 = this.sharedMeshVerts[this.sharedMeshTris[hit.triangleIndex * 3 + 1]];
			Vector3 vector3 = this.sharedMeshVerts[this.sharedMeshTris[hit.triangleIndex * 3 + 2]];
			Vector3 vector4 = meshCollider.transform.TransformDirection(Vector3.Cross(vector2 - vector, vector3 - vector).normalized);
			bool flag = Vector3.Dot(castDir, vector4) < 0f;
			if (this.debugDrawSurfaceCast)
			{
				Color color = (flag ? Color.blue : Color.red);
				DebugUtil.DrawLine(hit.point, hit.point + vector4 * 0.3f, color, false);
			}
			return flag;
		}

		private void DebugDrawMeshColliderHitTriangle(RaycastHit hit)
		{
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider != null)
			{
				Mesh sharedMesh = meshCollider.sharedMesh;
				int[] triangles = sharedMesh.triangles;
				Vector3[] vertices = sharedMesh.vertices;
				Vector3 vector = meshCollider.gameObject.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3]]);
				Vector3 vector2 = meshCollider.gameObject.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]]);
				Vector3 vector3 = meshCollider.gameObject.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]]);
				Vector3 normalized = Vector3.Cross(vector2 - vector, vector3 - vector).normalized;
				float num = 0.2f;
				DebugUtil.DrawLine(vector, vector + normalized * num, Color.blue, false);
				DebugUtil.DrawLine(vector2, vector2 + normalized * num, Color.blue, false);
				DebugUtil.DrawLine(vector3, vector3 + normalized * num, Color.blue, false);
				DebugUtil.DrawLine(vector, vector2, Color.blue, false);
				DebugUtil.DrawLine(vector, vector3, Color.blue, false);
				DebugUtil.DrawLine(vector2, vector3, Color.blue, false);
			}
		}

		public bool RaycastWater(Vector3 origin, Vector3 direction, out RaycastHit hit, float distance, int layerMask)
		{
			if (this.triggerCollider != null)
			{
				return Physics.Raycast(new Ray(origin, direction), out hit, distance, layerMask, QueryTriggerInteraction.Collide);
			}
			hit = default(RaycastHit);
			return false;
		}

		public bool CheckColliderInVolume(Collider collider, out bool inWater, out bool surfaceDetected)
		{
			for (int i = 0; i < this.persistentColliders.Count; i++)
			{
				if (this.persistentColliders[i].collider == collider)
				{
					inWater = this.persistentColliders[i].inWater;
					surfaceDetected = this.persistentColliders[i].surfaceDetected;
					return true;
				}
			}
			inWater = false;
			surfaceDetected = false;
			return false;
		}

		protected override void Awake()
		{
			base.Awake();
			this.RefreshColliders();
		}

		private void OnEnable()
		{
			TickSystem<object>.AddTickCallback(this);
		}

		public void RefreshColliders()
		{
			this.triggerCollider = base.GetComponent<Collider>();
			if (this.volumeColliders == null || this.volumeColliders.Count < 1)
			{
				this.volumeColliders = new List<Collider>();
				this.volumeColliders.Add(base.gameObject.GetComponent<Collider>());
			}
			float num = float.MinValue;
			float num2 = float.MaxValue;
			for (int i = 0; i < this.volumeColliders.Count; i++)
			{
				float y = this.volumeColliders[i].bounds.max.y;
				float y2 = this.volumeColliders[i].bounds.min.y;
				if (y > num)
				{
					num = y;
				}
				if (y2 < num2)
				{
					num2 = y2;
				}
			}
			this.volumeMaxHeight = num;
			this.volumeMinHeight = num2;
		}

		private void OnDisable()
		{
			if (ApplicationQuittingState.IsQuitting)
			{
				return;
			}
			for (int i = 0; i < this.persistentColliders.Count; i++)
			{
				WaterOverlappingCollider waterOverlappingCollider = this.persistentColliders[i];
				waterOverlappingCollider.inVolume = false;
				waterOverlappingCollider.playDripEffect = false;
				WaterVolume.WaterVolumeEvent colliderExitedVolume = this.ColliderExitedVolume;
				if (colliderExitedVolume != null)
				{
					colliderExitedVolume(this, waterOverlappingCollider.collider);
				}
				this.persistentColliders[i] = waterOverlappingCollider;
			}
			this.RemoveCollidersOutsideVolume(Time.time);
			TickSystem<object>.RemoveTickCallback(this);
		}

		public void Tick()
		{
			if (this.persistentColliders.Count < 1)
			{
				return;
			}
			float time = Time.time;
			this.RemoveCollidersOutsideVolume(time);
			if (!this.CanPlayerSwim())
			{
				return;
			}
			for (int i = 0; i < this.persistentColliders.Count; i++)
			{
				WaterOverlappingCollider waterOverlappingCollider = this.persistentColliders[i];
				bool inWater = waterOverlappingCollider.inWater;
				if (waterOverlappingCollider.inVolume)
				{
					this.CheckColliderAgainstWater(ref waterOverlappingCollider, time);
				}
				else
				{
					waterOverlappingCollider.inWater = false;
				}
				this.TryRegisterOwnershipOfCollider(waterOverlappingCollider.collider, waterOverlappingCollider.inWater, waterOverlappingCollider.surfaceDetected);
				if (waterOverlappingCollider.inWater && !inWater)
				{
					this.OnWaterSurfaceEnter(ref waterOverlappingCollider);
				}
				else if (!waterOverlappingCollider.inWater && inWater)
				{
					this.OnWaterSurfaceExit(ref waterOverlappingCollider, time);
				}
				if (this.HasOwnershipOfCollider(waterOverlappingCollider.collider) && waterOverlappingCollider.surfaceDetected)
				{
					if (!waterOverlappingCollider.inWater)
					{
						this.ColliderOutOfWaterUpdate(ref waterOverlappingCollider, time);
					}
					else
					{
						this.ColliderInWaterUpdate(ref waterOverlappingCollider, time);
					}
				}
				this.persistentColliders[i] = waterOverlappingCollider;
			}
		}

		private void RemoveCollidersOutsideVolume(float currentTime)
		{
			if (ApplicationQuittingState.IsQuitting)
			{
				return;
			}
			for (int i = this.persistentColliders.Count - 1; i >= 0; i--)
			{
				WaterOverlappingCollider waterOverlappingCollider = this.persistentColliders[i];
				if (waterOverlappingCollider.collider == null || !waterOverlappingCollider.collider.gameObject.activeInHierarchy || (!waterOverlappingCollider.inVolume && (!waterOverlappingCollider.playDripEffect || currentTime - waterOverlappingCollider.lastInWaterTime > this.waterParams.postExitDripDuration)) || !this.CanPlayerSwim())
				{
					this.UnregisterOwnershipOfCollider(waterOverlappingCollider.collider);
					GTPlayer instance = GTPlayer.Instance;
					if (waterOverlappingCollider.collider == instance.headCollider || waterOverlappingCollider.collider == instance.bodyCollider)
					{
						instance.OnExitWaterVolume(waterOverlappingCollider.collider, this);
					}
					this.persistentColliders.RemoveAt(i);
				}
			}
		}

		private void CheckColliderAgainstWater(ref WaterOverlappingCollider persistentCollider, float currentTime)
		{
			Vector3 position = persistentCollider.collider.transform.position;
			bool flag = true;
			if (persistentCollider.surfaceDetected && persistentCollider.scaleMultiplier > 0.99f && this.isStationary)
			{
				flag = (position - Vector3.Dot(position - persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal) * persistentCollider.lastSurfaceQuery.surfaceNormal - persistentCollider.lastSurfaceQuery.surfacePoint).sqrMagnitude > this.waterParams.recomputeSurfaceForColliderDist * this.waterParams.recomputeSurfaceForColliderDist;
			}
			if (flag)
			{
				WaterVolume.SurfaceQuery surfaceQuery;
				if (this.GetSurfaceQueryForPoint(position, out surfaceQuery, this.debugDrawSurfaceCast))
				{
					persistentCollider.surfaceDetected = true;
					persistentCollider.lastSurfaceQuery = surfaceQuery;
				}
				else
				{
					persistentCollider.surfaceDetected = false;
					persistentCollider.lastSurfaceQuery = default(WaterVolume.SurfaceQuery);
				}
			}
			if (persistentCollider.surfaceDetected)
			{
				bool flag2 = ((persistentCollider.collider is MeshCollider) ? persistentCollider.collider.ClosestPointOnBounds(position + Vector3.down * 10f) : persistentCollider.collider.ClosestPoint(position + Vector3.down * 10f)).y < persistentCollider.lastSurfaceQuery.surfacePoint.y;
				bool flag3 = ((persistentCollider.collider is MeshCollider) ? persistentCollider.collider.ClosestPointOnBounds(position + Vector3.up * 10f) : persistentCollider.collider.ClosestPoint(position + Vector3.up * 10f)).y > persistentCollider.lastSurfaceQuery.surfacePoint.y - persistentCollider.lastSurfaceQuery.maxDepth;
				persistentCollider.inWater = flag2 && flag3;
			}
			else
			{
				persistentCollider.inWater = false;
			}
			if (persistentCollider.inWater)
			{
				persistentCollider.lastInWaterTime = currentTime;
			}
		}

		private Vector3 GetColliderVelocity(ref WaterOverlappingCollider persistentCollider)
		{
			GTPlayer instance = GTPlayer.Instance;
			Vector3 vector = Vector3.one * (this.waterParams.splashSpeedRequirement + 0.1f);
			if (persistentCollider.velocityTracker != null)
			{
				vector = persistentCollider.velocityTracker.GetAverageVelocity(true, 0.1f, false);
			}
			else if (persistentCollider.collider == instance.headCollider || persistentCollider.collider == instance.bodyCollider)
			{
				vector = instance.AveragedVelocity;
			}
			else if (persistentCollider.collider.attachedRigidbody != null && !persistentCollider.collider.attachedRigidbody.isKinematic)
			{
				vector = persistentCollider.collider.attachedRigidbody.linearVelocity;
			}
			return vector;
		}

		private void OnWaterSurfaceEnter(ref WaterOverlappingCollider persistentCollider)
		{
			WaterVolume.WaterVolumeEvent colliderEnteredWater = this.ColliderEnteredWater;
			if (colliderEnteredWater != null)
			{
				colliderEnteredWater(this, persistentCollider.collider);
			}
			GTPlayer instance = GTPlayer.Instance;
			if (persistentCollider.collider == instance.headCollider || persistentCollider.collider == instance.bodyCollider)
			{
				instance.OnEnterWaterVolume(persistentCollider.collider, this);
			}
			if (this.HasOwnershipOfCollider(persistentCollider.collider))
			{
				Vector3 colliderVelocity = this.GetColliderVelocity(ref persistentCollider);
				bool flag = Vector3.Dot(colliderVelocity, -persistentCollider.lastSurfaceQuery.surfaceNormal) > this.waterParams.splashSpeedRequirement * persistentCollider.scaleMultiplier;
				bool flag2 = Vector3.Dot(colliderVelocity, -persistentCollider.lastSurfaceQuery.surfaceNormal) > this.waterParams.bigSplashSpeedRequirement * persistentCollider.scaleMultiplier;
				persistentCollider.PlayRippleEffect(this.waterParams.rippleEffect, persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal, this.waterParams.rippleEffectScale, Time.time, this);
				if (this.waterParams.playSplashEffect && flag && (flag2 || !persistentCollider.playBigSplash))
				{
					persistentCollider.PlaySplashEffect(this.waterParams.splashEffect, persistentCollider.lastRipplePosition, this.waterParams.splashEffectScale, persistentCollider.playBigSplash && flag2, true, this);
				}
			}
		}

		private void OnWaterSurfaceExit(ref WaterOverlappingCollider persistentCollider, float currentTime)
		{
			WaterVolume.WaterVolumeEvent colliderExitedWater = this.ColliderExitedWater;
			if (colliderExitedWater != null)
			{
				colliderExitedWater(this, persistentCollider.collider);
			}
			persistentCollider.nextDripTime = currentTime + this.waterParams.perDripTimeDelay + Random.Range(-this.waterParams.perDripTimeRandRange * 0.5f, this.waterParams.perDripTimeRandRange * 0.5f);
			GTPlayer instance = GTPlayer.Instance;
			if (persistentCollider.collider == instance.headCollider || persistentCollider.collider == instance.bodyCollider)
			{
				instance.OnExitWaterVolume(persistentCollider.collider, this);
			}
			if (this.HasOwnershipOfCollider(persistentCollider.collider))
			{
				float num = Vector3.Dot(this.GetColliderVelocity(ref persistentCollider), persistentCollider.lastSurfaceQuery.surfaceNormal);
				bool flag = num > this.waterParams.splashSpeedRequirement * persistentCollider.scaleMultiplier;
				bool flag2 = num > this.waterParams.bigSplashSpeedRequirement * persistentCollider.scaleMultiplier;
				persistentCollider.PlayRippleEffect(this.waterParams.rippleEffect, persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal, this.waterParams.rippleEffectScale, Time.time, this);
				if (this.waterParams.playSplashEffect && flag && (flag2 || !persistentCollider.playBigSplash))
				{
					persistentCollider.PlaySplashEffect(this.waterParams.splashEffect, persistentCollider.lastRipplePosition, this.waterParams.splashEffectScale, persistentCollider.playBigSplash && flag2, false, this);
				}
			}
		}

		private void ColliderOutOfWaterUpdate(ref WaterOverlappingCollider persistentCollider, float currentTime)
		{
			if (currentTime < persistentCollider.lastInWaterTime + this.waterParams.postExitDripDuration && currentTime > persistentCollider.nextDripTime && persistentCollider.playDripEffect)
			{
				persistentCollider.nextDripTime = currentTime + this.waterParams.perDripTimeDelay + Random.Range(-this.waterParams.perDripTimeRandRange * 0.5f, this.waterParams.perDripTimeRandRange * 0.5f);
				float num = this.waterParams.rippleEffectScale * 2f * (this.waterParams.perDripDefaultRadius + Random.Range(-this.waterParams.perDripRadiusRandRange * 0.5f, this.waterParams.perDripRadiusRandRange * 0.5f));
				persistentCollider.PlayDripEffect(this.waterParams.rippleEffect, persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal, num);
			}
		}

		private void ColliderInWaterUpdate(ref WaterOverlappingCollider persistentCollider, float currentTime)
		{
			Vector3 vector = Vector3.ProjectOnPlane(persistentCollider.collider.transform.position - persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal) + persistentCollider.lastSurfaceQuery.surfacePoint;
			bool flag;
			if (persistentCollider.overrideBoundingRadius)
			{
				flag = (persistentCollider.collider.transform.position - vector).sqrMagnitude < persistentCollider.boundingRadiusOverride * persistentCollider.boundingRadiusOverride;
			}
			else
			{
				flag = (persistentCollider.collider.ClosestPointOnBounds(vector) - vector).sqrMagnitude < 0.001f;
			}
			if (flag)
			{
				float num = Mathf.Max(this.waterParams.minDistanceBetweenRipples, this.waterParams.defaultDistanceBetweenRipples * (persistentCollider.lastRippleScale / this.waterParams.rippleEffectScale));
				bool flag2 = (persistentCollider.lastRipplePosition - vector).sqrMagnitude > num * num;
				bool flag3 = currentTime - persistentCollider.lastRippleTime > this.waterParams.minTimeBetweenRipples;
				if (flag2 || flag3)
				{
					persistentCollider.PlayRippleEffect(this.waterParams.rippleEffect, persistentCollider.lastSurfaceQuery.surfacePoint, persistentCollider.lastSurfaceQuery.surfaceNormal, this.waterParams.rippleEffectScale, currentTime, this);
					return;
				}
			}
			else
			{
				persistentCollider.lastRippleTime = currentTime;
			}
		}

		private void TryRegisterOwnershipOfCollider(Collider collider, bool isInWater, bool isSurfaceDetected)
		{
			WaterVolume waterVolume;
			if (WaterVolume.sharedColliderRegistry.TryGetValue(collider, out waterVolume))
			{
				if (waterVolume != this)
				{
					bool flag;
					bool flag2;
					waterVolume.CheckColliderInVolume(collider, out flag, out flag2);
					if ((isSurfaceDetected && !flag2) || (isInWater && !flag))
					{
						WaterVolume.sharedColliderRegistry.Remove(collider);
						WaterVolume.sharedColliderRegistry.Add(collider, this);
						return;
					}
				}
			}
			else
			{
				WaterVolume.sharedColliderRegistry.Add(collider, this);
			}
		}

		private void UnregisterOwnershipOfCollider(Collider collider)
		{
			if (WaterVolume.sharedColliderRegistry.ContainsKey(collider))
			{
				WaterVolume.sharedColliderRegistry.Remove(collider);
			}
		}

		private bool HasOwnershipOfCollider(Collider collider)
		{
			WaterVolume waterVolume;
			return WaterVolume.sharedColliderRegistry.TryGetValue(collider, out waterVolume) && waterVolume == this;
		}

		protected virtual bool CanPlayerSwim()
		{
			if (this.isMonkeblock && this.PlayerVRRig != null)
			{
				if (this.PlayerVRRig.scaleFactor < 0.5f)
				{
					return true;
				}
				BuilderTable builderTable;
				if (BuilderTable.TryGetBuilderTableForZone(this.PlayerVRRig.zoneEntity.currentZone, out builderTable))
				{
					return !builderTable.isTableMutable;
				}
			}
			return true;
		}

		public void OnTriggerEnter(Collider other)
		{
			if (!this.CanPlayerSwim())
			{
				return;
			}
			GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (other.isTrigger && component == null)
			{
				return;
			}
			WaterVolume.WaterVolumeEvent colliderEnteredVolume = this.ColliderEnteredVolume;
			if (colliderEnteredVolume != null)
			{
				colliderEnteredVolume(this, other);
			}
			for (int i = 0; i < this.persistentColliders.Count; i++)
			{
				if (this.persistentColliders[i].collider == other)
				{
					WaterOverlappingCollider waterOverlappingCollider = this.persistentColliders[i];
					waterOverlappingCollider.inVolume = true;
					this.persistentColliders[i] = waterOverlappingCollider;
					return;
				}
			}
			WaterOverlappingCollider waterOverlappingCollider2 = new WaterOverlappingCollider
			{
				collider = other
			};
			waterOverlappingCollider2.inVolume = true;
			waterOverlappingCollider2.lastInWaterTime = Time.time - this.waterParams.postExitDripDuration - 10f;
			WaterSplashOverride component2 = other.GetComponent<WaterSplashOverride>();
			if (component2 != null)
			{
				if (component2.suppressWaterEffects)
				{
					return;
				}
				waterOverlappingCollider2.playBigSplash = component2.playBigSplash;
				waterOverlappingCollider2.playDripEffect = component2.playDrippingEffect;
				waterOverlappingCollider2.overrideBoundingRadius = component2.overrideBoundingRadius;
				waterOverlappingCollider2.boundingRadiusOverride = component2.boundingRadiusOverride;
				waterOverlappingCollider2.scaleMultiplier = (component2.scaleByPlayersScale ? GTPlayer.Instance.scale : 1f);
			}
			else
			{
				if (other.GetComponent<BuilderPieceCollider>() != null)
				{
					return;
				}
				waterOverlappingCollider2.playDripEffect = true;
				waterOverlappingCollider2.overrideBoundingRadius = false;
				waterOverlappingCollider2.scaleMultiplier = 1f;
				waterOverlappingCollider2.playBigSplash = false;
			}
			GTPlayer instance = GTPlayer.Instance;
			if (component != null)
			{
				waterOverlappingCollider2.velocityTracker = instance.GetHandVelocityTracker(component.isLeftHand);
				waterOverlappingCollider2.scaleMultiplier = instance.scale;
			}
			else
			{
				waterOverlappingCollider2.velocityTracker = other.GetComponent<GorillaVelocityTracker>();
			}
			if (this.PlayerVRRig != null && this.waterParams.sendSplashEffectRPCs && (component != null || waterOverlappingCollider2.collider == instance.headCollider || waterOverlappingCollider2.collider == instance.bodyCollider))
			{
				waterOverlappingCollider2.photonViewForRPC = this.PlayerVRRig.netView;
			}
			this.persistentColliders.Add(waterOverlappingCollider2);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!this.CanPlayerSwim())
			{
				return;
			}
			GorillaTriggerColliderHandIndicator component = other.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (other.isTrigger && component == null)
			{
				return;
			}
			WaterVolume.WaterVolumeEvent colliderExitedVolume = this.ColliderExitedVolume;
			if (colliderExitedVolume != null)
			{
				colliderExitedVolume(this, other);
			}
			for (int i = 0; i < this.persistentColliders.Count; i++)
			{
				if (this.persistentColliders[i].collider == other)
				{
					WaterOverlappingCollider waterOverlappingCollider = this.persistentColliders[i];
					waterOverlappingCollider.inVolume = false;
					this.persistentColliders[i] = waterOverlappingCollider;
				}
			}
		}

		public void SetPropertiesFromPlaceholder(WaterVolumeProperties properties, List<Collider> waterVolumeColliders, WaterParameters parameters)
		{
			this.surfacePlane = properties.surfacePlane;
			this.surfaceColliders = properties.surfaceColliders;
			this.volumeColliders = waterVolumeColliders;
			this.liquidType = (GTPlayer.LiquidType)Math.Clamp(properties.liquidType - CMSZoneShaderSettings.EZoneLiquidType.Water, 0, 1);
			this.waterParams = parameters;
		}

		[SerializeField]
		public Transform surfacePlane;

		[SerializeField]
		private List<MeshCollider> surfaceColliders = new List<MeshCollider>();

		[SerializeField]
		public List<Collider> volumeColliders = new List<Collider>();

		[SerializeField]
		private GTPlayer.LiquidType liquidType;

		[SerializeField]
		private WaterCurrent waterCurrent;

		[SerializeField]
		private WaterParameters waterParams;

		[SerializeField]
		[Tooltip("The water volume be placed in the scene (not spawned) and not moved for this to be true")]
		public bool isStationary = true;

		[SerializeField]
		[Tooltip("Check scale of monke entering")]
		public bool isMonkeblock;

		public const string WaterSplashRPC = "RPC_PlaySplashEffect";

		public static float[] splashRPCSendTimes = new float[4];

		private static Dictionary<Collider, WaterVolume> sharedColliderRegistry = new Dictionary<Collider, WaterVolume>(16);

		private static Dictionary<Mesh, int[]> meshTrianglesDict = new Dictionary<Mesh, int[]>(16);

		private static Dictionary<Mesh, Vector3[]> meshVertsDict = new Dictionary<Mesh, Vector3[]>(16);

		private int[] sharedMeshTris;

		private Vector3[] sharedMeshVerts;

		private VRRig playerVRRig;

		private float volumeMaxHeight;

		private float volumeMinHeight;

		private bool debugDrawSurfaceCast;

		private Collider triggerCollider;

		private List<WaterOverlappingCollider> persistentColliders = new List<WaterOverlappingCollider>(16);

		private GuidedRefTargetIdSO _guidedRefTargetId;

		private Object _guidedRefTargetObject;

		public struct SurfaceQuery
		{
			public Plane surfacePlane
			{
				get
				{
					return new Plane(this.surfaceNormal, this.surfacePoint);
				}
			}

			public Vector3 surfacePoint;

			public Vector3 surfaceNormal;

			public float maxDepth;
		}

		public delegate void WaterVolumeEvent(WaterVolume volume, Collider collider);
	}
}
