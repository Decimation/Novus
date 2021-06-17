using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Novus.Memory;
using SimpleCore.Utilities;

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

		public static Process Py(string[] commands)
		{
			var args = commands.QuickJoin(Environment.NewLine);

			var startInfo = new ProcessStartInfo("python")
			{
				Arguments              = $"-c \"{args}\"",
				UseShellExecute        = false,
				RedirectStandardOutput = true,
				StandardOutputEncoding = Native.EncodingWin32Unicode
			};
			
			var proc = new Process() {StartInfo = startInfo};

			return proc;
		}

		public static Process Batch(string[] commands)
		{
			const string BAT_EXT = ".bat";
			return Batch(commands, FileSystem.CreateRandomName() + BAT_EXT);
		}

		public static Process Batch(string[] commands, string fname)
		{
			string fileName = FileSystem.CreateTempFile(fname, commands);

			/*var startInfo = new ProcessStartInfo
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

			return process;*/

			var startInfo = new ProcessStartInfo(fileName)
			{
				WindowStyle     = ProcessWindowStyle.Hidden,
				Verb            = "runas",
				UseShellExecute = true,

			};

			var proc = new Process
			{
				StartInfo = startInfo
			};

			return proc;
		}
	}
}