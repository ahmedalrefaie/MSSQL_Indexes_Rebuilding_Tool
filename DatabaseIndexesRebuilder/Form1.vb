Imports System.Data
Imports System.Data.SqlClient


Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim con As New SqlConnection(txtConnectionString.Text)
        Try
            con.Open()
            MsgBox("it's Working ", MsgBoxStyle.Information)
            con.Close()
            GetAllDatabases()
        Catch ex As Exception
            MsgBox("Error , It's not working :(", MsgBoxStyle.Critical)

        End Try
    End Sub

    Function GetAllDatabases() As Boolean
        Dim con As New SqlConnection(txtConnectionString.Text)

        Dim da As New SqlDataAdapter("select name from  master.dbo.sysdatabases", con)
        Dim dt As New DataTable
        da.Fill(dt)
        ComboBox1.DisplayMember = "name"
        '    ComboBox1.ValueMember = ""
        ComboBox1.DataSource = dt
        ComboBox1.Enabled = True
        Button2.Enabled = True
        Return True
    End Function

    Function GetListOfIndexesRbuildCommands() As Boolean
        Dim con As New SqlConnection(txtConnectionString.Text)

        Dim da As New SqlDataAdapter(" Select 'USE " & ComboBox1.Text & "  ALTER INDEX   [' +  name   + '] ON ' +    OBJECT_NAME (a.object_id,database_id) + ' ReBuild'   as 'Rebuild Command' FROM " & ComboBox1.Text & ".sys.dm_db_index_physical_stats (DB_ID(N'" & ComboBox1.Text & "'),NULL, NULL, NULL, NULL) AS a, " & ComboBox1.Text & ".sys.indexes AS b       where a.object_id = b.object_id And a.index_id = b.index_id   And name Is Not null And avg_fragmentation_in_percent >= " & txtPercent.Text & "    order by avg_fragmentation_in_percent desc", con)
        'TextBox1.Text = da.SelectCommand.CommandText
        Dim dt As New DataTable
        da.Fill(dt)
        DataGridView1.DataSource = dt
        Button3.Enabled = True
        Button4.Enabled = True
        Label4.Text = "There are : " & dt.Rows.Count & "  Indexes you should rebuild"
        Return True
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        GetListOfIndexesRbuildCommands()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim C As Integer = 0

        Try
            For i = 0 To DataGridView1.Rows.Count - 1
                Application.DoEvents()
                Try
                    PerformSQLCommand(DataGridView1.Rows(i).Cells(0).Value.ToString)
                    C += 1

                    '    Threading.Thread.Sleep(100)
                    ListBox1.Items.Add(Now.ToLongTimeString & ":Done:" & DataGridView1.Rows(i).Cells(0).Value.ToString)

                    Application.DoEvents()

                Catch ex As Exception
                    '  Me.Text = ex.Message
                    ListBox1.Items.Add(Now.ToLongTimeString & ":" & ex.Message)
                End Try
            Next
        Catch ex As Exception

        End Try
        MsgBox(C & " of indexs was rebuilded ", MsgBoxStyle.Information)
        GetListOfIndexesRbuildCommands()
    End Sub

    Function PerformSQLCommand(SqlCommandToExecute As String) As Boolean
        Dim con As New SqlConnection(txtConnectionString.Text)

        Dim com As New SqlCommand(SqlCommandToExecute, con)
        Try
            com.CommandTimeout = 300
            con.Open()

            com.ExecuteNonQuery()
        Catch ex As Exception
            ' Me.Text = ex.Message
            ListBox1.Items.Add(Now.ToLongTimeString & ":" & ex.Message)

        Finally
            con.Close()
        End Try
        Return True
    End Function

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim C As Integer = 0
        Try

            For i = 0 To DataGridView1.Rows.Count - 1
                If DataGridView1.Rows(i).Selected = True Then
                    Application.DoEvents()
                    Try
                        PerformSQLCommand(DataGridView1.Rows(i).Cells(0).Value.ToString)
                        C += 1
                        '    Threading.Thread.Sleep(100)
                        ListBox1.Items.Add(Now.ToLongTimeString & ":Done:" & DataGridView1.Rows(i).Cells(0).Value.ToString)

                        Application.DoEvents()

                    Catch ex As Exception
                        '   Me.Text = ex.Message
                        ListBox1.Items.Add(Now.ToLongTimeString & ":" & ex.Message)

                    End Try
                End If

            Next
        Catch ex As Exception

        End Try
        MsgBox(C & " of indexs was rebuilded ", MsgBoxStyle.Information)
        If C >= 1 Then
            GetListOfIndexesRbuildCommands()
        End If
    End Sub
End Class



'Select 'ALTER INDEX   [' +  name   + '] ON ' +    OBJECT_NAME (a.object_id,database_id) + ' ReBuild' --, avg_fragmentation_in_percent  FROM sys.dm_db_index_physical_stats (DB_ID(N'SMS_Authorization'),NULL, NULL, NULL, NULL) AS a, sys.indexes AS b       where a.object_id = b.object_id And a.index_id = b.index_id   And name Is Not null And avg_fragmentation_in_percent > 10   order by avg_fragmentation_in_percent desc