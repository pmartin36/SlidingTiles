using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gust : MonoBehaviour
{
	private Wind wind;
	private float CurveTime;

	private float SpeedModifier = 1;
	private float AmplitudeModifier;

	private AnimationCurve XCurve;
	private Vector3 StartPosition;

	private TrailRenderer tr;

	private Vector2 DirectionVector;
	private Quaternion WindDirection;

	private float tOffset;

	private int timesCompleted = 0;
	private bool active => timesCompleted < 2;
	private float completionTime => 1f + tr.time * SpeedModifier;


	void Awake() {
		wind = GetComponentInParent<Wind>();
		tr = GetComponent<TrailRenderer>();

		CurveTime = tr.time + Random.value;
	}

    void Update() {
		if(active && wind != null) {
			tr.enabled = true;

			// get latest from Wind
			float t = CurveTime * SpeedModifier;
			if(t > completionTime) {
				wind.GustCompleted();
				timesCompleted++;
				if(active) {
					ResetPosition();
					CurveTime = 0f;
				}
			}

			Vector3 movement = WindDirection * new Vector3(XCurve.Evaluate(t) * 2 * wind.Radius, wind.Y.Evaluate(t+tOffset) * AmplitudeModifier, 0);
			transform.position = StartPosition + movement;	
		}

		CurveTime += Time.deltaTime;
	}

	public void ResetPosition() {
		tr.enabled = false;
		float absX = Mathf.Abs(DirectionVector.x);
		float absY = Mathf.Abs(DirectionVector.y);
		float angle = WindDirection.eulerAngles.z + (Random.value * 60f - 30f);
		StartPosition = wind.CameraBounds.center - Utils.AngleToVector(angle) * wind.Radius;
		transform.position = StartPosition;
		tr.Clear();
	}

	public void SetProperties(int i) {
		if(wind != null) {
			DirectionVector = Utils.AngleToVector(wind.Direction);
			WindDirection = Quaternion.Euler(0,0,wind.Direction);
			SpeedModifier = wind.Strength * 1.25f;
			AmplitudeModifier = (20f + 10f * Random.value);// * Mathf.Sign(Random.value - 0.5f);
			tOffset = Random.value;

			ResetPosition();

			XCurve = wind.X;
			//XCurve = new AnimationCurve(Wind.X.keys);
			//Keyframe f1 = XCurve[1];
			//f1.time = 0.2f + 0.4f * Random.value;

			//Keyframe f2 = XCurve[2];
			//f2.time = f1.time + (0.15f + 0.1f * Random.value);

			//XCurve.MoveKey(2, f2);
			//XCurve.MoveKey(1, f1);

			CurveTime = -i * completionTime;
			timesCompleted = 0;

			//Debug.Log(CurveTime);
			//Debug.Log(d);
			//Debug.Log(SpeedModifier);
			//Debug.Log(AmplitudeModifier);
			//Debug.Log(StartPosition);
		}
	}
}
