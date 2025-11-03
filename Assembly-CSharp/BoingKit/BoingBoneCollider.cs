using System;
using UnityEngine;

namespace BoingKit
{
	public class BoingBoneCollider : MonoBehaviour
	{
		public Bounds Bounds
		{
			get
			{
				switch (this.Shape)
				{
				case BoingBoneCollider.Type.Sphere:
				{
					float num = VectorUtil.MinComponent(base.transform.localScale);
					return new Bounds(base.transform.position, 2f * num * this.Radius * Vector3.one);
				}
				case BoingBoneCollider.Type.Capsule:
				{
					float num2 = VectorUtil.MinComponent(base.transform.localScale);
					return new Bounds(base.transform.position, 2f * num2 * this.Radius * Vector3.one + this.Height * VectorUtil.ComponentWiseAbs(base.transform.rotation * Vector3.up));
				}
				case BoingBoneCollider.Type.Box:
					return new Bounds(base.transform.position, VectorUtil.ComponentWiseMult(base.transform.localScale, VectorUtil.ComponentWiseAbs(base.transform.rotation * this.Dimensions)));
				default:
					return default(Bounds);
				}
			}
		}

		public bool Collide(Vector3 boneCenter, float boneRadius, out Vector3 push)
		{
			switch (this.Shape)
			{
			case BoingBoneCollider.Type.Sphere:
			{
				float num = VectorUtil.MinComponent(base.transform.localScale);
				return Collision.SphereSphere(boneCenter, boneRadius, base.transform.position, num * this.Radius, out push);
			}
			case BoingBoneCollider.Type.Capsule:
			{
				float num2 = VectorUtil.MinComponent(base.transform.localScale);
				Vector3 vector = base.transform.TransformPoint(0.5f * this.Height * Vector3.up);
				Vector3 vector2 = base.transform.TransformPoint(0.5f * this.Height * Vector3.down);
				return Collision.SphereCapsule(boneCenter, boneRadius, vector, vector2, num2 * this.Radius, out push);
			}
			case BoingBoneCollider.Type.Box:
			{
				Vector3 vector3 = base.transform.InverseTransformPoint(boneCenter);
				Vector3 vector4 = 0.5f * VectorUtil.ComponentWiseMult(base.transform.localScale, this.Dimensions);
				if (!Collision.SphereBox(vector3, boneRadius, vector4, out push))
				{
					return false;
				}
				push = base.transform.TransformVector(push);
				return true;
			}
			default:
				push = Vector3.zero;
				return false;
			}
		}

		public void OnValidate()
		{
			this.Radius = Mathf.Max(0f, this.Radius);
			this.Dimensions.x = Mathf.Max(0f, this.Dimensions.x);
			this.Dimensions.y = Mathf.Max(0f, this.Dimensions.y);
			this.Dimensions.z = Mathf.Max(0f, this.Dimensions.z);
		}

		public void OnDrawGizmos()
		{
			this.DrawGizmos();
		}

		public void DrawGizmos()
		{
			switch (this.Shape)
			{
			case BoingBoneCollider.Type.Sphere:
			{
				float num = VectorUtil.MinComponent(base.transform.localScale) * this.Radius;
				Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
				if (this.Shape == BoingBoneCollider.Type.Sphere)
				{
					Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
					Gizmos.DrawSphere(Vector3.zero, num);
				}
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(Vector3.zero, num);
				Gizmos.matrix = Matrix4x4.identity;
				return;
			}
			case BoingBoneCollider.Type.Capsule:
			{
				float num2 = VectorUtil.MinComponent(base.transform.localScale);
				float num3 = num2 * this.Radius;
				float num4 = 0.5f * num2 * this.Height;
				Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
				if (this.Shape == BoingBoneCollider.Type.Capsule)
				{
					Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
					Gizmos.DrawSphere(num4 * Vector3.up, num3);
					Gizmos.DrawSphere(num4 * Vector3.down, num3);
				}
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(num4 * Vector3.up, num3);
				Gizmos.DrawWireSphere(num4 * Vector3.down, num3);
				for (int i = 0; i < 4; i++)
				{
					float num5 = (float)i * MathUtil.HalfPi;
					Vector3 vector = new Vector3(num3 * Mathf.Cos(num5), 0f, num3 * Mathf.Sin(num5));
					Gizmos.DrawLine(vector + num4 * Vector3.up, vector + num4 * Vector3.down);
				}
				Gizmos.matrix = Matrix4x4.identity;
				return;
			}
			case BoingBoneCollider.Type.Box:
			{
				Vector3 vector2 = VectorUtil.ComponentWiseMult(base.transform.localScale, this.Dimensions);
				Gizmos.matrix = base.transform.localToWorldMatrix;
				if (this.Shape == BoingBoneCollider.Type.Box)
				{
					Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
					Gizmos.DrawCube(Vector3.zero, vector2);
				}
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(Vector3.zero, vector2);
				Gizmos.matrix = Matrix4x4.identity;
				return;
			}
			default:
				return;
			}
		}

		public BoingBoneCollider.Type Shape;

		public float Radius = 0.1f;

		public float Height = 0.25f;

		public Vector3 Dimensions = new Vector3(0.1f, 0.1f, 0.1f);

		public enum Type
		{
			Sphere,
			Capsule,
			Box
		}
	}
}
