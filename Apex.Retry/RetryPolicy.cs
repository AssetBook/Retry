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
        private TimeSpan _waitTimeBetweenRetries = new TimeSpan(0, 0, 0, 1);
        private readonly Action _methodToExecute;
        private List<Type> _retryOnExceptions;
	    private Func<bool> _expressionToEvaluate;
 
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

		/// <summary>
		/// <param>expressionToEvaluate</param> should return <value>false</value>
		/// in order to not continue evaluating. A return of <value>true</value> will trigger
		/// a retry.
		/// </summary>
		/// <param name="expressionToEvaluate">An expression that evaluates to <value>true</value> or <value>false</value>.</param>
	    public RetryPolicy WithRetryOn(Func<bool> expressionToEvaluate)
	    {
		    _expressionToEvaluate = expressionToEvaluate;
		    return this;
	    }

		/// <summary>
		/// Determines how long to wait between retries.
		/// </summary>
		/// <param name="waitTime">How long to wait between attempts.</param>
        public RetryPolicy WithWaitStrategy(TimeSpan waitTime)
        {
            _waitTimeBetweenRetries = waitTime;
            return this;
        }

		/// <summary>
		/// Defines how many times to retry before giving up.
		/// </summary>
		/// <param name="maxRetries">Number of times to try.</param>
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
					if(_expressionToEvaluate == null || _expressionToEvaluate() == false) break;					
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