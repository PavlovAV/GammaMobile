using System;
using System.Windows.Forms;
using System.Threading;
using OpenNETCF.Threading;
using OpenNETCF.Windows.Forms;
using gamma_mob.Common;
using System.IO;
using System.Reflection;

namespace gamma_mob
{
    internal static class Program
    {
        private static NamedMutex _mutex;
        private static readonly string _executablePath = Application2.StartupPath + @"\";
        public static readonly string deviceName = System.Net.Dns.GetHostName();
        

        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [MTAThread]
        private static void Main()
        
        {
            /*
            Assembly assembly = Assembly.LoadFrom("SystemMobile_Net.dll");
            Type type = assembly.GetType("Cipherlab.SystemAPI.Member");
            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("SetWiFiPower");
                if (methodInfo != null)
                {
                    object result = null;
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    object classInstance = Activator.CreateInstance(type);
                    if (parameters.Length == 0)
                    {
                        //This works fine
                        result = methodInfo.Invoke(classInstance, null);
                    }
                    else
                    {
                        object[] parametersArray = new object[] { 1 };

                        //The invoke does NOT work it throws "Object does not match target type"             
                        result = methodInfo.Invoke(classInstance, parametersArray);
                    }
                }
            }
            */
            //var t = Environment.OSVersion;
            bool isNew;
            _mutex = new NamedMutex(false, "gammamob", out isNew);
            if (!isNew) return;
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application2.Run(new MainForm());
                try
                {
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Doc*.xml");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }
            else
            {
                //if (ConnectionState.TimerForCheckConnectionAvialabled != null)
                //    ConnectionState.TimerForCheckConnectionAvialabled.Dispose();
                ConnectionState.StopChecker();
                if (BarcodeScanner.IsInstanceInitialized)
                    BarcodeScanner.Scanner.Dispose();
                //if (base.Scanner != null)
                //    base.Scanner.Dispose();
            }
        }       
    }
}