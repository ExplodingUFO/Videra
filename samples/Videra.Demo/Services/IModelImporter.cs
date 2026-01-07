using System.Collections.Generic;
using System.Threading.Tasks;
using Videra.Core.Graphics;

// 引用 Object3D

namespace Videra.Demo.Services;

public interface IModelImporter
{
    // ViewModel 只管调用这个方法，拿到结果
    Task<IEnumerable<Object3D>> ImportModelsAsync();
}