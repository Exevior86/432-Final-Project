/* Tic Tac Toe Server
 * Made by Adam Snyder and Colby Easton
 * CSS 432 Networking
 * 3/12/2020
 * 
 * Description:
 * This class is handles all of the server logic needed
 * for passing messages to clients for both the game lobby
 * and the game itself. 
 * Has the following features:
 * Lobby Chat
 * Game chat
 * Create game
 * List game
 * Join game
 * Exit gracefully
 * User login
 * 
 */

using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;


public class handleClient 
{
    TcpClient clientSocket;
    public Hashtable clientsList;
    Hashtable gameList;
    //use Program.clientStreams to get opponent stream
    //figure out where

    string clNo;
    string msgContent = "";
    string opponentName;

    /********************************** Start Client ***************************
 * Initializes the thread and assigns the variables needed to run the client.
 * Also broadcasts to the lobby the user that joins, but not game users (hopefully)
 */
    public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList, Hashtable gList)
    {
        this.clientSocket = inClientSocket;
        this.clNo = clineNo;
        this.clientsList = cList;
        this.gameList = gList;
        Thread ctThread = new Thread(handle);
        ctThread.Start();
        if (clNo.Length < 5)
        {
            broadcast("TEXT|" + clNo + " joined |", clNo, false);
        } 
        else
        {
            if (!clientsList.Contains(clNo.Substring(0, clNo.Length - 4)))
            {
                broadcast("TEXT|" + clNo + " joined |", clNo, false);
            }
        }
    } // end start client method

    // Parses the messages needed for the server to separate the command and the messages.
    // All messages that are passed are text, so we split string based on a delimiter.
    // The first part of a message is the command and then the message or messages follow that.
    // Each command needs a different way to split the message to avoid errors.
    private string[] parseMessage(string message)
    {
        string command = "";
        string delimiter = "|";
        int nextMsg;
        string[] returnValue = new string[2];
        command = message.Substring(0, message.IndexOf(delimiter));
        nextMsg = command.Length;
        returnValue[0] = command;

        // If the message doesn't contain anything return just the command
        if (message.Substring(nextMsg + 1, 500).Length <= 0)       
            return returnValue;
        
        // If the command is equal to text means a lobby chat to be broadcast
        // to the everyone in the lobby.
        if (command.Equals("TEXT"))
        {
            // buffer is larger than ouor messages, so we want to shorten the message
            // but most importantly need to trim the command off the message to get
            // to the next delimiter, otherwise the indexOf method would return
            // the previous int and would cut off messages.
            message = message.Substring(nextMsg + 1, 500);
            message = message.Substring(0, message.IndexOf("|"));
            msgContent = message;
        }
        else if (command.Equals("INGAME") || command.Equals("GAMEEXIT"))
        {          
            message = message.Substring(nextMsg + 1, 500);
            message = message.Substring(0, message.IndexOf(delimiter) + 1);
            msgContent = message;
        }
        else if (command.Equals("TURN"))
        {
            // Turns only will have the button name being a 2 character name
            msgContent = message.Substring(nextMsg + 1, 2); // button pushed
            message = message.Substring(nextMsg + 1, 500);

            // Need the opponent name so the server can send message to the correct user
            opponentName = message.Substring(message.IndexOf("~") + 1, message.IndexOf(delimiter) - 3);
        }
        else if (command.Equals("LISTGAMES"))
        {
            message = "";
            msgContent = message;
        }
        else if (command.Equals("JOINGAME"))
        {
            message = message.Substring(nextMsg + 1, 500);
            message = message.Substring(0, message.IndexOf(delimiter));
            msgContent = message;
            opponentName = message;
        }
        else if (!message.Contains(delimiter))
        {
            message = message.Substring(nextMsg + 1, nextMsg + 20);
            msgContent = message;
        }
        else
        {
            message = message.Substring(nextMsg + 1, message.IndexOf(delimiter));
            msgContent = message;
        }
        returnValue[1] = message;

        return returnValue;
    } // end parse message method

    /********************* Send Text  *********************
     * Calls the broadcast method for sending text to every
     * user in the game lobby and sets a log of what was said  
     * in the server.
     */
    private void sendText(string command)
    {
        broadcast(msgContent, clNo, true);
        Console.WriteLine(clNo + " sent command: " + command + " sent message: " + msgContent);
    } // end send text method

    /********************* Game Lobby Exit ********************
     * Removes the user's name out of the client list, closes the
     * socket and alerts the other users in the lobby that the user 
     * has left the chat room.
     */
    private void exit()
    {
        broadcast("TEXT|" + clNo + " has disconnected from chat|", null, false);
        clientSocket.Client.Shutdown(SocketShutdown.Both);
        clientSocket.Dispose();
        clientsList.Remove(clNo);
        Console.WriteLine(clNo + " has disconnected from the server");
    } // end exit method

    /************************ Exit Game *************************
     * Handles the exiting of a game. Removes the user's game name 
     * from the clientList and if they are the first to leave the 
     * game, the opponent is alerted to the user leaving the game.
     * Also removes the game from the game list so other users can't
     * attempt to connect to the game.
     */
    private void exitGame(string message)
    {
        // split the message to extract the name and opponent name
        string name = message.Substring(0, message.IndexOf("~"));
        message = message.Substring(message.IndexOf("~") + 1, message.IndexOf("|") - name.Length - 1);
        string opponentName = message;

        if (gameList.ContainsKey(name))
        {
            // remove game from game list if the owner is the one who quits.
            gameList.Remove(name);
        }
        if (clientsList.Contains(opponentName))
        {
            byte[] outStream = Encoding.ASCII.GetBytes("EXIT|");

            TcpClient secondSocket = (TcpClient)clientsList[opponentName];

            NetworkStream sendOpponent = secondSocket.GetStream();
            sendOpponent.Write(outStream, 0, outStream.Length);
            sendOpponent.Flush();
        }
        clientsList.Remove(name);
    } // end exit game method

    /************************ Join Game ******************************
     * Sets the opponent name and adds it to the game in the game list,
     *  Sends a message to the game creator who their opponent is
     *  and the game will then be enabled. 
     */
    private void joinGame(string opponentName)
    {
        gameList[opponentName] = clNo;

        TcpClient socket = (TcpClient)clientsList[opponentName];
        NetworkStream sendOpponent = socket.GetStream();

        byte[] outStream = Encoding.ASCII.GetBytes("JOIN|" + clNo + "|");
        sendOpponent.Write(outStream, 0, outStream.Length);
        sendOpponent.Flush();
    } // end join game method

    /********************** Create Game **********************
     * Adds a game to the game list using the user's name and
     * sets the value to null, meaning it is a joinable game. 
     */
    private void createGame(string command)
    {
        // add game into list
        string player2 = null;
        gameList.Add(clNo, player2);
        Console.WriteLine(clNo + " has created a game " + "command = " + command);
    } // end create game method

    /*********************** List Games ************************
     * The Server sends out each game in the list one at a time
     * and will only send the games out that are joinable aka
     * don't have a player name in the value of the hash table. 
     */
    private void listGames(string command, NetworkStream stream)
    {
        byte[] outStream;

        foreach (DictionaryEntry Item in gameList)
        {
            if (Item.Value == null)
            {
                // prepare message to be sent
                outStream = Encoding.ASCII.GetBytes("LIST|" + Item.Key + "|");

                Console.WriteLine(clNo + " LIST|" + Item.Key + " | ");
                stream.Write(outStream, 0, outStream.Length);
                Thread.Sleep(30); // The messages are sent too fast, must slow it down 
                                  // so that all games will show up on user end.
            }
        }
    } // end list games method

    /*************************** Turn ************************
     * Receives a player's turn then passes that turn to the opponent
     * of said player. Gets the opponent's socket information from
     * the client list, then sends the turn to the opponent.
     */
    private void turn(string opponentName)
    {
        Console.WriteLine("clNo = " + clNo); //name + 4 digits
        Console.WriteLine("Opponent name in turn: " + opponentName); 
        byte[] outstream = Encoding.ASCII.GetBytes("TURN|" + msgContent + "|");

        // gets the opponent's socket info
        TcpClient firstSocket = (TcpClient)clientsList[opponentName];
        NetworkStream sendPlayer = firstSocket.GetStream();

        try
        {
            sendPlayer.Write(outstream, 0, outstream.Length);
            sendPlayer.Flush();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    } // end turn method

    /****************************** In Game Chat *******************************
     * Basically the same as Broadcast, but it only sends messages to the two
     * playes that are in the game and no one else. If there is no opponent
     * in the game, the server will not send the message to anyone but the
     * original sender.
     */
    private void inGameChat(NetworkStream stream, string message)
    {
        // split the message up to get the opponent name and the message to be sent
        string opponent = message.Substring(0, message.IndexOf("~"));
        message = message.Substring(message.IndexOf("~") + 1);
        byte[] outstream = System.Text.Encoding.ASCII.GetBytes("GAMETEXT|" + clNo.Substring(0, clNo.Length - 4) + " says : " + message + "|");

        // Checks to see if an opponent name has been passed if so it sends the message
        if (!opponent.Equals(""))
        {
            // get the opponent's socket info
            TcpClient opponentSocket = (TcpClient)clientsList[opponent];

            NetworkStream sendOpponent = opponentSocket.GetStream();

            // send the opponent the message
            sendOpponent.Write(outstream, 0, outstream.Length);
            sendOpponent.Flush();
        }
        // Send the message originator the message as well
        stream.Write(outstream, 0, outstream.Length);
        stream.Flush();
    } // end in game chat method

    /*********************** Broadcast ***********************
     * This method is for sending the chat to the game lobby to
     * all users in the lobby. Iterates through every item in the
     * client list and sends the message.
     */
    public void broadcast(string msg, string uName, bool flag)
    {
        foreach (DictionaryEntry Item in clientsList)
        {
            if (clientsList.Count > 0)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;

                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true) // means this is a message from a user not the server
                {
                    broadcastBytes = Encoding.ASCII.GetBytes("TEXT|" + uName + " says : " + msg + "|");
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg);
                }
                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }
    }  //end broadcast function

    /*************************** handle ****************************
     * This method handles all of the commands by calling the functions
     * that pertain to each command. This is where the thread is always
     * listening for messages and reacting to each message. 
     */
    private void handle()
    {
        byte[] bytesFrom;
        string dataFromClient = null;
        string cmd;
        string message;
        string[] parsedMessage;

        while ((true))
        {
            try
            {
                NetworkStream stream = clientSocket.GetStream();

                bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                stream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                
                // Get the string from the stream
                dataFromClient = Encoding.ASCII.GetString(bytesFrom);

                if (dataFromClient == "")
                {
                    continue;
                }

                // Parse the message which returns an array
                parsedMessage = parseMessage(dataFromClient);

                // Assign the command and the message from the parsed array
                cmd = parsedMessage[0];
                message = parsedMessage[1];

                if (cmd.Equals("TEXT"))
                {
                    sendText(cmd);
                    msgContent = String.Empty; // need to clear the contents so the messages don't overlap
                }
                else if (cmd.Equals("EXITAPP"))
                {
                    // exits the game and breaks this thread's loop
                    exit();
                    break;
                }
                else if (cmd.Equals("CREATEGAME"))
                {
                    // creates a game
                    createGame(cmd);
                }
                else if (cmd.Equals("LISTGAMES"))
                {
                    //List all available games
                    listGames(cmd, stream);
                }
                else if (cmd.Equals("JOINGAME"))
                {
                    //message is the opponent's name
                    joinGame(message);
                }
                else if (cmd.Equals("INGAME"))
                {
                    inGameChat(stream, message);
                }
                else if (cmd.Equals("GAMEEXIT"))
                {
                    exitGame(message);
                    break;
                }
                else if (cmd.Equals("TURN"))
                {
                    turn(opponentName);
                }
                stream.Flush(); // flush the stream so messages can't overlap
            }
            catch (Exception ex)
            {
                // Find out who crashed and why
                Console.WriteLine("ClientNumber = " + clNo);
                Console.WriteLine(ex);
                break;
            }
        }//end while
    }//end handle
} //end class handleClient

