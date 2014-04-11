
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


type Startup() =
    static let _publicClientId = "self"
    static let mutable _userManagerFactory = 
        (fun () -> new UserManager<IdentityUser>(new UserStore<IdentityUser>()))
    static let _oauthOptions = new OAuthAuthorizationServerOptions
                                   (TokenEndpointPath = PathString("/Token"),
                                    Provider = ApplicationOAuthProvider(_publicClientId, _userManagerFactory),
                                    AuthorizeEndpointPath = PathString("/api/Account/ExternalLogin"),
                                    AccessTokenExpireTimeSpan = TimeSpan.FromDays(14.0),
                                    AllowInsecureHttp = true)
    
    static member OAuthOptions with get() = _oauthOptions
    static member UserManagerFactory with get() = _userManagerFactory
                                     and  set(value) = _userManagerFactory <- value
    static member PublicClientId with get() = _publicClientId

    member x.ConfigureAuth(app: IAppBuilder) =
        // Enable the application to use a cookie to store information for the signed in user
        // and to use a cookie to temporarily store information about a user logging in with a third party login provider
        app.UseCookieAuthentication(CookieAuthenticationOptions()) 
           .UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)

        // Enable the application to use bearer tokens to authenticate users
        app.UseOAuthBearerTokens(Startup.OAuthOptions)

        // Uncomment the following lines to enable logging in with third party login providers
        //app.UseMicrosoftAccountAuthentication(
        //    clientId: "",
        //    clientSecret: "");

        //app.UseTwitterAuthentication(
        //    consumerKey: "",
        //    consumerSecret: "");

        //app.UseFacebookAuthentication(
        //    appId: "",
        //    appSecret: "");

        //app.UseGoogleAuthentication();

