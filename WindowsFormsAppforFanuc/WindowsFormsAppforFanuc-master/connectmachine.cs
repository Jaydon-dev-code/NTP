using IoTClient;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsAppforFanuc
{
    public partial class connectmachine : Form
    {
        private bool _isClosing = false; // 添加窗口关闭标志
        private DateTime _lastErrorTime = DateTime.MinValue; // 用于控制错误提示频率

        public connectmachine()
        {
            InitializeComponent();
        }

        // SQL Server 连接
        string connectionString = "server=localhost;database=sys;user=root;password=123456";

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isClosing = true;
            try
            {
                // 停止所有定时器
                if (timer1 != null) timer1.Enabled = false;
                if (refreshdata != null) refreshdata.Enabled = false;
                if (datetimeupdate != null) datetimeupdate.Enabled = false;

                // 断开设备连接
                if (FanucOpe.h != 0)
                {
                    FanucOpe.cnc_freelibhndl(FanucOpe.h);
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不显示，因为窗口正在关闭
                System.Diagnostics.Debug.WriteLine($"窗口关闭时清理资源出错: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }

        private void ShowError(string message)
        {
            // 控制错误提示频率，至少间隔3秒
            if ((DateTime.Now - _lastErrorTime).TotalSeconds >= 3)
            {
                MessageBox.Show(message);
                _lastErrorTime = DateTime.Now;
            }
        }

        private void btconn_Click(object sender, EventArgs e)
        {
            string ip = textBox2.Text;
            string port = textBox4.Text;
            string timeout = textBox9.Text;
            short ret = FanucOpe.cnc_allclibhndl3(ip, Convert.ToUInt16(port), Convert.ToInt32(timeout), out FanucOpe.h);

            if (ret != FanucOpe.EW_OK)
            {
                ShowError("设备连接失败" + "错误代码：" + ret);
            }
            else
            {
                MessageBox.Show("设备连接成功！", "连接提示");
                textBox5.Text = FanucOpe.h.ToString();
            }
        }

        private void cancelbut_Click(object sender, EventArgs e)
        {
            short ret = FanucOpe.cnc_freelibhndl(FanucOpe.h);
            MessageBox.Show("设备断开成功", "断开提示");
            textBox5.Text = "0";
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FanucOpe.h != 0)
            {
                FanucOpe.ODBPOS fos = new FanucOpe.ODBPOS();
                short num = FanucOpe.MAX_AXIS;
                short type = -1;
                short ret = FanucOpe.cnc_rdposition(FanucOpe.h, type, ref num, fos);
                if (ret == 0)
                {
                    double x = fos.p1.abs.data * Math.Pow(10, -fos.p1.abs.dec);
                    string namex = textBox7.Text;
                    string recordtime = DateTime.Now.ToString();
                    listBox1.Items.Add(x.ToString());
                    this.showtime.Text = DateTime.Now.ToString();
                }
                else
                {
                    MessageBox.Show($"坐标采集失败，错误码：{ret}");
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool EnsureTableExists(MySqlConnection connection)
        {
            try
            {
                string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Device (
                    ID INT AUTO_INCREMENT PRIMARY KEY,
                    DeviceName VARCHAR(50),
                    DeviceRunTime VARCHAR(50),
                    DeviceState VARCHAR(50),
                    CreateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

                using (MySqlCommand command = new MySqlCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"创建表失败: {ex.Message}");
                return false;
            }
        }

        //写入数据库按钮
        private void write_sql_Click(object sender, EventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("数据库连接成功！");

                    // 确保表存在
                    if (!EnsureTableExists(connection))
                    {
                        return;
                    }

                    string insertSql = @"INSERT INTO Device (DeviceName, DeviceRunTime, DeviceState)
                        VALUES (@DeviceName, @DeviceRunTime, @DeviceState);";

                    using (MySqlCommand command = new MySqlCommand(insertSql, connection))
                    {
                        // 获取实际的设备数据
                        string deviceState = "未知";
                        if (!string.IsNullOrEmpty(mcnstatus.Text))
                        {
                            switch (mcnstatus.Text)
                            {
                                case "3": deviceState = "运行"; break;
                                case "5": deviceState = "报警"; break;
                                default: deviceState = "待机"; break;
                            }
                        }

                        command.Parameters.AddWithValue("@DeviceName", textBox2.Text); // 使用IP地址作为设备名
                        command.Parameters.AddWithValue("@DeviceRunTime", worktimetotal.Text);
                        command.Parameters.AddWithValue("@DeviceState", deviceState);

                        int rowsAffected = command.ExecuteNonQuery();
                        MessageBox.Show($"数据写入成功，影响行数: {rowsAffected}");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"数据库操作失败: {ex.Message}\n错误代码: {ex.Number}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"操作失败: {ex.Message}");
                }
            }
        }

        private void testDataSetBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            // 保留一个空实现，删除其它重复定义
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 保留一个空实现，删除其它重复定义
        }

        private void spindlespeed_TextChanged(object sender, EventArgs e)
        {
            // 保留一个空实现，删除其它重复定义
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            // 保留一个空实现，删除其它重复定义
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 保留一个空实现，删除其它重复定义
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (_isClosing) return; // 如果窗口正在关闭，不执行数据采集

            refreshdata.Enabled = true;
            if (FanucOpe.h == 0)
            {
                ShowError("设备未连接，无法采集数据！");
                return;
            }

            try
            {
                //主轴实际转速：
                FanucOpe.ODBACT data = new FanucOpe.ODBACT();
                short ret = FanucOpe.cnc_acts(FanucOpe.h, data);
                this.spindlespeed.Text = ret == FanucOpe.EW_OK ? data.data.ToString() : "";

                //切削实际速度 F
                FanucOpe.ODBACT data1 = new FanucOpe.ODBACT();
                ret = FanucOpe.cnc_actf(FanucOpe.h, data1);
                this.cuttingspeed.Text = ret == FanucOpe.EW_OK ? data1.data.ToString() : "";

                // CNC类型
                FanucOpe.ODBSYS k1 = new FanucOpe.ODBSYS();
                ret = FanucOpe.cnc_sysinfo(FanucOpe.h, k1);
                string MaxAxis = ret == FanucOpe.EW_OK ? k1.max_axis.ToString() : "";
                string CNCType = "其它类型";
                string type1 = ret == FanucOpe.EW_OK ? k1.cnc_type[0].ToString() + k1.cnc_type[1].ToString() : "";
                switch (type1)
                {
                    case "15": CNCType = "Series 15/15i"; break;
                    case "16": CNCType = "Series 16/16i"; break;
                    case "18": CNCType = "Series 18/18i"; break;
                    case "21": CNCType = "Series 21/21i"; break;
                    case "30": CNCType = "Series 30i"; break;
                    case "31": CNCType = "Series 31i"; break;
                    case "32": CNCType = "Series 32i"; break;
                    case "35": CNCType = "Series 35i"; break;
                    case " 0": CNCType = "Series 0i"; break;
                    case "PD": CNCType = "Power Mate i-D"; break;
                    case "PH": CNCType = "Power Mate i-H"; break;
                    case "PM": CNCType = "Power Motion i"; break;
                }
                this.maxaxis.Text = MaxAxis;
                this.cnctype1.Text = CNCType;
                this.cncspec.Text = ret == FanucOpe.EW_OK ? k1.series[0].ToString() + k1.series[1].ToString() + k1.series[2].ToString() + k1.series[3].ToString() : "";

                //CNC工作模式
                FanucOpe.ODBST statinfo = new FanucOpe.ODBST();
                ret = FanucOpe.cnc_statinfo(FanucOpe.h, statinfo);
                short run = ret == FanucOpe.EW_OK ? statinfo.run : (short)0;
                short Alarm = ret == FanucOpe.EW_OK ? statinfo.alarm : (short)0;
                if (Alarm != 0) run = 5;
                string CNCModel = "others mode";
                if (ret == FanucOpe.EW_OK)
                {
                    switch (statinfo.aut)
                    {
                        case 0: CNCModel = "MDI"; break;
                        case 1: CNCModel = "MEMory"; break;
                        case 2: CNCModel = "Not Defined"; break;
                        case 3: CNCModel = "EDIT"; break;
                        case 4: CNCModel = "h"; break;
                        case 5: CNCModel = "JOG"; break;
                        case 6: CNCModel = "Teach in JOG"; break;
                        case 7: CNCModel = "Teach in h"; break;
                        case 8: CNCModel = "INC·feed"; break;
                        case 9: CNCModel = "REFerence"; break;
                        case 10: CNCModel = "ReMoTe"; break;
                    }
                }
                this.mcnstatus.Text = run.ToString();
                this.cncworktype.Text = CNCModel;

                //进给倍率
                FanucOpe.IODBPMC0 ig = new FanucOpe.IODBPMC0();
                ret = FanucOpe.pmc_rdpmcrng(FanucOpe.h, 0, 1, 12, 13, 8 + 1 * 2, ig);
                this.feedrate.Text = ret == FanucOpe.EW_OK ? (100 - (ig.cdata[0] - 155)).ToString() : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"进给倍率采集失败，错误码：{ret}");

                //程序相关
                FanucOpe.ODBPRO dbpro = new FanucOpe.ODBPRO();
                ret = FanucOpe.cnc_rdprgnum(FanucOpe.h, dbpro);
                this.mainprogno.Text = ret == FanucOpe.EW_OK ? dbpro.mdata.ToString() : "";
                this.subprogno.Text = ret == FanucOpe.EW_OK ? dbpro.data.ToString() : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"程序号采集失败，错误码：{ret}");

                //Progname
                FanucOpe.ODBEXEPRG buf = new FanucOpe.ODBEXEPRG();
                ret = FanucOpe.cnc_exeprgname(FanucOpe.h, buf);
                this.progname.Text = ret == FanucOpe.EW_OK ? new string(buf.name) : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"程序名采集失败，错误码：{ret}");

                //刀具寿命
                FanucOpe.ODBLFNO maxlfg = new FanucOpe.ODBLFNO();
                ret = FanucOpe.cnc_rdmaxgrp(FanucOpe.h, maxlfg);
                this.txtmaxgrp.Text = ret == FanucOpe.EW_OK ? maxlfg.data.ToString() : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"刀具组数采集失败，错误码：{ret}");

                //加工计数
                FanucOpe.ODBM bb = new FanucOpe.ODBM();
                ret = FanucOpe.cnc_rdmacro(FanucOpe.h, 0xf3d, 0x0a, bb);
                this.countpertime.Text = ret == FanucOpe.EW_OK ? (bb.mcr_val / 100000).ToString() : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"加工计数采集失败，错误码：{ret}");

                FanucOpe.IODBPSD_1 param6712 = new FanucOpe.IODBPSD_1();
                ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6712, 0, 8, param6712);
                this.counttotal.Text = ret == FanucOpe.EW_OK ? param6712.ldata.ToString() : "";
                if (ret != FanucOpe.EW_OK) MessageBox.Show($"总加工数采集失败，错误码：{ret}");

                //获取切削时间
                FanucOpe.IODBPSD_1 param6753 = new FanucOpe.IODBPSD_1();
                FanucOpe.IODBPSD_1 param6754 = new FanucOpe.IODBPSD_1();
                ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6753, 0, 8 + 32, param6753);
                int CutTime = 0;
                if (ret == FanucOpe.EW_OK)
                {
                    int cuttingTimeSec = param6753.ldata / 1000;
                    ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6754, 0, 8 + 32, param6754);
                    if (ret == FanucOpe.EW_OK)
                    {
                        int cuttingTimeMin = param6754.ldata;
                        CutTime = cuttingTimeMin * 60 + cuttingTimeSec;
                    }
                }
                this.cuttingtime.Text = CutTime.ToString();

                //获取运行时间
                FanucOpe.IODBPSD_1 param6751 = new FanucOpe.IODBPSD_1();
                FanucOpe.IODBPSD_1 param6752 = new FanucOpe.IODBPSD_1();
                ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6751, 0, 8, param6751);
                int CycSec = 0;
                if (ret == FanucOpe.EW_OK)
                {
                    int workingTimeSec = param6751.ldata / 1000;
                    ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6752, 0, 8, param6752);
                    if (ret == FanucOpe.EW_OK)
                    {
                        int workingTimeMin = param6752.ldata;
                        CycSec = workingTimeMin * 60 + workingTimeSec;
                    }
                }
                this.worktimetotal.Text = CycSec.ToString();

                //获取开机时间
                FanucOpe.IODBPSD_1 param6750 = new FanucOpe.IODBPSD_1();
                ret = FanucOpe.cnc_rdparam(FanucOpe.h, 6750, 0, 8 + 32, param6750);
                int PoweOnTime = ret == FanucOpe.EW_OK ? param6750.ldata * 60 : 0;
                this.ontimetotal.Text = PoweOnTime.ToString();

                //坐标
                FanucOpe.ODBPOS fos = new FanucOpe.ODBPOS();
                short num = FanucOpe.MAX_AXIS;
                short type = -1;
                ret = FanucOpe.cnc_rdposition(FanucOpe.h, type, ref num, fos);
                if (ret == 0)
                {
                    this.xposact.Text = (fos.p1.abs.data * Math.Pow(10, -fos.p1.abs.dec)).ToString();
                    this.yposact.Text = (fos.p2.abs.data * Math.Pow(10, -fos.p2.abs.dec)).ToString();
                    this.xpos.Text = (fos.p1.rel.data * Math.Pow(10, -fos.p1.rel.dec)).ToString();
                    this.ypos.Text = (fos.p2.rel.data * Math.Pow(10, -fos.p2.rel.dec)).ToString();
                }
                else
                {
                    MessageBox.Show($"坐标采集失败，错误码：{ret}");
                }

                //报警数据
                ret = FanucOpe.cnc_alarm2(FanucOpe.h, out int almdsta);
                string AlarmMessage = "未知错误";
                if (ret == FanucOpe.EW_OK)
                {
                    switch (almdsta)
                    {
                        case 0: AlarmMessage = "参数开启（SW）"; break;
                        case 1: AlarmMessage = "关机参数设置（PW）"; break;
                        case 2: AlarmMessage = "I / O错误（IO）"; break;
                        case 3: AlarmMessage = "前景P / S（PS"; break;
                        case 4: AlarmMessage = "超程，外部数据（OT"; break;
                        case 5: AlarmMessage = "过热报警（OH）"; break;
                        case 6: AlarmMessage = "伺服报警（SV"; break;
                        case 7: AlarmMessage = "数据I / O错误（SR）"; break;
                        case 8: AlarmMessage = "宏指令报警（MC"; break;
                        case 9: AlarmMessage = "主轴报警（SP）"; break;
                        case 10: AlarmMessage = "其他警报（DS）"; break;
                        case 11: AlarmMessage = "有关故障防止功能（IE）的警报"; break;
                        case 12: AlarmMessage = "背景P / S（BG）"; break;
                        case 13: AlarmMessage = "同步错误（SN）"; break;
                        case 14: AlarmMessage = "保留"; break;
                        case 15: AlarmMessage = "外部报警信息（EX）"; break;
                        case 16: AlarmMessage = "正向超程（软限位1）"; break;
                    }
                }
                this.txtalarmmsg.Text = AlarmMessage;

                // 更新时间显示
                this.showtime.Text = DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                if (!_isClosing) // 只在非关闭状态显示错误
                {
                    ShowError($"数据采集异常：{ex.Message}");
                }
            }
        }

        private void connectmachine_Load(object sender, EventArgs e)
        {
            this.showtime.Text = DateTime.Now.ToString();
            datetimeupdate.Enabled = true;
        }

        private void refreshtime_Tick(object sender, EventArgs e)
        {
            if (_isClosing) return;

            this.showtime.Text = DateTime.Now.ToString();

            // 直接调用数据采集逻辑，而不是通过 button2.PerformClick()
            if (FanucOpe.h != 0)
            {
                try
                {
                    // 只更新关键数据，减少采集频率
                    FanucOpe.ODBPOS fos = new FanucOpe.ODBPOS();
                    short num = FanucOpe.MAX_AXIS;
                    short type = -1;
                    short ret = FanucOpe.cnc_rdposition(FanucOpe.h, type, ref num, fos);
                    if (ret == 0)
                    {
                        this.xposact.Text = (fos.p1.abs.data * Math.Pow(10, -fos.p1.abs.dec)).ToString();
                        this.yposact.Text = (fos.p2.abs.data * Math.Pow(10, -fos.p2.abs.dec)).ToString();
                        this.xpos.Text = (fos.p1.rel.data * Math.Pow(10, -fos.p1.rel.dec)).ToString();
                        this.ypos.Text = (fos.p2.rel.data * Math.Pow(10, -fos.p2.rel.dec)).ToString();
                    }
                }
                catch (Exception ex)
                {
                    if (!_isClosing)
                    {
                        ShowError($"数据更新异常：{ex.Message}");
                    }
                }
            }
        }

        private void datetimeupdate_Tick(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                this.showtime.Text = DateTime.Now.ToString();
            }
        }
    }
}