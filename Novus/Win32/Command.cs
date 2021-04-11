using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Novus.Memory;

// ReSharper disable UnusedMember.Global
#nullable enable

namespace Novus.Win32
{
	/// <summary>
	///     Utilities for working with the command prompt.
	/// </summary>
	public static class Command
	{
		/// <summary>
		///     Creates a <see cref="Process" /> to execute <paramref name="cmd" /> with the command prompt.
		/// </summary>
		/// <param name="cmd">Command to run</param>
		/// <returns>Created command prompt process</returns>
		public static Process Shell(string cmd)
		{
			// https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp

			var startInfo = new ProcessStartInfo
			{
				FileName               = Native.CMD_EXE,
				Arguments              = $"/C {cmd}",
				RedirectStandardOutput = true,
				RedirectStandardError  = true,
				UseShellExecute        = false,
				CreateNoWindow         = true
			};

			var process = new Process
			{
				StartInfo           = startInfo,
				EnableRaisingEvents = true
			};

			return process;
		}

		public static void RunBatch(string[] commands, bool dispose)
		{
			const string s   = ".bat";
			var    fname = FileSystem.CreateRandomName() + s;
			RunBatch(commands, dispose, fname);
		}

		public static void RunBatch(string[] commands, bool dispose, string fname)
		{
			var fileName = FileSystem.CreateTempFile(fname, commands);

			var proc = CreateBatchFileProcess(fileName);

			proc.Start();


			if (dispose) {
				proc.ForceKill();
				File.Delete(fileName);
			}


		}

		public static string[] ReadAllLines(StreamReader stream)
		{
			// todo: move to SimpleCore [Streams] class?

			var list = new List<string>();

			while (!stream.EndOfStream) {
				string? line = stream.ReadLine();

				if (line != null) {
					list.Add(line);
				}
			}

			return list.ToArray();
		}

		public static Process CreateBatchFileProcess(string fileName)
		{
			var startInfo = new ProcessStartInfo
			{
				WindowStyle     = ProcessWindowStyle.Hidden,
				FileName        = Native.CMD_EXE,
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