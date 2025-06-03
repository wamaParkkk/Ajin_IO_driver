using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ajin_IO_driver
{
	public struct TdiValue
	{
		public string[] checkHigh;
		public string[] checkLow;
	}

	public struct TdoValue
	{
		public string[] readDigOut;		
	}

	public class DIOClass
	{
		public static Timer timer = new Timer();

        public static int iModuleID = 0;

		// Digital input data
		public static TdiValue diVal;

		// Digital output data
		public static TdoValue doVal;

		public static bool OpenDevice()
		{
            // Library initialization.
            if (CAXL.AxlOpen(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
			{
				uint uStatus = 0;

				if (CAXD.AxdInfoIsDIOModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
				{
					if ((AXT_EXISTENCE)uStatus == AXT_EXISTENCE.STATUS_EXIST)
					{
						int nModuleCounts = 0;

						if (CAXD.AxdInfoGetModuleCount(ref nModuleCounts) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
						{
							short i = 0;
							int nBoardNo = 0;
							int nModulePos = 0;
							uint uModuleID = 0;
							string strData = "";

							for (i = 0; i < nModuleCounts; i++)
							{
								if (CAXD.AxdInfoGetModule(i, ref nBoardNo, ref nModulePos, ref uModuleID) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
								{
									switch ((AXT_MODULE)uModuleID)
									{
										case AXT_MODULE.AXT_SIO_DI32: strData = String.Format("[{0:D2}:{1:D2}] SIO-DI32", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DO32P: strData = String.Format("[{0:D2}:{1:D2}] SIO-DO32P", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DB32P: strData = String.Format("[{0:D2}:{1:D2}] SIO-DB32P", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DO32T: strData = String.Format("[{0:D2}:{1:D2}] SIO-DO32T", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DB32T: strData = String.Format("[{0:D2}:{1:D2}] SIO-DB32T", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDI32: strData = String.Format("[{0:D2}:{1:D2}] SIO_RDI32", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDO32: strData = String.Format("[{0:D2}:{1:D2}] SIO_RDO32", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDB128MLII: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDB128MLII", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RSIMPLEIOMLII: strData = String.Format("[{0:D2}:{1:D2}] SIO-RSIMPLEIOMLII", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDO16AMLII: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDO16AMLII", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDO16BMLII: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDO16BMLII", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDB96MLII: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDB96MLII", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDO32RTEX: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDO32RTEX", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDI32RTEX: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDI32RTEX", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDB32RTEX: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDB32RTEX", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DI32_P: strData = String.Format("[{0:D2}:{1:D2}] SIO-DI32_P", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_DO32T_P: strData = String.Format("[{0:D2}:{1:D2}] SIO-DO32T_P", nBoardNo, i); break;
										case AXT_MODULE.AXT_SIO_RDB32T: strData = String.Format("[{0:D2}:{1:D2}] SIO-RDB32T", nBoardNo, i); break;
									}

									Global.EventLog(strData);
								}
							}

							//iModuleID = (int)(AXT_MODULE)uModuleID;
							iModuleID = 0;

							io_Init();

							timer_Init();

							return true;
						}
					}
					else
					{
                        Global.EventLog("The module does not exist");

                        //io_null();
                        io_Init();

						return false;
					}
				}
                else
                {
                    Global.EventLog("AxdIsDIOModule() Error!!");
                    return false;
                }
            }
			else
			{
                Global.EventLog("Could not do the open the driver");

                //io_null();
                io_Init();

				return false;
			}

			io_Init();

			return false;
		}

		public static void io_null()
		{
			diVal.checkHigh = new string[16] { null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null};

			diVal.checkLow = new string[16] { null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null};


			// 프로그램 재시작 시 outport의 마지막 상태를 읽기 위함 (Off로 초기화)
			doVal.readDigOut = new string[64];
			for (int lOffset = 0; lOffset < 64; lOffset++)
			{
				doVal.readDigOut[lOffset] = "Off";
			}
		}

		public static void io_Init()
		{
			diVal.checkHigh = new string[16] { "Off", "Off", "Off", "Off", "Off", "Off", "Off", "Off",
			"Off", "Off", "Off", "Off", "Off", "Off", "Off", "Off"};

			diVal.checkLow = new string[16] { "Off", "Off", "Off", "Off", "Off", "Off", "Off", "Off",
			"Off", "Off", "Off", "Off", "Off", "Off", "Off", "Off"};

			
			// 프로그램 재시작 시 outport의 마지막 상태를 읽기 위함
			doVal.readDigOut = new string[64];

			uint upValue = 0;

			for (int lOffset = 0; lOffset < 64; lOffset++)
            {
				CAXD.AxdoReadOutport(lOffset, ref upValue);
				if (upValue == 0)
					doVal.readDigOut[lOffset] = "Off";
				else
					doVal.readDigOut[lOffset] = "On";
			}			
		}

		public static void timer_Init()
        {			
			timer.Interval = 100;
			timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
			timer.Start();
		}

		public static bool SelectHighIndex(int nIndex, uint uValue)
		{
			int nModuleCount = 0;

			CAXD.AxdInfoGetModuleCount(ref nModuleCount);

			if (nModuleCount > 0)
			{
				int nBoardNo = 0;
				int nModulePos = 0;
				uint uModuleID = 0;

				CAXD.AxdInfoGetModule(iModuleID + 1, ref nBoardNo, ref nModulePos, ref uModuleID);

				switch ((AXT_MODULE)uModuleID)
				{
					case AXT_MODULE.AXT_SIO_DO32P:
					case AXT_MODULE.AXT_SIO_DO32T:
					case AXT_MODULE.AXT_SIO_RDO32:					
						CAXD.AxdoWriteOutportBit(iModuleID + 1, nIndex, uValue);
						break;

					default:
						return false;
				}
			}

			return true;
		}

		public static bool SelectHighIndex2(int nIndex, uint uValue)
		{
			int nModuleCount = 0;

			CAXD.AxdInfoGetModuleCount(ref nModuleCount);

			if (nModuleCount > 0)
			{
				int nBoardNo = 0;
				int nModulePos = 0;
				uint uModuleID = 0;

				CAXD.AxdInfoGetModule(iModuleID + 2, ref nBoardNo, ref nModulePos, ref uModuleID);

				switch ((AXT_MODULE)uModuleID)
				{
					case AXT_MODULE.AXT_SIO_DO32P:
					case AXT_MODULE.AXT_SIO_DO32T:
					case AXT_MODULE.AXT_SIO_RDO32:                    
                        CAXD.AxdoWriteOutportBit(iModuleID + 2, nIndex - 32, uValue);
						break;

					default:
						return false;
				}
			}

			return true;
		}

		public static bool SelectLowIndex(int nIndex, uint uValue)
		{
			int nModuleCount = 0;

			CAXD.AxdInfoGetModuleCount(ref nModuleCount);

			if (nModuleCount > 0)
			{
				int nBoardNo = 0;
				int nModulePos = 0;
				uint uModuleID = 0;

				CAXD.AxdInfoGetModule(iModuleID, ref nBoardNo, ref nModulePos, ref uModuleID);

				switch ((AXT_MODULE)uModuleID)
				{
					case AXT_MODULE.AXT_SIO_DO32P:
					case AXT_MODULE.AXT_SIO_DO32T:
					case AXT_MODULE.AXT_SIO_RDO32:
						CAXD.AxdoWriteOutportBit(iModuleID, nIndex + 16, uValue);
						break;
					case AXT_MODULE.AXT_SIO_DB32P:
					case AXT_MODULE.AXT_SIO_DB32T:
					case AXT_MODULE.AXT_SIO_RDB128MLII:
						CAXD.AxdoWriteOutportBit(iModuleID, nIndex, uValue);
						break;

					default:
						return false;
				}
			}

			return true;
		}

		private static void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			short nIndex = 0;
			uint uDataHigh = 0;
			uint uDataLow = 0;
			uint uFlagHigh = 0;
			uint uFlagLow = 0;
			int nBoardNo = 0;
			int nModulePos = 0;
			uint uModuleID = 0;

			CAXD.AxdInfoGetModule(iModuleID, ref nBoardNo, ref nModulePos, ref uModuleID);

			switch ((AXT_MODULE)uModuleID)
			{
				case AXT_MODULE.AXT_SIO_DI32:
				case AXT_MODULE.AXT_SIO_RDI32:
				case AXT_MODULE.AXT_SIO_RSIMPLEIOMLII:
				case AXT_MODULE.AXT_SIO_RDO16AMLII:
				case AXT_MODULE.AXT_SIO_RDO16BMLII:
				case AXT_MODULE.AXT_SIO_DI32_P:
				case AXT_MODULE.AXT_SIO_RDI32RTEX:
					//++
					// Read inputting signal in WORD
					CAXD.AxdiReadInportWord(iModuleID, 0, ref uDataHigh);
					CAXD.AxdiReadInportWord(iModuleID, 1, ref uDataLow);

					for (nIndex = 0; nIndex < 16; nIndex++)
					{
						// Verify the last bit value of data read
						uFlagHigh = uDataHigh & 0x0001;
						uFlagLow = uDataLow & 0x0001;

						// Shift rightward by bit by bit
						uDataHigh = uDataHigh >> 1;
						uDataLow = uDataLow >> 1;

						// Updat bit value in control
						if (uFlagHigh == 1)
							diVal.checkHigh[nIndex] = "On";
						else
							diVal.checkHigh[nIndex] = "Off";

						if (uFlagLow == 1)
							diVal.checkLow[nIndex] = "On";
						else
							diVal.checkLow[nIndex] = "Off";
					}
					break;

				case AXT_MODULE.AXT_SIO_DB32P:
				case AXT_MODULE.AXT_SIO_DB32T:
				case AXT_MODULE.AXT_SIO_RDB32T:
				case AXT_MODULE.AXT_SIO_RDB32RTEX:
				case AXT_MODULE.AXT_SIO_RDB96MLII:
				case AXT_MODULE.AXT_SIO_RDB128MLII:
					//++
					// Read inputting signal in WORD
					CAXD.AxdiReadInportWord(iModuleID, 0, ref uDataHigh);

					for (nIndex = 0; nIndex < 16; nIndex++)
					{
						// Verify the last bit value of data read
						uFlagHigh = uDataHigh & 0x0001;

						// Shift rightward by bit by bit
						uDataHigh = uDataHigh >> 1;

						// Updat bit value in control
						if (uFlagHigh == 1)
							diVal.checkHigh[nIndex] = "On";
						else
							diVal.checkHigh[nIndex] = "Off";
					}
					break;
			}
		}

		public static void CloseDevice()
		{
			timer.Stop();
			timer.Close();

			CAXL.AxlClose();

            Global.EventLog("Close the device");
        }
	}
}
