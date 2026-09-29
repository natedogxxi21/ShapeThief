using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(Timer))]
public class TimerDrawer : PropertyDrawer
{
	static readonly float Line = EditorGUIUtility.singleLineHeight;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		 => Line;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float endTime = property.FindPropertyRelative("<EndTime>k__BackingField").floatValue;
		float duration = property.FindPropertyRelative("<Duration>k__BackingField").floatValue;
		bool paused = property.FindPropertyRelative("<Paused>k__BackingField").boolValue;
		float pausedTime = property.FindPropertyRelative("pausedTime").floatValue;

		EditorGUI.BeginProperty(position, label, property);
		Rect barRect = EditorGUI.PrefixLabel(position, label);

		string text;
		float progress = 0f;

		if (!Application.isPlaying)
		{
			text = "Not running";
		}
		else
		{
			float evalTime = paused ? pausedTime : Time.time;
			float remaining = endTime - evalTime;
			progress = duration > 0f ? Mathf.Clamp01(remaining / duration) : 1f;

			text = $"{Mathf.Max(0f, remaining):F2}s / {duration:F2}s";
			if (paused) text = "Paused: " + text;
			else if (remaining <= 0f) text = "Done";
		}

		EditorGUI.ProgressBar(barRect, progress, text);
		EditorGUI.EndProperty();
	}
}

// Keeps the inspector repainting while the game runs so the bar animates.
[InitializeOnLoad]
static class TimerInspectorRepaint
{
	static TimerInspectorRepaint()
	{
		EditorApplication.update += () =>
		{
			if (Application.isPlaying) InternalEditorUtility.RepaintAllViews();
		};
	}
}