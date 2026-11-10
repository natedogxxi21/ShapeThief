using UnityEngine.InputSystem.EnhancedTouch;

public class TutPlayerTouchMovement : PlayerTouchMovement
{
	protected override void HandleFingerMove(Finger movedFinger)
	{
		base.HandleFingerMove(movedFinger);
		if (movedFinger == lookFinger)
		{
			TutorialFlow.looked = true;
		}
		if (movedFinger == movementFinger)
		{
			TutorialFlow.moved = true;
		}
	}
}