using UnityEngine;
using UnityEditor;
using ITISKIRUHERE;

public class ShiftProp : MonoBehaviour
{
	public Prop prop;

	[SerializeField] GameObject highlight;

	public void Highlight(bool on)
	{
		if (highlight != null) { highlight.SetActive(on); }
	}

#if UNITY_EDITOR
	public void SetValues()
	{
		Transform model = transform.GetChild(0);
		prop.mesh = model.GetComponent<MeshFilter>().sharedMesh;
		prop.scale = model.localScale;
		highlight = transform.GetChild(1).gameObject;
		highlight.transform.localScale = prop.scale;
		highlight.GetComponent<MeshFilter>().mesh = prop.mesh;
	}

	public void ApplyValues()
	{
		Transform model = transform.GetChild(0);
		model.GetComponent<MeshFilter>().sharedMesh = prop.mesh;
		model.GetComponent<MeshCollider>().sharedMesh = prop.mesh;
		model.localScale = prop.scale;
		highlight = transform.GetChild(1).gameObject;
		highlight.transform.localScale = prop.scale;
	}

	public void TryFixHighlight()
	{
		if (highlight == null) return;

		if (!highlight.TryGetComponent<MeshFilter>(out var meshFilter)) return;

		Undo.RecordObject(meshFilter, "Fix Highlight Mesh");
		meshFilter.mesh = prop.mesh;
		highlight.GetComponent<AdvancedOutline>().RefreshRenderers();
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(ShiftProp))]
public class ShiftPropEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Set Values"))
		{
			foreach (Object obj in targets)
			{
				ShiftProp prop = (ShiftProp)obj;
				prop.SetValues();
			}
		}

		if (GUILayout.Button("Apply Values"))
		{
			foreach(Object obj in targets)
			{
				ShiftProp prop = (ShiftProp)obj;
				prop.ApplyValues();
			}
		}

		if (GUILayout.Button("Try Fix Highlight"))
		{
			foreach (Object obj in targets)
			{
				ShiftProp prop = (ShiftProp)obj;
				prop.TryFixHighlight();
			}
		}

		if (GUILayout.Button("Toggle Highlight"))
		{
			bool newActive = !((ShiftProp)targets[0]).transform.GetChild(1).gameObject.activeSelf;
			foreach (Object obj in targets)
			{
				((ShiftProp)obj).transform.GetChild(1).gameObject.SetActive(newActive);
			}
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Set the properties of the child model for \"Set Values\" to work");
	}
#endif
}