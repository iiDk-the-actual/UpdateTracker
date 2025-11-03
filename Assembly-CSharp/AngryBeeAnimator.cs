using System;
using UnityEngine;

public class AngryBeeAnimator : MonoBehaviour
{
	private void Awake()
	{
		this.bees = new GameObject[this.numBees];
		this.beeOrbits = new GameObject[this.numBees];
		this.beeOrbitalRadii = new float[this.numBees];
		this.beeOrbitalAxes = new Vector3[this.numBees];
		for (int i = 0; i < this.numBees; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.transform;
			Vector2 vector = Random.insideUnitCircle * this.orbitMaxCenterDisplacement;
			gameObject.transform.localPosition = new Vector3(vector.x, Random.Range(-this.orbitMaxHeightDisplacement, this.orbitMaxHeightDisplacement), vector.y);
			gameObject.transform.localRotation = Quaternion.Euler(Random.Range(-this.orbitMaxTilt, this.orbitMaxTilt), (float)Random.Range(0, 360), 0f);
			this.beeOrbitalAxes[i] = gameObject.transform.up;
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.beePrefab, gameObject.transform);
			float num = Random.Range(this.orbitMinRadius, this.orbitMaxRadius);
			this.beeOrbitalRadii[i] = num;
			gameObject2.transform.localPosition = Vector3.forward * num;
			gameObject2.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
			gameObject2.transform.localScale = Vector3.one * this.beeScale;
			this.bees[i] = gameObject2;
			this.beeOrbits[i] = gameObject;
		}
	}

	private void Update()
	{
		float num = this.orbitSpeed * Time.deltaTime;
		for (int i = 0; i < this.numBees; i++)
		{
			this.beeOrbits[i].transform.Rotate(this.beeOrbitalAxes[i], num);
		}
	}

	public void SetEmergeFraction(float fraction)
	{
		for (int i = 0; i < this.numBees; i++)
		{
			this.bees[i].transform.localPosition = Vector3.forward * fraction * this.beeOrbitalRadii[i];
		}
	}

	[SerializeField]
	private GameObject beePrefab;

	[SerializeField]
	private int numBees;

	[SerializeField]
	private float orbitMinRadius;

	[SerializeField]
	private float orbitMaxRadius;

	[SerializeField]
	private float orbitMaxHeightDisplacement;

	[SerializeField]
	private float orbitMaxCenterDisplacement;

	[SerializeField]
	private float orbitMaxTilt;

	[SerializeField]
	private float orbitSpeed;

	[SerializeField]
	private float beeScale;

	private GameObject[] beeOrbits;

	private GameObject[] bees;

	private Vector3[] beeOrbitalAxes;

	private float[] beeOrbitalRadii;
}
