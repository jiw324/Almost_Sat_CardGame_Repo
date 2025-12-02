# Minigame Integration Guide

## Overview
Cards can now trigger minigames when played. The minigame result (0-1) multiplies the card's effect values.

## Setup Steps

### 1. Create MinigameRegistry GameObject
- Add a `MinigameRegistry` component to a GameObject in your scene (or create a new one)
- In the Inspector, add entries to the "Minigames" list:
  - **Minigame Id**: The ID string (e.g., "reaction", "forest_minigame")
  - **Prefab**: The minigame prefab (must have an `IMinigame` component)

### 2. Add Minigame to Card JSON
In `StreamingAssets/cards.json`, add a `minigameId` field to any card:

```json
{
    "id": "fireball",
    "cardName": "Fireball",
    "description": "Deal damage to all enemies.",
    "cost": 3,
    "type": "spell",
    "isRanged": true,
    "minigameId": "reaction",  // <-- Add this
    "effects": [
        {
            "effectId": "damage_enemy",
            "effectValue": 6
        }
    ]
}
```

### 3. How It Works
- When a card with `minigameId` is played, the minigame launches first
- Player plays the minigame (returns score 0-1)
- Card effects are resolved with values multiplied by the minigame result
  - Score 0.0 = 0% effectiveness (effects do nothing)
  - Score 0.5 = 50% effectiveness
  - Score 1.0 = 100% effectiveness (normal)

### 4. Example
If a card deals 10 damage and the player scores 0.7 in the minigame:
- Final damage = 10 × 0.7 = 7 damage

## Current Limitations
- Minigames only affect **spell cards** (not minions yet)
- Minigame result applies to all effect values (damage, healing, etc.)

## Files Modified
- `CardJSON.cs` - Added `minigameId` field
- `CardData.cs` - Added `minigamePrefab` field
- `CardFactory.cs` - Loads minigame prefabs from registry
- `CardInstance.cs` - Triggers minigames before resolving effects
- `MinigameRegistry.cs` - New: Maps minigame IDs to prefabs

