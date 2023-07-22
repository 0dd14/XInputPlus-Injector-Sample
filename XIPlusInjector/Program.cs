using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace XIPlusInjector
{
    class Program
    {

        /// <summary>
        /// Structure of XInputPlusLoaderSetting 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        class XInputPlusLoaderSetting
        {
            /* C/C+ Struct
            typedef struct XInputPlusLoaderSetting
            {
                WCHAR LoaderDLL32[MAX_PATH];
                WCHAR LoaderDLL64[MAX_PATH];
                WCHAR XInputDLL32[MAX_PATH];
                WCHAR DInputDLL32[MAX_PATH];
                WCHAR DInput8DLL32[MAX_PATH];
                WCHAR XInputDLL64[MAX_PATH];
                WCHAR DInputDLL64[MAX_PATH];
                WCHAR DInput8DLL64[MAX_PATH];
                WCHAR TargetProgram[MAX_PATH];
                WCHAR LoaderDir[MAX_PATH];
                bool HookChildProcess;
                bool Lunched;
            }
            */
            const int MAX_PATH = 260;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string LoaderDLL32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string LoaderDLL64;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string XInputDLL32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DInputDLL32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DInput8DLL32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string XInputDLL64;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DInputDLL64;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DInput8DLL64;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string TargetProgram;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string LoaderDir;
            [MarshalAs(UnmanagedType.U1)] public bool HookChildProcess;
            [MarshalAs(UnmanagedType.U1)] public bool Lunched;
        }


        static void Main(string[] args)
        {
            Console.WriteLine("XInputPlus Injector (Sample)");
            Console.WriteLine("copyright:2023 0dd14Lab https://0dd14lab.net");
            Console.WriteLine();


            if (args.Length < 1)
            {
                Console.WriteLine("usage:XIPlusInjector [TargetProcessID]");
                return;
            }

            Console.WriteLine("Inject XInputPlus to ProcessID [" + args[0] + "]...");

            string myDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            myDir = System.IO.Path.GetDirectoryName(myDir);

            /// インジェクターに各DLLの所在などを伝達するために、XInputPlusLoaderSetting構造体の内容をメモリーマップドファイルに書き出します。
            /// 対象プロセスにインジェクトされたDLLはがこの内容を読み込みXInpuPlusをインジェクトします。
            /// 
            /// Write contents of XInputPlusLoaderSetting structure to a memory-mapped file to tell the injector where each DLL is located, etc.
            /// DLLs injected into target process read this content and inject XInpuPlus.

            XInputPlusLoaderSetting setting = new XInputPlusLoaderSetting();
            setting.LoaderDLL32 = myDir +  @"\XInputPlusInjector.dll";
            setting.LoaderDLL64 = myDir +  @"\XInputPlusInjector64.dll";
            setting.XInputDLL32 = myDir +  @"\x86\xinput1_3.dl_";
            setting.DInputDLL32 = myDir +  @"\x86\dinput.dl_";
            setting.DInput8DLL32 = myDir + @"\x86\dinput8.dl_";
            setting.XInputDLL64 = myDir +  @"\x64\xinput1_3.dl_";
            setting.DInputDLL64 = myDir +  @"\x64\dinput.dl_";
            setting.DInput8DLL64 = myDir + @"\x64\dinput8.dl_";
            setting.TargetProgram = "";                                 //Internal use
            setting.LoaderDir = myDir;                                  //"XInputPlus.ini" in this folder is used
            setting.HookChildProcess = false;
            setting.Lunched = false;                                    //Intarnal use

            // Write Unmanged Struct to MemoryMappedFile
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("XInputPlusLoader", Marshal.SizeOf(typeof(XInputPlusLoaderSetting))))
            {
                using (MemoryMappedViewStream mmvs = mmf.CreateViewStream())
                {
                    // Struct to byte[]
                    int buffsize = Marshal.SizeOf(typeof(XInputPlusLoaderSetting));
                    byte[] buff = new byte[buffsize];
                    IntPtr ptr = Marshal.AllocCoTaskMem(buffsize);
                    Marshal.StructureToPtr(setting, ptr, false);
                    Marshal.Copy(ptr, buff, 0, buffsize);
                    Marshal.FreeCoTaskMem(ptr);

                    // Write to MemoryMappledFile
                    mmvs.Write(buff, 0, buffsize);
                    mmvs.Flush();
                }

                /// XInputPlusInjectorDLLに実装されているHookProcessを呼び出します。
                /// HookProcssはrundll32.exeからも呼び出せるように実装されれています。
                /// 対象プロセスのアーキテクチャ(x86/x64)が異なる場合、HookProcessは自動的に他のアーキテクチャのInjectorに処理をリダイレクトします。
                /// 
                /// Call "HookProcess" function implemented in XInputPlusInjectorDLL.
                /// HookProcess is also implemented to be called from rundll32.exe.
                /// If target process has a different architecture (x86/x64), HookProcess automatically redirects to Injector of other architecture.

                // > rundll32.exe XInputPlusInjector.dll,HookProces [Target PID] 
                String comline = "\"" + myDir + "\\XInputPlusInjector.dll\",HookProcess " + args[0];
                Process p = Process.Start("rundll32.exe", comline);
                p.WaitForExit();
            }

            Console.WriteLine("Done.");
        }
    }
}
