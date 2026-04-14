using Xunit;

namespace Videra.Core.IntegrationTests
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class ProcessEnvironmentCollection
    {
        public const string Name = "ProcessEnvironment";

        private ProcessEnvironmentCollection()
        {
        }
    }
}
