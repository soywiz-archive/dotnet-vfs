using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetVfs.Sqlite
{
	unsafe class Sqlite3
	{
		private const string LIB = "sqlite3";
		private const CallingConvention LibCallingConvention = CallingConvention.Cdecl;

		public delegate void sqlite3_destructor_type(IntPtr Data);

		public const int SQLITE_OK = 0;
		public const int SQLITE_ERROR = 1;
		public const int SQLITE_ROW = 100;
		public const int SQLITE_DONE = 101;

		public const int SQLITE_INTEGER = 1;
		public const int SQLITE_FLOAT    = 2;
		public const int SQLITE_BLOB     = 4;
		public const int SQLITE_NULL     = 5;
		public const int SQLITE_TEXT     = 3;

		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_open(string filename, out IntPtr ppDb);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_close(IntPtr pDb);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_finalize(IntPtr pStmt);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_reset(IntPtr pStmt);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern long sqlite3_last_insert_rowid(IntPtr db);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern string sqlite3_errmsg(IntPtr db);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_column_count(IntPtr pStmt);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_step(IntPtr pStmt);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_prepare(IntPtr db, string zSql, int nByte, out IntPtr ppStmt, char** pzTail);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_blob(IntPtr pStmt, int Column, byte[] Pointer, int n, IntPtr destructor);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_double(IntPtr pStmt, int Column, double Value);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_int(IntPtr pStmt, int Column, int Value);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_int64(IntPtr pStmt, int Column, long Value);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_null(IntPtr pStmt, int Column);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_text(IntPtr pStmt, int Column, string Text, int n, IntPtr destructor);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_text16(IntPtr pStmt, int Column, string Text, int n, IntPtr destructor);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_bind_zeroblob(IntPtr pStmt, int Column, int n);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_column_type(IntPtr pStmt, int iCol);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern IntPtr sqlite3_column_name(IntPtr pStmt, int N);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern IntPtr sqlite3_column_text(IntPtr pStmt, int iCol);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern double sqlite3_column_double(IntPtr pStmt, int iCol);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern long sqlite3_column_int64(IntPtr pStmt, int iCol);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern void *sqlite3_column_blob(IntPtr pStmt, int iCol);
		[DllImport(LIB, CallingConvention = LibCallingConvention)] static public extern int sqlite3_column_bytes(IntPtr pStmt, int iCol);

		public static readonly IntPtr SQLITE_STATIC = ((IntPtr)(0));
		public static readonly IntPtr SQLITE_TRANSIENT = ((IntPtr)(-1));
	}

	unsafe public class SqliteTable<TType> where TType : new()
	{
		public readonly SqliteClient Client;
		public readonly string Name;
		private ClassMap<TType> ClassMap;

		internal SqliteTable(SqliteClient SqliteClient, string TableName)
		{
			this.ClassMap = new ClassMap<TType>();
			this.Client = SqliteClient;
			this.Name = TableName;

			Client.Exec("CREATE TABLE IF NOT EXISTS " + this.Name + " (" + String.Join(", ", ClassMap.GetFieldList()) + ");");
		}

		//[DebuggerHidden]
		public long Insert(TType Value)
		{
			return Client.Insert(Name, ClassMap.ConvertObjectToDictionary(Value));
		}

		//[DebuggerHidden]
		public IEnumerable<TType> Select(string Where = "1=1", params object[] Params)
		{
			foreach (var Row in Client.Query("SELECT * FROM " + Name + " WHERE " + Where + ";", Params))
			{
				yield return ClassMap.ConvertDictionaryToObject(Row);
			}
		}
	}

	unsafe public class SqliteClient : IDisposable
	{
		internal IntPtr db;

		public SqliteClient(string File)
		{
			if (Sqlite3.sqlite3_open(File, out db) == Sqlite3.SQLITE_ERROR)
			{
				throw (new SqlException(Sqlite3.sqlite3_errmsg(db) + " when connecting"));
			}
		}

		//[DebuggerHidden]
		public void Exec(string SQL, params object[] Params)
		{
			foreach (var Row in Query(SQL, Params))
			{
				Console.WriteLine(Row);
			}
		}

		public string Escape(string Text)
		{
			return "\"" + Text + "\"";
		}

		public SqliteTable<TType> Table<TType>(string Name) where TType : new()
		{
			return new SqliteTable<TType>(this, Name);
		}

		//[DebuggerHidden]
		public long Insert(string Table, Dictionary<string, object> Pairs)
		{
			var Columns = Pairs.Keys.ToArray();
			var Values = Columns.Select(Column => Pairs[Column]).ToArray();
			var SQL = "INSERT INTO " + Escape(Table) + " (" + String.Join(",", Columns.Select(Item => Escape(Item))) + ") VALUES (" + String.Join(",", Columns.Select(Item => "?")) + ");";
			//Console.WriteLine("{0}", SQL);
			Exec(SQL, Values);
			return Sqlite3.sqlite3_last_insert_rowid(db);
		}

		//[DebuggerHidden]
		public long Insert<TType>(string Table, TType Value) where TType : new()
		{
			var ClassMap = new ClassMap<TType>();
			Insert(Table, ClassMap.ConvertObjectToDictionary(Value));
			return Sqlite3.sqlite3_last_insert_rowid(db);
		}

		private IntPtr _prepare(string SQL)
		{
			IntPtr stmt = default(IntPtr);
			if (Sqlite3.sqlite3_prepare(db, SQL, SQL.Length, out stmt, null) != Sqlite3.SQLITE_OK)
			{
				throw (new SqlException(Sqlite3.sqlite3_errmsg(db) + " in query " + SQL));
			}
			return stmt;
		}

		//[DebuggerHidden]
		public IEnumerable<Dictionary<string, Object>> Query(string SQL, params object[] Params)
		{
			IntPtr stmt = _prepare(SQL);
			for (int n = 0; n < Params.Length; n++)
			{
				var Param = Params[n];
				var Type = (Param != null) ? Param.GetType() : null;
				var Column = n + 1;
				//Console.WriteLine("Bind[{0}]: {1}", Column, Params[n]);
				if (Type == null || Param == null)
				{
					Sqlite3.sqlite3_bind_null(stmt, Column);
				}
				else if (Type == typeof(double) || Type == typeof(float))
				{
					Sqlite3.sqlite3_bind_double(stmt, Column, Convert.ToDouble(Param));
				}
				else if (Type == typeof(string))
				{
					var ParamString = (string)Param;
					Sqlite3.sqlite3_bind_text(stmt, Column, ParamString, ParamString.Length, Sqlite3.SQLITE_TRANSIENT);
				}
				else if (Type == typeof(byte[]))
				{
					var ByteArray = (byte[])Params[n];
					Sqlite3.sqlite3_bind_blob(stmt, Column, ByteArray, ByteArray.Length, Sqlite3.SQLITE_TRANSIENT);
				}
				else if (Type == typeof(int) || Type == typeof(long) || Type.IsEnum)
				{
					Sqlite3.sqlite3_bind_int64(stmt, Column, Convert.ToInt64(Param));
				}
				else
				{
					throw(new Exception("Can't handle type '" + Type + "'"));
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

					yield return _readRow(stmt);
				}
			}
			finally
			{
				Sqlite3.sqlite3_finalize(stmt);
			}
		}

		private Dictionary<string, Object> _readRow(IntPtr stmt)
		{
			var ColumnCount = Sqlite3.sqlite3_column_count(stmt);
			var Row = new Dictionary<string, Object>();
			for (int n = 0; n < ColumnCount; n++)
			{
				var ColumnType = (ColumnType)Sqlite3.sqlite3_column_type(stmt, n);
				//Console.WriteLine("Column: {0}: {1}", n, ColumnType);
				var ColumnName = Marshal.PtrToStringAuto(Sqlite3.sqlite3_column_name(stmt, n));
				//Console.WriteLine(" --> {0}", ColumnName);
				switch (ColumnType)
				{
					case ColumnType.Integer: Row[ColumnName] = Sqlite3.sqlite3_column_int64(stmt, n); break;
					case ColumnType.Float: Row[ColumnName] = Sqlite3.sqlite3_column_double(stmt, n); break;
					case ColumnType.Blob:
						var Bytes = new byte[Sqlite3.sqlite3_column_bytes(stmt, n)];
						Marshal.Copy(new IntPtr(Sqlite3.sqlite3_column_blob(stmt, n)), Bytes, 0, Bytes.Length);
						Row[ColumnName] = Bytes;
						break;
					case ColumnType.Null: Row[ColumnName] = null; break;
					case ColumnType.Text: Row[ColumnName] = Marshal.PtrToStringAuto(Sqlite3.sqlite3_column_text(stmt, n)); break;
					default: throw (new Exception("Unhandled column type " + ColumnType));
				}
			}

			return Row;
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

		public IEnumerable<TType> Query<TType>(string SQL, params object[] Params) where TType : new()
		{
			var ClassMap = new ClassMap<TType>();
			foreach (var Row in Query(SQL, Params))
			{
				yield return ClassMap.ConvertDictionaryToObject(Row);
			}
		}
	}

	class ClassMap<TType> where TType : new()
	{
		Type Type = typeof(TType);
		Dictionary<string, FieldInfo> FieldMap = new Dictionary<string, FieldInfo>();
		
		public ClassMap()
		{
			foreach (var Field in Type.GetFields())
			{
				FieldMap[Field.Name] = Field;
			}
		}

		public IEnumerable<string> GetFieldList()
		{
			foreach (var Field in FieldMap) yield return Field.Key;
		}

		public TType ConvertDictionaryToObject(Dictionary<string, object> Row)
		{
			var Item = new TType();
			foreach (var Pair in Row)
			{
				var FieldName = Pair.Key;
				if (FieldMap.ContainsKey(FieldName))
				{
					var Field = FieldMap[FieldName];
					var FieldType = Field.FieldType;
					var Value = Pair.Value;
					if (FieldType.IsEnum) Value = Enum.ToObject(FieldType, Value);
					//Console.WriteLine("{0}: {1}", FieldName, Value);
					Field.SetValue(Item, Value);
				}
			}
			return Item;
		}

		public Dictionary<string, object> ConvertObjectToDictionary(TType Item)
		{
			var Row = new Dictionary<string, object>();
			foreach (var Field in FieldMap)
			{
				Row[Field.Key] = Field.Value.GetValue(Item);
			}
			return Row;
		}
	}

	public class SqlException : Exception
	{
		public SqlException(string Message)
			: base(Message)
		{
		}
	}

	public class SqliteUniqueAttribute : Attribute
	{
		public string[] Keys;

		public SqliteUniqueAttribute(params string[] Keys)
		{
			this.Keys = Keys;
		}
	}
}
