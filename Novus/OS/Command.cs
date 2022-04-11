using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Kantan.Text;
using Novus.OS.Win32;
using Novus.Utilities;

// ReSharper disable UnusedMember.Global
#nullable enable

namespace Novus.OS;

/// <summary>
///     Utilities for working with the command prompt.
/// </summary>
public static class Command
{
	/// <summary>
	///     Creates a <see cref="System.Diagnostics.Process" /> to execute <paramref name="args" /> with the command prompt.
	/// </summary>
	/// <param name="args">Command to run</param>
	/// <returns>Created command prompt process</returns>
	public static Process Shell(string? args = null)
	{
		// https://stackoverflow.com/questions/5519328/executing-batch-file-in-c-sharp

		var process = Run(Native.CMD_EXE);

		if (args is { }) {
			process.StartInfo.Arguments = $"/C {args}";
		}

		return process;
	}

	public static Process Run(string fileName, params string[] args)
	{
		var qp = QProcess.Create(fileName, args);
		qp.Start();
		return qp;
	}

	public static Process Py(string[] commands)
	{
		string args = commands.QuickJoin(Environment.NewLine);

		args = args.Replace('\"', '\'');

		var startInfo = new ProcessStartInfo(Native.PYTHON_EXE)
		{
			Arguments              = $"-c \"{args}\"",
			UseShellExecute        = false,
			RedirectStandardOutput = true,
			StandardOutputEncoding = Encoding.UTF8
		};

		var proc = new Process
		{
			StartInfo           = startInfo,
			EnableRaisingEvents = true
		};

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

		var startInfo = new ProcessStartInfo(fileName)
		{
			WindowStyle     = ProcessWindowStyle.Hidden,
			Verb            = "runas",
			UseShellExecute = true,

		};

		var proc = new Process
		{
			StartInfo           = startInfo,
			EnableRaisingEvents = true
		};

		return proc;
	}
}