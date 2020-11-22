// Metadata version: v2.0.50215
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}
.assembly Functions

{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}


.module Novus.Native.Functions.dll
// MVID: {A224F460-A049-4A03-9E71-80A36DBBBCD3}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY


// C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm.exe /dll Functions.il

// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi abstract sealed beforefieldinit Novus.Native.Functions
	extends [mscorlib]System.Object
{             
	.method public hidebysig static !!TRet  Call<TRet,T1>(void* fn, !!T1 arg1) cil managed
	{
	  
	  // Code size       9 (0x9)
	  .maxstack  2
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.0
	  IL_0002:  conv.i
	  IL_0003:  calli      !!TRet(!!T1)
	  IL_0008:  ret
	} // end of method Functions::Call

	.method public hidebysig static !!TRet  Call<TRet,T1,T2>(void* fn, !!T1 arg1, !!T2 arg2) cil managed
	{
	  
	  // Code size       10 (0xa)
	  .maxstack  3
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.2
	  IL_0002:  ldarg.0
	  IL_0003:  conv.i
	  IL_0004:  calli      !!TRet(!!T1,!!T2)
	  IL_0009:  ret
	} // end of method Functions::Call

	.method public hidebysig static !!TRet  Call<TRet,T1,T2,T3>(void* fn, !!T1 arg1, !!T2 arg2, !!T3 arg3) cil managed
	{
	  
	  // Code size       11 (0xb)
	  .maxstack  4
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.2
	  IL_0002:  ldarg.3
	  IL_0003:  ldarg.0
	  IL_0004:  conv.i
	  IL_0005:  calli      !!TRet(!!T1,!!T2,!!T3)
	  IL_000a:  ret
	} // end of method Functions::Call

	.method public hidebysig static !!TRet  Call<TRet>(void* fn) cil managed
	{
	  
	  // Code size       8 (0x8)
	  .maxstack  8
	  IL_0000:  ldarg.0
	  IL_0001:  conv.i
	  IL_0002:  calli      !!TRet()
	  IL_0007:  ret
	} // end of method Functions::Call

	.method public hidebysig static void*  CallReturnPointer<T1,T2>(void* fn, !!T1 arg1, !!T2 arg2) cil managed
	{
	  
	  // Code size       10 (0xa)
	  .maxstack  3
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.2
	  IL_0002:  ldarg.0
	  IL_0003:  conv.i
	  IL_0004:  calli      void*(!!T1,!!T2)
	  IL_0009:  ret
	} // end of method Functions::CallReturnPointer

	.method public hidebysig static void*  CallReturnPointer<T1,T2,T3>(void* fn, !!T1 arg1, !!T2 arg2, !!T3 arg3) cil managed
	{
	  
	  // Code size       11 (0xb)
	  .maxstack  4
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.2
	  IL_0002:  ldarg.3
	  IL_0003:  ldarg.0
	  IL_0004:  conv.i
	  IL_0005:  calli      void*(void*,void*,!!T3)
	  IL_000a:  ret
	} // end of method Functions::CallReturnPointer

	.method public hidebysig static void*  CallReturnPointer<T1>(void* fn, !!T1 arg1) cil managed
	{
	  // Code size       9 (0x9)
	  .maxstack  2
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.0
	  IL_0002:  conv.i
	  IL_0003:  calli      void*(void*)
	  IL_0008:  ret
	} // end of method Functions::CallReturnPointer

	.method public hidebysig static void*  CallReturnPointer(void* fn) cil managed
	{
	  // Code size       8 (0x8)
	  .maxstack  8
	  IL_0000:  ldarg.0
	  IL_0001:  conv.i
	  IL_0002:  calli      void*()
	  IL_0007:  ret
	} // end of method Functions::CallReturnPointer

	.method public hidebysig static void  CallVoid(void* fn) cil managed
	{
	  // Code size       8 (0x8)
	  .maxstack  8
	  IL_0000:  ldarg.0
	  IL_0001:  conv.i
	  IL_0002:  calli      void()
	  IL_0007:  ret
	} // end of method Functions::CallVoid

	.method public hidebysig static void  CallVoid<T1>(void* fn, !!T1 arg1) cil managed
	{
	  
	  // Code size       9 (0x9)
	  .maxstack  2
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.0
	  IL_0002:  conv.i
	  IL_0003:  calli      void(!!T1)
	  IL_0008:  ret
	} // end of method Functions::CallVoid

	.method public hidebysig static void  CallVoid<T1,T2>(void* fn, !!T1 arg1, !!T2 arg2) cil managed
	{
	  
	  // Code size       10 (0xa)
	  .maxstack  3
	  IL_0000:  ldarg.1
	  IL_0001:  ldarg.2
	  IL_0002:  ldarg.0
	  IL_0003:  conv.i
	  IL_0004:  calli      void(!!T1,!!T2)
	  IL_0009:  ret
	} // end of method Functions::CallVoid



} // end of class Functions