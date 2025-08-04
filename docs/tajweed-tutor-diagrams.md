# Tajweed Tutor - Diagrammes d'Architecture

## 🏗️ Architecture Système Complète

```mermaid
graph TB
    subgraph "Frontend - Blazor UI"
        UI[Pages/Tajweed.razor]
        MODEL[TajweedModel]
        SELECTOR[Mode Selector<br/>Fast/Enhanced]
        VISUAL[Visualisation Interactive<br/>Codes Couleur]
    end
    
    subgraph "Backend - Agent Layer"
        TT[TajweedTutorAgent]
        LOGIC[Business Logic]
        FALLBACK[Fallback Handler]
        CACHE[Cache Manager]
    end
    
    subgraph "Data Layer"
        TS[TajweedService]
        DB[(SQLite Database<br/>TajweedRules)]
        RULES[Rules Repository]
    end
    
    subgraph "AI Layer"
        OS[OllamaService]
        MODEL_AI[DeepSeek-R1<br/>20min timeout]
        OLLAMA[Ollama Server<br/>localhost:11434]
    end
    
    subgraph "Performance Modes"
        FAST[🚀 Mode Rapide<br/>~2-3 seconds<br/>Rules from DB]
        ENHANCED[🧠 Mode Amélioré<br/>~15-20 seconds<br/>Parallel AI analysis]
    end
    
    subgraph "Learning Types"
        ANALYSIS[📖 Analyse de Verset]
        PRONUNCIATION[🗣️ Guide Prononciation]
        LESSON[🎓 Leçon Interactive]
        MISTAKES[⚠️ Erreurs Communes]
    end
    
    UI --> TT
    MODEL --> UI
    SELECTOR --> MODEL
    VISUAL --> UI
    
    TT --> LOGIC
    TT --> FALLBACK
    TT --> CACHE
    LOGIC --> TS
    LOGIC --> OS
    
    TS --> DB
    TS --> RULES
    
    OS --> MODEL_AI
    MODEL_AI --> OLLAMA
    
    TT --> FAST
    TT --> ENHANCED
    
    TT --> ANALYSIS
    TT --> PRONUNCIATION
    TT --> LESSON
    TT --> MISTAKES
    
    FALLBACK -.->|"Timeout/Error"| FAST
```

## 🔄 Flux de Données - Mode Rapide

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant UI as Tajweed.razor
    participant TT as TajweedTutorAgent
    participant TS as TajweedService
    participant DB as SQLite DB
    participant C as Cache
    
    U->>UI: Sélectionne "Mode Rapide"
    U->>UI: Saisit Sourate 1, Verset 1
    UI->>TT: AnalyzeVerseAsync(1:1, fastMode=true)
    
    Note over TT: Mode Rapide activé - Pas d'IA
    
    TT->>C: Check cache first
    
    alt Cache Miss
        TT->>TS: GetVerseWithTajweedAsync(1:1)
        TS->>DB: SELECT * FROM TajweedRules WHERE surah=1 AND ayah=1
        DB-->>TS: [Rule1: Nun Sakinah, Rule2: Qalqalah, Rule3: Madd]
        TS-->>TT: List<TajweedRule>
        TT->>C: Store in cache
    else Cache Hit
        C-->>TT: Cached rules
    end
    
    Note over TT: Transform to TajweedResponse without AI enhancement
    
    TT-->>UI: TajweedResponse with basic rules (~2-3 seconds)
    UI-->>U: Affichage des règles avec codes couleur
```

## 🧠 Flux de Données - Mode Amélioré

```mermaid
sequenceDiagram
    participant U as Utilisateur
    participant UI as Tajweed.razor
    participant TT as TajweedTutorAgent
    participant TS as TajweedService
    participant OS as OllamaService
    participant AI as DeepSeek-R1
    participant DB as SQLite DB
    
    U->>UI: Sélectionne "Mode Amélioré"
    U->>UI: Saisit Sourate 112, Verset 1
    UI->>TT: AnalyzeVerseAsync(112:1, fastMode=false)
    
    TT->>TS: GetVerseWithTajweedAsync(112:1)
    TS->>DB: SELECT * FROM TajweedRules
    DB-->>TS: [Rule1, Rule2, Rule3]
    TS-->>TT: Basic TajweedRules
    
    Note over TT: Mode Amélioré - Analyse parallèle IA
    
    par Enhanced Analysis Task 1
        TT->>OS: AnalyzeTajweedRule(Rule1: Nun Sakinah)
        OS->>AI: Detailed Nun Sakinah analysis prompt
        AI-->>OS: Enhanced explanation with examples
        OS-->>TT: RuleAnalysis1
    and Enhanced Analysis Task 2
        TT->>OS: AnalyzeTajweedRule(Rule2: Qalqalah)
        OS->>AI: Detailed Qalqalah analysis prompt
        AI-->>OS: Enhanced explanation with examples
        OS-->>TT: RuleAnalysis2
    and Pronunciation Guide Task
        TT->>OS: GeneratePronunciationGuide("الله")
        OS->>AI: Pronunciation guide prompt
        AI-->>OS: Step-by-step pronunciation
        OS-->>TT: PronunciationGuide
    end
    
    Note over TT: Combine Rules + AI Enhancements
    
    TT-->>UI: TajweedResponse with AI enhancements (~15-20 seconds)
    UI-->>U: Affichage enrichi avec explications détaillées
```

## ⚠️ Gestion d'Erreurs et Fallback

```mermaid
flowchart TD
    START[Requête Utilisateur] --> MODE{Mode Sélectionné?}
    
    MODE -->|Fast| FAST_PATH[Mode Rapide Direct]
    MODE -->|Enhanced| ENHANCED_PATH[Mode Amélioré]
    
    FAST_PATH --> CACHE_CHECK{Cache Available?}
    CACHE_CHECK -->|Oui| CACHE_HIT[Retour Cache]
    CACHE_CHECK -->|Non| DB_QUERY[Query Base de Données]
    
    DB_QUERY --> FAST_RESULT[Retour Rapide ~2-3s]
    CACHE_HIT --> FAST_RESULT
    
    ENHANCED_PATH --> AI_CALLS[Appels IA Parallèles]
    AI_CALLS --> TIMEOUT_CHECK{Timeout?}
    
    TIMEOUT_CHECK -->|Non| AI_SUCCESS[Succès IA]
    TIMEOUT_CHECK -->|Oui| TIMEOUT_HANDLER[Timeout Handler]
    
    AI_SUCCESS --> ENHANCED_RESULT[Résultat Enrichi ~15-20s]
    
    TIMEOUT_HANDLER --> LOG_WARNING[Log Warning]
    LOG_WARNING --> FALLBACK_FAST[Fallback Mode Rapide]
    FALLBACK_FAST --> FAST_RESULT
    
    AI_CALLS --> ERROR_CHECK{Erreur IA?}
    ERROR_CHECK -->|Oui| ERROR_HANDLER[Error Handler]
    ERROR_CHECK -->|Non| AI_SUCCESS
    
    ERROR_HANDLER --> LOG_ERROR[Log Error]
    LOG_ERROR --> FALLBACK_FAST
    
    FAST_RESULT --> VISUALIZE[Visualisation Interactive]
    ENHANCED_RESULT --> VISUALIZE
    VISUALIZE --> END[Réponse Utilisateur]
```

## 🔧 Architecture des Composants

```mermaid
classDiagram
    class TajweedTutorAgent {
        -IOllamaService _ollama
        -ITajweedService _tajweedService
        -ILogger _logger
        -ICacheService _cache
        +AnalyzeVerseAsync(verse, language, fastMode)
        +GetPronunciationGuideAsync(word, language, fastMode)
        +CreateRecitationLessonAsync(surah, level, fastMode)
        +GetCommonMistakesAsync(type, language)
        -BuildTajweedAnalysisPrompt(verse, language)
        -BuildPronunciationPrompt(word, language)
    }
    
    class TajweedService {
        -IslamicKnowledgeContext _context
        +GetVerseWithTajweedAsync(verse)
        +GetTajweedRulesByTypeAsync(ruleType)
        +GetRecitationLessonAsync(surah, level)
        +SearchTajweedRulesAsync(query)
    }
    
    class OllamaService {
        -HttpClient _httpClient
        -string _baseUrl
        +GenerateResponseAsync(model, prompt, temperature)
        +GenerateStructuredResponseAsync(model, prompt, temperature)
        +AnalyzeTajweedRuleAsync(rule, context)
    }
    
    class TajweedResponse {
        +string VerseText
        +List~TajweedRule~ Rules
        +string AudioExample
        +string PronunciationGuide
        +List~string~ Sources
        +string DetailedAnalysis
    }
    
    class TajweedRule {
        +string Name
        +string Category
        +string Description
        +int StartPosition
        +int EndPosition
        +string AffectedText
        +string ColorCode
        +List~string~ Examples
        +string AudioUrl
    }
    
    class TajweedModel {
        +string LearningType
        +string ResponseMode
        +int Surah
        +int Ayah
        +string Language
        +RecitationLevel Level
    }
    
    class PronunciationGuide {
        +string Word
        +List~LetterGuide~ Letters
        +string PhoneticTranscription
        +string AudioExample
        +List~string~ CommonMistakes
    }
    
    TajweedTutorAgent --> TajweedService
    TajweedTutorAgent --> OllamaService
    TajweedTutorAgent --> TajweedResponse
    TajweedService --> TajweedResponse
    OllamaService --> PronunciationGuide
    TajweedModel --> TajweedTutorAgent
    TajweedResponse --> TajweedRule
```

## 📊 Visualisation des Règles de Tajweed

```mermaid
graph LR
    subgraph "Codes Couleur Standards"
        QALQALAH[🔴 Qalqalah<br/>Rouge]
        IDGHAM[🟢 Idgham avec Ghunnah<br/>Vert]
        IKHFA[🔵 Ikhfa<br/>Bleu]
        MADD[🟡 Madd<br/>Jaune]
        IQLAB[🟠 Iqlab<br/>Orange]
        TAFKHEEM[🟣 Tafkheem<br/>Violet]
    end
    
    subgraph "Rendu Visuel"
        VERSE[بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ]
        COLORED[Texte avec codes couleur appliqués]
        INTERACTIVE[Hover pour détails des règles]
    end
    
    QALQALAH --> COLORED
    IDGHAM --> COLORED
    IKHFA --> COLORED
    MADD --> COLORED
    IQLAB --> COLORED
    TAFKHEEM --> COLORED
    
    VERSE --> COLORED
    COLORED --> INTERACTIVE
```

## 📈 Métriques de Performance

```mermaid
gantt
    title Timeline de Performance - Tajweed Tutor
    dateFormat X
    axisFormat %s
    
    section Mode Rapide
    Cache Check        :active, fast-cache, 0, 0.5s
    DB Query           :active, fast-db, 0.5s, 2s
    Rule Processing    :active, fast-process, 2s, 3s
    
    section Mode Amélioré (Actuel)
    DB Query           :done, old-db, 0, 2s
    AI Analysis 1      :done, old-ai1, 2s, 62s
    AI Analysis 2      :done, old-ai2, 62s, 122s
    AI Pronunciation   :done, old-ai3, 122s, 182s
    
    section Mode Amélioré (Optimisé)
    DB Query           :active, new-db, 0, 2s
    Parallel AI 1      :active, new-ai1, 2s, 20s
    Parallel AI 2      :active, new-ai2, 2s, 20s
    Parallel AI 3      :active, new-ai3, 2s, 20s
    Final Processing   :active, new-final, 20s, 22s
```

## 🔄 États du Système

```mermaid
stateDiagram-v2
    [*] --> Idle : Application Start
    
    Idle --> Processing : User Request
    
    Processing --> FastMode : Fast Mode Selected
    Processing --> EnhancedMode : Enhanced Mode Selected
    
    FastMode --> CacheCheck : Check Cache
    CacheCheck --> CacheHit : Cache Available
    CacheCheck --> DatabaseQuery : Cache Miss
    
    CacheHit --> FastResponse : Return Cached Rules
    DatabaseQuery --> FastResponse : Return DB Rules
    FastResponse --> Visualize : Apply Color Coding
    
    EnhancedMode --> DatabaseQuery2 : Get Base Rules
    DatabaseQuery2 --> AIProcessing : Send to AI
    
    AIProcessing --> AISuccess : All AI Calls Complete
    AIProcessing --> AITimeout : Timeout Occurred
    AIProcessing --> AIError : Error Occurred
    
    AISuccess --> EnhancedResponse : Return Enhanced Analysis
    EnhancedResponse --> Visualize : Apply Advanced Visualization
    
    AITimeout --> FallbackMode : Auto Fallback
    AIError --> FallbackMode : Auto Fallback
    FallbackMode --> FastResponse : Return Basic Rules
    
    Visualize --> Interactive : Add Hover Effects
    Interactive --> Idle : Complete
    
    note right of AITimeout : Logs warning and falls back
    note right of AIError : Logs error and falls back
    note right of Interactive : Color-coded rules with hover details
```

## 🎯 Types d'Analyse Supportés

```mermaid
mindmap
  root((Tajweed Tutor))
    Analyse de Verset
      Règles automatiques
      Identification des patterns
      Codes couleur
      Exemples contextuels
    Guide de Prononciation
      Décomposition phonétique
      Guide lettre par lettre
      Erreurs communes
      Audio de référence
    Leçon Interactive
      Progression par niveau
      Exercices pratiques
      Feedback personnalisé
      Suivi des progrès
    Erreurs Communes
      Patterns d'erreurs
      Corrections détaillées
      Exemples comparatifs
      Prévention
```

## 🚀 Architecture Future - Phase 2

```mermaid
graph TB
    subgraph "Audio Integration"
        AUDIO[Audio Engine]
        SPEECH[Speech Recognition]
        TTS[Text-to-Speech]
        COMPARE[Audio Comparison]
    end
    
    subgraph "Advanced AI"
        SPECIALIZED[Specialized Tajweed Model]
        CONTEXTUAL[Contextual Analysis]
        PERSONALIZED[Personalized Learning]
    end
    
    subgraph "Community Features"
        SHARING[Rule Sharing]
        PEER[Peer Review]
        CERTIFICATION[Certification System]
    end
    
    TajweedTutorAgent --> AUDIO
    TajweedTutorAgent --> SPECIALIZED
    TajweedTutorAgent --> SHARING
    
    AUDIO --> SPEECH
    AUDIO --> TTS
    AUDIO --> COMPARE
    
    SPECIALIZED --> CONTEXTUAL
    SPECIALIZED --> PERSONALIZED
    
    SHARING --> PEER
    SHARING --> CERTIFICATION
```

---

*Dernière mise à jour : Août 2025*  
*Version : 1.0.0 - Feature Branch: feature/tajweed-enhancement*
