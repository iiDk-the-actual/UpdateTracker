using System;
using GorillaExtensions;
using UnityEngine;

public class SpringyWobbler : MonoBehaviour
{
	private void Start()
	{
		int num = 1;
		Transform transform = base.transform;
		while (transform.childCount > 0)
		{
			transform = transform.GetChild(0);
			num++;
		}
		this.children = new Transform[num];
		transform = base.transform;
		this.children[0] = transform;
		int num2 = 1;
		while (transform.childCount > 0)
		{
			transform = transform.GetChild(0);
			this.children[num2] = transform;
			num2++;
		}
		this.lastEndpointWorldPos = this.children[this.children.Length - 1].transform.position;
	}

	private void Update()
	{
		float x = base.transform.lossyScale.x;
		Vector3 vector = base.transform.TransformPoint(this.idealEndpointLocalPos);
		this.endpointVelocity += (vector - this.lastEndpointWorldPos) * this.stabilizingForce * x * Time.deltaTime;
		Vector3 vector2 = this.lastEndpointWorldPos + this.endpointVelocity * Time.deltaTime;
		float num = this.maxDisplacement * x;
		if ((vector2 - vector).IsLongerThan(num))
		{
			vector2 = vector + (vector2 - vector).normalized * num;
		}
		this.endpointVelocity = (vector2 - this.lastEndpointWorldPos) * (1f - this.drag) / Time.deltaTime;
		Vector3 vector3 = base.transform.TransformPoint(this.rotateToFaceLocalPos);
		Vector3 vector4 = base.transform.TransformDirection(Vector3.up);
		Vector3 position = base.transform.position;
		Vector3 vector5 = position + base.transform.TransformDirection(this.idealEndpointLocalPos) * this.startStiffness * x;
		Vector3 vector6 = vector2;
		Vector3 vector7 = vector6 + (vector3 - vector6).normalized * this.endStiffness * x;
		for (int i = 1; i < this.children.Length; i++)
		{
			float num2 = (float)i / (float)(this.children.Length - 1);
			Vector3 vector8 = BezierUtils.BezierSolve(num2, position, vector5, vector7, vector6);
			Vector3 vector9 = BezierUtils.BezierSolve(num2 + 0.1f, position, vector5, vector7, vector6);
			this.children[i].transform.position = vector8;
			this.children[i].transform.rotation = Quaternion.LookRotation(vector9 - vector8, vector4);
		}
		this.lastIdealEndpointWorldPos = vector;
		this.lastEndpointWorldPos = vector2;
	}

	[SerializeField]
	private float stabilizingForce;

	[SerializeField]
	private float drag;

	[SerializeField]
	private float maxDisplacement;

	private Transform[] children;

	[SerializeField]
	private Vector3 idealEndpointLocalPos;

	[SerializeField]
	private Vector3 rotateToFaceLocalPos;

	[SerializeField]
	private float startStiffness;

	[SerializeField]
	private float endStiffness;

	private Vector3 lastIdealEndpointWorldPos;

	private Vector3 lastEndpointWorldPos;

	private Vector3 endpointVelocity;
}
