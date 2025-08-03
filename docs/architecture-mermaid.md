# 🏗️ Architecture Mermaid - Muwasala Islamic Knowledge Network

## Architecture Globale du Système

```mermaid
graph TB
    subgraph "Couche Présentation"
        WEB[Muwasala.Web<br/>Blazor Server]
        API[Muwasala.Api<br/>REST API]
        CLI[Muwasala.Console<br/>CLI Interface]
        MCP[Muwasala.MCP<br/>Model Context Protocol]
    end

    subgraph "Couche Application"
        CORE[Muwasala.Core<br/>Core Services]
        AGENTS[Muwasala.Agents<br/>Multi-Agent System]
    end

    subgraph "Couche Données"
        KB[Muwasala.KnowledgeBase<br/>Islamic Knowledge Services]
        DB[(SQLite Database)]
        ES[(Elasticsearch)]
        FILES[JSON Files<br/>Quran & Hadith]
    end

    subgraph "Services Externes"
        OLLAMA[Ollama AI<br/>DeepSeek Models]
        SUNNAH[Sunnah.com API]
        WEB_SOURCES[Islamic Web Sources]
    end

    %% Connections
    WEB --> CORE
    API --> CORE
    CLI --> CORE
    MCP --> CORE
    
    CORE --> AGENTS
    CORE --> KB
    
    AGENTS --> OLLAMA
    AGENTS --> SUNNAH
    AGENTS --> WEB_SOURCES
    
    KB --> DB
    KB --> ES
    KB --> FILES

    %% Styling
    classDef presentation fill:#e1f5fe
    classDef application fill:#f3e5f5
    classDef data fill:#e8f5e8
    classDef external fill:#fff3e0
    
    class WEB,API,CLI,MCP presentation
    class CORE,AGENTS application
    class KB,DB,ES,FILES data
    class OLLAMA,SUNNAH,WEB_SOURCES external
```

## Architecture Multi-Agents

```mermaid
graph LR
    subgraph "DeepSeek Brain Agent"
        BRAIN[DeepSeek Brain<br/>Central Orchestrator]
    end
    
    subgraph "Agents Spécialisés"
        QNA[Quran Navigator<br/>Agent]
        HVA[Hadith Verifier<br/>Agent]
        FIA[Fiqh Advisor<br/>Agent]
        DCA[Dua Companion<br/>Agent]
        TTA[Tajweed Tutor<br/>Agent]
        SSA[Sirah Scholar<br/>Agent]
        WCA[Web Crawler<br/>Agent]
        QAA[Query Analysis<br/>Agent]
        RQA[Response Quality<br/>Agent]
        RTS[Real-Time Search<br/>Agent]
    end

    subgraph "Services de Support"
        HYBRID[Enhanced Hybrid<br/>Search Service]
        FALLBACK[Fast Fallback<br/>Service]
        CIRCUIT[Circuit Breaker<br/>Service]
    end

    USER[👤 Utilisateur] --> BRAIN
    BRAIN --> QNA
    BRAIN --> HVA
    BRAIN --> FIA
    BRAIN --> DCA
    BRAIN --> TTA
    BRAIN --> SSA
    BRAIN --> WCA
    BRAIN --> QAA
    BRAIN --> RQA
    BRAIN --> RTS
    
    QNA --> HYBRID
    HVA --> HYBRID
    FIA --> HYBRID
    
    HYBRID --> FALLBACK
    FALLBACK --> CIRCUIT
    
    BRAIN --> USER

    %% Styling
    classDef brain fill:#ff6b6b
    classDef agents fill:#4ecdc4
    classDef services fill:#45b7d1
    classDef user fill:#96ceb4
    
    class BRAIN brain
    class QNA,HVA,FIA,DCA,TTA,SSA,WCA,QAA,RQA,RTS agents
    class HYBRID,FALLBACK,CIRCUIT services
    class USER user
```

## Architecture des Données

```mermaid
graph TB
    subgraph "Sources de Données"
        QF[Quran Files<br/>114 Sourates]
        HF[Hadith Collections<br/>Bukhari, Muslim, etc.]
        AF[Additional Resources<br/>Duas, Fiqh, etc.]
    end

    subgraph "Couche de Persistance"
        SQLITE[(SQLite Database<br/>HadithRecords<br/>QuranVerses)]
        ELASTIC[(Elasticsearch<br/>Index: hadiths<br/>Full-text search)]
        CACHE[Memory Cache<br/>Fast access]
    end

    subgraph "Services d'Accès aux Données"
        IKS[Islamic Knowledge<br/>Service]
        HSS[Hadith Search<br/>Service]
        ESS[Elasticsearch<br/>Service]
        AHS[Advanced Hadith<br/>Search Service]
    end

    subgraph "Modèles de Données"
        QV[QuranVerse]
        HR[HadithRecord]
        SH[ScoredHadith]
        SR[SearchResult]
    end

    %% Data Flow
    QF --> SQLITE
    HF --> SQLITE
    AF --> SQLITE
    
    HF --> ELASTIC
    
    SQLITE --> IKS
    ELASTIC --> ESS
    CACHE --> IKS
    
    IKS --> HSS
    ESS --> AHS
    HSS --> AHS
    
    AHS --> QV
    AHS --> HR
    AHS --> SH
    AHS --> SR

    %% Styling
    classDef sources fill:#ffeb3b
    classDef persistence fill:#4caf50
    classDef services fill:#2196f3
    classDef models fill:#9c27b0
    
    class QF,HF,AF sources
    class SQLITE,ELASTIC,CACHE persistence
    class IKS,HSS,ESS,AHS services
    class QV,HR,SH,SR models
```

## Flux de Recherche

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant W as Web Interface
    participant A as API Controller
    participant B as DeepSeek Brain
    participant H as Hybrid Search
    participant S as SQLite
    participant E as Elasticsearch
    participant F as Fallback Service

    U->>W: Recherche "moon"
    W->>A: POST /api/hadith/search
    A->>B: Analyser la requête
    B->>H: Recherche hybride
    
    par Recherche parallèle
        H->>S: Recherche SQLite
        H->>E: Recherche Elasticsearch
    end
    
    alt Elasticsearch disponible
        E-->>H: Résultats Elasticsearch
        H->>B: Résultats principaux
    else Elasticsearch indisponible
        S-->>H: Résultats SQLite
        H->>F: Service de secours
        F->>B: Résultats de secours
    end
    
    B->>A: Résultats enrichis
    A->>W: JSON Response
    W->>U: Affichage des résultats
```

## Architecture de Déploiement

```mermaid
graph TB
    subgraph "Environnement de Développement"
        DEV_WEB[Web App :5237]
        DEV_API[API :5000]
        DEV_DB[(SQLite Local)]
        DEV_ES[(Elasticsearch :9200)]
        DEV_OLLAMA[Ollama :11434]
    end

    subgraph "Environnement de Production"
        PROD_WEB[Web App]
        PROD_API[API Gateway]
        PROD_DB[(Database Cluster)]
        PROD_ES[(Elasticsearch Cluster)]
        PROD_AI[AI Service]
        LB[Load Balancer]
    end

    subgraph "Services de Monitoring"
        LOGS[Logging Service]
        METRICS[Metrics Collection]
        HEALTH[Health Checks]
    end

    %% Development connections
    DEV_WEB --> DEV_API
    DEV_API --> DEV_DB
    DEV_API --> DEV_ES
    DEV_API --> DEV_OLLAMA

    %% Production connections
    LB --> PROD_WEB
    LB --> PROD_API
    PROD_WEB --> PROD_API
    PROD_API --> PROD_DB
    PROD_API --> PROD_ES
    PROD_API --> PROD_AI

    %% Monitoring
    PROD_WEB --> LOGS
    PROD_API --> METRICS
    PROD_DB --> HEALTH
    PROD_ES --> HEALTH

    %% Styling
    classDef dev fill:#e3f2fd
    classDef prod fill:#e8f5e8
    classDef monitor fill:#fff3e0
    
    class DEV_WEB,DEV_API,DEV_DB,DEV_ES,DEV_OLLAMA dev
    class PROD_WEB,PROD_API,PROD_DB,PROD_ES,PROD_AI,LB prod
    class LOGS,METRICS,HEALTH monitor
```

## Technologies Utilisées

```mermaid
mindmap
  root((Muwasala<br/>Tech Stack))
    Backend
      ASP.NET Core 8
      Entity Framework Core
      SQLite Database
      Elasticsearch 8.12
      Swagger/OpenAPI
    Frontend
      Blazor Server
      HTML/CSS/JavaScript
      Bootstrap
      Chart.js
    AI/ML
      Ollama
      DeepSeek Models
      Vector Search
      Semantic Analysis
    DevOps
      Git/GitHub
      Docker
      CI/CD Pipelines
      Monitoring
    Testing
      xUnit
      Integration Tests
      Performance Tests
      Load Testing
```

## Patterns Architecturaux

### 1. **Clean Architecture**
- Séparation claire des responsabilités
- Inversion de dépendance
- Testabilité optimale

### 2. **Multi-Agent Pattern**
- Agents spécialisés par domaine
- Orchestration centralisée
- Communication asynchrone

### 3. **CQRS (Command Query Responsibility Segregation)**
- Séparation lecture/écriture
- Optimisation des performances
- Scalabilité améliorée

### 4. **Circuit Breaker Pattern**
- Résilience aux pannes
- Dégradation gracieuse
- Récupération automatique

### 5. **Fallback Strategy**
- Services de secours
- Haute disponibilité
- Expérience utilisateur continue

## Évolutivité et Performance

```mermaid
graph LR
    subgraph "Optimisations Actuelles"
        CACHE[Memory Caching]
        INDEX[Database Indexing]
        PARALLEL[Parallel Processing]
        COMPRESS[Data Compression]
    end

    subgraph "Améliorations Futures"
        REDIS[Redis Caching]
        CDN[Content Delivery Network]
        MICROSERVICES[Microservices]
        KUBERNETES[Kubernetes Orchestration]
    end

    subgraph "Métriques de Performance"
        RESPONSE[Response Time < 100ms]
        THROUGHPUT[1000+ req/sec]
        AVAILABILITY[99.9% Uptime]
        ACCURACY[95%+ Search Accuracy]
    end

    CACHE --> REDIS
    INDEX --> CDN
    PARALLEL --> MICROSERVICES
    COMPRESS --> KUBERNETES

    %% Styling
    classDef current fill:#4caf50
    classDef future fill:#ff9800
    classDef metrics fill:#2196f3
    
    class CACHE,INDEX,PARALLEL,COMPRESS current
    class REDIS,CDN,MICROSERVICES,KUBERNETES future
    class RESPONSE,THROUGHPUT,AVAILABILITY,ACCURACY metrics
```
