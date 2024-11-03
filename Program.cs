

//Первое задание
//Создайте приложение, позволяющее пользователям скачивать файлы из
//интернета.
//Пользователь указывает путь для скачивания и путь для сохранения, количество потоков для скачивания, теги для скачиваемого файла (необязательно). Пользователь может приостановить закачку, остановить закачку, удалить
//закачку.
//Пользователь может искать скачанные файлы по тегам. Интерфейс
//приложения отображает удачные, неудачные и текущие закачки. Пользователь
//может через интерфейс приложения удалять, переименовывать, перемещать
//скачанные файлы. Приложение должно обладать удобным пользовательским
//интерфейсом



using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace _31._10_exam
{
    internal class Program
    {
        
        private static string username = "anonymous";
        private static string password = "test@test.gmail.com";

        private static bool flagPause;
        private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);
        private static CancellationTokenSource cts = new CancellationTokenSource();

        private static string FilePathDownload;
        private static string FilePathSave;


        private static int countThread;
        private static long totalSize = 0;
        private static int filesDownloaded = 0;


        private static List<string> DataFile = new List<string>();
        private static List<FileInfo> filesInfo = new List<FileInfo>();


        private static string pathToSaveFileInfoDirectory = "E:\\STEP\\Сетевое программирование\\1111";
        private static string pathToSaveFileInfo = "FailTagsList.txt";

        private static string fullPath = Path.Combine(pathToSaveFileInfoDirectory, pathToSaveFileInfo);

        static async Task Main(string[] args)
        {
            filesInfo = ReadFileTags();
            await Menu();
            WriteToFile();

        }

        static async Task Menu()
        {
            Console.WriteLine("Выберите:");
            Console.WriteLine("1 - скачать файлы");

            Console.WriteLine("Операции над скаченными файлами:");
            Console.WriteLine("2 - поиск скачанных файлов по тегам");
            Console.WriteLine("3 - удалить скачанные файлы по тегам");
            Console.WriteLine("4 - переименовать скачанные файлы");
            Console.WriteLine("5 - переместить скачанные файлы в другую папку");

            Console.WriteLine("0 - выход");

            int inputMenu;
            while (!Int32.TryParse(Console.ReadLine(), out inputMenu) || inputMenu < 0 || inputMenu > 5)
            {
                Console.WriteLine("Не верный ввод.Введите число от 0 до 5:");
                Console.Write("Ваш выбор - ");
            }

            switch (inputMenu)
            {
                case 0: return; 
                case 1:
                    await SubMenu();
                    Console.Clear();
                    await Menu();
                    break;
                case 2:
                    await FindFile();
                    Console.WriteLine("Для выхода в меню нажмите любую клавишу...");
                    Console.ReadKey();
                    Console.Clear();
                    await Menu();
                    break;
                case 3:
                    await DeleteFile();
                    Console.Clear();
                    await Menu();
                    break;
                case 4:
                    await RenameFile();
                    Console.Clear();
                    await Menu();
                    break;
                case 5:
                    await MoveFile();
                    Console.Clear();
                    await Menu();
                    break;
            }
           
        }

        static async Task SubMenu()
        {

            Console.WriteLine("Укажите путь для скачивания файлов");

            FilePathDownload = "ftp://ftp.intel.com/images";
            // FilePathDownload = Console.ReadLine();

            Console.WriteLine("Укажите путь для сохранения файлов");
            //FilePathSave = Console.ReadLine();
            FilePathSave = "E:\\STEP\\Сетевое программирование\\1111";

            if (!Directory.Exists(FilePathSave))
            {
                Directory.CreateDirectory(FilePathSave);
            }


            Console.WriteLine("Укажите количество потоков для скачивания файлов");

            while (!Int32.TryParse(Console.ReadLine(), out countThread) || countThread < 0)
            {
                Console.WriteLine("Не верный ввод.Введите число:");
                Console.Write("количество потоков для скачивания файлов - ");
            }

            Console.WriteLine("Начать скачивание?(Y / N)");

            string flagStart = Console.ReadLine().ToUpper();
            while (flagStart != "Y" && flagStart != "N")
            {
                Console.WriteLine("Не верный ввод.Вывести данные таблицы? (Y/N)");
                flagStart = Console.ReadLine().ToUpper();
            }

            if (flagStart == "Y")
            {
                Console.Clear();

                Console.WriteLine("При необходимости, можете выполнить действия: пауза(P), продолжить выполнение работы(R),отменить загрузку(С)");
                await StartDownloadMenu();



                Console.Clear();
                Console.CursorVisible = true;
                await Task.Delay(1000);
                Console.WriteLine("Присвоить скаченным файлам тег?(Y / N)");
             
                string AssignTag = Console.ReadLine().ToUpper();
                while (AssignTag != "Y" && AssignTag != "N")
                {
                   
                    Console.WriteLine("Не верный ввод.Присвоить скаченным файлам тег ? (Y/N)");
                    AssignTag = Console.ReadLine().ToUpper();
                }

                if (AssignTag == "Y")
                {
                        AddDataBaseTeg();
                        WriteToFile();
                }
                    Console.Clear();
               // Menu();
            }
        }

        static void AddDataBaseTeg()
        {
            Console.WriteLine("Введите тег:");
            var teg = Console.ReadLine();
            foreach (var f in DataFile)
            {
                var FileTegInfo = new FileInfo(FilePathSave, f, teg);
                filesInfo.Add(FileTegInfo);
            }
        }


        static List<FileInfo> ReadFileTags()
        {
            var result = new List<FileInfo>();
            if (File.Exists(fullPath))
                using (StreamReader reader = new StreamReader(fullPath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(';');
                        result.Add(new FileInfo(parts[0], parts[1], parts[2]));
                    }
                }
            return result;
        }

        static void WriteToFile()
        {
            try
            {
                if (!Directory.Exists(pathToSaveFileInfoDirectory  ))
            {
                Directory.CreateDirectory(pathToSaveFileInfoDirectory);
            }
            if (!File.Exists(fullPath))
            {

                var fs = File.Create(fullPath);
                fs.Close();
            }

            using (StreamWriter writer = new StreamWriter(fullPath, true))
            {
                foreach (var f in filesInfo)
                {
                    writer.WriteLine(String.Join(";", f.Path,f.FileName, f.Tag));
                }
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
            }
        }

        static async Task StartDownloadMenu()
        {

            try
            {
                await GetFilesName();
                var fileIndex = 0;
                Console.WriteLine($"Найдено файлов {DataFile.Count}");



                var commandTask = Task.Run(() => GetCommand());
                Task.Run(() => UpdateProgressBar(DataFile.Count, cts.Token));


                while (fileIndex < DataFile.Count)
                {
                    Task[] tasks = new Task[countThread];
                    for (int i = 0; i < countThread; i++)
                    {
                        if (fileIndex >= DataFile.Count)
                        {
                            break;
                        }
                        var file = DataFile[fileIndex];
                        tasks[i] = StartDownload(cts.Token, file);
                        fileIndex++;
                    }

                    Task.WaitAll(tasks.Where(t => t != null).ToArray());

                }


                Console.SetCursorPosition(5, 10);
                Console.WriteLine($"\nЗагрузка завершена. Всего файлов: {filesDownloaded}, общий размер: {totalSize} байт.");
            }
            catch (OperationCanceledException)
            {
                Console.SetCursorPosition(5, 9);
                Console.WriteLine("Загрузка была отменена пользователем.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static async Task GetCommand()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.P:
                            OnPauseClick();
                            flagPause = true;
                            break;
                        case ConsoleKey.R:
                            OnResumeClick();
                            flagPause = false;
                            break;
                        case ConsoleKey.C:
                            OnCancelClick();
                            break;
                        default:
                            break;
                    }
                }
            }
            await Task.Delay(100);
        }
        public static void OnPauseClick()
        {
            waitHandle.Reset();
        }

        public static void OnResumeClick()
        {
            waitHandle.Set();
        }

        public static void OnCancelClick()
        {
            cts.Cancel();
            Console.SetCursorPosition(5, 9);
            Console.WriteLine("Загрузка отменена.");
        }
        static async Task GetFilesName()
        {
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(FilePathDownload);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectory;


            listRequest.Credentials = new NetworkCredential(username, password);


            using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (StreamReader reader = new StreamReader(listResponse.GetResponseStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    DataFile.Add(line);
                }
            }
        }

        static async Task StartDownload(CancellationToken cancellationToken, string file)
        {
            string remoteFileUrl = $"{FilePathDownload}/{file}";
            string localFilePath = Path.Combine(FilePathSave, file);


            FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(remoteFileUrl);
            downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;


            downloadRequest.Credentials = new NetworkCredential(username, password);


            using (FtpWebResponse response = (FtpWebResponse)downloadRequest.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fs = new FileStream(localFilePath, FileMode.Create))

            {
                byte[] buffer = new byte[4096];
                long bytesRead = 0;
                int size;

                while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {



                    cancellationToken.ThrowIfCancellationRequested();


                    fs.Write(buffer, 0, size);
                    bytesRead += size;


                    waitHandle.WaitOne();
                }

                totalSize += bytesRead;
                filesDownloaded++;

                Console.WriteLine("");

            }
        }

        static async Task UpdateProgressBar(int allFileCount, CancellationToken cancellationToken)
        {
            Console.CursorVisible = false;
            try
            {
                while (filesDownloaded <= allFileCount)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int maxProgress = 20;
                    int percent = (maxProgress / allFileCount) * filesDownloaded + 1;
                    string separator = new string('*', percent);


                    Console.SetCursorPosition(5, 4);
                    Console.WriteLine(flagPause ? "     Пауза..." : "     Загрузка...");

                    if (!flagPause)
                        percent = (percent + 1) % (maxProgress + 1);
                    await Task.Delay(500);

                    Console.SetCursorPosition(5, 5);

                    Console.SetCursorPosition(5, 6);
                    Console.WriteLine(new string('-', maxProgress + 2));

                    Console.SetCursorPosition(5, 7);
                    Console.Write("|");
                    Console.Write(separator);
                    Console.Write(new string(' ', maxProgress - percent+1));
                    Console.WriteLine("|");

                    Console.SetCursorPosition(5, 8);
                    Console.WriteLine(new string('-', maxProgress + 2));

                    await Task.Delay(100);
                    if (filesDownloaded == allFileCount)
                    {
                       
                        //Console.SetCursorPosition(5, 4);
                        //Console.WriteLine(flagPause ? "     Пауза..." : "     Загрузка...");

                        //if (!flagPause)
                        //    percent = (percent + 1) % (maxProgress + 1);
                        //await Task.Delay(500);

                        //Console.SetCursorPosition(5, 5);
                        ////Console.Write($"Загрузка... ");

                        //Console.SetCursorPosition(5, 6);
                        //Console.WriteLine(new string('-', maxProgress + 2));

                        //Console.SetCursorPosition(5, 7);
                        //Console.Write("|");
                        //Console.Write(separator);
                        //Console.Write(new string(' ', maxProgress - percent + 1));
                        //Console.WriteLine("|");

                        //Console.SetCursorPosition(5, 8);
                        //Console.WriteLine(new string('-', maxProgress + 2));
                        //await Task.Delay(500);
                        Console.Clear();
                        return;
                    }

                }
          

            }
            finally
            {
                Console.CursorVisible = true;
            }
        }

        static async Task FindFile()
        {
            Console.WriteLine("Введите тег, который следует найти файлы:");
            var teg = Console.ReadLine();
            foreach (var f in filesInfo)
            {
                if (f.Tag == teg)
                    Console.WriteLine(f.FileName);
            }
            await Task.Delay(2000);
        }

        static async Task DeleteFile()
        {
            try
            {
                Console.WriteLine("Введите тег, по которому следует удалить файлы:");
            var teg = Console.ReadLine();

            for(var i = filesInfo.Count-1; i>=0; i--)
            {
                var f = filesInfo[i];
                if (f.Tag == teg)
                {
                    string newPath = Path.Combine(f.Path, (f.FileName));
                    if (File.Exists(newPath))
                    {
                        WriteToFileDelete($"{f.Path};{f.FileName};{f.Tag}");
                        File.Delete(newPath);
                        Console.WriteLine("Файл успешно удалён.");
                    }
                    else
                    {
                        Console.WriteLine("Файл не найден.");
                    }


                    filesInfo.Remove(f);
                }
            }
            Console.WriteLine("Все файлы с данным тегом удалены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении файлов: {ex.Message}");
            }
        }

        static void WriteToFileDelete(string lineToDelete)
        {
            try
            {
                var lines = new List<string>(File.ReadAllLines(fullPath));

                lines.RemoveAll(line => line.Equals(lineToDelete));

                File.WriteAllLines(fullPath, lines);

                Console.WriteLine("Строка удалена успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static async Task RenameFile()
        {
            try
            {
                Console.WriteLine("Введите тег, по которому следует переименовать файлы:");
                var teg = Console.ReadLine();

                Console.WriteLine("Введите новое название файла");
                var newName = Console.ReadLine();

                for (var i = 0; i < filesInfo.Count; i++)
                {
                    string newNamePath = Path.Combine(filesInfo[i].Path, $"{newName}-{i}");
                    if (filesInfo[i].Tag == teg)
                    {
                        string pathFile = Path.Combine(pathToSaveFileInfoDirectory, filesInfo[i].FileName);
                        if (File.Exists(pathFile))
                        {
                            File.Move(pathFile, newNamePath);
                            Console.WriteLine("Файл успешно переименован.");
                        }
                        else
                        {
                            Console.WriteLine("Старый файл не найден.");
                        }

                        WriteToFileNewNameFile(filesInfo[i].FileName, $"{filesInfo[i].Path};{newName}-{i};{filesInfo[i].Tag}");
                        filesInfo[i].FileName = $"{newName}-{i}";
                    }
                    else
                    {
                        Console.WriteLine("Старый файл не найден.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переименовании файлов: {ex.Message}");
            }
        }


        static void WriteToFileNewNameFile(string oldValue, string newValue)
        {
            try
            {
                string[] lines = File.ReadAllLines(fullPath);

              
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(oldValue))
                    {
                        lines[i] = newValue; 
                    }
                }

                File.WriteAllLines(fullPath, lines);

                Console.WriteLine("Замена завершена успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static async Task MoveFile()
        {
            try
            {
            Console.WriteLine("Введите тег, по которому следует переименовать файлы:");
            var teg = Console.ReadLine();

            Console.WriteLine("Введите новый путь для сохранения файла");
            var newPath = Console.ReadLine();

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            for (var i = 0; i < filesInfo.Count; i++)
            {
                if (filesInfo[i].Tag == teg)
                {
                    string newFullPath = Path.Combine(newPath, filesInfo[i].FileName);
                    string pathFile = Path.Combine(pathToSaveFileInfoDirectory, filesInfo[i].FileName);
                    if (File.Exists(pathFile))
                    {
                        File.Move(pathFile, newFullPath);
                        WriteToFileNewNameFile(filesInfo[i].FileName, $"{newPath};{filesInfo[i].FileName};{filesInfo[i].Tag}");
                        filesInfo[i].Path = $"{newPath}";
                        Console.WriteLine("Файл успешно переименован.");
                    }
                    else
                    {
                        Console.WriteLine("Старый файл не найден.");
                    }
                }
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при перемещении файлов: {ex.Message}");
            }
        }

    }
    public class FileInfo
    { 
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Tag { get; set; }

        public FileInfo(string path, string fileName, string tag)
        {
            Path = path;
            FileName = fileName;
            Tag = tag;
        }
    };
}
