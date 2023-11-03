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

Public Class cRuntimeSettings

#Region " Persistence "

    ''' <summary>Time-out, in MS, to kill a task while waiting for data</summary>
    Public Property DataWaitTimeout As Long = 300000
    ''' <summary>Time-out, in MS, to kill a hanging application</summary>
    Public Property KillTimeout As Long = 30000
    ''' <summary>Number of threads to farm out work to.</summary>
    Public Property MaxThreads As Integer = 6
    ''' <summary>The names of the protocols used</summary>
    Public Property Protocol As String = ""

    Public Overridable Function Load(settings As cXMLSettings, section As String) As Boolean

        ' Sanity checks
        If (settings Is Nothing) Then Return False

        Me.DataWaitTimeout = settings.ReadSetting(section, "DataWaitTimeOut", 300000)
        Me.KillTimeout = settings.ReadSetting(section, "KillTimeOut", 30000)
        Me.MaxThreads = settings.ReadSetting(section, "MaxThreads", 6)
        Return True

    End Function

    Public Overridable Function Save(settings As cXMLSettings, section As String) As Boolean

        ' Sanity checks
        If (settings Is Nothing) Then Return False

        settings.WriteSetting(section, "DataWaitTimeOut", Me.DataWaitTimeout)
        settings.WriteSetting(section, "KillTimeout", Me.KillTimeout)
        settings.WriteSetting(section, "MaxThreads", Me.MaxThreads)
        Return True

    End Function

#End Region ' Persistence

End Class
