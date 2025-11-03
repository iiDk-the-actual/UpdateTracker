using System;
using UnityEngine;

[Serializable]
public class MonkeBallTeam
{
	public Color color;

	public int score;

	public Transform ballStartLocation;

	public Transform ballLaunchPosition;

	[Tooltip("The min/max random velocity of the ball when launched.")]
	public Vector2 ballLaunchVelocityRange = new Vector2(8f, 15f);

	[Tooltip("The min/max random x-angle of the ball when launched.")]
	public Vector2 ballLaunchAngleXRange = new Vector2(0f, 0f);

	[Tooltip("The min/max random y-angle of the ball when launched.")]
	public Vector2 ballLaunchAngleYRange = new Vector2(0f, 0f);
}
