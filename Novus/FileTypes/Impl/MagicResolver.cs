// Author: Deci | Project: Novus | Name: MagicResolver.cs
// Date: 2022/09/13 @ 22:09:37

global using ER = Novus.Properties.EmbeddedResources;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Novus.Streams;

namespace Novus.FileTypes.Impl;

/*
 * Adapted from https://github.com/hey-red/Mime
 */

public sealed class MagicResolver : IFileTypeResolver
{

	private bool m_disposed;


	/// <summary>
	///     Gets various limits related to the magic library.
	///     <see cref="MagicParams" />
	/// </summary>
	/// <param name="param"></param>
	/// <returns></returns>
	public int this[MagicParams param]
	{
		get
		{
			ThrowIfDisposed();

			if (MagicNative.magic_getparam(Magic, param, out int value) < 0) {
				throw new MagicException($"Invalid param \"{param}\".");
			}

			return value;
		}
		set
		{
			ThrowIfDisposed();

			if (MagicNative.magic_setparam(Magic, param, ref value) < 0) {
				throw new MagicException($"Invalid param \"{param}\".");
			}
		}
	}

	static MagicResolver()
	{
		_magicLock = new Lock();
		Version    = MagicNative.magic_version();
		Instance   = new MagicResolver();
	}

	/// <summary>
	///     Creates a magic cookie and load database from given path.
	/// </summary>
	/// <param name="dbPath"></param>
	/// <param name="flags"></param>
	public MagicResolver([CBN] string dbPath = null, MagicOpenFlags flags = MagicMimeFlags)
	{
		lock (_magicLock) {
			Magic = MagicNative.magic_open(flags);

			if (Magic == IntPtr.Zero) {
				throw new MagicException(LastError, "Cannot create magic cookie.");
			}

			dbPath ??= GetMagicFile();

			if (MagicNative.magic_load(Magic, dbPath) != 0) {
				throw new MagicException(LastError, "Unable to load magic database file.");
			}
		}
	}

	public const MagicOpenFlags MagicMimeFlags =
		MagicOpenFlags.MAGIC_ERROR             |
		MagicOpenFlags.MAGIC_MIME_TYPE         |
		MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
		MagicOpenFlags.MAGIC_NO_CHECK_ELF      |
		MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

	private static readonly Lock _magicLock;

	/// <summary>
	///     Contains the version number of this library which is compiled
	///     into the shared library using the constant.
	/// </summary>
	public static readonly int Version;

	public nint Magic { get; }


	public static IFileTypeResolver Instance { get; set; }

	public string LastError
	{
		get
		{
			var err = Marshal.PtrToStringAnsi(MagicNative.magic_error(Magic));
			return err != null ? Char.ToUpper(err[0]) + err[1..] : String.Empty;
		}
	}

	/// <summary>
	///     Returns a value representing current <see cref="MagicOpenFlags" /> set.
	/// </summary>
	/// <value></value>
	public MagicOpenFlags Flags
	{
		get
		{
			ThrowIfDisposed();

			return MagicNative.magic_getflags(Magic);
		}
		set
		{
			ThrowIfDisposed();

			if (MagicNative.magic_setflags(Magic, value) < 0) {
				throw new MagicException("Utime/Utimes not supported.");
			}
		}
	}


	private static string GetMagicFile()
	{
		var mgc = Path.Combine(Global.DataFolder, ER.F_Magic);

		if (!(File.Exists(mgc))) {
			throw new FileNotFoundException(mgc);
		}

		Debug.WriteLine($"magic file: {mgc}");

		return mgc;
	}

	/// <summary>
	///     Reads file from given path.
	/// </summary>
	/// <param name="filePath"></param>
	/// <returns>returns a textual description of the contents of file</returns>
	[CBN]
	public string Read(string filePath)
	{
		ThrowIfDisposed();

		var str = Marshal.PtrToStringAnsi(MagicNative.magic_file(Magic, filePath));

		return str;
	}

	public T2 Evaluate<T, T2>(Func<T, T2> fn, Func<T> f2)
	{
		ThrowIfDisposed();
		var t2 = fn(f2());

		if (t2 == null) {
			throw new Exception();
		}

		return t2;
	}

	/// <summary>
	///     Reads contents from buffer.
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="bufferSize"></param>
	/// <returns>returns a textual description of the contents of the buffer</returns>
	[CBN]
	public string Read(Memory<byte> buffer, int bufferSize)
	{
		ThrowIfDisposed();

		var length = buffer.Length < bufferSize ? buffer.Length : bufferSize;

		unsafe {
			using var mh = buffer.Pin();

			var str = Marshal.PtrToStringAnsi(MagicNative.magic_buffer(Magic, (nint) mh.Pointer, length));

			return str;

		}
	}

	/// <summary>
	///     Reads contents from stream with buffer size limit.
	/// </summary>
	/// <remarks>
	///     This method rewinds the stream if it's possible.
	/// </remarks>
	/// <param name="stream"></param>
	/// <param name="bufferSize">in bytes</param>
	/// <returns>returns a textual description of the contents of the stream</returns>
	[CBN]
	public string Read(Stream stream, int bufferSize)
	{
		ThrowIfDisposed();

		if (stream == null) {
			throw new ArgumentException(nameof(stream));
		}

		using var ms = new MemoryStream(bufferSize);
		stream.CopyTo(ms, bufferSize);

		/*byte[]    buffer = new byte[16 * 1024];
		using var ms     = new MemoryStream(bufferSize);
		int       readed;


		while ((readed = stream.Read(buffer, 0, buffer.Length)) > 0) {
			ms.Write(buffer, 0, readed);

			if (ms.Length >= bufferSize)
				break;
		}

		if (stream.CanSeek)
			stream.Position = 0;

		return Read(ms.ToArray(), (int) ms.Length);*/

		var buffer = ms.GetBuffer();
		return Read(buffer, bufferSize);
	}

	public FileType Resolve(byte[] rg, int l = FileType.RSRC_HEADER_LEN)
	{
		// var buf1 = stream.ReadBlockAsync(FileType.RSRC_HEADER_LEN);
		// buf1.Wait();
		// var buf  = buf1.Result;

		var s = Read(rg, l);
		return new FileType(s);
	}

	public FileType Resolve(Stream stream, int l = FileType.RSRC_HEADER_LEN)
	{
		return Resolve(stream.ReadHeader(), l);
	}

	/// <summary>
	///     Can be used to check the validity of entries
	///     in the colon separated database files.
	/// </summary>
	/// <param name="dbPath"></param>
	public void CheckDatabase([CBN] string dbPath = null)
	{
		ThrowIfDisposed();

		dbPath ??= GetMagicFile();

		int result = MagicNative.magic_check(Magic, dbPath);

		if (result < 0) {
			throw new MagicException(LastError);
		}
	}

	// TODO: Tests
	/// <summary>
	///     Can be used to compile the colon separated list of database files.
	/// </summary>
	/// <param name="dbPath"></param>
	public void CompileDatabase([CBN] string dbPath = null)
	{
		ThrowIfDisposed();

		if (MagicNative.magic_compile(Magic, dbPath ?? "") < 0) {
			throw new MagicException(LastError);
		}
	}

	private void ThrowIfDisposed()
	{
		if (m_disposed) {
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	private void DoDispose()
	{
		MagicNative.magic_close(Magic);
	}

	/// <summary>
	///     Cleanups all unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (m_disposed)
			return;

		DoDispose();

		m_disposed = true;

		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     <inheritdoc />
	/// </summary>
	~MagicResolver()
	{
		DoDispose();
	}

	public class MagicException : Exception
	{

		public MagicException() { }

		public MagicException([CBN] string message, string lastError) : base($"{message} : {lastError}") { }

		public MagicException([CBN] string message) : this(message, null) { }

	}

}