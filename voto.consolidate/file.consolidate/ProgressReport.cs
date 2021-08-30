using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file.consolidate
{
    public class ProgressReport
    {
        public static ProgressReport Instance { get; set; }
        public delegate void ProgressDelegate();

        private event ProgressDelegate ProgressEvent;
        private double _Bps;
        private int _Failed;
        private string _FileName;
        private string _Report;
        private int _Skipped;
        private int _Successful;
        private int _Total;
        private long _TotalBytes;
        public double Bps
        {
            get { return _Bps; }
            set { _Bps = value; ProgressEvent?.Invoke(); }
        }
        public int Failed
        {
            get { return _Failed; }
            set { _Failed = value; ProgressEvent?.Invoke(); }
        }
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; ProgressEvent?.Invoke(); }
        }
        public string Report
        {
            get { return _Report; }
            set { _Report = value; ProgressEvent?.Invoke(); }
        }
        public int Skipped
        {
            get { return _Skipped; }
            set { _Skipped = value; ProgressEvent?.Invoke(); }
        }
        public int Successful
        {
            get { return _Successful; }
            set { _Successful = value; ProgressEvent?.Invoke(); }
        }
        public int Total
        {
            get { return _Total; }
            set { _Total = value; ProgressEvent?.Invoke(); }
        }
        public long TotalBytes
        {
            get { return _TotalBytes; }
            set { _TotalBytes = value; ProgressEvent?.Invoke(); }
        }

        public ProgressReport(ProgressDelegate theEvent)
        {
            if (Instance == null)
            {
                ProgressEvent += theEvent;
                Instance = this;
            }
        }

        public void ClearReport()
        {
            _Total = 0;
            _Successful = 0;
            _Failed = 0;
            _Skipped = 0;
            _Bps = 0.0;
            _TotalBytes = 0;
            ProgressEvent?.Invoke();
        }

        public int Completed => (Successful + Skipped + Failed);
        public float PercentComplete => Completed / (float)Total;
    }
}
