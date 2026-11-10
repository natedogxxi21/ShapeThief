using UnityEngine;
using Unity.Scripting.LifecycleManagement;
using System.Collections;
using System;

public partial class RestartTransition : MonoBehaviour
{
	[AutoStaticsCleanup]
	public static RestartTransition Instance;

	[SerializeField] RectTransform shade;
	[SerializeField] RectTransform alert;
	[SerializeField] AnimationCurve alertScaleCurve;

	void Awake() => Instance = this;

	public static void Restart(Action RestartCallback)
	{
		Time.timeScale = 0;
		Instance.StartCoroutine(RestartCoroutine());
		Instance.shade.gameObject.SetActive(true);
		Instance.alert.gameObject.SetActive(true);

		const float duration = 2f;
		IEnumerator RestartCoroutine()
		{
			float t = 0;
			bool tped = false;

			while (t < duration)
			{
				t += Time.unscaledDeltaTime;
				float p = t / duration;
				if (!tped && p > 0.5f)
				{
					RestartCallback();
					tped = true;
				}
				float y = 2 - (p < 0.5f ? Instance.Ease(p * 2) : 1 + Instance.Ease((p * 2) - 1));
				float s = Instance.alertScaleCurve.Evaluate(p);
				float r = 10 * Mathf.Sin(20 * Mathf.PI * p);
				Instance.shade.anchorMin = Instance.shade.anchorMin.WithY(-1 + y);
				Instance.shade.anchorMax = Instance.shade.anchorMax.WithY(0 + y);
				Instance.alert.localScale = Vector3.one * s;
				Instance.alert.eulerAngles = new(0, 0, r);
				yield return null;
			}

			Time.timeScale = 1;
			Instance.shade.gameObject.SetActive(false);
			Instance.alert.gameObject.SetActive(false);
		}
	}

	float Ease(float x)
	{
		return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;
	}
}