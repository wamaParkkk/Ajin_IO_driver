using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ajin_IO_driver
{    
    public class AIOClass
	{
        private struct SAOChannel
        {
            public bool m_bUse;
            public short m_nWave;
            public short m_nPeriod;
            public short m_nVRange;
            public double m_dVoltage;
            public short m_nIndex;
        }

        static SAOChannel[] m_arrCAOChannel;
        public static int m_nChannelCnts = 0;

        public static Timer timer = new Timer();

        public static bool OpenDevice()
		{			
			// Initialize library 
			if (CAXL.AxlOpen(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				uint uStatus = 0;

                // The inspection to have AIO module
                if (CAXA.AxaInfoIsAIOModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				{
					if ((AXT_EXISTENCE)uStatus == AXT_EXISTENCE.STATUS_EXIST)
					{
						int nModuleCounts = 0;

                        // Get the cardinality of the module
                        if (CAXA.AxaInfoGetModuleCount(ref nModuleCounts) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
						{
                            InitControl();
							return true;
						}
                        else
                        {
                            Global.EventLog("Failed in geting the cardinality of the module");
                            return false;
                        }
                    }
                    else
                    {
                        Global.EventLog("The module does not exist");
                        return false;
                    }
                }
                else
                {
                    Global.EventLog("AxaIsAOModule() Error!!");
                    return false;
                }
            }
            else
            {
                Global.EventLog("Could not do the open the driver");
                return false;
            }            			
		}

        private static void InitControl()
        {
            // Get the information of channels
            CAXA.AxaoInfoGetChannelCount(ref m_nChannelCnts);

            if (m_nChannelCnts > 0)
            {
                m_arrCAOChannel = new SAOChannel[m_nChannelCnts];

                for (int i = 0; i < m_arrCAOChannel.Length; ++i)
                {
                    m_arrCAOChannel[i].m_nPeriod = 100;
                    m_arrCAOChannel[i].m_nVRange = 0;
                }
            }

            for (int i = 0; i < m_nChannelCnts; ++i)
            {
                CAXA.AxaoWriteVoltage(i, 0);                
            }
        }

        public static void ReleaseDeviceInformation()
        {
            if (CAXL.AxlIsOpened() == (int)AXT_BOOLEAN.TRUE)
            {
                for (int i = 0; i < m_nChannelCnts; i++)
                {
                    CAXA.AxaoWriteVoltage(i, 0);
                }

                CAXL.AxlClose();

                Global.EventLog("Close the device");
            }
        }

        public static void WriteVoltage(int nChannelNo, double dVolt)
        {
            try
            {
                if (dVolt < -10.0 || dVolt > 10.0)
                    return;

                uint nStatus = 0; 
                CAXA.AxaInfoIsAIOModule(ref nStatus);
                if (((CAXL.AxlIsOpened() & 1) != 0) && ((AXT_EXISTENCE)nStatus == AXT_EXISTENCE.STATUS_EXIST))
                {
                    // Set Volt range
                    CAXA.AxaoSetRange(nChannelNo, -10, 10);

                    // Output Voltage
                    CAXA.AxaoWriteVoltage(nChannelNo, dVolt);
                }
                else
                {
                    Global.EventLog("AxaIsAOModule() Error!!");
                }
            }
            catch (Exception e)
            {
                Global.EventLog(e.Message);
            }
        }        

        public static double ReadVoltage(int nChannelNo)
        {
            try
            {
                double dpVolt = 0.0;
                CAXA.AxaiSwReadVoltage(nChannelNo, ref dpVolt);
                return dpVolt;
            }
            catch (Exception e)
            {
                Global.EventLog(e.Message);
                return -9999;
            }            
        }
    }
}
