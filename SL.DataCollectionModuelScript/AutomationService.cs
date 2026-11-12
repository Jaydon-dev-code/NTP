using System;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace SL.DataCollectionModuelScript
{
    /// <summary>
    /// 自动化服务：所有 Selenium 流程集中在这一个类里。
    /// 不同模块（ModuelTypeEnum）用 switch 区分默认 IP；流程相同的走同一分支。
    /// 流程顺序：进入配置页 → 登录 → 填写IP/MAC/网关 → 提交二次确认 → 等待刷新 → 回读校验
    /// </summary>
    public class AutomationService : IDisposable
    {
        // 登录账号密码（所有模块通用）
        private const string User = "admin";
        private const string Pass = "admin";

        private EdgeDriver? _driver;

        /// <summary>
        /// 是否观察浏览器页面。
        /// true = 显示浏览器窗口（调试用）；false = 无头后台运行。
        /// 修改后若 Driver 已存在，下次执行会按新模式重建。
        /// </summary>
        public bool ShowBrowser { get; set; }

        /// <summary>进度日志回调：设备编号、步骤、状态(进行中/成功/错误)、详情</summary>
        public event Action<string, string, string, string?>? OnLog;

        private void Log(string id, string step, string status, string? detail = null) =>
            OnLog?.Invoke(id, step, status, detail);

        // ==================== 模块默认参数 ====================

        /// <summary>
        /// 各模块访问配置页用的默认地址（登录入口）。
        /// 目前项目里各型号一致，后续有差异改这里即可。
        /// </summary>
        private  string GetModuleHomeIp(ModuelTypeEnum type) =>
            type switch
            {
                ModuelTypeEnum.BCNet_Q => "192.168.1.100",
                ModuelTypeEnum.BCNet_FX => "192.168.1.100",
                ModuelTypeEnum.BCNet_DVP => "192.168.1.100",
                ModuelTypeEnum.BCNet_CP => "192.168.1.100",
                ModuelTypeEnum.BCNet_S7200 => "192.168.1.100",
                _ => "192.168.1.100",
            };

        /// <summary>
        /// 各模块【还原默认IP】时填入的固定参数。
        /// 当前统一：IP=192.168.1.178 掩码=255.255.255.0 网关=192.168.1.1
        /// 不同型号有差异时改对应 case。
        /// </summary>
        private  (string Ip, string Mask, string Gateway) GetModuleDefaultNetwork(
            ModuelTypeEnum type
        ) =>
            type switch
            {
                // 目前各型号默认网络参数一致，后续有差异分别写 case
                ModuelTypeEnum.BCNet_Q
                or ModuelTypeEnum.BCNet_FX
                or ModuelTypeEnum.BCNet_DVP
                or ModuelTypeEnum.BCNet_CP => ("192.168.1.178", "255.255.255.0", "192.168.1.1"),
                 ModuelTypeEnum.BCNet_S7200 => ("192.168.1.188", "255.255.255.0", "192.168.1.1"),
                _ => ("192.168.1.178", "255.255.255.0", "192.168.1.1"),
            };

        // ==================== 功能1：配置IP ====================

        /// <summary>
        /// 配置IP完整流程（异步，不卡界面）。
        /// 访问 Excel 该行 IP，填入该行 IP/掩码/网关（Excel「mac地址」列=掩码），提交后重新进入回读校验。
        /// </summary>
        public async Task<bool> ConfigureIpAsync(DeviceInfo d, CancellationToken ct = default)
        {
            string step = "";
            try
            {
                step = "启动浏览器";
                Log(d.DeviceId, step, "进行中");
                var driver = EnsureDriver();

                // 1 进入配置页面（地址 = Excel 当前行 IP）
                step = "进入配置页面";
                Log(d.DeviceId, step, "进行中", d.Ip);
                driver.Url = "http://" + GetModuleDefaultNetwork(d.ModuleType).Ip?.Trim();
                // 2 登录配置页
                step = "登录配置页";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => Login(driver), ct);

                // 3 跳转到网络配置页（config2）
                step = "进入网络配置页";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => JumpConfigPage(driver), ct);

                // 4 填写网络参数：IP / 掩码 / 网关 均来自 Excel 当前行；MAC 不填
                // Excel「mac地址」列实际是子网掩码
                step = "填写网络参数";
                Log(d.DeviceId, step, "进行中", $"IP={d.Ip} MASK={d.Mask} GW={d.Gateway}");
                await Task.Run(
                    () =>
                    {
                        FillNetwork(driver, d.Ip, mac: d.Mask, gateway: d.Gateway);
                    },
                    ct
                );

                // 5 提交 + 二次确认
                step = "提交配置";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => SubmitConfirm(driver), ct);

                // 6 检测信息中：提交后页面不会自动跳回，需重新进入原配置页再回读
                step = "检测信息中";
                Log(d.DeviceId, step, "进行中", "重新进入配置页回读");
                await Task.Run(
                    async () =>
                    {
                       //await Task.Delay(5000);
                        // 重新访问之前配置用的地址
                        driver = EnsureDriver();
                        driver.Url = "http://" + d.Ip.Trim();
                        Login(driver);
                        JumpConfigPage(driver);
                    },
                    ct
                );

                // 7 回读校验：IP / 掩码 / 网关（不校验MAC）
                step = "校验信息一致";
                Log(d.DeviceId, step, "进行中");
                return await Task.Run(
                    () =>
                        VerifyNetwork(
                            driver,
                            d.DeviceId,
                            expectIp: d.Ip,
                            expectMask: d.Mask,
                            expectGw: d.Gateway
                        ),
                    ct
                );
            }
            catch (Exception ex)
            {
                Log(d.DeviceId, step, "错误", ex.Message);
                return false;
            }
        }

        // ==================== 功能2：还原默认IP ====================

        /// <summary>
        /// 还原默认IP（异步）。
        /// 场景：之前配置错了，需要还原为模块默认网络。
        /// 进入访问地址 = Excel 里该设备的 IP；
        /// 填入模块默认 IP/掩码/网关并提交后，设备会工作在默认 IP 上，
        /// 因此回读校验要重新进入【默认 IP】页面再读。
        /// </summary>
        public async Task<bool> RestoreDefaultIpAsync(DeviceInfo d, CancellationToken ct = default)
        {
            string step = "";
            try
            {
                (string defIp, string defMask, string defGw) = GetModuleDefaultNetwork(
                    d.ModuleType
                );

                step = "启动浏览器";
                Log(d.DeviceId, step, "进行中");
                var driver = EnsureDriver();

                // 1 访问原IP
                step = "进入配置页面";
                Log(d.DeviceId, step, "进行中", $"{d.Ip}（原IP）");
                await Task.Run(
                    () =>
                    {
                        driver.Url = d.Ip.StartsWith("http") ? d.Ip : "http://" + d.Ip;
                        driver.Navigate().GoToUrl(driver.Url);
                    },
                    ct
                );

                // 2 登录
                step = "登录配置页";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => Login(driver), ct);

                // 3 跳转配置页
                step = "进入网络配置页";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => JumpConfigPage(driver), ct);

                // 4 填默认参数（MAC留空）
                step = "填写默认参数";
                Log(d.DeviceId, step, "进行中", $"IP={defIp} MASK={defMask} GW={defGw}");
                await Task.Run(
                    () =>
                    {
                        FillNetwork(driver, defIp, mac: "", gateway: defGw);
                        TryFillMask(driver, defMask);
                    },
                    ct
                );

                // 5 提交确认
                step = "二次确认提交配置";
                Log(d.DeviceId, step, "进行中");
                await Task.Run(() => SubmitConfirm(driver), ct);

                // 6 检测信息中：提交后页面不会自动跳回；
                // 还原成功后设备已在默认 IP，重新进入【默认 IP】页面回读
                step = "检测信息中";
                Log(d.DeviceId, step, "进行中", $"重新进入默认IP {defIp} 回读");
                await Task.Run(
                    () =>
                    {
                        string url = defIp.StartsWith("http") ? defIp : "http://" + defIp;
                        driver.Navigate().GoToUrl(url);
                        Login(driver);
                        JumpConfigPage(driver);
                    },
                    ct
                );

                // 7 回读校验：IP / 掩码 / 网关（不校验MAC）
                step = "校验信息一致";
                Log(d.DeviceId, step, "进行中");
                return await Task.Run(
                    () =>
                        VerifyNetwork(
                            driver,
                            d.DeviceId,
                            expectIp: defIp,
                            expectMask: defMask,
                            expectGw: defGw
                        ),
                    ct
                );
            }
            catch (Exception ex)
            {
                Log(d.DeviceId, step, "错误", ex.Message);
                return false;
            }
        }

        // ==================== 流程内部步骤 ====================

        /// <summary>懒启动 EdgeDriver：按 ShowBrowser 决定是否无头；模式变化时重建</summary>
        private EdgeDriver EnsureDriver()
        {
            // 已有 Driver 且模式一致 → 直接复用
            //if (_driver != null && _headlessCache == !ShowBrowser)
            //    return _driver;

            // 模式变了或首次创建：先关掉旧的
            if (_driver != null)
            {
                try
                {
                    _driver.Quit();
                }
                catch { }
                try
                {
                    _driver.Dispose();
                }
                catch { }
                _driver = null;
            }

            var opt = new EdgeOptions();
            if (!ShowBrowser)
                opt.AddArgument("--headless=new");
            ////opt.AddArgument("--ignore-certificate-errors");
            ////opt.AddArgument("--no-sandbox");
            ////opt.AddArgument("--window-size=1280,900");
            _driver = new EdgeDriver(opt);
            //_driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            _headlessCache = !ShowBrowser;
            return _driver;
        }

        /// <summary>当前 Driver 是否处于无头模式（用于检测 ShowBrowser 是否变化）</summary>
        private bool _headlessCache;

        /// <summary>登录：txtAct / txtPwd / frmSetting 提交（与原 ConfigurationLinkBase 草稿一致）</summary>
        private void Login(EdgeDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(d =>
            {
                try
                {
                    return d.FindElement(By.Id("txtAct")).Displayed;
                }
                catch
                {
                    return false;
                }
            });
            driver.FindElement(By.Id("txtAct")).SendKeys(User);
            driver.FindElement(By.Id("txtPwd")).SendKeys(Pass);
            driver.FindElement(By.Id("frmSetting")).Submit();
        }

        /// <summary>跳转到 config2.shtml 网络配置页，等待 IP1 出现</summary>
        private void JumpConfigPage(EdgeDriver driver)
        {
            driver.SwitchTo().Frame("main");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            var link = wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(By.LinkText("以太网接口参数"));
                    return el.Displayed ? el : null;
                }
                catch
                {
                    return null;
                }
            });
            link!.Click();
            wait.Until(d =>
            {
                try
                {
                    return d.FindElement(By.Name("IP1")).Displayed;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 填写网络参数。
        /// IP / 网关：IPv4 字符串按 "." 拆 4 段，填 IP1~IP4 / GW1~GW4；
        /// MAC：按 ":" "-" "." 或连续 hex 拆 6 段，填 MAC1~MAC6。
        /// 页面上其余 IP5~IP12 输入框一律清空。
        /// </summary>
        private void FillNetwork(EdgeDriver driver, string ip, string mac, string gateway)
        {
            // ---- IP：拆成 4 段填 IP1~IP4 ----
            string[] ipParts = SplitIpv4(ip);
            for (int i = 0; i < 4; i++)
            {
                var el = driver.FindElement(By.Name($"IP{i + 1}"));
                el.Clear();
                el.SendKeys(ipParts[i]);
            }

            // ---- MAC：拆成 6 段填 MAC1~MAC6 ----
            string[] macParts = SplitIpv4(mac);
            for (int i = 0; i < 4; i++)
            {
                var el = driver.FindElement(By.Name($"MK{i + 1}"));
                el.Clear();
                if (!string.IsNullOrWhiteSpace(mac))
                    el.SendKeys(macParts[i]);
            }

            // ---- 网关：拆成 4 段填 GW1~GW4 ----
            string[] gwParts = SplitIpv4(gateway);
            for (int i = 0; i < 4; i++)
            {
                var el = driver.FindElement(By.Name($"GW{i + 1}"));
                el.Clear();
                el.SendKeys(gwParts[i]);
            }
        }

        /// <summary>
        /// 将 IPv4 字符串拆成 4 段。不足补 "0"，超出取前 4 段。
        /// 例："192.168.1.100" → ["192","168","1","100"]
        /// </summary>
        private  string[] SplitIpv4(string ip)
        {
            var parts = (ip ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries);
            var result = new string[4];
            for (int i = 0; i < 4; i++)
                result[i] = i < parts.Length ? parts[i].Trim() : "0";
            return result;
        }

        /// <summary>
        /// 回读页面上的 IP1~IP4 并拼成完整 IPv4 字符串。
        /// 若页面只有整框 IP1（值含点），直接返回该值。
        /// </summary>
        private  string ReadBackIp(EdgeDriver driver) => ReadBackSegmented(driver, "IP");

        /// <summary>回读掩码：Mask1~Mask4 拼接；无分段则读整框 Mask/NETMASK</summary>
        private  string ReadBackMask(EdgeDriver driver)
        {
            string seg = ReadBackSegmented(driver, "MK", throwIfMissing: false);
            if (!string.IsNullOrEmpty(seg))
                return seg;
            try
            {
                return (driver.FindElement(By.Name("MK")).GetDomProperty("value") ?? "").Trim();
            }
            catch (NoSuchElementException)
            {
                return "";
            }
        }

        /// <summary>回读网关：GW1~GW4 拼接；无分段则读整框 GW</summary>
        private  string ReadBackGateway(EdgeDriver driver)
        {
            string seg = ReadBackSegmented(driver, "GW", throwIfMissing: false);
            if (!string.IsNullOrEmpty(seg))
                return seg;
            try
            {
                return (driver.FindElement(By.Name("GW")).GetDomProperty("value") ?? "").Trim();
            }
            catch (NoSuchElementException)
            {
                return "";
            }
        }

        /// <summary>
        /// 通用分段回读：{prefix}1~{prefix}4 拼成 a.b.c.d。
        /// 若 {prefix}1 值已含 "." 则视为整框直接返回。
        /// throwIfMissing=false 时元素不存在返回空串。
        /// </summary>
        private  string ReadBackSegmented(
            EdgeDriver driver,
            string prefix,
            bool throwIfMissing = true
        )
        {
            string v1;
            try
            {
                v1 = driver.FindElement(By.Name($"{prefix}1")).GetDomProperty("value") ?? "";
            }
            catch (NoSuchElementException)
            {
                if (throwIfMissing)
                    throw;
                return "";
            }

            // 整框（值含 "."）→ 直接返回
            if (v1.Contains('.'))
                return v1.Trim();

            // 分段 → 拼 4 段
            var parts = new string[4];
            parts[0] = v1.Trim();
            for (int i = 2; i <= 4; i++)
            {
                try
                {
                    parts[i - 1] = (
                        driver.FindElement(By.Name($"{prefix}{i}")).GetDomProperty("value") ?? ""
                    ).Trim();
                }
                catch (NoSuchElementException)
                {
                    if (throwIfMissing)
                        throw;
                    parts[i - 1] = "";
                }
            }
            return string.Join(".", parts);
        }

        /// <summary>
        /// 回读并校验 IP / 掩码 / 网关三项（不校验 MAC）。
        /// 任一不一致返回 false，日志写明差异。
        /// </summary>
        private bool VerifyNetwork(
            EdgeDriver driver,
            string deviceId,
            string expectIp,
            string expectMask,
            string expectGw
        )
        {
            string realIp = ReadBackIp(driver);
            string realMask = ReadBackMask(driver);
            string realGw = ReadBackGateway(driver);

            bool ipOk = string.Equals(
                realIp.Trim(),
                expectIp.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
            bool maskOk = string.Equals(
                realMask.Trim(),
                expectMask.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
            bool gwOk = string.Equals(
                realGw.Trim(),
                expectGw.Trim(),
                StringComparison.OrdinalIgnoreCase
            );

            if (ipOk && maskOk && gwOk)
            {
                Log(deviceId, "校验信息一致", "成功", $"IP={realIp} MASK={realMask} GW={realGw}");
                return true;
            }

            var errs = new List<string>();
            if (!ipOk)
                errs.Add($"IP 回读={realIp} 目标={expectIp}");
            if (!maskOk)
                errs.Add($"掩码 回读={realMask} 目标={expectMask}");
            if (!gwOk)
                errs.Add($"网关 回读={realGw} 目标={expectGw}");
            Log(deviceId, "校验信息一致", "错误", string.Join("；", errs));
            return false;
        }

        /// <summary>尝试填掩码：拆成 4 段填 Mask1~Mask4；无分段框则尝试整框</summary>
        private  void TryFillMask(EdgeDriver driver, string mask)
        {
            string[] parts = SplitIpv4(mask);
            bool filled = false;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    var el = driver.FindElement(By.Name($"Mask{i + 1}"));
                    el.Clear();
                    el.SendKeys(parts[i]);
                    filled = true;
                }
                catch (NoSuchElementException) { }
            }
            if (filled)
                return;

            // 整框候选
            foreach (var name in new[] { "Mask", "NETMASK", "mask" })
            {
                try
                {
                    var el = driver.FindElement(By.Name(name));
                    el.Clear();
                    el.SendKeys(mask);
                    return;
                }
                catch (NoSuchElementException) { }
            }
        }

        /// <summary>
        /// 提交 + 处理二次确认（JS alert 或页面确认按钮）。
        /// 注意：这里不能 driver.Quit()——提交后还要重新进入页面回读校验。
        /// </summary>
        private  void SubmitConfirm(EdgeDriver driver)
        {
            driver.FindElement(By.Name("config")).Submit();
            //driver.Quit();
            //// 处理可能出现的 JS alert 二次确认
            //try
            //{
            //    new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d =>
            //    {
            //        try
            //        {
            //            d.SwitchTo().Alert().Accept();
            //            return true;
            //        }
            //        catch (NoAlertPresentException)
            //        {
            //            return false;
            //        }
            //    });
            //}
            //catch (WebDriverTimeoutException)
            //{
            //    // 无 alert 则跳过
            //}
        }

        /// <summary>提交后等待页面刷新（IP1重新可用）</summary>
        private  void WaitRefresh(EdgeDriver driver)
        {
            Thread.Sleep(2000);
            new WebDriverWait(driver, TimeSpan.FromSeconds(20)).Until(d =>
            {
                try
                {
                    var el = d.FindElement(By.Name("IP1"));
                    return el.Displayed && el.Enabled;
                }
                catch
                {
                    return false;
                }
            });
        }

        private  IWebElement? FindBtn(EdgeDriver driver, params By[] bys)
        {
            foreach (var by in bys)
            {
                try
                {
                    var el = driver.FindElement(by);
                    if (el.Displayed)
                        return el;
                }
                catch { }
            }
            return null;
        }

        /// <summary>关闭浏览器</summary>
        public void Dispose()
        {
            try
            {
                _driver?.Quit();
            }
            catch { }
            try
            {
                _driver?.Dispose();
            }
            catch { }
            _driver = null;
        }
    }
}
