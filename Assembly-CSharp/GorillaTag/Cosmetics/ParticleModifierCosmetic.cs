using System;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class ParticleModifierCosmetic : MonoBehaviour
	{
		private void Awake()
		{
			this.StoreOriginalValues();
			this.currentIndex = -1;
		}

		private void OnValidate()
		{
			this.StoreOriginalValues();
		}

		private void OnEnable()
		{
			this.StoreOriginalValues();
		}

		private void OnDisable()
		{
			this.ResetToOriginal();
		}

		private void StoreOriginalValues()
		{
			if (this.ps == null)
			{
				return;
			}
			ParticleSystem.MainModule main = this.ps.main;
			this.originalStartSize = main.startSize.constant;
			this.originalStartColor = main.startColor.color;
		}

		public void ApplySetting(ParticleSettingsSO setting)
		{
			this.SetStartSize(setting.startSize);
			this.SetStartColor(setting.startColor);
		}

		public void ApplySettingLerp(ParticleSettingsSO setting)
		{
			this.LerpStartSize(setting.startSize);
			this.LerpStartColor(setting.startColor);
		}

		public void MoveToNextSetting()
		{
			this.currentIndex++;
			if (this.currentIndex > -1 && this.currentIndex < this.particleSettings.Length)
			{
				ParticleSettingsSO particleSettingsSO = this.particleSettings[this.currentIndex];
				this.ApplySetting(particleSettingsSO);
			}
		}

		public void MoveToNextSettingLerp()
		{
			this.currentIndex++;
			if (this.currentIndex > -1 && this.currentIndex < this.particleSettings.Length)
			{
				ParticleSettingsSO particleSettingsSO = this.particleSettings[this.currentIndex];
				this.ApplySettingLerp(particleSettingsSO);
			}
		}

		public void ResetSettings()
		{
			this.currentIndex = -1;
			this.ResetToOriginal();
		}

		public void MoveToSettingIndex(int index)
		{
			if (index > -1 && index < this.particleSettings.Length)
			{
				ParticleSettingsSO particleSettingsSO = this.particleSettings[index];
				this.ApplySetting(particleSettingsSO);
			}
		}

		public void MoveToSettingIndexLerp(int index)
		{
			if (index > -1 && index < this.particleSettings.Length)
			{
				ParticleSettingsSO particleSettingsSO = this.particleSettings[index];
				this.ApplySettingLerp(particleSettingsSO);
			}
		}

		public void SetStartSize(float size)
		{
			if (this.ps == null)
			{
				return;
			}
			this.ps.main.startSize = size;
			this.targetSize = null;
		}

		public void IncreaseStartSize(float delta)
		{
			if (this.ps == null)
			{
				return;
			}
			ParticleSystem.MainModule main = this.ps.main;
			float constant = main.startSize.constant;
			main.startSize = constant + delta;
			this.targetSize = null;
		}

		public void LerpStartSize(float size)
		{
			if (this.ps == null)
			{
				return;
			}
			if (Mathf.Abs(this.ps.main.startSize.constant - size) < 0.01f)
			{
				return;
			}
			this.targetSize = new float?(size);
		}

		public void SetStartColor(Color color)
		{
			if (this.ps == null)
			{
				return;
			}
			this.ps.main.startColor = color;
			this.targetColor = null;
		}

		public void LerpStartColor(Color color)
		{
			if (this.ps == null)
			{
				return;
			}
			Color color2 = this.ps.main.startColor.color;
			if (this.IsColorApproximatelyEqual(color2, color, 0.0001f))
			{
				return;
			}
			this.targetColor = new Color?(color);
		}

		public void SetStartValues(float size, Color color)
		{
			this.SetStartSize(size);
			this.SetStartColor(color);
		}

		public void LerpStartValues(float size, Color color)
		{
			this.LerpStartSize(size);
			this.LerpStartColor(color);
		}

		private void Update()
		{
			if (this.ps == null)
			{
				return;
			}
			ParticleSystem.MainModule main = this.ps.main;
			if (this.targetSize != null)
			{
				float num = Mathf.Lerp(main.startSize.constant, this.targetSize.Value, Time.deltaTime * this.transitionSpeed);
				main.startSize = num;
				if (Mathf.Abs(num - this.targetSize.Value) < 0.01f)
				{
					main.startSize = this.targetSize.Value;
					this.targetSize = null;
				}
			}
			if (this.targetColor != null)
			{
				Color color = Color.Lerp(main.startColor.color, this.targetColor.Value, Time.deltaTime * this.transitionSpeed);
				main.startColor = color;
				if (this.IsColorApproximatelyEqual(color, this.targetColor.Value, 0.0001f))
				{
					main.startColor = this.targetColor.Value;
					this.targetColor = null;
				}
			}
		}

		[ContextMenu("Reset To Original")]
		public void ResetToOriginal()
		{
			if (this.ps == null)
			{
				return;
			}
			this.targetSize = null;
			this.targetColor = null;
			ParticleSystem.MainModule main = this.ps.main;
			main.startSize = this.originalStartSize;
			main.startColor = this.originalStartColor;
		}

		private bool IsColorApproximatelyEqual(Color a, Color b, float threshold = 0.0001f)
		{
			float num = a.r - b.r;
			float num2 = a.g - b.g;
			float num3 = a.b - b.b;
			float num4 = a.a - b.a;
			return num * num + num2 * num2 + num3 * num3 + num4 * num4 < threshold;
		}

		[SerializeField]
		private ParticleSystem ps;

		[Tooltip("For calling gradual functions only")]
		[SerializeField]
		private float transitionSpeed = 5f;

		public ParticleSettingsSO[] particleSettings = new ParticleSettingsSO[0];

		private float originalStartSize;

		private Color originalStartColor;

		private float? targetSize;

		private Color? targetColor;

		private int currentIndex;
	}
}
