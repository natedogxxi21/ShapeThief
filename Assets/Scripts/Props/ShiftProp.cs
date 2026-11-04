using UnityEngine;
using ITISKIRUHERE;
using System;

public class ShiftProp : MonoBehaviour
{
	public Prop prop;

	[SerializeField] GameObject highlightGO;
	AdvancedOutline highlight;

	bool highlighted = false;
	bool dashed = false;

	void Awake()
	{
		if (!highlightGO.TryGetComponent(out AdvancedOutline outline))
		{ throw new Exception("Could not find AdvancedOutline on " + gameObject.name); }

		highlight = outline;
	}

	public void Highlight(bool v) { highlighted = v; EvaluateHighlight(); }
	public void HighlightDashed(bool v) { dashed = v; EvaluateHighlight(); }

	void EvaluateHighlight()
	{
		if (highlight != null)
		{
			if (highlighted)
			{
				highlightGO.SetActive(true);
				highlight.DashedPattern = false;
				highlight.OutlineWidth = 15;
				highlight.OutlineColor = new Color(0, .4f, 1, 1);
				highlight.RefreshRenderers();
			}
			else if (dashed)
			{
				highlightGO.SetActive(true);
				highlight.DashedPattern = true;
				highlight.OutlineWidth = 10;
				highlight.OutlineColor = new Color(0, 1, .5f, .15f);
				highlight.RefreshRenderers();
			}
			else
			{
				highlightGO.SetActive(false);
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.impulse.sqrMagnitude >= .5f)
		{
			Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
			AudioCueManager.PlayCollisionSound(prop, pos, collision.impulse.magnitude);
		}
	}
}