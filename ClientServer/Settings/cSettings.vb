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
Imports System.IO

#End Region ' Imports

Public Class cSettings

    Private Const cRuntime As String = "Runtime"
    Private Const cApplications As String = "Applications"
    Private Const cFolders As String = "Folders"

    Private m_persistence As New cXMLSettings()
    Private m_file As String = ""

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="tRuntime">Optional class to use for runtime settings.</param>
    Public Sub New(Optional tRuntime As Type = Nothing)

        Dim bTypeCorrect As Boolean = False
        If (tRuntime IsNot Nothing) Then
            bTypeCorrect = GetType(cRuntimeSettings).IsAssignableFrom(tRuntime)
        End If

        If (Not bTypeCorrect) Then tRuntime = GetType(cRuntimeSettings)

        Me.ApplicationSettings = New cApplicationAliases()
        Me.FolderSettings = New cFolderAliases()
        Me.RuntimeSettings = DirectCast(Activator.CreateInstance(tRuntime), cRuntimeSettings)
    End Sub

    Public ReadOnly Property RuntimeSettings As cRuntimeSettings = Nothing
    Public ReadOnly Property ApplicationSettings As cApplicationAliases = Nothing
    Public ReadOnly Property FolderSettings As cFolderAliases = Nothing

    Public ReadOnly Property Custom As cXMLSettings
        Get
            Return Me.m_persistence
        End Get
    End Property

    ''' <summary>
    ''' Get/set the file to save settings to. Set to an empty string to reset
    ''' this to the default option.
    ''' </summary>
    ''' <returns></returns>
    Public Property SettingsFile As String
        Get
            If (String.IsNullOrWhiteSpace(Me.m_file)) Then
                Return Me.DefaultSettingsFile
            End If
            Return Me.m_file
        End Get
        Set(value As String)
            If (Not String.IsNullOrWhiteSpace(value)) Then
                If String.Compare(Path.GetFullPath(value), Me.DefaultSettingsFile, True) = 0 Then
                    value = ""
                End If
            End If
            Me.m_file = value
        End Set
    End Property

    Public Sub Load()
        Me.m_persistence.Create(Me.SettingsFile)
        Me.RuntimeSettings.Load(Me.m_persistence, cRuntime)
        Me.ApplicationSettings.Load(Me.m_persistence, cApplications)
        Me.FolderSettings.Load(Me.m_persistence, cFolders)
    End Sub

    Public Sub Save()
        Me.RuntimeSettings.Save(Me.m_persistence, cRuntime)
        Me.ApplicationSettings.Save(Me.m_persistence, cApplications)
        Me.FolderSettings.Save(Me.m_persistence, cFolders)
        Me.m_persistence.Flush()
    End Sub

    Private ReadOnly Property DefaultSettingsFile As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MultiRunFramework", "config.xml")
        End Get
    End Property

End Class
