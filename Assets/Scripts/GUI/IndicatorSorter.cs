using UnityEngine;
using System.Collections.Generic;

public class IndicatorSorter : MonoBehaviour
{
	static readonly List<GuardHUD> sorted = new();

	void LateUpdate()
	{
		sorted.Clear();
		sorted.AddRange(GuardHUD.All);

		// farthest first, so the closest ends up last and draws on top
		sorted.Sort((a, b) => b.PlayerDistance.CompareTo(a.PlayerDistance));

		int index = 0;
		foreach (GuardHUD hud in sorted)
		{
			if (hud.Indicator == null) continue;
			if (hud.Indicator.GetSiblingIndex() != index)
				hud.Indicator.SetSiblingIndex(index);
			index++;
		}
	}
}