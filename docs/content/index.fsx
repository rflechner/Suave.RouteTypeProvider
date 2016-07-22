(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/Suave.RouteTypeProvider"
#r "Suave.RouteTypeProvider.dll"
#r "Suave.dll"

open System
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.RouteTypeProvider

(**
Suave.RouteTypeProvider
======================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Suave.RouteTypeProvider library can be <a href="https://nuget.org/packages/Suave.RouteTypeProvider">installed from NuGet</a>:
      <pre>PM> Install-Package Suave.RouteTypeProvider</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Suave.RouteTypeProvider library can be <a href="https://www.myget.org/Package/Details/romcyber?packageType=nuget&packageId=Suave.RouteTypeProvider">installed from Paket</a>:
      <pre>
group Romcyber
  source https://www.myget.org/F/romcyber/api/v3/index.json
  nuget Suave.RouteTypeProvider
      </pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

What is Suave.RouteTypeProvider ?
----------------------------

[Suave](https://suave.io/) is a FSharp lightweight web server principally used to develop REST APIs

We can declare templated and strongly typed routes like :

*)

// a and b params are integers
let additionRoute = GET >=> pathScan "/add/%d/%d" (fun (a,b) -> OK((a + b).ToString()))

(**
Suave.RouteTypeProvider is a library providing strongly typed and <b>named</b> params
*)
type SayBonjour = routeTemplate<template="/bonjour/{Name:string}/{FirstName:string}/{Age:int}">
SayBonjour.Returns (fun m -> OK <| sprintf "Bonjour %s %s, your age is %d" m.Name m.FirstName m.Age)

(**

The type provider will create a type for each route and you will be be able to access parameter properties using your autocompletion


Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork
the project and submit pull requests. If you're adding a new public API, please also
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and
redistribution for both commercial and non-commercial purposes. For more information see the
[License file][license] in the GitHub repository.

  [content]: https://github.com/fsprojects/Suave.RouteTypeProvider/tree/master/docs/content
  [gh]: https://github.com/fsprojects/Suave.RouteTypeProvider
  [issues]: https://github.com/fsprojects/Suave.RouteTypeProvider/issues
  [readme]: https://github.com/fsprojects/Suave.RouteTypeProvider/blob/master/README.md
  [license]: https://github.com/fsprojects/Suave.RouteTypeProvider/blob/master/LICENSE.txt
*)
