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
	private static Material MovableMaterial;
	private static Material ImmovableMaterial;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		SerializedProperty prop = property.FindPropertyRelative("Value");
		EditorGUI.PropertyField(position, prop, new GUIContent("Movable"));

		if (GUI.changed) {
			MonoBehaviour g = property.serializedObject.targetObject as MonoBehaviour;
			SpriteRenderer spriteRenderer = g.GetComponent<SpriteRenderer>();

			int world = Int32.Parse(SceneManager.GetActiveScene().name.Split('-')[0]);

			// Addressables don't work, fuuuuuuuu
			if (!prop.boolValue && !spriteRenderer.sharedMaterial.name.Contains("Immobile")) {
				string name = $"World{world}/Level_ImmobileTile";
				Addressables.LoadAssetAsync<Material>(name).Completed += (obj) => 
					spriteRenderer.sharedMaterial = obj.Result;
			}
			else if(prop.boolValue && spriteRenderer.sharedMaterial.name.Contains("Immobile")) {
				string name = $"World{world}/Level_NormalTile";
				Addressables.LoadAssetAsync<Material>(name).Completed += (obj) => 
					spriteRenderer.sharedMaterial = obj.Result;
			}
		}
		EditorGUI.EndProperty();
	}
}
