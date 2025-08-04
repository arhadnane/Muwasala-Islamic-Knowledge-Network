# Collections de Hadiths - Sources de Données

Ce dossier contient les fichiers sources pour alimenter la base de données de hadiths.

## 📁 Structure des dossiers

```
data/Hadith/
├── SourceFiles/           # Fichiers sources originaux
│   ├── bukhari.json      # Sahih al-Bukhari complet
│   ├── muslim.json       # Sahih Muslim complet
│   ├── tirmidhi.json     # Jami' at-Tirmidhi
│   ├── abudawud.json     # Sunan Abu Dawud
│   ├── nasai.json        # Sunan an-Nasa'i
│   ├── ibnmajah.json     # Sunan Ibn Majah
│   └── combined.json     # Collection combinée (optionnel)
└── README.md             # Ce fichier

```

## 🔄 Formats de fichiers supportés

### Format JSON recommandé :
```json
{
  "collection": "Sahih al-Bukhari",
  "language": "en",
  "hadiths": [
    {
      "book": 1,
      "hadith_number": "1",
      "arabic_text": "إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ",
      "translation": "Actions are but by intention...",
      "grade": "Sahih",
      "topic": "intention",
      "keywords": ["intention", "niyyah", "actions"],
      "chain": ["Umar ibn al-Khattab", "Alqama ibn Waqqas"],
      "explanation": "Cette hadith souligne l'importance de l'intention..."
    }
  ]
}
```

### Format CSV alternatif :
```csv
collection,book,hadith_number,arabic_text,translation,grade,topic,keywords,chain,explanation
Sahih al-Bukhari,1,1,إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ,Actions are but by intention...,Sahih,intention,"intention,niyyah,actions","Umar ibn al-Khattab,Alqama ibn Waqqas",Cette hadith souligne...
```

## 📥 Sources recommandées pour télécharger les hadiths

### 1. **Sunnah.com API**
- URL : https://sunnah.com/
- Format : JSON
- Collections : Bukhari, Muslim, Tirmidhi, Abu Dawud, Nasa'i, Ibn Majah
- Utilisation : API gratuite avec accès aux collections complètes

### 2. **IslamicFinder**
- URL : https://www.islamicfinder.org/
- Format : JSON/XML
- Collections : Collections principales + autres

### 3. **Hadith API**
- URL : https://github.com/fawazahmed0/hadith-api
- Format : JSON
- Avantage : Open source, collections multiples

### 4. **QuranAcademy Hadith Database**
- URL : https://github.com/spa5k/hadith_db
- Format : JSON/SQL
- Avantage : Base de données complète prête à l'usage

## 🚀 Comment utiliser ces fichiers

1. **Téléchargez** les collections de hadiths depuis les sources ci-dessus
2. **Placez** les fichiers dans le dossier `SourceFiles/`
3. **Modifiez** le `DatabaseInitializer.cs` pour charger ces fichiers
4. **Exécutez** la réinitialisation de la base de données

## 🔧 Implémentation automatique

Le système peut être étendu pour :
- ✅ Charger automatiquement tous les fichiers JSON du dossier SourceFiles
- ✅ Valider le format des données
- ✅ Éviter les doublons lors de l'importation
- ✅ Supporter plusieurs langues (arabe, français, anglais)
- ✅ Indexer automatiquement par topics et mots-clés

## 📊 Statistiques actuelles

- **Hadiths en base** : Déterminé automatiquement lors du seeding
- **Collections supportées** : Extensible selon les fichiers ajoutés
- **Langues** : Arabe + traductions (français/anglais)
