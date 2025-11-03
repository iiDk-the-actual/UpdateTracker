using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class CosmeticsProximityReactorManager : MonoBehaviour, IGorillaSliceableSimple
	{
		public static CosmeticsProximityReactorManager Instance
		{
			get
			{
				return CosmeticsProximityReactorManager._instance;
			}
		}

		private void Awake()
		{
			if (CosmeticsProximityReactorManager._instance != null && CosmeticsProximityReactorManager._instance != this)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			CosmeticsProximityReactorManager._instance = this;
		}

		public void OnEnable()
		{
			GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		}

		public void OnDisable()
		{
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
			if (CosmeticsProximityReactorManager._instance == this)
			{
				CosmeticsProximityReactorManager._instance = null;
			}
		}

		public void Register(CosmeticsProximityReactor cosmetic)
		{
			if (cosmetic == null)
			{
				return;
			}
			if (cosmetic.IsGorillaBody())
			{
				if (!this.gorillaBodyPart.Contains(cosmetic))
				{
					this.gorillaBodyPart.Add(cosmetic);
				}
				return;
			}
			if (!this.cosmetics.Contains(cosmetic))
			{
				this.cosmetics.Add(cosmetic);
			}
			foreach (string text in cosmetic.GetTypes())
			{
				if (!string.IsNullOrEmpty(text))
				{
					List<CosmeticsProximityReactor> list;
					if (!this.byType.TryGetValue(text, out list))
					{
						list = new List<CosmeticsProximityReactor>();
						this.byType[text] = list;
					}
					if (!list.Contains(cosmetic))
					{
						list.Add(cosmetic);
						this.typeKeysDirty = true;
					}
				}
			}
		}

		public void Unregister(CosmeticsProximityReactor cosmetic)
		{
			if (cosmetic == null)
			{
				return;
			}
			this.cosmetics.Remove(cosmetic);
			this.gorillaBodyPart.Remove(cosmetic);
			this.matchedFrame.Remove(cosmetic);
			foreach (KeyValuePair<string, List<CosmeticsProximityReactor>> keyValuePair in this.byType)
			{
				if (keyValuePair.Value.Remove(cosmetic))
				{
					this.typeKeysDirty = true;
				}
			}
		}

		public void SliceUpdate()
		{
			if (this.cosmetics.Count == 0)
			{
				return;
			}
			if (this.AnyGroupHasTwo())
			{
				if (this.typeKeysDirty)
				{
					this.RebuildTypeKeysCache();
				}
				if (this.typeKeysCache.Count > 0)
				{
					int num = 0;
					while (num < this.groupsPerSlice && this.typeKeysCache.Count > 0)
					{
						if (this.groupCursor >= this.typeKeysCache.Count)
						{
							this.groupCursor = 0;
						}
						string text = this.typeKeysCache[this.groupCursor];
						List<CosmeticsProximityReactor> list;
						if (this.byType.TryGetValue(text, out list) && list != null && list.Count > 0)
						{
							this.ProcessOneGroup(list);
						}
						this.groupCursor++;
						num++;
					}
				}
			}
			if (this.gorillaBodyPart.Count > 0)
			{
				foreach (CosmeticsProximityReactor cosmeticsProximityReactor in this.cosmetics)
				{
					if (!(cosmeticsProximityReactor == null))
					{
						if (!cosmeticsProximityReactor.AcceptsAnySource())
						{
							cosmeticsProximityReactor.OnSourceAboveAll();
						}
						else
						{
							bool flag = false;
							Vector3 vector = default(Vector3);
							foreach (CosmeticsProximityReactor cosmeticsProximityReactor2 in this.gorillaBodyPart)
							{
								if (!(cosmeticsProximityReactor2 == null) && cosmeticsProximityReactor.AcceptsThisSource(cosmeticsProximityReactor2.gorillaBodyParts))
								{
									bool flag2;
									float sourceThresholdFor = cosmeticsProximityReactor.GetSourceThresholdFor(cosmeticsProximityReactor2, out flag2);
									Vector3 vector2;
									if (flag2 && CosmeticsProximityReactorManager.AreCollidersWithinThreshold(cosmeticsProximityReactor2, cosmeticsProximityReactor, sourceThresholdFor, out vector2))
									{
										cosmeticsProximityReactor.OnSourceBelow(vector2, cosmeticsProximityReactor2.gorillaBodyParts);
										vector = vector2;
										flag = true;
									}
								}
							}
							if (flag)
							{
								cosmeticsProximityReactor.WhileSourceBelow(vector, CosmeticsProximityReactor.GorillaBodyPart.HandLeft | CosmeticsProximityReactor.GorillaBodyPart.HandRight | CosmeticsProximityReactor.GorillaBodyPart.Mouth);
							}
							else
							{
								cosmeticsProximityReactor.OnSourceAboveAll();
							}
						}
					}
				}
			}
			if (this.typeKeysDirty)
			{
				this.RebuildTypeKeysCache();
			}
			foreach (string text2 in this.typeKeysCache)
			{
				List<CosmeticsProximityReactor> list2;
				if (this.byType.TryGetValue(text2, out list2) && list2 != null && list2.Count > 0)
				{
					this.BreakTheBoundForGroup(list2);
				}
			}
		}

		private void ProcessOneGroup(List<CosmeticsProximityReactor> group)
		{
			if (!this.CheckProximity(group))
			{
				this.BreakTheBoundForGroup(group);
			}
		}

		private bool CheckProximity(List<CosmeticsProximityReactor> group)
		{
			bool flag = false;
			for (int i = 0; i < group.Count; i++)
			{
				CosmeticsProximityReactor cosmeticsProximityReactor = group[i];
				if (!(cosmeticsProximityReactor == null))
				{
					for (int j = i + 1; j < group.Count; j++)
					{
						CosmeticsProximityReactor cosmeticsProximityReactor2 = group[j];
						if (!(cosmeticsProximityReactor2 == null) && !CosmeticsProximityReactorManager.ShouldSkipSameIdPair(cosmeticsProximityReactor, cosmeticsProximityReactor2))
						{
							bool flag2;
							float cosmeticPairThresholdWith = cosmeticsProximityReactor.GetCosmeticPairThresholdWith(cosmeticsProximityReactor2, out flag2);
							bool flag3;
							float cosmeticPairThresholdWith2 = cosmeticsProximityReactor2.GetCosmeticPairThresholdWith(cosmeticsProximityReactor, out flag3);
							if (flag2 && flag3)
							{
								float num = Mathf.Min(cosmeticPairThresholdWith, cosmeticPairThresholdWith2);
								Vector3 vector;
								if (CosmeticsProximityReactorManager.AreCollidersWithinThreshold(cosmeticsProximityReactor, cosmeticsProximityReactor2, num, out vector))
								{
									cosmeticsProximityReactor.OnCosmeticBelowWith(cosmeticsProximityReactor2, vector);
									cosmeticsProximityReactor2.OnCosmeticBelowWith(cosmeticsProximityReactor, vector);
									if (cosmeticsProximityReactor.IsBelow && cosmeticsProximityReactor2.IsBelow)
									{
										cosmeticsProximityReactor.RefreshAggregateMatched();
										cosmeticsProximityReactor2.RefreshAggregateMatched();
										this.matchedFrame[cosmeticsProximityReactor] = Time.frameCount;
										this.matchedFrame[cosmeticsProximityReactor2] = Time.frameCount;
										flag = true;
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}

		private void BreakTheBoundForGroup(List<CosmeticsProximityReactor> group)
		{
			foreach (CosmeticsProximityReactor cosmeticsProximityReactor in group)
			{
				int num;
				if (!(cosmeticsProximityReactor == null) && cosmeticsProximityReactor.HasAnyCosmeticMatch() && (!this.matchedFrame.TryGetValue(cosmeticsProximityReactor, out num) || num != Time.frameCount))
				{
					CosmeticsProximityReactor cosmeticsProximityReactor2;
					Vector3 vector;
					if (this.TryFindAnyCosmeticPartner(cosmeticsProximityReactor, out cosmeticsProximityReactor2, out vector))
					{
						cosmeticsProximityReactor.WhileCosmeticBelowWith(cosmeticsProximityReactor2, vector);
						cosmeticsProximityReactor2.WhileCosmeticBelowWith(cosmeticsProximityReactor, vector);
					}
					else
					{
						cosmeticsProximityReactor.OnCosmeticAboveAll();
					}
				}
			}
		}

		private bool TryFindAnyCosmeticPartner(CosmeticsProximityReactor a, out CosmeticsProximityReactor partner, out Vector3 contact)
		{
			partner = null;
			contact = default(Vector3);
			foreach (string text in a.GetTypes())
			{
				List<CosmeticsProximityReactor> list;
				if (!string.IsNullOrEmpty(text) && this.byType.TryGetValue(text, out list) && list != null)
				{
					foreach (CosmeticsProximityReactor cosmeticsProximityReactor in list)
					{
						if (!(cosmeticsProximityReactor == null) && !(cosmeticsProximityReactor == a) && !CosmeticsProximityReactorManager.ShouldSkipSameIdPair(a, cosmeticsProximityReactor))
						{
							bool flag;
							float cosmeticPairThresholdWith = a.GetCosmeticPairThresholdWith(cosmeticsProximityReactor, out flag);
							bool flag2;
							float cosmeticPairThresholdWith2 = cosmeticsProximityReactor.GetCosmeticPairThresholdWith(a, out flag2);
							if (flag && flag2)
							{
								float num = Mathf.Min(cosmeticPairThresholdWith, cosmeticPairThresholdWith2);
								Vector3 vector;
								if (CosmeticsProximityReactorManager.AreCollidersWithinThreshold(a, cosmeticsProximityReactor, num, out vector))
								{
									partner = cosmeticsProximityReactor;
									contact = vector;
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		private static bool ShouldSkipSameIdPair(CosmeticsProximityReactor a, CosmeticsProximityReactor b)
		{
			return (a.ignoreSameCosmeticInstances || b.ignoreSameCosmeticInstances) && !string.IsNullOrEmpty(a.PlayFabID) && !string.IsNullOrEmpty(b.PlayFabID) && string.Equals(a.PlayFabID, b.PlayFabID, StringComparison.Ordinal);
		}

		private static bool AreCollidersWithinThreshold(CosmeticsProximityReactor a, CosmeticsProximityReactor b, float threshold, out Vector3 contactPoint)
		{
			Vector3 vector = ((b.collider == null) ? b.transform.position : b.collider.ClosestPoint(a.transform.position));
			Vector3 vector2 = ((a.collider == null) ? a.transform.position : a.collider.ClosestPoint(vector));
			contactPoint = (vector2 + vector) * 0.5f;
			return Vector3.Distance(vector2, vector) <= threshold;
		}

		private bool AnyGroupHasTwo()
		{
			foreach (KeyValuePair<string, List<CosmeticsProximityReactor>> keyValuePair in this.byType)
			{
				List<CosmeticsProximityReactor> value = keyValuePair.Value;
				if (value != null && value.Count >= 2)
				{
					return true;
				}
			}
			return false;
		}

		private void RebuildTypeKeysCache()
		{
			this.typeKeysCache.Clear();
			foreach (KeyValuePair<string, List<CosmeticsProximityReactor>> keyValuePair in this.byType)
			{
				List<CosmeticsProximityReactor> value = keyValuePair.Value;
				if (value != null && value.Count > 0)
				{
					this.typeKeysCache.Add(keyValuePair.Key);
				}
			}
			this.typeKeysDirty = false;
			if (this.groupCursor >= this.typeKeysCache.Count)
			{
				this.groupCursor = 0;
			}
		}

		private static CosmeticsProximityReactorManager _instance;

		private readonly List<CosmeticsProximityReactor> cosmetics = new List<CosmeticsProximityReactor>();

		private readonly List<CosmeticsProximityReactor> gorillaBodyPart = new List<CosmeticsProximityReactor>();

		private readonly Dictionary<string, List<CosmeticsProximityReactor>> byType = new Dictionary<string, List<CosmeticsProximityReactor>>(StringComparer.Ordinal);

		private readonly Dictionary<CosmeticsProximityReactor, int> matchedFrame = new Dictionary<CosmeticsProximityReactor, int>();

		[Tooltip("Perf - How many cosmetic groups should we fully process per frame (slice)")]
		[SerializeField]
		private int groupsPerSlice = 1;

		private readonly List<string> typeKeysCache = new List<string>();

		private bool typeKeysDirty;

		private int groupCursor;

		internal static readonly List<string> SharedKeysCache = new List<string>();
	}
}
