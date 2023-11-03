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

#End Region

<HideModuleName()>
Public Module modDefinitions

    Public Enum eClientServerStatus
        ''' <summary>A protocol is disconnected.</summary>
        Disconnected = 0
        ''' <summary>A protocol is connected.</summary>
        Connected
    End Enum

    ''' <summary>
    ''' Quality of service enumerator for data exchange protocols.
    ''' </summary>
    Public Enum eQoS As Integer
        Connect = 0
        Disconnect = 1
        Ping = 2
    End Enum

    ''' <summary>
    ''' Execution run results.
    ''' </summary>
    Public Enum eRunState As Integer
        ''' <summary>Task has been defined</summary>
        Idle = 0
        ''' <summary>Task has been assigned to a client</summary>
        Assigned = 1
        ''' <summary>Task is waiting for input</summary>
        WaitingForData = 2
        ''' <summary>Task is in execution</summary>
        Running = 4
        ''' <summary>Task succeeded</summary>
        Success = 8
        ''' <summary>Task failed to start</summary>
        ErrorStart = 10
        ''' <summary>Task failed do to a crash</summary>
        ErrorCrashed = 11
        ''' <summary>Task was aborted</summary>
        ErrorAborted = 12
        ''' <summary>Task was aborted</summary>
        ErrorNoInput = 13
        ''' <summary>Task failed due to configuration errors</summary>
        ErrorConfig = 14
        ''' <summary>Task command line could not be parsed</summary>
        ErrorBadCommandLine = 15
        ''' <summary>Input folder is missing</summary>
        ErrorInFolderMissing = 16
        ''' <summary>Work folder is missing</summary>
        ErrorWorkFolderMissing = 17
        ''' <summary>Output folder is missing</summary>
        ErrorOutFolderMissing = 18
        ''' <summary>A used alias was not declared</summary>
        ErrorAliasMissing = 19
        ''' <summary>A defined folder does not start with an alias</summary>
        ErrorPathNotAliased = 20
        ''' <summary>Task failed for another reason</summary>
        ErrorOther = 666
    End Enum

End Module
