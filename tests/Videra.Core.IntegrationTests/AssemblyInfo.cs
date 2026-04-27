using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Videra.Core.IntegrationTests
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "xUnit collection fixture names intentionally mirror collection concepts.")]
    public sealed class ProcessEnvironmentCollection
    {
        public const string Name = "ProcessEnvironment";

        private ProcessEnvironmentCollection()
        {
        }
    }
}
