namespace Suave.RouteTypeProvider

open System.Text.RegularExpressions
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System

type RouteTemplatePart =
  | PathPart of string
  | PathArg of name:string * ``type``:string option

type RouteDescriptor = 
  { Template: string
    Description: string
    Params: ParamDescriptor list
    Verb:HttpVerb }
  static member Empty =
    { Template=""; Description=""; Params=[]; Verb=Get }
and HttpVerb = 
  | Get
  | Put
  | Post
  | Delete
  | Options
  | Head
  | Patch
and ParamDescriptor = 
  { Name:string
    TypeName:string
    In:ParamContainer
    Required:bool }
// http://swagger.io/specification/#parameterObject
and ParamContainer =
  | Query
  | Header
  | Path
  | FormData
  | Body

type PathArgValue =
  { Name:string
    TypeName:string
    RawValue:string
    BoxedValue:obj }

module RouteParser =
  type Match with
    member __.HasGroup (name:string) =
      __.Groups.[name].Success
    member __.GroupOffsets (name:string) =
      let g = __.Groups.[name]
      (g.Index, g.Index + g.Value.Length)

  let stringOf (chars:char list) =
    chars |> Array.ofList |> String

  let stringOfRev =
    List.rev >> stringOf

  let parseTemplate (template:string) =
    let (|RouteParam|_|) chars =
      let s = stringOf chars
      let reg = Regex(@"^\{ (?<name>\w+) ([:] (?<type>\w+)) ? \}",
                  RegexOptions.Compiled ||| RegexOptions.IgnorePatternWhitespace)
      match reg.Match s with
      | m when not m.Success -> None
      | m when m.HasGroup "name" && m.HasGroup "type" ->
        let name = m.Groups.["name"].Value
        let typeName = m.Groups.["type"].Value
        let (_,e) = m.GroupOffsets "type"
        let token = PathArg (name, Some typeName)
        Some (token, e + 1)
      | m when m.HasGroup "name" ->
        let name = m.Groups.["name"].Value
        let (_,e) = m.GroupOffsets "name"
        let token = PathArg (name, None)
        Some (token, e + 1)
      | _ -> None
    let rec loop (tmp:char list) (acc:RouteTemplatePart list) (buffer:char list) =
      match tmp with
      | c :: t when c <> '{' && c <> '}' ->
          c :: buffer |> loop t acc
      | RouteParam (token, e) ->
          let nacc =
            if buffer.Length > 0
            then (token :: (PathPart(stringOfRev buffer)) :: acc)
            else (token :: acc)
          loop (tmp |> List.skip e) nacc []
      | _ when buffer.Length > 0 ->
          ((PathPart(stringOfRev buffer)) :: acc) |> List.rev
      | _ -> acc |> List.rev
    loop (template.ToCharArray() |> Array.toList) [] []

module UrlTypeParsers =
  let rec matchRouteAndUrl (url:string)
    (parts:RouteTemplatePart list)
    (acc:string list) =
    match parts with
    | PathPart s :: t ->
        let i = url.ToLowerInvariant().IndexOf(s.ToLowerInvariant())
        if i >= 0 then
            let start = url.Substring (0, i)
            let j = i + s.Length
            let rest = url.Substring (j, url.Length - j)
            matchRouteAndUrl rest t (if i > 0 then acc @ [start] else acc)
        else None
    | PathArg _ :: [] -> Some (acc @ [url])
    | _ :: t -> matchRouteAndUrl url t acc
    | [] -> if url.Length = 0 then Some acc else None

  let chekcInt (s:string) : obj option =
    let v = ref 0L
    if Int64.TryParse(s, v)
    then Some (box !v)
    else None
  let checkBool (s:string) : obj option =
    let v = ref false
    if Boolean.TryParse(s, v)
    then Some (box !v)
    else None
  let checkDate (s:string) : obj option =
    let v = ref DateTime.MinValue
    if DateTime.TryParse(s, v)
    then Some (box !v)
    else None

  let typeCheckers =
    [
      ("string", fun (v:string) -> Some(box v))
      ("int", chekcInt)
      ("integer", chekcInt)
      ("bool", checkBool)
      ("boolean", checkBool)
      ("date", checkDate)
      ("datetime", checkDate)
    ] |> dict

  let partTypes =
    [
      ("string", typeof<string>)
      ("int", typeof<Int64>)
      ("integer", typeof<Int64>)
      ("bool", typeof<Boolean>)
      ("boolean", typeof<Boolean>)
      ("date", typeof<DateTime>)
      ("datetime", typeof<DateTime>)
    ] |> dict

  let handle parts path =
    let parsed =
      parts
      |> List.filter (fun p -> match p with | PathArg _ -> true | _ -> false)
    match matchRouteAndUrl path parts [] with
    | None -> None
    | Some values when values.Length <> parsed.Length -> None
    | Some values ->
        let args =
          values
          |> List.zip parsed
          |> List.choose (
              fun (PathArg (n,t), v) ->
                let typeName =
                  match t with
                  | Some tn -> tn.ToLowerInvariant()
                  | None -> "string"
                match typeCheckers.Item typeName <| v with
                | Some boxedValue ->
                    Some { Name=n; TypeName=typeName; RawValue=v; BoxedValue=boxedValue }
                | None -> None
              )
        if args.Length <> parsed.Length
        then None
        else Some args

open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful

open RouteParser
open UrlTypeParsers

open System.Collections.Generic

type PathRoute = string -> WebPart
module Helpers =
  let pathDico (format:string) (h : IDictionary<string, obj> -> WebPart) : WebPart =
    let template = parseTemplate format
    let buildDictionary (args:PathArgValue list) =
      args
      |> List.map (fun a -> a.Name, a.BoxedValue)
      |> dict
    let F (r:HttpContext) =
      match handle template r.request.url.AbsolutePath with
      | Some p ->
        let d = buildDictionary p
        let part = h d
        part r
      | None ->
        fail
    F

  let pathModelOfType (ty:Type) (format:string) (h : obj -> WebPart) : WebPart =
    pathDico format
      (fun d ->
        let m = Activator.CreateInstance ty
        for p in ty.GetProperties() do
          if d.ContainsKey p.Name
          then 
            match (d.Item p.Name), p.PropertyType with
            | :? int64 as v, t when t = typeof<int32> -> p.SetValue(m, int32(unbox<int64> v))
            | :? int64 as v, t when t = typeof<uint32> -> p.SetValue(m, uint32(unbox<int64> v))
            | v,_ -> p.SetValue(m, v)
        h m
      )

  let pathModel<'t> (format:string) (h : 't -> WebPart) : WebPart =
    let ty = typeof<'t>
    pathModelOfType ty format (fun p -> h (unbox<'t> p))

  type PathModelBuilder (format) =
    member __.GetWebPart(f:FSharpFunc<#IDictionary<string, obj>, WebPart>) : WebPart =
      let template = parseTemplate format
      let buildDictionary (args:PathArgValue list) =
        args
        |> List.map (fun a -> a.Name, a.BoxedValue)
        |> dict
      let F (r:HttpContext) =
        match handle template r.request.url.AbsolutePath with
        | Some p ->
          let d = buildDictionary p
          let part = f.Invoke d
          part r
        | None ->
          fail
      F

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

type HandlerFunc<'t> = FSharpFunc<'t, unit>
type RouteHandler = 
  { Function:FSharpFunc<Object, unit>
    ArgType:Type }

[<TypeProvider>]
type RouteTypeProvider () as this =
  inherit TypeProviderForNamespaces ()
  let ns = "Suave.RouteTypeProvider"
  let asm = Assembly.GetExecutingAssembly()
  let tyName = "routeTemplate"
  let myType = ProvidedTypeDefinition(asm, ns, tyName, None)

  do myType.DefineStaticParameters(
    [
      ProvidedStaticParameter("template", typeof<string>)
      ProvidedStaticParameter("description", typeof<string>, "")
    ],
      fun typeName [|:? string as template; :? string as description |] ->
        let ty = ProvidedTypeDefinition(asm, ns, typeName, None)
        let parts = parseTemplate template

        ProvidedConstructor([],
                InvokeCode=(fun _ -> <@@ parseTemplate template @@> ))
            |> ty.AddMember
        
        let modelType = ProvidedTypeDefinition(
                                "Model",
                                baseType = Some typeof<IDictionary<string, obj>>,
                                HideObjectMethods = true)
        for part in parts do
          match part with
          | PathArg (n,t) ->
              let tn = if t.IsSome then t.Value else "string"
              let ty =
                if partTypes.ContainsKey tn
                then partTypes.Item tn
                else typeof<obj>
              
              ProvidedProperty(n, ty,
                        GetterCode=fun args ->
                          <@@
                            let values = (%%args.Head : IDictionary<string, obj>)
                            if values.ContainsKey n
                            then values.Item n
                            else failwithf "Cannont find %s" n
                          @@>
                          )
                          |> modelType.AddMember
          | _ -> ()
        ProvidedConstructor([ProvidedParameter("values", typeof<IDictionary<string, obj>>)],
                InvokeCode=(fun [c] -> <@@ %%c:IDictionary<string, obj> @@> ))
            |> modelType.AddMember
        do ty.AddMember modelType

        let t1 = typedefof<FSharpFunc<_,_>>.MakeGenericType(modelType, typeof<WebPart>)
        let buildRouteParam = ProvidedParameter("handler", t1)

        let buildRouteMeth =
            ProvidedMethod(
                methodName = "Returns",
                parameters = [buildRouteParam],
                returnType = typeof<WebPart>,
                IsStaticMethod = true,
                InvokeCode =
                    fun args ->
                        <@@
                          let o = (%%args.Head : FSharpFunc<IDictionary<string, obj>, WebPart>)
                          Helpers.PathModelBuilder(template).GetWebPart(o)
                        @@>)
        do ty.AddMember buildRouteMeth


        ty)
  do this.AddNamespace(ns, [myType])
  do myType.AddXmlDoc("""Create a route from template like "/user/modify/{id:int}/{name:string}/{birth:datetime}" """)

[<TypeProviderAssembly>]
do ()
