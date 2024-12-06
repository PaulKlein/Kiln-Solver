# Kiln Solver

## Getting Started

Check out the code or just download a pre-built release from the [releases page](https://github.com/PaulKlein/Kiln-Solver/releases)

Fill out the grid with the wares to fit in the kiln:

 - Name: Name of the item
 - Size: Percentage of space it takes up (assumes all levels are the same size)
 - Allowed Levels: If your ware can only be placed on a specific level, you can restrict the placement
 - Count: Number of items of this ware you need to fit in

Once you have your items filled, you can solve:

 - Optimal Solve: Tries to balance out the levels as much as possible
 - Basic Solve: Just generates a valid solution without trying to optimise (may be faster for some complicated layouts)

The solution can be printed if required for easy reference. Print -> Save to PDF is also useful if you don't want to physically print

The solver automatically saves the current wares list so it's there for next time you run the program. You can also save/load manually.

## Credits

This project is built with the help of:
 - [Zen](https://github.com/microsoft/Zen) - the constraint solving library
 - [ModernWpf](https://github.com/Kinnara/ModernWpf) - UI framework
