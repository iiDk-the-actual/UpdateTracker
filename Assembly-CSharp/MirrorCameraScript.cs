using System;
using UnityEngine;

public class MirrorCameraScript : MonoBehaviour
{
	private void Start()
	{
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
		GorillaTagger.Instance.MirrorCameraCullingMask.AddCallback(new Action<int>(this.MirrorCullingMaskChanged), true);
	}

	private void OnDestroy()
	{
		if (GorillaTagger.Instance != null)
		{
			GorillaTagger.Instance.MirrorCameraCullingMask.RemoveCallback(new Action<int>(this.MirrorCullingMaskChanged));
		}
	}

	private void MirrorCullingMaskChanged(int newMask)
	{
		this.mirrorCamera.cullingMask = newMask;
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		Vector3 right = base.transform.right;
		Vector4 vector = new Vector4(right.x, right.y, right.z, -Vector3.Dot(right, position));
		Matrix4x4 zero = Matrix4x4.zero;
		this.CalculateReflectionMatrix(ref zero, vector);
		this.mirrorCamera.worldToCameraMatrix = this.mainCamera.worldToCameraMatrix * zero;
		Vector4 vector2 = this.CameraSpacePlane(this.mirrorCamera, position, right, 1f);
		this.mirrorCamera.projectionMatrix = this.mainCamera.CalculateObliqueMatrix(vector2);
		Debug.Log(string.Format("Main Camera position {0}", this.mainCamera.transform.position));
		this.mirrorCamera.transform.position = zero.MultiplyPoint(this.mainCamera.transform.position);
		Debug.Log(string.Format("Reflected Camera position {0}", this.mirrorCamera.transform.position));
		this.mirrorCamera.transform.rotation = this.mainCamera.transform.rotation * Quaternion.Inverse(base.transform.rotation);
		Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (Material material in componentsInChildren[i].sharedMaterials)
			{
				if (material.shader == Shader.Find("Reflection"))
				{
					material.SetTexture("_ReflectionTex", this.mirrorCamera.targetTexture);
				}
			}
		}
	}

	private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
	{
		reflectionMatrix.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMatrix.m01 = -2f * plane[0] * plane[1];
		reflectionMatrix.m02 = -2f * plane[0] * plane[2];
		reflectionMatrix.m03 = -2f * plane[3] * plane[0];
		reflectionMatrix.m10 = -2f * plane[1] * plane[0];
		reflectionMatrix.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMatrix.m12 = -2f * plane[1] * plane[2];
		reflectionMatrix.m13 = -2f * plane[3] * plane[1];
		reflectionMatrix.m20 = -2f * plane[2] * plane[0];
		reflectionMatrix.m21 = -2f * plane[2] * plane[1];
		reflectionMatrix.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMatrix.m23 = -2f * plane[3] * plane[2];
		reflectionMatrix.m30 = 0f;
		reflectionMatrix.m31 = 0f;
		reflectionMatrix.m32 = 0f;
		reflectionMatrix.m33 = 1f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 vector = pos + normal * 0.07f;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 vector2 = worldToCameraMatrix.MultiplyPoint(vector);
		Vector3 vector3 = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(vector3.x, vector3.y, vector3.z, -Vector3.Dot(vector2, vector3));
	}

	public Camera mainCamera;

	public Camera mirrorCamera;
}
