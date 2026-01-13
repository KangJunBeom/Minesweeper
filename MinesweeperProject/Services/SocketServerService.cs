using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinesweeperProject.Services
{
    public class SocketServerService
    {
        private const int Port_ = 30000;

        private TcpListener? Listener_;
        private TcpClient? Client_;
        private CancellationTokenSource? CTS_;

        public bool IsHosting_ { get; private set; }
        public bool IsConnected_ => Client_?.Connected == true;

        public event Action? ClientConnected_;
        public event Action<string>? Log_;

        public event Action<string>? JsonReceived_;
        public event Action? Disconnected_;

        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// 비동기 서버 시작 + 클라이언트 접속 무한 대기
        /// </summary>
        public async Task StartServerAsync(string Ip)
        {
            StopServer(); // 중복 실행 방지

            CTS_ = new CancellationTokenSource();

            try
            {
                IPAddress BindIp_ = IPAddress.Parse(Ip);

                Listener_ = new TcpListener(BindIp_, Port_);
                Listener_.Start();
                IsHosting_ = true;

                Log_?.Invoke($"[HOST] Listening {Ip}:{Port_}");

                Client_ = await Listener_.AcceptTcpClientAsync(CTS_.Token);

                Log_?.Invoke("[HOST] Client connected");
                ClientConnected_?.Invoke();
                _ = Task.Run(() => ReceiveLoopAsync(CTS_.Token));
            }
            catch (OperationCanceledException)
            {
                Log_?.Invoke("[HOST] Canceled");
                StopServer();
            }
            catch (Exception Ex_)
            {
                Log_?.Invoke("[HOST] " + Ex_.Message);
                StopServer();
                throw;
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            if (Client_ == null) return;

            try
            {
                var stream = Client_.GetStream();
                byte[] lenBuf = new byte[4];

                while (!ct.IsCancellationRequested && Client_?.Connected == true)
                {
                    await ReadExactlyAsync(stream, lenBuf, 4, ct);
                    int length = BitConverter.ToInt32(lenBuf, 0);

                    if (length <= 0 || length > 50_000_000)
                        throw new IOException($"Invalid payload length: {length}");

                    byte[] payload = new byte[length];
                    await ReadExactlyAsync(stream, payload, length, ct);

                    string json = Encoding.UTF8.GetString(payload);
                    JsonReceived_?.Invoke(json);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                Log_?.Invoke("[HOST] ReceiveLoop error: " + ex.Message);
            }
            finally
            {
                Disconnected_?.Invoke();
                StopServer();
            }
        }

        public void StopServer()
        {
            try { CTS_?.Cancel(); } catch { }

            try { Client_?.Close(); } catch { }
            try { Listener_?.Stop(); } catch { }

            Client_ = null;
            Listener_ = null;

            IsHosting_ = false;

            try { CTS_?.Dispose(); } catch { }
            CTS_ = null;
        }


        public async Task SendAsync(string json)
        {
            if (Client_ == null || !Client_.Connected)
                return;

            await _sendLock.WaitAsync();
            try
            {
                NetworkStream stream = Client_.GetStream();

                byte[] payload = Encoding.UTF8.GetBytes(json);
                byte[] length = BitConverter.GetBytes(payload.Length);

                await stream.WriteAsync(length, 0, length.Length);
                await stream.WriteAsync(payload, 0, payload.Length);
                await stream.FlushAsync();
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public static async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken ct)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset, ct);
                if (read == 0) throw new IOException("Remote closed the connection.");
                offset += read;
            }
        }
    }
}
