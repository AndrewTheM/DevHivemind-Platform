# "Dev Hivemind" - Platform for Knowledge Sharing and Publishing

**Distributed .NET Application**

Based on microservice architecture.
Containerized via Docker Compose.
Uses IdentityServer4 as identity provider.
Integrates Azure Cognitive Services for AI functionality.
Implements a recommender system with Python/PyTorch.

To run, execute the following commands with Docker CLI:

```
docker compose build
```

```
docker compose up -d
```

Core features:
- post recommendations
- post archive
- post publishing
- comment thread
- post tagging
- post ratings
- automated moderation with AI
- post TTS

Project structure:

Services
 1. Posts
    - API
    - BusinessLogic
    - DataAccess
 2. Comments
    - API
    - BusinessLogic
    - DataAccess
 3. Accounts
    - API
    - Infrastructure
    - Application
    - Domain
 4. Files
    - API
 5. Identity
    - IdentityServer
 6. Intelligence
    - API
 7. Recommender

Gateway
1. Aggregator
2. ApiGateway

Presentation
1. UI
