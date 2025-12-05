# Card Game Database Structure

## 📊 Complete Table Overview

Your database now has **9 tables total**: 5 lookup tables + 4 main entity tables.

---

## 🎯 Main Entity Tables

### 1. **Cards Table**

| Column | Type | Description |
|--------|------|-------------|
| `card_id` | CHAR(36) | Primary Key |
| `name` | VARCHAR(255) | Card name (unique) |
| `description` | TEXT | Card description |
| `card_type_id` | CHAR(36) | FK → Neutral, Chapter Minigame, Special |
| `category_id` | CHAR(36) | FK → Unit, Attack, Defense, Utility |
| `faction_id` | CHAR(36) | FK → Faction reference |
| `health` | INT | Card health points |
| `shield_health` | INT | Shield/armor points |
| `attack_damage` | INT | Attack/damage value |
| `mana_cost` | INT | Mana required to play |
| `row_position` | INT | Board row position |
| `minigame_id` | CHAR(36) | FK → Associated minigame |
| `ability_id` | CHAR(36) | FK → Special ability |
| `rarity` | VARCHAR(50) | Common, Rare, Epic, Legendary |
| `unlock_condition` | TEXT | How to unlock this card |
| `icon_path` | VARCHAR(255) | Path to card icon/image |

---

### 2. **Relics Table**

| Column | Type | Description |
|--------|------|-------------|
| `relic_id` | CHAR(36) | Primary Key |
| `name` | VARCHAR(255) | Relic name (unique) |
| `description` | TEXT | Relic description |
| `rarity` | VARCHAR(50) | Common, Rare, Epic, Legendary |
| `effect_type` | VARCHAR(100) | Type of buff/effect |
| `faction_id` | CHAR(36) | FK → Faction reference |
| `unlock_condition` | TEXT | How to unlock this relic |
| `icon_path` | VARCHAR(255) | Path to relic icon/image |

---

### 3. **Curses Table**

| Column | Type | Description |
|--------|------|-------------|
| `curse_id` | CHAR(36) | Primary Key |
| `name` | VARCHAR(255) | Curse name (unique) |
| `description` | TEXT | Curse description |
| `rarity` | VARCHAR(50) | Common, Rare, Epic, Legendary |
| `effect_type` | VARCHAR(100) | Type of debuff/curse |
| `faction_id` | CHAR(36) | FK → Faction reference |
| `unlock_condition` | TEXT | How player gets this curse |
| `icon_path` | VARCHAR(255) | Path to curse icon/image |

---

### 4. **Encounters Table**

| Column | Type | Description |
|--------|------|-------------|
| `encounter_id` | CHAR(36) | Primary Key |
| `name` | VARCHAR(255) | Encounter name |
| `description` | TEXT | Encounter description |
| `rarity` | VARCHAR(50) | Common, Rare, Epic, Legendary |
| `effect_type` | VARCHAR(100) | Combat, Event, Boss, etc. |
| `faction_id` | CHAR(36) | FK → Faction reference |
| `unlock_condition` | TEXT | When encounter becomes available |

---

## 🔍 Lookup Tables

### 5. **card_types**
- Neutral
- Chapter Minigame  
- Special

### 6. **categories**
- Unit
- Attack
- Defense
- Utility

### 7. **factions**
- Faction groups/allegiances
- Has: name, description, icon_path

### 8. **minigames**
- Available game modes
- Has: code, name, description

### 9. **abilities**
- Special abilities for cards
- Has: code, name, description

---

## 🎨 Visual Structure

```
┌─────────────────────────────────────────────┐
│          LOOKUP TABLES (5)                   │
│                                              │
│  card_types  categories  factions           │
│  minigames   abilities                       │
└──────────────────┬──────────────────────────┘
                   │
       ┌───────────┼───────────────┐
       │           │               │
  ┌────▼────┐  ┌──▼─────┐  ┌──────▼────┐  ┌────────────┐
  │  CARDS  │  │ RELICS │  │  CURSES   │  │ ENCOUNTERS │
  └─────────┘  └────────┘  └───────────┘  └────────────┘
  
  Each has:
  - Name, Description
  - Rarity
  - Effect Type
  - Faction (FK)
  - Unlock Condition
  - Icon Path
```

---

## 🚀 How to Create Database

### Step 1: Run the Schema
```powershell
.\run_schema.ps1
```

### Step 2: Verify in MySQL Workbench
```sql
USE cardgame;
SHOW TABLES;
```

**Expected Output:** 9 tables
- abilities
- card_types
- cards
- categories
- curses
- encounters
- factions
- minigames
- relics

### Step 3: View Structure
```sql
DESCRIBE cards;
DESCRIBE relics;
DESCRIBE curses;
DESCRIBE encounters;
```

---

## 📝 Sample Data Examples

### Insert a Card
```sql
-- Get lookup IDs first
SET @neutral_type = (SELECT card_type_id FROM card_types WHERE name = 'Neutral' LIMIT 1);
SET @unit_category = (SELECT category_id FROM categories WHERE name = 'Unit' LIMIT 1);

-- Insert the card
INSERT INTO cards (
    card_id, name, description,
    card_type_id, category_id,
    health, shield_health, attack_damage, mana_cost, row_position,
    rarity, unlock_condition, icon_path
) VALUES (
    UUID(),
    'Fire Warrior',
    'A fierce warrior wielding flames',
    @neutral_type,
    @unit_category,
    100, 25, 50, 3, 1,
    'Common',
    'Available from start',
    'assets/cards/fire_warrior.png'
);
```

### Insert a Relic
```sql
INSERT INTO relics (
    relic_id, name, description,
    rarity, effect_type,
    unlock_condition, icon_path
) VALUES (
    UUID(),
    'Ancient Shield',
    'Grants +10 defense to all units',
    'Rare',
    'buff_defense',
    'Defeat 10 enemies',
    'assets/relics/ancient_shield.png'
);
```

### Insert a Curse
```sql
INSERT INTO curses (
    curse_id, name, description,
    rarity, effect_type,
    unlock_condition, icon_path
) VALUES (
    UUID(),
    'Weakening Hex',
    'Reduces attack damage by 20%',
    'Common',
    'debuff_attack',
    'Take damage from boss',
    'assets/curses/weakening_hex.png'
);
```

### Insert an Encounter
```sql
INSERT INTO encounters (
    encounter_id, name, description,
    rarity, effect_type,
    unlock_condition
) VALUES (
    UUID(),
    'Forest Ambush',
    'Battle against forest enemies',
    'Common',
    'combat',
    'Reach level 5'
);
```

---

## 🔗 View Diagram Online

1. Go to **[https://dbdiagram.io](https://dbdiagram.io)**
2. Open file: `db/cardgame_diagram.dbml`
3. Copy all content (Ctrl+A, Ctrl+C)
4. Paste into dbdiagram.io
5. See your database visualized! 🎨

---

## ✅ What You Have Now

| Feature | Status |
|---------|--------|
| Cards with all stats | ✅ |
| Card types (Neutral, Chapter, Special) | ✅ |
| Categories (Unit, Attack, Defense, Utility) | ✅ |
| Relics with unlock conditions | ✅ |
| Curses with unlock conditions | ✅ |
| Encounters with unlock conditions | ✅ |
| Factions system | ✅ |
| Minigames support | ✅ |
| Abilities system | ✅ |
| Icon paths for all entities | ✅ |
| Rarity system | ✅ |

---

## 📂 Database Files

- **Schema:** `db/schema_mysql.sql`
- **Diagram:** `db/cardgame_diagram.dbml`
- **Run Script:** `run_schema.ps1`
- **This Guide:** `docs/DatabaseStructure.md`

---

## 🎮 Next Steps

1. ✅ Run schema to create database
2. 📊 Visualize on dbdiagram.io
3. 📝 Add sample data for testing
4. 🔄 Connect Unity to database
5. 🎨 Import card/relic/curse data from spreadsheets

---

## Modification Report

- **Tables Created:** 9 total
  - 4 main entities (cards, relics, curses, encounters)
  - 5 lookup tables (card_types, categories, factions, minigames, abilities)

- **All Required Fields Included:**
  - ✅ Rarity for all entities
  - ✅ Effect type for relics, curses, encounters
  - ✅ Unlock conditions
  - ✅ Icon paths
  - ✅ Faction references
  - ✅ Row position for cards

- **Risk Assessment:** Low
  - Clean, normalized design
  - Proper foreign keys
  - All fields from requirements included

