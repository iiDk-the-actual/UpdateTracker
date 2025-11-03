using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class HandControlledSettingsSO : ScriptableObject
	{
		private bool IsAngle
		{
			get
			{
				return this.rotationControl == HandControlledCosmetic.RotationControl.Angle;
			}
		}

		private bool IsTranslation
		{
			get
			{
				return this.rotationControl == HandControlledCosmetic.RotationControl.Translation;
			}
		}

		private const string SENS_TT = "The difference between the current input and cached input is magnified by this number.";

		public HandControlledCosmetic.RotationControl rotationControl;

		[Tooltip("The difference between the current input and cached input is magnified by this number.")]
		public float inputSensitivity = 2f;

		[Tooltip("The difference between the current input and cached input is magnified by this number.")]
		public AnimationCurve verticalSensitivity = AnimationCurve.Constant(0f, 1f, 2f);

		[Tooltip("The difference between the current input and cached input is magnified by this number.")]
		public AnimationCurve horizontalSensitivity = AnimationCurve.Constant(0f, 1f, 2f);

		[Tooltip("How quickly the cached input approaches the current input. A high value will function more like a mouse, while a low value will function more like a joystick.")]
		public float inputDecaySpeed = 1f;

		[Tooltip("How quickly the cached input approaches the current input, as a function of distance. A high value will function more like a mouse, while a low value will function more like a joystick.")]
		public AnimationCurve inputDecayCurve = AnimationCurve.Constant(0f, 2f, 1f);

		[Tooltip("How quickly the transform approaches the intended angle (smaller value = more lag).")]
		public float rotationSpeed = 20f;

		[Tooltip("The transform's local rotation cannot exceed these euler angles.")]
		public Vector3 angleLimits = new Vector3(45f, 360f, 0f);
	}
}
