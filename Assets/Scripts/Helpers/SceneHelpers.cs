using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class SceneHelpers {
	public static readonly int SplashBuildIndex = 0;
	public static readonly int MenuBuildIndex = 1;
	public static readonly int LoadSceneBuildIndex = 2;
	public static readonly int WorldCompleteBuildIndex = 3;
	public static readonly int TutorialLevelStart = 4;

	public static int SceneCount => SceneManager.sceneCountInBuildSettings;

	public static int GetNextLevelBuildIndex() {
		return GetCurrentLevelBuildIndex() + 1;
	}

	public static int GetCurrentLevelBuildIndex() {
		var buildIndex = 0;
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt(i);
			if (s.isLoaded && s.buildIndex != LoadSceneBuildIndex) {
				return s.buildIndex;
			}
		}
		return buildIndex;
	}

	public static string GetSceneName() {
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			Scene s = SceneManager.GetSceneAt(i);
			if (s.buildIndex != LoadSceneBuildIndex) {
				return s.name;
			}
		}
		return "";
	}

	public static int GetBuildIndexFromLevel(int world, int level) {
		return
			TutorialLevelStart +
			2 + // two tutorial levels
			(world - 1) * 10 + // each world has 10 levels
			(level - 1); // 0 indexed levels
	}

	public static int GetWorldFromBuildIndex(int index) => (index - TutorialLevelStart - 2) / 10 + 1;
	public static int GetLevelFromBuildIndex(int index) => (index - TutorialLevelStart - 2) % 10 + 1;
	public static void GetWorldAndLevelFromBuildIndex(int index, out int world, out int level) {
		world = GetWorldFromBuildIndex(index);
		level = GetLevelFromBuildIndex(index);
	}
	public static ValueTuple<int, int> GetWorldAndLevelFromBuildIndex(int index) {
		return (GetWorldFromBuildIndex(index), GetLevelFromBuildIndex(index));
	}
}

