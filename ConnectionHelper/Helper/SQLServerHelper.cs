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

        public bool DeleteHotNumber(string targetId)
        {
            try
            {
                var target = db.HotNumbers.Where(m => m.Id.ToString() == targetId).FirstOrDefault();
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
        public void InsertLogToDB(string message, string status, string exportTime, string casename, DateTime eventTime, string interceptId, string interceptName)
        {
            var log = new ExportLog
            {
                Messages = message,
                Status = status,
                CaseName = casename,
                EventTime = eventTime,
                ExportTime = exportTime,
                InterceptId = interceptId,
                InterceptName = interceptName
            };
            db.ExportLogs.Add(log);
            db.SaveChanges();
        }
    }
}
