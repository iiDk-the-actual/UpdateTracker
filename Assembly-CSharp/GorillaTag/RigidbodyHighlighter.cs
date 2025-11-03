using System;
using System.Collections.Generic;
using GorillaExtensions;
using JetBrains.Annotations;
using UnityEngine;

namespace GorillaTag
{
	public class RigidbodyHighlighter : MonoBehaviour
	{
		private string ButtonText
		{
			get
			{
				if (!this.Active)
				{
					return "Highlight Rigidbodies";
				}
				return "Unhighlight Rigidbodies";
			}
		}

		public bool Active { get; set; }

		private void Awake()
		{
			Object.Destroy(base.gameObject);
			if (RigidbodyHighlighter.Instance != null && RigidbodyHighlighter.Instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			RigidbodyHighlighter.Instance = this;
			this._lineRenderer.startWidth = this._lineWidth;
			this._lineRenderer.endWidth = this._lineWidth;
		}

		private void Update()
		{
			if (!this.Active)
			{
				this._lineRenderer.positionCount = 0;
				return;
			}
			this._rigidbodies.Clear();
			this._rigidbodies.AddAll(RigidbodyHighlighter.GetAwakeRigidbodies());
			this.DrawTracers();
			foreach (Rigidbody rigidbody in this._rigidbodies)
			{
				RigidbodyHighlighter.DrawBox(rigidbody.transform, Color.red, 0.1f);
			}
		}

		private static List<Rigidbody> GetAwakeRigidbodies()
		{
			List<Rigidbody> list = new List<Rigidbody>();
			Object[] array = Object.FindObjectsByType(typeof(Rigidbody), FindObjectsSortMode.None);
			for (int i = 0; i < array.Length; i++)
			{
				Rigidbody rigidbody = array[i] as Rigidbody;
				if (rigidbody == null)
				{
					throw new Exception("Non-rigidbody found by FindObjectsByType.");
				}
				if (!rigidbody.IsSleeping())
				{
					list.Add(rigidbody);
				}
			}
			return list;
		}

		private void HighlightActiveRigidbodies()
		{
			this.Active = !this.Active;
		}

		private void GetRigidbodyNames()
		{
			List<Rigidbody> list = ((this._rigidbodies.Count > 0) ? this._rigidbodies : RigidbodyHighlighter.GetAwakeRigidbodies());
			for (int i = 0; i < list.Count; i++)
			{
				Debug.Log(string.Format("Rigidbody {0} of {1}: {2}", i, list.Count, list[i].name));
			}
		}

		private void OnDrawGizmos()
		{
			if (!this.Active)
			{
				return;
			}
			Gizmos.color = Color.red;
			foreach (Rigidbody rigidbody in this._rigidbodies)
			{
				Gizmos.DrawWireCube(rigidbody.transform.position, Vector3.one);
			}
		}

		private static void DrawBox(Transform tx, Color color, float duration)
		{
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(tx.position, tx.rotation, tx.lossyScale);
			Vector3 vector = matrix4x.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));
			Vector3 vector2 = matrix4x.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
			Vector3 vector3 = matrix4x.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));
			Vector3 vector4 = matrix4x.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
			Vector3 vector5 = matrix4x.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
			Vector3 vector6 = matrix4x.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
			Vector3 vector7 = matrix4x.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
			Vector3 vector8 = matrix4x.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
			Debug.DrawLine(vector, vector2, color, duration, false);
			Debug.DrawLine(vector2, vector4, color, duration, false);
			Debug.DrawLine(vector4, vector3, color, duration, false);
			Debug.DrawLine(vector3, vector, color, duration, false);
			Debug.DrawLine(vector8, vector7, color, duration, false);
			Debug.DrawLine(vector7, vector5, color, duration, false);
			Debug.DrawLine(vector5, vector6, color, duration, false);
			Debug.DrawLine(vector6, vector8, color, duration, false);
			Debug.DrawLine(vector, vector5, color, duration, false);
			Debug.DrawLine(vector2, vector6, color, duration, false);
			Debug.DrawLine(vector3, vector7, color, duration, false);
			Debug.DrawLine(vector4, vector8, color, duration, false);
		}

		private void DrawTracers()
		{
			Vector3[] array = new Vector3[this._rigidbodies.Count * 2 + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ((i % 2 == 0) ? (Camera.main.transform.position + this._tracerOffset) : this._rigidbodies[i / 2].transform.position);
			}
			this._lineRenderer.positionCount = array.Length;
			this._lineRenderer.SetPositions(array);
		}

		[CanBeNull]
		public static RigidbodyHighlighter Instance;

		[SerializeField]
		private float _inGameDuration = 10f;

		[SerializeField]
		private LineRenderer _lineRenderer;

		[SerializeField]
		private float _lineWidth = 0.01f;

		[SerializeField]
		private Vector3 _tracerOffset = 0.5f * Vector3.down;

		private readonly List<Rigidbody> _rigidbodies = new List<Rigidbody>();
	}
}
