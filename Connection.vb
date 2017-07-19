Imports System.Data.Common
Imports System.Data.SQLite

Public Class Connection
	Inherits Databasic.Connection

	Public Overrides ReadOnly Property Provider As DbConnection
		Get
			Return Me._provider
		End Get
	End Property
	Private _provider As SQLiteConnection

	Public Overrides ReadOnly Property ProviderResource As System.Type = GetType(ProviderResource)

	Public Overrides ReadOnly Property ClientName As String = "System.Data.SQLite"

	Public Overrides ReadOnly Property Statement As System.Type = GetType(Statement)

	Public Overrides Sub Open(dsn As String)
		Me._provider = New SQLiteConnection(dsn)
		Me._provider.Open()
		'AddHandler Me._provider.InfoMessage, AddressOf Connection.errorHandler
	End Sub

	Public Overrides Function CreateAndBeginTransaction(Optional transactionName As String = "", Optional isolationLevel As IsolationLevel = IsolationLevel.Unspecified) As Databasic.Transaction
		Return New Transaction() With {
			.ConnectionWrapper = Me,
			.Instance = Me._provider.BeginTransaction(DirectCast(isolationLevel, System.Data.IsolationLevel))
		}
	End Function

End Class
