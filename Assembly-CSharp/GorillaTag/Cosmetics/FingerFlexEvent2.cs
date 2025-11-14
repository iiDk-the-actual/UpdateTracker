using System;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class FingerFlexEvent2 : MonoBehaviour, ITickSystemTick
	{
		private bool TryLinkToNextEvent(int index)
		{
			if (index < this.list.Length - 1)
			{
				if (this.list[index].IsFlexTrigger && this.list[index + 1].IsReleaseTrigger)
				{
					this.list[index].linkIndex = index + 1;
					this.list[index + 1].linkIndex = index;
					return true;
				}
				this.list[index + 1].linkIndex = -1;
			}
			this.list[index].linkIndex = -1;
			return false;
		}

		private void Awake()
		{
			this.myRig = base.GetComponentInParent<VRRig>();
			this.myTransferrable = base.GetComponentInParent<TransferrableObject>();
			for (int i = 0; i < this.list.Length; i++)
			{
				FingerFlexEvent2.FlexEvent flexEvent = this.list[i];
				if (this.myTransferrable.IsNull() && flexEvent.UsesTransferrable)
				{
					this.myTransferrable = base.GetComponentInParent<TransferrableObject>();
				}
				if (flexEvent.tryLink && this.TryLinkToNextEvent(i))
				{
					FingerFlexEvent2.FlexEvent flexEvent2 = this.list[i + 1];
					flexEvent.releaseThreshold = flexEvent2.releaseThreshold;
					flexEvent2.flexThreshold = flexEvent.flexThreshold;
					flexEvent2.fingerType = flexEvent.fingerType;
					flexEvent2.handType = flexEvent.handType;
					flexEvent2.networked = flexEvent.networked;
					i++;
				}
			}
		}

		private void CalcFlex(bool disable)
		{
			for (int i = 0; i < this.list.Length; i++)
			{
				FingerFlexEvent2.FlexEvent flexEvent = this.list[i];
				if ((flexEvent.networked || this.myRig.isOfflineVRRig) && (!flexEvent.UsesTransferrable || !this.myTransferrable.IsNull()))
				{
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					switch (flexEvent.handType)
					{
					case FingerFlexEvent2.FlexEvent.HandType.TransferrableHeldHand:
						flag = this.myTransferrable.currentState == TransferrableObject.PositionState.InLeftHand;
						flag2 = this.myTransferrable.currentState == TransferrableObject.PositionState.InRightHand;
						flag3 = flag || flag2;
						break;
					case FingerFlexEvent2.FlexEvent.HandType.TransferrableEquippedSide:
						flag = (this.myTransferrable.storedZone & (BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.LeftBack)) > BodyDockPositions.DropPositions.None;
						flag2 = (this.myTransferrable.storedZone & (BodyDockPositions.DropPositions.RightArm | BodyDockPositions.DropPositions.RightBack)) > BodyDockPositions.DropPositions.None;
						break;
					case FingerFlexEvent2.FlexEvent.HandType.LeftHand:
						flag = true;
						break;
					case FingerFlexEvent2.FlexEvent.HandType.RightHand:
						flag2 = true;
						break;
					}
					if ((!flag || !flag2) && (flag || flag2 || flexEvent.wasHeld))
					{
						float num;
						if (disable || (flexEvent.wasHeld && !flag3))
						{
							num = 0f;
						}
						else
						{
							FingerFlexEvent2.FlexEvent.FingerType fingerType = flexEvent.fingerType;
							float num2;
							switch (fingerType)
							{
							case FingerFlexEvent2.FlexEvent.FingerType.Thumb:
								num2 = (flag ? this.myRig.leftThumb.calcT : this.myRig.rightThumb.calcT);
								break;
							case FingerFlexEvent2.FlexEvent.FingerType.Index:
								num2 = (flag ? this.myRig.leftIndex.calcT : this.myRig.rightIndex.calcT);
								break;
							case FingerFlexEvent2.FlexEvent.FingerType.Middle:
								num2 = (flag ? this.myRig.leftMiddle.calcT : this.myRig.rightMiddle.calcT);
								break;
							case FingerFlexEvent2.FlexEvent.FingerType.IndexAndMiddle:
								num2 = (flag ? Mathf.Min(this.myRig.leftIndex.calcT, this.myRig.leftMiddle.calcT) : Mathf.Min(this.myRig.rightIndex.calcT, this.myRig.rightMiddle.calcT));
								break;
							case FingerFlexEvent2.FlexEvent.FingerType.IndexOrMiddle:
								num2 = (flag ? Mathf.Max(this.myRig.leftIndex.calcT, this.myRig.leftMiddle.calcT) : Mathf.Max(this.myRig.rightIndex.calcT, this.myRig.rightMiddle.calcT));
								break;
							default:
								<PrivateImplementationDetails>.ThrowSwitchExpressionException(fingerType);
								break;
							}
							num = num2;
						}
						float num3 = num;
						flexEvent.ProcessState(flag, num3);
						flexEvent.wasHeld = flag3 && !disable;
						if (flexEvent.IsLinked)
						{
							FingerFlexEvent2.FlexEvent flexEvent2 = this.list[i + 1];
							flexEvent2.ProcessState(flag, num3);
							flexEvent2.wasHeld = flag3;
							i++;
						}
					}
				}
			}
		}

		public void OnEnable()
		{
			TickSystem<object>.AddTickCallback(this);
		}

		public void OnDisable()
		{
			TickSystem<object>.RemoveTickCallback(this);
			this.CalcFlex(true);
		}

		public bool TickRunning { get; set; }

		public void Tick()
		{
			this.CalcFlex(false);
		}

		public FingerFlexEvent2.FlexEvent[] list;

		private VRRig myRig;

		private TransferrableObject myTransferrable;

		[Serializable]
		public class FlexEvent
		{
			public bool IsFlexTrigger
			{
				get
				{
					return this.triggerType == FingerFlexEvent2.FlexEvent.TriggerType.OnFlex;
				}
			}

			public bool IsReleaseTrigger
			{
				get
				{
					return this.triggerType == FingerFlexEvent2.FlexEvent.TriggerType.OnRelease;
				}
			}

			public bool UsesTransferrable
			{
				get
				{
					FingerFlexEvent2.FlexEvent.HandType handType = this.handType;
					return handType == FingerFlexEvent2.FlexEvent.HandType.TransferrableHeldHand || handType == FingerFlexEvent2.FlexEvent.HandType.TransferrableEquippedSide;
				}
			}

			public bool HasValidLink
			{
				get
				{
					return this.linkIndex >= 0;
				}
			}

			public bool IsLinked
			{
				get
				{
					return this.tryLink && this.linkIndex >= 0;
				}
			}

			private bool ShowMainProperties
			{
				get
				{
					return !this.IsLinked || this.IsFlexTrigger;
				}
			}

			private bool ShowFlexThreshold
			{
				get
				{
					return this.ShowMainProperties;
				}
			}

			private bool ShowReleaseThreshold
			{
				get
				{
					return (!this.IsLinked || this.IsReleaseTrigger) && !this.IsFlexTrigger;
				}
			}

			public void ProcessState(bool leftHand, float flexValue)
			{
				this.currentState = ((flexValue < this.releaseThreshold) ? FingerFlexEvent2.FlexEvent.RangeState.Below : ((flexValue >= this.flexThreshold) ? FingerFlexEvent2.FlexEvent.RangeState.Above : FingerFlexEvent2.FlexEvent.RangeState.Within));
				if (this.ShowMainProperties && this.currentState != this.lastState && this.continuousProperties != null && this.continuousProperties.Count > 0)
				{
					float num = Mathf.InverseLerp(this.releaseThreshold, this.flexThreshold, flexValue);
					this.continuousProperties.ApplyAll(num);
				}
				if (this.currentState == FingerFlexEvent2.FlexEvent.RangeState.Above && this.lastState == FingerFlexEvent2.FlexEvent.RangeState.Below)
				{
					this.lastThresholdTime = Time.time;
					this.lastState = FingerFlexEvent2.FlexEvent.RangeState.Above;
					if (this.IsFlexTrigger)
					{
						UnityEvent<bool, float> unityEvent = this.unityEvent;
						if (unityEvent == null)
						{
							return;
						}
						unityEvent.Invoke(leftHand, flexValue);
						return;
					}
				}
				else if (this.currentState == FingerFlexEvent2.FlexEvent.RangeState.Below && this.lastState == FingerFlexEvent2.FlexEvent.RangeState.Above)
				{
					this.lastThresholdTime = Time.time;
					this.lastState = FingerFlexEvent2.FlexEvent.RangeState.Below;
					if (this.IsReleaseTrigger)
					{
						UnityEvent<bool, float> unityEvent2 = this.unityEvent;
						if (unityEvent2 == null)
						{
							return;
						}
						unityEvent2.Invoke(leftHand, flexValue);
					}
				}
			}

			public FingerFlexEvent2.FlexEvent.TriggerType triggerType;

			public bool tryLink = true;

			[HideInInspector]
			public int linkIndex = -1;

			[Space]
			public FingerFlexEvent2.FlexEvent.FingerType fingerType = FingerFlexEvent2.FlexEvent.FingerType.Index;

			[Space]
			public FingerFlexEvent2.FlexEvent.HandType handType;

			private const string ADVANCED = "Advanced Properties";

			[Tooltip("When this is checked, all players in the room will fire the event. Otherwise, only the local player will fire it. You should usually leave this on, unless you're using it for something local like controller haptics.")]
			public bool networked = true;

			[Range(0.01f, 0.75f)]
			public float flexThreshold = 0.75f;

			[Range(0.01f, 1f)]
			public float releaseThreshold = 0.01f;

			public ContinuousPropertyArray continuousProperties;

			public UnityEvent<bool, float> unityEvent;

			[NonSerialized]
			public bool wasHeld;

			[NonSerialized]
			public bool marginError;

			private FingerFlexEvent2.FlexEvent.RangeState currentState;

			private FingerFlexEvent2.FlexEvent.RangeState lastState;

			private float lastThresholdTime = -100000f;

			public enum TriggerType
			{
				OnFlex,
				OnRelease = 2
			}

			public enum FingerType
			{
				Thumb,
				Index,
				Middle,
				IndexAndMiddle,
				IndexOrMiddle
			}

			public enum HandType
			{
				TransferrableHeldHand,
				TransferrableEquippedSide,
				LeftHand,
				RightHand
			}

			private enum RangeState
			{
				Below,
				Within,
				Above
			}
		}
	}
}
