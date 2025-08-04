# Dua Companion - Diagrammes d'Architecture

## 🏗️ Architecture Système Complète

```mermaid
graph TB
    subgraph "Frontend - Blazor UI"
        UI[Pages/Dua.razor]
        MODEL[DuaModel]
        SELECTOR[Mode Selector<br/>Fast/Enhanced]
    end
    
    subgraph "Backend - Agent Layer"
        DC[DuaCompanionAgent]
        LOGIC[Business Logic]
        FALLBACK[Fallback Handler]
    end
    
    subgraph "Data Layer"
        DS[DuaService]
        DB[(SQLite Database<br/>DuaRecords)]
        CACHE[Cache Service]
    end
    
    subgraph "AI Layer"
        OS[OllamaService]
        MODEL_AI[DeepSeek-R1<br/>15min timeout]
        OLLAMA[Ollama Server<br/>localhost:11434]
    end
    
    subgraph "Performance Modes"
        FAST[🚀 Mode Rapide<br/>~2 seconds<br/>No AI calls]
        ENHANCED[🧠 Mode Amélioré<br/>~15 seconds<br/>Parallel AI processing]
    end
    
    UI --> DC
    MODEL --> UI
    SELECTOR --> MODEL
    
    DC --> LOGIC
    DC --> FALLBACK
    LOGIC --> DS
    LOGIC --> OS
    
    DS --> DB
    DS --> CACHE
    
    OS --> MODEL_AI
    MODEL_AI --> OLLAMA
    
    DC --> FAST
    DC --> ENHANCED
    
    FALLBACK -.->|"Timeout/Error"| FAST
```

## 🔄 Flux de Données - Mode Rapide

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant UI as Dua.razor
    participant DC as DuaCompanionAgent
    participant DS as DuaService
    participant DB as SQLite DB
    participant C as Cache
    
    U->>UI: Sélectionne "Mode Rapide"
    U->>UI: Saisit "morning prayers"
    UI->>DC: GetDuasForOccasionAsync("morning", fastMode=true)
    
    Note over DC: Mode Rapide activé - Pas d'IA
    
    DC->>DS: SearchDuasByOccasionAsync("morning")
    DS->>C: Check cache first
    
    alt Cache Miss
        DS->>DB: SELECT * FROM DuaRecords WHERE Occasion LIKE '%morning%'
        DB-->>DS: [DuaRecord1, DuaRecord2, DuaRecord3]
        DS->>C: Store in cache
    else Cache Hit
        C-->>DS: Cached results
    end
    
    DS-->>DC: List<DuaRecord>
    
    Note over DC: Transform to DuaResponse without AI enhancement
    
    DC-->>UI: List<DuaResponse> (~2 seconds)
    UI-->>U: Affichage des du'as authentiques
```

## 🧠 Flux de Données - Mode Amélioré

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant UI as Dua.razor
    participant DC as DuaCompanionAgent
    participant DS as DuaService
    participant OS as OllamaService
    participant AI as DeepSeek-R1
    participant DB as SQLite DB
    
    U->>UI: Sélectionne "Mode Amélioré"
    U->>UI: Saisit "evening supplications"
    UI->>DC: GetDuasForOccasionAsync("evening", fastMode=false)
    
    DC->>DS: SearchDuasByOccasionAsync("evening")
    DS->>DB: SELECT * FROM DuaRecords
    DB-->>DS: [DuaRecord1, DuaRecord2, DuaRecord3]
    DS-->>DC: List<DuaRecord>
    
    Note over DC: Mode Amélioré - Traitement parallèle IA
    
    par Enhancement Task 1
        DC->>OS: GenerateStructuredResponseAsync(dua1)
        OS->>AI: Scholarly explanation prompt
        AI-->>OS: Enhanced explanation
        OS-->>DC: DuaEnhancement1
    and Enhancement Task 2
        DC->>OS: GenerateStructuredResponseAsync(dua2)
        OS->>AI: Scholarly explanation prompt
        AI-->>OS: Enhanced explanation
        OS-->>DC: DuaEnhancement2
    and Enhancement Task 3
        DC->>OS: GenerateStructuredResponseAsync(dua3)
        OS->>AI: Scholarly explanation prompt
        AI-->>OS: Enhanced explanation
        OS-->>DC: DuaEnhancement3
    end
    
    Note over DC: Combine DuaRecords + AI Enhancements
    
    DC-->>UI: List<DuaResponse> with AI enhancements (~15 seconds)
    UI-->>U: Affichage des du'as enrichies
```

## ⚠️ Gestion d'Erreurs et Fallback

```mermaid
flowchart TD
    START[Requête Utilisateur] --> MODE{Mode Sélectionné?}
    
    MODE -->|Fast| FAST_PATH[Mode Rapide Direct]
    MODE -->|Enhanced| ENHANCED_PATH[Mode Amélioré]
    
    FAST_PATH --> DB_QUERY[Query Base de Données]
    DB_QUERY --> FAST_RESULT[Retour Rapide ~2s]
    
    ENHANCED_PATH --> AI_CALLS[Appels IA Parallèles]
    AI_CALLS --> TIMEOUT_CHECK{Timeout?}
    
    TIMEOUT_CHECK -->|Non| AI_SUCCESS[Succès IA]
    TIMEOUT_CHECK -->|Oui| TIMEOUT_HANDLER[Timeout Handler]
    
    AI_SUCCESS --> ENHANCED_RESULT[Résultat Enrichi ~15s]
    
    TIMEOUT_HANDLER --> LOG_WARNING[Log Warning]
    LOG_WARNING --> FALLBACK_FAST[Fallback Mode Rapide]
    FALLBACK_FAST --> FAST_RESULT
    
    AI_CALLS --> ERROR_CHECK{Erreur IA?}
    ERROR_CHECK -->|Oui| ERROR_HANDLER[Error Handler]
    ERROR_CHECK -->|Non| AI_SUCCESS
    
    ERROR_HANDLER --> LOG_ERROR[Log Error]
    LOG_ERROR --> FALLBACK_FAST
    
    FAST_RESULT --> END[Réponse Utilisateur]
    ENHANCED_RESULT --> END
```

## 🔧 Architecture des Composants

```mermaid
classDiagram
    class DuaCompanionAgent {
        -IOllamaService _ollama
        -IDuaService _duaService
        -ILogger _logger
        +GetDuasForOccasionAsync(occasion, language, maxResults, fastMode)
        +GetSpecificPrayerAsync(prayerType, language)
        +GetDailyDuaScheduleAsync(language)
        -GetAlternativeDuasAsync(occasion, language, fastMode)
        -BuildDuaExplanationPrompt(dua, occasion, language)
    }
    
    class DuaService {
        -IslamicKnowledgeContext _context
        +SearchDuasByOccasionAsync(occasion, language, maxResults)
        +GetSpecificPrayerAsync(prayerType, language)
        +GetDuasByKeywordsAsync(keywords, language)
    }
    
    class OllamaService {
        -HttpClient _httpClient
        -string _baseUrl
        +GenerateResponseAsync(model, prompt, temperature)
        +GenerateStructuredResponseAsync(model, prompt, temperature)
    }
    
    class DuaResponse {
        +string ArabicText
        +string Translation
        +string Transliteration
        +string Occasion
        +string Source
        +string Benefits
        +List~string~ RelatedDuas
        +List~string~ Sources
    }
    
    class DuaModel {
        +string Language
        +string ResponseMode
    }
    
    class DuaEnhancement {
        +string Benefits
        +string BestTime
        +List~string~ RelatedDuas
        +string Context
    }
    
    DuaCompanionAgent --> DuaService
    DuaCompanionAgent --> OllamaService
    DuaCompanionAgent --> DuaResponse
    DuaService --> DuaResponse
    OllamaService --> DuaEnhancement
    DuaModel --> DuaCompanionAgent
```

## 📊 Métriques de Performance

```mermaid
gantt
    title Timeline de Performance - Dua Companion
    dateFormat X
    axisFormat %s
    
    section Mode Rapide
    Query DB           :active, fast-db, 0, 1s
    Transform Data     :active, fast-transform, 1s, 2s
    
    section Mode Amélioré (Avant)
    Query DB           :done, old-db, 0, 1s
    AI Call 1          :done, old-ai1, 1s, 41s
    AI Call 2          :done, old-ai2, 41s, 81s
    AI Call 3          :done, old-ai3, 81s, 121s
    
    section Mode Amélioré (Optimisé)
    Query DB           :active, new-db, 0, 1s
    Parallel AI Call 1 :active, new-ai1, 1s, 15s
    Parallel AI Call 2 :active, new-ai2, 1s, 15s
    Parallel AI Call 3 :active, new-ai3, 1s, 15s
```

## 🔄 États du Système

```mermaid
stateDiagram-v2
    [*] --> Idle : Application Start
    
    Idle --> Processing : User Request
    
    Processing --> FastMode : Fast Mode Selected
    Processing --> EnhancedMode : Enhanced Mode Selected
    
    FastMode --> DatabaseQuery : Query Local DB
    DatabaseQuery --> FastResponse : Return Results
    FastResponse --> Idle : Complete (~2s)
    
    EnhancedMode --> DatabaseQuery2 : Query Local DB
    DatabaseQuery2 --> AIProcessing : Send to AI
    
    AIProcessing --> AISuccess : All AI Calls Complete
    AIProcessing --> AITimeout : Timeout Occurred
    AIProcessing --> AIError : Error Occurred
    
    AISuccess --> EnhancedResponse : Return Enhanced Results
    EnhancedResponse --> Idle : Complete (~15s)
    
    AITimeout --> FallbackMode : Auto Fallback
    AIError --> FallbackMode : Auto Fallback
    FallbackMode --> FastResponse : Return Basic Results
    
    note right of AITimeout : Logs warning and falls back
    note right of AIError : Logs error and falls back
```

---

*Dernière mise à jour : Août 2025*  
*Version : 2.0.0 - Feature Branch: feature/dua-companion*
