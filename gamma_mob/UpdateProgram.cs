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
using System.Runtime.InteropServices;
using gamma_mob.Common;

namespace gamma_mob
{
    public class UpdateProgram
    {
        private static readonly string program = @"gamma_mob";
        private static readonly string executablePath = Application2.StartupPath + @"\";
        private static readonly string updateRootPath = (Program.deviceName.Contains("Falcon")) ? @"\FlashDisk\" :
            (Program.deviceName.Contains("CPT")) ? @"\USER_DATA\" : "";
        private static readonly string updatePath = updateRootPath + @"GammaUpdate\"; //executablePath + @"update\";

        private static string errorMessage { get; set; }
        public static string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
                try
                {
                    Db.AddMessageToLog(value);
                }
                catch (Exception err)
                {
                    // MessageBox.Show(err.Message);
                }
                                                
            }
        }

        private static bool isNeededRebootingProgram { get; set; }

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
                string executeFullName = executablePath + file.DirName + @"\" + file.FileName;
                string updateFullName = (updatePath + file.DirName + @"\" + file.FileName)
                .Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\")
                .Replace(@"\.\", @"\").Replace(@"\.\", @"\").Replace(@"\.\", @"\");
                FileInfo fileInf = new FileInfo(executeFullName);
                if (file.Action)
                    if (fileInf.Exists && file.MD5 == ComputeMD5Checksum(executeFullName))
                    {
                        return false;
                        //Console.WriteLine("Файл '{0}' не сохранен, так как изменений нет.", file.Title);
                    }
                    else
                        if (new FileInfo(updateFullName).Exists && file.MD5 == ComputeMD5Checksum(updateFullName))
                        {
                            return false;
                            //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.Title);
                        }
                        else
                        {
                            if (new FileInfo(updateFullName + @".tmp").Exists && file.MD5 == ComputeMD5Checksum(updateFullName + @".tmp"))
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
            string updateDir = (updatePath + file.DirName + @"\")
                .Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\")
                .Replace(@"\.\", @"\").Replace(@"\.\", @"\").Replace(@"\.\", @"\");
            DirectoryInfo dirInfo = new DirectoryInfo(updateDir);
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
                string executeFullName = executablePath + file.DirName + @"\" + file.FileName;
                string updateFullName = updateDir + file.FileName.Replace(@".\", @"");
                FileInfo fileInf = new FileInfo(executeFullName);
                if (file.Action)
                {
                    {
                        //MessageBox.Show(file.Title + " - " + file.Image.Length.ToString());
                        DirectoryInfo di = new DirectoryInfo(updateDir);
                        if (!(di.Exists))
                            di.Create();
                        using (System.IO.FileStream fs = new System.IO.FileStream(updateFullName + @".tmp", FileMode.Create))
                        {
                            //fs.Write(image, 0, image.Length);
                            fs.Write(file.Image, 0, file.Image.Length);
                            //Console.WriteLine("Файл '{0}' сохранен.", file.Title);
                        }
                    }
                    FileInfo fi = new FileInfo(updateFullName + @".tmp");
                    if (file.MD5 == ComputeMD5Checksum(updateFullName + @".tmp"))
                        fi.MoveTo(updateFullName);
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
            catch (Exception ex)
            {
                //Console.WriteLine("Ошибка! Файл '{0}' не обработан!", file.Title);
            }
        }

        private static void LoadFile(FileOfRepositary file)
        {
            string updateDir = (updatePath + file.DirName + @"\")
                .Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\").Replace(@"\..\", @"\ParentDir\")
                .Replace(@"\.\", @"\").Replace(@"\.\", @"\").Replace(@"\.\", @"\");
            DirectoryInfo dirInfo = new DirectoryInfo(updateDir);
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
                string executeFullName = executablePath + file.DirName + @"\" + file.FileName;
                string updateFullName = updateDir + file.FileName.Replace(@".\", @"");
                FileInfo fileInf = new FileInfo(executeFullName);
                if (file.Action)
                    if (fileInf.Exists && file.MD5 == ComputeMD5Checksum(executeFullName))
                    {
                        //Console.WriteLine("Файл '{0}' не сохранен, так как изменений нет.", file.Title);
                    }
                    else
                        if (new FileInfo(updateFullName).Exists && file.MD5 == ComputeMD5Checksum(updateFullName))
                        {
                            //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.Title);
                        }
                        else
                        {
                            if (new FileInfo(updateFullName + @".tmp").Exists && file.MD5 == ComputeMD5Checksum(updateFullName + @".tmp"))
                            {
                                //Console.WriteLine("Файл '{0}' не сохранен, так как уже загружен.", updatePath + file.FileName + @".tmp");
                            }
                            else
                            {
                                //MessageBox.Show(file.Title + " - " + file.Image.Length.ToString());
                                DirectoryInfo di = new DirectoryInfo(updateDir);
                                if (!(di.Exists))
                                    di.Create();
                                using (System.IO.FileStream fs = new System.IO.FileStream(updateFullName + @".tmp", FileMode.Create))
                                {
                                    fs.Write(file.Image, 0, file.Image.Length);
                                    //Console.WriteLine("Файл '{0}' сохранен.", file.Title);
                                }
                            }
                            FileInfo fi = new FileInfo(updateFullName + @".tmp");
                            if (file.MD5 == ComputeMD5Checksum(updateFullName + @".tmp"))
                                fi.MoveTo(updateFullName);
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
        [DllImport("coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        public static void LoadUpdate(object obj)
        {
            Shared.SaveToLogInformation("Start LoadUpdate");
            if (!CheckAndCreateFlagUpdateLoading())
            {
                if (ConnectionState.CheckConnection() && Db.CheckSqlConnection() == 0)
                {
                    string connectionString = Db.GetConnectionString();
                    try
                    {
                        Shared.SaveToLogInformation("Start check files on DB in LoadUpdate");
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string sql = "SELECT FileID, DirName, FileName, Title, MD5, Action, CommandTimeOut, FileSize, TransitionOperation FROM [dbo].[mob_GetRepositoryOfProgramFiles] (@Program, @DeviceID)";
                            SqlCommand command = new SqlCommand(sql, connection);
                            command.Parameters.Add("@Program", SqlDbType.NVarChar, 50);
                            command.Parameters["@Program"].Value = program + @"#" + Program.deviceName;
                            command.Parameters.Add("@DeviceID", SqlDbType.NVarChar, 50);
                            command.Parameters["@DeviceID"].Value = Db.deviceName;

                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                string file_name = reader.GetString(2);
                                string transitionOperation = reader.GetString(8);
                                if (transitionOperation == "Upload")
                                {
                                    try
                                    {
                                        int id = reader.GetInt32(0);
                                        string dirname = reader.GetString(1);
                                        string filename = dirname + '\\' + file_name;
                                        string[] dirs = Directory.GetDirectories(dirname + '\\');
                                        if (File.Exists(filename))
                                        {
                                            string md5 = reader.GetString(4);
                                            if (md5 != ComputeMD5Checksum(filename))
                                            {
                                                using (SqlConnection connectionUpload = new SqlConnection(connectionString))
                                                {
                                                    connectionUpload.Open();
                                                    SqlCommand commandUpload = new SqlCommand();
                                                    commandUpload.Connection = connectionUpload;
                                                    //command.CommandText = @"exec AddProgramFileIntoRepository @FileName, @Title, @ImageData, @MD5, @Action, @IsActivity";
                                                    commandUpload.CommandText = @"mob_UploadFileIntoRepository";
                                                    commandUpload.CommandType = System.Data.CommandType.StoredProcedure;
                                                    commandUpload.Parameters.Add("@FileID", SqlDbType.Int);
                                                    commandUpload.Parameters.Add("@ImageData", SqlDbType.Image, 100000000);
                                                    commandUpload.Parameters.Add("@MD5", SqlDbType.NVarChar, 50);

                                                    // массив для хранения бинарных данных файла
                                                    byte[] imageData;
                                                    try
                                                    {
                                                        using (System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.Open))
                                                        {
                                                            if (fs.Length <= 100000000)
                                                            {
                                                                imageData = new byte[fs.Length];
                                                                fs.Read(imageData, 0, imageData.Length);
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("Ошибка!!! не загружен - размер>100МБ: {0}", filename);
                                                                return;
                                                            }
                                                        }

                                                        // передаем данные в команду через параметры
                                                        commandUpload.Parameters["@FileID"].Value = id;
                                                        commandUpload.Parameters["@ImageData"].Value = imageData;
                                                        commandUpload.Parameters["@MD5"].Value = ComputeMD5Checksum(filename);

                                                        //command.ExecuteNonQuery();
                                                        var result = commandUpload.ExecuteScalar();
                                                        if (Convert.ToInt32(result) > 0)
                                                            Console.WriteLine("Успешно выгружен на сервер: {0}", filename);
                                                        else
                                                            Console.WriteLine("Неудача при выгрузке на сервер: {0}", filename);

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine("Ошибка!!! не выгружен на сервер: {0} {1}", filename, ex.Message);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Shared.SaveToLogError("Error file operation in UploadUpdate");
                                        //Console.WriteLine("Ошибка при получении данных с БД! "+ file_name + ":" + ex.Message);
                                        //MessageBox.Show("Ошибка при получении данных с БД! " + file_name + ":" + ex.Message);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = reader.GetInt32(0);
                                        string dirname = reader.GetString(1);
                                        string filename = reader.GetString(2);
                                        string title = reader.GetString(3);
                                        byte[] image = (byte[])null; //(byte?[])reader.GetValue(4);
                                        string md5 = reader.GetString(4);
                                        bool action = (bool)reader.GetValue(5);
                                        string timeout = reader.GetValue(6).ToString();
                                        Int32 fileSize = (Int32)reader.GetValue(7);
                                        bool isFreeSpace = true;
                                        try
                                        {
                                            UInt64 userFreeBytes, totalDiskBytes, totalFreeBytesExecutable, totalFreeBytesUpdatable;

                                            GetDiskFreeSpaceEx(executablePath, out userFreeBytes, out totalDiskBytes, out totalFreeBytesExecutable);
                                            GetDiskFreeSpaceEx(updateRootPath, out userFreeBytes, out totalDiskBytes, out totalFreeBytesUpdatable);
                                            if (totalFreeBytesExecutable <= (ulong)fileSize || totalFreeBytesUpdatable <= (ulong)fileSize)
                                            {
                                                //MessageBox.Show("Нет места для обновления " + Environment.NewLine +
                                                //    filename + Environment.NewLine + "Требуется " + fileSize.ToString() +
                                                //    Environment.NewLine + "Доступно " + totalFreeBytesExecutable.ToString() + "/" +
                                                //    totalFreeBytesUpdatable.ToString());
                                                ErrorMessage = "Нет места для обновления " +
                                                filename + " Требуется " + fileSize.ToString() +
                                                 " Доступно " + totalFreeBytesExecutable.ToString() + "/" +
                                                totalFreeBytesUpdatable.ToString();
                                                isFreeSpace = false;
                                            }
                                        }
                                        catch
                                        {

                                        }

                                        if (isFreeSpace)
                                        {
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
                                                        command_image.CommandTimeout = Convert.ToInt32(timeout);
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
                                                Shared.SaveToLogInformation("File '" + filename + "' downloaded successfully in LoadUpdate");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Shared.SaveToLogError("Error file operation in LoadUpdate");
                                        //Console.WriteLine("Ошибка при получении данных с БД! "+ file_name + ":" + ex.Message);
                                        //MessageBox.Show("Ошибка при получении данных с БД! " + file_name + ":" + ex.Message);
                                    }
                                    //files.Add(file);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Shared.SaveToLogError("Error LoadUpdate");
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
                string currentFileName = fileName.Replace(updatePath, executablePath)
                        .Replace(@"\ParentDir", @"\..").Replace(@"\ParentDir", @"\..").Replace(@"\ParentDir", @"\..")
                        .Replace(@"\CurrentDir", @"\.").Replace(@"\CurrentDir", @"\.").Replace(@"\CurrentDir", @"\.");
                FileInfo fileInfNew = new FileInfo(fileName);
                FileInfo fileInfCurrent = new FileInfo(currentFileName);
                FileInfo fileInfCurrentOld = new FileInfo(currentFileName + @".old");
                //удалить предыдущий
                if (fileInfCurrentOld.Exists)
                    fileInfCurrentOld.Delete();
                //переименовать текущий
                if (fileInfCurrent.Exists)
                    fileInfCurrent.MoveTo(currentFileName + @".old");
                //скопировать bak в нормальный
                fileInfNew.MoveTo(currentFileName);
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
                    DirectoryInfo di = new DirectoryInfo(s.Replace(updatePath, executablePath)
                        .Replace(@"\ParentDir", @"\..").Replace(@"\ParentDir", @"\..").Replace(@"\ParentDir", @"\..")
                        .Replace(@"\CurrentDir", @"\.").Replace(@"\CurrentDir", @"\.").Replace(@"\CurrentDir", @"\."));
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
                isNeededRebootingProgram = true;
                //MessageBox.Show(
                //    "Есть обновление, перезапустите программу!",
                //    "Внимание!");
            }
            if (isNeededRebootingProgram)
                Shared.ShowMessageInformation(@"Есть обновление, перезапустите программу!");
        }
    }
}
