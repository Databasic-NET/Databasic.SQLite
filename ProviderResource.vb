Imports Databasic.ActiveRecord

Public Class ProviderResource
	Inherits Databasic.ProviderResource

	Public Overrides Function GetTableColumns(table As String, connection As Databasic.Connection) As Dictionary(Of String, Boolean)
		Dim result As New Dictionary(Of String, Boolean)
		Dim createSql As String = Databasic.Statement.Prepare("
			SELECT sql 
			FROM sqlite_master t
			WHERE 
				t.type = 'table' AND 
				t.name = @table
			",
			connection
		).FetchAll(New With {
			.table = table
		}).ToInstance(Of String)()
		Dim pos = createSql.IndexOf("(")
		If pos = -1 Then Return result
		createSql = createSql.Substring(pos + 1)
		pos = createSql.LastIndexOf(")")
		If pos = -1 Then Return result
		createSql = createSql.Substring(0, pos)
		Dim columnsSql As String() = createSql.Split(",")
		Dim columnSql As String
		Dim columnName As String
		Dim columnCouldBeNull As Boolean
		For index = 0 To columnsSql.Length - 1
			columnSql = columnsSql(index).Trim(" "c, "\t", "\r", "\n")
			pos = columnSql.IndexOf(" ")
			If (pos = -1) Then Continue For
			columnName = columnSql.Substring(0, pos)
			columnCouldBeNull = columnSql.ToLower().IndexOf("not null") = -1
			result.Add(columnName, columnCouldBeNull)
		Next
		Return result
	End Function

	Public Overrides Function GetLastInsertedId(ByRef transaction As Databasic.Transaction, Optional ByRef classMetaDescription As MetaDescription = Nothing) As Object
		Return Databasic.Statement.Prepare("SELECT LAST_INSERT_ID()", transaction).FetchOne().ToInstance(Of Object)()
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