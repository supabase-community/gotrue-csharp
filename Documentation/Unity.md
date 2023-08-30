# Unity Support

The Client works with Unity. You can find an example of a session persistence
implementation for Unity at this [gist](https://gist.github.com/wiverson/fbb07498743dff19b72c9c58599931e9).

# Easy Mode

Clone this project. By default it includes settings to connect to a local Supabase instance from the CLI. To connect it
to your own production Supabase instance, create a new Supabase Settings and then drop it onto the Supabase Manager.

## What's in the Template

Copies of the Supabase libraries.
Copies of the Supabase supporting libraries.

## Key Project Setup Details

UniTask is included in the project to help with async/await support. You don't want your game UI to lock up waiting for
network requests, so you'll have to get into the weeds on async/await.

Install the Unity-specific version of the NewtonSoft JSON libraries. This version is specifically designed to work with
Unity.

[Managed stripping is set to off/minimal](https://docs.unity3d.com/Manual/ManagedCodeStripping.html). This is mainly to
avoid issues around code stripping removing constructors for JSON-related operations and reflection. Some users have
reported setting custom ProGuard configurations also works.

## Update and Dirty Flags

Often you may want to perform the following steps:

- User clicks on some UI element, which kicks off an async request
- The async request succeeds or fails
- Now you want to update the UI based on those results.

Unfortunately, Unity does not allow for async methods to be called by the UI builder. `Update`, however, can be declared
as async. So, to solve this we have to perform the following steps:

- When the user clicks on a UI element, set a flag/data indicating that this has happened.
- In the Update loop, check to see if this data flag has been set. If so, call the async method. Send the resulting
  information back to the calling class, either by setting the data to a field/property or via a callback.
- In the Update loop, check to see if the result data is set. If so, update the UI as appropriate.

## Notifications and Debug Info

You'll want to have some mechanism for sending notifications to the user when events occur. For example, when the
application boots up you'll likely want to refresh the user's session. That's an async request, so you'll want to have a
mechanism for posting the notification back to both the user and/or the application itself.

# Session Persistence

Note that for a variety of reasons you should only use local device preferences (e.g. screen resolution) with
PlayerPrefs. PlayerPrefs has a number of limitations, including only supporting access via the user thread.

With Supabase, you want the option to store the user session data to the filesystem - for example, if the background
thread refreshes the user session - without affecting the UI. Fortunately, we can use the standard .NET APIs for file
system access. This also has the side-effect of reducing the surface area of the Unity Engine for automated testing.

A session is a JWT and is roughly analogous to a cookie. If you am want to increase the security of the session storage
see suggestions under Save Password below.

## Network Status and Caching Data

You'll want to take the Supabase client offline when the user doesn't have a network connection to avoid accidentally
signing the user out. In addition you may want to limit the operations a user can perform when the network goes
online/offline.

## Unit Testing

Testing your Supabase integration is much, much easier if you can develop test cases that run in Unity and/or NUnit.
Unfortunately, as of this writing async test cases seem to only work with prerelease versions of the Unity Test
Framework.

You'll want to install version X via the Package Manager. Select Add By Name... and use this version. You can now
declare individual test cases with async declarations and they will work. There is a gotcha however - as of this writing
the Setup and Teardown methods cannot be declared as async and will fail.

You are encouraged to voice your support for a full 2.0 release of the Unity Test framework with full async support on
the forum.

# Implementing Save Password

Implementing save password functionality can be a nice way to streamline the user experience. However, this can present
a security risk if not implemented correctly. This is complicated by the lack of portable system level secure storage.

At a minimum, you s should look at a strategy that includes:

- Encrypt the user password on disk using a well known two way encryption algorithm.
- Use a randomly generated key for the encryption. Include an app-specific salt in the key.
- Store the randomly generated key and the encrypted password in different locations (eg Player Prefs and the
  application data directory).

In this scenario, a hostile actor would have to have access to the key, the salt, and the stored encrypted password.
This level of access probably means the device is completely compromised (eg on the level of a key logger or network
root certificate), which is usually out of scope for most applications.

# Complex Local Cache

If you would like to add more comprehensive support for local SQL storage of cached data, check out SQLite. There are a
variety of different options for setting up SQLite on Unity depending on your target platforms.

Implementing a local sync storage solution is outside the scope of this document. You may want to post ideas, questions,
and strategies to the forum.
