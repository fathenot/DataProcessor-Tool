using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("test")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // For Moq or similar libraries that use dynamic proxies
[assembly: InternalsVisibleTo("TestIntValuesStorage")] // For unit tests that need access to internal members