using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            comboBox1.Text = "COM6";                            //串口默认选项
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
                                pictureBox2.Image = Image.FromFile("InCharge.jpg");
                            }
                            else if (str2 == "02")
                            {
                                pictureBox2.Image = Image.FromFile("Cutout.jpg");
                            }
                            else if (str2 == "03")
                            {
                                pictureBox2.Image = Image.FromFile("Full.jpg");
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
                serialPort1.Write("CMD:32\r\n");
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
                serialPort1.Write("CMD:72\r\n");
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
                serialPort1.Write("CMD:1203010c02\r\n");                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        /// <summary>
        /// 准备数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:1213010c02\r\n");                      //字符串写入
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write("CMD:1223010c02\r\n");                      //字符串写入
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
                sfd.Title = "请选择要保存的文件路径";
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

        /// <summary>
        /// 自动模式按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoButton_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        /// <summary>
        /// 定时器1函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //serialPort1.Write("CMD:1223010c02\r\n"); 
        }

        /// <summary>
        /// 手动模式按钮函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

    }
}
