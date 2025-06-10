# 🕌 Muwasala Islamic Knowledge Network

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-purple.svg)](https://blazor.net/)
[![DeepSeek](https://img.shields.io/badge/AI-DeepSeek-green.svg)](https://deepseek.com/)
[![Ollama](https://img.shields.io/badge/LLM-Ollama-orange.svg)](https://ollama.ai/)

> **Muwasala** (موصلة) means "connection" in Arabic - connecting seekers of knowledge with authentic Islamic wisdom through advanced AI-powered multi-agent systems.

## 🌟 Overview

Muwasala Islamic Knowledge Network is an innovative platform that combines traditional Islamic scholarship with cutting-edge AI technology. It features a sophisticated multi-agent system powered by DeepSeek AI to provide accurate, authenticated, and contextually relevant Islamic knowledge.

### Key Features

- 🧠 **Multi-Agent AI System** - Specialized agents for different Islamic domains
- 🔍 **Intelligent Search** - Real-time search across trusted Islamic sources
- 📖 **Quranic Integration** - Complete Quran with advanced search capabilities
- 📚 **Hadith Verification** - Authentication and classification of hadiths
- ⚖️ **Fiqh Advisory** - Islamic jurisprudence guidance across different madhabs
- 🤲 **Dua Companion** - Comprehensive collection of Islamic supplications
- 🎓 **Tajweed Tutorial** - Quranic recitation guidance
- 📰 **Sirah Scholar** - Prophetic biography and historical context

## 🏗️ Architecture

### Multi-Agent System

The platform implements a sophisticated multi-agent architecture with DeepSeek as the central brain:

```
User Query → DeepSeek Brain → Specialized Agents → Islamic Filter → Synthesized Response
```

#### Specialized Agents

- **WebCrawlerAgent** - Searches trusted Islamic websites
- **HadithVerifierAgent** - Validates hadith authenticity
- **FiqhAdvisorAgent** - Provides jurisprudential guidance
- **QuranNavigatorAgent** - Quranic search and analysis
- **DuaCompanionAgent** - Dua recommendations and explanations
- **SirahScholarAgent** - Prophetic biography insights
- **TajweedTutorAgent** - Quranic recitation guidance

#### Advanced AI Components

- **QueryAnalysisAgent** - Intelligent query classification and routing
- **RealTimeSearchAgent** - Live web search across Islamic sources
- **ResponseQualityAgent** - Quality assurance and validation

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0** or later
- **Visual Studio 2022** or VS Code
- **Ollama** (for local LLM execution)
- **16GB RAM** minimum (recommended for optimal performance)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/muwasala-islamic-knowledge-network.git
   cd muwasala-islamic-knowledge-network
   ```

2. **Install Ollama and DeepSeek model**
   ```bash
   # Install Ollama (Windows)
   winget install Ollama.Ollama
   
   # Pull DeepSeek model
   ollama pull deepseek-coder:1.3b
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the web application**
   ```bash
   cd src/Muwasala.Web
   dotnet run
   ```

6. **Access the application**
   - Open your browser and navigate to `http://localhost:5237`

### Configuration

1. **API Keys** (Optional for enhanced features)
   - DeepSeek API: Add your API key to `appsettings.json`
   - Configure external Islamic API sources

2. **Ollama Service**
   - Ensure Ollama is running: `ollama serve`
   - Verify model availability: `ollama list`

## 📁 Project Structure

```
├── src/
│   ├── Muwasala.Web/           # Blazor Server web application
│   ├── Muwasala.Api/           # REST API controllers
│   ├── Muwasala.Agents/        # Multi-agent system implementation
│   ├── Muwasala.Core/          # Core models and services
│   ├── Muwasala.KnowledgeBase/ # Islamic knowledge base services
│   └── Muwasala.Console/       # Console application for testing
├── tests/
│   └── IntegrationTest/        # Integration tests for multi-agent system
├── docs/
│   └── multi-agent-architecture.md # Detailed architecture documentation
├── data/
│   ├── Quran/                  # Quranic data and translations
│   ├── Hadith/                 # Hadith collections
│   └── Fiqh/                   # Jurisprudential content
└── README.md
```

## 🔧 Usage

### Web Interface

1. **Search Islamic Knowledge**
   - Enter your question in natural language
   - The system will analyze and route to appropriate agents
   - Receive comprehensive answers with authentic sources

2. **Explore Quranic Content**
   - Search verses by keyword, topic, or reference
   - Access multiple translations and interpretations
   - View verse context and related content

3. **Hadith Verification**
   - Submit hadith text for authentication
   - Receive classification (Sahih, Hasan, Da'if)
   - View chain of narration and scholarly opinions

### API Endpoints

The REST API provides programmatic access to all features:

```csharp
// Example API usage
POST /api/islamic-agents/enhanced-search
{
    "query": "What is the importance of prayer in Islam?",
    "language": "en"
}
```

### Console Application

For development and testing:

```bash
cd src/Muwasala.Console
dotnet run
```

## 🧪 Testing

### Integration Tests

Run the comprehensive integration test suite:

```bash
cd tests/IntegrationTest
dotnet run
```

The integration tests verify:
- ✅ Service dependency injection
- ✅ Multi-agent orchestration
- ✅ Response synthesis and quality
- ✅ External API integration

### Unit Tests

```bash
dotnet test
```

## 🔗 Trusted Islamic Sources

The system prioritizes content from verified Islamic sources:

| Source | Priority | Content Type |
|--------|----------|--------------|
| Quran.com | 100 | Quranic text and translations |
| Sunnah.com | 95 | Authenticated hadiths |
| IslamQA.org | 90 | Scholarly fatwas |
| Dar-alifta.org | 90 | Official religious rulings |
| IslamicFinder.org | 85 | General Islamic content |

## 🛡️ Islamic Content Validation

All responses undergo rigorous Islamic content filtering:

- **Forbidden Topics Detection** - Automatic filtering of non-Islamic content
- **Source Authentication** - Verification against trusted Islamic authorities
- **Scholarly Review** - Cross-referencing with established Islamic scholarship
- **Madhab Consideration** - Respect for different schools of Islamic jurisprudence

## 🤝 Contributing

We welcome contributions from the Islamic community and developers:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Contribution Guidelines

- Ensure all Islamic content is authentic and properly sourced
- Follow .NET coding standards and best practices
- Include comprehensive tests for new features
- Respect Islamic principles in all contributions

## 📚 Documentation

- [Multi-Agent Architecture](docs/multi-agent-architecture.md) - Detailed system architecture
- [API Documentation](docs/api-reference.md) - REST API reference
- [Islamic Sources](docs/islamic-sources.md) - Trusted source documentation

## 🔄 Roadmap

### Phase 1: Core System (Current)
- ✅ Multi-agent architecture implementation
- ✅ Basic web interface
- ✅ Integration with DeepSeek and Ollama

### Phase 2: Enhanced Features
- 🔄 Mobile application development
- 🔄 Advanced Tajweed analysis
- 🔄 Personalized learning paths

### Phase 3: Community Features
- 📋 Scholar verification system
- 📋 Community Q&A platform
- 📋 Multi-language support expansion

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Islamic Scholars** - For their invaluable guidance and review
- **DeepSeek Team** - For their advanced AI models
- **Ollama Community** - For local LLM infrastructure
- **Open Source Community** - For the foundational technologies

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/muwasala-islamic-knowledge-network/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/muwasala-islamic-knowledge-network/discussions)
- **Email**: support@muwasala.org

---

**"وَقُل رَّبِّ زِدْنِي عِلْمًا"**  
*"And say: My Lord, increase me in knowledge."* - Quran 20:114

Built with ❤️ for the Islamic community by developers who care about authentic Islamic knowledge.
