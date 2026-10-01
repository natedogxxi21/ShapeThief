using UnityEngine;
using ITISKIRUHERE;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
		Transform highlightTransform = transform.GetChild(1);
		MeshFilter highlightFilter = highlightTransform.GetComponent<MeshFilter>();

		// Record everything BEFORE changing it (also marks scene objects dirty + enables Ctrl+Z)
		Undo.RecordObject(this, "Set Prop Values");
		Undo.RecordObject(prop, "Set Prop Values");
		Undo.RecordObject(highlightTransform, "Set Prop Values");
		Undo.RecordObject(highlightFilter, "Set Prop Values");

		prop.mesh = model.GetComponent<MeshFilter>().sharedMesh;
		prop.scale = model.localScale;

		highlight = highlightTransform.gameObject;
		highlightTransform.localScale = prop.scale;
		highlightFilter.sharedMesh = prop.mesh; // sharedMesh avoids instancing the mesh in edit mode

		// Make sure the ScriptableObject asset is actually written to disk
		EditorUtility.SetDirty(prop);
		AssetDatabase.SaveAssets();
	}

	public void ApplyValues()
	{
		Transform model = transform.GetChild(0);
		Transform highlightTransform = transform.GetChild(1);
		MeshFilter modelFilter = model.GetComponent<MeshFilter>();
		MeshCollider modelCollider = model.GetComponent<MeshCollider>();

		Undo.RecordObject(this, "Apply Prop Values");
		Undo.RecordObject(modelFilter, "Apply Prop Values");
		Undo.RecordObject(modelCollider, "Apply Prop Values");
		Undo.RecordObject(model, "Apply Prop Values");
		Undo.RecordObject(highlightTransform, "Apply Prop Values");

		modelFilter.sharedMesh = prop.mesh;
		modelCollider.sharedMesh = prop.mesh;
		model.localScale = prop.scale;

		highlight = highlightTransform.gameObject;
		highlightTransform.localScale = prop.scale;
	}

	public void TryFixHighlight()
	{
		if (highlight == null) return;

		if (!highlight.TryGetComponent<MeshFilter>(out var meshFilter)) return;

		Undo.RecordObject(meshFilter, "Fix Highlight Mesh");
		meshFilter.sharedMesh = prop.mesh;

		if (highlight.TryGetComponent<AdvancedOutline>(out var outline))
		{
			outline.RefreshRenderers();
		}
	}
#endif
}

#if UNITY_EDITOR
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
			foreach (Object obj in targets)
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
				GameObject hl = ((ShiftProp)obj).transform.GetChild(1).gameObject;
				Undo.RecordObject(hl, "Toggle Highlight");
				hl.SetActive(newActive);
			}
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Set the properties of the child model for \"Set Values\" to work");
	}
}
#endif