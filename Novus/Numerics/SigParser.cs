﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novus.Runtime;
// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Novus.Numerics;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using static CorTokenType;
using static SigParser.CorElementType2;
using static SigParser.CEEToken;

/*
 *
 * https://github.com/microsoft/clrmd/blob/master/src/Microsoft.Diagnostics.Runtime/src/Utilities/SigParser/SigParser.cs
 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Linq.Expressions/tests/ILReader/SigParser.cs
 * https://gist.github.com/Decimation/41ce16a050fd137eebee2128915f7024
 *
 * https://github.com/dotnet/coreclr/blob/master/src/inc/sigparser.h
 *
 * https://github.com/microsoft/clrmd/blob/master/src/Microsoft.Diagnostics.Runtime/src/Common/ClrElementType.cs
 * https://github.com/microsoft/clrmd/blob/master/src/Microsoft.Diagnostics.Runtime/src/Common/ClrElementTypeExtensions.cs
 */

public unsafe struct SigParser
{
    private byte* _sig;
    private int _len;
    private int _offs;

    public SigParser(SigParser rhs)
    {
        _sig = rhs._sig;
        _len = rhs._len;
        _offs = rhs._offs;
    }

    public SigParser(nint sig, int len)
    {
        if (len != 0)
        {
            _sig = (byte*)sig.ToPointer();
        }
        else
        {
            _sig = null;
        }

        _len = len;
        _offs = 0;
    }

    public bool IsNull()
    {
        return _sig is null;
    }

    private void CopyFrom(SigParser rhs)
    {
        _sig = rhs._sig;
        _len = rhs._len;
        _offs = rhs._offs;
    }

    private void SkipBytes(int bytes)
    {
        Debug.Assert(bytes <= _len);
        _offs += bytes;
        _len -= bytes;
    }

    private bool SkipInt()
    {
        return GetData(out int _);
    }

    public bool GetData(out int data)
    {
        if (UncompressData(out data, out int size))
        {
            SkipBytes(size);
            return true;
        }

        return false;
    }

    private bool GetByte(out byte data)
    {
        if (_len <= 0)
        {
            data = 0xcc;
            return false;
        }

        data = _sig[_offs];
        SkipBytes(1);
        return true;
    }

    private bool PeekByte(out byte data)
    {
        if (_len <= 0)
        {
            data = 0xcc;
            return false;
        }

        data = _sig[_offs];
        return true;
    }

    private bool GetElemTypeSlow(out int etype)
    {
        var sigTemp = new SigParser(this);

        if (sigTemp.SkipCustomModifiers())
        {
            if (sigTemp.GetByte(out byte elemType))
            {
                etype = elemType;
                CopyFrom(sigTemp);
                return true;
            }
        }

        etype = 0;
        return false;
    }

    public bool GetElemType(out CorElementType etype)
    {
        if (GetElemType(out int e))
        {
            etype = (CorElementType)e;
            return true;
        }

        etype = CorElementType.Unknown;
        return false;
    }

    public bool GetElemType(out int etype)
    {
        if (_len > 0)
        {
            byte type = _sig[_offs];

            if (type < (int)ELEMENT_TYPE_CMOD_REQD) // fast path with no modifiers: single byte
            {
                etype = type;
                SkipBytes(1);
                return true;
            }
        }

        // Slower/normal path
        return GetElemTypeSlow(out etype);
    }

    public bool PeekCallingConvInfo(out int data)
    {
        return PeekByte(out data);
    }

    // Note: Calling convention is always one byte, not four bytes
    public bool GetCallingConvInfo(out int data)
    {
        if (PeekByte(out data))
        {
            SkipBytes(1);
            return true;
        }

        return false;
    }

    private bool GetCallingConv(out int data)
    {
        if (GetCallingConvInfo(out data))
        {
            data &= (int)IMAGE_CEE_CS_CALLCONV_MASK;
            return true;
        }

        return false;
    }

    private bool PeekData(out int data)
    {
        return UncompressData(out data, out _);
    }

    private bool PeekElemTypeSlow(out int etype)
    {
        var sigTemp = new SigParser(this);
        return sigTemp.GetElemType(out etype);
    }

    public bool PeekElemType(out CorElementType etype)
    {
        if (PeekElemType(out int e))
        {
            etype = (CorElementType)e;
            return true;
        }

        etype = CorElementType.Unknown;
        return false;
    }

    public bool PeekElemType(out int etype)
    {
        if (_len > 0)
        {
            byte type = _sig[_offs];

            if (type < (int)ELEMENT_TYPE_CMOD_REQD)
            {
                etype = type;
                return true;
            }
        }

        return PeekElemTypeSlow(out etype);
    }

    private bool PeekElemTypeSize(out int pSize)
    {
        pSize = 0;
        var sigTemp = new SigParser(this);

        if (!sigTemp.SkipAnyVASentinel())
            return false;

        if (!sigTemp.GetByte(out byte bElementType))
            return false;

        switch ((CorElementType2)bElementType)
        {
            case ELEMENT_TYPE_I8:
            case ELEMENT_TYPE_U8:
            case ELEMENT_TYPE_R8:
                pSize = sizeof(long);
                break;

            case ELEMENT_TYPE_I4:
            case ELEMENT_TYPE_U4:
            case ELEMENT_TYPE_R4:
                pSize = sizeof(int);
                break;

            case ELEMENT_TYPE_I2:
            case ELEMENT_TYPE_U2:
            case ELEMENT_TYPE_CHAR:
                pSize = sizeof(short);
                break;

            case ELEMENT_TYPE_I1:
            case ELEMENT_TYPE_U1:
            case ELEMENT_TYPE_BOOLEAN:
                pSize = sizeof(byte);
                break;

            case ELEMENT_TYPE_I:
            case ELEMENT_TYPE_U:
            case ELEMENT_TYPE_STRING:
            case ELEMENT_TYPE_PTR:
            case ELEMENT_TYPE_BYREF:
            case ELEMENT_TYPE_CLASS:
            case ELEMENT_TYPE_OBJECT:
            case ELEMENT_TYPE_FNPTR:
            case ELEMENT_TYPE_TYPEDBYREF:
            case ELEMENT_TYPE_ARRAY:
            case ELEMENT_TYPE_SZARRAY:
                pSize = nint.Size;
                break;

            case ELEMENT_TYPE_VOID:
                break;

            case ELEMENT_TYPE_END:
            case ELEMENT_TYPE_CMOD_REQD:
            case ELEMENT_TYPE_CMOD_OPT:
            case ELEMENT_TYPE_VALUETYPE:
                Debug.Fail("Asked for the size of an element that doesn't have a size!");
                return false;

            default:
                Debug.Fail("CorSigGetElementTypeSize given invalid signature to size!");
                return false;
        }

        return true;
    }

    private bool AtSentinel()
    {
        if (_len > 0)
            return _sig[_offs] == (int)ELEMENT_TYPE_SENTINEL;

        return false;
    }

    public bool GetToken(out int token)
    {
        if (UncompressToken(out token, out int size))
        {
            SkipBytes(size);
            return true;
        }

        return false;
    }

    public bool SkipCustomModifiers()
    {
        var sigTemp = new SigParser(this);

        if (!sigTemp.SkipAnyVASentinel())
            return false;

        if (!sigTemp.PeekByte(out byte bElementType))
            return false;

        while (bElementType == (int)ELEMENT_TYPE_CMOD_REQD || bElementType == (int)ELEMENT_TYPE_CMOD_OPT)
        {
            sigTemp.SkipBytes(1);

            if (!sigTemp.GetToken(out _))
                return false;

            if (!sigTemp.PeekByte(out bElementType))
                return false;
        }

        // Following custom modifiers must be an element type value which is less than ELEMENT_TYPE_MAX, or one of the other element types
        // that we support while parsing various signatures
        if (bElementType >= (int)ELEMENT_TYPE_MAX)
        {
            switch (bElementType)
            {
                case (byte)ELEMENT_TYPE_PINNED:
                    break;
                default:
                    return false;
            }
        }

        CopyFrom(sigTemp);
        return true;
    }

    private bool SkipFunkyAndCustomModifiers()
    {
        var sigTemp = new SigParser(this);

        if (!sigTemp.SkipAnyVASentinel())
            return false;

        if (!sigTemp.PeekByte(out byte bElementType))
            return false;

        while (bElementType == (int)ELEMENT_TYPE_CMOD_REQD ||
               bElementType == (int)ELEMENT_TYPE_CMOD_OPT ||
               bElementType == (int)ELEMENT_TYPE_MODIFIER ||
               bElementType == (int)ELEMENT_TYPE_PINNED)
        {
            sigTemp.SkipBytes(1);

            if (!sigTemp.GetToken(out int _))
                return false;

            if (!sigTemp.PeekByte(out bElementType))
                return false;
        }

        // Following custom modifiers must be an element type value which is less than ELEMENT_TYPE_MAX, or one of the other element types
        // that we support while parsing various signatures
        if (bElementType >= (int)ELEMENT_TYPE_MAX)
        {
            switch (bElementType)
            {
                case (byte)ELEMENT_TYPE_PINNED:
                    break;
                default:
                    return false;
            }
        }

        CopyFrom(sigTemp);
        return true;
    } // SkipFunkyAndCustomModifiers

    private bool SkipAnyVASentinel()
    {
        if (!PeekByte(out byte bElementType))
            return false;

        if (bElementType == (int)ELEMENT_TYPE_SENTINEL)
            SkipBytes(1);

        return true;
    } // SkipAnyVASentinel

    public bool SkipExactlyOne()
    {
        if (!GetElemType(out int typ))
            return false;

        if (!((CorElementType)typ).IsPrimitive())
        {
            switch ((CorElementType2)typ)
            {
                default:
                    return false;

                case ELEMENT_TYPE_VAR:
                case ELEMENT_TYPE_MVAR:
                    if (!GetData(out _))
                        return false;

                    break;

                case ELEMENT_TYPE_OBJECT:
                case ELEMENT_TYPE_STRING:
                case ELEMENT_TYPE_TYPEDBYREF:
                    break;

                case ELEMENT_TYPE_BYREF:
                case ELEMENT_TYPE_PTR:
                case ELEMENT_TYPE_PINNED:
                case ELEMENT_TYPE_SZARRAY:
                    if (!SkipExactlyOne())
                        return false;

                    break;

                case ELEMENT_TYPE_VALUETYPE:
                case ELEMENT_TYPE_CLASS:
                    if (!GetToken(out _))
                        return false;

                    break;

                case ELEMENT_TYPE_FNPTR:
                    if (!SkipSignature())
                        return false;

                    break;

                case ELEMENT_TYPE_ARRAY:
                    // Skip element type
                    if (!SkipExactlyOne())
                        return false;

                    // Get rank;
                    int rank;

                    if (!GetData(out rank))
                        return false;

                    if (rank > 0)
                    {
                        if (!GetData(out int sizes))
                            return false;

                        while (sizes-- != 0)
                            if (!GetData(out _))
                                return false;

                        if (!GetData(out int bounds))
                            return false;

                        while (bounds-- != 0)
                            if (!GetData(out _))
                                return false;
                    }

                    break;

                case ELEMENT_TYPE_SENTINEL:
                    // Should be unreachable since GetElem strips it
                    break;

                case ELEMENT_TYPE_INTERNAL:
                    if (!GetData(out _))
                        return false;

                    break;

                case ELEMENT_TYPE_GENERICINST:
                    // Skip generic type
                    if (!SkipExactlyOne())
                        return false;

                    // Get number of parameters
                    int argCnt;

                    if (!GetData(out argCnt))
                        return false;

                    // Skip the parameters
                    while (argCnt-- != 0)
                        SkipExactlyOne();
                    break;
            }
        }

        return true;
    }

    private bool SkipMethodHeaderSignature(out int pcArgs)
    {
        pcArgs = 0;

        // Skip calling convention
        if (!GetCallingConvInfo(out int uCallConv))
            return false;

        if (uCallConv == (int)IMAGE_CEE_CS_CALLCONV_FIELD || uCallConv == (int)IMAGE_CEE_CS_CALLCONV_LOCAL_SIG)
            return false;

        // Skip type parameter count
        if ((uCallConv & (int)IMAGE_CEE_CS_CALLCONV_GENERIC) == (int)IMAGE_CEE_CS_CALLCONV_GENERIC)
            if (!GetData(out _))
                return false;

        // Get arg count;
        if (!GetData(out pcArgs))
            return false;

        // Skip return type;
        if (!SkipExactlyOne())
            return false;

        return true;
    } // SigParser::SkipMethodHeaderSignature

    private bool SkipSignature()
    {
        if (!SkipMethodHeaderSignature(out int args))
            return false;

        // Skip args.
        while (args-- > 0)
            if (!SkipExactlyOne())
                return false;

        return false;
    }

    private bool UncompressToken(out int token, out int size)
    {
        if (!UncompressData(out token, out size))
            return false;

        int tkType = s_tkCorEncodeToken[token & 3];
        token = token >> 2 | tkType;
        return true;
    }

    private byte GetSig(int offs)
    {
        Debug.Assert(offs < _len);
        return _sig[_offs + offs];
    }

    private bool UncompressData(out int pDataOut, out int pDataLen)
    {
        pDataOut = 0;
        pDataLen = 0;

        if (_len <= 0)
            return false;

        byte byte0 = GetSig(0);

        // Smallest.
        if ((byte0 & 0x80) == 0x00) // 0??? ????
        {
            if (_len < 1)
            {
                return false;
            }

            pDataOut = byte0;
            pDataLen = 1;
        }

        // Medium.
        else if ((byte0 & 0xC0) == 0x80) // 10?? ????
        {
            if (_len < 2)
            {
                return false;
            }

            pDataOut = (byte0 & 0x3f) << 8 | GetSig(1);
            pDataLen = 2;
        }
        else if ((byte0 & 0xE0) == 0xC0) // 110? ????
        {
            if (_len < 4)
            {
                return false;
            }

            pDataOut = (byte0 & 0x1f) << 24 | GetSig(1) << 16 | GetSig(2) << 8 | GetSig(3);
            pDataLen = 4;
        }
        else // We don't recognize this encoding
        {
            return false;
        }

        return true;
    }

    private bool PeekByte(out int data)
    {
        if (!PeekByte(out byte tmp))
        {
            data = 0xcc;
            return false;
        }

        data = tmp;
        return true;
    }

#pragma warning disable CA1823
    //private const int mdtModule                 = 0x00000000;
    //private const int mdtTypeRef                = 0x01000000;
    //private const int mdtTypeDef                = 0x02000000;
    //private const int mdtFieldDef               = 0x04000000;
    //private const int mdtMethodDef              = 0x06000000;
    //private const int mdtParamDef               = 0x08000000;
    //private const int mdtInterfaceImpl          = 0x09000000;
    //private const int mdtMemberRef              = 0x0a000000;
    //private const int mdtCustomAttribute        = 0x0c000000;
    //private const int mdtPermission             = 0x0e000000;
    //private const int mdtSignature              = 0x11000000;
    //private const int mdtEvent                  = 0x14000000;
    //private const int mdtProperty               = 0x17000000;
    //private const int mdtMethodImpl             = 0x19000000;
    //private const int mdtModuleRef              = 0x1a000000;
    //private const int mdtTypeSpec               = 0x1b000000;
    //private const int mdtAssembly               = 0x20000000;
    //private const int mdtAssemblyRef            = 0x23000000;
    //private const int mdtFile                   = 0x26000000;
    //private const int mdtExportedType           = 0x27000000;
    //private const int mdtManifestResource       = 0x28000000;
    //private const int mdtGenericParam           = 0x2a000000;
    //private const int mdtMethodSpec             = 0x2b000000;
    //private const int mdtGenericParamConstraint = 0x2c000000;
    //private const int mdtString = 0x70000000;
    //private const int mdtName   = 0x71000000;
    //private const int mdtBaseType = 0x72000000;

    private static readonly int[] s_tkCorEncodeToken =
        {(int) TypeDef, (int) TypeRef, (int) TypeSpec, (int) BaseType};

    public enum CEEToken
    {
        IMAGE_CEE_CS_CALLCONV_DEFAULT = 0x0,

        IMAGE_CEE_CS_CALLCONV_VARARG = 0x5,
        IMAGE_CEE_CS_CALLCONV_FIELD = 0x6,
        IMAGE_CEE_CS_CALLCONV_LOCAL_SIG = 0x7,
        IMAGE_CEE_CS_CALLCONV_PROPERTY = 0x8,
        IMAGE_CEE_CS_CALLCONV_UNMGD = 0x9,
        IMAGE_CEE_CS_CALLCONV_GENERICINST = 0xa,
        IMAGE_CEE_CS_CALLCONV_NATIVEVARARG = 0xb,
        IMAGE_CEE_CS_CALLCONV_MAX = 0xc,

        IMAGE_CEE_CS_CALLCONV_MASK = 0x0f,
        IMAGE_CEE_CS_CALLCONV_HASTHIS = 0x20,
        IMAGE_CEE_CS_CALLCONV_EXPLICITTHIS = 0x40,
        IMAGE_CEE_CS_CALLCONV_GENERIC = 0x10,
    }

    public enum CorElementType2
    {
        ELEMENT_TYPE_END = 0x0,
        ELEMENT_TYPE_VOID = 0x1,
        ELEMENT_TYPE_BOOLEAN = 0x2,
        ELEMENT_TYPE_CHAR = 0x3,
        ELEMENT_TYPE_I1 = 0x4,
        ELEMENT_TYPE_U1 = 0x5,
        ELEMENT_TYPE_I2 = 0x6,
        ELEMENT_TYPE_U2 = 0x7,
        ELEMENT_TYPE_I4 = 0x8,
        ELEMENT_TYPE_U4 = 0x9,
        ELEMENT_TYPE_I8 = 0xa,
        ELEMENT_TYPE_U8 = 0xb,
        ELEMENT_TYPE_R4 = 0xc,
        ELEMENT_TYPE_R8 = 0xd,
        ELEMENT_TYPE_STRING = 0xe,
        ELEMENT_TYPE_PTR = 0xf,
        ELEMENT_TYPE_BYREF = 0x10,
        ELEMENT_TYPE_VALUETYPE = 0x11,
        ELEMENT_TYPE_CLASS = 0x12,
        ELEMENT_TYPE_VAR = 0x13,
        ELEMENT_TYPE_ARRAY = 0x14,
        ELEMENT_TYPE_GENERICINST = 0x15,
        ELEMENT_TYPE_TYPEDBYREF = 0x16,
        ELEMENT_TYPE_I = 0x18,
        ELEMENT_TYPE_U = 0x19,
        ELEMENT_TYPE_FNPTR = 0x1B,
        ELEMENT_TYPE_OBJECT = 0x1C,
        ELEMENT_TYPE_SZARRAY = 0x1D,
        ELEMENT_TYPE_MVAR = 0x1e,
        ELEMENT_TYPE_CMOD_REQD = 0x1F,
        ELEMENT_TYPE_CMOD_OPT = 0x20,
        ELEMENT_TYPE_INTERNAL = 0x21,
        ELEMENT_TYPE_MAX = 0x22,
        ELEMENT_TYPE_MODIFIER = 0x40,
        ELEMENT_TYPE_SENTINEL = 0x01 | ELEMENT_TYPE_MODIFIER,
        ELEMENT_TYPE_PINNED = 0x05 | ELEMENT_TYPE_MODIFIER,
    }
}