using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinesweeperProject.Services
{
    public class SocketClientService
    {
        private const int Port_ = 30000;

        private TcpClient? Client_;
        private CancellationTokenSource? CTS_;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public bool IsConnected_ => Client_?.Connected == true;

        // 이벤트 (필요 최소)
        public event Action? Connected_;
        public event Action<string>? Log_;

        public event Action<string>? JsonReceived_;
        public event Action? Disconnected_;


        /// <summary>
        /// 서버에 비동기로 접속 (무한 대기)
        /// </summary>
        public async Task ConnectAsync(string Ip)
        {
            Disconnect(); // 중복 방지

            CTS_ = new CancellationTokenSource();

            try
            {
                Client_ = new TcpClient();

                Log_?.Invoke($"[CLIENT] Connecting {Ip}:{Port_}");

                await Client_.ConnectAsync(Ip, Port_, CTS_.Token);

                Log_?.Invoke("[CLIENT] Connected");
                Connected_?.Invoke();
                _ = Task.Run(() => ReceiveLoopAsync(CTS_.Token));
            }
            catch (OperationCanceledException)
            {
                Log_?.Invoke("[CLIENT] Canceled");
                Disconnect();
            }
            catch (Exception Ex_)
            {
                Log_?.Invoke("[CLIENT] " + Ex_.Message);
                Disconnect();
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
                Log_?.Invoke("[CLIENT] ReceiveLoop error: " + ex.Message);
            }
            finally
            {
                Disconnected_?.Invoke();
                Disconnect();
            }
        }

        public void Disconnect()
        {
            try { CTS_?.Cancel(); } catch { }

            try { Client_?.Close(); } catch { }

            Client_ = null;

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