using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RotationInfo", menuName = "ScriptableObjects/RotationInfo", order = 1)]
public class RotationInfo : ScriptableObject {
	public AnimationCurve leftPoint_x;
	public AnimationCurve rightPoint_x;
	public AnimationCurve Y;
}

