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
        FIA[Enhanced Fiqh Advisor<br/>Agent ⭐]
        DCA[Dua Companion<br/>Agent]
        TTA[Tajweed Tutor<br/>Agent]
        SSA[Sirah Scholar<br/>Agent]
        WCA[Web Crawler<br/>Agent]
        QAA[AI Query Analysis<br/>Agent]
        RQA[Response Quality<br/>Agent]
        RTS[Real-Time Search<br/>Agent]
        TSA[Text Summarization<br/>Agent]
        CSA[Context Search<br/>Agent]
    end

    subgraph "Services de Support"
        HYBRID[Enhanced Hybrid<br/>Search Service]
        FALLBACK[Fast Fallback<br/>Service]
        CIRCUIT[Circuit Breaker<br/>Service]
        ENHANCED[Enhanced Fiqh<br/>Service ⭐]
        PHI3[Phi-3 AI Integration<br/>Service]
        VECTOR[Vector Similarity<br/>Service]
        SEMANTIC[Semantic Analysis<br/>Service]
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
    BRAIN --> TSA
    BRAIN --> CSA
    
    QNA --> HYBRID
    HVA --> HYBRID
    FIA --> ENHANCED
    FIA --> PHI3
    FIA --> VECTOR
    FIA --> SEMANTIC
    
    ENHANCED --> HYBRID
    HYBRID --> FALLBACK
    FALLBACK --> CIRCUIT
    
    BRAIN --> USER

    %% Styling
    classDef brain fill:#ff6b6b
    classDef agents fill:#4ecdc4
    classDef services fill:#45b7d1
    classDef user fill:#96ceb4
    
    class BRAIN brain
    class QNA,HVA,FIA,DCA,TTA,SSA,WCA,QAA,RQA,RTS,TSA,CSA agents
    class HYBRID,FALLBACK,CIRCUIT,ENHANCED,PHI3,VECTOR,SEMANTIC services
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
        EFS[Enhanced Fiqh<br/>Service ⭐]
        VES[Vector Embedding<br/>Service]
        SAS[Semantic Analysis<br/>Service]
    end

    subgraph "Modèles de Données"
        QV[QuranVerse]
        HR[HadithRecord]
        SH[ScoredHadith]
        SR[SearchResult]
        EFR[EnhancedFiqhResponse ⭐]
        EFQ[EnhancedFiqhQuestion]
        FE[FiqhEvidence]
        SR[SchoolRuling]
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
    EFS --> VES
    EFS --> SAS
    
    AHS --> QV
    AHS --> HR
    AHS --> SH
    AHS --> SR
    EFS --> EFR
    EFS --> EFQ
    EFS --> FE
    EFS --> SR

    %% Styling
    classDef sources fill:#ffeb3b
    classDef persistence fill:#4caf50
    classDef services fill:#2196f3
    classDef models fill:#9c27b0
    
    class QF,HF,AF sources
    class SQLITE,ELASTIC,CACHE persistence
    class IKS,HSS,ESS,AHS,EFS,VES,SAS services
    class QV,HR,SH,SR,EFR,EFQ,FE models
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

## Flux Hybrid Fiqh Service ⭐

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant W as Blazor Web
    participant EFC as Enhanced Fiqh Controller
    participant HFS as Hybrid Fiqh Service ⭐
    participant QA as Question Analysis
    participant DB as SQLite Database
    participant AI as Ollama AI Service
    participant CACHE as Cache Layer

    U->>W: Question: "Is it permissible to pray with shoes on?"
    W->>EFC: POST /api/enhanced-fiqh/ask
    EFC->>HFS: GetEnhancedRulingAsync()
    
    Note over HFS: Phase 1: Question Analysis
    HFS->>QA: AnalyzeQuestionAsync()
    QA-->>HFS: Category: Ibadah, Complexity: Basic
    
    Note over HFS: Phase 2: Search Decision Logic
    HFS->>HFS: MakeSearchDecision()
    
    par Database Search
        HFS->>DB: SearchRulingsAsync("pray shoes")
        DB-->>HFS: Database results [0-N]
    end
    
    alt Database has sufficient results (≥3)
        Note over HFS: BD First Strategy
        HFS->>HFS: EvaluateResultQuality()
        HFS->>AI: EnhanceExistingRuling()
        AI-->>HFS: Enhanced response with context
    else Database insufficient or no results
        Note over HFS: AI Fallback Strategy  
        HFS->>AI: GenerateNewAIRuling()
        AI-->>HFS: Complete AI-generated ruling
    end
    
    Note over HFS: Phase 3: Response Enhancement
    HFS->>HFS: FormatEnhancedResponse()
    HFS->>CACHE: Cache response for future
    
    HFS->>EFC: EnhancedFiqhResponse
    EFC->>W: JSON with comprehensive ruling
    W->>U: Display: Ruling + Evidence + Schools + Context
    
    Note over U,CACHE: Response includes: Main ruling, Quranic/Hadith evidence,<br/>4 madhab positions, contemporary applications
```

## Architecture Enhanced Fiqh Hybrid System ⭐

```mermaid
graph TB
    subgraph "Enhanced Fiqh Frontend"
        UI[Enhanced Fiqh Page<br/>Blazor UI]
        TEMPLATES[Question Templates<br/>Prayer, Business, Marriage]
        SEARCH[Advanced Search Interface]
    end

    subgraph "Hybrid Service Layer ⭐"
        CONTROLLER[Enhanced Fiqh Controller]
        HYBRID[HybridFiqhService<br/>🎯 BD + AI Decision Logic]
        DECISION{SearchDecision<br/>Engine}
    end

    subgraph "Knowledge Sources"
        SQLITE[(SQLite DB<br/>FiqhRulings)]
        ENHANCED[Enhanced Fiqh Cache<br/>Pre-computed Rulings]
        HARDCODED[Hardcoded Expert<br/>Rulings (~20)]
    end

    subgraph "AI Processing Pipeline"
        ANALYSIS[Question Analysis<br/>Category + Complexity]
        OLLAMA[Ollama AI Service<br/>phi3:3.8b Model]
        ENHANCEMENT[Response Enhancement<br/>Context + Evidence]
    end

    subgraph "Intelligent Caching ⭐"
        SEARCH_CACHE[Search Results Cache]
        RESPONSE_CACHE[Response Cache]
        COMPARISON_CACHE[Comparison Cache]
        SESSION_CACHE[Session Management]
    end

    UI --> CONTROLLER
    TEMPLATES --> UI
    SEARCH --> UI
    
    CONTROLLER --> HYBRID
    HYBRID --> DECISION
    
    DECISION -->|BD Sufficient| SQLITE
    DECISION -->|BD + Enhancement| ENHANCED
    DECISION -->|AI Generation| OLLAMA
    
    HYBRID --> ANALYSIS
    ANALYSIS --> OLLAMA
    OLLAMA --> ENHANCEMENT
    
    HYBRID --> SEARCH_CACHE
    HYBRID --> RESPONSE_CACHE
    HYBRID --> COMPARISON_CACHE
    HYBRID --> SESSION_CACHE
    
    SQLITE --> HARDCODED
    ENHANCED --> HARDCODED

    %% Styling
    classDef frontend fill:#e3f2fd
    classDef hybrid fill:#fff3e0
    classDef knowledge fill:#e8f5e8
    classDef ai fill:#f3e5f5
    classDef cache fill:#fce4ec
    
    class UI,TEMPLATES,SEARCH frontend
    class CONTROLLER,HYBRID,DECISION hybrid
    class SQLITE,ENHANCED,HARDCODED knowledge
    class ANALYSIS,OLLAMA,ENHANCEMENT ai
    class SEARCH_CACHE,RESPONSE_CACHE,COMPARISON_CACHE,SESSION_CACHE cache
```

### 🎯 Système Enhanced Fiqh - Modèles et Flux Hybrides ⭐

#### Modèles Enhanced Fiqh - Structures de Données Avancées

Le système Enhanced Fiqh utilise des modèles de données sophistiqués pour représenter la complexité de la jurisprudence islamique moderne avec l'approche hybride BD+IA.

```mermaid
classDiagram
    class EnhancedFiqhRuling {
        +string Id
        +string Question
        +string Ruling
        +string Madhab
        +List~string~ Evidence
        +List~string~ Keywords
        +List~string~ ScholarlyReferences
        +string ModernApplication
        +FiqhCategory Category
        +FiqhComplexity Complexity
        +float ConfidenceScore
        +string Context
        +DateTime CreatedAt
        +string Source
        +bool IsValidated
        +Dictionary~string,object~ Metadata
    }

    class EnhancedFiqhResponse {
        +bool Success
        +string Question
        +List~EnhancedFiqhRuling~ Rulings
        +List~string~ RelatedQuestions
        +string Summary
        +EnhancedFiqhComparison Comparison
        +Dictionary~string,object~ Metadata
        +float OverallConfidence
        +string SearchStrategy
        +TimeSpan ResponseTime
        +string SessionId
        +int CacheHits
        +string Source
    }

    class EnhancedFiqhComparison {
        +List~MadhabRuling~ MadhabRulings
        +string ConsensusLevel
        +List~string~ DifferencesExplained
        +string RecommendedApproach
        +Dictionary~string,string~ ScholarlyNotes
        +List~string~ ModernConsiderations
        +string ComparativeAnalysis
        +float AgreementScore
    }

    class MadhabRuling {
        +string Madhab
        +string Ruling
        +string Evidence
        +string ScholarlyOpinion
        +float Confidence
        +List~string~ Sources
        +string ModernApplication
        +bool IsMainstream
    }

    class FiqhCategory {
        <<enumeration>>
        Prayer
        Fasting
        Zakat
        Hajj
        Marriage
        Divorce
        Business
        Inheritance
        Criminal
        Other
    }

    class FiqhComplexity {
        <<enumeration>>
        Simple
        Intermediate
        Advanced
        Scholarly
    }

    EnhancedFiqhResponse --> EnhancedFiqhRuling
    EnhancedFiqhResponse --> EnhancedFiqhComparison
    EnhancedFiqhComparison --> MadhabRuling
    EnhancedFiqhRuling --> FiqhCategory
    EnhancedFiqhRuling --> FiqhComplexity
```

#### Flux de Traitement Hybride BD+IA ⭐

Le système hybride implémente une stratégie intelligente de décision pour optimiser la qualité et la performance des réponses.

```mermaid
sequenceDiagram
    participant User as 👤 Utilisateur
    participant UI as 🖥️ Enhanced Fiqh UI
    participant Controller as 🎯 Controller
    participant HybridService as ⭐ HybridFiqhService
    participant DecisionEngine as 🧠 SearchDecision Engine
    participant Database as 🗄️ SQLite Database
    participant AI as 🤖 Phi-3 AI
    participant Cache as 💾 Cache System

    User->>UI: Question de Fiqh
    UI->>Controller: POST /api/enhanced-fiqh
    Controller->>HybridService: GetEnhancedRulingAsync(question)
    
    HybridService->>Cache: Check existing cache
    alt Cache Hit
        Cache-->>HybridService: Cached response
        HybridService-->>Controller: Enhanced response
    else Cache Miss
        HybridService->>DecisionEngine: MakeSearchDecision(question)
        
        DecisionEngine->>Database: Search database
        Database-->>DecisionEngine: DB results
        
        alt DB results >= 3 (Threshold)
            DecisionEngine-->>HybridService: USE_DATABASE
            HybridService->>Database: Get detailed rulings
            Database-->>HybridService: Enhanced database rulings
            HybridService->>AI: Enhance with AI context
            AI-->>HybridService: Enhanced response
        else DB results < 3
            DecisionEngine-->>HybridService: USE_AI
            HybridService->>AI: Generate comprehensive response
            AI-->>HybridService: AI-generated ruling
            HybridService->>Database: Store new ruling
        end
        
        HybridService->>Cache: Store response
        HybridService-->>Controller: Enhanced response
    end
    
    Controller-->>UI: JSON Response
    UI-->>User: Formatted Fiqh Response

    Note over HybridService,AI: ⭐ Le système hybride optimise<br/>la qualité et la performance<br/>en combinant BD + IA
```

## Flux de Décision Hybrid Fiqh ⭐

```mermaid
flowchart TD
    START([Question Fiqh reçue])
    
    ANALYZE[Analyse de la question<br/>Catégorie + Complexité]
    SEARCH_DB[Recherche en BD<br/>SQLite FiqhRulings]
    
    EVALUATE{Évaluation des résultats BD}
    COUNT_CHECK{Nombre ≥ 3 ?}
    QUALITY_CHECK{Qualité suffisante ?}
    
    ENHANCE_DB[Améliorer réponse BD<br/>avec contexte AI]
    GENERATE_AI[Générer nouvelle<br/>réponse avec AI]
    
    FORMAT[Formatter réponse<br/>Enhanced Structure]
    CACHE[Mise en cache<br/>intelligente]
    RESPOND[Réponse utilisateur]
    
    ERROR[Réponse de secours<br/>Consultation requise]
    
    START --> ANALYZE
    ANALYZE --> SEARCH_DB
    SEARCH_DB --> EVALUATE
    
    EVALUATE --> COUNT_CHECK
    COUNT_CHECK -->|Oui| QUALITY_CHECK
    COUNT_CHECK -->|Non| GENERATE_AI
    
    QUALITY_CHECK -->|Bonne| ENHANCE_DB
    QUALITY_CHECK -->|Faible| GENERATE_AI
    
    ENHANCE_DB --> FORMAT
    GENERATE_AI --> FORMAT
    
    FORMAT --> CACHE
    CACHE --> RESPOND
    
    SEARCH_DB -->|Erreur| ERROR
    GENERATE_AI -->|Erreur| ERROR
    ERROR --> RESPOND

    %% Styling
    classDef start fill:#4caf50,color:#fff
    classDef process fill:#2196f3,color:#fff
    classDef decision fill:#ff9800,color:#fff
    classDef ai fill:#e91e63,color:#fff
    classDef end fill:#9c27b0,color:#fff
    classDef error fill:#f44336,color:#fff
    
    class START,RESPOND start
    class ANALYZE,SEARCH_DB,FORMAT,CACHE process
    class EVALUATE,COUNT_CHECK,QUALITY_CHECK decision
    class ENHANCE_DB,GENERATE_AI ai
    class ERROR error
```

## Flux Enhanced Fiqh Advisor ⭐

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant W as Blazor Web
    participant EFC as Enhanced Fiqh Controller
    participant EFS as Enhanced Fiqh Service
    participant EFA as Enhanced Fiqh Agent
    participant PHI as Phi-3 AI Service
    participant VES as Vector Embedding Service
    participant IKS as Islamic Knowledge Service
    participant DB as Database

    U->>W: Question Fiqh: "Is cryptocurrency halal?"
    W->>EFC: POST /api/enhanced-fiqh/ask
    EFC->>EFS: ProcessQuestionAsync()
    
    EFS->>EFA: AnalyzeQuestionAsync()
    EFA->>PHI: Analyze question context
    PHI-->>EFA: Question analysis result
    
    par Recherche de preuves parallèle
        EFA->>VES: Search relevant verses
        EFA->>IKS: Search relevant hadiths
        EFA->>IKS: Search scholarly opinions
    end
    
    VES-->>EFA: Quran verses
    IKS-->>EFA: Hadith evidences
    IKS-->>EFA: Scholarly references
    
    EFA->>PHI: Generate comprehensive response
    PHI-->>EFA: AI-enhanced response
    
    EFA->>EFS: Format final response
    EFS->>EFC: EnhancedFiqhResponse
    EFC->>W: JSON with structured answer
    W->>U: Display comprehensive Fiqh guidance
    
    Note over U,DB: Response includes: Main ruling, evidence,<br/>school differences, practical guidance
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
      Enhanced Fiqh UI ⭐
      HTML/CSS/JavaScript
      Bootstrap
      Chart.js
    AI/ML
      Ollama
      Phi-3 Integration ⭐
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
        AI_CACHE[AI Response Caching ⭐]
        VECTOR_OPT[Vector Search Optimization ⭐]
    end

    subgraph "Améliorations Futures"
        REDIS[Redis Caching]
        CDN[Content Delivery Network]
        MICROSERVICES[Microservices]
        KUBERNETES[Kubernetes Orchestration]
        GPU_ACCEL[GPU Acceleration]
        EDGE_AI[Edge AI Processing]
    end

    subgraph "Métriques de Performance"
        RESPONSE[Response Time < 200ms]
        THROUGHPUT[1000+ req/sec]
        AVAILABILITY[99.9% Uptime]
        ACCURACY[98%+ Fiqh Accuracy ⭐]
        AI_LATENCY[AI Response < 3s ⭐]
    end

    CACHE --> REDIS
    INDEX --> CDN
    PARALLEL --> MICROSERVICES
    COMPRESS --> KUBERNETES
    AI_CACHE --> GPU_ACCEL
    VECTOR_OPT --> EDGE_AI

    %% Styling
    classDef current fill:#4caf50
    classDef future fill:#ff9800
    classDef metrics fill:#2196f3
    
    class CACHE,INDEX,PARALLEL,COMPRESS,AI_CACHE,VECTOR_OPT current
    class REDIS,CDN,MICROSERVICES,KUBERNETES,GPU_ACCEL,EDGE_AI future
    class RESPONSE,THROUGHPUT,AVAILABILITY,ACCURACY,AI_LATENCY metrics
```

## 🌟 Enhanced Fiqh Advisor - Architecture Hybride Détaillée ⭐

### Vue d'ensemble du système Enhanced Fiqh Advisor Hybride

L'Enhanced Fiqh Advisor représente désormais le système le plus avancé de consultation jurisprudentielle islamique intégré dans Muwasala, combinant une approche hybride BD+IA avec une vaste base de connaissances islamiques pour une couverture optimale.

```mermaid
graph TB
    subgraph "Interface Utilisateur Avancée"
        UI[Enhanced Fiqh Page<br/>Blazor UI + Real-time]
        TEMPLATES[Question Templates<br/>Prayer, Fasting, Marriage, Business]
        CHAT[Interactive Chat Interface<br/>Multi-turn Conversations]
        FILTERS[Advanced Filters<br/>Madhab, Category, Complexity]
    end

    subgraph "Couche de Traitement Hybride ⭐"
        CONTROLLER[Enhanced Fiqh Controller<br/>API Endpoint]
        HYBRID_SERVICE[HybridFiqhService ⭐<br/>Intelligent BD+AI Logic]
        DECISION_ENGINE[SearchDecision Engine<br/>Quality Assessment]
        SESSION_MGR[Session Management<br/>User Context Tracking]
    end

    subgraph "Intelligence Artificielle Intégrée"
        PHI3[Phi-3 Model 3.8B<br/>Islamic Jurisprudence Fine-tuned]
        ANALYSIS[Question Analysis<br/>Category + Complexity Detection]
        GENERATION[Response Generation<br/>Evidence-based Rulings]
        ENHANCEMENT[DB Enhancement<br/>AI Context Enrichment]
    end

    subgraph "Base de Connaissances Hybride ⭐"
        SQLITE_FIQH[(SQLite FiqhRulings<br/>Authentic Database)]
        ENHANCED_CACHE[Enhanced Rulings Cache<br/>Pre-computed Expert Answers]
        HARDCODED_EXPERT[Expert Rulings<br/>~20 Validated Cases]
        QURAN_REF[Quran References<br/>Verse Integration]
        HADITH_REF[Hadith References<br/>Authentic Collections]
    end

    subgraph "Système de Cache Intelligent ⭐"
        SEARCH_CACHE[Search Results Cache<br/>Query Optimization]
        RESPONSE_CACHE[Response Cache<br/>Performance Boost]
        COMPARISON_CACHE[Madhab Comparison Cache<br/>Cross-school Analysis]
        SESSION_CACHE[Session State Cache<br/>User Journey Tracking]
    end

    subgraph "Services de Support Avancés"
        VECTOR[Vector Similarity<br/>Semantic Search Enhancement]
        EMBEDDING[Text Embedding<br/>Multilingual Support]
        QUALITY_CHECKER[Quality Assessment<br/>Response Validation]
        CONFIDENCE_SCORER[Confidence Scoring<br/>Reliability Metrics]
    end

    UI --> CONTROLLER
    TEMPLATES --> UI
    CHAT --> UI
    FILTERS --> UI
    
    CONTROLLER --> HYBRID_SERVICE
    HYBRID_SERVICE --> DECISION_ENGINE
    HYBRID_SERVICE --> SESSION_MGR
    
    DECISION_ENGINE --> SQLITE_FIQH
    DECISION_ENGINE --> ENHANCED_CACHE
    DECISION_ENGINE --> PHI3
    
    HYBRID_SERVICE --> ANALYSIS
    HYBRID_SERVICE --> GENERATION
    HYBRID_SERVICE --> ENHANCEMENT
    
    SQLITE_FIQH --> HARDCODED_EXPERT
    ENHANCED_CACHE --> QURAN_REF
    ENHANCED_CACHE --> HADITH_REF
    
    HYBRID_SERVICE --> SEARCH_CACHE
    HYBRID_SERVICE --> RESPONSE_CACHE
    HYBRID_SERVICE --> COMPARISON_CACHE
    SESSION_MGR --> SESSION_CACHE
    
    HYBRID_SERVICE --> VECTOR
    HYBRID_SERVICE --> EMBEDDING
    GENERATION --> QUALITY_CHECKER
    ENHANCEMENT --> CONFIDENCE_SCORER

    %% Styling
    classDef ui fill:#e3f2fd
    classDef hybrid fill:#fff3e0
    classDef ai fill:#f3e5f5
    classDef knowledge fill:#e8f5e8
    classDef cache fill:#fce4ec
    classDef support fill:#e0f2f1
    
    class UI,TEMPLATES,CHAT,FILTERS ui
    class CONTROLLER,HYBRID_SERVICE,DECISION_ENGINE,SESSION_MGR hybrid
    class PHI3,ANALYSIS,GENERATION,ENHANCEMENT ai
    class SQLITE_FIQH,ENHANCED_CACHE,HARDCODED_EXPERT,QURAN_REF,HADITH_REF knowledge
    class SEARCH_CACHE,RESPONSE_CACHE,COMPARISON_CACHE,SESSION_CACHE cache
    class VECTOR,EMBEDDING,QUALITY_CHECKER,CONFIDENCE_SCORER support
```

### Architecture des Composants Enhanced Fiqh

```mermaid
classDiagram
    class EnhancedFiqhController {
        +AskQuestionAsync(request)
        +GetQuestionTemplates()
        +GetFiqhHistory()
    }

    class EnhancedFiqhService {
        +ProcessQuestionAsync(question)
        +GenerateResponseAsync(context)
        +ValidateResponse(response)
        -_agent: EnhancedFiqhAgent
        -_cache: IMemoryCache
    }

    class EnhancedFiqhAgent {
        +AnalyzeQuestionAsync(question)
        +SearchEvidenceAsync(context)
        +GenerateRulingAsync(evidence)
        +FormatResponseAsync(ruling)
        -_phi3Service: IPhi3Service
        -_knowledgeService: IIslamicKnowledgeService
    }

    class EnhancedFiqhRequest {
        +Question: string
        +Context: string
        +School: FiqhSchool
        +Language: string
    }

    class EnhancedFiqhResponse {
        +MainRuling: string
        +Evidence: List~FiqhEvidence~
        +SchoolDifferences: List~SchoolRuling~
        +PracticalGuidance: string
        +Confidence: double
        +Sources: List~string~
    }

    class FiqhEvidence {
        +Type: EvidenceType
        +Text: string
        +Source: string
        +Reference: string
        +Relevance: double
    }

    class SchoolRuling {
        +School: FiqhSchool
        +Ruling: string
        +Evidence: string
        +Scholars: List~string~
    }

    EnhancedFiqhController --> EnhancedFiqhService
    EnhancedFiqhService --> EnhancedFiqhAgent
    EnhancedFiqhController ..> EnhancedFiqhRequest
    EnhancedFiqhController ..> EnhancedFiqhResponse
    EnhancedFiqhResponse --> FiqhEvidence
    EnhancedFiqhResponse --> SchoolRuling
```

### Flux de Données Enhanced Fiqh

```mermaid
flowchart TD
    START([Utilisateur pose une question])
    
    RECEIVE[Réception de la question<br/>Enhanced Fiqh Controller]
    VALIDATE{Validation de<br/>la requête}
    
    ANALYZE[Analyse de la question<br/>Enhanced Fiqh Agent]
    CONTEXT[Extraction du contexte<br/>Phi-3 AI Analysis]
    
    SEARCH_PARALLEL{Recherche parallèle<br/>de preuves}
    SEARCH_QURAN[Recherche dans<br/>le Coran]
    SEARCH_HADITH[Recherche dans<br/>les Hadiths]
    SEARCH_FIQH[Recherche dans<br/>les règles de Fiqh]
    
    GENERATE[Génération de la réponse<br/>Phi-3 AI Generation]
    FORMAT[Formatage de la réponse<br/>Structure JSON]
    
    CACHE[Mise en cache<br/>Pour performances]
    RESPOND[Envoi de la réponse<br/>à l'utilisateur]
    
    ERROR[Gestion d'erreur<br/>Réponse de secours]
    
    START --> RECEIVE
    RECEIVE --> VALIDATE
    VALIDATE -->|Valide| ANALYZE
    VALIDATE -->|Invalide| ERROR
    
    ANALYZE --> CONTEXT
    CONTEXT --> SEARCH_PARALLEL
    
    SEARCH_PARALLEL --> SEARCH_QURAN
    SEARCH_PARALLEL --> SEARCH_HADITH
    SEARCH_PARALLEL --> SEARCH_FIQH
    
    SEARCH_QURAN --> GENERATE
    SEARCH_HADITH --> GENERATE
    SEARCH_FIQH --> GENERATE
    
    GENERATE --> FORMAT
    FORMAT --> CACHE
    CACHE --> RESPOND
    
    ERROR --> RESPOND

    %% Styling
    classDef startEnd fill:#4caf50,color:#fff
    classDef process fill:#2196f3,color:#fff
    classDef decision fill:#ff9800,color:#fff
    classDef search fill:#9c27b0,color:#fff
    classDef ai fill:#e91e63,color:#fff
    classDef error fill:#f44336,color:#fff
    
    class START,RESPOND startEnd
    class RECEIVE,ANALYZE,FORMAT,CACHE process
    class VALIDATE,SEARCH_PARALLEL decision
    class SEARCH_QURAN,SEARCH_HADITH,SEARCH_FIQH search
    class CONTEXT,GENERATE ai
    class ERROR error
```

### Intégration AI/ML dans Enhanced Fiqh

```mermaid
graph LR
    subgraph "Modèles AI Utilisés"
        PHI3[Phi-3 Mini<br/>3.8B Parameters<br/>Multilingual]
        EMBEDDING[Text Embedding<br/>Sentence Transformers<br/>Vector Similarity]
        NER[Named Entity Recognition<br/>Islamic Terms<br/>Context Extraction]
    end

    subgraph "Tâches AI"
        QA[Question Analysis<br/>Intent Recognition<br/>Context Extraction]
        ES[Evidence Search<br/>Semantic Matching<br/>Relevance Scoring]
        RG[Response Generation<br/>Structured Output<br/>Multi-lingual Support]
        QC[Quality Control<br/>Fact Checking<br/>Consistency Validation]
    end

    subgraph "Optimisations"
        CACHE[Model Caching<br/>Response Memoization]
        BATCH[Batch Processing<br/>Parallel Inference]
        QUANT[Model Quantization<br/>Memory Optimization]
    end

    PHI3 --> QA
    PHI3 --> RG
    PHI3 --> QC
    
    EMBEDDING --> ES
    NER --> QA
    
    QA --> CACHE
    ES --> BATCH
    RG --> QUANT

    %% Styling
    classDef models fill:#e3f2fd
    classDef tasks fill:#f3e5f5
    classDef optimizations fill:#e8f5e8
    
    class PHI3,EMBEDDING,NER models
    class QA,ES,RG,QC tasks
    class CACHE,BATCH,QUANT optimizations
```

### Métriques et Monitoring Enhanced Fiqh

```mermaid
graph TB
    subgraph "Métriques de Performance"
        RESPONSE_TIME[Temps de Réponse<br/>Objectif: < 3 secondes]
        ACCURACY[Précision des Réponses<br/>Objectif: > 95%]
        COVERAGE[Couverture des Sujets<br/>Objectif: 90% domaines fiqh]
        USER_SATISFACTION[Satisfaction Utilisateur<br/>Objectif: > 4.5/5]
    end

    subgraph "Métriques Techniques"
        AI_LATENCY[Latence IA<br/>Phi-3 Processing Time]
        SEARCH_EFFICIENCY[Efficacité Recherche<br/>Vector Search Performance]
        CACHE_HIT_RATE[Taux de Cache Hit<br/>Response Caching]
        ERROR_RATE[Taux d'Erreur<br/>System Reliability]
    end

    subgraph "Monitoring Tools"
        LOGS[Application Logs<br/>Structured Logging]
        METRICS[Metrics Collection<br/>Prometheus/Grafana]
        ALERTS[Alert System<br/>Automated Notifications]
        DASHBOARD[Monitoring Dashboard<br/>Real-time Insights]
    end

    RESPONSE_TIME --> LOGS
    ACCURACY --> METRICS
    COVERAGE --> DASHBOARD
    USER_SATISFACTION --> ALERTS
    
    AI_LATENCY --> LOGS
    SEARCH_EFFICIENCY --> METRICS
    CACHE_HIT_RATE --> DASHBOARD
    ERROR_RATE --> ALERTS

    %% Styling
    classDef performance fill:#4caf50
    classDef technical fill:#2196f3
    classDef monitoring fill:#ff9800
    
    class RESPONSE_TIME,ACCURACY,COVERAGE,USER_SATISFACTION performance
    class AI_LATENCY,SEARCH_EFFICIENCY,CACHE_HIT_RATE,ERROR_RATE technical
    class LOGS,METRICS,ALERTS,DASHBOARD monitoring
```

### Roadmap Enhanced Fiqh

```mermaid
gantt
    title Enhanced Fiqh Advisor Development Roadmap
    dateFormat  YYYY-MM-DD
    section Phase 1 ✅
    Core Implementation     :done, phase1, 2025-01-01, 2025-02-01
    Basic UI Integration    :done, ui1, 2025-02-01, 2025-02-15
    AI Integration          :done, ai1, 2025-02-15, 2025-03-01
    section Phase 2 🚧
    Advanced Templates      :active, templates, 2025-03-01, 2025-03-15
    Multi-language Support :multi-lang, 2025-03-15, 2025-04-01
    Enhanced Caching        :caching, 2025-04-01, 2025-04-15
    section Phase 3 📋
    Voice Interface         :voice, 2025-04-15, 2025-05-01
    Mobile Optimization     :mobile, 2025-05-01, 2025-05-15
    Offline Mode            :offline, 2025-05-15, 2025-06-01
    section Phase 4 🔮
    Personalization         :personal, 2025-06-01, 2025-06-15
    Community Features      :community, 2025-06-15, 2025-07-01
    Advanced Analytics      :analytics, 2025-07-01, 2025-07-15
```
