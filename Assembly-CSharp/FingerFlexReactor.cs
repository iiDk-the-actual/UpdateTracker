using System;
using UnityEngine;

public class FingerFlexReactor : MonoBehaviour
{
	private void Setup()
	{
		this._rig = base.GetComponentInParent<VRRig>();
		if (!this._rig)
		{
			return;
		}
		this._fingers = new VRMap[]
		{
			this._rig.leftThumb,
			this._rig.leftIndex,
			this._rig.leftMiddle,
			this._rig.rightThumb,
			this._rig.rightIndex,
			this._rig.rightMiddle
		};
	}

	private void Awake()
	{
		this.Setup();
	}

	private void FixedUpdate()
	{
		this.UpdateBlendShapes();
	}

	public void UpdateBlendShapes()
	{
		if (!this._rig)
		{
			return;
		}
		if (this._blendShapeTargets == null || this._fingers == null)
		{
			return;
		}
		if (this._blendShapeTargets.Length == 0 || this._fingers.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this._blendShapeTargets.Length; i++)
		{
			FingerFlexReactor.BlendShapeTarget blendShapeTarget = this._blendShapeTargets[i];
			if (blendShapeTarget != null)
			{
				int sourceFinger = (int)blendShapeTarget.sourceFinger;
				if (sourceFinger != -1)
				{
					SkinnedMeshRenderer targetRenderer = blendShapeTarget.targetRenderer;
					if (targetRenderer)
					{
						float lerpValue = FingerFlexReactor.GetLerpValue(this._fingers[sourceFinger]);
						Vector2 inputRange = blendShapeTarget.inputRange;
						Vector2 outputRange = blendShapeTarget.outputRange;
						float num = MathUtils.Linear(lerpValue, inputRange.x, inputRange.y, outputRange.x, outputRange.y);
						blendShapeTarget.currentValue = num;
						targetRenderer.SetBlendShapeWeight(blendShapeTarget.blendShapeIndex, num);
					}
				}
			}
		}
	}

	private static float GetLerpValue(VRMap map)
	{
		VRMapThumb vrmapThumb = map as VRMapThumb;
		float num;
		if (vrmapThumb == null)
		{
			VRMapIndex vrmapIndex = map as VRMapIndex;
			if (vrmapIndex == null)
			{
				VRMapMiddle vrmapMiddle = map as VRMapMiddle;
				if (vrmapMiddle == null)
				{
					num = 0f;
				}
				else
				{
					num = vrmapMiddle.calcT;
				}
			}
			else
			{
				num = vrmapIndex.calcT;
			}
		}
		else
		{
			num = ((vrmapThumb.calcT > 0.1f) ? 1f : 0f);
		}
		return num;
	}

	[SerializeField]
	private VRRig _rig;

	[SerializeField]
	private VRMap[] _fingers = new VRMap[0];

	[SerializeField]
	private FingerFlexReactor.BlendShapeTarget[] _blendShapeTargets = new FingerFlexReactor.BlendShapeTarget[0];

	[Serializable]
	public class BlendShapeTarget
	{
		public FingerFlexReactor.FingerMap sourceFinger;

		public SkinnedMeshRenderer targetRenderer;

		public int blendShapeIndex;

		public Vector2 inputRange = new Vector2(0f, 1f);

		public Vector2 outputRange = new Vector2(0f, 1f);

		[NonSerialized]
		public float currentValue;
	}

	public enum FingerMap
	{
		None = -1,
		LeftThumb,
		LeftIndex,
		LeftMiddle,
		RightThumb,
		RightIndex,
		RightMiddle
	}
}
