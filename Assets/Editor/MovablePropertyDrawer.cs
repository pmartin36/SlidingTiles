using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[CustomPropertyDrawer(typeof(Movable))]
public class MovablePropertyDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		SerializedProperty prop = property.FindPropertyRelative("Value");
		EditorGUI.PropertyField(position, prop, new GUIContent("Movable"));

		if (GUI.changed) {
			var g = property.serializedObject.targetObject as MonoBehaviour;

			SpriteRenderer spriteRenderer = g.GetComponent<SpriteRenderer>();

			var m = new Material(spriteRenderer.sharedMaterial);
			m.SetFloat("_Mobile", prop.boolValue ? 1 : 0);
			spriteRenderer.sharedMaterial = m;
			
			//spriteRenderer.material.SetFloat("_Mobile", prop.boolValue ? 1 : 0);

			//int world = Int32.Parse(SceneManager.GetActiveScene().name.Split('-')[0]);
		}
		EditorGUI.EndProperty();
	}
}
