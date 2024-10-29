# DotNetAPI (3-Layer Architecture)

## Introduction

This is a simple ASP.NET Core API.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- .NET 8 or higher.
- PostgreSQL is recommended.

## Getting Started

- Create and set up your `appsettings.json` inside the API project as following:

```
{
  "ConnectionStrings": {
    "LocalDB": "Your local database connection string",
    "DeployDB": "Your deploy database connection string"
  },
  "JWT": {
    "ValidAudience": "Your valid audience",
    "ValidIssuer": "Your valid issuer",
    "Secret": "Your secret key (more than 256 bits)",
    "TokenValidityInMinutes": 5,
    "RefreshTokenValidityInDays": 7
  },
  "EmailSettings": {
    "MailServer": "Your mail server",
    "MailPort": 587,
    "SenderName": "Your sender name",
    "FromEmail": "Your from email",
    "Password": "Your password"
  },
  "OAuth2": {
    "Server": {
      "RedirectURI": "Your client uri"
    },
    "Google": {
      "ClientId": "Your client id",
      "ClientSecret": "Your client secret"
    }
  }
}
```

## References

Updating...
