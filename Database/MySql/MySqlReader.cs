using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System;
using System.Data;

namespace CF_Server
{
    public class MySqlReader : IDisposable
    {
        private DataSet _dataset;
        private DataRow _datarow;
        private int _row;
        private const string Table = "table";

        public ushort method_4(string columnName)
        {
            ushort num = 0;
            ushort.TryParse(this._datarow[columnName].ToString(), out num);
            ushort num1 = num;
            return num1;
        }

        public uint method_6(string columnName)
        {
            uint num = 0;
            uint.TryParse(this._datarow[columnName].ToString(), out num);
            uint num1 = num;
            return num1;
        }

        public long method_7(string columnName)
        {
            long num = (long)0;
            long.TryParse(this._datarow[columnName].ToString(), out num);
            long num1 = num;
            return num1;
        }

        public MySqlReader(MySqlCommand command)
        {
            if (command.Type == MySqlCommandType.SELECT)
            {
                _dataset = new DataSet();
                _row = 0;
                using (MySql.Data.MySqlClient.MySqlConnection conn = Program.MySqlConnection)
                {
                    conn.Open();
                    using (var DataAdapter = new MySqlDataAdapter(command.Command, conn))
                        DataAdapter.Fill(_dataset, Table);
                    ((IDisposable)command).Dispose();
                }

            }
        }

        void IDisposable.Dispose()
        {
            if (_dataset != null)
                _dataset.Dispose();
        }

        public void Dispose()
        {
        }

        public int NumberOfRows
        {
            get
            {
                if (_dataset == null) return 0;
                if (_dataset.Tables.Count == 0) return 0;
                return _dataset.Tables[Table].Rows.Count;
            }
        }

        #region [Boolean-Reading]
        public bool Read()
        {
            if (_dataset == null) return false;
            if (_dataset.Tables.Count == 0) return false;
            if (_dataset.Tables[Table].Rows.Count > _row)
            {
                _datarow = _dataset.Tables[Table].Rows[_row];
                _row++;
                return true;
            }
            _row++;
            return false;
        }
        #endregion
        #region [8-Bits]
        public sbyte ReadSByte(string columnName)
        {
            sbyte result = 0;
            try { sbyte.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        public byte ReadByte(string columnName)
        {
            byte result = 0;
            try { byte.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        #endregion
        #region [16-Bits]
        public short ReadInt16(string columnName)
        {
            short result = 0;
            try { short.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        public ushort ReadUInt16(string columnName)
        {
            ushort result = 0;
            try { ushort.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        #endregion
        #region [32-Bits]
        public int ReadInt32(string columnName)
        {
            int result = 0;
            try { int.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        public uint ReadUInt32(string columnName)
        {
            uint result = 0;
            try { uint.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        #endregion
        #region [64-Bits]
        public long ReadInt64(string columnName)
        {
            long result = 0;
            try { long.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        public ulong ReadUInt64(string columnName)
        {
            ulong result = 0;
            try { ulong.TryParse(_datarow[columnName].ToString(), out result); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        #endregion
        #region [Unicode-Text]
        public string ReadString(string columnName)
        {
            string result = "";
            try { result = _datarow[columnName].ToString(); }
            catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
            return result;
        }
        #endregion
        #region [Read-Row]
        public object Read(string Section)
        {
            object Val = _datarow[Section];
            return Val;
        }
        #endregion
        #region [Boolean]
        public bool ReadBoolean(string columnName)
        {
            bool result = false;
            try { bool.TryParse(_datarow[columnName].ToString(), out result); }
            catch
            {
                byte value = 0;
                try { byte.TryParse(_datarow[columnName].ToString(), out value); }
                catch { Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed); }
                result = value == 0 ? false : true;
            }
            return result;
        }
        #endregion
        #region [Byte Array]
        public byte[] ReadBlob(string columnName)
        {
            try
            {
                if (_datarow.IsNull(columnName)) return new byte[0];
                return (byte[])_datarow[columnName];
            }
            catch
            {
                Console.WriteLine("[MySql-Reader] No such field named " + columnName + " in the row.", ConsoleColor.DarkRed);
                return null;
            }
        }
        #endregion
    }
}