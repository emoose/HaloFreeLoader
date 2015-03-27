using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HaloFreeLoader
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesWritten);


        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        static void Main(string[] args)
        {
            if(args.Length <= 0)
            {
                Console.WriteLine("usage: HaloFreeLoader.exe <mapfilename>");
                Console.WriteLine("eg. HaloFreeLoader.exe s3d_turf");
                Console.WriteLine("Run that after the game gets to the 15 second countdown");
            }

            string mapFile = args[0];

            Process[] procs = Process.GetProcessesByName("eldorado");
            if (procs.Length <= 0)
            {
                Console.WriteLine("Couldn't find eldorado.exe, make sure the game is running.\nPress enter to continue.");
                Console.ReadLine();
                return;
            }

            IntPtr p = OpenProcess(0x001F0FFF, true, procs[0].Id); // 0x001F0FFF = all rights

            bool forceLoad = true;
            string map = @"maps\" + mapFile; //riverworld";

            Console.WriteLine("Patching game shutdown...");
            // disable game shutdown after timer runs out
            // note: the game constantly runs this code to try and close down the game after it's patched, making the game use a lot of CPU
            // im too lazy to fix that
            byte[] retn = { 0xC3 };
            int lpNumberOfBytesWritten = 0;
            WriteProcessMemory(p, (IntPtr)(0x5056D0), retn, 1, out lpNumberOfBytesWritten);

            if(forceLoad)
            {
                Console.WriteLine("Forceloading map \"" + map + "\"...");
                byte[] mapReset = { 0x1 };
                byte[] mapType = { 0x1, 0, 0, 0 };
                byte[] mapName = Encoding.ASCII.GetBytes(map);
                
                WriteProcessMemory(p, (IntPtr)(0x23917F0), mapReset, 1, out lpNumberOfBytesWritten);
                WriteProcessMemory(p, (IntPtr)(0x2391800), mapType, 4, out lpNumberOfBytesWritten);
                WriteProcessMemory(p, (IntPtr)(0x2391824), mapName, mapName.Length, out lpNumberOfBytesWritten);
            }
        }
    }
}
