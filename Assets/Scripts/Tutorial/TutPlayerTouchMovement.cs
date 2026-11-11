using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class TutPlayerTouchMovement : PlayerTouchMovement
{
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		TutorialFlow.moved += rb.linearVelocity.magnitude * Time.fixedDeltaTime;
	}

	protected override void HandleFingerMove(Finger movedFinger)
	{
		base.HandleFingerMove(movedFinger);
		if (movedFinger == lookFinger)
		{
			TutorialFlow.looked += movedFinger.currentTouch.delta.magnitude;
		}
	}

	public override bool ToggleRun()
	{
		Running = !Running;
		if (Running) { TutorialFlow.ran = true; }
		return Running;
	}
}