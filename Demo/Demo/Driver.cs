
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

	public class Driver{

			public Driver(){



				    responseData = new List<ResponseData>();
		   	}

				private Device device;

				private Connection connection;

				private IPValidator validate;

				
					public List<ResponseData> responseData{get; set;}
				

			 	public  string  Name   {get; set;}  

			 	public  string  Description   {get; set;}  


			 	private  string  iP   ;  

			 	private  int  port   ;  

			 	private  BinaryFormatter  bf   ;  

			 	private  MemoryStream  ms   ;  

			public List<ResponseData> ModbusTcpMasterReadInputs( Device Device ) {
							connection = new Connection();
            validate = new IPValidator();
            device = new Device();
		bf = new BinaryFormatter();
            ms = new MemoryStream();
            bool valid = validate.ValidateIP(Device.IPAddress);
            if (valid == true)
            {
               // log4net.Config.XmlConfigurator.Configure();
            
            
                iP = Device.IPAddress;
                port = Device.Port;
                 try
                 {
                
                    connection.Connect(iP, port);
                      foreach (Destination des in Device.destination)
                        {
                         foreach (Inputs input in des.Inputs)
                            {
                               ResponseData response = new ResponseData();
                               ushort startAddress = input.StartingAddress;
                               ushort numInputs = input.NumofInput;
                               string location = des.Location;



                               object data = connection.getData((locationType)Enum.Parse(typeof(locationType), location.ToString()), startAddress, numInputs);
    
                                //bf.Serialize(ms, data);
                              string senddata = Utility.ObjectToString(data);
                        
                              response.RequestID = iP + port.ToString() + location + startAddress.ToString() + numInputs.ToString();
                              response.Type = location;
                              response.Data = senddata;
                       
                              responseData.Add(response);
                              Console.WriteLine("Type " + location + "  Data:" + data);

                            }
                         }
                
                         connection.Close();
                           //handle.Shutdown(SocketShutdown.Both);
                          //handle.Close();
               

                     }
                     catch (Exception err)
                     {
                       ResponseData response = new ResponseData();
                       string data = "Connection Cannot be established because Machine Refuse it ";
                       Console.WriteLine(data);
                       response.RequestID = iP + port.ToString();
                       response.Type = "No Location could be accessible - Connection Missing";
                       response.Data = data;

                       responseData.Add(response);
             
                      }
                 }
                else
                {
                ResponseData response = new ResponseData();
                string data = "No Such IP exist";
                response.RequestID = iP + port.ToString();
                response.Type = "No Location could be accessible - Connection Missing";
                response.Data = data;

                responseData.Add(response);
                Console.WriteLine("invalid IP found" + Device.IPAddress);
                }
             return responseData;

					
		     	}


	}

