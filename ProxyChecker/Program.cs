using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxyChecker
{
    internal class Program
    {
        private static string PathFile { get; set; }
        private static string PathFileValidate { get; set; }
        static async Task Main(string[] args)
        {
            await BrutProxy();
            Log("Файл с проксями создан", ConsoleColor.White);
            Log("Введите название файла для сохранения валидных прокси (без .txt)");
            PathFileValidate = Console.ReadLine();

            using (var sr = new StreamReader(PathFile + ".txt"))
            {
                var list = sr.ReadToEnd().Split(':', '\n', '\r').ToList();
                for (int i = 0; i < list.Count; i += 5)
                {
                    await ProccesCheckProxy(list[i], list[i + 1], list[i + 2], list[i + 3]);
                }
            }
        }

        static void Log(string message, ConsoleColor consoleColor = ConsoleColor.Green)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {message}");
        }

        static async Task BrutProxy()
        {
            Log("Введите название для сохранение файла (без .txt)");
            PathFile = Console.ReadLine();
            Log("Введите IP адресс формата IP:PORT:USER:PASS");
            string[] str1 = Console.ReadLine().Split('.', ':');
            for (int i = 1; i < 256; i++)
            {
                using (var sw = new StreamWriter(PathFile + ".txt", true))
                {
                    await sw.WriteLineAsync($"{str1[0]}.{str1[1]}.{str1[2]}.{i}:{str1[4]}:{str1[5]}:{str1[6]}");
                }

            }
        }

        static async Task ProccesCheckProxy(string url, string port, string username, string password)
        {
            string proxyURL = $"http://{url}:{port}";
            string proxyUsername = username;
            string proxyPassword = password;

            WebProxy proxy = new WebProxy()
            {
                Address = new Uri(proxyURL),
                Credentials = new NetworkCredential(proxyUsername, proxyPassword)
            };

            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = proxy
            };
            HttpClient client = new HttpClient(handler);
            try
            {
                using (HttpResponseMessage response = await client.GetAsync("https://httpbin.org/ip"))
                    if (response.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Log($"{proxyURL} status code (200 OK)");
                        using (var sw = new StreamWriter(PathFileValidate + ".txt", true))
                        {
                            sw.WriteLine($"{url}:{port}:{username}:{password}");
                        }
                    }
                    else
                    {
                        Log($"{proxyURL} status code (400 Bad request)", ConsoleColor.Red);
                    }
            }
            catch (HttpRequestException ex)
            {
                Log($"{proxyURL} status code (400 Bad request)", ConsoleColor.Red);
            }

            catch (Exception ex)
            {

            }
        }
    }
}
