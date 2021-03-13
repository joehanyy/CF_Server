using System;
using System.Collections.Generic;
using System.Text;
using CF_Server.Database;

namespace CF_Server
{
    using MYSQLCOMMAND = MySql.Data.MySqlClient.MySqlCommand;
    using MYSQLCONNECTION = MySql.Data.MySqlClient.MySqlConnection;

    public class MySqlCommand : IDisposable
    {
        private MySqlCommandType _type;

        public MySqlCommandType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        protected StringBuilder _command;

        public string Command
        {
            get { return _command.ToString(); }
            set { _command = new StringBuilder(value); }
        }

        private bool firstPart = true;

        private Dictionary<byte, string> insertFields;
        private Dictionary<byte, string> insertValues;
        private byte lastpair;

        public MySqlCommand(MySqlCommandType Type)
        {
            this.Type = Type;
            switch (Type)
            {
                case MySqlCommandType.SELECT:
                    {
                        _command = new StringBuilder("SELECT * FROM <R>");
                        break;
                    }
                case MySqlCommandType.C:
                    {
                        _command = new StringBuilder("DELETE FROM <R>");
                        break;
                    }
                case MySqlCommandType.UPDATE:
                    {
                        _command = new StringBuilder("UPDATE <R> SET ");
                        break;
                    }
                case MySqlCommandType.INSERT:
                    {
                        insertFields = new Dictionary<byte, string>();
                        insertValues = new Dictionary<byte, string>();
                        lastpair = 0;
                        _command = new StringBuilder("INSERT INTO <R> (<F>) VALUES (<V>)");
                        break;
                    }
                case MySqlCommandType.DELETE:
                    {
                        _command = new StringBuilder("DELETE FROM <R> WHERE <C> = <V>");
                        break;
                    }
                case MySqlCommandType.COUNT:
                    {
                        _command = new StringBuilder("SELECT count(<V>) FROM <R>");
                        break;
                    }
            }
        }

        private bool Comma()
        {
            if (firstPart)
            {
                firstPart = false;
                return false;
            }
            string command = _command.ToString();
            if (command[command.Length - 1] == ',' || command[command.Length - 2] == ',' || command[command.Length - 3] == ',')
                return false;
            return true;
        }

        #region Select

        public MySqlCommand Select(string table)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            return this;
        }

        #endregion Select

        #region Count

        public MySqlCommand Count(string table)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            return this;
        }

        #endregion Count

        #region Delete

        public MySqlCommand Delete(string table, string column, string value)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            _command = _command.Replace("<C>", "`" + column + "`");
            _command = _command.Replace("<V>", "'" + value.MySqlEscape() + "'");
            return this;
        }

        public MySqlCommand C(string table)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            return this;
        }

        public MySqlCommand Delete(string table, string column, long value)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            _command = _command.Replace("<C>", "`" + column + "`");
            _command = _command.Replace("<V>", value.ToString());
            return this;
        }

        public MySqlCommand Delete(string table, string column, ulong value)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            _command = _command.Replace("<C>", "`" + column + "`");
            _command = _command.Replace("<V>", value.ToString());
            return this;
        }

        public MySqlCommand Delete(string table, string column, bool value)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            _command = _command.Replace("<C>", "`" + column + "`");
            _command = _command.Replace("<V>", (value ? "1" : "0"));
            return this;
        }

        #endregion Delete

        #region Update

        public MySqlCommand Update(string table)
        {
            _command = _command.Replace("<R>", "`" + table + "`");

            return this;
        }

        public MySqlCommand Set(string column, long value)
        {
            if (Type == MySqlCommandType.UPDATE)
            {
                if (Comma())
                    _command = _command.Append(",`" + column + "` = " + value.ToString() + " ");
                else
                    _command = _command.Append("`" + column + "` = " + value.ToString() + " ");
            }
            return this;
        }

        public MySqlCommand Set(string column, ulong value)
        {
            if (Type == MySqlCommandType.UPDATE)
            {
                if (Comma())
                    _command = _command.Append(",`" + column + "` = " + value.ToString() + " ");
                else
                    _command = _command.Append("`" + column + "` = " + value.ToString() + " ");
            }
            return this;
        }

        public MySqlCommand Set(string column, string value)
        {
            if (Type == MySqlCommandType.UPDATE)
            {
                if (Comma())
                    _command = _command.Append(",`" + column + "` = '" + value + "' ");
                else
                    _command = _command.Append("`" + column + "` = '" + value.MySqlEscape() + "' ");
            }
            return this;
        }

        public MySqlCommand Set(string column, bool value)
        {
            if (Type == MySqlCommandType.UPDATE)
            {
                if (Comma())
                    _command = _command.Append(",`" + column + "` = " + (value ? "1" : "0") + " ");
                else
                    _command = _command.Append("`" + column + "` = " + (value ? "1" : "0") + " ");
            }
            return this;
        }

        public MySqlCommand Set(string column, object value)
        {
            if (value is bool) Set(column, (bool)value);
            else Set(column, value.ToString());
            return this;
        }

        #endregion Update

        #region Insert

        public MySqlCommand Insert(string table)
        {
            _command = _command.Replace("<R>", "`" + table + "`");
            return this;
        }

        public MySqlCommand Insert(string field, long value)
        {
            insertFields[lastpair] = field;
            insertValues[lastpair] = value.ToString();
            lastpair++;
            return this;
        }

        public MySqlCommand Insert(string field, ulong value)
        {
            insertFields[lastpair] = field;
            insertValues[lastpair] = value.ToString();
            lastpair++;
            return this;
        }

        public MySqlCommand Insert(string field, bool value)
        {
            insertFields[lastpair] = field;
            insertValues[lastpair] = (value ? 1 : 0).ToString();
            lastpair++;
            return this;
        }

        public MySqlCommand Insert(string field, string value)
        {
            var array = value.ToCharArray();
            string str = Encoding.Default.GetString(Encoding.Unicode.GetBytes(array, 0, array.Length));
            insertFields[lastpair] = field;
            insertValues[lastpair] = value.MySqlEscape();
            lastpair++;
            return this;
        }

        #endregion Insert

        #region Where

        public MySqlCommand Where(string column, long value)
        {
            _command = _command.Append("WHERE `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand Where(string column, long value, bool greater)
        {
            if (greater)
                _command = _command.Append("WHERE `" + column + "` > " + value);
            else
                _command = _command.Append("WHERE `" + column + "` < " + value);
            return this;
        }

        public MySqlCommand Where(string column, ulong value)
        {
            _command = _command.Append("WHERE `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand Where(string column, string value)
        {
            _command = _command.Append("WHERE `" + column + "` = '" + value.MySqlEscape() + "'");
            return this;
        }

        public MySqlCommand Where(string column, bool value)
        {
            _command = _command.Append("WHERE `" + column + "` = " + (value ? "1" : "0"));
            return this;
        }

        #endregion Where

        #region And

        public MySqlCommand And(string column, long value)
        {
            _command = _command.Append(" AND `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand And(string column, long value, bool greater)
        {
            if (greater)
                _command = _command.Append(" AND `" + column + "` > " + value);
            else
                _command = _command.Append(" AND `" + column + "` < " + value);
            return this;
        }

        public MySqlCommand And(string column, ulong value)
        {
            _command = _command.Append(" AND `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand And(string column, string value)
        {
            _command = _command.Append(" AND `" + column + "` = '" + value.MySqlEscape() + "'");
            return this;
        }

        public MySqlCommand And(string column, bool value)
        {
            _command = _command.Append(" AND `" + column + "` = " + (value ? "1" : "0"));
            return this;
        }

        #endregion And

        #region Or

        public MySqlCommand Or(string column, long value)
        {
            _command = _command.Append(" Or `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand Or(string column, ulong value)
        {
            _command = _command.Append(" Or `" + column + "` = " + value);
            return this;
        }

        public MySqlCommand Or(string column, string value)
        {
            _command = _command.Append(" Or `" + column + "` = '" + value.MySqlEscape() + "'");
            return this;
        }

        public MySqlCommand Or(string column, bool value)
        {
            _command = _command.Append(" Or `" + column + "` = " + (value ? "1" : "0"));
            return this;
        }

        #endregion Or

        #region Order

        public MySqlCommand Order(string column)
        {
            _command = _command.Append("ORDER BY " + column + "");
            return this;
        }

        #endregion Order

        public int Execute()
        {
            using (var conn = Program.MySqlConnection)
            {
                conn.Open();
                return Execute(conn);
            }
        }

        public int Execute(MYSQLCONNECTION conn)
        {
            if (Type == MySqlCommandType.INSERT)
            {
                string fields = "";
                string values = "";
                byte x;
                for (x = 0; x < lastpair; x++)
                {
                    bool comma = (x + 1) == lastpair ? false : true;

                    #region Fields

                    if (comma)
                        fields += "`" + insertFields[x] + "`,";
                    else
                        fields += "`" + insertFields[x] + "`";

                    #endregion Fields

                    #region Values

                    if (comma)
                        values += "'" + insertValues[x] + "'" + ",";
                    else
                        values += "'" + insertValues[x] + "'";

                    #endregion Values
                }
                _command = _command.Replace("<F>", fields);
                _command = _command.Replace("<V>", values);
            }

            MYSQLCOMMAND cmd = new MYSQLCOMMAND(Command, conn);
            return cmd.ExecuteNonQuery();
        }

        public MySqlReader CreateReader()
        {
            return new MySqlReader(this);
        }

        void IDisposable.Dispose()
        {
            if (insertValues != null)
            {
                insertValues.Clear();
                insertFields.Clear();
            }
            _command = null;
        }
    }

    public enum MySqlCommandType
    {
        DELETE, INSERT, SELECT, UPDATE, C, COUNT
    }
}