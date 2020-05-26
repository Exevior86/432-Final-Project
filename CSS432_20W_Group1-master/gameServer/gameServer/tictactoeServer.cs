/* Tic Tac Toe Server
 * Made by Adam Snyder and Colby Easton
 * CSS 432 Networking
 * 3/12/2020
 * 
 * Description:
 * This class is just to accept connections and create threads
 * for the class that handles the clients.
 */

using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;


namespace gameServer
{
    class tictactoeServer
    {
        public static Hashtable clientsList = new Hashtable();
        public static Hashtable gList = new Hashtable();


        static void Main(string[] args)
        {
            // Set up the connection
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("172.31.18.58");

            // *** Loopback can be uncommented out for testing on local machine ***
            // just make sure you comment out the IP above

            //IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // get the socket information
            TcpListener serverSocket = new TcpListener(localAddr, port);
            TcpClient clientSocket = default(TcpClient);
         
            // start listening for conenctions
            serverSocket.Start();
            Console.WriteLine("Tic Tac Toe Server Started ....");

            // Always listen for incoming connections and when they are successful 
            // creates a new thread for the incoming connection
            while ((true))
            {
                try
                {
                    // Accepts the user's connection
                    clientSocket = serverSocket.AcceptTcpClient();
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    string dataFromClient = null;

                    NetworkStream stream = clientSocket.GetStream();
                    stream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("|"));
                    Console.WriteLine("dataFromClient   =  " + dataFromClient);

                    // If there is a duplicate user in the client list, send an error message
                    // and continue listening but don't create the thread
                    if (clientsList.ContainsKey(dataFromClient))
                    {
                        bytesFrom = System.Text.Encoding.ASCII.GetBytes("ERROR|" + dataFromClient + " That user name has been taken!|");
                        stream.Write(bytesFrom, 0, bytesFrom.Length);
                        stream.Flush(); // clear stream to make sure messages can't overlap
                        continue;
                    }

                    // Add the user's name and socket information to the clientList
                    clientsList.Add(dataFromClient, clientSocket);

                    // log the user on server side console
                    Console.WriteLine(dataFromClient + " joined chat room ");

                    // create a new handleClient then start the thread
                    handleClient client = new handleClient();
                    client.startClient(clientSocket, dataFromClient, clientsList, gList);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString()); // capture any errors that may occur and keeps server 
                                                     // running in case of an error (hopefully)
                }
            }
        }

    }//end Main class


}