using System;
using System.Collections.Generic;

namespace PQAnalyze.data
{
    class DWDataReader
    {
        private DWDataReaderLib.DWFileinfo dwFileInfo;
        internal DWDataReaderLib.DWFileinfo DwFileInfo
        {
            get { return dwFileInfo; }
            set { dwFileInfo = value; }
        }

        private Int64 dwChannelCount;
        public Int64 DwChannelCount//获取信道个数
        {
            get { return dwChannelCount; }
            set { dwChannelCount = value; }
        }

        private DWDataReaderLib.DWChannel[] dwChannelList;
        internal DWDataReaderLib.DWChannel[] DwChannelList//获取频道的结构体数组
        {
            get { return dwChannelList; }
            set { dwChannelList = value; }
        }

        private DWDataReaderLib.DWChannelData[] dwChannelData;
        internal DWDataReaderLib.DWChannelData[] DwChannelData
        {
            get { return dwChannelData; }
            set { dwChannelData = value; }
        }

        private MainWindow mainwindow;
        public MainWindow Mainwindow
        {
            get { return mainwindow; }
            set { mainwindow = value; }
        }

        private List<string> channel_list = new List<string> { "U_L1_H", "U_L2_H", "U_L3_H", "I_L1_H", "I_L2_H", "I_L3_H", "U_L1_IH", "U_L2_IH", "U_L3_IH",
        "I_L1_IH", "I_L2_IH", "I_L3_IH", "THD_U_L1", "THD_U_L2", "THD_U_L3", "U_rms_L1", "U_rms_L2", "U_rms_L3", "Pst_L1", "Pst_L2", "Pst_L3", "Plt_L1", "Plt_L2",
        "Plt_L3", "u2", "P_H1", "Q_H1", "S_H1", "PF", "Frequency"};  //用来检查，判断有无channel_list里的字段名

        public DWDataReader()
        {
            DWDataReaderLib.DWInit();
        }

        public bool DWDataOpenFile(string dxdFilePath)
        {
            if (DWDataReaderLib.DWOpenDataFile(dxdFilePath, ref dwFileInfo) != DWDataReaderLib.DWStatus.DWSTAT_OK)
                return false;
            dwChannelCount = DWDataReaderLib.DWGetChannelListCount();//获取字段个数
            dwChannelList = new DWDataReaderLib.DWChannel[dwChannelCount];//获取这么多字段个数长度的结构体数组
            DWDataReaderLib.DWGetChannelList(dwChannelList);

            Int64 numberOfSamples = 0;
            dwChannelData = new DWDataReaderLib.DWChannelData[dwChannelCount];
            for (int i = 0; i < dwChannelCount; i++)
            {
                if (!channel_list.Contains(dwChannelList[i].name))
                    continue;

                Console.WriteLine(dwChannelList[i].name + ": " + i.ToString());

                //mainform.ControlProgressBar();自己注释掉的
                numberOfSamples = DWDataReaderLib.DWGetScaledSamplesCount(dwChannelList[i].index);
                //jump if no data
                if (numberOfSamples <= 0)
                    continue;
                dwChannelData[i].index = i;//只有满足以上两个条件，index才不为0
                double[] data;
                double[][] array_data;
                double[] timeStamp = new double[numberOfSamples];

                if (dwChannelList[i].array_size > 1)
                {
                    data = new double[dwChannelList[i].array_size * numberOfSamples];
                    DWDataReaderLib.DWGetScaledSamples(dwChannelList[i].index, 0, (int)numberOfSamples * dwChannelList[i].array_size, data, timeStamp);

                    dwChannelData[i].isArray = true;//dwChannelList里array_size大于1的，dwChannelData才是数组
                    array_data = new double[dwChannelList[i].array_size][];
                    for (int j = 0; j < dwChannelList[i].array_size; j++)
                    {
                        array_data[j] = new double[numberOfSamples];
                        for (int k = 0; k < numberOfSamples; k++)
                        {
                            array_data[j][k] = data[j + k * dwChannelList[i].array_size];
                        }
                    }
                    dwChannelData[i].array_data = array_data;
                }
                else
                {
                    dwChannelData[i].isArray = false;
                    data = new double[numberOfSamples];
                    DWDataReaderLib.DWGetScaledSamples(dwChannelList[i].index, 0, (int)numberOfSamples, data, timeStamp);
                    dwChannelData[i].data = data;
                }

            }

            return true;
        }

        public bool DWDataCloseFile()
        {
            return (DWDataReaderLib.DWCloseDataFile() == DWDataReaderLib.DWStatus.DWSTAT_OK);
        }


        ~DWDataReader()
        {
            //DataReaderLib.DWCloseDataFile();
            DWDataReaderLib.DWDeInit();
        }
    }
}
