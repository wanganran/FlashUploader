using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;

namespace FlashUploader
{
    public class Entry
    {
        public int ID;
        public string filename;
        public string path;
        public int length;
        public long time;
        public string checksum;
    }

    public class SqliteDBAdapter
    {
        private string path;
        private SQLiteConnection conn;
        public SqliteDBAdapter(string path)
        {
            this.path = path;
            conn = new SQLiteConnection("Data Source=" + path);
            conn.Open();
        }
        public List<Entry> QueryAll(){
            var command = new SQLiteCommand(conn);
            command.CommandText = "select ID, filename, path, length, uploadtime, checksum from FlashUploader";
            var reader = command.ExecuteReader();
            if (reader == null) return null;
            List<Entry> result = new List<Entry>();
            while (reader.Read())
            {
                result.Add(new Entry
                {
                    ID = reader.GetInt32(0),
                    filename = reader.GetString(1),
                    path = reader.GetString(2),
                    length = reader.GetInt32(3),
                    time = reader.GetInt64(4),
                    checksum = reader.GetString(5)
                });
            }
            reader.Close();
            return result;
        }
        public Entry QueryByID(int ID)
        {
            var command = new SQLiteCommand(conn);
            command.CommandText = "select ID, filename, path, length, uploadtime, checksum from FlashUploader where ID=" + ID.ToString();
            var reader = command.ExecuteReader();
            if (reader == null) return null;
            if (!reader.Read()) return null;
            var result= new Entry
            {
                ID = reader.GetInt32(0),
                filename = reader.GetString(1),
                path=reader.GetString(2),
                length = reader.GetInt32(3),
                time = reader.GetInt64(4),
                checksum = reader.GetString(5)
            };
            reader.Close();
            return result;
        }
        public void RemoveByID(int ID)
        {
            var command = new SQLiteCommand(conn);
            command.CommandText = "delete from FlashUploader where ID=" + ID.ToString();
            command.ExecuteNonQuery();
        }
        public Entry QueryByChecksum(string checksum)
        {
            var command = new SQLiteCommand(conn);
            checksum = checksum.Replace('\"', '_');
            command.CommandText = "select ID, filename, path, length, uploadtime, checksum from FlashUploader where checksum=\"" + checksum+"\"";
            var reader = command.ExecuteReader();
            if (reader == null) return null;
            if (!reader.Read()) return null;
            var result=new Entry
            {
                ID = reader.GetInt32(0),
                filename = reader.GetString(1),
                path = reader.GetString(2),
                length = reader.GetInt32(3),
                time = reader.GetInt64(4),
                checksum = reader.GetString(5)
            };
            reader.Close();
            return result;
        }
        public List<Entry> QueryByLength(int len)
        {
            var command = new SQLiteCommand(conn);
           
            command.CommandText = "select ID, filename, path, length, uploadtime, checksum from FlashUploader where length=" + len;
            var reader = command.ExecuteReader();
            if (reader == null) return null;
            List<Entry> result = new List<Entry>();

            while (reader.Read())
            {
                result.Add(new Entry
                {
                    ID = reader.GetInt32(0),
                    filename = reader.GetString(1),
                    path = reader.GetString(2),
                    length = reader.GetInt32(3),
                    time = reader.GetInt64(4),
                    checksum = reader.GetString(5)
                });
            }
            reader.Close();
            return result;
        }
        public int Insert(Entry entry)
        {
            var command = new SQLiteCommand(conn);
            command.CommandText = "insert into FlashUploader (filename, path, length, uploadtime, checksum) values (\""+
                entry.filename+"\", \""+
                entry.path+"\", "+entry.length.ToString()+", "+entry.time.ToString()+", \""+entry.checksum+"\")";
            var reader = command.ExecuteReader();
            if (reader == null) return -1;
            if (!reader.Read()) return -1;

            command = new SQLiteCommand(conn);
            command.CommandText = "select max(ID) from FlashUploader";
            var result = command.ExecuteScalar();
            
            return (int)result;
        }
    }
}