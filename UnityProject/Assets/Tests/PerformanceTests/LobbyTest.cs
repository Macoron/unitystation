using System;
using System.Collections;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace Tests
{
	class LobbyTest : PlayModePerformanceTest
	{
		const string LoadTimeName = "LoadTime";
		const string OutpostTimeName = "OutpostTime";

		protected override string Scene => "Lobby";
		protected override SampleGroupDefinition[] SampleGroupDefinitions => null;

		#region Custom SampleGroupDefinitions
		readonly SampleGroupDefinition loadTimeSg = new SampleGroupDefinition(LoadTimeName, SampleUnit.Second);
		readonly SampleGroupDefinition outpostTimeSg = new SampleGroupDefinition(OutpostTimeName, SampleUnit.Second);
		readonly SampleGroupDefinition fpsSg = new SampleGroupDefinition(FpsName, SampleUnit.None, AggregationType.Min, increaseIsBetter: true);
		readonly SampleGroupDefinition totalMemorySg = new SampleGroupDefinition(TotalMemoryName, SampleUnit.Megabyte);
		#endregion

		#region Times
		DateTime? startTime;
		DateTime? loadTime;
		DateTime? outpostTime;

		void StartTime() => startTime = DateTime.Now;
		void LoadTime() => loadTime = DateTime.Now;
		void OutpostTime() => outpostTime = DateTime.Now;
		#endregion

		#region Tests
		[UnityTest, Performance]
		public IEnumerator NanotrasenAssistant()
		{
			yield return null;

			/*var mainScene = EditorSceneManager.OpenScene("Assets/Scenes/OutpostStation.unity", OpenSceneMode.Single);

			yield return new EnterPlayMode();

			yield return EditorRoutine.Execute(SkipRoundWaiting());

			yield return EditorRoutine.Execute(ClickButton(JobType.ASSISTANT.ToString()));

			yield return EditorRoutine.Execute(Settle());

			yield return new ExitPlayMode();*/

			/*yield return new WaitForSeconds(20);

			yield return new EnterPlayMode();



			StartTime();
			yield return LoadSceneAndSetActive();
			LoadTime();

			yield return SkipLogin();

			yield return DoActionWaitSceneLoad(ClickButton("StartGameButton"));
			yield return SkipRoundWaiting();
			yield return ClickButton("ASSISTANT");

			OutpostTime();

			yield return Settle();

			yield return UpdateBenchmark();

			GUI_IngameMenu.Instance.isTest = true;
			GUI_IngameMenu.Instance.OpenMenuPanel();
			yield return ClickButton("ExitButton");
			yield return DoActionWaitSceneUnload(ClickButton("Button1"));

			EndBenchmark();

			yield return new ExitPlayMode();*/
		}

		// This test is legacy now
		/*[UnityTest, Performance]
		public IEnumerator NukeOps()
		{
			StartTime();
			yield return LoadSceneAndSetActive();
			LoadTime();
			yield return ClickButton("LoginButton");
			yield return DoActionWaitSceneLoad(ClickButton("StartGameButton"));
			yield return ClickButton("NukeOps");
			OutpostTime();

			yield return Settle();

			yield return UpdateBenchmark();

			GUI_IngameMenu.Instance.isTest = true;
			GUI_IngameMenu.Instance.OpenMenuPanel();
			yield return ClickButton("ExitButton");
			yield return DoActionWaitSceneUnload(ClickButton("Button1"));

			EndBenchmark();
		}*/
		#endregion

		protected override IEnumerator CustomUpdateBenchmark(int sampleCount)
		{
			yield return new WaitWhile(LoopFunction);

			bool LoopFunction()
			{
				Measure.Custom(fpsSg, 1 / Time.unscaledDeltaTime);
				Measure.Custom(totalMemorySg, Profiler.GetTotalAllocatedMemoryLong() / Math.Pow(1024, 2));
				sampleCount--;
				return sampleCount > 0;
			}
		}

		void EndBenchmark()
		{
			var loadTime = (DateTime)this.loadTime;
			var startTime = (DateTime)this.startTime;
			var outpostTime = (DateTime)this.outpostTime;

			Measure.Custom(loadTimeSg, (loadTime - startTime).TotalSeconds);
			Measure.Custom(outpostTimeSg, (outpostTime - startTime).TotalSeconds);

			this.loadTime = this.startTime = this.outpostTime = null;
		}
	}
}