using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GuardBehaviour))]
public class GuardHUD : MonoBehaviour
{
	static readonly Vector3[] corners = new Vector3[4];
	public static readonly List<GuardHUD> All = new();
	public RectTransform Indicator => indicator;
	public float PlayerDistance { get; private set; }

	GuardBehaviour behaviour;
	[SerializeField] GuardVision vision;
	[SerializeField] GuardHearing hearing;
	float TotalSuspicion => vision.Suspicion + hearing.Suspicion;
	[Header("Indicator")]
	[SerializeField] Transform target;
	[SerializeField] GameObject indicatorPrefab;
	[SerializeField] float screenEdgePadding;
	RectTransform indicator;
	Transform ChaseIcon => indicator.GetChild(0);
	Transform SuspicionBar => indicator.GetChild(1);
	RectTransform MiddleAlign => (RectTransform)SuspicionBar.GetChild(0);
	RectTransform VisionBar => (RectTransform)MiddleAlign.GetChild(0);
	RectTransform HearingBar => (RectTransform)MiddleAlign.GetChild(1);

	// Distance scale decay
	const float curve = 20f;
	const float decayStart = 10f;

	Camera Cam => Camera.main;
	Canvas Canvas => HUD.Instance.GetComponent<Canvas>();

	void Start()
	{
		behaviour = GetComponent<GuardBehaviour>();
		indicator = Instantiate(indicatorPrefab, HUD.Instance.indicators).GetComponent<RectTransform>();
	}

	public void Update()
	{
		UpdateIndicator();
		if (ChaseIcon.gameObject.activeSelf || SuspicionBar.gameObject.activeSelf)
		{ PositionIndicator(); }
	}

	void UpdateIndicator()
	{
		ChaseIcon.gameObject.SetActive(behaviour.State == GuardState.Chase);
		SuspicionBar.gameObject.SetActive(behaviour.State != GuardState.Chase && TotalSuspicion > 0.1f);

		PlayerDistance = Vector3.Distance(target.position, PlayerTouchMovement.Instance.transform.position);
		float distanceFactor = Mathf.Min(1f, curve / ((PlayerDistance - decayStart) + curve));
		indicator.localScale = Vector3.one * Mathf.Pow(distanceFactor, 2);

		if (behaviour.State == GuardState.Chase)
		{
			Image icon = ChaseIcon.GetComponent<Image>();
			float p = behaviour.chaseInvestigateTimer.Remaining / behaviour.chaseInvestigateTimer.Duration;
			icon.fillAmount = p;
			icon.color = Color.Lerp(Color.white, Color.yellow, p);
			icon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, p);
		}
		else
		{
			SuspicionBar.GetComponent<Image>().color = Color.Lerp(new(.3f, .3f, .3f, .7f), Color.orangeRed, TotalSuspicion);
			SuspicionBar.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, TotalSuspicion);
			float halfTotal = TotalSuspicion / 2f;
			MiddleAlign.anchorMin = MiddleAlign.anchorMin.WithX(.5f - halfTotal);
			MiddleAlign.anchorMax = MiddleAlign.anchorMax.WithX(.5f + halfTotal);
			float visionP = TotalSuspicion > 0f
				 ? vision.Suspicion / TotalSuspicion
				 : 0.5f;
			visionP = Mathf.Clamp01(visionP);
			VisionBar.anchorMax = VisionBar.anchorMax.WithX(visionP);
			HearingBar.anchorMin = HearingBar.anchorMin.WithX(visionP);
		}
	}

	void PositionIndicator()
	{
		Camera uiCam = Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera;

		// target in screen space
		Vector3 targetScreenPos = Cam.WorldToScreenPoint(target.position);
		bool behind = targetScreenPos.z < 0;
		if (behind)
		{
			// projection is mirrored behind the camera
			targetScreenPos.x = Screen.width - targetScreenPos.x;
			targetScreenPos.y = Screen.height - targetScreenPos.y;
		}
		Vector2 targetScreen = targetScreenPos;

		// measure size of indicator
		Vector2 lo = new(float.PositiveInfinity, float.PositiveInfinity);
		Vector2 hi = new(float.NegativeInfinity, float.NegativeInfinity);

		foreach (Graphic g in indicator.GetComponentsInChildren<Graphic>(false))
		{
			if (!g.enabled) continue;
			g.rectTransform.GetWorldCorners(corners);
			for (int i = 0; i < 4; i++)
			{
				Vector2 s = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
				lo = Vector2.Min(lo, s);
				hi = Vector2.Max(hi, s);
			}
		}

		if (float.IsInfinity(lo.x)) return;
		Vector2 pivotScreen = RectTransformUtility.WorldToScreenPoint(uiCam, indicator.position);
		Vector2 half = (hi - lo) * 0.5f;
		Vector2 centerOffset = (lo + hi) * 0.5f - pivotScreen;

		// allowed range for the PIVOT so the whole element stays inside the padded safe area
		Rect safe = Screen.safeArea;
		Vector2 pad = Vector2.one * screenEdgePadding;
		Vector2 min = safe.min + pad + half - centerOffset;
		Vector2 max = safe.max - pad - half - centerOffset;

		Vector2 finalScreen;
		if (!behind)
		{
			// nearest point inside the allowed rect: tracks the target on screen,
			// slides along the border when the target is off screen
			finalScreen = new Vector2(
				Mathf.Clamp(targetScreen.x, min.x, max.x),
				Mathf.Clamp(targetScreen.y, min.y, max.y));
		}
		else
		{
			// behind the camera the mirrored point can land mid-screen, so force it to the edge
			Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
			Vector2 dir = targetScreen - center;
			if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;

			float sx = dir.x > 0 ? (max.x - center.x) / dir.x
					 : dir.x < 0 ? (min.x - center.x) / dir.x
					 : float.PositiveInfinity;
			float sy = dir.y > 0 ? (max.y - center.y) / dir.y
					 : dir.y < 0 ? (min.y - center.y) / dir.y
					 : float.PositiveInfinity;

			finalScreen = center + dir * Mathf.Min(sx, sy);
		}

		// screen to world position on the indicator's parent
		RectTransformUtility.ScreenPointToWorldPointInRectangle(
			(RectTransform)indicator.parent, finalScreen, uiCam, out Vector3 world);
		indicator.position = world;
	}

	void OnEnable() => All.Add(this);
	
	void OnDisable() => All.Remove(this);

	void OnDestroy()
	{
		if (indicator != null) Destroy(indicator.gameObject);
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