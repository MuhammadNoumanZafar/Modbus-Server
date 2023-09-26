
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

	public class Connection{

			public Connection(){
		   	}

				private Device device;


			 	private  TcpClient  tcpclient   ;  

			 	private  ModbusIpMaster  modbusMaster   ;  

			public void Connect( string IPAddress,int Port ) {
							 tcpclient = new TcpClient(IPAddress, Port);
            modbusMaster = ModbusIpMaster.CreateIp(tcpclient);
            

					
		     	}

			public object getData( locationType locType,ushort startAddress,ushort numofInputs ) {
							switch (locType)
            {
                case locationType.Coils:
                    var coils = modbusMaster.ReadCoils(startAddress, numofInputs);
                    return coils;
                case locationType.Holding_Registers:
                    var Holding_registers = modbusMaster.ReadHoldingRegisters(startAddress, numofInputs);
                    return Holding_registers;
                case locationType.Input_Registers:
                    var inputs = modbusMaster.ReadInputRegisters(startAddress, numofInputs);
                    return inputs;
                case locationType.Inputs:
                    var inputRegister = modbusMaster.ReadInputs(startAddress, numofInputs);
                    return inputRegister;
                default:
                    throw new Exception("Type Mismatch Exception");
            }

					
		     	}

			public void Close(  ) {
							modbusMaster.Dispose();

					
		     	}

    
}

public enum locationType
{
    Coils, Holding_Registers, Inputs, Input_Registers
}
