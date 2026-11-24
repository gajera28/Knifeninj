# Iteration 3 Playtest — Meheraj

## Regular playthrough
- Reached win screen: Yes
- Startup and controls: A - Left, D - right, space - jump, F - Knife
- Platforms, coins, ammo, and checkpoints, etc.:
        - Collected all coins
        - When hit with obstacles, player gets helth dropped and pushed back
        - When hit/pushed back by enemy, player gets helth dropped and pushed back
        - Went to oth checkpoints
            - Checkpoints turn green once crossed
            - previous already attemped checkpoint turn grey when went to another one that turn green
            - Respawn once health droped/ died when taken by shadows from one of the green checkpoint(latest one) or the start point.
        - Collected +n knives from ammo boxes avialiable
        - Killed the enemy and destroyed the target using "F" key by throwing the knives
        - Reached win screen: Yes
- Targets, gate, and win screen:
        - Destroyed all 3 training targets and reached the shrine gate.
        - The Level 1 Complete screen showed a clear time of 2:38.
        - Final score of 480, 8 coins collected, and a time bonus of +0.
        - "R" key restarts the game.

- Full playthrough — Completed. You reached the win screen in 2:38 with 480 points, 8 coins, and 3/3 targets.

- Win-screen input — Pass. A/D, Space, and F produced no gameplay actions.

- Respawn input — Blocked. The lock window was too brief to verify by eye.

## Bugs discovered

### 1. Tapping Space produced a full-height jump

- Where it happened: Level 1, while jumping from the ground.
- Steps to reproduce: On the pre-fix version, quickly tap and release Space. Compare the height with a jump where you hold Space.
- Expected behavior: Tapping Space produces a shorter hop than holding it.
- Actual behavior before the fix: A quick tap could produce a full-height jump because the release was missed between physics updates.

### 2. Pressing Space again just after takeoff triggered a second jump

- Where it happened: Level 1, immediately after jumping from the ground.
- Steps to reproduce: On the pre-fix version, press Space to jump, then quickly press it again as the player leaves the ground.
- Expected behavior: The second press gives no extra upward boost. Jumping works again after landing.
- Actual behavior before the fix: The ground check could still report the player as grounded just after takeoff, allowing a second jump.