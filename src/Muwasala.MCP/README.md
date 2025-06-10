# Muwasala Islamic Knowledge MCP Server

A Model Context Protocol (MCP) server that provides access to the Muwasala Islamic Knowledge Network SQLite database.

## Features

- **Database Queries**: Execute safe SELECT queries on the Islamic knowledge database
- **Content Search**: Search across Quran, Hadith, Fiqh, and Du'a content
- **Structured Access**: Get specific verses, rulings, or prayers with proper metadata
- **Schema Inspection**: Explore database structure and table schemas
- **Islamic Content Types**: Specialized tools for different Islamic knowledge domains

## Installation

1. Build the project:
```bash
dotnet build
```

2. Run the MCP server:
```bash
dotnet run --project src/Muwasala.MCP -- "path/to/IslamicKnowledge.db"
```

## Configuration for VS Code

Add this to your VS Code settings.json:

```json
{
  "mcp": {
    "servers": {
      "islamic-knowledge": {
        "command": "dotnet",
        "args": [
          "run",
          "--project",
          "d:/Coding/VSCodeProject/Muwasala Islamic Knowledge Network/src/Muwasala.MCP",
          "--",
          "d:/Coding/VSCodeProject/Muwasala Islamic Knowledge Network/data/database/IslamicKnowledge.db"
        ],
        "env": {}
      }
    }
  }
}
```

## Available Tools

### Database Tools
- `query_database` - Execute SQL SELECT queries
- `list_tables` - List all database tables
- `get_table_schema` - Get schema for a specific table

### Content Search Tools
- `search_quran` - Search Quranic verses
- `search_hadith` - Search Hadith collections
- `search_fiqh` - Search Islamic jurisprudence
- `search_dua` - Search prayers and supplications

### Specific Content Tools
- `get_quran_verse` - Get a specific verse by Surah:Verse

## Available Prompts

### islamic_search
Search across all Islamic content types with a query.

Parameters:
- `query` (required): Search term
- `content_types` (optional): Specific content types to search

### verse_analysis  
Analyze a specific Quran verse with context and related content.

Parameters:
- `surah` (required): Surah number (1-114)
- `verse` (required): Verse number

## Database Schema

The Islamic Knowledge database contains:

- **QuranVerses**: Complete Quranic text with translations and metadata
- **HadithRecords**: Authenticated Hadith with chains and gradings
- **FiqhRulings**: Islamic jurisprudence by different schools (madhabs)
- **DuaRecords**: Islamic prayers with occasions and benefits
- **TajweedRules**: Quranic recitation rules and applications
- **SirahEvents**: Events from Prophet Muhammad's biography
- **SearchHistory**: Analytics and search tracking

## Security

- Only SELECT queries are allowed for database safety
- Input validation and parameterized queries prevent SQL injection
- Read-only access ensures data integrity

## Usage Examples

### Search for verses about prayer:
```json
{
  "method": "tools/call",
  "params": {
    "name": "search_quran",
    "arguments": {
      "searchTerm": "prayer",
      "limit": 5
    }
  }
}
```

### Get a specific verse:
```json
{
  "method": "tools/call", 
  "params": {
    "name": "get_quran_verse",
    "arguments": {
      "surah": 1,
      "verse": 1
    }
  }
}
```

### Execute custom query:
```json
{
  "method": "tools/call",
  "params": {
    "name": "query_database", 
    "arguments": {
      "query": "SELECT * FROM QuranVerses WHERE SurahNumber = 1 LIMIT 5"
    }
  }
}
```

## Development

The MCP server is built with .NET 8 and follows the MCP specification for tools, resources, and prompts. It provides a clean interface for AI systems to access Islamic knowledge data.
