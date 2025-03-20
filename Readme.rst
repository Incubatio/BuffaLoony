*"Summarize your approach, architectural decisions, and note any future improvements you’d make."*

My Approach
===========

I began by brainstorming and lay out a plan for 20-30 minutes. If this were not a test and I had more time, I would likely create diagrams, at least for the data layer.

Here are my notes::

    Key Points
    ----------

    1. **Multi-State Character**:
       - The emphasis in the title made me prioritize making it easy to add new states in the future.
         State switching should cleanly transition between states, ensuring the old state is properly cleaned
         up before entering the new one.

    2. **State Change Implementation**:
       - I implemented orbiting projectiles/orbs around the player, which could serve as both life points and a
         melee weapon. The orbit algorithm is also reusable for bullet-hell mechanics.
       - Collecting a certain number of orbs transforms the player into a Titan (automatic attacks).
       - Taking damage reverts the player to Ghost form (invulnerable for 3 seconds).

    3. **Debug Overlay**:
       - I created a debug overlay for designers to test and debug quickly.
         This included a custom inspector in the `MainMono` GameObject and custom buttons in UIToolkit.


    Assets
    ------

        - **Characters**: Simple shapes (capsule for characters, sphere for orbs/projectiles).
        - **Colors**: Blue for the local player, red for AI.
        - **AI Movement**: Waypoints for simple AI movement, visualized with gizmos.
        - **UI**: Health bars above characters using UGUI, and a setup for UIDocument and buttons using UIToolkit.


    Tasks
    -----

       ☑ Create the camera, ground, player prefab, `StartGame` scene, `MainMonoBehavior`, and basic movement.

       ☑ Add waypoints with gizmos, spawn AI from the UI, and implement AI movement using random waypoints.

       ☑ Create a projectile prefab, pre-warm a large pool of projectiles,
       add orbiting orbs around players (used as life points and melee weapons).

       ☑ Implemente a custom collision detection system using basic circle collisions.

       ☑ Add life and death mechanics, including a Ghost state when hit (invulnerable for 3 seconds),
       add revive button on death.

       ☑ Add a Titan state with a powerful attack.

       ☑ Add a canvas and create a progress bar.


Beyond Requirements
-------------------

I added few feature that were not in the original exercise:

- **Custom Collision System**: A custom collision detection system using circle collisions.
- **AI Movement**: AI that moves between waypoints.
- **Orb Gameplay**: Orbiting orbs that act as life points and melee weapons.

The idea was to get closer to a game as making something interactive and fun is huge motivator for me.
This resulted in 6-7 hours of work instead of the original 3 requested hours.


Architecture Decisions
======================

Folder Structure
----------------

Here is the folder architecture I used:

.. code-block:: text

    Assets
    ├── Art
    ├── Code
    │   ├── Components
    │   ├── Gizmos
    │   ├── Helpers
    │   └── RouterMono.cs
    └── StartGame.scene

- **Root of Assets**: The only file at the root of the `Assets` folder is the scene (`StartGame.scene`) that initializes the game.
- **Root of Code**: At the root of the `Code` folder, you will find the single point of entry for the application: `RouterMono.cs`.

Design Philosophy
-----------------

- **Single Update Loop**: My go-to method in Unity is to use a single update loop for the entire game, which results in a single `MonoBehavior` as the entry point.
- **Art Folder**: All materials and prefabs are placed in the `Art` folder.
- **Code Folder**: This contains all C# scripts, shaders, and UI files (XHTML and CSS).

Code Organization
-----------------

- **Components**: This folder contains `MonoBehavior` scripts that are used solely to hold data. All data is public, and there are no functions that modify state (though there may be some convenient accessors).
- **Helpers**: This folder contains static classes that perform specific tasks.
- **RouterMono.cs**: This script is designed to contain only high-level code, also referred to as "flow." This concept originates from Martin Fowler's "Container Object" or "Flow Object" patterns. While `RouterMono` does include a small amount of OOP, I generally avoid OOP as it can create dependencies and technical debt over time.


Future Improvements
===================

1. **Code Cleanup**: Move more code from `RouterMono` to `Helpers`, as some of the code in `RouterMono` is not strictly related to flow.
2. **Data Structures**: Create data structures to allow passing chunks of data, which would reduce the number of parameters in functions.
3. **Tooling for Designers and Artists**: Improve the tools available in the inspector and cheat buttons. Currently, these tools are very basic.
4. **Object Pooling**: Implement a proper object pooling system. While I pre-allocated 1000 orbs/projectiles, a more robust solution would be cleaner and more efficient.
