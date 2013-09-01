using Community.CsharpSqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetVfs.Sqlite
{
	public class SqliteClient : IDisposable
	{
		internal Sqlite3.sqlite3 db;

		public SqliteClient(string File)
		{
			Sqlite3.sqlite3_open(File, out db);
		}

		[DebuggerHidden]
		public void Exec(string SQL, params object[] Params)
		{
			foreach (var Row in Query(SQL, Params))
			{
			}
		}

		public string Escape(string Text)
		{
			return "\"" + Text + "\"";
		}

		public void Insert(string Table, Dictionary<string, object> Pairs)
		{
			var Columns = Pairs.Keys.ToArray();
			var Values = Columns.Select(Column => Pairs[Column]).ToArray();
			var SQL = "INSERT INTO " + Escape(Table) + " (" + String.Join(",", Columns.Select(Item => Escape(Item))) + ") VALUES (" + String.Join(",", Columns.Select(Item => "?")) + ");";
			//Console.WriteLine("{0}", SQL);
			Exec(SQL, Values);
		}

		[DebuggerHidden]
		public IEnumerable<object[]> Query(string SQL, params object[] Params)
		{
			Sqlite3.Vdbe stmt = default(Sqlite3.Vdbe);
			if (Sqlite3.sqlite3_prepare(db, SQL, SQL.Length, ref stmt, 0) != Sqlite3.SQLITE_OK)
			{
				throw (new SqlException(Sqlite3.sqlite3_errmsg(db)));
			}

			for (int n = 0; n < Params.Length; n++)
			{
				var Param = Params[n];
				var Type = (Param != null) ? Param.GetType() : null;
				var Column = n + 1;
				//Console.WriteLine("Bind[{0}]: {1}", Column, Params[n]);
				if (Type == null)
				{
					Sqlite3.sqlite3_bind_null(stmt, Column);
				}
				if (Type == typeof(double) || Type == typeof(float))
				{
					Sqlite3.sqlite3_bind_double(stmt, Column, Convert.ToDouble(Param));
				}
				else if (Type == typeof(string))
				{
					Sqlite3.sqlite3_bind_text(stmt, Column, (string)Param, -1, Sqlite3.SQLITE_STATIC);
				}
				else if (Type == typeof(byte[]))
				{
					Sqlite3.sqlite3_bind_blob(stmt, Column, (byte[])Params[n], -1, Sqlite3.SQLITE_STATIC);
				}
				else
				{
					Sqlite3.sqlite3_bind_int64(stmt, Column, Convert.ToInt64(Param));
				}
			}

			try
			{
				if (Sqlite3.sqlite3_reset(stmt) != Sqlite3.SQLITE_OK)
				{
					throw (new SqlException(Sqlite3.sqlite3_errmsg(db)));
				}
				int Result;
				while ((Result = Sqlite3.sqlite3_step(stmt)) != Sqlite3.SQLITE_DONE)
				{
					if (Result != Sqlite3.SQLITE_ROW)
					{
						throw (new SqlException(Sqlite3.sqlite3_errmsg(db)));
					}

					var ColumnCount = Sqlite3.sqlite3_column_count(stmt);
					var Row = new object[ColumnCount];
					for (int n = 0; n < ColumnCount; n++)
					{
						var ColumnType = (ColumnType)Sqlite3.sqlite3_column_type(stmt, n);
						//Console.WriteLine("Column: {0}: {1}", n, ColumnType);
						switch (ColumnType)
						{
							case ColumnType.Integer: Row[n] = Sqlite3.sqlite3_column_int64(stmt, n); break;
							case ColumnType.Float: Row[n] = Sqlite3.sqlite3_column_double(stmt, n); break;
							case ColumnType.Blob: Row[n] = Sqlite3.sqlite3_column_blob(stmt, n); break;
							case ColumnType.Null: Row[n] = null; break;
							case ColumnType.Text: Row[n] = Sqlite3.sqlite3_column_text(stmt, n); break;
							default: throw (new Exception("Unhandled column type " + ColumnType));
						}
					}
					yield return Row;
				}
			}
			finally
			{
				Sqlite3.sqlite3_finalize(stmt);
			}
		}

		public enum ColumnType : int
		{
			Integer = Sqlite3.SQLITE_INTEGER,
			Float = Sqlite3.SQLITE_FLOAT,
			Blob = Sqlite3.SQLITE_BLOB,
			Null = Sqlite3.SQLITE_NULL,
			Text = Sqlite3.SQLITE_TEXT,
		}

		public void Dispose()
		{
			Sqlite3.sqlite3_close(db);
		}
	}

	public class SqlException : Exception
	{
		public SqlException(string Message)
			: base(Message)
		{
		}
	}
}
