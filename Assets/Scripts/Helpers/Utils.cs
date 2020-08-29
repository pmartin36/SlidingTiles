using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Utils {
	public static Vector2 RandomVectorInBox(Bounds b, float padding = 1) {
		return new Vector2(
			UnityEngine.Random.Range(b.min.x+padding, b.max.x-padding),
			UnityEngine.Random.Range(b.min.y+padding, b.max.y-padding)
		);
	}

	public static Vector3 AngleToVector(float angle) {
		return new Vector2(
			Mathf.Cos(Mathf.Deg2Rad * angle), 
			Mathf.Sin(Mathf.Deg2Rad * angle)
		).normalized;
	}

	public static float xyToAngle(float x, float y) {
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

	public static float VectorToAngle(Vector2 vector) {
		return xyToAngle(vector.x, vector.y);
	}

	public static Vector2 QuadraticBezier(Vector2 start, Vector2 end, Vector2 ctrl, float t) {
		return (1-t)*(1-t)*start + 2*t*(1-t)*ctrl + t*t*end;
	}

	public static float NegativeMod(float a, float b) {
		return a - b * Mathf.Floor(Mathf.Abs(a / b));
	}

	public static float AngleDiff(float a, float b) {
		return 180 - Mathf.Abs(Mathf.Abs(a - b) - 180);
	}

	public static float Cosh(float d) {
		return (Mathf.Exp(d) + Mathf.Exp(-d))/2f;
	}

	public static float Sinh(float d) {
		return (Mathf.Exp(d) - Mathf.Exp(-d)) / 2f;
	}

	public static IEnumerator SimpleLerp(float timeToComplete, System.Action<float> loopAction) {
		float t = 0;
		while (t < timeToComplete) {
			loopAction(t / timeToComplete);
			t += Time.deltaTime;
			yield return null;
		}
		loopAction(t / timeToComplete);
	}

	public static string SplitTime(float inputTime, MillisecondDisplay msDisplay) {
		int minutes = Mathf.FloorToInt(inputTime / 60f);
		int intSeconds = Mathf.FloorToInt(inputTime);
		int seconds = intSeconds % 60;
		float ms = inputTime - intSeconds;
		string time = $"{minutes:0}:{seconds:00}";
		switch (msDisplay) {
			case MillisecondDisplay.Sub:
				time += $"<sub>{ms:.000}</sub>";
				break;
			case MillisecondDisplay.Normal:
				time += $"{ms:.000}";
				break;
			case MillisecondDisplay.None:
			default:
				break;
		}
		return time;
	}
}


public struct Vector3Int {
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public Vector3Int(int x, int y) : this() {
		this.x = x;
		this.y = y;
	}

	public Vector3Int(int x, int y, int z) : this() {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public override string ToString() {
		return $"({x}, {y}, {z})";
	}
}

public struct TimeInfo {
	public float Time { get; set; }
	public bool Record { get; set; }

	public TimeInfo(bool record, float time) : this() {
		Record = record;
		Time = time;
	}
}

public enum MillisecondDisplay {
	None,
	Sub,
	Normal
}