using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Novus.Utilities;

//todo: WIP
// [Experimental(Global.DIAG_ID_EXPERIMENTAL)]
public class QProcess : IDisposable
{

	public Process Process { get; private set; }

	public DataReceivedEventHandler ErrorData { get; private set; }

	public DataReceivedEventHandler OutputData { get; private set; }

	private ConcurrentQueue<string> ErrorQueue { get; set; }

	private ConcurrentQueue<string> OutputQueue { get; set; }

	private StringBuilder ErrorBuffer { get; set; } = new();

	private StringBuilder OutputBuffer { get; set; } = new();

	public bool IsStarted { get; private set; }

	private readonly ManualResetEvent m_outputResetEvent = new(false);

	private readonly ManualResetEvent m_errResetEvent = new(false);

	public QProcess(string fileName, DataReceivedEventHandler errorData = null,
	                DataReceivedEventHandler outputData = null, params string[] args)
	{
		ErrorQueue  = new();
		OutputQueue = new();

		OutputData = outputData ?? ((sender, eventArgs) =>
				                           HandleStreamData(eventArgs, OutputQueue, m_outputResetEvent, OutputBuffer));

		ErrorData = errorData ?? ((sender, eventArgs)
				                         => HandleStreamData(eventArgs, ErrorQueue, m_errResetEvent, ErrorBuffer));

		var proc = Create(fileName, args);

		proc.StartInfo.RedirectStandardInput = true;

		proc.ErrorDataReceived  += ErrorData;
		proc.OutputDataReceived += OutputData;

		Process = proc;
	}

	private static void HandleStreamData(DataReceivedEventArgs eventArgs, ConcurrentQueue<string> q,
	                                     EventWaitHandle a, StringBuilder b)
	{
		var data = eventArgs.Data;
		q.Enqueue(data);
		a.Set();
		b.Append(data);
	}

	public string ReadOutputBuffer()
	{
		m_outputResetEvent.WaitOne();
		string s = OutputBuffer.ToString();
		OutputBuffer.Clear();
		m_outputResetEvent.Reset();
		return s;
	}

	public IEnumerable<string> ReadOutput()
		=> ReadConcurrent(m_outputResetEvent, OutputQueue);

	public static IEnumerable<T> ReadConcurrent<T>(ManualResetEvent h, ConcurrentQueue<T> q)
	{
		h.WaitOne();

		// var rg = q.ToArray();
		// q.Clear();
		// h.Reset();

		while (q.TryDequeue(out var dd)) {
			yield return dd;
		}

		h.Reset();

		// h.Reset();

		// return rg;

	}

	public static Process Create(string fileName, string[] args)
	{
		var startInfo = new ProcessStartInfo
		{
			FileName               = fileName,
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
			UseShellExecute        = false,
			CreateNoWindow         = true,
		};

		foreach (string s in args) {
			startInfo.ArgumentList.Add(s);
		}

		var process = new Process
		{
			StartInfo           = startInfo,
			EnableRaisingEvents = true
		};

		return process;
	}

	public bool Start()
	{
		IsStarted = Process.Start();

		if (IsStarted) {
			Process.BeginOutputReadLine();
			Process.BeginErrorReadLine();
		}

		return IsStarted;
	}

	#region IDisposable

	public void Dispose()
	{
		Process?.Dispose();
		IsStarted = false;

	}

	#endregion

}