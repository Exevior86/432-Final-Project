# 432-Final-Project

Online Multiplayer Tic-Tac-Toe

Using windows forms and c# (Only compatable with windows)

Installation:


Download both the game server and the game lobby.

Go to gamelobby -> TicTacToe.sln and open with Visual Studio, then find and open game.cs and gameLobby.cs.

Set the IP address to loopback to run on local machine by uncommenting line 55 and commenting out line 50 (game.cs) uncomment line 158 and comment out line 153 (gameLobby.cs).

Build the game to produce an executable.

Then go to gameServer -> TicTacToeserver.cs and open in Visual Studio.

Set the IP address to loopback to run on local machine by uncommenting line 36 and commenting out line 31.

Then click debug -> run to start the server.

Then find (usually in the debug folder TicTacToe -> Obj -> debug) and double click the executable you made before.

Start two tic-tac-toe games to play against yourself.

Make sure you start the server before the game.
