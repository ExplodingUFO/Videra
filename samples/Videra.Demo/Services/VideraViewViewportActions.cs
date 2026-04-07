using System;
using Videra.Avalonia.Controls;

namespace Videra.Demo.Services;

public sealed class VideraViewViewportActions : IDemoViewportActions
{
    private readonly VideraView _view;

    public VideraViewViewportActions(VideraView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    public bool FrameAll()
    {
        return _view.FrameAll();
    }

    public void ResetCamera()
    {
        _view.ResetCamera();
    }
}
