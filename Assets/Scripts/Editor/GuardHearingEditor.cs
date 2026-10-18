using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GuardHearing))]
public class GuardHearingEditor : Editor
{
	const int Samples = 128;
	const float GraphHeight = 150f;
	const float PadLeft = 38f, PadRight = 10f, PadTop = 8f, PadBottom = 20f;

	static readonly Color Background = new(0.12f, 0.12f, 0.12f, 1f);
	static readonly Color GridColor = new(1f, 1f, 1f, 0.08f);
	static readonly Color AxisColor = new(1f, 1f, 1f, 0.35f);
	static readonly Color CurveColor = new(0.35f, 0.8f, 1f, 1f);
	static readonly Color CutoffColor = new(1f, 0.45f, 0.35f, 0.8f);
	static readonly Color CueColor = new(1f, 0.9f, 0.3f, 1f);

	GUIStyle labelStyle;

	// Repaints while a cue dot is visible, plus a short margin so the final repaint
	// happens after expiry and the dot actually disappears
	public override bool RequiresConstantRepaint()
	{
		if (!Application.isPlaying) { return false; }
		var cues = ((GuardHearing)target).RecentCues;
		return cues.Count > 0 &&
			Time.realtimeSinceStartup - cues[cues.Count - 1].time < GuardHearing.CueDebugLifetime + 0.25f;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space(8);
		EditorGUILayout.LabelField("Hearing Response", EditorStyles.boldLabel);

		DrawGraph((GuardHearing)target);
	}

	void DrawGraph(GuardHearing guard)
	{
		Rect outer = GUILayoutUtility.GetRect(0, GraphHeight, GUILayout.ExpandWidth(true));
		if (Event.current.type != EventType.Repaint) { return; }

		labelStyle ??= new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 1f, 1f, 0.7f) } };

		// Read maxDistance from the serialized data so the X axis matches the field being edited
		float maxDist = Mathf.Max(serializedObject.FindProperty("maxDistance").floatValue, 0.0001f);

		// Sample the real curve
		var values = new float[Samples];
		float yMax = GuardHearing.AudibleCutoff * 2f; // ensures the cutoff line is always visible
		for (int i = 0; i < Samples; i++)
		{
			float d = maxDist * i / (Samples - 1);
			values[i] = guard.EvaluateCurve(d);
			if (float.IsNaN(values[i])) { values[i] = 0f; }
			yMax = Mathf.Max(yMax, values[i]);
		}

		// Grow the Y axis to fit any live cue, so dots are always drawn at their true height
		float now = Time.realtimeSinceStartup;
		foreach (var cue in guard.RecentCues)
		{
			if (now - cue.time <= GuardHearing.CueDebugLifetime)
			{
				yMax = Mathf.Max(yMax, cue.relativeLoudness);
			}
		}
		yMax *= 1.1f; // headroom

		// Layout
		EditorGUI.DrawRect(outer, Background);
		Rect plot = new(outer.x + PadLeft, outer.y + PadTop,
			outer.width - PadLeft - PadRight, outer.height - PadTop - PadBottom);

		Vector2 ToPoint(float x01, float y)
		{
			return new Vector2(plot.x + x01 * plot.width, plot.yMax - Mathf.Clamp01(y / yMax) * plot.height);
		}

		// Grid + tick labels
		const int divisions = 4;
		for (int i = 0; i <= divisions; i++)
		{
			float t = i / (float)divisions;

			// horizontal
			float y = plot.yMax - t * plot.height;
			Handles.color = i == 0 ? AxisColor : GridColor;
			Handles.DrawLine(new Vector3(plot.x, y), new Vector3(plot.xMax, y));
			GUI.Label(new Rect(outer.x, y - 8, PadLeft - 4, 16), (t * yMax).ToString("0.0#"),
				new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight });

			// vertical
			float x = plot.x + t * plot.width;
			Handles.color = i == 0 ? AxisColor : GridColor;
			Handles.DrawLine(new Vector3(x, plot.y), new Vector3(x, plot.yMax));
			GUI.Label(new Rect(x - 20, plot.yMax + 2, 40, 16), (t * maxDist).ToString("0.#"),
				new GUIStyle(labelStyle) { alignment = TextAnchor.UpperCenter });
		}

		// Inaudible cutoff line (cues below this are ignored by HearCue)
		float cutoffY = ToPoint(0, GuardHearing.AudibleCutoff).y;
		Handles.color = CutoffColor;
		Handles.DrawDottedLine(new Vector3(plot.x, cutoffY), new Vector3(plot.xMax, cutoffY), 4f);
		GUI.Label(new Rect(plot.xMax - 110, cutoffY - 15, 110, 14), "inaudible below",
			new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight, normal = { textColor = CutoffColor } });

		// Curve
		var pts = new Vector3[Samples];
		for (int i = 0; i < Samples; i++)
		{
			Vector2 p = ToPoint(i / (float)(Samples - 1), values[i]);
			pts[i] = new Vector3(p.x, p.y);
		}
		Handles.color = CurveColor;
		Handles.DrawAAPolyLine(2.5f, pts);

		// Recent cues: a dot at the cue's real height, with a thin line down/up to the curve
		foreach (var cue in guard.RecentCues)
		{
			float age = now - cue.time;
			if (age < 0f || age > GuardHearing.CueDebugLifetime) { continue; }

			float x01 = Mathf.Clamp01(cue.distance / maxDist);
			Vector2 onCurve = ToPoint(x01, guard.EvaluateCurve(x01 * maxDist));
			Vector2 actual = ToPoint(x01, cue.relativeLoudness);

			Handles.color = CueColor;
			Handles.DrawLine(new Vector3(onCurve.x, onCurve.y), new Vector3(actual.x, actual.y));
			Handles.DrawSolidDisc(new Vector3(actual.x, actual.y), Vector3.forward, 4f);

			GUI.Label(new Rect(actual.x + 7, actual.y - 8, 60, 16), cue.relativeLoudness.ToString("0.00"),
				new GUIStyle(labelStyle) { normal = { textColor = CueColor } });
		}
	}
}