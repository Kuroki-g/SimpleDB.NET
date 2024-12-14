namespace SimpleDB.Sql;

/// <summary>
/// JDBCの型をそのままコピペしている。
/// 現状は学習上入れているだけで、元々のJavaのライセンスが怪しいので差し替えるべきである。
/// </summary>
/// <see href="https://docs.oracle.com/javase/jp/17/docs/api/constant-values.html#java.sql.Types.BIT"/>
public static class Types
{
    public static readonly int ARRAY = 2003;

    public static readonly int BIGINT = -5;
    public static readonly int BINARY = -2;
    public static readonly int BIT = -7;
    public static readonly int BLOB = 2004;
    public static readonly int BOOLEAN = 16;
    public static readonly int CHAR = 1;
    public static readonly int CLOB = 2005;
    public static readonly int DATALINK = 70;
    public static readonly int DATE = 91;
    public static readonly int DECIMAL = 3;
    public static readonly int DISTINCT = 2001;
    public static readonly int DOUBLE = 8;
    public static readonly int FLOAT = 6;
    public static readonly int INTEGER = 4;
    public static readonly int JAVA_OBJECT = 2000;
    public static readonly int LONGNVARCHAR = -16;
    public static readonly int LONGVARBINARY = -4;
    public static readonly int LONGVARCHAR = -1;
    public static readonly int NCHAR = -15;
    public static readonly int NCLOB = 2011;
    public static readonly int NULL = 0;
    public static readonly int NUMERIC = 2;
    public static readonly int NVARCHAR = -9;
    public static readonly int OTHER = 1111;
    public static readonly int REAL = 7;
    public static readonly int REF = 2006;
    public static readonly int REF_CURSOR = 2012;
    public static readonly int ROWID = -8;
    public static readonly int SMALLINT = 5;
    public static readonly int SQLXML = 2009;
    public static readonly int STRUCT = 2002;
    public static readonly int TIME = 92;
    public static readonly int TIME_WITH_TIMEZONE = 2013;
    public static readonly int TIMESTAMP = 93;
    public static readonly int TIMESTAMP_WITH_TIMEZONE = 2014;
    public static readonly int TINYINT = -6;
    public static readonly int VARBINARY = -3;
    public static readonly int VARCHAR = 12;
}
