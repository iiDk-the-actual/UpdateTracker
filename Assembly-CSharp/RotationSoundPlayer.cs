using System;
using System.Collections.Generic;
using UnityEngine;

public class RotationSoundPlayer : MonoBehaviour
{
	private void Awake()
	{
		List<Transform> list = new List<Transform>(this.transforms);
		list.RemoveAll((Transform xform) => xform == null);
		this.transforms = list.ToArray();
		this.initialUpAxis = new Vector3[this.transforms.Length];
		this.lastUpAxis = new Vector3[this.transforms.Length];
		this.lastRotationSpeeds = new float[this.transforms.Length];
		for (int i = 0; i < this.transforms.Length; i++)
		{
			this.initialUpAxis[i] = this.transforms[i].localRotation * Vector3.up;
			this.lastUpAxis[i] = this.initialUpAxis[i];
			this.lastRotationSpeeds[i] = 0f;
		}
	}

	private void Update()
	{
		this.cooldownTimer -= Time.deltaTime;
		for (int i = 0; i < this.transforms.Length; i++)
		{
			Vector3 vector = this.transforms[i].localRotation * Vector3.up;
			float num = Vector3.Angle(vector, this.initialUpAxis[i]);
			float num2 = Vector3.Angle(vector, this.lastUpAxis[i]);
			float deltaTime = Time.deltaTime;
			float num3 = num2 / deltaTime;
			if (this.cooldownTimer <= 0f && num > this.rotationAmountThreshold && num3 > this.rotationSpeedThreshold && !this.soundBankPlayer.isPlaying)
			{
				this.cooldownTimer = this.cooldown;
				this.soundBankPlayer.Play();
			}
			this.lastUpAxis[i] = vector;
			this.lastRotationSpeeds[i] = num3;
		}
	}

	[Tooltip("Transforms that will make a noise when they rotate.")]
	[SerializeField]
	private Transform[] transforms;

	[SerializeField]
	private SoundBankPlayer soundBankPlayer;

	[Tooltip("How much the transform must rotate from it's initial rotation before a sound is played.")]
	private float rotationAmountThreshold = 30f;

	[Tooltip("How fast the transform must rotate before a sound is played.")]
	private float rotationSpeedThreshold = 45f;

	private float cooldown = 0.6f;

	private float cooldownTimer;

	private Vector3[] initialUpAxis;

	private Vector3[] lastUpAxis;

	private float[] lastRotationSpeeds;
}
