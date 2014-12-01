using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Apex.Retry
{
    public class RetryPolicy
    {
        private int _numberOfAttempts = 0;
        private int _maxRetryAttempts = 3;
        private TimeSpan _waitTimeBetweenRetries = new TimeSpan(0, 0, 0, 1000);
        private readonly Action _methodToExecute;
        private List<Type> _retryOnExceptions;

        /// <summary>
        /// 
        /// </summary>        
        /// <param name="methodToExecute"></param>
        public RetryPolicy(Action methodToExecute)
        {
            _methodToExecute = methodToExecute;
        }

        public RetryPolicy WithRetryOnExceptions(params Type[] exceptions)
        {
            _retryOnExceptions = exceptions.ToList();
            return this;
        }

        public RetryPolicy WithWaitStrategy(TimeSpan waitTime)
        {
            _waitTimeBetweenRetries = waitTime;
            return this;
        }

        public RetryPolicy WithStopStrategy(int maxRetries)
        {
            _maxRetryAttempts = maxRetries;
            return this;
        }

        public void Process()
        {
            while (_numberOfAttempts < _maxRetryAttempts)
            {
                if (_numberOfAttempts > 0) Thread.Sleep(_waitTimeBetweenRetries);

                _numberOfAttempts++;
                try
                {
                    _methodToExecute();
                    break;
                }
                catch (Exception ex)
                {
                    if (!ShouldRetry(ex)) throw;
                }
            }
        }

        private bool ShouldRetry(Exception ex)
        {
            return _numberOfAttempts < _maxRetryAttempts &&
                   _retryOnExceptions.Any(retryEx => ex.GetType() == retryEx);
        }

        public enum TimeUnit
        {
            Millisecond,
            Second,
            Minute,
            Hour,
            Day
        }
    }
}