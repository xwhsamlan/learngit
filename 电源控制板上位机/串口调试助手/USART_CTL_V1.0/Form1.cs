using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USART_CTL_V1._0
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");                   //串口接收编码
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }


        private void 端口_Click(object sender, EventArgs e)
        {

        }

        private void 接收格式_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < 35; i++)
            {
                comboBox1.Items.Add("COM" + i.ToString());       //添加串口到下拉列表
            }
            comboBox1.Text = "COM29";                            //串口默认选项
            comboBox2.Text = "9600";
        }

        /// <summary>
        /// 打开串口 按键函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;                    //端口号
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);   //波特率
                serialPort1.Open();                                       //打开串口
                button1.Enabled = false;
                button2.Enabled = true;
                //timer1.Enabled = true;
            }
            catch
            {
                MessageBox.Show("端口错误", "错误");
            }
        }

        /// <summary>
        /// 关闭串口 按键函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();                                      //关闭串口        
                button1.Enabled = true;
                button2.Enabled = false;
                timer1.Enabled = false;
            }
            catch
            {

            }
        }


        /// <summary>
        /// 发送 按键函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];                                            //单字节发数据     
            if (serialPort1.IsOpen)
            {
                if (textBox2.Text != "")
                {
                    if (!radioButton1.Checked)
                    {
                        try
                        {
                            serialPort1.Write(textBox2.Text);                      //字符串写入

                            //serialPort1.WriteLine();                             //换行形式
                        }
                        catch
                        {
                            MessageBox.Show("串口数据写入错误", "错误");
                        }
                    }
                    else                                                           //数据模式
                    {
                        try                                                        //如果此时用户输入字符串中含有非法字符（字母，汉字，符号等等，try，catch块可以捕捉并提示）
                        {
                            for (int i = 0; i < (textBox2.Text.Length - textBox2.Text.Length % 2) / 2; i++)//转换偶数个
                            {
                                Data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);           //转换
                                serialPort1.Write(Data, 0, 1);
                            }
                            if (textBox2.Text.Length % 2 != 0)
                            {
                                Data[0] = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);//单独处理最后一个字符
                                serialPort1.Write(Data, 0, 1);                     //写入
                            }
                            //Data = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);
                            //  }
                        }
                        catch
                        {
                            MessageBox.Show("数据转换错误，请输入数字。", "错误");
                        }
                    }
                }
            }
        }

        //定义接受到的数据的缓存字符
        string BufferData;
        int Temp_cnt_num = 1;

        //响应标志
        bool ACK_Flag = false;
        bool ACK_Flag_2 = false;
        bool Set_Value_Flag = false;
        bool Start_Work_Flag = false;

        /// <summary>
        /// 串口接收处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (!radioButton3.Checked)
            {
                try
                {
                    //textBox1.AppendText(serialPort1.ReadExisting());        //串口类会自动处理汉字，所以不需要特别转换
                    BufferData = serialPort1.ReadExisting();
                    BufferData = BufferData.Replace("\r", "").Replace("\n", "");
                    string str1 = BufferData.Substring(4, 2);
                    switch (str1)
                    {
                        //电池信息部分
                        case "01":
                            string str2 = BufferData.Substring(8, 2);
                            if (str2 == "01")
                            {
                                pictureBox2.Image = Image.FromFile("Full.jpg");
                            }
                            else if (str2 == "02")
                            {
                                pictureBox2.Image = Image.FromFile("InCharge.jpg");
                            }
                            else if (str2 == "03")
                            {
                                pictureBox2.Image = Image.FromFile("Cutout.jpg");
                            }
                            break;
                        //电压部分
                        case "02":
                            string str3 = BufferData.Substring(6);
                            textBox5.Text = str3;
                            break;
                        //电流部分
                        case "03":
                            string str4 = BufferData.Substring(6);
                            textBox6.Text = str4;
                            break;
                        //应答部分
                        case "04":
                            string str_watch = BufferData.Substring(6, 2);
                            //string str_ack = BufferData.Substring(8, 2);
                            if (str_watch == "01")
                            {
                                ACK_Flag = true;
                                ACK_Flag_2 = true;
                                pictureBox3.Image = Image.FromFile("Green.jpg");
                            }
                            else if (str_watch == "04")
                            {
                                Set_Value_Flag = true;
                                pictureBox5.Image = Image.FromFile("Green.jpg");
                            }
                            else if (str_watch == "05")
                            {
                                Start_Work_Flag = true;
                                pictureBox4.Image = Image.FromFile("Green.jpg");
                            }
                            break;
                        //数据部分
                        case "05":
                            float[] Data_Temp = new float[10];
                            Data_Temp[0] = ((float)Int32.Parse(BufferData.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[1] = ((float)Int32.Parse(BufferData.Substring(8, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[2] = ((float)Int32.Parse(BufferData.Substring(10, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[3] = ((float)Int32.Parse(BufferData.Substring(12, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[4] = ((float)Int32.Parse(BufferData.Substring(14, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[5] = ((float)Int32.Parse(BufferData.Substring(16, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[6] = ((float)Int32.Parse(BufferData.Substring(18, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[7] = ((float)Int32.Parse(BufferData.Substring(20, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[8] = ((float)Int32.Parse(BufferData.Substring(22, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            Data_Temp[9] = ((float)Int32.Parse(BufferData.Substring(24, 2), System.Globalization.NumberStyles.HexNumber) + 100) / 10;
                            for (int i = 0; i < 10; i++)
                            {
                                textBox1.AppendText(DateTime.Now.ToString() + "   " + "Temp" + Temp_cnt_num + "   " + Data_Temp[i] + "\r\n");
                                if (Temp_cnt_num >= 500)
                                {
                                    Temp_cnt_num = 1;
                                }
                                else
                                {
                                    Temp_cnt_num++;
                                }

                            }
                            //textBox1.AppendText(DateTime.Now.ToString() + "   " + abc + "\r\n");
                            //textBox1.AppendText(DateTime.Now.ToString() + "   " + str5 + "\r\n");        //串口类会自动处理汉字，所以不需要特别转换
                            break;
                        case "06":
                            string str_error = BufferData.Substring(6, 2);
                            if (str_error == "01") //ACK错误
                            {
                                pictureBox3.Image = Image.FromFile("Red.jpg");
                                textBox1.AppendText("ACK Error!\r\n");
                            }
                            else if (str_error == "02") //采集组数设置错误
                            {
                                pictureBox5.Image = Image.FromFile("Red.jpg");
                                textBox1.AppendText("CatchNumber Error!\r\n");
                            }
                            else if (str_error == "03") //采集时间间隔设置错误
                            {
                                pictureBox5.Image = Image.FromFile("Red.jpg");
                                textBox1.AppendText("CatchTime Error!\r\n");
                            }
                            else if (str_error == "04") //开始延时设置错误
                            {
                                pictureBox5.Image = Image.FromFile("Red.jpg");
                                textBox1.AppendText("DelayTime Error!\r\n");
                            }
                            else if (str_error == "05") //开始工作指令设置错误
                            {
                                pictureBox4.Image = Image.FromFile("Red.jpg");
                                textBox1.AppendText("StartCMD Error!\r\n");
                            }
                            else if (str_error == "06") //获取数据指令设置错误
                            {
                                textBox1.AppendText("GetDataCMD Error!\r\n");
                            }
                            break;
                        default: break;
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    byte[] data = new byte[serialPort1.BytesToRead];    //定义缓冲区，因为串口事件触发时有可能收到不止一个字节
                    serialPort1.Read(data, 0, data.Length);
                    //if (F2 != null)
                    //    F2.Add_SendData(data);
                    foreach (byte Member in data)                       //遍历用法
                    {
                        string str = Convert.ToString(Member, 16).ToUpper();
                        textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 清空接收按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Clear();
            }
            catch { }
        }

        /// <summary>
        /// 清空发送按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                textBox2.Clear();
            }
            catch { }
        }

        /// <summary>
        /// 检测电池状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:020000\r\n");
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 电参数采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:030000\r\n");
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 寻求响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox3.Image = Image.FromFile("Red.jpg");
                serialPort1.Write("CMD:013100\r\n");                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox4.Image = Image.FromFile("Red.jpg");
                serialPort1.Write("CMD:013500\r\n");                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        string Data_i = "CMD:0136";
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                string i = textBox8.Text;
                StringBuilder Str_i = new StringBuilder();

                Str_i.Append(Data_i);
                Str_i.Append(i + "\r\n");

                serialPort1.Write(Str_i.ToString());                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 采集参数配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:1233010102\r\n");                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 保存按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = @"C:\Users\Administrator\Desktop";
                sfd.Title = "请选择要保存的文件路径  格式.txt";
                sfd.Filter = "文本文件|*.txt|所有文件|*.*";
                sfd.ShowDialog();

                //获得用户要保存的文件路径
                string path = sfd.FileName;
                if (path == "")
                {
                    return;
                }
                using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(textBox1.Text);
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
                MessageBox.Show("保存成功");
            }
            catch { }
        }

        Thread th;
        //
        /// <summary>
        /// 自动模式按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoButton_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            ACK_Flag = false;
            Set_Value_Flag = false;
            Start_Work_Flag = false;
            AutoButton.Enabled = false;
            button14.Enabled = true;

            pictureBox3.Image = Image.FromFile("Red.jpg");
            pictureBox4.Image = Image.FromFile("Red.jpg");
            pictureBox5.Image = Image.FromFile("Red.jpg");


            //创建一个线程去执行这个方法
            th = new Thread(Auto_Handle);
            //标记这个线程准备就绪了，可以随时被执行。具体什么时候执行这个线程，
            //由cpu决定
            //将线程设置为后台线程
            th.IsBackground = true;
            //th.Start();
            //th.Abort();
            th.Start();
            //timer1.Enabled = true;



        }

        bool Time1_Start_Flag = false;

        private void Auto_Handle()
        {
            try
            {
                string Time = textBox3.Text;
                string Delay = textBox4.Text;
                string Cnt_number = textBox7.Text;

                StringBuilder Str_T = new StringBuilder();
                StringBuilder Str_D = new StringBuilder();
                StringBuilder Str_C = new StringBuilder();

                while (ACK_Flag == false)
                {
                    serialPort1.Write("CMD:013100\r\n");
                    Delay_Time(1);
                }
                while (Set_Value_Flag == false && ACK_Flag == true)
                {

                    Str_T.Append(Time_cmd);
                    Str_T.Append(Time + "\r\n");
                    Str_D.Append(Delay_cmd);
                    Str_D.Append(Delay + "\r\n");
                    Str_C.Append(Cnt_cmd);
                    Str_C.Append(Cnt_number + "\r\n");

                    serialPort1.Write(Str_C.ToString());                      //字符串写入
                    Delay_Time(1);
                    serialPort1.Write(Str_T.ToString());                      //字符串写入
                    Delay_Time(1);
                    serialPort1.Write(Str_D.ToString());                      //字符串写入
                    Delay_Time(1);

                    Str_T.Clear();
                    Str_D.Clear();
                    Str_C.Clear();
                }
                while (Start_Work_Flag == false && Set_Value_Flag == true && ACK_Flag == true)
                {
                    serialPort1.Write("CMD:013500\r\n");
                    Delay_Time(1);
                }
                while (Start_Work_Flag == true && Set_Value_Flag == true && ACK_Flag == true)
                {
                    //serialPort1.Write("OKOK\r\n");
                    //Delay_Time(1);
                    string CatchTime = textBox3.Text;
                    string CatchNum = textBox7.Text;
                    CT = Convert.ToInt32(CatchTime);
                    CN = Convert.ToInt32(CatchNum);

                    time_set = CT * 10 * CN;

                    //serialPort1.Write(time_set.ToString());

                    Set_Value_Flag = false;
                    Start_Work_Flag = false;
                    ACK_Flag = false;

                    Time1_Start_Flag = true;
                    //timer1.Interval = 1000;
                    //timer1.Enabled = true;

                }
            }
            catch
            {

            }
        }

        int CT;
        int CN;
        int time_set;
        int time1_i;
        int group_i;

        /// <summary>
        /// 定时器1函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Time1_Start_Flag == true)
                {
                    time1_i++;
                    if (time1_i == (time_set + 10))
                    {
                        time1_i = 0;
                        timer3.Enabled = true;

                        timer1.Enabled = false;
                    }
                }
            }
            catch
            { }

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:013100\r\n");
                if (ACK_Flag == true)
                {
                    timer2.Enabled = true;
                    timer3.Enabled = false;
                }
            }
            catch
            { }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                group_i++;
                //serialPort1.Write("CMD:013600\r\n");
                //string i = textBox8.Text;
                if (group_i <= CN)
                {
                    StringBuilder Str_i = new StringBuilder();

                    Str_i.Append(Data_i);
                    if (group_i < 10)
                    {
                        Str_i.Append("0" + group_i + "\r\n");
                    }
                    else
                    {
                        Str_i.Append(group_i + "\r\n");
                    }

                    serialPort1.Write(Str_i.ToString());                      //字符串写入

                    Str_i.Clear();
                    //time1_i = 0;
                }
                else
                {
                    group_i = 0;
                    timer2.Enabled = false;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 手动模式按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            AutoButton.Enabled = true;
            button14.Enabled = false;
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;

            pictureBox3.Image = Image.FromFile("Red.jpg");
            pictureBox4.Image = Image.FromFile("Red.jpg");
            pictureBox5.Image = Image.FromFile("Red.jpg");

            ACK_Flag = false;
            Set_Value_Flag = false;
            Start_Work_Flag = false;

            th.Abort();
        }


        public static bool Delay_Time(int delayTime)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
                Application.DoEvents();
            }
            while (s < delayTime);
            return true;
        }

        string Time_cmd = "CMD:0133";
        string Delay_cmd = "CMD:0134";
        string Cnt_cmd = "CMD:0132";





        /// <summary>
        /// 参数配置（确认更改）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox5.Image = Image.FromFile("Red.jpg");

                string Time = textBox3.Text;
                string Delay = textBox4.Text;
                string Cnt_number = textBox7.Text;

                StringBuilder Str_T = new StringBuilder();
                StringBuilder Str_D = new StringBuilder();
                StringBuilder Str_C = new StringBuilder();

                Str_T.Append(Time_cmd);
                Str_T.Append(Time + "\r\n");
                Str_D.Append(Delay_cmd);
                Str_D.Append(Delay + "\r\n");
                Str_C.Append(Cnt_cmd);
                Str_C.Append(Cnt_number + "\r\n");

                serialPort1.Write(Str_C.ToString());                      //字符串写入
                Delay_Time(1);
                serialPort1.Write(Str_T.ToString());                      //字符串写入
                Delay_Time(1);
                serialPort1.Write(Str_D.ToString());                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //当你点击关闭窗体的时候，判断新线程是否为null
            if (th != null)
            {
                //结束这个线程
                th.Abort();
            }
        }





    }
}
