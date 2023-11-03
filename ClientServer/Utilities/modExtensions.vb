' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM run framework is distributed in the hope that it will be useful, but 
' WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
' FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
' details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http:'www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 2020- 
'    Ecopath International Initiative, Barcelona, Spain
' ==============================================================================='

#Region " Imports "

Option Strict On
Imports System.Management
Imports System.Runtime.CompilerServices

#End Region

Module modExtensions

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get all child processes of a given process. Windows only!
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns>A collection of child processes.</returns>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Function GetChildProcesses(p As Process) As IEnumerable(Of Process)
        Dim children As New List(Of Process)
#Disable Warning CA1416 ' Validate platform compatibility
        Using mos As New ManagementObjectSearcher(String.Format("Select * From Win32_Process Where ParentProcessID={0}", p.Id))
            For Each mo As ManagementObject In mos.Get()
                Try
                    children.Add(Process.GetProcessById(Convert.ToInt32(mo("ProcessID"))))
                Catch ex As Exception
                    ' Ignore
                End Try
            Next
        End Using
#Enable Warning CA1416 ' Validate platform compatibility
        Return children

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Murder a process and all of its children. Windows only!
    ''' </summary>
    ''' <param name="p"></param>
    ''' -----------------------------------------------------------------------
    <Extension()>
    Public Sub Kill(p As Process)
        Try
            For Each pChild In p.GetChildProcesses()
                Kill(pChild)
            Next
            p.Kill()
        Catch ex As Exception
            ' Whoah
            Debug.Assert(False, ex.Message)
        End Try
    End Sub

End Module
