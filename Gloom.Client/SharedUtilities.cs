using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Gloom;

public static class SimpleParallel
{
	/// <summary>
	/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
	/// </summary>
	public static void ForEach<T>(IEnumerable<T> values, Action<T> process)
	{
		ForEachAsync(values, process).Wait();
	}

	/// <summary>
	/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
	/// </summary>
	public static void ForEach<T>(IEnumerable<T> values, Func<T, Task> asyncProcess)
	{
		ForEachAsync(values, asyncProcess).Wait();
	}

	/// <summary>
	/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
	/// </summary>
	public static Task ForEachAsync<T>(IEnumerable<T> values, Action<T> process)
	{
		values.TryGetNonEnumeratedCount(out var cnt);
		var taskList = new List<Task>(cnt);
		foreach (T item in values)
			taskList.Add(Task.Run(() => process(item)));
		return Task.WhenAll(taskList);
	}

	/// <summary>
	/// Similar to 'Parallel.ForEach', but every action is FORCED TO run in parallel.
	/// </summary>
	public static Task ForEachAsync<T>(IEnumerable<T> values, Func<T, Task> asyncProcess)
	{
		values.TryGetNonEnumeratedCount(out var cnt);
		var taskList = new List<Task>(cnt);
		foreach (T item in values)
			taskList.Add(Task.Run(async () => await asyncProcess(item)));
		return Task.WhenAll(taskList);
	}
}
