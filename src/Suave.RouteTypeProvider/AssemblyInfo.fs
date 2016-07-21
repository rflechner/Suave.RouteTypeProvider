namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Suave.RouteTypeProvider")>]
[<assembly: AssemblyProductAttribute("Suave.RouteTypeProvider")>]
[<assembly: AssemblyDescriptionAttribute("A Type Provider helping to create strongly typed routes like WebApi format")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
    let [<Literal>] InformationalVersion = "0.0.1"
