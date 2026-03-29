using System.Runtime.InteropServices;

namespace Videra.Platform.macOS;

/// <summary>
/// Centralized Objective-C runtime interop for the Metal backend.
/// All P/Invoke declarations for objc runtime live here.
/// Individual Metal classes can use these helpers instead of declaring their own DllImport entries.
///
/// Existing files retain their own DllImport for backward compatibility during the migration.
/// New code should prefer ObjCRuntime methods over inline DllImport declarations.
/// </summary>
internal static class ObjCRuntime
{
    #region Core P/Invoke

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    public static extern IntPtr GetClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    public static extern IntPtr RegisterSelector(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePtr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageInt(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageBool(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessagePtrVoid(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePtrIndex(IntPtr receiver, IntPtr selector, IntPtr arg1, nuint arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageIndex(IntPtr receiver, IntPtr selector, int index);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern double SendMessageDouble(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageDoubleArg(IntPtr receiver, IntPtr selector, double arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern bool SendMessageBoolReturn(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessagePipelineState(IntPtr receiver, IntPtr selector, IntPtr descriptor, ref IntPtr error);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageNewLibrary(IntPtr receiver, IntPtr selector, IntPtr source, IntPtr options, ref IntPtr error);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageInitWithChars(IntPtr receiver, IntPtr selector, IntPtr characters, int length);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageWithBytes(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length, nuint options);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageWithLength(IntPtr receiver, IntPtr selector, nuint length, nuint options);

    [DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
    public static extern IntPtr MTLCreateSystemDefaultDevice();

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageCGSize(IntPtr receiver, IntPtr selector, CGSize size);

    #endregion

    #region Type-safe Helpers

    /// <summary>Register an Objective-C selector by name.</summary>
    public static IntPtr SEL(string name) => RegisterSelector(name);

    /// <summary>Get an Objective-C class by name.</summary>
    public static IntPtr objc_getClass(string name) => GetClass(name);

    /// <summary>Allocate and initialize an Objective-C object (alloc + init).</summary>
    public static IntPtr AllocInit(string className)
    {
        var cls = GetClass(className);
        var alloc = SendMessage(cls, SEL("alloc"));
        return SendMessage(alloc, SEL("init"));
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
        return SendMessageWithBytes(device, SEL("newBufferWithBytes:length:options:"), data, length, 0);
    }

    /// <summary>Create an empty Metal buffer.</summary>
    public static IntPtr CreateMetalBufferEmpty(IntPtr device, uint length, uint options)
    {
        return SendMessageWithLength(device, SEL("newBufferWithLength:options:"), length, options);
    }

    /// <summary>Get object at indexed subscript (NSArray-like).</summary>
    public static IntPtr GetObjectAtIndex(IntPtr array, int index)
    {
        return SendMessageIndex(array, SEL("objectAtIndexedSubscript:"), index);
    }

    /// <summary>Create a pipeline state from descriptor.</summary>
    public static IntPtr CreatePipelineState(IntPtr device, IntPtr descriptor, ref IntPtr error)
    {
        return SendMessagePipelineState(device, SEL("newRenderPipelineStateWithDescriptor:error:"), descriptor, ref error);
    }

    /// <summary>Create a library from source code.</summary>
    public static IntPtr CreateLibraryFromSource(IntPtr device, IntPtr source, ref IntPtr error)
    {
        return SendMessageNewLibrary(device, SEL("newLibraryWithSource:options:error:"), source, IntPtr.Zero, ref error);
    }

    /// <summary>Get a shader function by name from a library.</summary>
    public static IntPtr GetFunction(IntPtr library, string functionName)
    {
        var nameStr = CreateNSString(functionName);
        var function = SendMessagePtr(library, SEL("newFunctionWithName:"), nameStr);
        SendMessageVoid(nameStr, SEL("release"));
        return function;
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
