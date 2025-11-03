using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GorillaTag.Cosmetics
{
	[Serializable]
	public class ContinuousProperty
	{
		private static ContinuousProperty.Cast GetTargetCast(Object o)
		{
			ContinuousProperty.Cast cast;
			if (!(o is ParticleSystem))
			{
				if (!(o is SkinnedMeshRenderer))
				{
					if (!(o is Animator))
					{
						if (!(o is AudioSource))
						{
							if (!(o is Rigidbody))
							{
								if (!(o is Transform))
								{
									if (!(o is Renderer))
									{
										if (!(o is Behaviour))
										{
											if (!(o is GameObject))
											{
												cast = ContinuousProperty.Cast.Null;
											}
											else
											{
												cast = ContinuousProperty.Cast.GameObject;
											}
										}
										else
										{
											cast = ContinuousProperty.Cast.Behaviour;
										}
									}
									else
									{
										cast = ContinuousProperty.Cast.Renderer;
									}
								}
								else
								{
									cast = ContinuousProperty.Cast.Transform;
								}
							}
							else
							{
								cast = ContinuousProperty.Cast.Rigidbody;
							}
						}
						else
						{
							cast = ContinuousProperty.Cast.AudioSource;
						}
					}
					else
					{
						cast = ContinuousProperty.Cast.Animator;
					}
				}
				else
				{
					cast = ContinuousProperty.Cast.SkinnedMeshRenderer;
				}
			}
			else
			{
				cast = ContinuousProperty.Cast.ParticleSystem;
			}
			return cast;
		}

		public static bool CastMatches(ContinuousProperty.Cast cast, ContinuousProperty.Cast test)
		{
			if (cast <= ContinuousProperty.Cast.Any)
			{
				if (cast == ContinuousProperty.Cast.Null)
				{
					return false;
				}
				if (cast == ContinuousProperty.Cast.Any)
				{
					return true;
				}
			}
			else
			{
				if (cast == ContinuousProperty.Cast.Renderer)
				{
					return test == ContinuousProperty.Cast.Renderer || test == ContinuousProperty.Cast.SkinnedMeshRenderer;
				}
				if (cast == ContinuousProperty.Cast.Behaviour)
				{
					return test != ContinuousProperty.Cast.Transform && test != ContinuousProperty.Cast.GameObject && test != ContinuousProperty.Cast.Rigidbody;
				}
			}
			return test == cast;
		}

		public static bool HasAllFlags(ContinuousProperty.DataFlags flags, ContinuousProperty.DataFlags test)
		{
			return (flags & test) == test;
		}

		public static bool HasAnyFlag(ContinuousProperty.DataFlags flags, ContinuousProperty.DataFlags test)
		{
			return (flags & test) > ContinuousProperty.DataFlags.None;
		}

		private static IEnumerable<Object> GetAllObjects(Object target)
		{
			Component component = target as Component;
			IEnumerable<Object> enumerable;
			if (component == null)
			{
				GameObject gameObject = target as GameObject;
				if (gameObject == null)
				{
					enumerable = null;
				}
				else
				{
					enumerable = (from c in gameObject.GetComponents<Component>()
						where ContinuousProperty.IsValidComponent(c.GetType())
						select c).Append(gameObject);
				}
			}
			else
			{
				enumerable = (from c in component.GetComponents<Component>()
					where ContinuousProperty.IsValidComponent(c.GetType())
					select c).Append(component.gameObject);
			}
			return enumerable;
		}

		private static bool IsValidComponent(global::System.Type t)
		{
			return t != typeof(Renderer) && t != typeof(ParticleSystemRenderer);
		}

		public ContinuousProperty()
		{
		}

		public ContinuousProperty(ContinuousPropertyModeSO mode, Transform initialTarget, Vector2 range = default(Vector2))
		{
			this.mode = mode;
			this.range = range;
			this.FindATarget();
		}

		private string ModeTooltip
		{
			get
			{
				if (!this.mode)
				{
					return "";
				}
				return string.Format("{0}: {1}", this.mode.type, this.mode.GetDescriptionForCast(ContinuousProperty.GetTargetCast(this.target)));
			}
		}

		private bool ModeErrorVisible
		{
			get
			{
				return !this.IsValid();
			}
		}

		private bool ModeInfoVisible
		{
			get
			{
				return this.mode == null;
			}
		}

		private string ModeErrorMessage
		{
			get
			{
				string text = "I can't find a target on '{0}' to apply my '{1}' to. Did you drag in the wrong GameObject?\n\n{2}";
				Object @object = this.target;
				object obj = ((@object != null) ? @object.name : null);
				object obj2 = this.MyType;
				ContinuousPropertyModeSO continuousPropertyModeSO = this.mode;
				return string.Format(text, obj, obj2, (continuousPropertyModeSO != null) ? continuousPropertyModeSO.ListValidCasts() : null);
			}
		}

		public ContinuousPropertyModeSO Mode
		{
			get
			{
				return this.mode;
			}
		}

		public ContinuousProperty.Type MyType
		{
			get
			{
				if (!(this.mode != null))
				{
					return ContinuousProperty.Type.Color;
				}
				return this.mode.type;
			}
		}

		private bool HasTarget
		{
			get
			{
				return this.MyType != ContinuousProperty.Type.UnityEvent;
			}
		}

		private bool TargetInfoVisible
		{
			get
			{
				return this.HasTarget && this.target == null;
			}
		}

		private string TargetTooltip
		{
			get
			{
				if (!(this.mode != null))
				{
					return "";
				}
				return this.mode.ListValidCasts();
			}
		}

		private bool AssignButtonVisible
		{
			get
			{
				return this.mode != null && (this.target == null || !this.mode.IsCastValid(ContinuousProperty.GetTargetCast(this.target)));
			}
		}

		private bool ShiftButtonsVisible
		{
			get
			{
				IEnumerable<Object> allValidObjectsOnMyTarget = this.GetAllValidObjectsOnMyTarget();
				return allValidObjectsOnMyTarget != null && allValidObjectsOnMyTarget.Count<Object>() > 1;
			}
		}

		public Object Target
		{
			get
			{
				return this.target;
			}
		}

		private void FindATarget()
		{
		}

		private IEnumerable<Object> GetAllValidObjectsOnMyTarget()
		{
			if (!this.mode)
			{
				return null;
			}
			IEnumerable<Object> allObjects = ContinuousProperty.GetAllObjects(this.target);
			if (allObjects == null)
			{
				return null;
			}
			return allObjects.Where((Object c) => this.mode.IsCastValid(ContinuousProperty.GetTargetCast(c)));
		}

		private void PreviousTarget()
		{
			this.ShiftTarget(-1);
		}

		private void NextTarget()
		{
			this.ShiftTarget(1);
		}

		public bool ShiftTarget(int amount)
		{
			IEnumerable<Object> allValidObjectsOnMyTarget = this.GetAllValidObjectsOnMyTarget();
			List<Object> list = ((allValidObjectsOnMyTarget != null) ? allValidObjectsOnMyTarget.ToList<Object>() : null);
			if (list == null || list.Count == 0)
			{
				return false;
			}
			int num = Mathf.Max(list.IndexOf(this.target), 0);
			this.target = list[(num + amount + list.Count) % list.Count];
			return true;
		}

		private static AnimationCurve StepCurve
		{
			get
			{
				return new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 0f, float.PositiveInfinity, float.PositiveInfinity),
					new Keyframe(0.5f, 1f, float.PositiveInfinity, float.PositiveInfinity)
				});
			}
		}

		private void OnValueChanged()
		{
			if (!this.IsValid())
			{
				this.ShiftTarget(0);
			}
		}

		public bool IsShaderProperty_Cached { get; private set; }

		public bool UsesThreshold_Cached { get; private set; }

		public bool IsValid()
		{
			return this.mode == null || this.target == null || this.mode.IsCastValid(ContinuousProperty.GetTargetCast(this.target));
		}

		public int GetTargetInstanceID()
		{
			return this.target.GetInstanceID();
		}

		private bool HasAllFlags(ContinuousProperty.DataFlags test)
		{
			return this.mode != null && ContinuousProperty.HasAllFlags(this.mode.GetFlagsForClosestCast(ContinuousProperty.GetTargetCast(this.target)), test);
		}

		private bool HasAnyFlag(ContinuousProperty.DataFlags test)
		{
			return this.mode != null && ContinuousProperty.HasAnyFlag(this.mode.GetFlagsForClosestCast(ContinuousProperty.GetTargetCast(this.target)), test);
		}

		private bool HasGradient
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasColor);
			}
		}

		private bool HasCurve
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasCurve);
			}
		}

		private string DynamicIntLabel()
		{
			if (!this.HasAllFlags(ContinuousProperty.DataFlags.IsShaderProperty))
			{
				ContinuousProperty.Type myType = this.MyType;
				if (myType != ContinuousProperty.Type.Color && myType != ContinuousProperty.Type.BlendShape)
				{
					return "Int Value";
				}
			}
			return "Material Index";
		}

		private bool HasInt
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasInteger);
			}
		}

		public int IntValue
		{
			get
			{
				return this.intValue;
			}
		}

		private string DynamicStringLabel()
		{
			if (this.HasAllFlags(ContinuousProperty.DataFlags.IsShaderProperty))
			{
				return "Property Name";
			}
			if (this.HasAllFlags(ContinuousProperty.DataFlags.IsAnimatorParameter))
			{
				return "Parameter Name";
			}
			return "String Value";
		}

		private bool HasString
		{
			get
			{
				return this.HasAnyFlag(ContinuousProperty.DataFlags.IsShaderProperty | ContinuousProperty.DataFlags.IsAnimatorParameter);
			}
		}

		public string StringValue
		{
			get
			{
				return this.stringValue;
			}
		}

		private bool HasBezier
		{
			get
			{
				return this.MyType == ContinuousProperty.Type.BezierInterpolation;
			}
		}

		private bool MissingBezier
		{
			get
			{
				return this.bezierCurve == null;
			}
		}

		private bool AxisError
		{
			get
			{
				return !Enum.IsDefined(typeof(ContinuousProperty.RotationAxis), this.localAxis);
			}
		}

		private bool HasAxis
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasAxis);
			}
		}

		private bool InterpolationError
		{
			get
			{
				return !Enum.IsDefined(typeof(ContinuousProperty.InterpolationMode), this.interpolationMode);
			}
		}

		private bool HasInterpolation
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasInterpolation);
			}
		}

		private bool HasStopAction
		{
			get
			{
				return this.MyType == ContinuousProperty.Type.PlayStop && this.target is ParticleSystem;
			}
		}

		private bool HasXforms
		{
			get
			{
				return this.MyType == ContinuousProperty.Type.TransformInterpolation;
			}
		}

		private bool MissingXforms
		{
			get
			{
				return this.transformA == null || this.transformB == null;
			}
		}

		private bool HasOffsets
		{
			get
			{
				return this.MyType == ContinuousProperty.Type.OffsetInterpolation;
			}
		}

		private string ThresholdErrorMessage
		{
			get
			{
				return "The threshold will always be " + (((this.thresholdOption == ContinuousProperty.ThresholdOption.Normal) ^ (this.range.x >= this.range.y)) ? "true." : "false.");
			}
		}

		private string ThresholdTooltip
		{
			get
			{
				if (!this.ThresholdError)
				{
					return "The threshold will be true" + ((this.thresholdOption == ContinuousProperty.ThresholdOption.Normal) ? ((this.range.x > 0f && this.range.y < 1f) ? string.Format(" between {0} and {1}", this.range.x, this.range.y) : ((this.range.x > 0f) ? (" above " + this.range.x.ToString()) : (" below " + this.range.y.ToString()))) : (((this.range.x > 0f) ? (" below " + this.range.x.ToString()) : "") + ((this.range.x > 0f && this.range.y < 1f) ? " and" : "") + ((this.range.y < 1f) ? (" above " + this.range.y.ToString()) : ""))) + ", and false otherwise.";
				}
				return this.ThresholdErrorMessage;
			}
		}

		private bool HasThreshold
		{
			get
			{
				return this.HasAllFlags(ContinuousProperty.DataFlags.HasThreshold);
			}
		}

		private bool ThresholdError
		{
			get
			{
				return (this.range.x <= 0f && this.range.y >= 1f) || this.range.x >= this.range.y;
			}
		}

		private bool HasUnityEvent
		{
			get
			{
				return this.MyType == ContinuousProperty.Type.UnityEvent;
			}
		}

		public void Init()
		{
			if (this.mode == null)
			{
				this.internalSwitchValue = 0;
				return;
			}
			ContinuousProperty.Type type = this.mode.type;
			ContinuousProperty.Cast cast = this.mode.GetClosestCast(ContinuousProperty.GetTargetCast(this.target));
			ContinuousProperty.DataFlags dataFlags = this.mode.GetFlagsForCast(cast);
			if ((type == ContinuousProperty.Type.BezierInterpolation && this.MissingBezier) || (type == ContinuousProperty.Type.TransformInterpolation && this.MissingXforms) || (type == ContinuousProperty.Type.UnityEvent && this.unityEvent == null))
			{
				this.internalSwitchValue = 0;
				return;
			}
			if (type == ContinuousProperty.Type.Color && ContinuousProperty.CastMatches(ContinuousProperty.Cast.Renderer, cast))
			{
				type = ContinuousProperty.Type.ShaderColor;
				cast = ContinuousProperty.Cast.Renderer;
				dataFlags |= ContinuousProperty.DataFlags.IsShaderProperty;
				this.stringValue = "_BaseColor";
			}
			else if (type == ContinuousProperty.Type.PlayStop && cast == ContinuousProperty.Cast.Animator)
			{
				type = ContinuousProperty.Type.EnableDisable;
				cast = ContinuousProperty.Cast.Behaviour;
			}
			this.internalSwitchValue = (int)(type | (ContinuousProperty.Type)cast | (ContinuousProperty.Type)(ContinuousProperty.HasAllFlags(dataFlags, ContinuousProperty.DataFlags.HasAxis) ? this.localAxis : ((ContinuousProperty.RotationAxis)0)) | (ContinuousProperty.Type)(ContinuousProperty.HasAllFlags(dataFlags, ContinuousProperty.DataFlags.HasInterpolation) ? this.interpolationMode : ((ContinuousProperty.InterpolationMode)0)));
			this.IsShaderProperty_Cached = ContinuousProperty.HasAllFlags(dataFlags, ContinuousProperty.DataFlags.IsShaderProperty);
			this.UsesThreshold_Cached = ContinuousProperty.HasAllFlags(dataFlags, ContinuousProperty.DataFlags.HasThreshold);
			if (cast == ContinuousProperty.Cast.ParticleSystem)
			{
				this.particleMain = ((ParticleSystem)this.target).main;
				this.particleEmission = ((ParticleSystem)this.target).emission;
				this.speedCurveCache = this.particleMain.startSpeed;
				this.rateCurveCache = this.particleEmission.rateOverTime;
			}
			if (this.IsShaderProperty_Cached)
			{
				this.stringHash = Shader.PropertyToID(this.stringValue);
			}
			else if (ContinuousProperty.HasAllFlags(dataFlags, ContinuousProperty.DataFlags.IsAnimatorParameter))
			{
				this.stringHash = Animator.StringToHash(this.stringValue);
			}
			if (!ContinuousProperty.HasAnyFlag(dataFlags, ContinuousProperty.DataFlags.HasCurve))
			{
				this.curve = AnimationCurves.Linear;
			}
		}

		public void InitThreshold()
		{
			if (!this.UsesThreshold_Cached)
			{
				return;
			}
			this.CheckThreshold(0f);
			if (this.IsShaderProperty_Cached)
			{
				return;
			}
			this.previousBoolValue = !this.previousBoolValue;
			this.Apply(0f, null);
		}

		public void Apply(float f, MaterialPropertyBlock mpb)
		{
			int num = this.internalSwitchValue | (int)this.CheckThreshold(f);
			if (num <= 1056784)
			{
				if (num <= 5131)
				{
					if (num <= 3073)
					{
						if (num <= 1041)
						{
							if (num == 0)
							{
								return;
							}
							if (num != 1041)
							{
								return;
							}
						}
						else
						{
							if (num == 2049)
							{
								((Transform)this.target).localScale = this.curve.Evaluate(f) * Vector3.one;
								return;
							}
							if (num == 3072)
							{
								this.particleMain.startColor = this.color.Evaluate(f);
								return;
							}
							if (num != 3073)
							{
								return;
							}
							this.particleMain.startSize = this.curve.Evaluate(f);
							return;
						}
					}
					else if (num <= 3084)
					{
						if (num == 3083)
						{
							this.particleMain.startSpeed = this.ScaleCurve(in this.speedCurveCache, this.curve.Evaluate(f));
							return;
						}
						if (num != 3084)
						{
							return;
						}
						this.particleEmission.rateOverTime = this.ScaleCurve(in this.rateCurveCache, this.curve.Evaluate(f));
						return;
					}
					else
					{
						if (num == 4098)
						{
							((SkinnedMeshRenderer)this.target).SetBlendShapeWeight(this.intValue, this.curve.Evaluate(f) * 100f);
							return;
						}
						if (num == 5123)
						{
							((Animator)this.target).SetFloat(this.stringHash, this.curve.Evaluate(f));
							return;
						}
						if (num != 5131)
						{
							return;
						}
						((Animator)this.target).speed = this.curve.Evaluate(f);
						return;
					}
				}
				else if (num <= 1051663)
				{
					if (num <= 6158)
					{
						if (num == 6157)
						{
							((AudioSource)this.target).volume = Mathf.Clamp01(this.curve.Evaluate(f));
							return;
						}
						if (num != 6158)
						{
							return;
						}
						((AudioSource)this.target).pitch = Mathf.Clamp(this.curve.Evaluate(f), -3f, 3f);
						return;
					}
					else
					{
						switch (num)
						{
						case 7171:
							mpb.SetFloat(this.stringHash, this.curve.Evaluate(f));
							return;
						case 7172:
							mpb.SetVector(this.stringHash, new Vector2(this.curve.Evaluate(f), 0f));
							return;
						case 7173:
							mpb.SetColor(this.stringHash, this.color.Evaluate(f));
							return;
						default:
							if (num != 1049617)
							{
								if (num != 1051663)
								{
									return;
								}
								((ParticleSystem)this.target).Play();
								return;
							}
							break;
						}
					}
				}
				else if (num <= 1053714)
				{
					if (num == 1053706)
					{
						goto IL_0638;
					}
					if (num != 1053714)
					{
						return;
					}
					((Animator)this.target).SetTrigger(this.stringHash);
					return;
				}
				else
				{
					if (num == 1054735)
					{
						((AudioSource)this.target).Play();
						return;
					}
					if (num == 1055760)
					{
						goto IL_0753;
					}
					if (num != 1056784)
					{
						return;
					}
					goto IL_076A;
				}
				this.unityEvent.Invoke(this.curve.Evaluate(f));
				return;
			}
			if (num <= 3146769)
			{
				if (num <= 2102290)
				{
					if (num <= 2098193)
					{
						if (num != 1057808)
						{
							return;
						}
					}
					else
					{
						if (num == 2100239)
						{
							((ParticleSystem)this.target).Stop(true, this.stopType);
							return;
						}
						if (num != 2102282)
						{
							return;
						}
						goto IL_0638;
					}
				}
				else if (num <= 2104336)
				{
					if (num == 2103311)
					{
						((AudioSource)this.target).Stop();
						return;
					}
					if (num != 2104336)
					{
						return;
					}
					goto IL_0753;
				}
				else
				{
					if (num == 2105360)
					{
						goto IL_076A;
					}
					if (num != 2106384)
					{
						return;
					}
				}
				((GameObject)this.target).SetActive(this.previousBoolValue);
				return;
			}
			if (num <= 3152912)
			{
				if (num <= 3150858)
				{
					if (num != 3148815)
					{
						return;
					}
					return;
				}
				else
				{
					if (num != 3150866 && num != 3151887)
					{
						return;
					}
					return;
				}
			}
			else if (num <= 3154960)
			{
				if (num != 3153936)
				{
					return;
				}
				return;
			}
			else
			{
				switch (num)
				{
				case 4196358:
					((Transform)this.target).position = this.bezierCurve.GetPoint(this.curve.Evaluate(f));
					return;
				case 4196359:
					((Transform)this.target).localRotation = Quaternion.Euler(this.curve.Evaluate(f) * 360f, 0f, 0f);
					return;
				case 4196360:
					((Transform)this.target).position = Vector3.Lerp(this.transformA.position, this.transformB.position, this.curve.Evaluate(f));
					return;
				case 4196361:
					((Transform)this.target).localPosition = Vector3.Lerp(this.offsetA.pos, this.offsetB.pos, this.curve.Evaluate(f));
					return;
				default:
					switch (num)
					{
					case 8390662:
						((Transform)this.target).rotation = Quaternion.LookRotation(this.bezierCurve.GetDirection(this.curve.Evaluate(f)));
						return;
					case 8390663:
						((Transform)this.target).localRotation = Quaternion.Euler(0f, this.curve.Evaluate(f) * 360f, 0f);
						return;
					case 8390664:
						((Transform)this.target).rotation = Quaternion.Slerp(this.transformA.rotation, this.transformB.rotation, this.curve.Evaluate(f));
						return;
					case 8390665:
						((Transform)this.target).localRotation = Quaternion.Slerp(this.offsetA.rot, this.offsetB.rot, this.curve.Evaluate(f));
						return;
					default:
						switch (num)
						{
						case 12584966:
						{
							float num2 = this.curve.Evaluate(f);
							((Transform)this.target).SetPositionAndRotation(this.bezierCurve.GetPoint(num2), Quaternion.LookRotation(this.bezierCurve.GetDirection(num2)));
							return;
						}
						case 12584967:
							((Transform)this.target).localRotation = Quaternion.Euler(0f, 0f, this.curve.Evaluate(f) * 360f);
							return;
						case 12584968:
						{
							Vector3 vector;
							Quaternion quaternion;
							this.transformA.GetPositionAndRotation(out vector, out quaternion);
							Vector3 vector2;
							Quaternion quaternion2;
							this.transformB.GetPositionAndRotation(out vector2, out quaternion2);
							float num3 = this.curve.Evaluate(f);
							((Transform)this.target).SetPositionAndRotation(Vector3.Lerp(vector, vector2, num3), Quaternion.Slerp(quaternion, quaternion2, num3));
							return;
						}
						case 12584969:
						{
							float num4 = this.curve.Evaluate(f);
							((Transform)this.target).SetLocalPositionAndRotation(Vector3.Lerp(this.offsetA.pos, this.offsetB.pos, num4), Quaternion.Slerp(this.offsetA.rot, this.offsetB.rot, num4));
							return;
						}
						default:
							return;
						}
						break;
					}
					break;
				}
			}
			IL_0638:
			((Animator)this.target).SetBool(this.stringHash, this.previousBoolValue);
			return;
			IL_0753:
			((Renderer)this.target).enabled = this.previousBoolValue;
			return;
			IL_076A:
			((Behaviour)this.target).enabled = this.previousBoolValue;
		}

		private ParticleSystem.MinMaxCurve ScaleCurve(in ParticleSystem.MinMaxCurve inCurve, float scale)
		{
			ParticleSystem.MinMaxCurve minMaxCurve = inCurve;
			switch (minMaxCurve.mode)
			{
			case ParticleSystemCurveMode.Constant:
				minMaxCurve.constant *= scale;
				break;
			case ParticleSystemCurveMode.Curve:
			case ParticleSystemCurveMode.TwoCurves:
				minMaxCurve.curveMultiplier *= scale;
				break;
			case ParticleSystemCurveMode.TwoConstants:
				minMaxCurve.constantMin *= scale;
				minMaxCurve.constantMax *= scale;
				break;
			}
			return minMaxCurve;
		}

		private ContinuousProperty.ThresholdResult CheckThreshold(float f)
		{
			if (!this.UsesThreshold_Cached)
			{
				return ContinuousProperty.ThresholdResult.Null;
			}
			bool flag = f >= this.range.x && f <= this.range.y;
			if (!this.previousBoolValue && ((this.thresholdOption == ContinuousProperty.ThresholdOption.Normal && flag) || (this.thresholdOption == ContinuousProperty.ThresholdOption.Invert && !flag)))
			{
				this.previousBoolValue = true;
				return ContinuousProperty.ThresholdResult.RisingEdge;
			}
			if (this.previousBoolValue && ((this.thresholdOption == ContinuousProperty.ThresholdOption.Normal && !flag) || (this.thresholdOption == ContinuousProperty.ThresholdOption.Invert && flag)))
			{
				this.previousBoolValue = false;
				return ContinuousProperty.ThresholdResult.FallingEdge;
			}
			return ContinuousProperty.ThresholdResult.Unchanged;
		}

		[SerializeField]
		private ContinuousPropertyModeSO mode;

		[FormerlySerializedAs("component")]
		[SerializeField]
		protected Object target;

		private static int linearCurveHash = AnimationCurves.Linear.GetHashCode();

		private static int stepCurveHash = ContinuousProperty.StepCurve.GetHashCode();

		[SerializeField]
		private Gradient color;

		[SerializeField]
		private AnimationCurve curve = AnimationCurves.Linear;

		[FormerlySerializedAs("materialIndex")]
		[SerializeField]
		private int intValue;

		[SerializeField]
		private string stringValue;

		[SerializeField]
		private BezierCurve bezierCurve;

		private const string ENUM_ERROR = "Internal values were changed at some point. Please select a new value.";

		[SerializeField]
		private ContinuousProperty.RotationAxis localAxis = ContinuousProperty.RotationAxis.X;

		[SerializeField]
		private ContinuousProperty.InterpolationMode interpolationMode = ContinuousProperty.InterpolationMode.PositionAndRotation;

		[SerializeField]
		private ParticleSystemStopBehavior stopType = ParticleSystemStopBehavior.StopEmitting;

		[SerializeField]
		private Transform transformA;

		[SerializeField]
		private Transform transformB;

		[SerializeField]
		private XformOffset offsetA;

		[SerializeField]
		private XformOffset offsetB;

		[SerializeField]
		private Vector2 range = new Vector2(0.5f, 1f);

		[SerializeField]
		private ContinuousProperty.ThresholdOption thresholdOption = ContinuousProperty.ThresholdOption.Normal;

		[SerializeField]
		private UnityEvent<float> unityEvent;

		private int internalSwitchValue;

		private ParticleSystem.MainModule particleMain;

		private ParticleSystem.EmissionModule particleEmission;

		private ParticleSystem.MinMaxCurve speedCurveCache;

		private ParticleSystem.MinMaxCurve rateCurveCache;

		private bool previousBoolValue;

		private int stringHash;

		public enum Type
		{
			Color,
			Scale,
			BlendShape,
			Float,
			ShaderVector2_X,
			ShaderColor,
			BezierInterpolation,
			AxisAngle,
			TransformInterpolation,
			OffsetInterpolation,
			Boolean,
			Speed,
			Rate,
			Volume,
			Pitch,
			PlayStop,
			EnableDisable,
			UnityEvent,
			Trigger
		}

		public enum Cast
		{
			Null,
			Any = 1024,
			Transform = 2048,
			ParticleSystem = 3072,
			SkinnedMeshRenderer = 4096,
			Animator = 5120,
			AudioSource = 6144,
			Renderer = 7168,
			Behaviour = 8192,
			GameObject = 9216,
			Rigidbody = 10240
		}

		[Flags]
		public enum DataFlags
		{
			None = 0,
			HasCurve = 1,
			HasColor = 2,
			HasAxis = 4,
			HasInteger = 8,
			HasInterpolation = 16,
			IsShaderProperty = 32,
			IsAnimatorParameter = 64,
			HasThreshold = 128
		}

		private enum ThresholdResult
		{
			Null,
			RisingEdge = 1048576,
			FallingEdge = 2097152,
			Unchanged = 3145728
		}

		private enum ThresholdOption
		{
			Invert,
			Normal
		}

		private enum RotationAxis
		{
			X = 4194304,
			Y = 8388608,
			Z = 12582912
		}

		public enum InterpolationMode
		{
			Position = 4194304,
			Rotation = 8388608,
			PositionAndRotation = 12582912
		}
	}
}
