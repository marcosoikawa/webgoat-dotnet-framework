using System;
using System.Security.Cryptography;

namespace OWASP.WebGoat.NET.App_Code
{
    // SECURE VERSION: Cryptographically secure random number generator
    public class SecureRandom : IDisposable
    {
        private readonly RNGCryptoServiceProvider _rng;
        private bool _disposed = false;

        public SecureRandom()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        public uint Next(uint min, uint max)
        {
            if (_disposed)
                throw new ObjectDisposedException("SecureRandom");
                
            if (min >= max)
                throw new ArgumentException("Min must be smaller than max");

            byte[] bytes = new byte[4];
            _rng.GetBytes(bytes);
            
            // Convert to uint and normalize to range
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (value % (max - min)) + min;
        }

        public int Next(int min, int max)
        {
            if (_disposed)
                throw new ObjectDisposedException("SecureRandom");
                
            if (min >= max)
                throw new ArgumentException("Min must be smaller than max");

            byte[] bytes = new byte[4];
            _rng.GetBytes(bytes);
            
            // Convert to positive int and normalize to range
            int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return (value % (max - min)) + min;
        }

        public void GetBytes(byte[] data)
        {
            if (_disposed)
                throw new ObjectDisposedException("SecureRandom");
                
            if (data == null)
                throw new ArgumentNullException(nameof(data));
                
            _rng.GetBytes(data);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _rng?.Dispose();
                _disposed = true;
            }
        }
    }
}