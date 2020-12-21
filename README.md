# TitanShark.Thresher
Provides extendable **Interception Mechanism** for `HttpClient`.

## TitanShark.Thresher.Core
Introduces **Fundamentals** for **intercepting** `HttpClient` via customization of `HttpClientHandler`.

**Nuget Package:** https://www.nuget.org/packages/TitanShark.Thresher.Core/

**Minimum DotNet's Version required:** 
- .Net Framework 4.6.2
- .Net Standard 2.0 
- .Net 5.0 (yayy! IoI)

### Core concepts:
`InterceptableHttpClientHandler` is your new friend :-), as it provides you the possibilities for customizing behaviors of `HttpClientHandler` via your own Interceptors (`IInterceptor`).

Interceptors will be executed by an `InterceptorRunner`. For your choice, `SequentialInterceptorsRunner` and `ParallelInterceptorsRunner` were built-in.

### Use case 1: Mocking Request (in Unit-Test)
If you need to mock the Response in your Unit-Test, you should write your own `Transmitter`, like in the below example:

``` CSharp
const string responseContentText = "Hello World!";
var transmitter = new Transmitter
    (
        (callId, request, cancellationToken) =>
        {
            _output.WriteLine($"Request to '{request.RequestUri}' was sent out.");

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Created) 
            { 
                Content = new StringContent(responseContentText) 
            });
        }
    );
var handler = new InterceptableHttpClientHandler(transmitter: transmitter);
var sut = new HttpClient(handler);
```

[More details](TitanShark.Thresher.Core.Tests/TransmitterTests.cs)

### Use case 2: Recording and replaying your Request/Reponse

For tracing, backup or anylazing, you might need to capture all Requests sent out and corresponding Responses received. Then `Recorder` and `Replayer` are your weapons.

Generally, `Recorder` is just a special `Interceptor` which saves passed-thru Request and Response as `Record` into the target `Persistence`. In the other direction, `Replayer` filters `Record` in `Persistence` to form a `Snapshot` for replaying.

``` CSharp
// recording
var persistence = new InMemoryRecordsPersistence();
var recorder = new Recorder(persistence);
var handler = new InterceptableHttpClientHandler(
    transmitter: transmitter, 
    interceptorsRunner: new SequentialInterceptorsRunner(recorder));
var client = new HttpClient(handler);

// code for sending Requests
//...

// replaying Requests with given Response's Status Codes
var snapshot = await persistence.Snapshot(
                CancellationToken.None, 
                started, ended, 
                new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound });
var replayer = new Replayer(new SequentialReplayingStrategy(), client, snapshot);
replayer.Start();
```

[More details](TitanShark.Thresher.Core.Tests/ReplayerTests.cs)

You are not limited to `InMemoryRecordsPersistence`; just write a new `Persistence` for your own need. All you have to do is adding a new implementation of `IRecordsPersistence`. 

By default, `SystemJsonRecordSerializer` (powered by `System.Text.Json` - a built-in, lightweight JSON-Lib of .Net Framework) is employed for serializing/deserializing `Record`. You can however plug your own Serialization Mechanism (implementing `IRecordSerializer`) into your `Persistence`'s Instance. An example for using `Newtonsoft.Json` is given [here](TitanShark.Thresher.Core.Tests/InterceptorTests.cs).

## TitanShark.Thresher.Realm
Introduces **Mongo Realm** as **Embedded NoSQL Persistence** for Recording/Replaying of Requests/Responses.

**Nuget Package:** https://www.nuget.org/packages/TitanShark.Thresher.Realm/

**Minimum DotNet's Version required:** 
- .Net Framework 4.6.2
- .Net Standard 2.0 
- .Net 5.0 (yayy! IoI)

Ìt cannot be simpler! Just use `RealmRecordsPersistence`!

``` CSharp
var location = Assembly.GetExecutingAssembly().Location;
var persistence = new RealmRecordsPersistence(Path.Combine(Path.GetDirectoryName(location), "records.realm"));
var recorder = new Recorder(persistence);
```

[More details](TitanShark.Thresher.Realm.Tests/RealmRecordsPersistenceTests.cs)

More information about Mongo Realm, please refer to https://docs.mongodb.com/realm/dotnet.

## Next cool features (OpenAPI Compatibility, etc.) are coming soon! ;-)