using System.Collections.Generic;
using System.Threading.Tasks;
using Videra.Core.Graphics;

namespace Videra.Demo.Services;

public interface IModelImporter
{
    Task<IEnumerable<Object3D>> ImportModelsAsync();
}
