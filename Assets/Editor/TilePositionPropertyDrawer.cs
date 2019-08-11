using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TilePosition))]
public class TilePositionPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		EditorGUI.BeginProperty(position, label, property);

		// Draw label
		position = EditorGUI.PrefixLabel(position, label);

		// Don't make child fields be indented
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Calculate rects
		var xrect = new Rect(position.x, position.y, 75, position.height);
		var yrect = new Rect(position.x + 90, position.y, 75, position.height);

		EditorGUIUtility.labelWidth = 14f;

		var xprop = property.FindPropertyRelative("x");
		var yprop = property.FindPropertyRelative("y");

		// Draw fields - passs GUIContent.none to each so they are drawn without labels
		EditorGUI.PropertyField(xrect, property.FindPropertyRelative("x"), new GUIContent("X"));
		EditorGUI.PropertyField(yrect, property.FindPropertyRelative("y"), new GUIContent("Y"));

		// Set indent back to what it was
		EditorGUI.indentLevel = indent;

		if(GUI.changed) {
			Tilespace t = property.serializedObject.targetObject as Tilespace;
			float scale = t.transform.localScale.x;
			t.transform.localPosition = new Vector2(xprop.intValue * scale, yprop.intValue * scale);
		}

		EditorGUI.EndProperty();
	}
}
