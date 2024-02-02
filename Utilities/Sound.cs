using System.Media;
using System.Text;
using Win32.LowLevel;

namespace ConsoleGame
{
    public class Sound
    {
        readonly struct AudioDevice : IDisposable
        {
            readonly string Alias;
            readonly StringBuilder sb = new();

            public AudioDevice(string file)
            {
                Alias = Path.GetFileName(file);
                _ = MCI.mciSendStringW($"open \"{file}\" alias {Alias}", sb, 0, IntPtr.Zero);
            }

            public void Dispose()
            {
                _ = MCI.mciSendStringW($"close {Alias}", sb, 0, IntPtr.Zero);
            }

            public void Play()
            {
                _ = MCI.mciSendStringW($"play {Alias}", sb, 0, IntPtr.Zero);
            }
        }

        static readonly Dictionary<string, SoundPlayer> Players = new();
        static SoundPlayer GetPlayer(string file)
        {
            if (!Players.TryGetValue(file, out SoundPlayer? player))
            {
                player = new SoundPlayer(file);
                player.Load();
                Players.Add(file, player);
            }
            return player;
        }

        static readonly Dictionary<string, AudioDevice> Devices = new();
        static AudioDevice GetDevice(string file)
        {
            if (!Devices.TryGetValue(file, out AudioDevice device))
            {
                device = new AudioDevice(file);
                Devices.Add(file, device);
            }
            return device;
        }

        public static unsafe void Play(string? file)
        {
            if (string.IsNullOrEmpty(file)) return;

            // uint error;
            // MCI_OPEN_PARMS mciOpenParams = default;
            // 
            // fixed (char* ptr1 = "waveaudio")
            // fixed (char* ptr2 = file)
            // {
            //     mciOpenParams.lpstrDeviceType = ptr1;
            //     mciOpenParams.lpstrElementName = ptr2;
            //     if ((error = MCI.mciSendCommandW(0, 0x0803, 0x00002000 | 0x00000200, (nint)(&mciOpenParams))) != 0)
            //     {
            //         throw new MciException(error);
            //     }
            // }
            // 
            // uint wDeviceID = mciOpenParams.wDeviceID;
            // MCI_PLAY_PARMS mciPlayParams;
            // if ((error = MCI.mciSendCommandW(wDeviceID, 0x0806, 0x00000001, (nint)(&mciPlayParams))) != 0)
            // {
            //     _ = MCI.mciSendCommandW(wDeviceID, 0x0804, 0, 0);
            //     throw new MciException(error);
            // }
            // 
            // return;

            AudioDevice device = GetDevice(file);
            device.Play();
        }

        public static void Dispose()
        {
            foreach (AudioDevice device in Devices.Values)
            { device.Dispose(); }
            Devices.Clear();
        }
    }
}
