using System.Diagnostics;
using System.IO;

// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Shell
{
	/// <summary>
	/// Represents an executable batch file.
	/// </summary>
	public class BatchFileCommand : BaseCommand
	{
		public BatchFileCommand(string[] commands, string fileName)
		{
			Commands       = commands;
			FileName       = CreateBatchFile(fileName);
			CommandProcess = CreateProcess(FileName);
		}

		public BatchFileCommand(string[] commands) : this(commands, FileOperations.CreateRandomName() + ".bat") { }

		public string[] Commands { get; }

		public string FileName { get; }

		public override void Dispose()
		{
			base.Dispose();
			File.Delete(FileName);
		}


		public static void CreateAndRun(string[] commands, bool dispose)
		{
			var b = new BatchFileCommand(commands);
			b.Start();

			if (dispose) {
				b.Dispose();
			}
		}

		private string CreateBatchFile(string fname)
		{
			string file = Path.Combine(Path.GetTempPath(), fname);

			File.WriteAllLines(file, Commands);

			return file;
		}

		private static Process CreateProcess(string fileName)
		{
			var startInfo = new ProcessStartInfo
			{
				WindowStyle     = ProcessWindowStyle.Hidden,
				FileName        = "cmd.exe",
				Arguments       = "/C \"" + fileName + "\"",
				Verb            = "runas",
				UseShellExecute = true
			};

			var process = new Process
			{
				StartInfo           = startInfo,
				EnableRaisingEvents = true
			};

			return process;
		}
	}
}