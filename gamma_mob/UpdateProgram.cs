using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using OpenNETCF.Windows.Forms;
using System.Threading;

namespace gamma_mob
{
    public class UpdateProgram
    {
        private static readonly string program = @"gamma_mob";
        private static readonly string executablePath = Application2.StartupPath + @"\";
        private static readonly string updatePath = executablePath + @"update\";

        private static string ComputeMD5Checksum(string path)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var stream = File.OpenRead(path);
            byte[] checkSum = md5.ComputeHash(stream);
            stream.Close();
            string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
            return result;
        }

        private class FileOfRepositary
        {
            public FileOfRepositary(int fileId, string dirName, string fileName, string title, byte[] image, string md5, bool action)
            {
                Id = fileId;
                DirName = dirName;
                FileName = fileName;
                Title = title;
                Image = image;
                MD5 = md5;
                Action = action;
            }
            public int Id { get; private set; }
            public string DirName { get; private set; }
            public string FileName { get; private set; }
            public string Title { get; private set; }
            public byte[] Image { get; private set; }
            public string MD5 { get; private set; }
            public bool Action { get; private set; }

        }

        public static bool CheckAndCreateFlagUpdateLoading()
        {
            try
            {
                if (!File.Exists(executablePath + @"UpdateLoading.inf"))
                {
                    File.CreateText(executablePath + @"UpdateLoading.inf");
                    return false;
                }
                else
                {
                    return true;                
                }
            }
            catch
            {
                //Console.WriteLine("Ошибка при проверке файла {0}!", executablePath + @"UpdateLoading.inf");
                return false;
            }

        }

        public static void DropFlagUpdateLoading()
        {
            try
            {
                File.Delete(executablePath + @"UpdateLoading.inf");
            }
            catch
            {
                //Console.WriteLine("Ошибка при удалении файла {0}!", executablePath + @"UpdateLoading.inf");
            }
            
        }

        public static void LoadUpdate(object obj)
        {
            if (!CheckAndCreateFlagUpdateLoading())
            {
                if (ConnectionState.CheckConnection() && Db.CheckSqlConnection() == 0)
                {
                    string connectionString = Db.GetConnectionString();
                    List<FileOfRepositary> files = new List<FileOfRepositary>();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();
                            string sql = "SELECT FileID, DirName, FileName, Title, Image, MD5, Action FROM RepositoryOfProgramFiles WHERE IsActivity = 1 AND ProgramName = @Program AND action IS NOT NULL";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@Program", SqlDbType.NVarChar, 50);
                            command.Parameters["@Program"].Value = program;
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string dirname = reader.GetString(1);
                                string filename = reader.GetString(2);
                                string title = reader.GetString(3);
                                byte[] image = (byte[])reader.GetValue(4);
                                string md5 = reader.GetString(5);
                                bool action = (bool)reader.GetValue(6);

                                FileOfRepositary file = new FileOfRepositary(id, dirname, filename, title, image, md5, action);
                                files.Add(file);
                            }
                        }
                        catch
                        {
                            //Console.WriteLine("Ошибка при получении данных с БД!");
                        }
                    }
                    DirectoryInfo dirInfo = new DirectoryInfo(updatePath);
                    if (!dirInfo.Exists)
                    {
                        try
                        {
                            dirInfo.Create();
                        }
                        catch
                        {
                            //Console.WriteLine("Ошибка при содании папки {0}", updatePath);
                            return;
                        }
                    }
                    // сохраним файлы из списка
                    for (int i = 0; i < files.Count; i++)
                    {
                        try
                        {
                            FileInfo fileInf = new FileInfo(executablePath + files[i].FileName);
                            if (files[i].Action)
                                if (fileInf.Exists && files[i].MD5 == ComputeMD5Checksum(executablePath + files[i].FileName))
                                {
                                    //Console.WriteLine("Файл '{0}' не сохранен, так как изменений нет.", files[i].Title);
                                }
                                else
                                    if (new FileInfo(updatePath + files[i].FileName).Exists && files[i].MD5 == ComputeMD5Checksum(updatePath + files[i].FileName))
                                    {
                                        //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + files[i].Title);
                                    }
                                    else
                                    {
                                        if (new FileInfo(updatePath + files[i].FileName + @".bak").Exists && files[i].MD5 == ComputeMD5Checksum(updatePath + files[i].FileName + @".bak"))
                                        {
                                            //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + files[i].FileName + @".bak");
                                        }
                                        else
                                        {
                                            DirectoryInfo di = new DirectoryInfo(updatePath + files[i].DirName);
                                            if (!(di.Exists))
                                                di.Create();
                                            using (System.IO.FileStream fs = new System.IO.FileStream(updatePath + files[i].FileName + @".bak", FileMode.Create))
                                            {
                                                fs.Write(files[i].Image, 0, files[i].Image.Length);
                                                //Console.WriteLine("Файл '{0}' сохранен.", files[i].Title);
                                            }
                                        }
                                        FileInfo fi = new FileInfo(updatePath + files[i].FileName + @".bak");
                                        fi.MoveTo(updatePath + files[i].FileName);
                                    }
                            else
                            {
                                if (fileInf.Exists)
                                {
                                    DeleteFile(fileInf.FullName);
                                    //Console.WriteLine("Файл '{0}' удален.", files[i].Title);
                                }
                            }
                        }
                        catch
                        {
                            //Console.WriteLine("Ошибка! Файл '{0}' не обработан!", files[i].Title);
                        }

                    }
                }
                DropFlagUpdateLoading();
            }
            AplyUpdate();
        }

        private static void DeleteFile(string fileName)
        {
            try
            {
                FileInfo fileInfCurrent = new FileInfo(fileName);
                FileInfo fileInfCurrentOld = new FileInfo(fileName + @".old");
                //удалить предыдущий
                if (fileInfCurrentOld.Exists)
                    fileInfCurrentOld.Delete();
                //переименовать текущий
                if (fileInfCurrent.Exists)
                    fileInfCurrent.MoveTo(fileName + @".old");
            }
            catch
            {
                //Console.WriteLine("Ошибка! Файл '{0}' не заменен на новый!", fileName);
            }
        }
        
        private static void UpdateFile(string fileName)
        {
            try
            {
                FileInfo fileInfNew = new FileInfo(fileName);
                FileInfo fileInfCurrent = new FileInfo(fileName.Replace(updatePath,executablePath));
                FileInfo fileInfCurrentOld = new FileInfo(fileName.Replace(updatePath,executablePath)+@".old");
                //удалить предыдущий
                if (fileInfCurrentOld.Exists)
                    fileInfCurrentOld.Delete();
                //переименовать текущий
                if (fileInfCurrent.Exists)
                    fileInfCurrent.MoveTo(fileName.Replace(updatePath,executablePath)+@".old");
                //скопировать bak в нормальный
                fileInfNew.MoveTo(fileName.Replace(updatePath,executablePath));
            }
            catch
            {
                //Console.WriteLine("Ошибка! Файл '{0}' не заменен на новый!", fileName);
            }
        }

        private static void ApplyUpdateFiles(string dirName)
        {
            if (Directory.Exists(dirName))
            {
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string s in dirs)
                {
                    //обработать подпапку
                    DirectoryInfo di = new DirectoryInfo(s.Replace(updatePath,executablePath));
                    if (!(di.Exists))
                        di.Create();
                    ApplyUpdateFiles(s);
                }
                string[] files = Directory.GetFiles(dirName);
                foreach (string s in files)
                {
                    UpdateFile(s);
                }
                if (Directory.GetFiles(dirName).Length == 0)
                    Directory.Delete(dirName);
            }
        }

        public static void AplyUpdate()
        {
            if (Directory.Exists(updatePath))
            {
                ApplyUpdateFiles(updatePath);
                MessageBox.Show(
                    "Есть обновление, перезапустите программу!",
                    "Внимание!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
            }
        }
    }
}
