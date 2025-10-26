using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Files_Transfer.UserControls;

namespace Files_Transfer.Pages;

public partial class TaskPage : Page
{
    public readonly List<TransferSend> TransferSendList = [];
    public readonly List<TransferReceive> TransferReceiveList = [];
    private string _filePath = "";
    public TaskPage()
    {
        InitializeComponent();
    }
    
    // 发送 由发送页面发送按钮触发
    public void StartSend(Socket mainSocket, string path)
    {
        _filePath = path;
        new Thread(() =>
        {
            SendTread(mainSocket);
        }){IsBackground = true}.Start();
    }
    
    // 接受 服务端连接后触发
    public void StartServerReceive(Socket mainSocket, string selfIp)
    {
        new Thread(() =>
        { 
            ServerReceiveThread(mainSocket, selfIp);
        }){IsBackground = true}.Start();
    }
    
    // 接受 客户端连接后触发
    public void StartClientReceive(Socket mainSocket, string targetIp)
    {
        new Thread(() =>
        { 
            ClientReceiveThread(mainSocket, targetIp);
        }){IsBackground = true}.Start();
    }

    // 创建传输Socket ====================================================================================================
    private void SendTread(Socket mainSocket)
    {
        var buffer = new byte[1];
        buffer[0] = 1;
        mainSocket.Send(buffer); // 告知对方建立传输任务
    }

    private void ServerReceiveThread(Socket mainSocket, string selfIp)
    {
        var buffer = new byte[1];
        while (true)
        {
            try
            {
                mainSocket.Receive(buffer); // 主socket，负责发现文件发送消息，监听是否断开连接
            }
            catch (SocketException)
            {
                ForcedCancelAll();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    new MessageWindow("警告", "对方断开连接")
                    {
                        Owner = Application.Current.MainWindow
                    }.ShowDialog();
                    MainWindow._networkPage.ServerPage.Over(null, null);
                }));
                return;
            }

            switch (buffer[0])
            {
                // 对方发送文件，接受方触发
                case 1:
                {
                    var index = 0;
                    var listenSocket1 = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var listenSocket2 = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var port1 = 0;
                    for (var port = 65535; port > 0; port--)
                    {
                        try
                        {
                            if (index == 0)
                            {
                                listenSocket1.Bind(new IPEndPoint(IPAddress.Parse(selfIp), port));
                            }
                            else
                            {
                                listenSocket2.Bind(new IPEndPoint(IPAddress.Parse(selfIp), port));
                            }
                        }
                        catch (SocketException)
                        {
                            if (port == 1)
                            {
                                buffer[0] = 2;
                                mainSocket.Send(buffer); // 告知对方即将发送端口
                                var portBuffer = BitConverter.GetBytes(0);
                                mainSocket.Send(portBuffer); // 发送0端口告知对方无法建立连接
                                mainSocket.Send(portBuffer); // 发送0端口告知对方无法建立连接
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    new MessageWindow("错误", "所有端口均被占用")
                                    {
                                        Owner = Application.Current.MainWindow
                                    }.ShowDialog();
                                }));
                                break;
                            }
                            continue;
                        }
                        if (index == 0)
                        {
                            index++;
                            port1 = port;
                        }
                        else
                        {
                            buffer[0] = 2;
                            mainSocket.Send(buffer); // 告知对方即将发送端口
                            var portBuffer1 = BitConverter.GetBytes(port1);
                            var portBuffer2 = BitConverter.GetBytes(port);
                            mainSocket.Send(portBuffer1); // 发送端口1
                            mainSocket.Send(portBuffer2); // 发送端口2
                            listenSocket1.Listen(1);
                            listenSocket2.Listen(1);
                            var controlReceiveSocket = listenSocket1.Accept(); // 等待连接
                            var transferReceiveSocket = listenSocket2.Accept(); // 等待连接
                            listenSocket1.Close();
                            listenSocket2.Close();
                            TransferReceiveList.Add(new TransferReceive(controlReceiveSocket, transferReceiveSocket));
                            break;
                        }
                    }
                    break;
                }
                // 我方发送文件，发送方触发
                case 2:
                {
                    var index = 0;
                    var listenSocket1 = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var listenSocket2 = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var port1 = 0;
                    for (var port = 65535; port > 0; port--)
                    {
                        try
                        {
                            if (index == 0)
                            {
                                listenSocket1.Bind(new IPEndPoint(IPAddress.Parse(selfIp), port));
                            }
                            else
                            {
                                listenSocket2.Bind(new IPEndPoint(IPAddress.Parse(selfIp), port));
                            }
                        }
                        catch (SocketException)
                        {
                            if (port == 1)
                            {
                                var portBuffer = BitConverter.GetBytes(0);
                                mainSocket.Send(portBuffer); // 发送0端口告知对方无法建立连接
                                mainSocket.Send(portBuffer); // 发送0端口告知对方无法建立连接
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    new MessageWindow("错误", "所有端口均被占用")
                                    {
                                        Owner = Application.Current.MainWindow
                                    }.ShowDialog();
                                }));
                                break;
                            }
                            continue;
                        }
                        if (index == 0)
                        {
                            index++;
                            port1 = port;
                        }
                        else
                        {
                            var portBuffer1 = BitConverter.GetBytes(port1);
                            var portBuffer2 = BitConverter.GetBytes(port);
                            mainSocket.Send(portBuffer1); // 发送端口1
                            mainSocket.Send(portBuffer2); // 发送端口2
                            listenSocket1.Listen(1);
                            listenSocket2.Listen(1);
                            var controlSendSocket = listenSocket1.Accept(); // 等待连接
                            var transferSendSocket = listenSocket2.Accept(); // 等待连接
                            listenSocket1.Close();
                            listenSocket2.Close();
                            TransferSendList.Add(new TransferSend(controlSendSocket, transferSendSocket, _filePath));
                            break;
                        }
                    }
                    break;
                }
                default:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageWindow("错误", "收到未知命令(发送文件时断开连接)")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        MainWindow._sendPage.SendButton.IsEnabled = true;
                        MainWindow._sendPage.SendingLabel.Visibility = Visibility.Collapsed;
                        MainWindow._sendPage.SuccessLabel.Visibility = Visibility.Collapsed;
                        MainWindow._sendPage.FailLabel.Visibility = Visibility.Visible;
                        MainWindow._sendPage.InfoLabel.Content = "发送失败";
                    }));
                    ForcedCancelAll();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageWindow("警告", "对方断开连接")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        MainWindow._networkPage.ServerPage.Over(null, null);
                    }));
                    return;
            }
        }
    }
    private void ClientReceiveThread(Socket mainSocket, string targetIp)
    {
        var buffer = new byte[1];
        while (true)
        {
            try
            {
                mainSocket.Receive(buffer); // 主socket，负责发现文件发送消息，监听是否断开连接
            }
            catch (SocketException)
            {
                ForcedCancelAll();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    new MessageWindow("警告", "对方关闭服务")
                    {
                        Owner = Application.Current.MainWindow
                    }.ShowDialog();
                    MainWindow._networkPage.ClientPage.Over(null, null);
                }));
                return;
            }

            switch (buffer[0])
            {
                // 对方发送文件，接受方触发
                case 1:
                {
                    buffer[0] = 2;
                    mainSocket.Send(buffer); // 回复对方
                    var portBuffer1 = new byte[4];
                    var portBuffer2 = new byte[4];
                    mainSocket.Receive(portBuffer1); // 获取端口1
                    mainSocket.Receive(portBuffer2); // 获取端口2
                    var port1 = BitConverter.ToInt32(portBuffer1);
                    var port2 = BitConverter.ToInt32(portBuffer2);
                    if (port1 == 0 || port2 == 0)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            new MessageWindow("错误", "对方端口均被占用")
                            {
                                Owner = Application.Current.MainWindow
                            }.ShowDialog();
                            MainWindow._sendPage.SendButton.IsEnabled = true;
                            MainWindow._sendPage.SendingLabel.Visibility = Visibility.Collapsed;
                            MainWindow._sendPage.SuccessLabel.Visibility = Visibility.Collapsed;
                            MainWindow._sendPage.FailLabel.Visibility = Visibility.Visible;
                            MainWindow._sendPage.InfoLabel.Content = "发送失败";
                        }));
                    }
                    else
                    {
                        var controlReceiveSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        var transferReceiveSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        controlReceiveSocket.Connect(new IPEndPoint(IPAddress.Parse(targetIp), port1));
                        transferReceiveSocket.Connect(new IPEndPoint(IPAddress.Parse(targetIp), port2));
                        TransferReceiveList.Add(new TransferReceive(controlReceiveSocket, transferReceiveSocket));
                    }
                    break;
                }
                // 我方发送文件，发送方触发
                case 2:
                {
                    var portBuffer1 = new byte[4];
                    var portBuffer2 = new byte[4];
                    mainSocket.Receive(portBuffer1); // 获取端口1
                    mainSocket.Receive(portBuffer2); // 获取端口2
                    var port1 = BitConverter.ToInt32(portBuffer1);
                    var port2 = BitConverter.ToInt32(portBuffer2);
                    if (port1 == 0 || port2 == 0)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            new MessageWindow("错误", "对方端口均被占用")
                            {
                                Owner = Application.Current.MainWindow
                            }.ShowDialog();
                            MainWindow._sendPage.SendButton.IsEnabled = true;
                            MainWindow._sendPage.SendingLabel.Visibility = Visibility.Collapsed;
                            MainWindow._sendPage.SuccessLabel.Visibility = Visibility.Collapsed;
                            MainWindow._sendPage.FailLabel.Visibility = Visibility.Visible;
                            MainWindow._sendPage.InfoLabel.Content = "发送失败";
                        }));
                    }
                    else
                    {
                        var controlSendSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        var transferSendSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        controlSendSocket.Connect(new IPEndPoint(IPAddress.Parse(targetIp), port1));
                        transferSendSocket.Connect(new IPEndPoint(IPAddress.Parse(targetIp), port2));
                        TransferSendList.Add(new TransferSend(controlSendSocket, transferSendSocket, _filePath));
                    }
                    break;
                }
                default:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageWindow("错误", "收到未知命令(发送文件时断开连接)")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        MainWindow._sendPage.SendButton.IsEnabled = true;
                        MainWindow._sendPage.SendingLabel.Visibility = Visibility.Collapsed;
                        MainWindow._sendPage.SuccessLabel.Visibility = Visibility.Collapsed;
                        MainWindow._sendPage.FailLabel.Visibility = Visibility.Visible;
                        MainWindow._sendPage.InfoLabel.Content = "发送失败";
                    }));
                    ForcedCancelAll();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageWindow("警告", "对方关闭服务")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        MainWindow._networkPage.ClientPage.Over(null, null);
                    }));
                    return;
            }
        }
    }

    // 传输类 ===========================================================================================================
    public class TransferSend
    {
        private readonly Socket _controlSocket;
        private readonly Socket _transferSocket;
        private readonly string _path;
        private string _fileName = "";
        private long _size;
        private long _nowSize;
        private bool _selfFinish;
        private double _waitTime;
        private bool _cancel;
        private TaskBar _taskbar;
        public bool Work;
        public bool Run = true;
        public double Speed;
        public TransferSend(Socket controlSendSocket, Socket transferSendSocket, string path)
        {
            _controlSocket = controlSendSocket;
            _transferSocket = transferSendSocket;
            _path = path;
            new Thread(MainTread){IsBackground = true}.Start();
        }

        // 发送基本信息
        private void MainTread()
        {
            var file = new FileInfo(_path);
            _size = file.Length;
            _fileName = file.Name;
            var infoName = Encoding.UTF8.GetBytes(_fileName);
            var infoNameLength = BitConverter.GetBytes(infoName.Length);
            _controlSocket.Send(infoNameLength);
            _controlSocket.Send(infoName);
            _controlSocket.Send(BitConverter.GetBytes(_size));
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar= new TaskBar
                {
                    Margin = new Thickness(0, 0, 15, 20),
                    Height = 120
                };
                MainWindow._taskPage.TaskStackPanel.Children.Add(_taskbar);
                _taskbar.MainBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Content = "对方未回复";
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.ResumeButton.IsEnabled = false;
                _taskbar.RemoveButton.IsEnabled = false;
                _taskbar.FileName = file.Name;
                _taskbar.DetailSizeLabel.Content = $"0 B / {file.Length} B";
                _taskbar.InstantSize = $"0.0 B / {SizeConvert(file.Length, 1, false)}";
                _taskbar.PathLabel.Content = _path;
                _taskbar.PauseButton.Click += Pause;
                _taskbar.ResumeButton.Click += Resume;
                _taskbar.RemoveButton.Click += Remove;
                
                MainWindow._sendPage.SendButton.IsEnabled = true;
                MainWindow._sendPage.SendingLabel.Visibility = Visibility.Collapsed;
                MainWindow._sendPage.SuccessLabel.Visibility = Visibility.Visible;
                MainWindow._sendPage.FailLabel.Visibility = Visibility.Collapsed;
                MainWindow._sendPage.InfoLabel.Content = "发送成功";
                MainWindow._sendPage.PathTextBox.Text = "";
                MainWindow._sendPage.FileSizeLabel.Content = "";
            });
            var buffer = new byte[1];
            try
            {
                _controlSocket.Receive(buffer);
            }
            catch (SocketException)
            {
                return;
            }
            switch (buffer[0])
            {
                // 接受
                case 1:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _taskbar.RemindBorder.Visibility = Visibility.Hidden;
                    });
                    ContinueCheck();
                    new Thread(ControlTread){IsBackground = true}.Start();
                    break;
                // 拒绝
                case 2:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _taskbar.RemindLabel.Content = "对方已拒绝";
                        _taskbar.Opacity = 0.5;
                        _taskbar.IsHitTestVisible = false;
                    });
                    _controlSocket.Close();
                    _transferSocket.Close();
                    break;
                default:
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        new MessageWindow("错误", "收到未知命令2")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                    }));
                    break;
            }
        }
        
        // 断点续传检测
        private void ContinueCheck()
        {
            var buffer = new byte[1];
            try
            {
                _controlSocket.Receive(buffer);
            }
            catch (SocketException)
            {
                return;
            }
            switch (buffer[0])
            {
                // 无需断点续传
                case 2:
                    break;
                // 需要断点续传
                case 3:
                    var currentLengthBuffer = new byte[8];
                    try
                    {
                        _controlSocket.Receive(currentLengthBuffer);
                    }
                    catch (SocketException)
                    {
                        return;
                    }
                    _nowSize = BitConverter.ToInt64(currentLengthBuffer);
                    break;
            }
        }

        public void Pause(object sender, RoutedEventArgs e)
        {
            _controlSocket.Send(new byte[] { 1 });
            Run = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Collapsed;
                _taskbar.ResumeButton.Visibility = Visibility.Visible;
            });
        }
        
        public void Resume(object sender, RoutedEventArgs e)
        {
            _controlSocket.Send(new byte[] { 2 });
            Run = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Visible;
                _taskbar.ResumeButton.Visibility = Visibility.Collapsed;
            });
        }

        public void Remove(object sender, RoutedEventArgs e)
        {
            var confirm = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                confirm = (bool)new ConfirmWindow("确定要取消任务吗？", _fileName)
                {
                    Owner = Application.Current.MainWindow
                }.ShowDialog()!;
            });
            if (!confirm) return;
            Work = false;
            _cancel = true;
            _controlSocket.Send(new byte[] { 3 });
            _controlSocket.Close();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Content = "已取消";
                _taskbar.Opacity = 0.5;
                _taskbar.IsHitTestVisible = false;
            });
        }

        // 开始传输
        private void ControlTread()
        {
            new Thread(TransferThread){IsBackground = true}.Start();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Visible;
                _taskbar.ResumeButton.Visibility = Visibility.Collapsed;
                _taskbar.ResumeButton.IsEnabled = true;
                _taskbar.RemoveButton.IsEnabled = true;
            });
            Work = true;
            
            // 捕获暂停、开始、取消、完成
            while (Work)
            {
                var instruct = new byte[1];
                try
                {
                    _controlSocket.Receive(instruct);
                }
                catch (Exception)
                {
                    return;
                }
                switch (instruct[0])
                {
                    case 1:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Content = "对方已暂停";
                        });
                        continue;
                    case 2:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Hidden;
                        });
                        continue;
                    case 3:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Content = "对方已取消";
                            _taskbar.Opacity = 0.5;
                            _taskbar.IsHitTestVisible = false;
                        });
                        Work = false;
                        _controlSocket.Close();
                        _cancel = true;
                        break;
                    case 4:
                        while (!_selfFinish) Thread.Sleep(100);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush2");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush2");
                            _taskbar.RemindLabel.Content = "完成";
                            _taskbar.Opacity = 0.5;
                            _taskbar.IsHitTestVisible = false;
                        });
                        Work = false;
                        _controlSocket.Close();
                        break;
                    case 5:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Content = "文件错误";
                            _taskbar.Opacity = 0.5;
                            _taskbar.IsHitTestVisible = false;
                        });
                        Work = false;
                        _controlSocket.Close();
                        break;
                    default:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            new MessageWindow("错误", $"{_fileName}意外的指令")
                            {
                                Owner = Application.Current.MainWindow
                            }.ShowDialog();
                        });
                        continue;
                }
                break;
            }
        }

        private void TransferThread()
        {
            // 哈希值计算、发送
            var tempFile = new FileStream(_path, FileMode.Open);
            _controlSocket.Send(SHA256.Create().ComputeHash(tempFile));
            tempFile.Close();
            
            var file = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite);
            var buffer = new byte[1024];
            int length;
            new Thread(ProgressThread){IsBackground = true}.Start();
            file.Seek(_nowSize, SeekOrigin.Begin);
            while ((length = file.Read(buffer, 0, 1024)) != 0)
            {
                if (MainWindow._settingPage.Config.AppSettings.Settings["UpSpeedLimitSwitch"].Value == "true")
                {
                    Delay(_waitTime);
                }
                if (_cancel)
                {
                    _transferSocket.Close();
                    file.Close();
                    return;
                }
                while (!Run) Thread.Sleep(100);
                try
                {
                    _transferSocket.Send(buffer);
                }
                catch (Exception)
                {
                    // ignored
                }
                _nowSize += length;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush3");
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush3");
                _taskbar.RemindLabel.Content = "文件校验中...";
            });
            _transferSocket.Close();
            file.Close();
            _controlSocket.Send(new byte[] { 4 });
            _selfFinish = true;
            Work = false;
        }

        private void ProgressThread()
        {
            var lastTime = DateTime.Now;
            long lastLength = 0;
            var roundedTotalSize = SizeConvert(_size, 1, false);
            var run = 0;
            double coefficient = 6; // 0最快 6最慢
            var buff = 0; // 追赶增益
            var keepUpTimes = 1; // 连续追赶次数
            double lastDifference = 0;
            while (!_cancel && run != 2)
            {
                var value = (double)_nowSize / _size * 100;
                var percent = Math.Round(value, 1) + " %";
                var nowTime = DateTime.Now;
                var speed = (_nowSize - lastLength) / (nowTime - lastTime).TotalSeconds;
                Speed = speed;
                lastTime = nowTime;
                lastLength = _nowSize;
                var restTime = 0;
                if (_size - _nowSize != 0 && speed != 0)
                {
                    restTime = (int)((_size - _nowSize) / speed);
                } else if (_size - _nowSize != 0 && speed == 0)
                {
                    restTime = -1;
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _taskbar.ProgressValue = value;
                    _taskbar.InstantSize = SizeConvert(_nowSize, 1, false) + " / " + roundedTotalSize;
                    _taskbar.Percent = percent;
                    _taskbar.UpSpeed = "\u2191 " + SpeedConvert(speed, 1);
                    _taskbar.RestTime = "剩余 " + TimeConvert(restTime);
                    _taskbar.DetailSizeLabel.Content = $"{_nowSize} B / {_size} B";
                }));
                if (_nowSize == _size) run++;
                // 限速计算
                if (MainWindow._settingPage.Config.AppSettings.Settings["UpSpeedLimitSwitch"].Value == "true" && Run)
                {
                    var difference = speed - MainWindow._settingPage.UsLimit * 1024;
                    if (lastDifference * difference > 0)
                    {
                        if (++keepUpTimes >= 2)
                        {
                            buff++;
                        }
                    }
                    else
                    {
                        keepUpTimes = 0;
                        buff--;
                    }
                    switch (difference)
                    {
                        // 减速
                        case > 0:
                            coefficient += Math.Pow(2, buff);
                            break;
                        // 加速
                        case < 0:
                            coefficient -= Math.Pow(2, buff);
                            break;
                    }
                    if (coefficient < 0) coefficient = 0;
                    if (coefficient > 6) coefficient = 6;
                    _waitTime = Math.Pow(10, coefficient);
                    lastDifference = difference;
                }
                Thread.Sleep(MainWindow._settingPage.Time);
            }
            Speed = 0;
        }

        public void ForcedCancel()
        {
            Work = false;
            _cancel = true;
            _controlSocket.Close();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Content = "已强制取消";
                _taskbar.Opacity = 0.5;
                _taskbar.IsHitTestVisible = false;
            });
        }
    }
    
    // =================================================================================================================
    public class TransferReceive
    {
        private readonly Socket _controlSocket;
        private readonly Socket _transferSocket;
        private string _filename;
        private long _size;
        private long _nowSize;
        private string _savePath;
        private string _hash1;
        private string _hash2;
        private bool _hashError;
        private bool _selfFinish;
        private readonly bool _isHash = MainWindow._settingPage.Config.AppSettings.Settings["FileCompletenessCheck"].Value == "true";
        private double _waitTime = 1000000;
        private bool _cancel;
        private TaskBar _taskbar;
        public bool Work;
        public bool Run = true;
        public double Speed;
        public TransferReceive(Socket controlReceiveSocket, Socket transferReceiveSocket)
        {
            _controlSocket = controlReceiveSocket;
            _transferSocket = transferReceiveSocket;
            new Thread(MainTread){IsBackground = true}.Start();
        }

        // 获取基本信息
        private void MainTread()
        {
            var infoNameLength = new byte[4];
            try
            {
                _controlSocket.Receive(infoNameLength);
            }
            catch (SocketException)
            {
                return;
            }
            var nameLength = BitConverter.ToInt32(infoNameLength);
            var infoName = new byte[nameLength];
            try
            {
                _controlSocket.Receive(infoName);
            }
            catch (SocketException)
            {
                return;
            }
            _filename = Encoding.UTF8.GetString(infoName);
            var infoLength = new byte[8];
            try
            {
                _controlSocket.Receive(infoLength);
            }
            catch (SocketException)
            {
                return;
            }
            _size = BitConverter.ToInt64(infoLength);
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar = new TaskBar
                {
                    Margin = new Thickness(0, 0, 15, 20),
                    Height = 120
                };
                MainWindow._taskPage.TaskStackPanel.Children.Add(_taskbar);
                _taskbar.StartBorder.Visibility = Visibility.Visible;
                _taskbar.FileName = _filename;
                _taskbar.SizeLabel.Content = SizeConvert(_size, 5, true);
                _taskbar.Path = MainWindow._settingPage.Config.AppSettings.Settings["DownloadPath"].Value;
                _taskbar.AcceptButton.Click += Accept;
                _taskbar.RejectButton.Click += Reject;
                _taskbar.PauseButton.Click += Pause;
                _taskbar.ResumeButton.Click += Resume;
                _taskbar.RemoveButton.Click += Remove;
            });
            if (MainWindow._settingPage.Config.AppSettings.Settings["AutoReceive"].Value == "true")
            {
                Accept(null, null);
            }
        }
        
        public void Pause(object sender, RoutedEventArgs e)
        {
            _controlSocket.Send(new byte[] { 1 });
            Run = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Collapsed;
                _taskbar.ResumeButton.Visibility = Visibility.Visible;
            });
        }
        
        public void Resume(object sender, RoutedEventArgs e)
        {
            _controlSocket.Send(new byte[] { 2 });
            Run = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Visible;
                _taskbar.ResumeButton.Visibility = Visibility.Collapsed;
            });
        }

        public void Remove(object sender, RoutedEventArgs e)
        {
            var confirm = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                confirm = (bool)new ConfirmWindow("确定要取消任务吗？", _filename)
                {
                    Owner = Application.Current.MainWindow
                }.ShowDialog()!;
            });
            if (!confirm) return;
            Work = false;
            _cancel = true;
            _controlSocket.Send(new byte[] { 3 });
            _controlSocket.Close();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Content = "已取消";
                _taskbar.Opacity = 0.5;
                _taskbar.IsHitTestVisible = false;
            });
        }
        
        private void Accept(object sender, RoutedEventArgs e)
        {
            ContinueCheck();
            new Thread(ControlTread){IsBackground = true}.Start();
        }
        
        // 断点续传检测
        private void ContinueCheck()
        {
            // 判断是否存在同名文件
            var tempPath = "";
            Application.Current.Dispatcher.Invoke(() =>
            {
                tempPath = _taskbar.PathTextBox.Text;
            });
            if (!Directory.Exists(tempPath)) // 不存在文件夹
            {
                try
                {
                    Directory.CreateDirectory(tempPath);
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        new MessageWindow("错误", "无效路径")
                        {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                    });
                    return;
                }
                _controlSocket.Send(new byte[] { 1 }); // 同意接收
                _controlSocket.Send(new byte[] { 2 }); // 无需断点续传
                _savePath = tempPath;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _taskbar.StartBorder.Visibility = Visibility.Collapsed;
                    _taskbar.MainBorder.Visibility = Visibility.Visible;
                    _taskbar.PathLabel.Content = _savePath + "\\" + _filename;
                });
                return;
            }
            var filePath = tempPath + "\\" + _filename;
            if (File.Exists(filePath)) // 存在同名文件
            {
                _controlSocket.Send(new byte[] { 1 }); // 同意接收
                _controlSocket.Send(new byte[] { 3 }); // 需要断点续传
                _nowSize = new FileInfo(filePath).Length;
                _controlSocket.Send(BitConverter.GetBytes(_nowSize));
            }
            else
            {
                _controlSocket.Send(new byte[] { 1 }); // 同意接收
                _controlSocket.Send(new byte[] { 2 }); // 无需断点续传
            }
            _savePath = tempPath;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.StartBorder.Visibility = Visibility.Collapsed;
                _taskbar.MainBorder.Visibility = Visibility.Visible;
                _taskbar.PathLabel.Content = _savePath + "\\" + _filename;
            });
        }
        
        private void Reject(object sender, RoutedEventArgs e)
        {
            _controlSocket.Send(new byte[] { 2 });
            Application.Current.Dispatcher.Invoke(() =>
            {
                new MessageWindow("提醒", "已拒绝")
                {
                    Owner = Application.Current.MainWindow
                }.ShowDialog();
                _taskbar.Opacity = 0.5;
                _taskbar.IsHitTestVisible = false;
            });
            _controlSocket.Close();
            _transferSocket.Close();
        }

        // 开始传输
        private void ControlTread()
        {
            new Thread(TransferThread){IsBackground = true}.Start();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.PauseButton.Visibility = Visibility.Visible;
                _taskbar.ResumeButton.Visibility = Visibility.Collapsed;
            });
            Work = true;
            
            // 哈希值接收
            var tempBuffer = new byte[32];
            _controlSocket.Receive(tempBuffer);
            _hash1 = BitConverter.ToString(tempBuffer);
            
            // 捕获暂停、开始、取消、完成
            while (Work)
            {
                var instruct = new byte[1];
                try
                {
                    _controlSocket.Receive(instruct);
                }
                catch (Exception)
                {
                    return;
                }
                switch (instruct[0])
                {
                    case 1:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Content = "对方已暂停";
                        });
                        continue;
                    case 2:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Hidden;
                        });
                        continue;
                    case 3:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _taskbar.RemindBorder.Visibility = Visibility.Visible;
                            _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                            _taskbar.RemindLabel.Content = "对方已取消";
                            _taskbar.Opacity = 0.5;
                            _taskbar.IsHitTestVisible = false;
                        });
                        Work = false;
                        _controlSocket.Close();
                        _cancel = true;
                        break;
                    case 4:
                        while (!_selfFinish) Thread.Sleep(100);
                        if (_hashError)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                                _taskbar.RemindLabel.Content = "文件错误";
                                _taskbar.Opacity = 0.5;
                                _taskbar.IsHitTestVisible = false;
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush2");
                                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush2");
                                _taskbar.RemindLabel.Content = "完成";
                                _taskbar.Opacity = 0.5;
                                _taskbar.IsHitTestVisible = false;
                            });
                        }
                        Work = false;
                        _controlSocket.Close();
                        break;
                    default:
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            new MessageWindow("错误", $"{_filename}意外的指令")
                            {
                                Owner = Application.Current.MainWindow
                            }.ShowDialog();
                        });
                        continue;
                }
                break;
            }
        }

        private void TransferThread()
        {
            var file = new FileStream(_savePath + "\\" + _filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var buffer = new byte[1024];
            var length = 1024;
            new Thread(ProgressThread){IsBackground = true}.Start();
            file.Seek(_nowSize, SeekOrigin.Begin);
            while (_nowSize < _size)
            {
                if (MainWindow._settingPage.Config.AppSettings.Settings["DownSpeedLimitSwitch"].Value == "true")
                {
                    Delay(_waitTime);
                }
                if (_cancel)
                {
                    _transferSocket.Close();
                    file.Close();
                    return;
                }
                while (!Run) Thread.Sleep(100);
                try
                {
                    length = _transferSocket.Receive(buffer);
                }
                catch (Exception)
                {
                    // ignored
                }
                if ((_size - _nowSize) / 1024 == 0)
                {
                    length = Convert.ToInt32(_size - _nowSize);
                }
                file.Write(buffer, 0, length);
                _nowSize += length;
            }
            _transferSocket.Close();
            file.Close();
            if (_isHash)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _taskbar.RemindBorder.Visibility = Visibility.Visible;
                    _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush3");
                    _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush3");
                    _taskbar.RemindLabel.Content = "文件校验中...";
                });
                // 哈希值计算、比较
                var tempFile = new FileStream(_savePath + "\\" + _filename, FileMode.Open);
                _hash2 = BitConverter.ToString(SHA256.Create().ComputeHash(tempFile));
                tempFile.Close();
                if (_hash1 == _hash2)
                {
                    _controlSocket.Send(new byte[] { 4 });
                }
                else
                {
                    _controlSocket.Send(new byte[] { 5 });
                    _hashError = true;
                }
            }
            else
            {
                _controlSocket.Send(new byte[] { 4 });
            }
            _selfFinish = true;
            Work = false;
        }

        private void ProgressThread()
        {
            var lastTime = DateTime.Now;
            long lastLength = 0;
            var roundedTotalSize = SizeConvert(_size, 1, false);
            var run = 0;
            double coefficient = 6; // 0最快 6最慢
            var buff = 0; // 追赶增益
            var keepUpTimes = 1; // 连续追赶次数
            double lastDifference = 0;
            while (!_cancel && run != 2)
            {
                var value = (double)_nowSize / _size * 100;
                var percent = Math.Round(value, 1) + " %";
                var nowTime = DateTime.Now;
                var speed = (_nowSize - lastLength) / (nowTime - lastTime).TotalSeconds;
                Speed = speed;
                lastTime = nowTime;
                lastLength = _nowSize;
                var restTime = 0;
                if (_size - _nowSize != 0 && speed != 0)
                {
                    restTime = (int)((_size - _nowSize) / speed);
                } else if (_size - _nowSize != 0 && speed == 0)
                {
                    restTime = -1;
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _taskbar.ProgressValue = value;
                    _taskbar.InstantSize = SizeConvert(_nowSize, 1, false) + " / " + roundedTotalSize;
                    _taskbar.Percent = percent;
                    _taskbar.DownSpeed = "\u2193 " + SpeedConvert(speed, 1);
                    _taskbar.RestTime = "剩余 " + TimeConvert(restTime);
                    _taskbar.DetailSizeLabel.Content = $"{_nowSize} B / {_size} B";
                }));
                if (_nowSize == _size) run++;
                // 限速计算
                if (MainWindow._settingPage.Config.AppSettings.Settings["DownSpeedLimitSwitch"].Value == "true" && Run)
                {
                    var difference = speed - MainWindow._settingPage.DsLimit * 1024;
                    if (lastDifference * difference > 0)
                    {
                        if (++keepUpTimes >= 2)
                        {
                            buff++;
                        }
                    }
                    else
                    {
                        keepUpTimes = 0;
                        buff--;
                    }
                    switch (difference)
                    {
                        // 减速
                        case > 0:
                            coefficient += Math.Pow(2, buff);
                            break;
                        // 加速
                        case < 0:
                            coefficient -= Math.Pow(2, buff);
                            break;
                    }
                    if (coefficient < 0) coefficient = 0;
                    if (coefficient > 6) coefficient = 6;
                    _waitTime = Math.Pow(10, coefficient);
                    lastDifference = difference;
                }
                Thread.Sleep(MainWindow._settingPage.Time);
            }
            Speed = 0;
        }
        
        public void ForcedCancel()
        {
            Console.WriteLine("vcoascopqhwpcudqw");
            Work = false;
            _cancel = true;
            _controlSocket.Close();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _taskbar.RemindBorder.Visibility = Visibility.Visible;
                _taskbar.RemindBorder.BorderBrush = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Foreground = (Brush)Application.Current.FindResource("RemindTextBrush1");
                _taskbar.RemindLabel.Content = "已强制取消";
                _taskbar.Opacity = 0.5;
                _taskbar.IsHitTestVisible = false;
            });
        }
    }
    
    // 单位转换===========================================================================================================
    private static string SizeConvert(long size, int num, bool detail)
    {
        if (detail)
        {
            return size switch
            {
                < 1024 => $"{size} B",
                < 1024 * 1024 => $"{Math.Round(size / 1024.0, num)} KB ({size} B)",
                < 1024 * 1024 * 1024 => $"{Math.Round(size / 1024.0 / 1024, num)} MB ({size} B)",
                < 1024 * 1024 * 1024 * 1024L => $"{Math.Round(size / 1024.0 / 1024 / 1024, num)} GB ({size} B)",
                _ => $"{Math.Round(size / 1024.0 / 1024 / 1024 / 1024, num)} TB ({size} B)"
            };
        }

        return size switch
        {
            < 1024 => $"{size} B",
            < 1024 * 1024 => $"{Math.Round(size / 1024.0, num)} KB",
            < 1024 * 1024 * 1024 => $"{Math.Round(size / 1024.0 / 1024, num)} MB",
            < 1024 * 1024 * 1024 * 1024L => $"{Math.Round(size / 1024.0 / 1024 / 1024, num)} GB",
            _ => $"{Math.Round(size / 1024.0 / 1024 / 1024 / 1024, num)} TB"
        };
    }

    private static string SpeedConvert(double speed, int num)
    {
        return speed switch
        {
            < 1024 => $"{speed} B/s",
            < 1024 * 1024 => $"{Math.Round(speed / 1024, num)} KB/s",
            < 1024 * 1024 * 1024 => $"{Math.Round(speed / 1024 / 1024, num)} MB/s",
            < 1024 * 1024 * 1024 * 1024L => $"{Math.Round(speed / 1024 / 1024 / 1024, num)} GB/s",
            _ => $"{Math.Round(speed / 1024 / 1024 / 1024 / 1024, num)} TB/s"
        };
    }

    private static string TimeConvert(int seconds)
    {
        return seconds switch
        {
            < 0 => "\u221e秒",
            < 60 => $"{seconds}秒",
            < 3600 => $"{seconds / 60}分 {seconds % 60}秒",
            < 86400 => $"{seconds / 3600}时 {seconds % 3600 / 60}分 {seconds % 3600 % 60}秒",
            _ => $"{seconds / 86400}天 {seconds % 86400 / 3600}时 {seconds % 86400 % 3600 / 60}分 {seconds % 86400 % 3600 % 60}秒"
        };
    }

    // 总控按钮===========================================================================================================
    private void RemoveAll(object sender, RoutedEventArgs e)
    {
        var confirm = false;
        Application.Current.Dispatcher.Invoke(() =>
        {
            confirm = (bool)new ConfirmWindow("取消所有任务！", "确定要取消所有任务吗？")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog()!;
        });
        if (!confirm) return;
        foreach (var variable in TransferSendList.Where(variable => variable.Work))
        {
            variable.Remove(sender, e);
        }
        
        foreach (var variable in TransferReceiveList.Where(variable => variable.Work))
        {
            variable.Remove(sender, e);
        }
    }

    private void ResumeAll(object sender, RoutedEventArgs e)
    {
        foreach (var variable in TransferSendList.Where(variable => variable.Work).Where(variable => !variable.Run))
        {
            variable.Resume(sender, e);
        }

        foreach (var variable in TransferReceiveList.Where(variable => variable.Work).Where(variable => !variable.Run))
        {
            variable.Resume(sender, e);
        }
    }

    private void PauseAll(object sender, RoutedEventArgs e)
    {
        foreach (var variable in TransferSendList.Where(variable => variable.Work).Where(variable => variable.Run))
        {
            variable.Pause(sender, e);
        }
        
        foreach (var variable in TransferReceiveList.Where(variable => variable.Work).Where(variable => variable.Run))
        {
            variable.Pause(sender, e);
        }
    }

    private void ForcedCancelAll()
    {
        foreach (var variable in TransferSendList.Where(variable => variable.Work))
        {
            variable.ForcedCancel();
        }
        
        foreach (var variable in TransferReceiveList.Where(variable => variable.Work))
        {
            variable.ForcedCancel();
        }
    }
    
    // 延迟方法
    private static void Delay(double microseconds)
    {
        var stopTime = new System.Diagnostics.Stopwatch();
        stopTime.Start();
        while (stopTime.Elapsed.TotalMicroseconds < microseconds) { }
        stopTime.Stop();
    }
}