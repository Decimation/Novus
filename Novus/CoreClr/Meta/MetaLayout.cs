using System;
using System.Collections.Generic;
using System.Text;
using Novus.CoreClr.VM.EE;
using Novus.Memory;

namespace Novus.CoreClr.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="EEClassLayoutInfo"/></description></item>
	///     </list>
	/// </summary>
	public sealed unsafe class MetaLayout : ClrStructure<EEClassLayoutInfo>
	{
		#region Constructors

		public MetaLayout(Pointer<EEClassLayoutInfo> ptr) : base(ptr) { }

		#endregion


		#region Accessors

		public int NativeSize => Value.Reference.NativeSize;

		public int ManagedSize => Value.Reference.ManagedSize;

		public LayoutFlags Flags => Value.Reference.Flags;

		public int PackingSize => Value.Reference.PackingSize;

		public int NumCTMFields => Value.Reference.NumCTMFields;

		public Pointer<byte> FieldMarshalers => Value.Reference.FieldMarshalers;

		#endregion
	}
}
