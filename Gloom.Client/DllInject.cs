using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gloom.Client
{
	internal static class DllInject
	{
		private void InjectDLLIntoProcess(Process process)
		{
			// Don't re-inject into a process we've already injected
			foreach (var module in CollectModules(process))
			{
				if (module.ModuleName == this.DLL32Bit || module.ModuleName == this.DLL64Bit)
					return;
			}

			// Actually inject the DLL now!
			Injector injector = new Injector(process);

			// SHould we load the 32 bit or 64 bit DLL in the target process?
			string DLL = Is64Bit(process) ? this.DLL64Bit : this.DLL32Bit;

			injector.Inject(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + DLL);
			injector.Dispose();
		}

		private static bool Is64Bit(Process process)
		{
			if (!Environment.Is64BitOperatingSystem)
				return false;

			if (!IsWow64Process(process.Handle, out bool isWow64))
				throw new Win32Exception();

			return !isWow64;
		}

		private static string GetProcessFilename(Process p)
		{
			int capacity = 2000;
			StringBuilder builder = new StringBuilder(capacity);
			IntPtr ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);

			if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity))
			{
				return string.Empty;
			}

			return builder.ToString();
		}

		private static List<Module> CollectModules(Process process)
		{
			List<Module> collectedModules = new List<Module>();

			IntPtr[] modulePointers = new IntPtr[0];

			// Determine number of modules
			if (!EnumProcessModulesEx(process.Handle, modulePointers, 0, out int bytesNeeded, (uint)ModuleFilter.ListModulesAll))
			{
				return collectedModules;
			}

			int totalNumberofModules = bytesNeeded / IntPtr.Size;
			modulePointers = new IntPtr[totalNumberofModules];

			// Collect modules from the process
			if (EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)ModuleFilter.ListModulesAll))
			{
				for (int index = 0; index < totalNumberofModules; index++)
				{
					StringBuilder moduleFilePath = new StringBuilder(1024);
					GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (uint)moduleFilePath.Capacity);

					string moduleName = Path.GetFileName(moduleFilePath.ToString());
					GetModuleInformation(process.Handle, modulePointers[index], out var moduleInformation, (uint)(IntPtr.Size * modulePointers.Length));

					// Convert to a normalized module and add it to our list
					Module module = new Module(moduleName, moduleInformation.lpBaseOfDll, moduleInformation.SizeOfImage);
					collectedModules.Add(module);
				}
			}

			return collectedModules;
		}

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags,
			[Out] StringBuilder lpExeName, ref int lpdwSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle,
			int processId);

		[DllImport("psapi.dll")]
		public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

		[DllImport("psapi.dll")]
		public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] uint nSize);

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);

		[Flags]
		private enum ProcessAccessFlags : uint
		{
			QueryLimitedInformation = 0x00001000
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ModuleInformation
		{
			public IntPtr lpBaseOfDll;
			public uint SizeOfImage;
			public IntPtr EntryPoint;
		}

		internal enum ModuleFilter
		{
			ListModulesDefault = 0x0,
			ListModules32Bit = 0x01,
			ListModules64Bit = 0x02,
			ListModulesAll = 0x03,
		}

		private class Module
		{
			public string ModuleName { get; set; }
			public IntPtr BaseAddress { get; set; }
			public uint Size { get; set; }

			public Module(string moduleName, IntPtr baseAddress, uint size)
			{
				ModuleName = moduleName;
				BaseAddress = baseAddress;
				Size = size;
			}
		}
	}
}
