Imports System.Text.RegularExpressions
Imports Databasic.ActiveRecord

Public Class ProviderResource
	Inherits Databasic.ProviderResource

	Public Overrides Function GetTableColumns(table As String, connection As Databasic.Connection) As Dictionary(Of String, Boolean)
        Dim createSql = Me._getCreateTableStatement(table, connection)
        createSql = Me._prepareCreateTableStatement(createSql)
        Return Me._parseColumnsFromCreateTable(createSql)
    End Function

    Private Function _getCreateTableStatement(table As String, connection As Databasic.Connection) As String
        Return Databasic.Statement.Prepare("
			SELECT sql 
			FROM sqlite_master t
			WHERE 
				t.type = 'table' AND 
				t.name = @table
			", connection
        ).FetchAll(New With {
            .table = table
        }).ToInstance(Of String)()
    End Function

    Private Function _prepareCreateTableStatement(createSql As String) As String
        '' createSql example:
        'CREATE TABLE persons (
        '	id_person INT NOT NULL,
        '	id_parent INT NULL,
        '	id_department INT NOT NULL,
        '	name VARCHAR(100) NOT NULL,
        '	surname VARCHAR(100) NULL,
        '	salary DECIMAL(9, 2) NOT NULL DEFAULT 0,
        '	gender CHAR(1) NOT NULL DEFAULT 'O'
        ')
        Dim result As String = "",
            indexPos = 0,
            m As Match,
            pos = createSql.IndexOf("(")
        createSql = createSql.Substring(pos + 1)
        pos = createSql.LastIndexOf(")")
        createSql = createSql.Substring(0, pos)
        ' replace temporarily all comma chars in value type definition places
        ' to be able to split all columns by comma char later
        For Each m In Regex.Matches(createSql, "\(\d+(,)\s*\d+\)")
            result &= createSql.Substring(indexPos, m.Index - indexPos) _
                & m.Value.Replace(",", "__DATABASIC_COMMA_CHAR__")
            indexPos = m.Index + m.Length
        Next
        result &= createSql.Substring(m.Index + m.Length)
        ' comma replacing end
        result = result.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ")
        Return result
    End Function

    Private Function _parseColumnsFromCreateTable(createSql As String) As Object
        Dim result As New Dictionary(Of String, Boolean),
            columnsSql As String() = createSql.Split(","),
            columnSql As String,
            columnName As String,
            columnCouldBeNull As Boolean,
            pos As Int32
        For index = 0 To columnsSql.Length - 1
            columnSql = columnsSql(index).Trim(" "c, "\t", "\r", "\n")
            pos = columnSql.IndexOf(" ")
            If (pos = -1) Then Continue For
            ' columnSql example:
            ' salary DECIMAL(9__DATABASIC_COMMA_CHAR__ 2) NOT NULL DEFAULT 0
            columnSql = columnSql.Replace("__DATABASIC_COMMA_CHAR__", ",")
            ' columnSql example:
            ' salary DECIMAL(9, 2) NOT NULL DEFAULT 0
            columnName = columnSql.Substring(0, pos)
            columnCouldBeNull = columnSql.ToLower().IndexOf("not null") = -1
            result.Add(columnName, columnCouldBeNull)
        Next
        Return result
    End Function

    Public Overrides Function GetLastInsertedId(
        ByRef transaction As Databasic.Transaction,
        Optional ByRef classMetaDescription As MetaDescription = Nothing
    ) As Object
        Return Databasic.Statement.Prepare(
            "SELECT LAST_INSERT_ROWID()", transaction
        ).FetchOne().ToInstance(Of Object)()
    End Function

    'Public Overrides Function GetAll(
    '		connection As Databasic.Connection,
    '		columns As String,
    '		table As String,
    '		Optional offset As Int64? = Nothing,
    '		Optional limit As Int64? = Nothing,
    '		Optional orderByStatement As String = ""
    '	) As Databasic.Statement
    '	Dim sql = $"SELECT {columns} FROM {table}"
    '	offset = If(offset, 0)
    '	limit = If(limit, 0)
    '	If limit > 0 Then
    '		sql += If(orderByStatement.Length > 0, " ORDER BY " + orderByStatement, "") +
    '				$" LIMIT {If(limit = 0, "18446744073709551615", limit.ToString())} OFFSET {offset}"
    '	End If
    '	Return Databasic.Statement.Prepare(sql, connection).FetchAll()
    'End Function

End Class