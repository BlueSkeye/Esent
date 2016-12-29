//-----------------------------------------------------------------------
// <copyright file="Win32NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>P/Invoke constants for Win32 functions.</summary>
//-----------------------------------------------------------------------

//namespace EsentLib.Win32
//{
    ///// <summary>P/Invoke methods for Win32 functions.</summary>
    //internal static class NativeMethods
    //{
//#if MANAGEDESENT_ON_CORECLR
//        /// <summary>The name of the DLL that holds the Core Memory API set.</summary>
//        private const string WinCoreMemoryDll = "api-ms-win-core-memory-l1-1-1.dll";

//        /// <summary>The name of the DLL that holds the Obsolete Heap API set.
//        (Might be api-ms-win-core-heap-obsolete-l1-1-0.dll.)</summary>
//        private const string HeapObsolete = "kernelbase";

//        /// <summary>The name of the DLL that holds the Core process/threads API set.</summary>
//        private const string WinCoreProcessThreads = "api-ms-win-core-processthreads-l1-1-1.dll";
//#else
        ///// <summary>The name of the DLL that holds the Core Memory API set.</summary>
        //private const string WinCoreMemoryDll = "kernel32.dll";

        ///// <summary>The name of the DLL that holds the Obsolete Heap API set.</summary>
        //private const string HeapObsolete = "kernel32.dll";

        ///// <summary>The name of the DLL that holds the Core process/threads API set.</summary>
        //private const string WinCoreProcessThreads = "kernel32.dll";
//#endif // MANAGEDESENT_ON_CORECLR || MANAGEDESENT_ON_WSA

        //[DllImport(WinCoreMemoryDll, SetLastError = true)]
        //internal static extern IntPtr VirtualAlloc(IntPtr plAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        //[DllImport(WinCoreMemoryDll, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        //[DllImport(HeapObsolete)]
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        //public static extern IntPtr LocalAlloc(int uFlags, UIntPtr sizetdwBytes);

        //[DllImport(HeapObsolete)]
        //public static extern IntPtr LocalFree(IntPtr hglobal);

        //// Win32 APIs that are white-listed for Windows Store Apps can be safely referenced here.
        //[DllImport(WinCoreProcessThreads)]
        //public static extern int GetCurrentProcessId();
    //}
//}
