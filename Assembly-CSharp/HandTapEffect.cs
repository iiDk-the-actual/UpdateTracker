using System;
using GorillaTag;
using UnityEngine;
using UnityEngine.Events;

public class HandTapEffect : MonoBehaviour
{
	private void Awake()
	{
		VRRig componentInParent = base.GetComponentInParent<VRRig>();
		this.leftHandEffect.handContext = componentInParent.LeftHandEffect;
		this.rightHandEffect.handContext = componentInParent.RightHandEffect;
	}

	private void OnEnable()
	{
		this.leftHandEffect.OnEnable();
		this.rightHandEffect.OnEnable();
	}

	private void OnDisable()
	{
		this.leftHandEffect.OnDisable();
		this.rightHandEffect.OnDisable();
	}

	public HandTapEffect.HandTapEffectLeftRight leftHandEffect;

	public HandTapEffect.HandTapEffectLeftRight rightHandEffect;

	[Serializable]
	public class HandTapEffectDownUp
	{
		public bool HasOverrides
		{
			get
			{
				return this.overrides.overrideSurfacePrefab || this.overrides.overrideGamemodePrefab || this.overrides.overrideSound;
			}
		}

		internal void OnTap(HandEffectContext handContext)
		{
			UnityEvent unityEvent = this.onTapUnityEvents;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			for (int i = 0; i < this.onTapBehaviours.Length; i++)
			{
				this.onTapBehaviours[i].OnTap(handContext);
			}
		}

		public HandTapBehaviour[] onTapBehaviours;

		public UnityEvent onTapUnityEvents;

		[Tooltip("Must be in the global object pool and have a tag.\n\nPrefabs can have an FXModifier component to be adjusted after creation.")]
		public HashWrapper onTapPrefabToSpawn;

		public HandTapOverrides overrides;
	}

	[Serializable]
	public class HandTapEffectLeftRight
	{
		public void OnEnable()
		{
			if (this.separateUpTapCooldown)
			{
				this.handContext.SeparateUpTapCooldown = true;
			}
			if ((in this.downTapEffect.onTapPrefabToSpawn) != -1)
			{
				this.handContext.AddFXPrefab(in this.downTapEffect.onTapPrefabToSpawn);
			}
			if (this.downTapEffect.HasOverrides)
			{
				this.handContext.DownTapOverrides = this.downTapEffect.overrides;
			}
			if (this.upTapEffect.HasOverrides)
			{
				this.handContext.UpTapOverrides = this.upTapEffect.overrides;
			}
			this.handContext.handTapDown += this.downTapEffect.OnTap;
			this.handContext.handTapUp += this.upTapEffect.OnTap;
		}

		public void OnDisable()
		{
			if (this.separateUpTapCooldown)
			{
				this.handContext.SeparateUpTapCooldown = false;
			}
			if ((in this.downTapEffect.onTapPrefabToSpawn) != -1)
			{
				this.handContext.RemoveFXPrefab(in this.downTapEffect.onTapPrefabToSpawn);
			}
			if (this.downTapEffect.HasOverrides && this.handContext.DownTapOverrides == this.downTapEffect.overrides)
			{
				this.handContext.DownTapOverrides = null;
			}
			if (this.upTapEffect.HasOverrides && this.handContext.UpTapOverrides == this.upTapEffect.overrides)
			{
				this.handContext.UpTapOverrides = null;
			}
			this.handContext.handTapDown -= this.downTapEffect.OnTap;
			this.handContext.handTapUp -= this.upTapEffect.OnTap;
		}

		public bool separateUpTapCooldown;

		public HandTapEffect.HandTapEffectDownUp downTapEffect;

		public HandTapEffect.HandTapEffectDownUp upTapEffect;

		internal HandEffectContext handContext;
	}
}
