using UnityEngine;
using UnityEditor;
using ITISKIRUHERE;

public class ShiftProp : MonoBehaviour
{
	[SerializeField] Mesh mesh;
	public Mesh Mesh { get => mesh; }
	[SerializeField] Material material;
	public Material Material { get => material; }
	public Vector3 scale;

	[SerializeField] GameObject highlight;

	public void Highlight(bool on)
	{
		if (highlight != null) { highlight.SetActive(on); }
	}

#if UNITY_EDITOR
	public void SetValues()
	{
		Transform model = transform.GetChild(0);
		model.GetComponent<MeshFilter>().mesh = mesh;
		scale = model.localScale;
		highlight = transform.GetChild(1).gameObject;
		highlight.transform.localScale = scale;
		highlight.GetComponent<MeshFilter>().mesh = mesh;
	}

	public void TryFixHighlight()
	{
		if (highlight == null) return;

		if (!highlight.TryGetComponent<MeshFilter>(out var meshFilter)) return;

		Undo.RecordObject(meshFilter, "Fix Highlight Mesh");
		meshFilter.mesh = Mesh;
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
	}
#endif
}