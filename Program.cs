using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace rebooter
{
    class Program
    {
        static int Main(string[] args)
        {
            TimeSpan waitBeforeReboot = TimeSpan.FromMinutes(10);

            if (args.Length > 0)
            {
                long rebootTimeInSecs = 0;
                if (long.TryParse(args[0], out rebootTimeInSecs))
                {
                    waitBeforeReboot = TimeSpan.FromSeconds(rebootTimeInSecs);
                }
                else
                {
                    Console.Error.WriteLine("First argument should be number of inactive user seconds after which to restart the machine.");
                    return 1;
                }
            }

            Console.WriteLine("Will restart machine after {0} seconds of inactivity.", waitBeforeReboot.TotalSeconds);

            while (true)
            {
                Thread.Sleep((int) waitBeforeReboot.TotalMilliseconds);

                var seconds = GetLastInputTime();
                Console.WriteLine("{0} : No user input for last {1} seconds", DateTime.Now, seconds);

                if (seconds >= waitBeforeReboot.TotalSeconds)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0 -f");
                    return 0;
                }
            }
        }

        static uint GetLastInputTime()
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf( lastInputInfo );
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if ( GetLastInputInfo( ref lastInputInfo ) )
            {
                uint lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }
            else
            {
                Console.Error.WriteLine("{0} : GetLastInputInfo returned false.", DateTime.Now);
            }

            return (( idleTime > 0 ) ? ( idleTime / 1000 ) : 0);
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout( LayoutKind.Sequential )]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }
    }
}
