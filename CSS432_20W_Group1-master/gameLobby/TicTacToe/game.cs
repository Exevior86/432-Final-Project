/* Tic Tac Toe Game 
 * Made by Adam Snyder and Colby Easton
 * CSS 432 Networking
 * 3/12/2020
 * 
 * Description:
 * The game window form and all the logic for playing the game,
 * passing messages to the server, and in game chat. 
 */

using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class game : Form
    {
        short turnCount = 0;
        string readData = null;
        string winner = "";
        bool win = false;
        string opponent;
        string userName;
        int gameNum;
        string gameName;
        string chatName;
        gameLobby gLobby;
        Thread ctThread;
        Random ran = new Random();

        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream = default(NetworkStream);

        /****************************** Game ********************************
         * Initializes a game. Starts by creating a new connection to the server 
         * to receive chat messages, turns, and exit messages. Creates a new name
         * to connect to the server by adding a random number at the end of the users
         * name. Also starts a new thread to handle the incoming messages.
         */
        public game(gameLobby lobby, string name, bool creator, string opponentName)
        {
            byte[] outStream;
            InitializeComponent();
            this.AcceptButton = sendButton;

            // Initial server connection
            clientSocket.Connect("18.216.181.228", 13000);

            // *** Loopback can be uncommented out for testing on local machine ***
            // just make sure you comment out the IP above

            //clientSocket.Connect("127.0.0.1", 13000);

            // Gets the receiving stream
            serverStream = clientSocket.GetStream();

            // Makes a new user name for the game server
            gLobby = lobby;
            userName = name;           
            gameNum = ran.Next(0, 9999);
            opponent = opponentName;
            gameName = userName + gameNum.ToString();

            // Connect to server with nnew user name
            outStream = System.Text.Encoding.ASCII.GetBytes(gameName + "|");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            // Start the new thread
            ctThread = new Thread(getMessage);
            ctThread.Start();

            if (creator)
            {
                // This is for the person that creates the game
                // a small pause to let the other message get through before sending the next message could be unnoticed if a ton of people connect
                Thread.Sleep(200);
                outStream = System.Text.Encoding.ASCII.GetBytes("CREATEGAME|" + userName + "|");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                inGameTextBox.AppendText(Environment.NewLine + "You have created a game!" + Environment.NewLine + "Waiting for another user to join the game.");

                disableButtons();
            }
            else
            {
                // This is for the person that joins a game
                inGameTextBox.AppendText(Environment.NewLine + opponent);
                outStream = System.Text.Encoding.ASCII.GetBytes("JOINGAME" + "|" + opponent + "|");
                serverStream.Write(outStream, 0, outStream.Length);
                disableButtons();
                inGameTextBox.AppendText(Environment.NewLine + "You have joined " + opponent.Substring(0, opponent.Length - 4) + "'s game." + Environment.NewLine + "It is their turn.");
            }
        } // end of Game method

        /*************************** Enable Buttons *******************************
         * This iterates through the buttons and enables them if they have not been
         * pushed yet. Uses the text that the button is set with to determine if it
         * has been pressed.
         */
        private void enableButtons()
        {
            if (A1.Text == "")
                A1.Enabled = true;
            if (A2.Text == "")
                A2.Enabled = true;
            if (A3.Text == "")
                A3.Enabled = true;
            if (B1.Text == "")
                B1.Enabled = true;
            if (B2.Text == "")
                B2.Enabled = true;
            if (B3.Text == "")
                B3.Enabled = true;
            if (C1.Text == "")
                C1.Enabled = true;
            if (C2.Text == "")
                C2.Enabled = true;
            if (C3.Text == "")
                C3.Enabled = true;
        } // end of enableButtons

        /****************************** Disable Buttons *****************************************
         * Disables all buttons so they can't be pressed, doesn't discriminate if text has been
         * set of not.
         */
        private void disableButtons()
        {
            A1.Enabled = false;
            A2.Enabled = false;
            A3.Enabled = false;
            B1.Enabled = false;
            B2.Enabled = false;
            B3.Enabled = false;
            C1.Enabled = false;
            C2.Enabled = false;
            C3.Enabled = false;
        } // end of disableButtons

        /********************************** Button Press *****************************
         * Handles the pressing of a button and checks for a win at the end of each turn.
         */
        private void buttonPress(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int size = 40;
            byte[] outStream;
            b.Font = new Font(b.Font.FontFamily, size);

            // Creator always goes first, He is always X, opponent is O
            // toggles button text based off of turn count.
            if (turnCount % 2 == 0)
            {
                b.ForeColor = Color.Blue;
                b.Text = "X";
            }
            else
            {
                b.ForeColor = Color.Red;
                b.Text = "O";
            }
            // Increments the turn count
            turnCount++;
            // Disables the pressed button, not sure its needed at this point
            b.Enabled = false;
            
            // At the end of the turn disable all buttons and wait until the opponent plays
            disableButtons();

            // send turn to server
            if (win == false)
            {
                outStream = System.Text.Encoding.ASCII.GetBytes("TURN|" + b.Name + "~" + opponent + "|");
                serverStream.Write(outStream, 0, outStream.Length);
                // Check win if there hasn't been a win
                winCondition();
            }
        } // end of buttonPress

        /**************************** Win Condition ********************************* 
         * Checks for a win based on the buttons text and alerts the players who has won.
         */
        private string winCondition()
        {
            //Horizontal
            if (A1.Text == A2.Text && A2.Text == A3.Text && turnCount > 4 && !A1.Text.Equals(""))            
                win = true;            
            else if (B1.Text == B2.Text && B2.Text == B3.Text && turnCount > 4 && !B1.Text.Equals(""))            
                win = true;            
            else if (C1.Text == C2.Text && C2.Text == C3.Text && turnCount > 4 && !C1.Text.Equals(""))
                win = true;            

            //Vertical
            else if (A1.Text == B1.Text && B1.Text == C1.Text && turnCount > 4 && !A1.Text.Equals(""))            
                win = true;            
            else if (A2.Text == B2.Text && B2.Text == C2.Text && turnCount > 4 && !A2.Text.Equals(""))            
                win = true;           
            else if (A3.Text == B3.Text && B3.Text == C3.Text && turnCount > 4 && !A3.Text.Equals(""))            
                win = true;
            
            //Diagonal
            else if (A1.Text == B2.Text && B2.Text == C3.Text && turnCount > 4 && !A1.Text.Equals(""))            
                win = true;           
            else if (A3.Text == B2.Text && B2.Text == C1.Text && turnCount > 4 && !A3.Text.Equals(""))           
                win = true;
            
            // If a win occurs detect which person won, either X or O
            if (win)
            {
                if (turnCount % 2 == 0)
                {
                    winner = "O";
                }
                else { winner = "X"; }
                MessageBox.Show("The winner is " + winner + "!", "Game End");
                //this.Close();
                disableButtons();
            }
            else if (turnCount >= 9 && win == false)
            {
                // Displays a cats game message for ties
                MessageBox.Show("Cats game, there is no winner!");
               // this.Close();
            }
            return winner;
        } // end of winCondtition

        /************************* Exit Button *****************************
        * Closes the form when the button is pressed
        */  
        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        } // end of exitButton

        /************************ Game Closure ****************************
         * Handles the actual closing of the form meaning alt F4, the X button, and the exit button
         * will exit the game properly telling the server the client has quit.         
         */
        private void game_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] outStream;

            outStream = System.Text.Encoding.ASCII.GetBytes("GAMEEXIT|" + gameName + "~" + opponent + "|");
           
            serverStream.Write(outStream, 0, outStream.Length);
            ctThread.Abort();
            gLobby.Show();
        } // end of game_FormClosing

        /**************************** Send Button ***************************
         * Sends the tet the user input into the chat box to the server so the
         * server can broadcast the message to the opponent.
         */
        private void sendButton_Click(object sender, EventArgs e)
        {
            if (!textBox2.Text.Equals(""))
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes("INGAME|" + opponent + "~" + textBox2.Text + "|");
                serverStream.Write(outStream, 0, outStream.Length);
                textBox2.Text = "";
            }
        } // end of sendButton

        /************************** Get Message ***************************
         * Continues to listen for messages from the server. There are only
         * a few messages that the games processes, chat, turns, exit
         * and join.
         */
        private void getMessage()
        {
            string returnData;
            string[] parsedMessage;
            byte[] inStream = new byte[clientSocket.ReceiveBufferSize];
            string command;

            while (true)
            {
                // Reads data from the server's stream
                serverStream.Read(inStream, 0, inStream.Length);
                returnData = System.Text.Encoding.ASCII.GetString(inStream);

                // Calls the parse message to receive the command and message
                parsedMessage = parseMessage(returnData);
                command = parsedMessage[0];
                
                // Set the global variable for chat
                readData = parsedMessage[1];

                // Actually appends user text to the in game text box instead of receiving from the server.
                if (command.Equals("GAMETEXT"))
                {
                    inGameTextBox.AppendText(Environment.NewLine + " >> "  +  readData);
                }
                serverStream.Flush();
            }
        } // end of getMessage

        /********************* Parse Message *****************************
         * Splits a server message into the proper parts. Command is always the first
         * part of the message and the contents of the message follow.
         */
        private string[] parseMessage(string message)
        {
            string command = "";
            string delimiter = "|";
            int nextMsg;
            command = message.Substring(0, message.IndexOf(delimiter));
            nextMsg = command.Length;
            string[] returnValue = new string[2];

            // In game chat messages
            if (command.Equals("GAMETEXT"))
            {
                message = message.Substring(nextMsg + 1, 500);
                message = message.Substring(0, message.IndexOf("|"));
            }

            // User turns
            if (command.Equals("TURN"))
            {
                //longer messages allowed for chat
                message = message.Substring(nextMsg + 1, 2);
                //first arg has to be button where name == message

                // Finds the button that needs to be pressed by its name
                // then updates the button that was pressed from the opponent
                foreach (Control control in Controls)
                {
                    if (control.Name == message)
                    {
                        int size = 40;
                        Button butt = control as Button;
                        butt.Font = new Font(butt.Font.FontFamily, size);

                        if (turnCount % 2 == 0)
                        {
                            butt.Text = "X";
                            butt.Enabled = false;
                        }
                        else
                        {
                            butt.Text = "O";
                            butt.Enabled = false;
                        }
                        // Update turn count to match the opponent
                        turnCount++;

                        // Check to see if someone has won
                        winCondition();

                        // If no one has won yet, allow another turn.
                        if (win == false)
                            enableButtons();
                    }
                }
            } 

            // If the opponent has left, inform this client and remove their name so the server
            // doesn't try to send messages to a person who disconnected.
            if (command.Equals("EXIT"))
            {
                inGameTextBox.AppendText(Environment.NewLine + opponent + " has left the game!");
                disableButtons();
                opponent = "";
            }

            // When a person joins a game it updates the opponent name so the opponent can be
            // sent turns and in game chat, all based off the opponents name.
            if (command.Equals("JOIN"))
            {
                // Have to clip the message so the delimiter will be the next one, not the first one.
                message = message.Substring(nextMsg + 1, 500);
                opponent = message.Substring(0, message.IndexOf("|"));

                inGameTextBox.AppendText(Environment.NewLine + opponent.Substring(0, opponent.Length - 4) + " has joined the game!" + Environment.NewLine + "It is now your turn.");
                // Creator always gets to go first, so when a person joins a game enable their buttons
                enableButtons();
            }
            // Return commands and messages for chat to be handled in the getMessage() method
            returnValue[0] = command;
            returnValue[1] = message;
            return returnValue;
        } // end of parseMessage
    } // end of game form
} // end of windowsFormApp1
