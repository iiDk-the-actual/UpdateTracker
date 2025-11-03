using System;
using UnityEngine;

namespace GorillaTag.Rendering
{
	public class RendererCullerByTriggers : MonoBehaviour, IBuildValidation
	{
		protected void OnEnable()
		{
			this.camWasTouching = false;
			foreach (Renderer renderer in this.renderers)
			{
				if (renderer != null)
				{
					renderer.enabled = false;
				}
			}
			if (this.mainCameraTransform == null)
			{
				this.mainCameraTransform = Camera.main.transform;
			}
		}

		protected void LateUpdate()
		{
			if (this.mainCameraTransform == null)
			{
				this.mainCameraTransform = Camera.main.transform;
			}
			Vector3 position = this.mainCameraTransform.position;
			bool flag = false;
			foreach (Collider collider in this.colliders)
			{
				if (!(collider == null) && (collider.ClosestPoint(position) - position).sqrMagnitude < 0.010000001f)
				{
					flag = true;
					break;
				}
			}
			if (this.camWasTouching == flag)
			{
				return;
			}
			this.camWasTouching = flag;
			foreach (Renderer renderer in this.renderers)
			{
				if (renderer != null)
				{
					renderer.enabled = flag;
				}
			}
		}

		public bool BuildValidationCheck()
		{
			for (int i = 0; i < this.renderers.Length; i++)
			{
				if (this.renderers[i] == null)
				{
					Debug.LogError("rendererculllerbytriggers has null renderer", base.gameObject);
					return false;
				}
			}
			for (int j = 0; j < this.colliders.Length; j++)
			{
				if (this.colliders[j] == null)
				{
					Debug.LogError("rendererculllerbytriggers has null collider", base.gameObject);
					return false;
				}
			}
			return true;
		}

		[Tooltip("These renderers will be enabled/disabled depending on if the main camera is the colliders.")]
		public Renderer[] renderers;

		public Collider[] colliders;

		private bool camWasTouching;

		private const float cameraRadiusSq = 0.010000001f;

		private Transform mainCameraTransform;
	}
}
