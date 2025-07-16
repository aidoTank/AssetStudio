using System;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public class LuaJitDecompileHandler : ILuaDecompileHandler
    {
        private const string LUAJIT_DECOMPILER_PATH = "ljdv2/luajit_decompiler_v2.exe";
        private const string DEPENDENCY_PATH = "Dependencies";
        private const string TEMP_FILE = "tempCompiledLua";
        private const string TEMP_OUTPUT_FILE = "tempCompiledLua.lua";

        public byte[] Decompile(LuaByteInfo luaByteInfo)
        {
            if (TryDecompile(luaByteInfo.RawByte, out byte[] luaCode))
            {
                // 缓存反编译结果
                luaByteInfo.SetDecompiledContent(luaCode);
            }

            return luaByteInfo.ProcessedByte;
        }

        private bool TryDecompile(byte[] luaBytes, out byte[] luaCode)
        {
            var dependencyPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DEPENDENCY_PATH));
            var decompilerPath = Path.GetFullPath(Path.Combine(dependencyPath, LUAJIT_DECOMPILER_PATH));
            
            // 检查反编译器是否存在
            if (!File.Exists(decompilerPath))
            {
                Console.WriteLine($"LuaJIT反编译器未找到: {decompilerPath}");
                luaCode = null;
                return false;
            }

            // 写入临时输入文件
            File.WriteAllBytes(TEMP_FILE, luaBytes);

            // 构建命令参数：使用静默模式和强制覆盖
            var args = $"\"{TEMP_FILE}\" -s -f -o \".\"";
            var decompileProcess = BuildProcess(decompilerPath, args);

            bool success = false;
            luaCode = null;
            try
            {
                decompileProcess.Start();
                decompileProcess.WaitForExit();
                
                if (decompileProcess.ExitCode == 0)
                {
                    // 检查输出文件是否存在
                    if (File.Exists(TEMP_OUTPUT_FILE))
                    {
                        luaCode = File.ReadAllBytes(TEMP_OUTPUT_FILE);
                        success = true;
                    }
                    else
                    {
                        Console.WriteLine("反编译完成但未找到输出文件");
                    }
                }
                else
                {
                    Console.WriteLine($"反编译失败，退出码: {decompileProcess.ExitCode}");
                    if (!string.IsNullOrEmpty(decompileProcess.Error))
                    {
                        Console.WriteLine($"错误信息: {decompileProcess.Error}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"反编译过程出现异常: {e.Message}");
                success = false;
            }
            finally
            {
                decompileProcess.Close();
                
                // 清理临时文件
                try
                {
                    if (File.Exists(TEMP_FILE))
                        File.Delete(TEMP_FILE);
                    if (File.Exists(TEMP_OUTPUT_FILE))
                        File.Delete(TEMP_OUTPUT_FILE);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清理临时文件时出现错误: {ex.Message}");
                }
            }

            return success;
        }

        private OutputProcess BuildProcess(string exePath, string args)
        {
            var decompileProcess = new OutputProcess();
            decompileProcess.StartInfo.FileName = exePath;
            decompileProcess.StartInfo.Arguments = args;
            decompileProcess.StartInfo.UseShellExecute = false;
            decompileProcess.StartInfo.CreateNoWindow = true;
            decompileProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            decompileProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            return decompileProcess;
        }
    }
}