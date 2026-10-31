---
type: c4-context
title: System context and containers
description: C4-style view of SmartMetroService boundaries and current external systems.
tags: [c4, architecture]
spec_version: 1
last_review: 2026-10-01
---
# System Context

## Level 1: System context

```mermaid
flowchart LR
    Passenger[Passenger / client]
    Admin[Metro administrator]
    Gate[Gate scanner client]
    Service[SmartMetroService API]
    Gateway[AamarPay]
    Identity[Keycloak]
    Email[Email / SMTP]
    Passenger --> Service
    Admin --> Service
    Gate --> Service
    Service <--> Gateway
    Identity --> Service
    Service --> Email
```

Actors/relationships are inferred from endpoints and integrations; deployment trust boundaries are **Confirm**.

## Level 2: Containers

```mermaid
flowchart LR
    Client[Passenger or admin client]
    Api[ASP.NET Core API]
    App[Application services]
    Domain[Domain entities]
    Storage[Repositories and EF Core]
    Sql[(SQL Server)]
    Redis[(Redis distributed cache)]
    Gateway[AamarPay]
    IdP[Keycloak]
    Email[Email service]
    Client --> Api --> App
    App --> Domain
    App --> Storage --> Sql
    App --> Redis
    App --> Gateway
    Api --> IdP
    App --> Email
```

The domain project defines types and has no database engine. The current repository has no separately deployed frontend or gate device implementation.
