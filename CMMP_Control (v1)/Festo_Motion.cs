using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections;
using System.Net.Sockets;
using Modbus.Device;

namespace CMMP_Control__v1_
{
    class Festo_Motion
    {

        public String IP_Address;

        private bool OperationModeB1, OperationModeB2, ControlModeB1, ControlModeB2;

        //FHPP OUT
        public Boolean EnableDrive;
        public Boolean Stop;
        public Boolean Brake;
        public Boolean ResetFault;
        public Boolean HMIAccessLocked;
        public Int16 OPM;
        public Boolean Halt;
        public Boolean StartTask;
        public Boolean StartHoming;
        public Boolean JogPos;
        public Boolean JogNeg;
        public Boolean ClearRemainingPosition;
        public Boolean AbsoluteRelative;
        public byte RecordNumber;

        public byte SetValueVelocity;
        public Int32 SetValuePosition;
       // public Int16 SetValueForceRamp;
        public ushort SetValueForce;

        //FHPP IN
        public Boolean DriveEnabled;
        public Boolean Ready;
        public Boolean Warning;
        public Boolean Fault;
        public Boolean SupplyVoltagePresent;
        public Boolean ControlFCT_HMI;
        public Int16 StateOPM;
        public Boolean HaltActive;
        public Boolean AckStart;
        public bool MotionComplete;
        public Boolean DriveIsMoving;
        public Boolean DragError;
        public Boolean StandstillControl;
        public Boolean DriveIsReferenced;
        public byte ActualRecordNumber;
        public Boolean RC1;
        public Boolean RCC;
        public Boolean FNUM1;
        public Boolean FNUM2;
        public Boolean FUNC;

        public Int32 ActualPosition;
        public byte ActualForce;
        public byte ActualVelocity;
        ModbusIpMaster master;
        TcpClient client;

        ushort[] FHPP_OUT = {0,0,0,0};

        public unsafe int ConnectDrive()
        {

           

            try
            {
                client = new TcpClient(IP_Address, 502);
                master = ModbusIpMaster.CreateIp(client);
                return 0;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                System.Console.WriteLine(e.Message);
                return (int)e.NativeErrorCode;
            }


    
          
        }

        public unsafe void CloseDrive()
        {
            client.Close();
        }

        private static bool GetBit(ushort bits, int offset)
        {
            return (bits & (1 << offset)) != 0;
        }

        private static ushort SetBit(ushort b, int bitNumber, bool state)
        {
            if (bitNumber < 1 || bitNumber > 16)
                throw new ArgumentOutOfRangeException("bitNumber", "Must be 1 - 16");

           if (state)
            {
                b = (ushort)(b | (1 << (bitNumber - 1)));
            }
            else
            {
                b = (ushort)(b & ~(1 << (bitNumber - 1)));
            }

            return b;   
        }

        public unsafe void UpdateFHPP()
        {

            //Mapping of all FHPP Data to signals. See Manual for more information (https://www.festo.com/net/en-ca_ca/SupportPortal/Downloads/385511/403818/555696g1.pdf)

            //FHPP IN    


            ModbusIpMaster master = ModbusIpMaster.CreateIp(client);


                ushort[] FHPP_IN = master.ReadHoldingRegisters(0, 4);

                DriveEnabled = GetBit(FHPP_IN[0], 8);
                Ready = GetBit(FHPP_IN[0], 9); ;
                Warning = GetBit(FHPP_IN[0], 10); ;
                Fault = GetBit(FHPP_IN[0], 11); ;
                SupplyVoltagePresent = GetBit(FHPP_IN[0], 12);
                ControlFCT_HMI = GetBit(FHPP_IN[0], 13);
                OperationModeB1 = GetBit(FHPP_IN[0], 14);
                OperationModeB2 = GetBit(FHPP_IN[0], 15);
                HaltActive = GetBit(FHPP_IN[0], 0);
                AckStart = GetBit(FHPP_IN[0], 1);
                MotionComplete = GetBit(FHPP_IN[0], 2);
                DriveIsMoving = GetBit(FHPP_IN[0], 4);
                DragError = GetBit(FHPP_IN[0], 5);
                StandstillControl = GetBit(FHPP_IN[0], 6);
                DriveIsReferenced = GetBit(FHPP_IN[0], 7);
                ControlModeB1 = GetBit(FHPP_IN[1], 9);
                ControlModeB2 = GetBit(FHPP_IN[1], 10);

                if (!OperationModeB1 && !OperationModeB2)
                {
                    StateOPM = 0;
                }
                else if (OperationModeB1 && !OperationModeB2 && !ControlModeB1 && !ControlModeB2)
                {
                    StateOPM = 1;
                }
                else if (OperationModeB1 && !OperationModeB2 && !ControlModeB1 && ControlModeB2)
                {
                    StateOPM = 5;
                }
                else if (OperationModeB1 && !OperationModeB2 && ControlModeB1 && !ControlModeB2)
                {
                    StateOPM = 9;
                }


                byte[] ActualPositionHigh = BitConverter.GetBytes(FHPP_IN[2]);
                byte[] ActualPositionLow = BitConverter.GetBytes(FHPP_IN[3]);
                byte[] ActualPositionInt32 = new byte[4];

                ActualPositionInt32[1] = ActualPositionLow[1];
                ActualPositionInt32[0] = ActualPositionLow[0];
                ActualPositionInt32[3] = ActualPositionHigh[1];
                ActualPositionInt32[2] = ActualPositionHigh[0];

                ActualPosition = BitConverter.ToInt32(ActualPositionInt32, 0);

                byte[] ActualByte = BitConverter.GetBytes(FHPP_IN[1]);


                if (StateOPM == 0)
                {
                    ActualRecordNumber = ActualByte[1];
                    RC1 = GetBit(FHPP_IN[1], 8);
                    RCC = GetBit(FHPP_IN[1], 9);
                    FNUM1 = GetBit(FHPP_IN[1], 11);
                    FNUM2 = GetBit(FHPP_IN[1], 12);
                    FUNC = GetBit(FHPP_IN[1], 15);
                    
                }
                else if (StateOPM == 5)
                {
                    ActualForce = ActualByte[0];
                }
                else if (StateOPM == 1)
                {
                    ActualVelocity = ActualByte[0];
                }
            

            //FHPP OUT
            


               
                byte[] FHPP_3_Array = new byte[2];

                
            
                FHPP_OUT[1] = SetValueVelocity;

                if (OPM == 0)
                {
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 15, false);
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 16, false);
                    FHPP_3_Array[1] = RecordNumber;
                    FHPP_OUT[1] = BitConverter.ToUInt16(FHPP_3_Array, 0);
                }
                else if (OPM == 1)
                {
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 15, true);
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 16, false);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 10, false);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 11, false);
                    FHPP_3_Array[0] = SetValueVelocity;
                    FHPP_3_Array[1] = 0;
                    FHPP_OUT[1] = SetValueVelocity;

                    byte[] SetPositionBytes = BitConverter.GetBytes(SetValuePosition);
                    FHPP_OUT[3] = BitConverter.ToUInt16(SetPositionBytes, 0);
                    FHPP_OUT[2] = BitConverter.ToUInt16(SetPositionBytes, 2);
                }
                else if (OPM == 5)
                {
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 15, true);
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 16, false);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 10, true);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 11, false);

                    FHPP_OUT[3] = SetValueForce;
                    FHPP_OUT[2] = 0;
                }
                else if (OPM == 6)
                {
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 15, true);
                    FHPP_OUT[0] = SetBit(FHPP_OUT[0], 16, false);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 10, false);
                    FHPP_OUT[1] = SetBit(FHPP_OUT[1], 11, true);
                }




            FHPP_OUT[0] = SetBit(FHPP_OUT[0], 9, EnableDrive);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 10, Stop);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 11, Brake);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 12, ResetFault);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 14, HMIAccessLocked);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 1, Halt);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 2, StartTask);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 3, StartHoming);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 4, JogPos);
                FHPP_OUT[0] = SetBit(FHPP_OUT[0], 5, JogNeg);

               

              

            try
            {
                master.WriteMultipleRegisters(0, FHPP_OUT);
            }
            catch (Modbus.SlaveException e)
            {
                System.Console.WriteLine(e.Message);
                if (e.SlaveExceptionCode == 1)
                {
                    throw new Modbus.SlaveException("Exception 1 (Illegal Function): The function code received in the query is not an allowable action for the slave.  This may be because the function code is only applicable to newer devices, and was not implemented in the unit selected.");
                }
                else if (e.SlaveExceptionCode == 2)
                {
                    throw new Modbus.SlaveException("Exception 2 (Illegal Data Address): The data address received in the query is not an allowable address for the slave. More specifically, the combination of reference number and transfer length is invalid.");
                }
                else if (e.SlaveExceptionCode == 3)
                {
                    throw new Modbus.SlaveException("Exception 3 (Illegal Data Value): A value contained in the query data field is not an allowable value for server (or slave). This indicates a fault in the structure of remainder of a complex request, such as that the implied length is incorrect");
                }
                else if (e.SlaveExceptionCode == 4)
                {
                    throw new Modbus.SlaveException("Exception 4 (Slave Device Failure): An unrecoverable error occurred while the slave was attempting to perform the requested action.");
                }
               

            }



        }

    }

    }

   


