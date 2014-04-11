namespace FSharpWeb1

open System
open System.Collections.Generic
open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Mvc.Ajax
open Owin
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.Owin
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.Owin
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth
open FSharpWeb1.Providers

module Owin =
    type Startup with    
        member x.Configuration(app: IAppBuilder) = x.ConfigureAuth(app)


    [<assembly: OwinStartup(typeof<Startup>)>]
    do()
