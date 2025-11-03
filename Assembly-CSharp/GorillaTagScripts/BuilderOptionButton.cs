using System;

namespace GorillaTagScripts
{
	public class BuilderOptionButton : GorillaPressableButton
	{
		public override void Start()
		{
			base.Start();
		}

		private void OnDestroy()
		{
		}

		public void Setup(Action<BuilderOptionButton, bool> onPressed)
		{
			this.onPressed = onPressed;
		}

		public override void ButtonActivationWithHand(bool isLeftHand)
		{
			Action<BuilderOptionButton, bool> action = this.onPressed;
			if (action == null)
			{
				return;
			}
			action(this, isLeftHand);
		}

		public void SetPressed(bool pressed)
		{
			this.buttonRenderer.material = (pressed ? this.pressedMaterial : this.unpressedMaterial);
		}

		private new Action<BuilderOptionButton, bool> onPressed;
	}
}
