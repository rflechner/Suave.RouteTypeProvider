namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Suave.RouteTypeProvider")>]
[<assembly: AssemblyProductAttribute("Suave.RouteTypeProvider")>]
[<assembly: AssemblyDescriptionAttribute("A Type Provider helping to create strongly typed routes like WebApi format")>]
[<assembly: AssemblyVersionAttribute("0.0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.3"
    let [<Literal>] InformationalVersion = "0.0.3"
