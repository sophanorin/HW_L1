using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_
{
    public class AsynchronousSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            IPAddress   ipAddress       =   IPAddress.Loopback;
            IPEndPoint  localEndPoint   =   new IPEndPoint(ipAddress, 11000);
            Socket      listener        =   new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");

                    listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
                    
                    allDone.WaitOne(); 
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket      listener     =   (Socket)ar.AsyncState;
            Socket      handler      =   listener.EndAccept(ar);
            StateObject state        =   new StateObject();

            state.workSocket         =   handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String       content   =     String.Empty;

            StateObject  state     =     (StateObject)ar.AsyncState;

            Socket       handler   =     state.workSocket;

            int          bytesRead =     handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));  
                content = state.sb.ToString();
                if (content.Length > 0)
                {  
                    Console.WriteLine("Receive from Client : " + content);
                     
                    Send(handler, "Hello Client");
                }
                else
                { 
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
