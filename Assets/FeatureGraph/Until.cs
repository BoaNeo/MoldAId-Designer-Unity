using System;
using Utility;

namespace FeatureGraph
{
	public static class Until
	{
		public static UntilRunningOnMainThread RunningOnMainThread = new();
		public static UntilRunningInBackground RunningInBackground = new();

		public static IYield CallsBackAfterRunningOnMain(Action<Action> gpuproc)
		{
			return new UntilActionCallsBackFromMain(gpuproc);
		}
	}

	public class UntilActionCallsBackFromMain : IYield
	{
		private readonly Action<Action> _mainProc;
		private WorkerThread _worker;
		private Action _continue;

		public UntilActionCallsBackFromMain(Action<Action> main)
		{
			_mainProc = main;
		}

		public void OnYield(WorkerThread worker, Action continuation)
		{
			_worker = worker;
			_continue = continuation;
			worker.RunOnMain( MainSequence );
		}

		private void MainSequence()
		{
			_mainProc(_continue);
		}
	}

	public interface IYield
	{
		void OnYield(WorkerThread worker, Action continuation);
	}

	public class UntilRunningInBackground : IYield
	{
		public void OnYield(WorkerThread worker, Action continuation)
		{
			worker.RunInBackground(continuation);
		}
	}

	public class UntilRunningOnMainThread : IYield
	{
		public void OnYield(WorkerThread worker, Action continuation)
		{
			worker.RunOnMain(continuation);
		}
	}
}