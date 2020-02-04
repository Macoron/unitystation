using System;
using System.Collections;
using UnityEngine;

namespace Tests
{
	/// <summary>
	/// Becouse of Editor Test limitation you can't yield return anything except null from editor test
	/// This class allows you to wrap Coroutine IEnumerator or YieldInstruction
	/// And run them in editor test like in Playmode Tests
	/// </summary>
	public static class EditorRoutine
	{
		private class EditorRoutineBehaviour : MonoBehaviour { }
		private static EditorRoutineBehaviour instance;

		/// <summary>
		/// Coroutine host created in runtime
		/// </summary>
		private static EditorRoutineBehaviour Instance
		{
			get
			{
				if (!instance)
				{
					var routineHost = new GameObject("Test Coroutine Host");
					GameObject.DontDestroyOnLoad(routineHost);
					instance = routineHost.AddComponent<EditorRoutineBehaviour>();
				}

				return instance;
			}
		}

		/// <summary>
		/// Set true when target coroutine is finished
		/// </summary>
		private static bool isComplete = true;

		/// <summary>
		/// Execute target coroutine in a scene context
		/// Works only in a play mode
		/// </summary>
		/// <param name="targrRoutine"></param>
		/// <returns></returns>
		public static IEnumerator Execute(IEnumerator targrRoutine)
		{
			if (!Application.isPlaying)
				throw new Exception("Coroutine can't be run in editor mode!");

			if (!isComplete)
				throw new Exception("Editor already executing another coroutine!");
			isComplete = false;

			// Start a new coroutine
			Instance.StartCoroutine(RoutineWrapper(targrRoutine));

			// Wait when it finish
			while (!isComplete)
			{
				if (!Application.isPlaying)
					break;

				yield return null;
			}
		}

		/// <summary>
		/// Execute target YieldInstruction in a scene context
		/// Works only in a play mode
		/// </summary>
		/// <param name="instruction"></param>
		/// <returns></returns>
		public static IEnumerator Execute(YieldInstruction instruction)
		{
			yield return Execute(YieldInstructionWrapper(instruction));
		}

		private static IEnumerator RoutineWrapper(IEnumerator basicRoutine)
		{
			yield return basicRoutine;
			isComplete = true;
		}

		private static IEnumerator YieldInstructionWrapper(YieldInstruction instruction)
		{
			yield return instruction;
		}
	}
}

