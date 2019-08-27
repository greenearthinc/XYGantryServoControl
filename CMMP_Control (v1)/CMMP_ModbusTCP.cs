/****************************************/
/*	Developer: Ben Hope         	    */
/*	ben.hope@festo.com			        */
/*  Festo Motion (CMMP) Modbus TCP		*/
/****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMMP_Control__v1_;
using System.Runtime.InteropServices;

namespace CMMP_ModbusTCP
{
    class Program
    {
       
        static unsafe void Main(string[] args)
        {
 
            int CMMP_Sequence=0;                        //integer for sequence control
            int connectError;
            bool EnableMotionTask = false;

            Festo_Motion CMMP = new Festo_Motion();
            
            
            CMMP.IP_Address = "169.254.77.148";           //Modbus TCP IP Address
            connectError = CMMP.ConnectDrive();
            
          
            if (connectError == 0)
            {
                EnableMotionTask = true;

                do
                {
                    CMMP.UpdateFHPP();
                    Console.SetCursorPosition(1, 0);
                    Console.WriteLine("Actual Position: {0} ", CMMP.ActualPosition);
               

                    // Example of Motion Task Sequencing

                    switch (CMMP_Sequence)
                    {
                        case 0:
                            if (CMMP.SupplyVoltagePresent && !CMMP.ControlFCT_HMI)
                            {
                                CMMP.HMIAccessLocked = true;
                                CMMP.EnableDrive = true;
                                CMMP.Stop = true;
                                CMMP.Halt = true;
                                CMMP_Sequence++;
                            }
                            break;

                        case 1:
                            if (CMMP.DriveEnabled)
                            {
                                //OPM 0 = Record Mode
                                CMMP.OPM = 0;
                                //OPM 1 = Direct Position Mode
                               //CMMP.OPM = 1;     
                                //OPM 5 = Direct Force Mode
                                //CMMP.OPM = 5;
                                CMMP_Sequence++;
                                System.Threading.Thread.Sleep(250);
                            }
                           break;

                        case 2:
                            if (CMMP.Ready)
                            {
                                //Set Force Value for Force Mode (OPM = 5)
                                //CMMP.SetValueForce = 50;
                                //Set Velocity and Position for Position Mode (OPM = 1)
                                //CMMP.SetValueVelocity = 20;
                                //CMMP.SetValuePosition = 10000;
                                CMMP.RecordNumber = 1;
                                System.Threading.Thread.Sleep(250);
                                CMMP_Sequence++;
                            }
                            break;
                        case 3:
                            CMMP.StartTask = true;
                            System.Threading.Thread.Sleep(250);
                            CMMP_Sequence++;
                            break;
                        case 4:
                            if (CMMP.AckStart)
                            {
                                CMMP.StartTask = false;
                                CMMP_Sequence++;
                            }  
                            break;
                        case 5:
                            if (CMMP.MotionComplete)
                            {
                                Console.WriteLine("Motion is Complete");
                                CMMP_Sequence++;
                                CMMP.HMIAccessLocked = false;          
                            }
                            break;
                        case 6:
                            EnableMotionTask = false;
                            break;
                    }
                } while (EnableMotionTask);

                CMMP.CloseDrive();
            }

        }
    }
}
