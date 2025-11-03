using System;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace GorillaLocomotion.Gameplay
{
	public sealed class SplineFollow : MonoBehaviour
	{
		public void Start()
		{
			base.transform.rotation *= this._rotationFix;
			this._smoothRotationTrackingRateExp = Mathf.Exp(this._smoothRotationTrackingRate);
			this._progress = this._splineProgressOffset;
			this._progressPerFixedUpdate = Time.fixedDeltaTime / this._duration;
			this._secondsToCycles = (double)(1f / this._duration);
			this._nativeSpline = new NativeSpline(this._unitySpline.Spline, this._unitySpline.transform.localToWorldMatrix, Allocator.Persistent);
			if (this._approximate)
			{
				this.CalculateApproximationNodes();
			}
		}

		private void CalculateApproximationNodes()
		{
			for (int i = 0; i < this._approximationResolution; i++)
			{
				float3 @float;
				float3 float2;
				float3 float3;
				this._nativeSpline.Evaluate((float)i / (float)this._approximationResolution, out @float, out float2, out float3);
				SplineFollow.SplineNode splineNode = new SplineFollow.SplineNode(@float, float2, float3);
				this._approximationNodes.Add(splineNode);
			}
			if (this._nativeSpline.Closed)
			{
				this._approximationNodes.Add(this._approximationNodes[0]);
			}
		}

		private void FixedUpdate()
		{
			if (!this._approximate)
			{
				this.FollowSpline();
			}
		}

		private void Update()
		{
			if (this._approximate)
			{
				this.FollowSpline();
			}
		}

		private void FollowSpline()
		{
			if (PhotonNetwork.InRoom)
			{
				double num = PhotonNetwork.Time * this._secondsToCycles + (double)this._splineProgressOffset;
				this._progress = (float)(num % 1.0);
			}
			else
			{
				this._progress = (this._progress + this._progressPerFixedUpdate) % 1f;
			}
			SplineFollow.SplineNode splineNode = this.EvaluateSpline(this._progress);
			base.transform.position = splineNode.Position;
			Quaternion quaternion = Quaternion.LookRotation(splineNode.Tangent) * this._rotationFix;
			base.transform.rotation = Quaternion.Slerp(quaternion, base.transform.rotation, Mathf.Exp(-this._smoothRotationTrackingRateExp * Time.deltaTime));
		}

		private SplineFollow.SplineNode EvaluateSpline(float t)
		{
			t %= 1f;
			if (this._approximate)
			{
				float num = t * (float)this._approximationNodes.Count;
				int num2 = (int)num;
				float num3 = num - (float)num2;
				num2 %= this._approximationNodes.Count;
				SplineFollow.SplineNode splineNode = this._approximationNodes[num2];
				SplineFollow.SplineNode splineNode2 = this._approximationNodes[(num2 + 1) % this._approximationNodes.Count];
				return SplineFollow.SplineNode.Lerp(splineNode, splineNode2, num3);
			}
			float3 @float;
			float3 float2;
			float3 float3;
			this._nativeSpline.Evaluate(t, out @float, out float2, out float3);
			return new SplineFollow.SplineNode(@float, float2, float3);
		}

		private void OnDestroy()
		{
			this._nativeSpline.Dispose();
		}

		[SerializeField]
		[Tooltip("If true, approximates the spline position. Only use when exact position does not matter.")]
		private bool _approximate;

		[SerializeField]
		private SplineContainer _unitySpline;

		[SerializeField]
		private float _duration;

		private double _secondsToCycles;

		[SerializeField]
		private float _smoothRotationTrackingRate = 0.5f;

		private float _smoothRotationTrackingRateExp;

		private float _progressPerFixedUpdate;

		[SerializeField]
		private float _splineProgressOffset;

		[SerializeField]
		private Quaternion _rotationFix = Quaternion.identity;

		private NativeSpline _nativeSpline;

		private float _progress;

		[Header("Approximate Spline Parameters")]
		[SerializeField]
		[Range(4f, 200f)]
		private int _approximationResolution = 100;

		private readonly List<SplineFollow.SplineNode> _approximationNodes = new List<SplineFollow.SplineNode>();

		private struct SplineNode
		{
			public SplineNode(Vector3 position, Vector3 tangent, Vector3 up)
			{
				this.Position = position;
				this.Tangent = tangent;
				this.Up = up;
			}

			public static SplineFollow.SplineNode Lerp(SplineFollow.SplineNode a, SplineFollow.SplineNode b, float t)
			{
				return new SplineFollow.SplineNode(Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.Tangent, b.Tangent, t), Vector3.Lerp(a.Up, b.Up, t));
			}

			public readonly Vector3 Position;

			public readonly Vector3 Tangent;

			public readonly Vector3 Up;
		}
	}
}
