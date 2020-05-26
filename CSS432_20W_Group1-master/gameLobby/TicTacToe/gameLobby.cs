/* Tic Tac Toe Game Lobby
 * Made by Adam Snyder and Colby Easton
 * CSS 432 Networking
 * 3/12/2020
 * 
 * Description:
 * This is the game lobby where a user can login with a user name, 
 * create a game, find games to join, and chat with the other players.
 */

using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class gameLobby : Form
    {
        //Creates socket for client
        TcpClient clientSocket = new TcpClient();

        //gets the stream from gameServer
        NetworkStream serverStream = default(NetworkStream);
        Hashtable gameList = new Hashtable();

        // initalizes a game
        game gameWindow;

        // Set up a client name and an opponent name
        string clientNo;
        string opponentName;

        /*************************** Game Lobby **************************************
         * initializes the lobby and allows only the exit button and the login button the
         * be available before connecting to the server.
         */
        public gameLobby()
        {
            InitializeComponent();
            CreateButton.Hide();
            ListGamesButton.Hide();
            JoinButton.Hide();
            SendButton.Hide();
            this.AcceptButton = SendButton;
        } // end of gameLobby

        /**************************** Get Message ***********************************
         * Continually listen for messages and then parse the messages that are received
         * from the server.
         */
        private void getMessage()
        {
            while (true) // never stop listening
            {
                try
                {
                    serverStream = clientSocket.GetStream();
                    byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                    // Receive the messages from the stream
                    serverStream.Read(inStream, 0, inStream.Length);
                    string returnData = System.Text.Encoding.ASCII.GetString(inStream);

                    // Call the parseMessage command to extract the command and text from the messages
                    parseMessage(returnData);
                }
                catch (Exception ex)
                {
                    textBox1.AppendText(ex.ToString());
                }
            }
        } // end of getMessage

        
        /********************************* Parse Message ******************************
         * Parse the messages received from the server and splits them into a command
         * and a message based on a delimiter.
         */
        private void parseMessage(string message)
        {
            string command = "";
            string delimiter = "|";
            int nextMsg; // holds the index of the start of the next message

            command = message.Substring(0, message.IndexOf(delimiter)); // command is always the first part of a message
            nextMsg = command.Length; // sets the start for the next message
            message = message.Substring(nextMsg + 1, 500); // clip the first part of the message off so the next delimiter will be used accurately
            message = message.Substring(0, message.IndexOf(delimiter));

            if (command.Equals("JOIN"))
            {
                //join game
                //message will be other party. Update value in Hashtable
                gameList[clientNo] = message;
            }
            else if (command.Equals("LIST"))
            {
                //add all items to the list
                listBox1.Items.Add(message);
                listBox1.Sorted = true;
            }
            else if (command.Equals("TEXT"))
            {
                // Puts the text into the chat box for users to see
                textBox1.AppendText(Environment.NewLine + ">> " + message);
            }
        } // end of parseMessage

        /********************************  Send Button ************************************
         * The send button takes the text from the text box and sends it to the server to
         * be broadcast to all the current users logged into the lobby.
         */
        private void SendButton_Click(object sender, EventArgs e)
        {
            if(!textBox2.Text.Equals(""))
            {
                // Get the message
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes("TEXT|" + textBox2.Text + "|");
                // Send the message to the server
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                // Clear the textbox so the message isn't be replayed
                textBox2.Text = "";
            }
        } // end of sendButton

        /*************************** Login Button *************************************
         * Takes the users name they typed in the username textbox and requests a connction
         * with the server, then logs in to the server enabling the create button, list 
         * games button, and join button.
         */
        private void LoginButton_Click_1(object sender, EventArgs e)
        {
            // handle a blank username and don't allow people to have names longer than 16 characters
            if (textBox3.Text.Trim() == "")
            {
                textBox1.Text = "Please enter a Username before logging in";
                return;
            }
            if (textBox3.Text.Length > 16)
            {
                textBox1.Text = "You shall not enter names longer than 16 characters!";
                return;
            }
            // Username is what was typed into the textbox
            clientNo = textBox3.Text;

            try
            {
                // Try to connect to the server
                clientSocket.Connect("18.216.181.228", 13000);

                // *** Loopback can be uncommented out for testing on local machine ***
                // just make sure you comment out the IP above

                //clientSocket.Connect("127.0.0.1", 13000);

                serverStream = clientSocket.GetStream();

                // Send the user name to the server
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox3.Text + "|");

                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                // Create a new thread top handle the client
                Thread ctThread = new Thread(getMessage);
                ctThread.Start();
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.ToString());
            }
            if (clientSocket.Connected)
            {
                // If the client connects successfully, enables button functions
                LoginButton.Hide();
                CreateButton.Show();
                ListGamesButton.Show();
                JoinButton.Show();
                SendButton.Show();
            }
        } // end of LoginButton

        /********************************* Exit Button ***************************************
         * Closes the game and sends a message to the server to remove the client name from the
         * client list so the server won't send any more messages.
         */
        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        } // end of exitButton

        /********************************** Form Closure *************************************
         * Handles the actual closing of the form meaning alt F4, the X button, and the exit button
         * will exit the game properly telling the server the client has quit.
         */
        private void gameLobby_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!clientSocket.Connected)
            {
                Application.Exit();
            }
            else
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes("EXITAPP|");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Close();
                clientSocket.Close();
                Application.Exit();
            }
        } // end of gameLobby_FormClosing

        /********************************** Create Button ***********************************
         * Tells the server a client has created a game and adds the game to the game list
         * allowing other users to join the game and play.
         */
        private void CreateButton_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            gameWindow = new game(this, clientNo, true, "");
            gameWindow.Show();
            this.Hide();
        } // end of CreatButton

        /***************************** List Games *************************************
         * Requests all the games in the game list from the server and recieves them one
         * at a time, but clears the list first so duplicates and full games don't show up.
         */
        private void ListGamesButton_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("LISTGAMES|");
            serverStream.Write(outStream, 0, outStream.Length);
        } // end of ListGamesButton

        /************************************ Join Button *****************************
         * Allows users to joing an game that isn't full and checks to make sure the game
         * isn't full before connecting. Once connected it starts a game for the user and
         * connects to the creators game.
         */
        private void JoinButton_Click(object sender, EventArgs e)
        {
            //Protect against null listbox selection
            try
            {
                opponentName = listBox1.SelectedItem.ToString();

                //Check to see if game has been removed from list
                ListGamesButton_Click(null, null); 
                
                //wait for server to complete update of game list
                Thread.Sleep(300);

                // Lists all items in the game list
                if (listBox1.Items.Contains(opponentName))
                {

                    if (opponentName.Length > 4)
                    {
                        listBox1.Items.Clear();
                        //initialize game window
                        gameWindow = new game(this, clientNo, false, opponentName);
                        gameWindow.Show();
                        listBox1.Items.Clear();
                        this.Hide();
                    }
                }
            }
            catch (Exception j)
            {
                _ = j.StackTrace;
            }
        } // end of JoingButton       
    } // end of gameLobby
} // WindowsFormsApp1
