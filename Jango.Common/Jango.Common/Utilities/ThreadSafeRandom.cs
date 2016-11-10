using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jango.Common.Utilities
{
    /// <summary>
    ///  用线程安全随机数解决Random在多线程中随机性重复的问题
    /// </summary>
    public class ThreadSafeRandom : Random
    {
        private static readonly RNGCryptoServiceProvider _global = new RNGCryptoServiceProvider();
        private ThreadLocal<Random> _local = new ThreadLocal<Random>(() =>
        {
            //This is the valueFactory function  
            //This code will run for each thread to initialize each independent instance of Random.  
            var buffer = new byte[4];
            //Calls the GetBytes method for RNGCryptoServiceProvider because this class is thread-safe  
            //for this usage.  
            _global.GetBytes(buffer);
            //Return the new thread-local Random instance initialized with the generated seed.  
            return new Random(BitConverter.ToInt32(buffer, 0));
        });
        public override int Next()
        {
            return _local.Value.Next();
        }

        public override int Next(int maxValue)
        {
            return _local.Value.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return _local.Value.Next(minValue, maxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            _local.Value.NextBytes(buffer);
        }

        public override double NextDouble()
        {
            return _local.Value.NextDouble();
        }
    }
}
