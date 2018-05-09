using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("WebFlow.UnitTest")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("WebFlow.UnitTest")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("b51cdc92-a44f-469d-9d2a-b99276bc3657")]

// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
/* Approval Tests */
[assembly: UseApprovalSubdirectory(nameof(ApprovalTests))]
[assembly: UseReporter(typeof(DiffReporter), typeof(ClipboardReporter))]