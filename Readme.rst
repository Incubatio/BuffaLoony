*"Summarize your approach, architectural decisions, and note any future improvements you’d make."*


My Approach
===========

I began by brainstorming on a Google Doc for 20-30 minutes. If this were not a test and I had more time, I would likely create diagrams, at least for the data layer.

1. **Key Pillars**: I first listed the pillars or key points of the job.
2. **Assets**: I then listed all the assets required for the project.
3. **Tasks**: I created a list of tasks and ordered them based on priority.
4. **Testing**: I considered whether automated tests or a testing environment (Gym) would be beneficial. However, given the small scale of the game, I decided that implementing these would likely delay progress at this stage.

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