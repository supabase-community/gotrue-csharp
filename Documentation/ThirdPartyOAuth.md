# 3rd Party OAuth

Once again, Gotrue client is written to be agnostic of platform. In order for Gotrue to sign in a user from an Oauth
callback, the PKCE flow is preferred:

1) The Callback Url must be set in the Supabase Admin panel
2) The Application should have listener to receive that Callback
3) Generate a sign in request using: `client.SignIn(PROVIDER, options)` and setting the options to use the
   PKCE `FlowType`
4) Store `ProviderAuthState.PKCEVerifier` so that the application callback can use it to verify the returned code
5) In the Callback, use stored `PKCEVerifier` and received `code` to exchange for a session.

```c#
var state = await client.SignIn(Constants.Provider.Github, new SignInOptions
{
    FlowType = Constants.OAuthFlowType.PKCE,
    RedirectTo = "http://localhost:3000/oauth/callback"
});

// In callback received from Supabase returning to RedirectTo (set above)
// Url is set as: http://REDIRECT_TO_URL?code=CODE
var session = await client.ExchangeCodeForSession(state.PKCEVerifier, RETRIEVE_CODE_FROM_GET_PARAMS);
```

## Additional Example

Note: Make sure Project Settings -> Auth -> Auth settings -> User Signups is turned ON.

After your implementation with [GoogleSignInClient](https://github.com/googlesamples/google-signin-unity) or another, use the IdToken like this:

```csharp
var identityToken = Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(GoogleIdToken), 0, GoogleIdToken.Length);
```

And finally use SignInWithIdToken for Login or Create New User:

```csharp
var user = await Supabase.Auth.SignInWithIdToken(Supabase.Gotrue.Constants.Provider.Google, identityToken);
```

Thanks to [Phantom-KNA](https://gist.github.com/Phantom-KNA) for this example.