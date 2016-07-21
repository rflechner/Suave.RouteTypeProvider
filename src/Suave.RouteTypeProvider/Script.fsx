#r "../packages/Suave/lib/net40/Suave.dll"

#load "ProvidedTypes.fs"
#load "RouteTypeProvider.fs"

open RouteTypeProvider
open System
open System.Text
open System.Text.RegularExpressions
open FSharp.Quotations
//open FSharp.Quotations.Evaluator
open FSharp.Quotations.Patterns
open FSharp.Quotations.DerivedPatterns
open FSharp.Quotations.ExprShape

let format1 = "/home"
let format2 = "/user/{id:int}"
let format3 = "/user/fromname/{name}" // default type of arg is string
let format4 = "/user/fromname/{name:string}" // same result
let format5 = "/user/modify/{id:int}/{name:string}/{birth:datetime}"

open RouteParser
open UrlTypeParsers

let r1 = parseTemplate format1
let r2 = parseTemplate format2
let r3 = parseTemplate format3
let r4 = parseTemplate format4
let r5 = parseTemplate format5

printf "%A" r5

matchRouteAndUrl format1 r1 []
matchRouteAndUrl format2 r2 []
matchRouteAndUrl format3 r3 []
matchRouteAndUrl format4 r4 []
matchRouteAndUrl format5 r5 []

matchRouteAndUrl "/user/modify/98/toto/1985" r5 []

handle r5 "/user/modify/98/toto/1985-02-11"
handle r5 "/user/modify/98a/toto/1985-02-11"

//let o = box 4
//let o2 = unbox<int> o
//o2.GetType()

type public FSharpFuncUtil =
    //[<Extension>]
    static member ToFSharpFunc<'a,'b> (func:System.Converter<'a,'b>) = fun x -> func.Invoke(x)
    //[<Extension>]
    static member ToFSharpFunc<'a,'b> (func:System.Func<'a,'b>) = fun x -> func.Invoke(x)
    //[<Extension>]
    static member ToFSharpFunc<'a,'b,'c> (func:System.Func<'a,'b,'c>) = fun x y -> func.Invoke(x,y)
    //[<Extension>]
    static member ToFSharpFunc<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = fun x y z -> func.Invoke(x,y,z)
    static member Create<'a,'b> (func:System.Func<'a,'b>) = FSharpFuncUtil.ToFSharpFunc func
    static member Create<'a,'b,'c> (func:System.Func<'a,'b,'c>) = FSharpFuncUtil.ToFSharpFunc func
    static member Create<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = FSharpFuncUtil.ToFSharpFunc func

let h = Func<string, unit>(fun s -> ()) |> FSharpFuncUtil.ToFSharpFunc

let exp = <@ fun (s:string) -> printfn "coucou" @>

let meth (exp:Expr) =
  printfn "exp: %A" exp

meth (<@ fun s -> () @>)


//type HandlerFunc<'t> = FSharpFunc<'t, unit>
let t1 = typedefof<FSharpFunc<_,_>>.MakeGenericType(typeof<string>, typeof<unit>)
