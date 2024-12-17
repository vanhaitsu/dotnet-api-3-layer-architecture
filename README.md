# DotNetAPI (3-Layer Architecture)

## Introduction

This is a simple ASP.NET Core API supports authentication, real-time conversations, and messaging.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- .NET 9 or highers
- PostgreSQL
- Redis
- Cloudinary

## Getting Started

- Set up your `appsettings.json` inside the API project as following:

```
{
  // Your other settings
  "ConnectionStrings": {
    "LocalDb": "",
    "DeployDb": ""
  },
  "Redis": {
    "Configuration": "",
    "IsEnabled": "true"
  },
  "Cloudinary": {
    "Cloud": "",
    "ApiKey": "",
    "ApiSecret": "",
    "URL": ""
  },
  "URL": {
    "Client": "",
    "Server": ""
  },
  "JWT": {
    "ValidIssuer": "",
    "ValidAudience": "",
    "Secret": ""
  },
  "EmailSettings": {
    "Host": "",
    "Port": ,
    "DisplayName": "",
    "From": "",
    "Password": ""
  },
  "OAuth2": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

- **Add Migration** if needed, then **Update Database** through **Entity Framework Core** and finally you can run the project.

## References

Updating...
