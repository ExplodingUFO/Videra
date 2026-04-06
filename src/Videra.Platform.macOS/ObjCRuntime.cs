using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Videra.Core.Exceptions;

namespace Videra.Platform.macOS;

/// <summary>
/// Centralized Objective-C runtime interop for the Metal backend.
/// All P/Invoke declarations for objc runtime live here so callers can share
/// one consistent set of selectors, message signatures, and safety helpers.
/// </summary>
internal static class ObjCRuntime
{
    #region Core P/Invoke

    [SuppressMessage("Interoperability", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Objective-C runtime class names are UTF-8 C strings on Darwin and this interop is isolated to the macOS backend.")]
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetClass(string name);

    [SuppressMessage("Interoperability", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Objective-C selector names are UTF-8 C strings on Darwin and this interop is isolated to the macOS backend.")]
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
    public static extern IntPtr RegisterSelector(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePtr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern bool SendMessagePtrBoolReturn(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageInt(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageBool(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessagePtrVoid(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePtrIndex(IntPtr receiver, IntPtr selector, IntPtr arg1, nuint arg2);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageIndex(IntPtr receiver, IntPtr selector, int index);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern double SendMessageDouble(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageDoubleArg(IntPtr receiver, IntPtr selector, double arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern bool SendMessageBoolReturn(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePipelineState(IntPtr receiver, IntPtr selector, IntPtr descriptor, ref IntPtr error);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageNewLibrary(IntPtr receiver, IntPtr selector, IntPtr source, IntPtr options, ref IntPtr error);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageInitWithChars(IntPtr receiver, IntPtr selector, IntPtr characters, int length);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageWithBytes(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length, nuint options);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageWithLength(IntPtr receiver, IntPtr selector, nuint length, nuint options);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVertexBuffer(IntPtr receiver, IntPtr selector, IntPtr buffer, nuint offset, nuint index);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageDrawPrimitives(IntPtr receiver, IntPtr selector, nuint primitiveType, nuint vertexStart, nuint vertexCount);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageDrawIndexedPrimitives(IntPtr receiver, IntPtr selector, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageViewport(IntPtr receiver, IntPtr selector, MTLViewport viewport);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageScissorRect(IntPtr receiver, IntPtr selector, MTLScissorRect scissor);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageClearColor(IntPtr receiver, IntPtr selector, MTLClearColor color);

    [DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
    public static extern IntPtr MTLCreateSystemDefaultDevice();

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageCGSize(IntPtr receiver, IntPtr selector, CGSize size);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageInitWithCGRect(IntPtr receiver, IntPtr selector, CGRect frame);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageCGRect(IntPtr receiver, IntPtr selector, CGRect rect);

    #endregion

    #region Type-safe Helpers

    /// <summary>Register an Objective-C selector by name.</summary>
    public static IntPtr SEL(string name) => RegisterSelector(name);

    /// <summary>Get an Objective-C class by name.</summary>
    public static IntPtr objc_getClass(string name) => GetClass(name);

    /// <summary>Allocate and initialize an Objective-C object (alloc + init).</summary>
    public static IntPtr AllocInit(string className)
    {
        var alloc = Alloc(className);
        return RequireNonZeroHandle(SendMessage(alloc, SEL("init")), "AllocInit", $"Failed to allocate Objective-C object '{className}'.");
    }

    /// <summary>Allocate an Objective-C object without invoking init.</summary>
    public static IntPtr Alloc(string className)
    {
        var cls = GetClass(className);
        if (cls == IntPtr.Zero)
            throw new PlatformDependencyException(
                $"Failed to resolve Objective-C class '{className}'.",
                "Alloc",
                "macOS");

        return RequireNonZeroHandle(SendMessage(cls, SEL("alloc")), "Alloc", $"Failed to allocate Objective-C class '{className}'.");
    }

    /// <summary>Initialize an Objective-C view-like object with a frame rectangle.</summary>
    public static IntPtr InitWithFrame(IntPtr receiver, double x, double y, double width, double height)
    {
        var frame = new CGRect { x = x, y = y, width = width, height = height };
        return RequireNonZeroHandle(
            SendMessageInitWithCGRect(receiver, SEL("initWithFrame:"), frame),
            "InitWithFrame",
            "Failed to initialize Objective-C object with frame.");
    }

    /// <summary>Apply a frame rectangle to an Objective-C view-like object.</summary>
    public static void SetFrame(IntPtr receiver, double x, double y, double width, double height)
    {
        var frame = new CGRect { x = x, y = y, width = width, height = height };
        SendMessageCGRect(receiver, SEL("setFrame:"), frame);
    }

    /// <summary>Create an NSString from a C# string.</summary>
    public static IntPtr CreateNSString(string str)
    {
        var nsStringClass = GetClass("NSString");
        var alloc = SendMessage(nsStringClass, SEL("alloc"));

        unsafe
        {
            fixed (char* strPtr = str)
            {
                return SendMessageInitWithChars(alloc, SEL("initWithCharacters:length:"), (IntPtr)strPtr, str.Length);
            }
        }
    }

    /// <summary>Create a Metal buffer with existing data.</summary>
    public static IntPtr CreateMetalBuffer(IntPtr device, IntPtr data, uint length)
    {
        return RequireNonZeroHandle(
            SendMessageWithBytes(device, SEL("newBufferWithBytes:length:options:"), data, length, 0),
            "CreateMetalBuffer",
            "Failed to create Metal buffer.");
    }

    /// <summary>Create an empty Metal buffer.</summary>
    public static IntPtr CreateMetalBufferEmpty(IntPtr device, uint length, uint options)
    {
        return RequireNonZeroHandle(
            SendMessageWithLength(device, SEL("newBufferWithLength:options:"), length, options),
            "CreateMetalBufferEmpty",
            "Failed to create empty Metal buffer.");
    }

    /// <summary>Get object at indexed subscript (NSArray-like).</summary>
    public static IntPtr GetObjectAtIndex(IntPtr array, int index)
    {
        return RequireNonZeroHandle(
            SendMessageIndex(array, SEL("objectAtIndexedSubscript:"), index),
            "GetObjectAtIndex",
            $"Failed to retrieve Objective-C object at index {index}.");
    }

    /// <summary>Create a pipeline state from descriptor.</summary>
    public static IntPtr CreatePipelineState(IntPtr device, IntPtr descriptor, ref IntPtr error)
    {
        return SendMessagePipelineState(device, SEL("newRenderPipelineStateWithDescriptor:error:"), descriptor, ref error);
    }

    /// <summary>Create a library from source text.</summary>
    public static IntPtr CreateLibraryFromSource(IntPtr device, string source, ref IntPtr error)
    {
        var sourceStr = CreateNSString(source);
        try
        {
            return SendMessageNewLibrary(device, SEL("newLibraryWithSource:options:error:"), sourceStr, IntPtr.Zero, ref error);
        }
        finally
        {
            SendMessageVoid(sourceStr, SEL("release"));
        }
    }

    /// <summary>Get a shader function by name from a library.</summary>
    public static IntPtr GetFunction(IntPtr library, string functionName)
    {
        var nameStr = CreateNSString(functionName);
        var function = SendMessagePtr(library, SEL("newFunctionWithName:"), nameStr);
        SendMessageVoid(nameStr, SEL("release"));
        return RequireNonZeroHandle(function, "GetFunction", $"Failed to resolve Metal function '{functionName}'.");
    }

    /// <summary>Set the clear color on a render pass color attachment.</summary>
    public static void SetClearColor(IntPtr colorAttachment, MTLClearColor color)
    {
        SendMessageClearColor(colorAttachment, SEL("setClearColor:"), color);
    }

    public static IntPtr RequireNonZeroHandle(IntPtr handle, string operation, string message)
    {
        if (handle == IntPtr.Zero)
            throw new PlatformDependencyException(message, operation, "macOS");

        return handle;
    }

    #endregion
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGSize
{
    public double width;
    public double height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGRect
{
    public double x;
    public double y;
    public double width;
    public double height;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MTLClearColor
{
    public double red;
    public double green;
    public double blue;
    public double alpha;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MTLViewport
{
    public double originX;
    public double originY;
    public double width;
    public double height;
    public double znear;
    public double zfar;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MTLScissorRect
{
    public nuint x;
    public nuint y;
    public nuint width;
    public nuint height;
}
