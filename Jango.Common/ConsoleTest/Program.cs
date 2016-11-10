using Jango.Common.NetWork.Sockets;
using Jango.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTestServer
{
    class Program
    {
        private static int _count = 1000;
        private static int _timeout_ms = 10;

        static void Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(SocketUtils.GetLocalIPV4(), 5000);
            var setting = new SocketSetting();
            var server = new ServerSocket(ipEndPoint, setting);
            server.Start();

            Console.ReadLine();
        }

        #region thread sleep  & SpinWait
        private static void Spin_Run()
        {
            //NoSleep();
            ThreadSleepInThread();
            SpinWaitInThread();
        }

        // SpinWait(自旋)的性能高于Thread.Sleep   SpinLock(自旋锁)
        private static void NoSleep()
        {
            Thread thread = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < _count; i++)
                {

                }
                Console.WriteLine("No Sleep Consume Time:{0}", sw.Elapsed.ToString());
            });
            thread.IsBackground = true;
            thread.Start();
        }


        private static void ThreadSleepInThread()
        {
            Thread thread = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < _count; i++)
                {
                    Thread.Sleep(_timeout_ms);
                }
                Console.WriteLine("Thread Sleep Consume Time:{0}", sw.Elapsed.ToString());
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void SpinWaitInThread()
        {
            Thread thread = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < _count; i++)
                {
                    SpinWait.SpinUntil(() => true, _timeout_ms);
                }
                Console.WriteLine("SpinWait Consume Time:{0}", sw.Elapsed.ToString());
            });
            thread.IsBackground = true;
            thread.Start();

            var th01 = new Thread(() =>
            {
                var spinWait = new SpinWait();
                for (int i = 0; i < _count; i++)
                {
                    var sw = Stopwatch.StartNew();
                    spinWait.SpinOnce();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("SpinWait Consume Time:{0}", sw.Elapsed.ToString());
                }
            });
            thread.IsBackground = true;
            thread.Start();


        }

        #endregion

        #region Run Function  
        public static void Run()
        {
            Console.WriteLine("Started!");

            var sw = Stopwatch.StartNew();
            //int normalRandomSameCount = randomSerial(generateNormalRadoms);
            //Console.WriteLine("Normal Random Same Count:{0}, Consume Time:{1}", normalRandomSameCount, sw.Elapsed.ToString());

            //sw.Restart();
            int threadSafeRandomSameCount = randomSerial(generateThreadSafeRadoms);
            Console.WriteLine("Thread Safe Random Same Count:{0}, Consume Time:{1}", threadSafeRandomSameCount, sw.Elapsed.ToString());

            Console.WriteLine("Completed!");
            Console.ReadLine();
        }

        private static int randomSerial(Func<int, List<int>> generateRadoms)
        {
            int randomCount = 10000000;

            Task<List<int>>[] tasks = new Task<List<int>>[2];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    return generateRadoms(randomCount);
                });
            }
            Task.WaitAll(tasks);
            int sameCount = 0;
            Task finalTask = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < randomCount; i++)
                {
                    if (tasks[0].Result[i] == tasks[1].Result[i])
                    {
                        sameCount++;
                    }
                }
            });
            finalTask.Wait();
            return sameCount;
        }

        private static List<int> generateNormalRadoms(int randomCount)
        {
            GC.Collect(GC.MaxGeneration);
            List<int> randoms = new List<int>();
            for (int i = 0; i < randomCount; i++)
            {
                Random random = new Random();
                randoms.Add(random.Next());
            }
            return randoms;
        }

        private static List<int> generateThreadSafeRadoms(int randomCount)
        {
            GC.Collect(GC.MaxGeneration);
            List<int> randoms = new List<int>();
            for (int i = 0; i < randomCount; i++)
            {
                ThreadSafeRandom safeRandom = new ThreadSafeRandom();
                randoms.Add(safeRandom.Next());
            }
            return randoms;
        }
        #endregion





        private static void generateRandomDictionary(string savePath)
        {

            GC.Collect(GC.MaxGeneration);
            int begin = 1;
            int end = 99999999;
            int count = begin + end;
            Encoding encoding = Encoding.UTF8;
            Random random = new Random(Guid.NewGuid().GetHashCode());

            //生成1到99999999的所有整数  
            int[] codes = new int[count + 1];

            Parallel.For(0, 10000, a =>
            {
                for (int i = begin; i <= end; i++)
                {
                    codes[i] = i;
                }
            });


            GC.Collect(GC.MaxGeneration);
            //随机交换数据  
            int index = 0;
            int tempCode = 0;
            Parallel.For(0, 10000, b =>
            {
                for (int i = begin; i <= end; i++)
                {
                    index = random.Next(count + 1);
                    tempCode = codes[index];
                    codes[index] = codes[i];
                    codes[i] = tempCode;
                }
            });

            GC.Collect(GC.MaxGeneration);
            int signal = 0;
            var sl = new SpinLock();
            Parallel.For(0, 10000, x =>
            {

                //while (Interlocked.Exchange(ref signal, 1) != 0) //加自旋锁 方法1
                //{
                bool gotLock = false;     //释放成功
                sl.Enter(ref gotLock);    //进入锁  自旋锁方法2
                                          //数据写入文件  
                var destStream = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                for (int i = begin; i <= end; i++)
                {
                    string codeStr = string.Format("{0:00000000}", codes[i]);
                    byte[] codeBytes = encoding.GetBytes(codeStr);
                    destStream.Write(codeBytes, 0, codeBytes.Length);
                }
                destStream.Close();

                if (gotLock) sl.Exit();  //释放
                //}
                //Interlocked.Exchange(ref signal, 0); //释放锁

            });
        }
    }
}
