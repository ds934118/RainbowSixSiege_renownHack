﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace R6Shack {
    public abstract class Helper {
        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead
        );

        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        [DllImport("kernel32.dll")]
        private static extern void CloseHandle(
            IntPtr hObject
        );


        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            int[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesWritten
        );

        public static int GetPidByProcessName(string processName) {
            Process[] arrayProcess = Process.GetProcessesByName(processName);
            foreach (Process p in arrayProcess) {
                return p.Id;
            }
            return 0;
        }

        [HandleProcessCorruptedStateExceptions]
        public static long ReadMemoryValue(long baseAddress, string processName) {
            byte[] buffer = new byte[8];
            IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName));
            ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 8, IntPtr.Zero);
            CloseHandle(hProcess);
            return Marshal.ReadInt64(byteAddress); 
        }

        [HandleProcessCorruptedStateExceptions]
        public static void WriteMemoryValue(long baseAddress, string processName, int value) {
            IntPtr hProcess = OpenProcess(0x1F0FFF, false, GetPidByProcessName(processName));
            while (true) {
                try {
                    WriteProcessMemory(hProcess, (IntPtr)baseAddress, new int[] { value }, 4, IntPtr.Zero);
                    CloseHandle(hProcess);
                    return;
                } catch (AccessViolationException) {
                    Thread.Sleep(500);
                    continue;
                }
            }   
        }
    }
}
