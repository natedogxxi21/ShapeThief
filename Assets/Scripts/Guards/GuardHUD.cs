using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GuardBehaviour))]
public class GuardHUD : MonoBehaviour
{
	GuardBehaviour behaviour;
	[SerializeField] GuardVision vision;
	[SerializeField] GuardHearing hearing;
	float TotalSuspicion => vision.Suspicion + hearing.Suspicion;
	[Space]
	[SerializeField] GameObject chaseIcon;
	[SerializeField] Image suspicionBar;
	[SerializeField] RectTransform middleAlign;
	[SerializeField] RectTransform visionBar;
	[SerializeField] RectTransform hearingBar;

	void Start() => behaviour = GetComponent<GuardBehaviour>();

	void Update()
	{
		chaseIcon.SetActive(behaviour.State == GuardState.Chase);
		suspicionBar.gameObject.SetActive(behaviour.State != GuardState.Chase && TotalSuspicion > 0.1f);

		if (behaviour.State == GuardState.Chase)
		{
			Image icon = chaseIcon.GetComponent<Image>();
			float p = behaviour.chaseInvestigateTimer.Remaining / behaviour.chaseInvestigateTimer.Duration;
			icon.fillAmount = p;
			icon.color = Color.Lerp(Color.white, Color.yellow, p);
			icon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, p);
		}
		else
		{
			suspicionBar.color = Color.Lerp(new(.3f, .3f, .3f, .7f), Color.orangeRed, TotalSuspicion);
			suspicionBar.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, TotalSuspicion);
			float halfTotal = TotalSuspicion / 2f;
			middleAlign.anchorMin = middleAlign.anchorMin.WithX(.5f - halfTotal);
			middleAlign.anchorMax = middleAlign.anchorMax.WithX(.5f + halfTotal);
			float visionP = TotalSuspicion > 0f
				 ? vision.Suspicion / TotalSuspicion
				 : 0.5f;
			visionP = Mathf.Clamp01(visionP);
			visionBar.anchorMax = visionBar.anchorMax.WithX(visionP);
			hearingBar.anchorMin = hearingBar.anchorMin.WithX(visionP);
		}
	}
#if UNITY_EDITOR
	public GuardVision Vision => vision;
	public GuardHearing Hearing => hearing;
	public float TotalSus => TotalSuspicion;
}

[CustomEditor(typeof(GuardHUD))]
public class GuardHUDEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		EditorGUILayout.Space();
		if (!Application.isPlaying)
		{
			EditorGUILayout.LabelField("Vision Suspicion: Not playing");
			EditorGUILayout.LabelField("Hearing Suspicion: Not playing");
			EditorGUILayout.LabelField("Total Suspicion: Not playing");
		}
		else
		{
			GuardHUD hud = (GuardHUD)target;
			EditorGUILayout.LabelField($"Vision Suspicion: {hud.Vision.Suspicion:0.0}");
			EditorGUILayout.LabelField($"Hearing Suspicion: {hud.Hearing.Suspicion:0.0}");
			EditorGUILayout.LabelField($"Total Suspicion: {hud.TotalSus:0.00}");
		}
	}
#endif
}