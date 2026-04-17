using System.Reflection;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;

namespace Videra.Core.IntegrationTests.Rendering;

internal static class VideraViewRuntimeTestAccess
{
    public static object ReadRuntime(VideraView view)
    {
        var runtimeField = typeof(VideraView).GetField("_runtime", BindingFlags.Instance | BindingFlags.NonPublic);
        runtimeField.Should().NotBeNull("VideraView should centralize shell internals behind a runtime field");

        var runtime = runtimeField!.GetValue(view);
        runtime.Should().NotBeNull();
        return runtime!;
    }

    public static RenderSession ReadRenderSession(VideraView view)
    {
        return ReadRuntimeField<RenderSession>(view, "_renderSession");
    }

    public static object ReadSessionBridge(VideraView view)
    {
        return ReadRuntimeField<object>(view, "_sessionBridge");
    }

    public static T ReadRuntimeField<T>(VideraView view, string fieldName)
    {
        var runtime = ReadRuntime(view);
        var field = runtime.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull($"runtime field {fieldName} should exist on {runtime.GetType().FullName}");

        var value = field!.GetValue(runtime);
        value.Should().BeAssignableTo<T>();
        return (T)value!;
    }

    public static MethodInfo ReadRuntimeMethod(VideraView view, string methodName)
    {
        var runtime = ReadRuntime(view);
        var method = runtime.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull($"runtime method {methodName} should exist on {runtime.GetType().FullName}");
        return method!;
    }

    public static void WriteRuntimeField(VideraView view, string fieldName, object? value)
    {
        var runtime = ReadRuntime(view);
        var field = runtime.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull($"runtime field {fieldName} should exist on {runtime.GetType().FullName}");
        field!.SetValue(runtime, value);
    }
}
