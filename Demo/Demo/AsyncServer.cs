
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Device;
using Modbus.Utility;
using System.Timers;
using System.Threading;
using System.Net.Sockets;
using System.IO.Ports;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FtdAdapter;
using Modbus.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

	public class AsyncServer{

			public AsyncServer(){
				    responseData = new List<ResponseData>();



		   	}

				
			    public List<ResponseData> responseData{get; set;}

                private List<Socket> _clients = new List<Socket>();

                public StateObject state;

				public Driver driver;

				public Device device;

			 	public  static  ManualResetEvent  allDone   {get; set;}  


			public void StartListening(  ) {
							allDone = new ManualResetEvent(false);
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[4024];

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer   
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            Console.WriteLine(Dns.GetHostName().ToString());
            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Started listening \n Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
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

			private void AcceptCallback( IAsyncResult ar ) {
							// Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            state = new StateObject();
            state.workSocket = handler;
            _clients.Add(handler);
            Console.WriteLine("Connection has establisted with " + handler.RemoteEndPoint.ToString());
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

					
		     	}

			private void ReadCallback( IAsyncResult ar ) {
							String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                device = new Device();
                // There  might be more data, so store the data received so far.  
               state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));
                
                
                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf("</Device>") > -1)
                {
                    device = Utility.StreamToDeviceModel(content);
                        // All the data has been read from the   
                        // client. Display it on the console.  
                        Console.WriteLine("Request Recieved For Device:"+ device.IPAddress +":"+device.Port);
                    // Echo the data back to the client.  
                    Send(handler, device);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }

					
		     	}

			private void Send( Socket handler,Device data ) {
							 // Convert the string data to byte data using ASCII encoding.  
           driver = new Driver();
            responseData = driver.ModbusTcpMasterReadInputs(data);
            string sendData = Utility.ResponseModelToStream(responseData);
            byte[] byteData = Encoding.ASCII.GetBytes(sendData);


            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

					
		     	}

			private void SendCallback( IAsyncResult ar ) {
							try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Data Sent!", bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

					
		     	}


	}

