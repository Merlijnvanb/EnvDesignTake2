---
name: charactercontroller-velocity-unreliable
description: CharacterController.velocity is unreliable for movement detection — use raw input instead
metadata:
  type: feedback
---

Don't use `CharacterController.velocity.magnitude` to detect whether the player is moving. It can report 0 even when the controller is moving via `Move()`.

**Why:** Caused head bobbing to silently not work — the moving check always returned false so the bob timer never advanced.

**How to apply:** When needing to know if the player is moving, track raw input (e.g. `Input.GetAxisRaw`) as a field and check its magnitude instead.
