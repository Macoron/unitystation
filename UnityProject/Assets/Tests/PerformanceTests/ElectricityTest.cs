using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
	public static class EditorRoutine
	{
		private class EditorRoutineBehaviour : MonoBehaviour { }

		private static EditorRoutineBehaviour instance;
		private static EditorRoutineBehaviour Instance
		{
			get
			{
				if (!instance)
				{
					var routineHost = new GameObject();
					instance = routineHost.AddComponent<EditorRoutineBehaviour>();
				}

				return instance;

			}
		}

		public static bool isComplete = true;

		public static IEnumerator Execute(IEnumerator basicRoutine)
		{
			if (!isComplete)
				throw new Exception("Editor already executing another routine!");
			isComplete = false;

			Instance.StartCoroutine(RoutineWrapper(basicRoutine));

			while (!isComplete)
			{
				if (!Application.isPlaying)
					break;

				yield return null;
			}

		}

		private static IEnumerator RoutineWrapper(IEnumerator basicRoutine)
		{
			yield return basicRoutine;
			isComplete = true;
		}

	}

    class ElectricityTest : PlayModePerformanceTest
	{
		protected override string Scene => "OutpostStation";

		protected override SampleGroupDefinition[] SampleGroupDefinitions => sampleGroupDefinitions;

		readonly SampleGroupDefinition[] sampleGroupDefinitions =
			new[] {
				new SampleGroupDefinition(ElectricalSynchronisation.updateName) }.Concat(
			ElectricalSynchronisation.markerNames.Select(mn => new SampleGroupDefinition(mn))).ToArray();

		PlayerSync player;

		/*[UnitySetUp]
		private IEnumerator Setup()
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene != SceneManager.GetActiveScene())
					yield return SceneManager.UnloadSceneAsync(scene);
			}
		}*/

		[UnityTest, Performance]
		public IEnumerator NanotrasenAssistantTest()
		{
			var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/OutpostStation.unity", OpenSceneMode.Single);

			yield return new EnterPlayMode();

			yield return EditorRoutine.Execute(SkipRoundWaiting());

			yield return EditorRoutine.Execute(ClickButton(JobType.ASSISTANT.ToString()));

			yield return EditorRoutine.Execute(Settle());

			yield return new ExitPlayMode();
		}

		[UnityTest, Performance]
        public IEnumerator ElectricityGeneratorTest()
		{
			//var testScene = SceneManager.CreateScene("TestRunner");
			//SceneManager.SetActiveScene(testScene);

			var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/OutpostStation.unity", OpenSceneMode.Single);

			yield return new EnterPlayMode();

			yield return EditorRoutine.Execute(SkipRoundWaiting());

			yield return EditorRoutine.Execute(ClickButton("CHIEF_ENGINEER"));

			yield return EditorRoutine.Execute(Settle());

			yield return new ExitPlayMode();

			/*yield return new ConditionalRoutine(SkipRoundWaiting()).MainRoutine();
			yield return ClickButton("CHIEF_ENGINEER");

			yield return Settle();

			player = PlayerManager.LocalPlayer.GetComponent<PlayerSync>();
			yield return Move(10, MoveAction.MoveUp);
			yield return Settle();
			yield return Move(3, MoveAction.MoveUp);

			yield return Settle();
			yield return UpdateBenchmark(300);

			yield return new ExitPlayMode();

			/*GUI_IngameMenu.Instance.isTest = true;
			GUI_IngameMenu.Instance.OpenMenuPanel();
			yield return ClickButton("ExitButton");
			yield return DoActionWaitSceneUnload(ClickButton("Button1"));*/

			//yield return new WaitForSeconds(20);

			/*var testScene = SceneManager.GetSceneAt(0);
			SceneManager.SetActiveScene(testScene);

			//yield return new WaitForSeconds(5);

			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene != testScene)
					yield return SceneManager.UnloadSceneAsync(scene);
			}

			var go = new GameObject("Sacrificial Lamb");
			GameObject.DontDestroyOnLoad(go);

			foreach (var root in go.scene.GetRootGameObjects())
				GameObject.Destroy(root);*/

			//yield return new WaitForSeconds(20);
		}

		protected override IEnumerator CustomUpdateBenchmark(int sampleCount)
		{
			int disableGeneratorPoint = sampleCount / 3;
			int enableGeneratorPoint = sampleCount * 2 / 3;
			yield return new WaitWhile(LoopFunction);

			bool LoopFunction()
			{
				sampleCount--;
				if(sampleCount == disableGeneratorPoint || sampleCount == enableGeneratorPoint)
				{
					var generator = GetAtRelative<PowerGenerator>(Orientation.Right);
					InteractionUtils.RequestInteract(HandApply.ByLocalPlayer(generator.gameObject), generator);
				}
				return sampleCount > 0;
			}
		}

		T GetAtRelative<T>(Orientation orientation) where T : MonoBehaviour
		{
			return MatrixManager.GetAt<T>(player.ClientPosition + Vector3Int.FloorToInt(orientation.Vector), true).First();
		}

		IEnumerator Move(int repeat, params MoveAction[] moves)
		{
			for (int i = 0; i < repeat; i++)
			{
				yield return new WaitUntil(() => TryMove(moves));
			}
		}

		IEnumerator Move(params MoveAction[] moves)
		{
			return new WaitUntil(() => TryMove(moves));
		}

		bool TryMove(params MoveAction[] moves)
		{
			int[] intMoves = Array.ConvertAll(moves, value => (int)value);
			return player.DoAction(new PlayerAction() { moveActions = intMoves});
		}
    }
}
