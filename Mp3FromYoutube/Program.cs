using FluentFTP;
using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using VideoLibrary;

namespace Mp3FromYoutube
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var client = ConnectServer();

            Console.WriteLine("Selecione a pasta para onde sera baixado");
            Console.WriteLine($"---------------------------");
            Console.ReadKey();
            var folder = BrowserFolder();

            Console.WriteLine("Selecione p arquivo text com os links");
            Console.WriteLine($"---------------------------");
            Console.ReadKey();
            var txt = BrowserTxtArchive();

            Console.WriteLine("1 - Audiolivro \n2 - Musica");
            var opcao = Console.ReadLine();
            opcao = (opcao == "1") ? "audiolivros" : "Music";

            DownloadAudiolivro(txt, folder);

            Console.WriteLine($"Passando Arquivos para o celular");
            Console.WriteLine($"---------------------------");
            var folderInfo = new DirectoryInfo(folder);
            folderInfo.GetFiles().ToList().ForEach(x => client.UploadFile(x.FullName, $"/sdcard/{opcao}/{x.Name}"));
            folderInfo.GetFiles().ToList().ForEach(x => x.Delete());
            Console.WriteLine($"Terminamos por aqui");
            Console.WriteLine($"---------------------------");

            // get a list of files and directories in the "/htdocs" folder
            foreach (FtpListItem item in client.GetListing($"/sdcard/{opcao}/"))
            {
                Console.WriteLine($"{item.FullName}");
            }

            Console.ReadKey();
        }

        public static string BrowserFolder()
        {
            var folder = @"C:\";

            var FolderBrowserDialog = new FolderBrowserDialog();

            if (FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folder = FolderBrowserDialog.SelectedPath;
            }

            return folder;
        }

        public static string BrowserTxtArchive()
        {
            var txt = "";
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txt = dialog.FileName;
            }

            return txt;
        }

        public static FtpClient ConnectServer()
        {
            Console.WriteLine("Conectando no servidor");
            Console.WriteLine("----------------------");

            try
            {
                // create an FTP client
                var client = new FtpClient("192.168.1.113")
                {
                    Port = 2221,

                    Credentials = new NetworkCredential("android", "123456")
                };

                // begin connecting to the server
                client.Connect();

                return client;
            }
            catch (Exception)
            {
                Console.WriteLine("Impossivel se conectar coom o servidor");
                return null;
            }
        }

        public static void DownloadAudiolivro(string txt, string folder)
        {
            var lines = File.ReadAllLines(txt);

            foreach (var item in lines)
            {
                var youtube = YouTube.Default;
                var vid = youtube.GetVideo(item);

                Console.WriteLine($"Baixando Video: {vid.Title}");
                Console.WriteLine($"---------------------------");
                File.WriteAllBytes(folder + vid.FullName, vid.GetBytes());

                var inputFile = new MediaFile { Filename = folder + vid.FullName };
                var outputFile = new MediaFile { Filename = folder + vid.FullName.Replace(".mp4", ".mp3") };

                Console.WriteLine($"Convertendo Video: {vid.Title}");
                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    engine.Convert(inputFile, outputFile);
                }

                Console.WriteLine($"{vid.Title} - Convertido");
                Console.WriteLine($"---------------------------");

                Console.WriteLine("Deletando arquivo mp4");
                Console.WriteLine($"---------------------------");

                DirectoryInfo folderInfo = new DirectoryInfo(folder);
                folderInfo.GetFiles().Where(x => x.FullName.Contains(".mp4")).ToList().ForEach(x => x.Delete());
            }
        }
    }
}