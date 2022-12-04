using ConnectionHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionHelper.Helper
{
    public class SQLServerHelper
    {
        public ExportHistoryEntities db = new ExportHistoryEntities();

        public List<HotNumber> GetAllHotNumber()
        {
            try
            {
                var listAllNumbers = db.HotNumbers.ToList();
                return listAllNumbers;
            }
            catch { return new List<HotNumber>(); }
        }
        public void AddHotNumber(HotNumber phone)
        {
            try
            {
                db.HotNumbers.Add(phone);
                db.SaveChanges();
            }
            catch { }
        }

        public bool DeleteHotNumber(string casename, string interceptname)
        {
            try
            {
                var target = db.HotNumbers.Where(m => m.CaseName.ToString() == casename && m.PhoneNumber == interceptname).FirstOrDefault();
                db.HotNumbers.Remove(target);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }


        public bool DeleteTarget(string targetId)
        {
            try
            {
                var target = db.ExportTargets.Where(m => m.TargetId.ToString() == targetId).FirstOrDefault();
                db.ExportTargets.Remove(target);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }
        public void AddTarget(ExportTarget target)
        {
            try
            {
                db.ExportTargets.Add(target);
                db.SaveChanges();
            }
            catch { }
        }
        public bool IsTargetAdded(string id, string name)
        {
            try
            {
                var target = db.ExportTargets.Where(m => m.TargetId == id && m.TargetName == name).FirstOrDefault();
                if (target != null)
                    return true;
                return false;
            }
            catch { return false; }
        }

        public Login GetUserByUsernameAndPass(string username, string password)
        {
            try
            {
                //var alluser = db.Logins.ToList();
                var user = db.Logins.Where(m => m.Username == username && m.Password.ToLower() == password).FirstOrDefault();
                return user;
            }
            catch { return null; }
        }

        public List<ExportTarget> GetListExportTarget()
        {
            try
            {
                var listAllTarget = db.ExportTargets.ToList();
                return listAllTarget;
            }
            catch { return new List<ExportTarget>(); }

        }

        public List<ExportLog> GetLogByTime(string fromDate, string toDate)
        {
            var listExport = new List<ExportLog>();
            var tempFromDate = DateTime.ParseExact(fromDate, "dd-MM-yyyy H:mm",
                                    System.Globalization.CultureInfo.InvariantCulture);

            var tempToDate = DateTime.ParseExact(toDate, "dd-MM-yyyy H:mm",
                                    System.Globalization.CultureInfo.InvariantCulture);
            listExport = db.ExportLogs.Where(m=>m.ExportTime >= tempFromDate && m.ExportTime <=tempToDate).ToList();
            return listExport;
        }

        public void InsertLogToDB(string message, DateTime exportTime, string casename,string logType, string interceptId, string interceptName)
        {
            var log = new ExportLog
            {
                Messages = message,
                CaseName = casename,
                LogType = logType,
                ExportTime = exportTime,
                InterceptId = interceptId,
                InterceptName = interceptName
            };
            db.ExportLogs.Add(log);
            db.SaveChanges();
        }
    }
}
