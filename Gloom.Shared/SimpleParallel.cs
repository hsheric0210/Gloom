namespace Gloom
{
	/// <summary>
	/// Similar to Parallel.ForEach and Parallel.ForEachAsync, but it is ALWAYS forced to parallel.
	/// </summary>
	public static class SimpleParallel
	{
		/// <summary>
		/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
		/// </summary>
		public static void ForEach<T>(IEnumerable<T> values, Action<T> process) => ForEachAsync(values, process).Wait();

		/// <summary>
		/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
		/// </summary>
		public static void ForEach<T>(IEnumerable<T> values, Func<T, Task> asyncProcess) => ForEachAsync(values, asyncProcess).Wait();

		/// <summary>
		/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
		/// </summary>
		public static Task ForEachAsync<T>(IEnumerable<T> values, Action<T> process)
		{
			if (!values.TryGetNonEnumeratedCount(out var cnt))
				cnt = 128;
			var taskList = new List<Task>(cnt);
			foreach (var item in values)
				taskList.Add(Task.Run(() => process(item)));
			return Task.WhenAll(taskList);
		}

		/// <summary>
		/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
		/// </summary>
		public static Task ForEachAsync<T>(IEnumerable<T> values, Func<T, Task> asyncProcess)
		{
			if (!values.TryGetNonEnumeratedCount(out var cnt))
				cnt = 128;
			var taskList = new List<Task>(cnt);
			foreach (var item in values)
				taskList.Add(Task.Run(async () => await asyncProcess(item)));
			return Task.WhenAll(taskList);
		}
	}
}