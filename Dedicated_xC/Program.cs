﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace Dedicated_xC
{
    class Program
    {
        private static TcpClient client;
        static void Main(string[] args)
        {
            Console.WriteLine("Version 1.5");
            Console.Write("Ip : ");
            string ip = Console.ReadLine();
            Console.Write("Port : ");
            string port = Console.ReadLine();
            Start(ip, port);

        }

        public static void Start(string ip, string port)
        {
            Console.WriteLine("Waiting for the server");
            bool State = true;
            while (State)
            {
               
                try
                {
                    int Port = Convert.ToInt32(port);
                    client = new TcpClient(ip, Port);
                    Console.WriteLine("Connected");
                    Thread msgSend = new Thread(MsgHandler);
                    msgSend.Start();
                    State = HandlingSteam();
                }
                catch (SocketException e)
                {
                    Console.Write(".");
                    string _Exception = Convert.ToString(e);
                }
                Thread.Sleep(10000);
            }

        }


        public static bool HandlingSteam()
        {
            NetworkStream clientStream = client.GetStream();
            byte[] _receivdata = new byte[4096];
            ASCIIEncoding encoder = new ASCIIEncoding();
            int bytesRead = 0;
            ///////////////////////////////////
            ///////Wait return from Client:AkA.infected client
            bool x = true;
            bool State = false;
            while (x)
            {
                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(_receivdata, 0, 4096);
                    //message has successfully been received
                    string ReceivData = encoder.GetString(_receivdata, 0, bytesRead);
                    switch (ReceivData)
                    {
                        case "" :
                            break;
                        case "ConnectionClose":
                            client.GetStream().Close();
                            client.Close();
                            x = false;
                            State = false;
                            break;
                        case "ConnectionRetry":
                            x = false;
                            State = true;
                            break;
                        case "/FileSend":
                            Console.WriteLine("Receiving File ....");
                            FileReceiv();
                            break;
                        default :
                            Console.WriteLine("Serveur : " + ReceivData);
                            break;
                    }

                }
                catch (Exception ex)
                {
                    //a socket error has occured
                    Console.WriteLine(ex);
                    x = false;
                    State = true;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client Disconnected ...");
                    Thread.Sleep(5000);
                }
            }
            return State;
             
        }

        public static void MsgHandler()
        {
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            string msg;
            while (true)
            {
                msg = Console.ReadLine();
                if (msg != "")
                {
                    byte[] buffer = encoder.GetBytes(msg);
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }
                Thread.Sleep(250);
            }
        }

        public static void FileReceiv()
        {
            byte[] RecData = new byte[4096];
            int RecBytes;
            NetworkStream clientStream = client.GetStream();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/SendFile";
            try
            {
                int totalrecbytes = 0;
                FileStream Fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                while ((RecBytes = clientStream.Read(RecData, 0, RecData.Length)) > 0)
                {
                    Fs.Write(RecData, 0, RecBytes);
                    totalrecbytes += RecBytes;
                }
                Fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Write(" Done");
        }




    }
}
