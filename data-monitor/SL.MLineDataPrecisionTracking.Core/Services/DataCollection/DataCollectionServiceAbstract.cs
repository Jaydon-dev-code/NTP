using System;
using System.Threading;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Enum;

namespace SL.MLineDataPrecisionTracking.Core.Services.DataCollection
{
    /// <summary>
    /// 数据采集服务基类 — 基于 握手 → 交互 → 通知 三阶段模型
    /// </summary>
    public abstract class DataCollectionServiceAbstract
    {
        public string ServiceName => _serviceName;
        /// <summary>
        /// 服务名称
        /// </summary>
       protected abstract string _serviceName { get; }
        /// <summary>
        /// 间隔时间
        /// </summary>
        TimeSpan _sleepTimeSpan { get; set; }= TimeSpan.FromMilliseconds(500);

        /// <summary>服务运行状态</summary>
        public ServiceStatusEnum Status { get; private set; } = ServiceStatusEnum.Stopped;

        /// <summary>当前步骤描述，用于外部观察执行进度</summary>
        public string Description { get; private set; } = string.Empty;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _serviceTask;

        /// <summary>启动服务</summary>
        public void Start()
        {
            if (Status == ServiceStatusEnum.Running)
            {
                Serilog.Log.Warning("[服务控制]【{_serverName}】服务已在运行中", _serviceName);
                return;
            }

            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            _serviceTask = Task.Run(
                () => ExecuteAsync(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token
            );
        }

        /// <summary>停止服务</summary>
        public void Stop()
        {
            if (Status == ServiceStatusEnum.Stopped)
                return;

            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = null;
            _serviceTask = null;
            Status = ServiceStatusEnum.Stopped;
            Description = "已停止";
        }

        /// <summary>重启服务</summary>
        public void Restart()
        {
            Stop();
            Task.Delay(1000).Wait();
            Start();
        }

        async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Status = ServiceStatusEnum.Running;
                Description = "正在初始化...";

                if ((await InitAsync()).IsSuccess is false)
                {
                    Description = "初始化失败";
                    Status = ServiceStatusEnum.Stopped;
                    return;
                }

                Description = "初始化完成，等待握手";

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // 第一步：握手 — 判断是否具备交互条件
                        Description = "握手中...";
                        var handshake = await HandshakeAsync();
                        if (handshake.IsSuccess is false)
                        {
                            Description = "握手失败，等待重试";
                            continue;
                        }

                        // 第二步：交互 — 执行核心数据交互
                        Description = "交互中...";
                        var interact = await InteractAsync();
                        if (interact.IsSuccess)
                        {
                            // 第三步：通知 — 交互完成后的处理
                            Description = "交互成功，通知中...";
                            Serilog.Log.Information(
                                "[数据交互]【{_serverName}】{@Data}",
                                _serviceName,
                                interact.Data
                            );

                            await NotifyAsync(interact);
                            Description = "通知完成，等待下次握手";
                        }
                        else
                        {
                            Description = "交互失败";
                        }
                    }
                    catch (Exception ex)
                    {
                        Description = $"交互异常: {ex.Message}";
                        Serilog.Log.Warning(
                            "[数据交互]{_serverName}交互失败:{ex.Message}",
                            _serviceName,
                            ex.Message
                        );
                    }
                    finally
                    {
                        await Task.Delay(_sleepTimeSpan, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Description = "服务已停止";
                Serilog.Log.Information("[服务控制]【{_serverName}】服务已停止", _serviceName);
            }
            catch (Exception ex)
            {
                Description = $"初始化异常: {ex.Message}";
                Serilog.Log.Warning(
                    "[数据初始化]{_serverName}初始化失败:{ex.Message}",
                    _serviceName,
                    ex.Message
                );
            }
            finally
            {
                Status = ServiceStatusEnum.Stopped;
                Description = "已停止";
            }
        }

        /// <summary>初始化资源，返回 false 则服务无法启动</summary>
        protected abstract Task<Result> InitAsync();

        /// <summary>
        /// 握手 — 判断当前是否具备交互条件
        /// </summary>
        /// <returns></returns>
        protected abstract Task<Result> HandshakeAsync();

        /// <summary>
        /// 交互 — 执行核心数据交互逻辑
        /// </summary>
        /// <returns></returns>
        protected abstract Task<Result<object>> InteractAsync();

        /// <summary>
        /// 通知 — 交互完成后的后续处理
        /// </summary>
        /// <returns></returns>
        protected abstract Task NotifyAsync(Result<object> interact);
    }
}
