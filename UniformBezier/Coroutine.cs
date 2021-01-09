using System;


namespace UniformBezier
{
    public class Coroutine
    {
        /// <summary>
        /// Call back delegate will be called every time tick
        /// </summary>
        /// <param name="fullFime"></param>
        // delegate void CoroutineTimeTick(double fullFime);
        // CoroutineTimeTick timeTickCallBack = null;

        /// <summary>
        /// Record total time before every pause
        /// </summary>
        double totalTime = 0f;
        TimeSpan startTime = TimeSpan.Zero;

        /// <summary>
        /// Get the full milliseconds from the start time to now
        /// </summary>
        public double FullTime
        {
            get
            {
                if (startTime.Equals(TimeSpan.Zero))
                {
                    // Have not start yet, or is paused now
                    // If have not start yet, totalTime = 0
                    // If is paused now, totalTime records the valid time span frorm startTime to now
                    return totalTime;
                }
                // Return the time span from startTime to now
                return totalTime + (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0) - startTime).TotalMilliseconds;
            }
        }

        public void Start()
        {
            // Record the time as the time span from 1970.1.1 to now
            startTime = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        }

        public void Stop()
        {
            // Reset the start time and totalTime
            startTime = TimeSpan.Zero;
            totalTime = 0f;
        }

        public void Pause()
        {
            // Set totalTime as FullTime, the += is already included in FullTime-get definition
            totalTime = FullTime;
            // Then frozen the start time
            startTime = TimeSpan.Zero;
        }

        public void Continue()
        {
            Start();
        }

    }
}
