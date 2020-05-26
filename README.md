# 432-Final-Project

Online Multiplayer Tic-Tac-Toe

Using windows forms and c# (Only compatable with windows)

Installation:
Download both the game server and the game lobby.\n
Go to gamelobby -> TicTacToe.sln and open with Visual Studio, then find and open game.cs and gameLobby.cs.\n
Set the IP address to loopback to run on local machine by uncommenting line 55 and commenting out line 50 (game.cs) uncomment line 158 and comment out line 153.\n
Build the game to produce an executable.\n
Then go to gameServer -> TicTacToeserver.cs and open in Visual Studio.\n
Set the IP address to loopback to run on local machine by uncommenting line 36 and commenting out line 31.\n
Then click debug -> run to start the server.\n
Then find (usually in the debug folder TicTacToe -> Obj -> debug) and double click the executable you made before.\n
Start two tic-tac-toe games to play against yourself.\n

Make sure you start the server before the game.
