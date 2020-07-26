using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadScreen : MonoBehaviour
{
	private Animator animator;
	private bool Showing;

	private List<RectTransform> tileRTs;
	private GameObject panel;

	private List<Vector2> startPositions;
	private List<Vector2> midPositions;
	private List<Vector2> endPositions;

	[Range(0,3.5f)]
	public float PositionVal = 0;

	public void OnEnable() {
		
	}

	public void Awake() {
		animator = GetComponent<Animator>();

		Transform panelTransform = transform.GetChild(0);
		Camera c = CameraManager.Instance.Camera;

		Canvas canvas = gameObject.GetComponentInParent<Canvas>();
		// Debug.Log(canvas.pixelRect);

		tileRTs = new List<RectTransform>();
		for(int i = 0; i < panelTransform.childCount; i++) {
			tileRTs.Add(panelTransform.GetChild(i).GetComponent<RectTransform>());
		}

		tileRTs[0].gameObject.GetComponentInChildren<TMPro.TMP_Text>().fontSharedMaterial.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Disabled);

		tileRTs = tileRTs.OrderBy(t => t.anchoredPosition.x).ToList();
		int count = tileRTs.Count;
		startPositions = new List<Vector2>(count);
		midPositions = new List<Vector2>(count);
		endPositions = new List<Vector2>(count);
		foreach(RectTransform rt in tileRTs) {
			Vector2 start = rt.anchoredPosition + Vector2.right * 1000;
			startPositions.Add(start);
			midPositions.Add(rt.anchoredPosition);
			endPositions.Add(rt.anchoredPosition - Vector2.right * 1000);

			rt.anchoredPosition = start;
		}

		panel = panelTransform.gameObject;
	}

	public void Show(bool show) {	
		Showing = show;
		if(show) {
			panel.SetActive(true);
			animator.SetBool("Hide", false);
			animator.Play("Show", 0, 0);
		}
		else {
			animator.SetBool("Hide", true);
		}
	}

	public void Update() {
		var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
		if (clipInfo.Length > 0 && clipInfo[0].clip.name == "Highlight") {
			for(int i = 0; i < tileRTs.Count; i++) {
				RectTransform rt = tileRTs[i];
				float modifiedPosition = PositionVal - i * 0.1f;
				if(modifiedPosition <= 1) {
					rt.anchoredPosition = Vector2.Lerp(startPositions[i], midPositions[i], Mathf.SmoothStep(0, 1, modifiedPosition));
				}
				else {
					rt.anchoredPosition = Vector2.Lerp(midPositions[i], endPositions[i], Mathf.SmoothStep(0, 1, modifiedPosition - 2f));
				}
			}
		}
	}
}
