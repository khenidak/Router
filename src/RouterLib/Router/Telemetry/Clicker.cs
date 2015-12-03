using System;
using System.Threading;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// clicker is part of previous work for more details refer to: http://henidak.com/2015/10/on-train-rides-and-building-histograms/ 
    /// in brief, it is an memory histogram for telemetry booking keeping 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class Clicker<T, V> : IDisposable where T : class, IClick<V>, ICloneable, new()
    {
        private Task mTrimTask = null;
        private T mhead = new T();
        private CancellationTokenSource mcts = new CancellationTokenSource();
        private Action<T> mOnTrim = null;
        private bool mOnTrimChanged = false;

        public Clicker(TimeSpan KeepFor)
        {
            this.mhead.When = 0; // this ensure that head is never counted. 
            this.KeepClicksFor = KeepFor;
            this.mTrimTask = Task.Run(async () => await this.TrimLoop());
        }

        public Clicker() : this(TimeSpan.FromMinutes(1))
        {
        }

        /// <summary>
        /// Will be called whenever KeepClicksFor elabsed
        /// </summary>
        public Action<T> OnTrim
        {
            get { return this.mOnTrim; }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException("OnTrim");
                }

                this.mOnTrim = value;
                this.mOnTrimChanged = true;
            }
        }

        public TimeSpan KeepClicksFor { get; set; }

        public void Click(T newNode)
        {
            newNode.When = DateTime.UtcNow.Ticks;
            // set new head. 
            do
            {
                newNode.Next = this.mhead;
            } while (newNode.Next != Interlocked.CompareExchange<T>(ref this.mhead, newNode, (T)newNode.Next));
        }

        public void Click()
        {
            T node = new T();
            this.Click(node);
        }

        public M Do<M>(Func<T, M> func)
        {
            return this.Do(this.KeepClicksFor, func);
        }

        public M Do<M>(TimeSpan ts, Func<T, M> func)
        {
            if (ts > this.KeepClicksFor)
            {
                throw new ArgumentException("Can not do for a timespan more than what clicker is keeping track of");
            }


            // since we are not sure what will happen in
            // the func, we are handing out a copy not the original thing
            return func(this.CloneInTimeSpan(ts));
        }

        public int Count()
        {
            return this.Count(this.KeepClicksFor);
        }

        public int Count(TimeSpan ts)
        {
            if (ts > this.KeepClicksFor)
            {
                throw new ArgumentException("Can not count for a timespan more than what clicker is keeping track of");
            }

            long ticksWhen = DateTime.UtcNow.Ticks - ts.Ticks;
            int count = 0;
            IClick<V> cur = this.mhead;

            while (null != cur && cur.When >= ticksWhen)
            {
                count++;
                cur = cur.Next;
            }
            return count;
        }

        private T CloneInTimeSpan(TimeSpan ts)
        {
            long ticksWhen = DateTime.UtcNow.Ticks - ts.Ticks;

            T head = new T();
            T ret = head;
            T cur = this.mhead;

            while (cur != null && cur.When >= ticksWhen)
            {
                head.Next = (T)cur.Clone();
                head = (T)head.Next;
                cur = (T)cur.Next;
            }
            head.Next = null;
            return (T)ret.Next;
        }

        private async Task TrimLoop()
        {
            while (!this.mcts.IsCancellationRequested)
            {
                await Task.Delay((int)this.KeepClicksFor.TotalMilliseconds);
                this.Trim();
            }
        }

        private void Trim()
        {
            // trim keeps the head. 
            long ticksWhen = DateTime.UtcNow.Ticks - this.KeepClicksFor.Ticks;
            IClick<V> cur = this.mhead;
            IClick<V> next = cur.Next;
            while (null != next)
            {
                if (next.When <= ticksWhen)
                {
                    cur.Next = null;
                    break;
                }
                cur = next;
                next = next.Next;
            }

            // call on trim
            if (this.mOnTrimChanged)
            {
                this.OnTrim(this.CloneInTimeSpan(this.KeepClicksFor));
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.mcts.Cancel();
                }
                this.disposedValue = true;
            }
        }


        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }

}
