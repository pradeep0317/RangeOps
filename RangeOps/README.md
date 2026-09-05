# Shooting Range — Interaction, Inventory \& Equipment System

Take-home assignment submission for the Jr. Unity Developer role — Edgeforce Solutions.

## Unity Version

**6000.3.16f1**

## How to Run

1. Open the project in Unity Hub with version `6000.3.16f1` (or later 6000.x).
2. Open `Assets/Rangeops/Scenes/MainScene`.
3. Press Play.
4. Controls:

   * **WASD** — Move
   * **Mouse** — Look
   * **E** — Interact / Pick up (shown only when looking at an interactable within range)
   * **Q** — Switch equipped weapon (Primary ↔ Secondary)
   * **G** — Drop the currently held weapon
   * Click the backpack icon to open/close the inventory
   * **H** — Toggle the instructions panel (Welcome panel dismisses on any key press)
   * Drag a weapon/attachment/backpack item onto the on-screen **Drop Zone** to drop it via the UI instead of the keyboard

## Architecture Overview

The project is split into three independent systems that only communicate through interfaces and events — no system reaches into another's internals.

### Interaction (`Game.Interaction`)

* `IInteractable` — a two-method contract (`GetInteractionPrompt`, `Interact`). Any object that implements it works with the interactor automatically; no per-object special-casing.
* `Interactor` — sits on the player, raycasts forward each frame, checks range, and raises an event when the look target changes. It has no knowledge of inventory, equipment, or UI.
* `InteractionPromptUI` / `EquipMessageUI` — pure view layers that only listen to events and update text/toasts.

### Items \& Equipment (`Game.Items`)

* `ItemData` (ScriptableObject) — data-driven definition for every item (guns, ammo, attachments, melee). Category (`Primary` / `Secondary` / `Ammo` / `Attachment`) decides routing. Adding a new item is a new asset — zero code changes.
* `WorldItem` — the bridge between a world object and the player systems. It inspects the item's category and routes to either `EquipmentController` (weapons/attachments) or `InventorySystem` (ammo/generic items). It never contains inventory or equip logic itself.
* `EquipmentController` — owns the two weapon slots (Primary/Secondary) and the three attachment slots (Magazine/Grip/Scope). Reuses the actual picked-up world object as the hand model (no separate viewmodel prefab), refuses to equip into an already-occupied slot until the current item is dropped, and tracks which weapon is currently active in-hand (only one visible at a time — switching hides the other).

### Inventory (`Game.Inventory`)

* `InventorySystem` — a plain C# class (no `MonoBehaviour`, no UI reference) holding a fixed number of slots. Handles stacking, full-inventory rejection, and partial-quantity removal. Because it has no Unity/UI dependency, it's trivially testable in isolation.
* `PlayerInventoryHolder` — the only `MonoBehaviour` that owns an `InventorySystem` instance; the bridge between Unity's component world and the plain data class.
* `InventoryUI` — the only class that reads from `InventorySystem` and `EquipmentController` to draw the backpack, weapon panels, and attachment panels. It never mutates data except in direct response to a drop action.
* `DraggableSlot` / `DropHandler` — generic drag-to-drop-zone support and world-spawn logic shared by every drop path (weapon, attachment, or backpack item).

## Key Design Decisions

* **Single source of truth**: ammo counts, weapon state, and backpack contents are always read directly from `InventorySystem` / `EquipmentController` at UI-refresh time — no parallel/duplicate counters are kept anywhere, which is what keeps the UI and the underlying data permanently in sync.
* **Category-based routing, not type-checking**: `WorldItem` decides where an item goes purely from `ItemData.category`, so adding a new category of behaviour doesn't require touching existing classes (Open/Closed Principle).
* **Composition over inheritance for stacking**: stackability is a flag + max size on `ItemData`, not a separate class hierarchy per item type.
* **Chunked stack visualization**: a single inventory slot holding a large stack (e.g. 35 rounds of ammo) is displayed as multiple UI chunks (e.g. 10+10+10+5) so the backpack panel never shows an unbounded number on one card. Dropping one chunk removes only that chunk's quantity from the underlying slot, not the whole stack.
* **Real-object equip**: rather than a separate viewmodel prefab, the actual world object that was picked up is reparented into the player's hand point (collider disabled, physics made kinematic while held) — simpler for this scope and avoids maintaining two models per item.
* **Modeled attachments**: attachment meshes (scope, magazine, grip) already exist as disabled child objects on the primary weapon's prefab; equipping an attachment enables the matching child (and disables its Rigidbody/Collider while attached) rather than spawning anything new.
* **Input System package**: all key handling uses Unity's New Input System (`Keyboard.current`), matching the FPS Starter Assets controller.

## Assumptions \& Limitations

* No actual firing/shooting mechanic is implemented — this is intentional. The assignment scope is interaction + inventory + item-behaviour systems, not a combat mechanic, so guns are treated purely as pickup/equip/drop items.
* Ammo count shown on the primary weapon is the player's total reserve ammo for the compatible ammo type (matched via an `ammoType` string on the weapon's `ItemData`), not a magazine-loaded count, since there is no reload/fire loop to consume it.
* Only one Primary and one Secondary weapon can be owned at a time; picking up a second of either category is blocked with an on-screen message until the currently held one is dropped.
* The backpack has a fixed number of slots (set via `PlayerInventoryHolder.slotCount`); this governs when "Bag Full" triggers, independent of how many visual chunks a single stack is split into.
* Attachment items only affect the currently equipped Primary weapon; there is no support for attachments on the Secondary weapon in this build.

