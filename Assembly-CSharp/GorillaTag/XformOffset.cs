using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag
{
	[Serializable]
	public struct XformOffset
	{
		[Tooltip("The rotation of the offset relative to the parent bone.")]
		public Quaternion rot
		{
			get
			{
				return this._rotQuat;
			}
			set
			{
				this._rotQuat = value;
			}
		}

		public XformOffset(Vector3 pos, Quaternion rot, Vector3 scale)
		{
			this.pos = pos;
			this._rotQuat = rot;
			this._rotEulerAngles = rot.eulerAngles;
			this.scale = scale;
		}

		public XformOffset(Vector3 pos, Vector3 rot, Vector3 scale)
		{
			this.pos = pos;
			this._rotQuat = Quaternion.Euler(rot);
			this._rotEulerAngles = rot;
			this.scale = scale;
		}

		public XformOffset(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this._rotQuat = rot;
			this._rotEulerAngles = rot.eulerAngles;
			this.scale = Vector3.one;
		}

		public XformOffset(Vector3 pos, Vector3 rot)
		{
			this.pos = pos;
			this._rotQuat = Quaternion.Euler(rot);
			this._rotEulerAngles = rot;
			this.scale = Vector3.one;
		}

		public XformOffset(Transform parentXform, Transform childXform)
		{
			this.pos = parentXform.InverseTransformPoint(childXform.position);
			this._rotQuat = Quaternion.Inverse(parentXform.rotation) * childXform.rotation;
			this._rotEulerAngles = this._rotQuat.eulerAngles;
			this.scale = childXform.lossyScale.SafeDivide(parentXform.lossyScale);
		}

		public XformOffset(Matrix4x4 matrix)
		{
			this.pos = matrix.GetPosition();
			this.scale = matrix.lossyScale;
			if (Vector3.Dot(Vector3.Cross(matrix.GetColumn(0), matrix.GetColumn(1)), matrix.GetColumn(2)) < 0f)
			{
				this.scale = -this.scale;
			}
			Matrix4x4 matrix4x = matrix;
			matrix4x.SetColumn(0, matrix4x.GetColumn(0) / this.scale.x);
			matrix4x.SetColumn(1, matrix4x.GetColumn(1) / this.scale.y);
			matrix4x.SetColumn(2, matrix4x.GetColumn(2) / this.scale.z);
			this._rotQuat = Quaternion.LookRotation(matrix4x.GetColumn(2), matrix4x.GetColumn(1));
			this._rotEulerAngles = this._rotQuat.eulerAngles;
		}

		public bool Approx(XformOffset other)
		{
			return this.pos.Approx(other.pos, 1E-05f) && this._rotQuat.Approx(other._rotQuat, 1E-06f) && this.scale.Approx(other.scale, 1E-05f);
		}

		[Tooltip("The position of the offset relative to the parent bone.")]
		public Vector3 pos;

		[FormerlySerializedAs("_edRotQuat")]
		[FormerlySerializedAs("rot")]
		[HideInInspector]
		[SerializeField]
		private Quaternion _rotQuat;

		[FormerlySerializedAs("_edRotEulerAngles")]
		[FormerlySerializedAs("_edRotEuler")]
		[HideInInspector]
		[SerializeField]
		private Vector3 _rotEulerAngles;

		[Tooltip("The scale of the offset relative to the parent bone.")]
		public Vector3 scale;

		public static readonly XformOffset Identity = new XformOffset
		{
			_rotQuat = Quaternion.identity,
			scale = Vector3.one
		};
	}
}
