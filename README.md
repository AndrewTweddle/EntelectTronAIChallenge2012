Entelect Challenge 2012: Tron AI
================================

# Introduction

This is my entry to the 2012 Entelect AI Challenge. This entry placed 4th out of 101 entries.

The competition was won by Jaco Cronje. His blog post and code can be found at http://www.jacocronje.co.za/tron-bot/.

## Rules overview

The challenge was to write a command line utility to play the game of Tron against an opponent's "bot".

The bot had to be written in either Java or a Visual Studio language (C#, VB.Net or Visual C++). The command line utility would be launched on every turn, read a board state from a file (passed as the single command line argument), make a move and write out the replacement file. Each bot had 5 seconds to make each move.

However there was a twist... The game would be fought on a 30x30 grid representing a sphere. The board wrapped around horizontally. And rows 0 and 29 represented a single point each (the North and South pole). From a point in row 0 you could move to any point in row 1, and similarly for rows 29 and 28.

An archive of the rules is available on [the way back machine](http://web.archive.org/web/20120721045001/http://challenge.entelect.co.za/Home/Rules)

## Testing

I decided to write a WPF utility to test my application rather than writing unit tests. Unit testing is a great technique in a corporate setting, but isn't as suitable for something as fluid as a heuristic AI algorithm.

The WPF test utility can be used to play any two algorithms against one another. Additionally a human player can replace either or both algorithms.

# Tools used:

Visual C# 2010 Express.

# Running the application

1. Open up solution file AndrewTweddle.Tron.PlayAgainstAI.sln and hit F5 to build and run it.
2. Select algorithms for the 2 players in both drop-down combo boxes in the top-left.
3. Click the Start button.
4. If playing against the AI, simply double-click the next space to move to. Alternatively use the arrow keys to navigate around the grid, and ENTER to move to the selected cell.
5. There are buttons to save and load the game state to a file. You can also use the Reload button to quickly reload the last loaded file.
6. You can use the "Go to" button to move to a particular past point in a game. Use the edit box next to the button to specify the move number and the drop-down box to specify the next player to move.
7. You can use the Stop, Pause, Resume and Step buttons to control the flow of the game. For example, you can change the algorithms partway through a game.

# Known issues

1. I have seen situations occur where the resume button is not enabled after a pause or load.
   This prevents the game from continuing. I haven't been able to reproduce the error consistently.

2. The terms You and Opponent are confusing. They are terms in the official game state file format. They DON'T correspond to the human player and AI player.

# Configurable value functions

## Two configurable "solvers" (bots)

Two of the solvers are named ConfigurableNegaMaxSolver1 and ConfigurableNegaMaxSolver2.

These solvers use a pair of xml files to define weightings for their value functions.
The configuration files must in the same location as the executable.

There are 2 files. One is used when the opponent is in the same "compartment" in the solver. 
The other weightings file is used when the two bots become separated from one another.

The names of the files should be similar to the following:
ConfigurableSolver1_SameCompartment.xml
ConfigurableSolver1_SeparateCompartments.xml

## A sample weightings files

Below is a sample weightings file:

```xml
<Weightings>
  <NumberOfCellsReachableByPlayerFactor>0.0</NumberOfCellsReachableByPlayerFactor>
  <TotalDegreesOfCellsReachableByPlayerFactor>0.0</TotalDegreesOfCellsReachableByPlayerFactor>
  <NumberOfCellsClosestToPlayerFactor>10000.0</NumberOfCellsClosestToPlayerFactor>
  <TotalDegreesOfCellsClosestToPlayerFactor>1</TotalDegreesOfCellsClosestToPlayerFactor>
  <NumberOfComponentBranchesInTreeFactor>-0.1</NumberOfComponentBranchesInTreeFactor>
  <SumOfDistancesFromThisPlayerOnClosestCellsFactor>0.0</SumOfDistancesFromThisPlayerOnClosestCellsFactor>
  <SumOfDistancesFromOtherPlayerOnClosestCellsFactor>100.0</SumOfDistancesFromOtherPlayerOnClosestCellsFactor>
</Weightings>

# The pendulum solver

I was at the beach for the last week of the competition. I ran games between the configurable solvers while I was on the beach, and tweaked the parameters at lunch time and in the evening.

I also played games against my algorithm and found I could easily beat it by forcing it into a decision between two areas. The easiest way to do this was to move to the "equator", keep a decent-sized gap open along the equator, force the bot to close down the other gap on the equator to a single "corridor", and then race back to the bigger gap. The bot would be forced to commit to one or other hemisphere, and I could eat up space in that hemisphere before moving back to the other hemisphere.

While driving back from my beach holiday, I was thinking about how to implement my strategy. The pendulum solver was an attempt to build a rules engine which could play out a sequence of opening moves as described above, then revert to the standard algorithm.

The competition closed the next morning. Trying to implement this algorithm after a 500 km drive proved to be a bridge too far.

So the pendulum solver is an incomplete attempt at creating a killer strategy for the opening period of the game.
