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
            public byte[] Image { get; set; }
            public string MD5 { get; private set; }
            public bool Action { get; private set; }

        }

        public static bool CheckAndCreateFlagUpdateLoading()
        {
            try
            {
                if (!File.Exists(executablePath + @"UpdateLoading.tmp"))
                {
                    //Создаем флаг и возвращаем Ложь, что бы загрузка обновления пошла дальше.
                    FileStream fileUpdateLoadingInf = new FileStream(executablePath + @"UpdateLoading.tmp",FileMode.OpenOrCreate);
                    fileUpdateLoadingInf.Close();
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
                if (File.Exists(executablePath + @"UpdateLoading.tmp"))
                {
                    File.Delete(executablePath + @"UpdateLoading.tmp");
                }
            }
            catch
            {
                //Control.WriteLine("Ошибка при удалении файла {0}!", executablePath + @"UpdateLoading.inf");
            }
            
        }

        private static bool CheckFile(FileOfRepositary file)
        {
            try
            {
                FileInfo fileInf = new FileInfo(executablePath + file.FileName);
                if (file.Action)
                    if (fileInf.Exists && file.MD5 == ComputeMD5Checksum(executablePath + file.FileName))
                    {
                        return false;
                        //Console.WriteLine("Файл '{0}' не сохранен, так как изменений нет.", file.Title);
                    }
                    else
                        if (new FileInfo(updatePath + file.FileName).Exists && file.MD5 == ComputeMD5Checksum(updatePath + file.FileName))
                        {
                            return false;
                            //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.Title);
                        }
                        else
                        {
                            if (new FileInfo(updatePath + file.FileName + @".tmp").Exists && file.MD5 == ComputeMD5Checksum(updatePath + file.FileName + @".tmp"))
                            {
                                return false;
                                //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.FileName + @".tmp");
                            }
                            else
                            {
                                return true;
                                    //Console.WriteLine("Файл '{0}' сохранен.", file.Title);
                                
                            }
                        }
                else
                {
                    if (fileInf.Exists)
                    {
                        return true;
                        //Console.WriteLine("Файл '{0}' удален.", file.Title);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return true;
                //Console.WriteLine("Ошибка! Файл '{0}' не обработан!", file.Title);
            }
        }

        private static void SaveFile(FileOfRepositary file)
        {
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
            try
            {
                FileInfo fileInf = new FileInfo(executablePath + file.FileName);
                if (file.Action)
                {
                    {
                        //MessageBox.Show(file.Title + " - " + file.Image.Length.ToString());
                        DirectoryInfo di = new DirectoryInfo(updatePath + file.DirName);
                        if (!(di.Exists))
                            di.Create();
                        using (System.IO.FileStream fs = new System.IO.FileStream(updatePath + file.FileName + @".tmp", FileMode.Create))
                        {
                            //fs.Write(image, 0, image.Length);
                            fs.Write(file.Image, 0, file.Image.Length);
                            //Console.WriteLine("Файл '{0}' сохранен.", file.Title);
                        }
                    }
                    FileInfo fi = new FileInfo(updatePath + file.FileName + @".tmp");
                    if (file.MD5 == ComputeMD5Checksum(updatePath + file.FileName + @".tmp"))
                        fi.MoveTo(updatePath + file.FileName);
                    else
                    {
                        //MessageBox.Show("Не совпадает MD5" + updatePath + file.FileName + @".tmp");
                        fi.Delete();
                        //MessageBox.Show("Удален Не совпадает MD5" + updatePath + file.FileName + @".tmp");
                    }
                }
                else
                {
                    if (fileInf.Exists)
                    {
                        DeleteFile(fileInf.FullName);
                        //Console.WriteLine("Файл '{0}' удален.", file.Title);
                    }
                }
            }
            catch
            {
                //Console.WriteLine("Ошибка! Файл '{0}' не обработан!", file.Title);
            }
        }

        private static void LoadFile(FileOfRepositary file)
        {
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
            try
            {
                FileInfo fileInf = new FileInfo(executablePath + file.FileName);
                if (file.Action)
                    if (fileInf.Exists && file.MD5 == ComputeMD5Checksum(executablePath + file.FileName))
                    {
                        //Console.WriteLine("Файл '{0}' не сохранен, так как изменений нет.", file.Title);
                    }
                    else
                        if (new FileInfo(updatePath + file.FileName).Exists && file.MD5 == ComputeMD5Checksum(updatePath + file.FileName))
                        {
                            //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.Title);
                        }
                        else
                        {
                            if (new FileInfo(updatePath + file.FileName + @".tmp").Exists && file.MD5 == ComputeMD5Checksum(updatePath + file.FileName + @".tmp"))
                            {
                                //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.FileName + @".tmp");
                            }
                            else
                            {
                                //MessageBox.Show(file.Title + " - " + file.Image.Length.ToString());
                                DirectoryInfo di = new DirectoryInfo(updatePath + file.DirName);
                                if (!(di.Exists))
                                    di.Create();
                                using (System.IO.FileStream fs = new System.IO.FileStream(updatePath + file.FileName + @".tmp", FileMode.Create))
                                {
                                    fs.Write(file.Image, 0, file.Image.Length);
                                    //Console.WriteLine("Файл '{0}' сохранен.", file.Title);
                                }
                            }
                            FileInfo fi = new FileInfo(updatePath + file.FileName + @".tmp");
                            if (file.MD5 == ComputeMD5Checksum(updatePath + file.FileName + @".tmp"))
                                fi.MoveTo(updatePath + file.FileName);
                            else
                            {
                                //MessageBox.Show("Не совпадает MD5" + updatePath + file.FileName + @".tmp");
                                fi.Delete();
                                //MessageBox.Show("Удален Не совпадает MD5" + updatePath + file.FileName + @".tmp");
                            }
                        }
                else
                {
                    if (fileInf.Exists)
                    {
                        DeleteFile(fileInf.FullName);
                        //Console.WriteLine("Файл '{0}' удален.", file.Title);
                    }
                }
            }
            catch
            {
                //Console.WriteLine("Ошибка! Файл '{0}' не обработан!", file.Title);
            }
        }

        public static void LoadUpdate(object obj)
        {
            if (!CheckAndCreateFlagUpdateLoading())
            {
                if (ConnectionState.CheckConnection() && Db.CheckSqlConnection() == 0)
                {
                    string connectionString = Db.GetConnectionString();
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string sql = "SELECT FileID, DirName, FileName, Title, MD5, Action FROM vRepositoryOfProgramFiles WHERE ProgramName = @Program";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@Program", SqlDbType.NVarChar, 50);
                            command.Parameters["@Program"].Value = program;
                            SqlDataReader reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    string file_name = reader.GetString(2);
                                    try
                                    {
                                        int id = reader.GetInt32(0);
                                        string dirname = reader.GetString(1);
                                        string filename = reader.GetString(2);
                                        string title = reader.GetString(3);
                                        byte[] image = (byte[])null; //(byte?[])reader.GetValue(4);
                                        string md5 = reader.GetString(4);
                                        bool action = (bool)reader.GetValue(5);

                                        FileOfRepositary file = new FileOfRepositary(id, dirname, filename, title, image, md5, action);
                                        //LoadFile(file);
                                        if (CheckFile(file))
                                        {
                                            if (file.Action)
                                            using (SqlConnection connection_image = new SqlConnection(connectionString))
                                            {
                                              connection_image.Open();
                                                string sql_image = "SELECT Image FROM vRepositoryOfProgramFiles WHERE FileID = @FileID";
                                                SqlCommand command_image = new SqlCommand(sql_image, connection_image);
                                                command_image.Parameters.Add("@FileID", SqlDbType.Int);
                                                command_image.Parameters["@FileID"].Value = file.Id;
                                                SqlDataReader reader_image = command_image.ExecuteReader();
                                                if (reader_image.Read())
                                                {
                                                    file.Image = (byte[])reader_image.GetValue(0);
                                                }
                                                reader_image.Close();
                                            }
                                            SaveFile(file);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //Console.WriteLine("Ошибка при получении данных с БД!");
                                        //MessageBox.Show("Ошибка при получении данных с БД! " + file_name + ":" + ex.Message);
                                    }
                                    //files.Add(file);
                                }
                        }
                    }
                    catch
                    {
                        //Console.WriteLine("Ошибка при получении данных с БД!");
                        //MessageBox.Show("Ошибка при получении данных с БД!");
                        //return;
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
                try
                {
                    if (Directory.GetFiles(dirName).Length == 0)
                        Directory.Delete(dirName);
                }
                catch
                {
                    //Console.WriteLine("Ошибка! Папка '{0}' не удалена!", dirName);
                }
            }
        }

        public static void AplyUpdate()
        {
            if (Directory.Exists(updatePath))
            {
                ApplyUpdateFiles(updatePath);
                MessageBox.Show(
                    "Есть обновление, перезапустите программу!",
                    "Внимание!");
            }
        }
    }
}
