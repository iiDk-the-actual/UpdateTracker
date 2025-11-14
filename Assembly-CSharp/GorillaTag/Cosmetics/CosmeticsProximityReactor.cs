using System;
using System.Collections.Generic;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	public class CosmeticsProximityReactor : MonoBehaviour, ISpawnable
	{
		public bool IsMatched { get; set; }

		private VRRig MyRig { get; set; }

		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public bool IsBelow
		{
			get
			{
				using (List<CosmeticsProximityReactor.InteractionSetting>.Enumerator enumerator = this.blocks.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.wasBelow)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public void OnSpawn(VRRig rig)
		{
			if (this.MyRig == null)
			{
				this.MyRig = rig;
			}
		}

		public void OnDespawn()
		{
		}

		private void Start()
		{
			this.IsMatched = false;
			if (CosmeticsProximityReactorManager.Instance != null)
			{
				CosmeticsProximityReactorManager.Instance.Register(this);
			}
		}

		private void OnEnable()
		{
			if (this.MyRig == null)
			{
				this.MyRig = base.GetComponentInParent<VRRig>();
			}
			if (CosmeticsProximityReactorManager.Instance != null)
			{
				CosmeticsProximityReactorManager.Instance.Register(this);
			}
		}

		private void OnDisable()
		{
			if (CosmeticsProximityReactorManager.Instance)
			{
				CosmeticsProximityReactorManager.Instance.Unregister(this);
			}
		}

		public IReadOnlyList<string> GetTypes()
		{
			List<string> sharedKeysCache = CosmeticsProximityReactorManager.SharedKeysCache;
			sharedKeysCache.Clear();
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.interactionKeys != null && interactionSetting.interactionKeys.Count != 0)
				{
					foreach (string text in interactionSetting.interactionKeys)
					{
						if (!string.IsNullOrEmpty(text) && !sharedKeysCache.Contains(text))
						{
							sharedKeysCache.Add(text);
						}
					}
				}
			}
			return sharedKeysCache;
		}

		public bool IsGorillaBody()
		{
			return this.itemKind == CosmeticsProximityReactor.ItemKind.GorillaBody;
		}

		public bool IsCosmeticItem()
		{
			return this.itemKind == CosmeticsProximityReactor.ItemKind.Cosmetic;
		}

		public bool AcceptsAnySource()
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.gorillaBodyMask != CosmeticsProximityReactor.GorillaBodyPart.None)
				{
					return true;
				}
			}
			return false;
		}

		public bool AcceptsThisSource(CosmeticsProximityReactor.GorillaBodyPart kind)
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.AcceptsGorillaBodyPart(kind))
				{
					return true;
				}
			}
			return false;
		}

		public float GetCosmeticPairThresholdWith(CosmeticsProximityReactor other, out bool any)
		{
			any = false;
			float num = float.MaxValue;
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.AllowsRig(this.MyRig, other.MyRig))
				{
					foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting2 in other.blocks)
					{
						if (interactionSetting2.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting2.AllowsRig(other.MyRig, this.MyRig) && interactionSetting.SharesKeyWith(interactionSetting2))
						{
							any = true;
							float num2 = Mathf.Min(interactionSetting.proximityThreshold, interactionSetting2.proximityThreshold);
							if (num2 < num)
							{
								num = num2;
							}
						}
					}
				}
			}
			return num;
		}

		public float GetSourceThresholdFor(CosmeticsProximityReactor gorillaBody, out bool any)
		{
			any = false;
			float num = float.MaxValue;
			CosmeticsProximityReactor.GorillaBodyPart gorillaBodyPart = gorillaBody.gorillaBodyParts;
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.AcceptsGorillaBodyPart(gorillaBodyPart) && interactionSetting.AllowsRig(this.MyRig, gorillaBody.MyRig))
				{
					any = true;
					if (interactionSetting.proximityThreshold < num)
					{
						num = interactionSetting.proximityThreshold;
					}
				}
			}
			return num;
		}

		public void OnCosmeticBelowWith(CosmeticsProximityReactor other, Vector3 contact)
		{
			float time = Time.time;
			bool flag = false;
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.AllowsRig(this.MyRig, other.MyRig))
				{
					bool flag2 = false;
					foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting2 in other.blocks)
					{
						if (interactionSetting2.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting2.AllowsRig(other.MyRig, this.MyRig) && interactionSetting.SharesKeyWith(interactionSetting2))
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						interactionSetting.FireBelow(this.MyRig, contact, time);
						if (interactionSetting.wasBelow)
						{
							interactionSetting.isMatched = true;
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				this.IsMatched = true;
			}
		}

		public void WhileCosmeticBelowWith(CosmeticsProximityReactor other, Vector3 contact)
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.isMatched && interactionSetting.AllowsRig(this.MyRig, other.MyRig))
				{
					bool flag = false;
					foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting2 in other.blocks)
					{
						if (interactionSetting2.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting2.AllowsRig(other.MyRig, this.MyRig) && interactionSetting.SharesKeyWith(interactionSetting2))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						interactionSetting.FireWhile(this.MyRig, contact);
					}
				}
			}
		}

		public void OnCosmeticAboveAll()
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.isMatched)
				{
					interactionSetting.FireAbove(this.MyRig);
				}
			}
			this.RefreshAggregateMatched();
		}

		public void OnSourceBelow(Vector3 contact, CosmeticsProximityReactor.GorillaBodyPart kind, VRRig sourceRig)
		{
			float time = Time.time;
			bool flag = false;
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.AcceptsGorillaBodyPart(kind) && interactionSetting.AllowsRig(this.MyRig, sourceRig))
				{
					interactionSetting.FireBelow(this.MyRig, contact, time);
					if (interactionSetting.wasBelow)
					{
						interactionSetting.isMatched = true;
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.RefreshAggregateMatched();
			}
		}

		public void WhileSourceBelow(Vector3 contact, CosmeticsProximityReactor.GorillaBodyPart kind, VRRig sourceRig)
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.AcceptsGorillaBodyPart(kind) && interactionSetting.isMatched && interactionSetting.AllowsRig(this.MyRig, sourceRig))
				{
					interactionSetting.FireWhile(this.MyRig, contact);
				}
			}
		}

		public void OnSourceAboveAll()
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.isMatched)
				{
					interactionSetting.FireAbove(this.MyRig);
				}
			}
			this.RefreshAggregateMatched();
		}

		public bool HasAnyCosmeticMatch()
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic && interactionSetting.isMatched)
				{
					return true;
				}
			}
			return false;
		}

		private bool HasAnyGorillaBodyPartMatch()
		{
			foreach (CosmeticsProximityReactor.InteractionSetting interactionSetting in this.blocks)
			{
				if (interactionSetting.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && interactionSetting.isMatched)
				{
					return true;
				}
			}
			return false;
		}

		public void RefreshAggregateMatched()
		{
			this.IsMatched = this.HasAnyCosmeticMatch() || this.HasAnyGorillaBodyPartMatch();
		}

		[Tooltip("Is this object a Cosmetic or a gorilla body part like hand? (gorilla body slot is reserved for Gorilla Player Networked)")]
		public CosmeticsProximityReactor.ItemKind itemKind;

		[FormerlySerializedAs("sourceKinds")]
		public CosmeticsProximityReactor.GorillaBodyPart gorillaBodyParts;

		public List<CosmeticsProximityReactor.InteractionSetting> blocks = new List<CosmeticsProximityReactor.InteractionSetting>();

		[Tooltip("If enabled, this cosmetic ignores other instances that share the same PlayFabID.")]
		public bool ignoreSameCosmeticInstances;

		public string PlayFabID = "";

		[Tooltip("If collider is not assigned, we will use the position of this object to find the distance between two cosmetic/body part")]
		public Collider collider;

		private RubberDuckEvents _events;

		public enum ItemKind
		{
			Cosmetic,
			GorillaBody
		}

		[Flags]
		public enum GorillaBodyPart
		{
			None = 0,
			HandLeft = 1,
			HandRight = 2,
			Mouth = 4
		}

		public enum InteractionMode
		{
			CosmeticToCosmetic,
			GorillaBodyToCosmetic
		}

		public enum TargetType
		{
			Owner,
			Others,
			All
		}

		[Serializable]
		public class InteractionSetting
		{
			public bool IsCosmeticToCosmetic()
			{
				return this.mode == CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic;
			}

			public bool IsGorillaBodyToCosmetic()
			{
				return this.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic;
			}

			public bool AcceptsGorillaBodyPart(CosmeticsProximityReactor.GorillaBodyPart kind)
			{
				return this.mode == CosmeticsProximityReactor.InteractionMode.GorillaBodyToCosmetic && (this.gorillaBodyMask & kind) > CosmeticsProximityReactor.GorillaBodyPart.None;
			}

			public bool SharesKeyWith(CosmeticsProximityReactor.InteractionSetting other)
			{
				if (this.mode != CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic)
				{
					return false;
				}
				if (other == null)
				{
					return false;
				}
				if (other.mode != CosmeticsProximityReactor.InteractionMode.CosmeticToCosmetic)
				{
					return false;
				}
				if (this.interactionKeys == null || other.interactionKeys == null)
				{
					return false;
				}
				foreach (string text in this.interactionKeys)
				{
					if (!string.IsNullOrEmpty(text) && other.interactionKeys.Contains(text))
					{
						return true;
					}
				}
				return false;
			}

			public bool CanPlay(float now)
			{
				return now - this.lastEffectTime >= this.cooldownTime;
			}

			public void FireBelow(VRRig rig, Vector3 contact, float now)
			{
				if (!this.wasBelow && this.CanPlay(now))
				{
					if (rig != null && rig.isLocal)
					{
						UnityEvent<Vector3> unityEvent = this.onBelowLocal;
						if (unityEvent != null)
						{
							unityEvent.Invoke(contact);
						}
					}
					UnityEvent<Vector3> unityEvent2 = this.onBelowShared;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke(contact);
					}
					this.wasBelow = true;
					this.lastEffectTime = now;
				}
			}

			public void FireWhile(VRRig rig, Vector3 contact)
			{
				if (rig != null && rig.isLocal)
				{
					UnityEvent<Vector3> unityEvent = this.whileBelowLocal;
					if (unityEvent != null)
					{
						unityEvent.Invoke(contact);
					}
				}
				UnityEvent<Vector3> unityEvent2 = this.whileBelowShared;
				if (unityEvent2 == null)
				{
					return;
				}
				unityEvent2.Invoke(contact);
			}

			public void FireAbove(VRRig rig)
			{
				if (this.wasBelow)
				{
					if (rig != null && rig.isLocal)
					{
						UnityEvent unityEvent = this.onAboveLocal;
						if (unityEvent != null)
						{
							unityEvent.Invoke();
						}
					}
					UnityEvent unityEvent2 = this.onAboveShared;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke();
					}
					this.wasBelow = false;
					this.isMatched = false;
				}
			}

			public bool AllowsRig(VRRig myRig, VRRig otherRig)
			{
				if (myRig == null || otherRig == null)
				{
					return true;
				}
				switch (this.targetType)
				{
				case CosmeticsProximityReactor.TargetType.Owner:
					return myRig == otherRig;
				case CosmeticsProximityReactor.TargetType.Others:
					return myRig != otherRig;
				}
				return true;
			}

			[Tooltip("Determines what type of interaction this block handles.\n• CosmeticToCosmetic: triggers when two cosmetics with matching keys are nearby.\n• GorillaBodyToCosmetic: triggers when a Gorilla body part (hand, head, etc.) is near this cosmetic.")]
			public CosmeticsProximityReactor.InteractionMode mode;

			[Tooltip("List of shared string identifiers that link this cosmetic to others.\nCosmetics with matching keys can trigger interactions with each other.")]
			public List<string> interactionKeys = new List<string>();

			[Tooltip("Specifies which Gorilla body parts (e.g., Hands, Head) can trigger this interaction.\nUse this when the Mode is set to GorillaBodyToCosmetic.")]
			public CosmeticsProximityReactor.GorillaBodyPart gorillaBodyMask;

			[Tooltip("The distance threshold (in meters) for triggering the interaction.\nIf another object enters this range, the OnBelow and WhileBelow events are fired.")]
			public float proximityThreshold = 0.15f;

			[Tooltip("Minimum time (in seconds) between consecutive triggers for this interaction block.\nPrevents rapid re-triggering when objects remain within proximity.")]
			[SerializeField]
			private float cooldownTime = 0.5f;

			[Tooltip("Who is allowed to trigger this block (if gorilla body part is selected).\n• Owner: only this cosmetic's own rig/body can trigger this.\n• Others: only other players' rigs/bodies can trigger this.\n• All: anyone can trigger.\n\nNote: everyone will still be able to see the result when it triggers.")]
			public CosmeticsProximityReactor.TargetType targetType = CosmeticsProximityReactor.TargetType.All;

			public UnityEvent<Vector3> onBelowLocal;

			public UnityEvent<Vector3> onBelowShared;

			public UnityEvent<Vector3> whileBelowLocal;

			public UnityEvent<Vector3> whileBelowShared;

			public UnityEvent onAboveLocal;

			public UnityEvent onAboveShared;

			[NonSerialized]
			public bool wasBelow;

			[NonSerialized]
			public bool isMatched;

			[NonSerialized]
			public float lastEffectTime = -9999f;
		}
	}
}
