using System;
using UnityEngine;

public class MouseRotationDrag : MonoBehaviour
{
	private void Start()
	{
		this.m_currFrameHasFocus = false;
		this.m_prevFrameHasFocus = false;
	}

	private void Update()
	{
		this.m_currFrameHasFocus = Application.isFocused;
		bool prevFrameHasFocus = this.m_prevFrameHasFocus;
		this.m_prevFrameHasFocus = this.m_currFrameHasFocus;
		if (!prevFrameHasFocus && !this.m_currFrameHasFocus)
		{
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		Vector3 prevMousePosition = this.m_prevMousePosition;
		Vector3 vector = mousePosition - prevMousePosition;
		this.m_prevMousePosition = mousePosition;
		if (!prevFrameHasFocus)
		{
			this.m_euler = base.transform.rotation.eulerAngles;
			return;
		}
		if (Input.GetMouseButton(0))
		{
			this.m_euler.x = this.m_euler.x + vector.y;
			this.m_euler.y = this.m_euler.y + vector.x;
			base.transform.rotation = Quaternion.Euler(this.m_euler);
		}
	}

	private bool m_currFrameHasFocus;

	private bool m_prevFrameHasFocus;

	private Vector3 m_prevMousePosition;

	private Vector3 m_euler;
}
