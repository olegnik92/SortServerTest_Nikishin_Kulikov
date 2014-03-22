using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SortServer
{
    public class Program
    {
        private static HttpListener listener;

        private static string port;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                port = "31337";
            else
                port = args[0];

            listener = new HttpListener();
            listener.Prefixes.Add(string.Format("http://+:{0}/sort/", port));
            listener.Start();

            while (true)
            {
                try
                {
                    var context = listener.GetContext();
                    //Console.WriteLine("-------------------");
                    //Console.WriteLine("client connect");

                    RunSortAsync(context);                  
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }


        public static ulong[] ToULongArray(byte[] inputBytes)
        {
            ulong[] result = new ulong[inputBytes.Length/8];
            for (int i = 0, j = 0; i < inputBytes.Length; i += 8, j++)
            {
                result[j] = BitConverter.ToUInt64(inputBytes, i);
            }
            return result;
        }

        public async static Task RunSortAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var stream = new MemoryStream();
              //  Console.WriteLine("start copy");
                await request.InputStream.CopyToAsync(stream);

               // Console.WriteLine("start sort");
                var inputBytes = stream.ToArray();
                ulong[] inputLongs = ToULongArray(inputBytes);
                //var sortedLongs = inputLongs.AsParallel().OrderBy(l => l).ToList();
                Array.Sort(inputLongs);
                byte[] result = inputLongs.SelectMany(BitConverter.GetBytes).ToArray();
               // Console.WriteLine("end sort");

                context.Response.OutputStream.Write(result, 0, result.Length);
                context.Response.Close();
            }
            catch (Exception ex)
            {           
                Console.WriteLine(ex);
            }
        }
    }
}
