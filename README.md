# Word Tiles Clone

## Architecture & Design Reasoning

The project adopts a layered architecture, prioritizing maintainability and scalability. Game logic, visuals, and UI are separated into distinct layers/components. This separation makes the codebase more modular and easier to test or extend in the future.

Key design principles and patterns used:
- **SOLID Principles:** For flexible and maintainable code.
- **Observer Pattern:** Allows components to communicate game events without tight coupling.
- **Dependency Injection:** Minimizes dependencies between components.

## How the Auto Solver is Implemented

The auto solver logic is entirely contained in the `AIGameAgent` script. The process is as follows:

- I restricted the player from interfering with the game in any way while the AI agent is active.
- When started, the AI gathers all letters on the board and identifies which are available for play.
- It runs a coroutine loop where, at each step:
  - It checks possible moves by finding all playable letters.
  - It searches the dictionary (accessed via reflection on the `WordValidator` component) for the best word that can be formed from the available letters. The function `FindBestWord_ConsideringPenalty` evaluates all possible words, maximizes the word score, and minimizes penalties for unused letters.
  - If a valid word is found, the AI picks and places the corresponding letters onto the board, then submits the word.
  - If no valid word is found, the AI undoes any partial moves and waits, handling end-of-game scenarios gracefully.
- The AI manages all interaction using coroutines to ensure actions happen with human-like timing, and handles various edge cases (such as destroyed letters or blocked moves) robustly.

## AI/Tooling Used

Several AI solutions and development tools are integrated to enhance automation and maintain code quality:

- **ClaudeAI:** Used for visual adjustments .
- **GitHub Copilot/ChatGPT:** Assisted in code design and documentation.
- **GeminiAI:** For code quality and  analysis.
