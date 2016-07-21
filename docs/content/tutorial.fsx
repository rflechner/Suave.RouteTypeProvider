(*** hide ***)
#I "../../bin/Suave.RouteTypeProvider"

(**
Tutorial
========================

Example
-------

This example demonstrates using a the type provider to declare your routes in Suave.

*)

#r "Suave.RouteTypeProvider.dll"
#r "Suave.dll"
open RouteTypeProvider
open System
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful

module Routes =
  type FindUserById = routeTemplate<"/findUser/{id:int}">
  type SayBonjour = routeTemplate<template="/bonjour/{Name:string}/{FirstName:string}/{Age:int}", description="Say hello in french">
  type AdditionRoute = routeTemplate<"/add/{value1:int}/{value2:int}">

let now1 : WebPart =
  fun (x : HttpContext) ->
    async {
      return! OK (DateTime.Now.ToString()) x
    }
let time1 = GET >=> path "/time" >=> now1

let api =
 choose [
    time1
    // go to http://localhost:8083/time

    GET >=> Routes.FindUserById.Returns(fun m -> OK <| sprintf "id is: %A" m.id)
    // go to http://localhost:8083/findUser/789

    GET >=> Routes.SayBonjour.Returns (
      fun m -> 
        OK <| sprintf "Bonjour %s %s, your age is %d" m.Name m.FirstName m.Age
      )
    // go to http://localhost:8083/bonjour/homer/simpson/63

    GET >=> Routes.AdditionRoute.Returns(fun m -> OK <| (m.value1 + m.value2).ToString())
    // go to http://localhost:8083/add/98/5
  ]

startWebServer defaultConfig api


(**

Without the type provider
-------------------------

If you want to use an exiting type with this URL format, you can do this:

*)

open Suave.RouteTypeProvider
open Suave.RouteTypeProvider.Helpers

[<CLIMutable>]
type UserModel =
  { Id:int
    Name:string
    Activated:bool }

let api =
 choose [
     // pathModel is used with an existing type
    pathModel<UserModel> "/user/{Id:int}/{Name:string}/{Activated:bool}"
      (fun m -> OK <| sprintf "%A" m )
    // go to: http://localhost:8083/user/87/roro/true

    // pathDico id used to get params in a dictionary
    pathDico "/user/{id:int}" (fun d -> OK <| sprintf "%A" d)
    // go to: http://localhost:8083/user/87
  ]

startWebServer defaultConfig api


