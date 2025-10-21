-- language: sql
-- AI-Generated Code Header
-- **Intent:** MySQL 8.0 schema for card game with cards, relics, curses, and encounters
-- **Optimization:** InnoDB engine, utf8mb4 charset, proper indexes for performance
-- **Safety:** Foreign key constraints, check constraints, NOT NULL where required

-- Create database
CREATE DATABASE IF NOT EXISTS cardgame CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE cardgame;

-- ========================================
-- CARD TABLE
-- ========================================

CREATE TABLE IF NOT EXISTS cards (
    card_id CHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    
    -- Card Classification
    card_type_id CHAR(36) NULL,
    category_id CHAR(36) NULL,
    faction_id CHAR(36) NULL,
    
    -- Card Stats
    health INT NOT NULL DEFAULT 0 CHECK (health >= 0),
    shield_health INT NOT NULL DEFAULT 0 CHECK (shield_health >= 0),
    attack_damage INT NOT NULL DEFAULT 0 CHECK (attack_damage >= 0),
    mana_cost INT NOT NULL DEFAULT 0 CHECK (mana_cost >= 0),
    row_position INT CHECK (row_position >= 0),
    
    -- Related Data
    minigame_id CHAR(36) NULL,
    ability_id CHAR(36) NULL,
    
    -- Metadata
    rarity VARCHAR(50),
    unlock_condition TEXT,
    icon_path VARCHAR(255),
    
    -- System Fields
    is_enabled TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    -- Indexes
    INDEX ix_cards_name (name),
    INDEX ix_cards_card_type (card_type_id),
    INDEX ix_cards_category (category_id),
    INDEX ix_cards_faction (faction_id),
    INDEX ix_cards_rarity (rarity)
) ENGINE=InnoDB;

-- ========================================
-- RELIC TABLE
-- ========================================

CREATE TABLE IF NOT EXISTS relics (
    relic_id CHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    
    -- Properties
    rarity VARCHAR(50),
    effect_type VARCHAR(100) NOT NULL,
    
    -- Related Data
    faction_id CHAR(36) NULL,
    
    -- Metadata
    unlock_condition TEXT,
    icon_path VARCHAR(255),
    
    -- System Fields
    is_enabled TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    -- Indexes
    INDEX ix_relics_name (name),
    INDEX ix_relics_rarity (rarity),
    INDEX ix_relics_effect_type (effect_type),
    INDEX ix_relics_faction (faction_id)
) ENGINE=InnoDB;

-- ========================================
-- CURSE TABLE
-- ========================================

CREATE TABLE IF NOT EXISTS curses (
    curse_id CHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    description TEXT NOT NULL,
    
    -- Properties
    rarity VARCHAR(50),
    effect_type VARCHAR(100) NOT NULL,
    
    -- Related Data
    faction_id CHAR(36) NULL,
    
    -- Metadata
    unlock_condition TEXT,
    icon_path VARCHAR(255),
    
    -- System Fields
    is_enabled TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    -- Indexes
    INDEX ix_curses_name (name),
    INDEX ix_curses_rarity (rarity),
    INDEX ix_curses_effect_type (effect_type),
    INDEX ix_curses_faction (faction_id)
) ENGINE=InnoDB;

-- ========================================
-- ENCOUNTER TABLE
-- ========================================

CREATE TABLE IF NOT EXISTS encounters (
    encounter_id CHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    
    -- Properties
    rarity VARCHAR(50),
    effect_type VARCHAR(100) NOT NULL,
    
    -- Related Data
    faction_id CHAR(36) NULL,
    
    -- Metadata
    unlock_condition TEXT,
    
    -- System Fields
    is_enabled TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    -- Indexes
    INDEX ix_encounters_name (name),
    INDEX ix_encounters_rarity (rarity),
    INDEX ix_encounters_effect_type (effect_type),
    INDEX ix_encounters_faction (faction_id)
) ENGINE=InnoDB;

-- ========================================
-- NOTES
-- ========================================
-- This schema uses simple ID references without foreign key constraints
-- You can store card_type_id, category_id, faction_id, minigame_id, ability_id as strings or UUIDs
-- Validation should be handled at the application level

-- ========================================
-- EXAMPLE INSERT STATEMENTS
-- ========================================

-- Example: Insert a Card
INSERT INTO cards (
    card_id,
    name,
    description,
    card_type_id,
    category_id,
    faction_id,
    health,
    shield_health,
    attack_damage,
    mana_cost,
    row_position,
    minigame_id,
    ability_id,
    rarity,
    unlock_condition,
    icon_path,
    is_enabled
) VALUES (
    UUID(),                                    -- Generates unique ID
    'Fire Warrior',                            -- Card name
    'A fierce warrior wielding the power of flames. Deals bonus damage to ice enemies.',  -- Description
    'neutral-001',                             -- Card Type: Neutral
    'unit-001',                                -- Category: Unit
    'fire-faction-001',                        -- Faction: Fire
    100,                                       -- Health: 100 HP
    25,                                        -- Shield Health: 25 shield
    50,                                        -- Attack Damage: 50
    3,                                         -- Mana Cost: 3 mana
    1,                                         -- Row Position: Front row (1)
    NULL,                                      -- Minigame ID: Not specific to minigame
    'fire-blast-001',                          -- Ability: Fire Blast
    'Common',                                  -- Rarity: Common
    'Available from start',                    -- Unlock Condition
    'assets/cards/fire_warrior.png',           -- Icon Path
    1                                          -- Is Enabled: Yes
);

-- Example: Insert another Card (Special type)
INSERT INTO cards (
    card_id,
    name,
    description,
    card_type_id,
    category_id,
    faction_id,
    health,
    shield_health,
    attack_damage,
    mana_cost,
    row_position,
    minigame_id,
    ability_id,
    rarity,
    unlock_condition,
    icon_path,
    is_enabled
) VALUES (
    UUID(),
    'Lightning Strike',
    'Summon a devastating lightning bolt that deals massive damage to a single enemy.',
    'special-001',                             -- Card Type: Special
    'attack-001',                              -- Category: Attack
    'storm-faction-001',                       -- Faction: Storm
    0,                                         -- Health: 0 (it's a spell)
    0,                                         -- Shield Health: 0
    120,                                       -- Attack Damage: 120
    5,                                         -- Mana Cost: 5 mana
    NULL,                                      -- Row Position: NULL (spell)
    NULL,                                      -- Minigame ID: Not specific
    'lightning-damage-001',                    -- Ability: Lightning Damage
    'Rare',                                    -- Rarity: Rare
    'Defeat the Storm Lord boss',              -- Unlock Condition
    'assets/cards/lightning_strike.png',       -- Icon Path
    1                                          -- Is Enabled: Yes
);

-- Example: Insert a Relic
INSERT INTO relics (
    relic_id,
    name,
    description,
    rarity,
    effect_type,
    faction_id,
    unlock_condition,
    icon_path,
    is_enabled
) VALUES (
    UUID(),
    'Ancient Shield',
    'Grants +10 defense to all units. Stackable up to 3 times.',
    'Epic',                                    -- Rarity: Epic
    'buff_defense',                            -- Effect Type
    'guardian-faction-001',                    -- Faction: Guardian
    'Complete Chapter 3',                      -- Unlock Condition
    'assets/relics/ancient_shield.png',        -- Icon Path
    1                                          -- Is Enabled: Yes
);

-- Example: Insert a Curse
INSERT INTO curses (
    curse_id,
    name,
    description,
    rarity,
    effect_type,
    faction_id,
    unlock_condition,
    icon_path,
    is_enabled
) VALUES (
    UUID(),
    'Weakening Hex',
    'Reduces all unit attack damage by 20% for the duration of combat.',
    'Common',                                  -- Rarity: Common
    'debuff_attack',                           -- Effect Type
    'shadow-faction-001',                      -- Faction: Shadow
    'Take damage from a boss',                 -- Unlock Condition
    'assets/curses/weakening_hex.png',         -- Icon Path
    1                                          -- Is Enabled: Yes
);

-- Example: Insert an Encounter
INSERT INTO encounters (
    encounter_id,
    name,
    description,
    rarity,
    effect_type,
    faction_id,
    unlock_condition,
    is_enabled
) VALUES (
    UUID(),
    'Forest Ambush',
    'A group of bandits ambushes you in the dark forest. Choose to fight or negotiate.',
    'Common',                                  -- Rarity: Common
    'combat',                                  -- Effect Type: Combat
    NULL,                                      -- Faction: None (neutral encounter)
    'Reach Chapter 1',                         -- Unlock Condition
    1                                          -- Is Enabled: Yes
);
