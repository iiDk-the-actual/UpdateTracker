using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class AOESender : MonoBehaviour
	{
		private void Awake()
		{
			if (this.hits == null || this.hits.Length != this.maxColliders)
			{
				this.hits = new Collider[Mathf.Max(8, this.maxColliders)];
			}
		}

		private void OnEnable()
		{
			if (this.applyOnEnable)
			{
				this.ApplyAOE();
			}
			this.nextTime = Time.time + this.repeatInterval;
		}

		private void Update()
		{
			if (this.repeatInterval > 0f && Time.time >= this.nextTime)
			{
				this.ApplyAOE();
				this.nextTime = Time.time + this.repeatInterval;
			}
		}

		public void ApplyAOE()
		{
			this.ApplyAOE(base.transform.position);
		}

		public void ApplyAOE(Vector3 worldOrigin)
		{
			this.visited.Clear();
			int num = Physics.OverlapSphereNonAlloc(worldOrigin, this.radius, this.hits, this.layerMask, this.triggerInteraction);
			float num2 = Mathf.Max(0.0001f, this.radius);
			for (int i = 0; i < num; i++)
			{
				Collider collider = this.hits[i];
				if (collider)
				{
					AOEReceiver componentInChildren = (collider.attachedRigidbody ? collider.attachedRigidbody.transform : collider.transform).GetComponentInChildren<AOEReceiver>(true);
					if (componentInChildren != null && this.TagValidation(componentInChildren.gameObject) && !this.visited.Contains(componentInChildren))
					{
						this.visited.Add(componentInChildren);
						float num3 = Vector3.Distance(worldOrigin, componentInChildren.transform.position);
						float num4 = Mathf.Clamp01(num3 / num2);
						float num5 = this.EvaluateFalloff(num4);
						float num6 = Mathf.Max(this.minStrength, this.strength * num5);
						AOEReceiver.AOEContext aoecontext = new AOEReceiver.AOEContext
						{
							origin = worldOrigin,
							radius = this.radius,
							instigator = base.gameObject,
							baseStrength = this.strength,
							finalStrength = num6,
							distance = num3,
							normalizedDistance = num4
						};
						componentInChildren.ReceiveAOE(in aoecontext);
					}
				}
			}
		}

		private float EvaluateFalloff(float t)
		{
			switch (this.falloffMode)
			{
			case AOESender.FalloffMode.None:
				return 1f;
			case AOESender.FalloffMode.Linear:
				return 1f - t;
			case AOESender.FalloffMode.AnimationCurve:
				return Mathf.Max(0f, this.falloffCurve.Evaluate(t));
			default:
				return 1f;
			}
		}

		private bool TagValidation(GameObject go)
		{
			if (go == null)
			{
				return false;
			}
			if (this.includeTags == null || this.includeTags.Length == 0)
			{
				return true;
			}
			string tag = go.tag;
			foreach (string text in this.includeTags)
			{
				if (!string.IsNullOrEmpty(text) && tag == text)
				{
					return true;
				}
			}
			return false;
		}

		[Min(0f)]
		[SerializeField]
		private float radius = 3f;

		[SerializeField]
		private LayerMask layerMask = -1;

		[SerializeField]
		private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

		[Tooltip("If empty, all AOEReceiver targets pass. If not empty, only receivers with these tags pass.")]
		[SerializeField]
		private string[] includeTags;

		[SerializeField]
		private AOESender.FalloffMode falloffMode = AOESender.FalloffMode.Linear;

		[SerializeField]
		private AnimationCurve falloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

		[Tooltip("Base strength before distance falloff.")]
		[SerializeField]
		private float strength = 1f;

		[Tooltip("Optional after falloff, applied as: max(minStrength, base*falloff).")]
		[SerializeField]
		private float minStrength;

		[SerializeField]
		private bool applyOnEnable;

		[Min(0f)]
		[SerializeField]
		private float repeatInterval;

		[SerializeField]
		[Tooltip("Max colliders captured per trigger/apply.")]
		private int maxColliders = 16;

		private Collider[] hits;

		private readonly HashSet<AOEReceiver> visited = new HashSet<AOEReceiver>();

		private float nextTime;

		private enum FalloffMode
		{
			None,
			Linear,
			AnimationCurve
		}
	}
}
