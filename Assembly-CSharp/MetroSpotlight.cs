using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MetroSpotlight : MonoBehaviour
{
	public void Tick()
	{
		if (!this._light)
		{
			return;
		}
		if (!this._target)
		{
			return;
		}
		this._time += this.speed * Time.deltaTime * Time.deltaTime;
		Vector3 position = this._target.position;
		Vector3 normalized = (position - this._light.position).normalized;
		Vector3 vector = Vector3.Cross(normalized, this._blimp.forward);
		Vector3 vector2 = Vector3.Cross(normalized, vector);
		Vector3 vector3 = MetroSpotlight.Figure8(position, vector, vector2, this._radius, this._time, this._offset, this._theta);
		this._light.LookAt(vector3);
	}

	private static Vector3 Figure8(Vector3 origin, Vector3 xDir, Vector3 yDir, float scale, float t, float offset, float theta)
	{
		float num = 2f / (3f - Mathf.Cos(2f * (t + offset)));
		float num2 = scale * num * Mathf.Cos(t + offset);
		float num3 = scale * num * Mathf.Sin(2f * (t + offset)) / 2f;
		Vector3 vector = Vector3.Cross(xDir, yDir);
		Quaternion quaternion = Quaternion.AngleAxis(theta, vector);
		xDir = quaternion * xDir;
		yDir = quaternion * yDir;
		Vector3 vector2 = xDir * num2 + yDir * num3;
		return origin + vector2;
	}

	[SerializeField]
	private Transform _blimp;

	[SerializeField]
	private Transform _light;

	[SerializeField]
	private Transform _target;

	[FormerlySerializedAs("_scale")]
	[SerializeField]
	private float _radius = 1f;

	[SerializeField]
	private float _offset;

	[SerializeField]
	private float _theta;

	public float speed = 16f;

	[Space]
	private float _time;
}
