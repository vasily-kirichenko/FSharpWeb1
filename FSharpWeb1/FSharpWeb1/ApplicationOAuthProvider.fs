namespace FSharpWeb1

open System
open System.Collections.Generic
open System.Security.Cryptography
open System.Threading.Tasks
open System.Web
open Microsoft.AspNet.Identity
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Security.Cookies
open Microsoft.Owin.Security.OAuth
open Utilities

module Providers =
    
    type ApplicationOAuthProvider(publicClientId : string, 
                                  userManagerFactory : unit -> UserManager<IdentityUser>) =
        inherit OAuthAuthorizationServerProvider()

        /// This is a finished task (that returns no result) for a synchronous method to return 
        let finshedPlainTask = Task.FromResult(()) :> Task        

        do
            if publicClientId = null then raise (ArgumentNullException("publicClientId"))

        override x.GrantResourceOwnerCredentials(context) =
            use userManager = userManagerFactory()
            async {
                let! user = 
                    userManager.FindAsync(context.UserName, context.Password)
                    |> Async.AwaitTask
                if user = null
                then context.SetError("invalid_grant", "The user name or password is incorrect.")
                else 
                    let! oAuthIdentity = 
                        userManager.CreateIdentityAsync(user, context.Options.AuthenticationType)
                        |> Async.AwaitTask

                    let! cookiesIdentity = 
                        userManager.CreateIdentityAsync(user, CookieAuthenticationDefaults.AuthenticationType)
                        |> Async.AwaitTask
                    
                    let properties = ApplicationOAuthProvider.CreateProperties(user.UserName)
                    let ticket = AuthenticationTicket(oAuthIdentity, properties)
                    context.Validated(ticket) |> ignore
                    context.Request.Context.Authentication.SignIn(cookiesIdentity)
            } |> Async.startAsPlainTask


        override x.TokenEndpoint(context) =
            context.Properties.Dictionary
            |> Seq.iter (fun kvp -> context.AdditionalResponseParameters.Add(kvp.Key,kvp.Value))

            finshedPlainTask

        override x.ValidateClientAuthentication(context) =
            // Resource owner password credentials does not provide a client ID.
            if context.ClientId = null
            then context.Validated() |> ignore

            finshedPlainTask

        override x.ValidateClientRedirectUri(context) =
            if context.ClientId = publicClientId
            then
                let expectedRootUri : Uri = Uri(context.Request.Uri, "/")
                if expectedRootUri.AbsoluteUri = context.RedirectUri
                then context.Validated() |> ignore

            finshedPlainTask

        static member CreateProperties(userName : string) =
            let data = dict [("userName", userName)]
            AuthenticationProperties(data)


[<Measure>] type bit
[<Measure>] type byte

[<RequireQualifiedAccess>]
module RandomOAuthStateGenerator =

    let private random = new RNGCryptoServiceProvider()

    let generate (strength : int<bit>) =
        if strength <= 0<bit> 
        then invalidArg "strength" "The bit length of the token must be positive."

        let bitsPerByte = 8<bit/byte>
        let roundError = 
            let bitsPerByte = 8<bit>
            if strength % bitsPerByte > 0<bit>
            then 1<byte>
            else 0<byte>
        let length = (strength / bitsPerByte) + roundError 

        let data = Array.zeroCreate <| int length
        random.GetBytes(data)
        HttpServerUtility.UrlTokenEncode(data)
