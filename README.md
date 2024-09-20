# Logging.Component

**Logging.Component** is a .NET component that simplifies logging integration within your application and facilitates sending logs to RabbitMQ.

**Trace Request and Response:**
Tracks all incoming requests and their corresponding responses in your application.

**Handle All Exceptions:**
Captures all unhandled exceptions to ensure comprehensive error reporting.

### Installation

Install the component

### Configuration

Open program.cs in your project and add the code bellow

```csharp
builder.Services.AddRabbitMqLoggingServices(builder.Configuration);
```

In your appsettings.json configure the RabbitMQ

```json
"RabbitMqLoggingConfig": {
  "ConnectionString": "amqp://user:password@host:port",
  "QueueNameTrace": "userTraceAPIQueue", 
  "QueueNameLog": "userLogAPIQueue"  
}
```
**QueueNameTrace:** Name of queue to save trace log
**QueueNameLog:** name of queue to save exceptions log

**Enable Trace Request and Response:**

```csharp
app.UseRequestLoggingMiddleware();
```

**Enable Handle All Exceptions:**
```csharp
app.UseExceptionLoggingMiddleware();
```
# Example final result:

**Trace Request and Response:**

```json
{
    "RequestId": "0542aa30-3b2f-48d6-a14c-3e802208c349",
    "RequestTimestamp": "2024-09-20T00:34:00.1500126Z",
    "RequestURL": "http://host/api/v1/Authenticate/Login",
    "RequestHeaders": {
        "Accept": "*/*",
        "Host": "http://host.com.br",
        "User-Agent": "PostmanRuntime/7.42.0",
        "Accept-Encoding": "gzip, deflate, br",
        "Content-Type": "application/json",
        "traceparent": "00-593afe042a63f336c06bad8aa4886d5e-dc47bab512d672f8-00",
        "Content-Length": "61",
        "Postman-Token": "16c902a5-92c8-4ca9-9b97-71386621349d",
        "X-Forwarded-For": "172.18.0.1",
        "X-Forwarded-Host": "localhost:8085",
        "X-Forwarded-Proto": "https"
    },
    "RequestBody": "{\"Login\":\"administrator@email.com.br\",\"Password\":\"123@mudar\"}",
    "TraceResponse": {
        "ResponseId": "0542aa30-3b2f-48d6-a14c-3e802208c349",
        "ResponseStatusCode": 200,
        "ResponseHeaders": {
            "Content-Type": "application/json; charset=utf-8"
        },
        "ResponseBody": "{\"token\":\"eyJhbGciOiJQUzI1NiIsImtpZCI6IkRkM1R0ZWZLaF9oeHh\",\"name\":\"Administrator\"}"
    },
    "Id": "adfd64ae-1748-4045-8cfb-6504a4e24e0b",
    "CreatedAt": "2024-09-20T00:34:00.1500152+00:00"
}
```

**Handle All Exceptions:**

```json
{
    "Title": "Bad Request",
    "Detail": "Value cannot be null. (Parameter 'Error authentication')",
    "StatusCode": 400,
    "Type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "Extensions": null,
    "Id": "3a64a1c9-b137-4ce4-bcee-b5ff5c01154e",
    "CreatedAt": "2024-09-20T00:54:03.5141767+00:00"
}
```
