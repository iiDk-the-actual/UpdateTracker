using System;
using System.Collections.Generic;
using BoingKit;
using UnityEngine;

public class BushFieldReactorMain : MonoBehaviour
{
	public void Start()
	{
		Random.InitState(0);
		for (int i = 0; i < this.NumBushes; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.Bush);
			float num = Random.Range(this.BushScaleRange.x, this.BushScaleRange.y);
			gameObject.transform.position = new Vector3(Random.Range(-0.5f * this.FieldBounds.x, 0.5f * this.FieldBounds.x), 0.2f * num, Random.Range(-0.5f * this.FieldBounds.y, 0.5f * this.FieldBounds.y));
			gameObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			gameObject.transform.localScale = num * Vector3.one;
			BoingBehavior component = gameObject.GetComponent<BoingBehavior>();
			if (component != null)
			{
				component.Reboot();
			}
		}
		for (int j = 0; j < this.NumBlossoms; j++)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(this.Blossom);
			float num2 = Random.Range(this.BlossomScaleRange.x, this.BlossomScaleRange.y);
			gameObject2.transform.position = new Vector3(Random.Range(-0.5f * this.FieldBounds.x, 0.5f * this.FieldBounds.y), 0.2f * num2, Random.Range(-0.5f * this.FieldBounds.y, 0.5f * this.FieldBounds.y));
			gameObject2.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			gameObject2.transform.localScale = num2 * Vector3.one;
			BoingBehavior component2 = gameObject2.GetComponent<BoingBehavior>();
			if (component2 != null)
			{
				component2.Reboot();
			}
		}
		this.m_aSphere = new List<GameObject>(this.NumSpheresPerCircle * this.NumCircles);
		for (int k = 0; k < this.NumCircles; k++)
		{
			for (int l = 0; l < this.NumSpheresPerCircle; l++)
			{
				this.m_aSphere.Add(Object.Instantiate<GameObject>(this.Sphere));
			}
		}
		this.m_basePhase = 0f;
	}

	public void Update()
	{
		int num = 0;
		for (int i = 0; i < this.NumCircles; i++)
		{
			float num2 = this.MaxCircleRadius / (float)(i + 1);
			for (int j = 0; j < this.NumSpheresPerCircle; j++)
			{
				float num3 = this.m_basePhase + (float)j / (float)this.NumSpheresPerCircle * 2f * 3.1415927f;
				num3 *= ((i % 2 == 0) ? 1f : (-1f));
				this.m_aSphere[num].transform.position = new Vector3(num2 * Mathf.Cos(num3), 0.2f, num2 * Mathf.Sin(num3));
				num++;
			}
		}
		this.m_basePhase -= this.CircleSpeed / this.MaxCircleRadius * Time.deltaTime;
	}

	public GameObject Bush;

	public GameObject Blossom;

	public GameObject Sphere;

	public int NumBushes;

	public Vector2 BushScaleRange;

	public int NumBlossoms;

	public Vector2 BlossomScaleRange;

	public Vector2 FieldBounds;

	public int NumSpheresPerCircle;

	public int NumCircles;

	public float MaxCircleRadius;

	public float CircleSpeed;

	private List<GameObject> m_aSphere;

	private float m_basePhase;
}
