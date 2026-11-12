//using OpenQA.Selenium;
//using OpenQA.Selenium.Edge;
//using OpenQA.Selenium.Support.UI;
//using System;
//using System.Collections.Generic;
//using System.Drawing.Printing;
//using System.Text;

//namespace SL.DataCollectionModuelScript
//{
//    public abstract class ConfigurationLinkBase
//    {
//        ModuelTypeEnum _moduelType { set; get; }
//        EdgeOptions _options;
//        public void Configuration(ModuelTypeEnum moduelTypeEnum)
//        {
//            _moduelType = moduelTypeEnum;
//            LoginConfigPage();
//            JumpConfigPage();
//            WirteLinIfo();
//            CheckConfig();
//        }

//        string GetDefaultIp()
//        {
//            switch (_moduelType)
//            {
//                case ModuelTypeEnum.BCNet_FX:
//                case ModuelTypeEnum.BCNet_Q:
//                case ModuelTypeEnum.BCNet_DVP:
//                case ModuelTypeEnum.BCNet_CP:
//                case ModuelTypeEnum.BCNet_S7200:
//                    return "192.168.1.100";
//            }
//            return "192.168.1.100";
//        }

//        private void CheckConfig()
//        {
//            throw new NotImplementedException();
//        }

//        private void WirteLinIfo()
//        {
//            throw new NotImplementedException();
//        }

//        private void JumpConfigPage()
//        {
//            var linkBu = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
//            linkBu.Until(d => d.FindElement(By.CssSelector("a[href='config2.shtml']")));
//            driver.FindElement(By.CssSelector("a[href='config2.shtml']")).Click();
//            var linkInfo = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
//            linkInfo.Until(d => d.FindElement(By.Name("IP1")));
//        }

//        private void LoginConfigPage()
//        {
//            _options = new EdgeOptions();
//           // _options.AddArguments("--headless=new"); // 无头模式（可选）
//            using IWebDriver driver = new EdgeDriver(_options);
//            driver.Url = GetDefaultIp();
//            driver.FindElement(By.Id("txtAct")).SendKeys("admin");
//            driver.FindElement(By.Id("txtPwd")).SendKeys("admin");
//            var form = driver.FindElement(By.Id("frmSetting"));
//            form.Submit();
           
//        }
//    }
//}
