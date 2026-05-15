using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Serilog;

namespace SL.MLineDataPrecisionTracking.Client.Http
{
    public class HubClien
    {
        private HubConnection _connection;
        private IHubProxy _hubProxy;

        private readonly string _hubName;
        private bool _isConnected;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="url">服务地址</param>
        /// <param name="hubName">Hub类名</param>
        public HubClien()
        {
            _connection = new HubConnection(ConfigurationManager.AppSettings["ServiceUri"]);
            _connection.Closed += Reconnect;
            _hubProxy = _connection.CreateHubProxy("ChatHub");
        }

        /// <summary>
        /// 启动并监听推送（泛型，支持任意模型）
        /// </summary>
        /// <typeparam name="T">你的模型</typeparam>
        /// <param name="methodName">后台推送的方法名</param>
        /// <param name="onReceived">收到数据</param>
        public void Start<T>(string methodName, Action<T> onReceived)
        {
            try
            {
                // 🔥 泛型接收：支持任何模型
                _hubProxy.On<T>(
                    methodName,
                    data =>
                    {
                        onReceived?.Invoke(data);
                    }
                );

                _connection.Start().Wait();
                _isConnected = true;
                Log.Information($"{methodName}✅ 连接成功");
            }
            catch
            {
                Reconnect();
            }
        }

        private async void Reconnect()
        {
            _isConnected = false;

            while (!_isConnected)
            {
                try
                {
                    Log.Information($"🔌 正在重连...");
                    await _connection.Start();
                    _isConnected = true;
                    Log.Information($"✅ 重连成功");
                }
                catch
                {
                    await Task.Delay(2000);
                }
            }
        }

        public void Stop()
        {
            _isConnected = false;
            _connection?.Stop();
        }
    }
}
