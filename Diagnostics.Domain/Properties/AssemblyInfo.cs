using System.Runtime.CompilerServices;

// Make internal members visible to test projects
[assembly: InternalsVisibleTo("LinaSys.Tests.UnitTesting")]
[assembly: InternalsVisibleTo("LinaSys.Tests.E2E")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // For Moq
