using System.Threading.Tasks;
using Videra.Avalonia.Controls;

namespace Videra.Demo.Services;

public interface IModelImporter
{
    Task<ModelLoadBatchResult> ImportModelsAsync();
}
